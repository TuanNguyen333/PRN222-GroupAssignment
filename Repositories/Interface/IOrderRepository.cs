using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IOrderRepository : IRepositoryBase<Order, int>
    {
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId, 
            int pageNumber, 
            int pageSize,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? minOrderDate = null,
            DateTime? maxOrderDate = null);
    }
}
