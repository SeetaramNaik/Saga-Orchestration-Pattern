namespace Shared.Messages.Messages
{
    public class StockRollbackMessage : IStockRollbackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }    
}
