using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Interface
{
    public interface IOrderDetailRepository : IRepositoryBase<OrderDetail, int>
    {
        Task<OrderDetail> GetByIdsAsync(int orderId, int productId);
        
    }
}
