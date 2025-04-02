using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.Product;

namespace BusinessObjects.Dto.OrderDetail;

public class OrderDetailDto
{
    public int OrderId { get; set; }
    public OrderDto OrderDto { get; set; }
    public int ProductId { get; set; }
    public ProductDto ProductDto { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }
}