using System.ComponentModel.DataAnnotations;

namespace eStore.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(40, ErrorMessage = "Tên sản phẩm không được vượt quá 40 ký tự")]
        public required string ProductName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [StringLength(20, ErrorMessage = "Cân nặng không được vượt quá 20 ký tự")]
        public required string Weight { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Units in Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Units in Stock must be 0 or greater")]
        public int UnitsInStock { get; set; }
    }
} 