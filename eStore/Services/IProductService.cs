using eStore.Models;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResponse<Product>>> GetAllProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null);
        Task<ApiResponse<Product>> GetProductByIdAsync(int id);
        Task<ApiResponse<Product>> CreateProductAsync(Product product);
        Task<ApiResponse<Product>> UpdateProductAsync(int id, Product product);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
    }
} 