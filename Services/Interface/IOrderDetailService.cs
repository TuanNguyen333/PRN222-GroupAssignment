using System.Threading.Tasks;
using BusinessObjects.Base;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Entities;

namespace Services.Interface
{
    public interface IOrderDetailService
    {
        Task<PagedApiResponse<OrderDetailDto>> GetAllAsync(
      int? pageNumber = null,
      int? pageSize = null,
      decimal? minUnitPrice = null,
      decimal? maxUnitPrice = null,
      int? minQuantity = null,
      int? maxQuantity = null,
      double? minDiscount = null,
      double? maxDiscount = null);
        Task<ApiResponse<OrderDetailForCreationResponseDto>> CreateAsync(OrderDetailForCreationDto orderDetail);
        Task<ApiResponse<OrderDetailDto>> UpdateAsync(int id, OrderDetailForUpdateDto orderDetail);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<OrderDetailDto>> GetByIdAsync(int orderId, int productId);
        Task<MemoryStream> ExportOrderDetailsToExcelAsync(DateTime startDate, DateTime endDate);
    }
}
