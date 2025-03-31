using BusinessObjects.Dto.OrderDetail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/orderdetails")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService ?? throw new ArgumentNullException(nameof(orderDetailService));
        }

        [HttpGet("{orderId}/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderDetail(int orderId, int productId)
        {
            var response = await _orderDetailService.GetOrderDetailAsync(orderId, productId);
            if (response == null)
            {
                return NotFound(new { Message = $"Order detail with Order ID {orderId} and Product ID {productId} not found" });
            }
            return Ok(response);
        }
    }
}