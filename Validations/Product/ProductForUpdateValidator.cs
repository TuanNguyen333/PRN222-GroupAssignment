using FluentValidation;
using BusinessObjects.Dto.Product;

namespace Validations.Product
{
    public class ProductForUpdateValidator : AbstractValidator<ProductForUpdateDto>
    {
        public ProductForUpdateValidator()
        {
            RuleFor(product => product.ProductName)
                .NotEmpty().WithMessage("Product Name is required.")
                .MaximumLength(40).WithMessage("Product Name cannot exceed 40 characters.");

            RuleFor(product => product.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit Price must be a positive value.");

            RuleFor(product => product.UnitsInStock)
                .GreaterThanOrEqualTo(0).WithMessage("Units In Stock must be a positive value.");
        }
    }
}
