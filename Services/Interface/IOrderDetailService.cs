using System.Threading.Tasks;
using BusinessObjects.Dto.OrderDetail;

namespace Services.Interface
{
    public interface IOrderDetailService
    {
        Task<OrderDetailDto> GetOrderDetailAsync(int orderId, int productId);
    }
}