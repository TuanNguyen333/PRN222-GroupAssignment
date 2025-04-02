using eStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
    }
}
