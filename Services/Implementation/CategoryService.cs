using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using FluentValidation;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Dto.Category;
using BusinessObjects.Entities;

namespace Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryForCreationDto> _creationValidator;
        private readonly IValidator<CategoryForUpdateDto> _updateValidator;
        private const int DEFAULT_PAGE_SIZE = 10;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CategoryForCreationDto> creationValidator, IValidator<CategoryForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _creationValidator = creationValidator ?? throw new ArgumentNullException(nameof(creationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<PagedApiResponse<CategoryDto>> GetAllAsync(
       int? pageNumber = null,
       int? pageSize = null,
       string? search = null)
        {
            try
            {
                // Validate pagination
                if (pageNumber.HasValue && pageNumber < 1)
                    return InvalidPageResponse("Page number must be greater than 0");

                if (pageSize.HasValue && pageSize < 1)
                    return InvalidPageResponse("Page size must be greater than 0");

                // Get all categories first
                var allCategories = await GetAllCategoriesWithPaginationFallback();

                // Apply search filter
                var filteredCategories = ApplySearchFilter(allCategories, search);
                var totalItems = filteredCategories.Count();

                // Handle pagination
                var (actualPageNumber, actualPageSize, pagedCategories) = ApplyPagination(filteredCategories, pageNumber, pageSize);

                // Map to DTO
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(pagedCategories);

                return PagedApiResponse<CategoryDto>.SuccessPagedResponse(
                    categoryDtos,
                    actualPageNumber,
                    actualPageSize,
                    totalItems,
                    "Categories retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = "Failed to retrieve categories",
                    Errors = new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        private (int pageNumber, int pageSize, IEnumerable<Category> categories) ApplyPagination(
    IEnumerable<Category> categories,
    int? pageNumber,
    int? pageSize)
        {
            var actualPageNumber = pageNumber ?? 1;
            var actualPageSize = pageSize ?? categories.Count();

            var pagedCategories = categories
                .Skip((actualPageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .ToList();

            return (actualPageNumber, actualPageSize, pagedCategories);
        }


        private async Task<IEnumerable<Category>> GetAllCategoriesWithPaginationFallback()
        {
            const int largePageSize = int.MaxValue;
            return await _unitOfWork.CategoryRepository.GetAllAsync(1, largePageSize);
        }

        private IEnumerable<Category> ApplySearchFilter(IEnumerable<Category> categories, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return categories;

            var keyword = search.Trim().ToLower();
            return categories.Where(c =>
                (!string.IsNullOrEmpty(c.CategoryName) && c.CategoryName.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(keyword))
            ).ToList();
        }

        private PagedApiResponse<CategoryDto> InvalidPageResponse(string message)
        {
            return new PagedApiResponse<CategoryDto>
            {
                Success = false,
                Message = message,
                Errors = new ErrorResponse("VALIDATION_ERROR", message)
            };
        }


        public async Task<ApiResponse<CategoryDto>> CreateAsync(CategoryForCreationDto category)
        {
            try
            {
                if (category == null)
                    return ApiResponse<CategoryDto>.ErrorResponse("Category cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));
                var validationResult = await _creationValidator.ValidateAsync(category);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<CategoryDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }
                await _unitOfWork.BeginTransactionAsync();
                var categoryEntity = _mapper.Map<Category>(category);
                var lastCategory = await _unitOfWork.CategoryRepository.GetAllAsync(1, int.MaxValue);
                categoryEntity.CategoryId = lastCategory.Max(c => c.CategoryId) + 1;
                _unitOfWork.CategoryRepository.Add(categoryEntity);
                await _unitOfWork.CommitTransactionAsync();
                var categoryDto = _mapper.Map<CategoryDto>(categoryEntity);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to create category",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateAsync(int id, CategoryForUpdateDto category)
        {
            try
            {
                if (category == null)
                    return ApiResponse<CategoryDto>.ErrorResponse("Category cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var validationResult = await _updateValidator.ValidateAsync(category);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<CategoryDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                var existingCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                    return ApiResponse<CategoryDto>.ErrorResponse($"Category with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Category does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(category, existingCategory);
                _unitOfWork.CategoryRepository.Update(existingCategory);

                await _unitOfWork.CommitTransactionAsync();

                var categoryDto = _mapper.Map<CategoryDto>(existingCategory);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to update category",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                    return ApiResponse<string>.ErrorResponse($"Category with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Category does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.CategoryRepository.Delete(category);
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.SuccessResponse(null, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.ErrorResponse("Failed to delete category",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetByIdAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                    return ApiResponse<CategoryDto>.ErrorResponse($"Category with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Category does not exist"));

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to retrieve category",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }
    }
}