using System.Threading.Tasks;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly eStoreDBContext _context;

        public OrderDetailRepository(eStoreDBContext context)
        {
            _context = context;
        }

        public async Task<OrderDetail> GetByOrderAndProductIdAsync(int orderId, int productId)
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
        }
    }
}