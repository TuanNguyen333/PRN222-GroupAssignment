using eStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
    }
} 