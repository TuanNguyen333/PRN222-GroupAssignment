using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository CategoryRepository { get; }
        IMemberRepository MemberRepository { get; }
        IProductRepository ProductRepository { get; }
        IOrderRepository OrderRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
