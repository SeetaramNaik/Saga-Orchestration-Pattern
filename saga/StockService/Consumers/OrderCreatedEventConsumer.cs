using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;
using StockService.Models;

namespace StockService.Consumers
{
    public class OrderCreatedEventConsumer: IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        
        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();
            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (OrderItemMessage item in context.Message.OrderItems)
                {
                    Stock stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
                    if (stock is not null)
                    {
                        stock.Count -= item.Count;
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Stock was reserved for Correlation Id :{context.Message.CorrelationId}");

                var stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };

                await _publishEndpoint.Publish(stockReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock"
                });

                _logger.LogInformation($"Not enough stock for Correlation Id :{context.Message.CorrelationId}");
            }
        }
    }
}
