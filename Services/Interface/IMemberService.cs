using System;
using System.Threading.Tasks;
using BusinessObjects.Base;
using BusinessObjects.Dto.Member;

namespace Services.Interface
{
    public interface IMemberService
    {
        Task<PagedApiResponse<MemberDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
        Task<ApiResponse<MemberDto>> CreateAsync(MemberForCreationDto member);
        Task<ApiResponse<MemberDto>> UpdateAsync(int id, MemberForUpdateDto member);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<MemberDto>> GetByIdAsync(int id);
    }
}