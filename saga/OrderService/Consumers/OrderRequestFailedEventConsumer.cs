using MassTransit;
using OrderService.Models;
using Shared.Interfaces;

namespace OrderService.Consumers
{
    public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRequestFailedEventConsumer> _logger;
        
        public OrderRequestFailedEventConsumer(AppDbContext context, ILogger<OrderRequestFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
        {
            Models.Order order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Reason;
                _ = await _context.SaveChangesAsync();

                _logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed : {order.Status}");
            }
            else
            {
                _logger.LogError($"Order (Id={context.Message.OrderId}) not found");
            }
        }
    }    
}
