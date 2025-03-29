using DataAccess.Data;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Impl
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly EStoreDbContext _context;

        public OrderDetailRepository(EStoreDbContext context)
        {
            _context = context;
        }

        public Task<OrderDetail> CreateAsync(OrderDetail entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(OrderDetail entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetail> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetail> UpdateAsync(OrderDetail entity)
        {
            throw new NotImplementedException();
        }
    }
}
