using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
// Ensure this namespace is included
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
        private const int DEFAULT_PAGE_SIZE = 10;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedApiResponse<CategoryDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                IEnumerable<Category> categories;
                int totalItems;
                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    categories = await _unitOfWork.CategoryRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = categories.Count();
                    pageNumber = 1;
                    pageSize = totalItems;
                }
                else
                {
                    if (pageNumber < 1)
                        return (PagedApiResponse<CategoryDto>)PagedApiResponse<CategoryDto>.ErrorResponse("Page number must be greater than 0",
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number"));

                    if (pageSize < 1)
                        return (PagedApiResponse<CategoryDto>)PagedApiResponse<CategoryDto>.ErrorResponse("Page size must be greater than 0",
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size"));

                    categories = await _unitOfWork.CategoryRepository.GetAllAsync(pageNumber.Value, pageSize.Value);
                    var allCategories = await _unitOfWork.CategoryRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = allCategories.Count();
                }

                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return PagedApiResponse<CategoryDto>.SuccessPagedResponse(
                    categoryDtos,
                    pageNumber.Value,
                    pageSize.Value,
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
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateAsync(CategoryForCreationDto category)
        {
            try
            {
                if (category == null)
                    return ApiResponse<CategoryDto>.ErrorResponse("Category cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                await _unitOfWork.BeginTransactionAsync();

                var categoryEntity = _mapper.Map<Category>(category);
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