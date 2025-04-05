using BusinessObjects.Entities;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementation
{
    public class OrderRepository : RepositoryBase<Order, int>, IOrderRepository
    {
        public OrderRepository(eStoreDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.Orders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId, 
            int pageNumber, 
            int pageSize,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? minOrderDate = null,
            DateTime? maxOrderDate = null)
        {
            // Bắt đầu với query cơ bản
            var query = _context.Orders.Where(o => o.MemberId == userId);

            // Áp dụng các bộ lọc
            if (minFreight.HasValue)
                query = query.Where(o => o.Freight >= minFreight.Value);
            
            if (maxFreight.HasValue)
                query = query.Where(o => o.Freight <= maxFreight.Value);
            
            if (minOrderDate.HasValue)
                query = query.Where(o => o.OrderDate >= minOrderDate.Value);
            
            if (maxOrderDate.HasValue)
                query = query.Where(o => o.OrderDate <= maxOrderDate.Value);

            // Đếm tổng số bản ghi sau khi lọc
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang và sắp xếp
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}
