using Shared.Interfaces;
using Shared.Messages;

namespace Shared.Events
{
    public class StockReservedEvent : IStockReservedEvent
    {
        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }    
}
