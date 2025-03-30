using BusinessObjects.Dto.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace Services.Interface
{
    public interface IProductService
    {
        Task<PagedApiResponse<ProductDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
        Task<ApiResponse<ProductDto>> CreateAsync(ProductForCreationDto product);
        Task<ApiResponse<ProductDto>> UpdateAsync(int id, ProductForUpdateDto product);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    }
}
