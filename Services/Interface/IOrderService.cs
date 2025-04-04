using System;
using System.Threading.Tasks;
using BusinessObjects.Base;
using BusinessObjects.Dto.Order;

namespace Services.Interface
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> GetByIdAsync(int id);
        Task<PagedApiResponse<OrderDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<ApiResponse<OrderDto>> CreateAsync(OrderForCreationDto order);
        Task<ApiResponse<OrderDto>> UpdateAsync(int id, OrderForUpdateDto order);
        Task<ApiResponse<string>> DeleteAsync(int id, int userId);
        Task<PagedApiResponse<OrderDto>> GetAllAsync(
           int? pageNumber = null,
           int? pageSize = null,
           decimal? minFreight = null,
           decimal? maxFreight = null,
           DateTime? minOrderDate = null,
           DateTime? maxOrderDate = null);
        Task<MemoryStream> ExportSalesToExcelAsync(DateTime startDate, DateTime endDate)
        Task<PagedApiResponse<OrderDto>> GetOrdersByUserIdAsync(
            int userId,
            int? pageNumber = null,
            int? pageSize = null,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? minOrderDate = null,
            DateTime? maxOrderDate = null);
    }
}