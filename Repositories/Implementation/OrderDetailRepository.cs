using System.Threading.Tasks;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Impl;
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
    }
}
