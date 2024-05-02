using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

        public OrderRepository(DbContextOptions<ApplicationDbContext> contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            await using var _db = new ApplicationDbContext(_contextOptions);
            await _db.OrderHeaders.AddAsync(orderHeader);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
        {
            await using var _db = new ApplicationDbContext(_contextOptions);
            var orderHeaderFormDb = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.OrderHeaderId == orderHeaderId);
            
            if (orderHeaderFormDb != null)
            {
                orderHeaderFormDb.PaymentStatus = paid;
                await _db.SaveChangesAsync();
            }
        }
    }
}
