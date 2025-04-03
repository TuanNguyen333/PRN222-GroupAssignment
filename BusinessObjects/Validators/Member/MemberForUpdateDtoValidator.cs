using FluentValidation;
using BusinessObjects.Dto.Member;

namespace BusinessObjects.Validators.Member
{
    public class MemberForUpdateDtoValidator : AbstractValidator<MemberForUpdateDto>
    {
        public MemberForUpdateDtoValidator()
        {
            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress()
                    .WithMessage("Invalid email format")
                    .MaximumLength(100)
                    .WithMessage("Email cannot exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.CompanyName), () =>
            {
                RuleFor(x => x.CompanyName)
                    .MaximumLength(40)
                    .WithMessage("Company name cannot exceed 40 characters");
            });

            When(x => !string.IsNullOrEmpty(x.City), () =>
            {
                RuleFor(x => x.City)
                    .MaximumLength(15)
                    .WithMessage("City cannot exceed 15 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Country), () =>
            {
                RuleFor(x => x.Country)
                    .MaximumLength(15)
                    .WithMessage("Country cannot exceed 15 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password)
                    .MinimumLength(6)
                    .WithMessage("Password must be at least 6 characters")
                    .MaximumLength(30)
                    .WithMessage("Password cannot exceed 30 characters");
            });
        }
    }
} 