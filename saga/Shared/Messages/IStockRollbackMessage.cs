namespace Shared.Messages.Messages
{
    public interface IStockRollbackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }    
    
}

