using eStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<PagedResponse<Category>>> GetAllCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null);

        Task<ApiResponse<Category>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<Category>> CreateCategoryAsync(Category category);
        Task<ApiResponse<Category>> UpdateCategoryAsync(int id, Category category);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
    }
} 