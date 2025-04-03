using FluentValidation;
using BusinessObjects.Dto.OrderDetail; // Ensure you have this DTO

namespace Validations.OrderDetail
{
    public class OrderDetailForUpdateValidator : AbstractValidator<OrderDetailForUpdateDto>
    {
        public OrderDetailForUpdateValidator()
        {
            RuleFor(x => x.UnitPrice)
                .NotEmpty().WithMessage("UnitPrice is required.")
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");

            RuleFor(x => x.Quantity)
                .NotEmpty().WithMessage("Quantity is required.")
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.")
                .LessThanOrEqualTo(100).WithMessage("Discount must be between 0 and 100 (as a percentage).");
        }
    }
}
