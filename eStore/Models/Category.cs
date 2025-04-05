using System.ComponentModel.DataAnnotations;

namespace eStore.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Category name must not contain special characters.")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Description must not contain special characters.")]
        public string Description { get; set; }
    }
}