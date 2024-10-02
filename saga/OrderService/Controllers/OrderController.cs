using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Models;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderController(AppDbContext context, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address { Line = orderCreate.Address.Line, Province = orderCreate.Address.Province, District = orderCreate.Address.District },
                CreatedDate = DateTime.Now
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem() { Price = item.Price, ProductId = item.ProductId, Count = item.Count });
            });

            await _context.AddAsync(newOrder);

            await _context.SaveChangesAsync();

            var orderCreatedRequestEvent = new OrderCreatedRequestEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreate.Payment.CardName,
                    CardNumber = orderCreate.Payment.CardNumber,
                    Expiration = orderCreate.Payment.Expiration,
                    CVV = orderCreate.Payment.CVV,
                    TotalPrice = orderCreate.OrderItems.Sum(x => x.Price * x.Count)
                },
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage { Count = item.Count, ProductId = item.ProductId });
            });

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.OrderSaga}"));
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);

            return Ok();
        }
        
        
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                // Your logic here
                return Ok("Order service is running.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }    
}

