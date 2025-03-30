using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
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
        private const int DEFAULT_PAGE_SIZE = 10;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedApiResponse<ProductDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                IEnumerable<Product> products;
                int totalItems;

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    products = await _unitOfWork.ProductRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = products.Count();
                    pageNumber = 1;
                    pageSize = totalItems;
                }
                else
                {
                    if (pageNumber < 1)
                        return new PagedApiResponse<ProductDto>
                        {
                            Success = false,
                            Message = "Page number must be greater than 0",
                            Errors = new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number")
                        };

                    if (pageSize < 1)
                        return new PagedApiResponse<ProductDto>
                        {
                            Success = false,
                            Message = "Page size must be greater than 0",
                            Errors = new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size")
                        };

                    products = await _unitOfWork.ProductRepository.GetAllAsync(pageNumber.Value, pageSize.Value);
                    var allProducts = await _unitOfWork.ProductRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = allProducts.Count();
                }

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
                return PagedApiResponse<ProductDto>.SuccessPagedResponse(
                    productDtos,
                    pageNumber.Value,
                    pageSize.Value,
                    totalItems,
                    "Products retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Failed to retrieve products",
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(ProductForCreationDto product)
        {
            try
            {
                if (product == null)
                    return ApiResponse<ProductDto>.ErrorResponse("Product cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                await _unitOfWork.BeginTransactionAsync();

                var productEntity = _mapper.Map<Product>(product);
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