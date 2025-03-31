using FluentValidation;
using BusinessObjects.Dto.Member;

namespace Validations.Member
{
    public class MemberForCreationValidator : AbstractValidator<MemberForCreationDto>
    {
        public MemberForCreationValidator()
        {
            RuleFor(member => member.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(member => member.CompanyName)
                .NotEmpty().WithMessage("Company Name is required.")
                .MaximumLength(40).WithMessage("Company Name cannot exceed 40 characters.");

            RuleFor(member => member.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(15).WithMessage("City cannot exceed 15 characters.");

            RuleFor(member => member.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(15).WithMessage("Country cannot exceed 15 characters.");

            RuleFor(member => member.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(30).WithMessage("Password cannot exceed 30 characters.");
        }
    }
}
