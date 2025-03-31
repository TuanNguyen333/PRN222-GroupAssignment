using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IMemberRepository : IRepositoryBase<Member, int>
    {
        Task<Member?> GetByEmailAsync(string email);
    }
}
