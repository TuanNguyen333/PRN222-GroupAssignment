using BusinessObjects.Entities;
using Repositories.Impl;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class OrderRepository : RepositoryBase<Order, int>, IOrderRepository
    {
        public OrderRepository(eStoreDBContext context) : base(context)
        {
        }
    }
}
