using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages.Messages;
using StockService.Models;

namespace StockService.Consumers
{
    public class StockRollbackMessageConsumer  : IConsumer<IStockRollbackMessage>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StockRollbackMessageConsumer> _logger;

        public StockRollbackMessageConsumer(AppDbContext context, ILogger<StockRollbackMessageConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task Consume(ConsumeContext<IStockRollbackMessage> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                Stock stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation($"Stock was released.");
        }
    }    
}
