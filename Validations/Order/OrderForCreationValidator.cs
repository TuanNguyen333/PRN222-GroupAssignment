using FluentValidation;
using BusinessObjects.Dto.Order;

namespace Validations.Order
{
    public class OrderForCreationValidator : AbstractValidator<OrderForCreationDto>
    {
        public OrderForCreationValidator()
        {
            RuleFor(order => order.MemberId)
                .GreaterThan(0).WithMessage("Member ID is required.");

            RuleFor(order => order.OrderDate)
                .NotEmpty().WithMessage("Order Date is required.");

            RuleFor(order => order.Freight)
                .GreaterThanOrEqualTo(0).WithMessage("Freight must be a positive value.");
        }
    }
}
