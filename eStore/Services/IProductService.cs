using eStore.Models;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface IProductService
    {
        Task<PagedResponse<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }
} 