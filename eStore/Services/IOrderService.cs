using eStore.Models;
using System;
using System.Threading.Tasks;

namespace eStore.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<PagedResponse<Order>>> GetAllOrdersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<ApiResponse<Order>> GetOrderByIdAsync(int id);
        Task<ApiResponse<Order>> CreateOrderAsync(Order order);
        Task<ApiResponse<Order>> UpdateOrderAsync(int id, Order order);
        Task<ApiResponse<bool>> DeleteOrderAsync(int id);
        Task<ApiResponse<PagedResponse<Order>>> GetUserOrdersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }
}
