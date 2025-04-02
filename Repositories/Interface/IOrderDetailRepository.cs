using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;

namespace Repositories.Interface
{
    public interface IOrderDetailRepository 
    {
        Task<OrderDetail> GetByOrderAndProductIdAsync(int orderId, int productId);
    }
}
