using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using FluentValidation;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Entities;
using BusinessObjects.Dto.Product;

namespace Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductForCreationDto> _creationValidator;
        private readonly IValidator<ProductForUpdateDto> _updateValidator;
        private const int DEFAULT_PAGE_SIZE = 10;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<ProductForCreationDto> creationValidator, IValidator<ProductForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _creationValidator = creationValidator ?? throw new ArgumentNullException(nameof(creationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<PagedApiResponse<ProductDto>> GetAllAsync(
    int? pageNumber = null,
    int? pageSize = null,
    string? search = null,
    decimal? minUnitPrice = null,
    decimal? maxUnitPrice = null)
        {
            try
            {
                // Validate input
                if (pageNumber.HasValue && pageNumber < 1)
                    return InvalidPageResponse("Page number must be greater than 0");

                if (pageSize.HasValue && pageSize < 1)
                    return InvalidPageResponse("Page size must be greater than 0");

                if (minUnitPrice.HasValue && maxUnitPrice.HasValue && minUnitPrice > maxUnitPrice)
                    return new PagedApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Min price cannot be greater than max price",
                        Errors = new ErrorResponse("VALIDATION_ERROR", "Invalid price range")
                    };

                // Get all products first (since repository doesn't support parameterless GetAll)
                var allProducts = await GetAllProductsWithPaginationFallback();

                // Apply filters
                var filteredProducts = ApplyFilters(allProducts, search, minUnitPrice, maxUnitPrice);
                var totalItems = filteredProducts.Count();

                // Handle pagination
                var (actualPageNumber, actualPageSize, pagedProducts) = ApplyPagination(
                    filteredProducts, pageNumber, pageSize);

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts);

                return PagedApiResponse<ProductDto>.SuccessPagedResponse(
                    productDtos,
                    actualPageNumber,
                    actualPageSize,
                    totalItems,
                    "Products retrieved successfully");
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Failed to retrieve products",
                    Errors = new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        private async Task<IEnumerable<Product>> GetAllProductsWithPaginationFallback()
        {
            // Use a large page size to effectively get all products
            const int largePageSize = int.MaxValue;
            return await _unitOfWork.ProductRepository.GetAllAsync(1, largePageSize);
        }

        private (int pageNumber, int pageSize, IEnumerable<Product> products) ApplyPagination(
            IEnumerable<Product> products,
            int? pageNumber,
            int? pageSize)
        {
            var actualPageNumber = pageNumber ?? 1;
            var actualPageSize = pageSize ?? products.Count();

            var pagedProducts = products
                .Skip((actualPageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .ToList();

            return (actualPageNumber, actualPageSize, pagedProducts);
        }

        private IEnumerable<Product> ApplyFilters(
            IEnumerable<Product> products,
            string? search,
            decimal? minUnitPrice,
            decimal? maxUnitPrice)
        {
            var query = products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (minUnitPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice >= minUnitPrice.Value);
            }

            if (maxUnitPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice <= maxUnitPrice.Value);
            }

            return query.ToList();
        }

        private PagedApiResponse<ProductDto> InvalidPageResponse(string message)
        {
            return new PagedApiResponse<ProductDto>
            {
                Success = false,
                Message = message,
                Errors = new ErrorResponse("VALIDATION_ERROR", message)
            };
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(ProductForCreationDto product)
        {
            try
            {
                if (product == null)
                    return ApiResponse<ProductDto>.ErrorResponse("Product cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var validationResult = await _creationValidator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<ProductDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                await _unitOfWork.BeginTransactionAsync();

                var productEntity = _mapper.Map<Product>(product);

                // Increment the ID by 1
                var lastProduct = await _unitOfWork.ProductRepository.GetAllAsync(1, int.MaxValue);
                productEntity.ProductId = lastProduct.Max(p => p.ProductId) + 1;

                _unitOfWork.ProductRepository.Add(productEntity);
                await _unitOfWork.CommitTransactionAsync();

                var productDto = _mapper.Map<ProductDto>(productEntity);
                return ApiResponse<ProductDto>.SuccessResponse(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ProductDto>.ErrorResponse("Failed to create product",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, ProductForUpdateDto product)
        {
            try
            {
                if (product == null)
                    return ApiResponse<ProductDto>.ErrorResponse("Product cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var validationResult = await _updateValidator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<ProductDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(id);
                if (existingProduct == null)
                    return ApiResponse<ProductDto>.ErrorResponse($"Product with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Product does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(product, existingProduct);
                _unitOfWork.ProductRepository.Update(existingProduct);

                await _unitOfWork.CommitTransactionAsync();

                var productDto = _mapper.Map<ProductDto>(existingProduct);
                return ApiResponse<ProductDto>.SuccessResponse(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ProductDto>.ErrorResponse("Failed to update product",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
                if (product == null)
                    return ApiResponse<string>.ErrorResponse($"Product with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Product does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.ProductRepository.Delete(product);
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.SuccessResponse(null, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.ErrorResponse("Failed to delete product",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
                if (product == null)
                    return ApiResponse<ProductDto>.ErrorResponse($"Product with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Product does not exist"));

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(productDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDto>.ErrorResponse("Failed to retrieve product",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }
    }
}