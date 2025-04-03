using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using BusinessObjects.Dto.OrderDetail;
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
     [FromQuery] int? pageNumber = null,
     [FromQuery] int? pageSize = null,
     [FromQuery] decimal? minUnitPrice = null,
     [FromQuery] decimal? maxUnitPrice = null,
     [FromQuery] int? minQuantity = null,
     [FromQuery] int? maxQuantity = null,
     [FromQuery] double? minDiscount = null,
     [FromQuery] double? maxDiscount = null)
        {
            var response = await _orderDetailService.GetAllAsync(pageNumber, pageSize, minUnitPrice, maxUnitPrice, minQuantity, maxQuantity, minDiscount, maxDiscount);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("{orderId}/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int orderId, int productId)
        {
            var response = await _orderDetailService.GetByIdAsync(orderId, productId);
            if (response == null || response.Success == false)
            {
                return NotFound(new { Message = $"Order detail for Order ID {orderId} and Product ID {productId} not found" });
            }
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrderDetailForCreationDto orderDetail)
        {
            var response = await _orderDetailService.CreateAsync(orderDetail);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetById), new { orderId = response.Data.OrderId, productId = response.Data.ProductId }, response);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] OrderDetailForUpdateDto orderDetail)
        {
            var response = await _orderDetailService.UpdateAsync(id, orderDetail);
            if (!response.Success)
            {
                if (response.Errors?.ErrorCode == "NOT_FOUND")
                    return NotFound(response);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _orderDetailService.DeleteAsync(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return NoContent();
        }

        [HttpGet("order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByOrderId(
            int orderId,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            var response = await _orderDetailService.GetByOrderIdAsync(orderId, pageNumber, pageSize);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
