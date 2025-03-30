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
    public class MemberRepository : RepositoryBase<Member, int>, IMemberRepository
    {
        public MemberRepository(eStoreDBContext context) : base(context)
        {
        }

    }
}

