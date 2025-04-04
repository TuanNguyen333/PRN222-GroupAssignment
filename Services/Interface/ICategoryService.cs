using BusinessObjects.Dto.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace Services.Interface
{
    public interface ICategoryService
    {
        Task<PagedApiResponse<CategoryDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null, string? search = null);
        Task<ApiResponse<CategoryDto>> CreateAsync(CategoryForCreationDto category);
        Task<ApiResponse<CategoryDto>> UpdateAsync(int id, CategoryForUpdateDto category);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<CategoryDto>> GetByIdAsync(int id);
    }
}
