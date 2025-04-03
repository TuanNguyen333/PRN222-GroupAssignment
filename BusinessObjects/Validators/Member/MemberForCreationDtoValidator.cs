using FluentValidation;
using BusinessObjects.Dto.Member;

namespace BusinessObjects.Validators.Member
{
    public class MemberForCreationDtoValidator : AbstractValidator<MemberForCreationDto>
    {
        public MemberForCreationDtoValidator()
        {
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

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .MaximumLength(30)
                .WithMessage("Password cannot exceed 30 characters");
        }
    }
} 