using eStore.Models;

namespace eStore.Services
{
    public interface IOrderDetailService
    {
        Task<ApiResponse<PagedResponse<OrderDetailDto>>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int? minQuantity = null,
            int? maxQuantity = null,
            double? minDiscount = null,
            double? maxDiscount = null);
        Task<ApiResponse<OrderDetailDto>> GetByIdAsync(int orderId, int productId);
        Task<ApiResponse<OrderDetailDto>> CreateAsync(OrderDetailDto orderDetail);
        Task<ApiResponse<OrderDetailDto>> UpdateAsync(int orderId, int productId, OrderDetailDto orderDetail);
        Task<ApiResponse<bool>> DeleteAsync(int orderId, int productId);
        Task<ApiResponse<PagedResponse<OrderDetailDto>>> GetByOrderIdAsync(int orderId);
        Task<byte[]> ExportToExcelAsync(
            int? pageNumber = null,
            int? pageSize = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int? minQuantity = null,
            int? maxQuantity = null,
            double? minDiscount = null,
            double? maxDiscount = null);
    }
}
