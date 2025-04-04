using eStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
        Task<ApiResponse<Member>> CreateMemberAsync(Member member);
        Task<ApiResponse<Member>> UpdateMemberAsync(int id, Member member);
        Task<ApiResponse<bool>> DeleteMemberAsync(int id);
        Task<ApiResponse<Member>> GetCurrentUserAsync();
        Task<ApiResponse<Member>> UpdateCurrentUserAsync(Member member);
    }
}
