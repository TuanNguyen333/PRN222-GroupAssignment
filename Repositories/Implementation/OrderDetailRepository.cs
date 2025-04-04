using System.Threading.Tasks;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class OrderDetailRepository : RepositoryBase<OrderDetail, int>, IOrderDetailRepository
    {
        private readonly eStoreDBContext _context;

        public OrderDetailRepository(eStoreDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OrderDetail> GetByIdsAsync(int orderId, int productId)
        {
            var orderDetail = await _context.OrderDetails
                                               .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
            return orderDetail;
        }

        public async Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }
    }
}
