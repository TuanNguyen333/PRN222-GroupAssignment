using BusinessObject.Base;
using DataAccess.Entities;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Services
{
    public class OrderDetailService : ICrudService<OrderDetail>
    {
        private readonly IOrderDetailRepository _orderDetailRepository;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public Task<OrderDetail> CreateAsync(OrderDetail entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(int id)
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
