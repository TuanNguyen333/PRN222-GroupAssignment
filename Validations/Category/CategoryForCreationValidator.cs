using FluentValidation;
using BusinessObjects.Dto.Category;

namespace Validations.Category
{
    public class CategoryForCreationValidator : AbstractValidator<CategoryForCreationDto>
    {
        public CategoryForCreationValidator()
        {
            RuleFor(category => category.CategoryName)
                .NotEmpty().WithMessage("Category Name is required.")
                .MaximumLength(40).WithMessage("Category Name cannot exceed 40 characters.");

            RuleFor(category => category.Description)
                .MaximumLength(100).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}
