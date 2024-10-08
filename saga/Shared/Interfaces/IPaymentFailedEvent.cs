using MassTransit;
using Shared.Messages;

namespace Shared.Interfaces
{
    public interface IPaymentFailedEvent : CorrelatedBy<Guid>
    {
        public string Reason { get; set; }

        public List<OrderItemMessage> OrderItems { get; set; }
    }    
}
