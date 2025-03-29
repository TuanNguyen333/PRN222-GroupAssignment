using DataAccess.Data;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Impl
{
    public class MemberRepository : IMemberRepository
    {
        private readonly EStoreDbContext _context;

        public MemberRepository(EStoreDbContext context)
        {
            _context = context;
        }

        public Task<Member> CreateAsync(Member entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Member entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Member>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Member> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Member> UpdateAsync(Member entity)
        {
            throw new NotImplementedException();
        }
    }
}
