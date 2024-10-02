using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Messages;

namespace Shared.Interfaces
{
    public interface IStockReservedRequestPayment : CorrelatedBy<Guid>
    {
        PaymentMessage Payment { get; set; }

        List<OrderItemMessage> OrderItems { get; set; }

        public string BuyerId { get; set; }
    }    
}
