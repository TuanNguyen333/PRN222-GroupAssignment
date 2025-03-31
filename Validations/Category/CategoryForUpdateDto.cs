using FluentValidation;
using BusinessObjects.Dto.Category;

namespace Validations.Category
{
    public class CategoryForUpdateValidator : AbstractValidator<CategoryForUpdateDto>
    {
        public CategoryForUpdateValidator()
        {
            RuleFor(category => category.CategoryName)
                .NotEmpty().WithMessage("Category Name is required.")
                .MaximumLength(40).WithMessage("Category Name cannot exceed 40 characters.");

            RuleFor(category => category.Description)
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}
