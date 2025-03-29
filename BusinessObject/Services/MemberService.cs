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
    public class MemberService : ICrudService<Member>
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public Task<Member> CreateAsync(Member entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(int id)
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
