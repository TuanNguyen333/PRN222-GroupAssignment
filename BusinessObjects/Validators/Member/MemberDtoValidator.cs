using FluentValidation;
using BusinessObjects.Dto.Member;

namespace BusinessObjects.Validators.Member
{
    public class MemberDtoValidator : AbstractValidator<MemberDto>
    {
        public MemberDtoValidator()
        {
            RuleFor(x => x.MemberId)
                .GreaterThan(0)
                .WithMessage("Member ID must be greater than 0");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(100)
                .WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage("Company name is required")
                .MaximumLength(40)
                .WithMessage("Company name cannot exceed 40 characters");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required")
                .MaximumLength(15)
                .WithMessage("City cannot exceed 15 characters");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Country is required")
                .MaximumLength(15)
                .WithMessage("Country cannot exceed 15 characters");
        }
    }
} 