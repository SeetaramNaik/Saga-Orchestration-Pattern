using Shared.Interfaces;
using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent : IOrderCreatedEvent
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
