using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Dto.Product;
using BusinessObjects.Entities;
using FluentValidation;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderDetailForCreationDto> _creationValidator;
        private readonly IValidator<OrderDetailForUpdateDto> _updateValidator;

        public OrderDetailService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<OrderDetailForCreationDto> creationValidator,
            IValidator<OrderDetailForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _creationValidator = creationValidator ?? throw new ArgumentNullException(nameof(creationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }


        public async Task<ApiResponse<OrderDetailDto>> GetByIdAsync(int orderId, int productId)
        {
            var orderDetail = await _unitOfWork.OrderDetailRepository.GetByIdsAsync(orderId, productId);
            if (orderDetail == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order detail not found", new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

            // Fetch related Order and Product by their IDs
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderDetail.OrderId);
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(orderDetail.ProductId);

            if (order == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order not found", new ErrorResponse("NOT_FOUND", "Order does not exist"));

            if (product == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Product not found", new ErrorResponse("NOT_FOUND", "Product does not exist"));

            // Map Order and Product to DTOs
            var orderDto = _mapper.Map<OrderDto>(order);
            var productDto = _mapper.Map<ProductDto>(product);

            // Map OrderDetail to OrderDetailDto and include OrderDto and ProductDto
            var orderDetailDto = new OrderDetailDto
            {
                OrderId = orderDetail.OrderId,
                OrderDto = orderDto, // Add the mapped OrderDto
                ProductId = orderDetail.ProductId,
                ProductDto = productDto, // Add the mapped ProductDto
                UnitPrice = orderDetail.UnitPrice,
                Quantity = orderDetail.Quantity,
                Discount = orderDetail.Discount
            };

            return ApiResponse<OrderDetailDto>.SuccessResponse(orderDetailDto);
        }
        public async Task<ApiResponse<OrderDetailForCreationResponseDto>> CreateAsync(OrderDetailForCreationDto orderDetail)
        {
            // Validate input
            var validationResult = await _creationValidator.ValidateAsync(orderDetail);
            if (!validationResult.IsValid)
            {
                return ApiResponse<OrderDetailForCreationResponseDto>.ErrorResponse(
                    "Validation failed",
                    new ErrorResponse(
                        "VALIDATION_ERROR",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                    ));
            }

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                // Create entity
                var orderDetailEntity = new OrderDetail
                {
                    OrderId = orderDetail.OrderId,
                    ProductId = orderDetail.ProductId,
                    UnitPrice = orderDetail.UnitPrice,
                    Quantity = orderDetail.Quantity,
                    Discount = orderDetail.Discount
                };

                // Add to repository
                _unitOfWork.OrderDetailRepository.Add(orderDetailEntity);

                // Save changes
                await _unitOfWork.CommitTransactionAsync();

                // Map to response DTO
                var responseDto = new OrderDetailForCreationResponseDto
                {
                    OrderId = orderDetailEntity.OrderId,
                    ProductId = orderDetailEntity.ProductId,
                    UnitPrice = orderDetailEntity.UnitPrice,
                    Quantity = orderDetailEntity.Quantity,
                    Discount = orderDetailEntity.Discount
                };

                return ApiResponse<OrderDetailForCreationResponseDto>.SuccessResponse(
                    responseDto,
                    "Order detail created successfully");
            }
            catch (Exception ex)
            {
                // Rollback on error
                await _unitOfWork.RollbackTransactionAsync();

                return ApiResponse<OrderDetailForCreationResponseDto>.ErrorResponse(
                    "Failed to create order detail",
                    new ErrorResponse("TRANSACTION_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<OrderDetailDto>> UpdateAsync(int id, OrderDetailForUpdateDto orderDetail)
        {
            var existingOrderDetail = await _unitOfWork.OrderDetailRepository.GetByIdAsync(id);
            if (existingOrderDetail == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order detail not found", new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

            var validationResult = await _updateValidator.ValidateAsync(orderDetail);
            if (!validationResult.IsValid)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Validation failed", new ErrorResponse("VALIDATION_ERROR", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

            await _unitOfWork.BeginTransactionAsync();
            _mapper.Map(orderDetail, existingOrderDetail);
            _unitOfWork.OrderDetailRepository.Update(existingOrderDetail);
            await _unitOfWork.CommitTransactionAsync();

            var orderDetailDto = _mapper.Map<OrderDetailDto>(existingOrderDetail);
            return ApiResponse<OrderDetailDto>.SuccessResponse(orderDetailDto, "Order detail updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var orderDetail = await _unitOfWork.OrderDetailRepository.GetByIdAsync(id);
            if (orderDetail == null)
                return ApiResponse<string>.ErrorResponse("Order detail not found", new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

            await _unitOfWork.BeginTransactionAsync();
            _unitOfWork.OrderDetailRepository.Delete(orderDetail);
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<string>.SuccessResponse(null, "Order detail deleted successfully");
        }

        public async Task<PagedApiResponse<OrderDetailDto>> GetAllAsync(
     int? pageNumber = null,
     int? pageSize = null,
     decimal? minUnitPrice = null,
     decimal? maxUnitPrice = null,
     int? minQuantity = null,
     int? maxQuantity = null,
     double? minDiscount = null,
     double? maxDiscount = null)
        {
            try
            {
                // Default values when pageNumber or pageSize is null
                int actualPageNumber = pageNumber ?? 1;
                int actualPageSize = pageSize ?? int.MaxValue;

                // Fetch all order details
                var allOrderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(actualPageNumber, actualPageSize);

                // Apply filters
                var filteredOrderDetails = ApplyFilters(allOrderDetails, minUnitPrice, maxUnitPrice, minQuantity, maxQuantity, minDiscount, maxDiscount);
                var totalItems = filteredOrderDetails.Count();

                // Handle pagination
                var (finalPageNumber, finalPageSize, pagedOrderDetails) = ApplyPagination(filteredOrderDetails, actualPageNumber, actualPageSize);

                // Map each OrderDetail to OrderDetailDto manually
                var orderDetailDtos = new List<OrderDetailDto>();
                foreach (var orderDetail in pagedOrderDetails)
                {
                    var orderDto = await _unitOfWork.OrderRepository.GetByIdAsync(orderDetail.OrderId);
                    var productDto = await _unitOfWork.ProductRepository.GetByIdAsync(orderDetail.ProductId);

                    var orderDetailDto = new OrderDetailDto
                    {
                        OrderId = orderDetail.OrderId,
                        OrderDto = _mapper.Map<OrderDto>(orderDto), 
                        ProductId = orderDetail.ProductId,
                        ProductDto = _mapper.Map<ProductDto>(productDto), 
                        UnitPrice = orderDetail.UnitPrice,
                        Quantity = orderDetail.Quantity,
                        Discount = orderDetail.Discount
                    };

                    orderDetailDtos.Add(orderDetailDto);
                }

                return PagedApiResponse<OrderDetailDto>.SuccessPagedResponse(
                    orderDetailDtos,
                    finalPageNumber,
                    finalPageSize,
                    totalItems,
                    "Order details retrieved successfully");
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = "Failed to retrieve order details",
                    Errors = new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        // Filtering logic
        private IEnumerable<OrderDetail> ApplyFilters(
            IEnumerable<OrderDetail> orderDetails,
            decimal? minUnitPrice,
            decimal? maxUnitPrice,
            int? minQuantity,
            int? maxQuantity,
            double? minDiscount,
            double? maxDiscount)
        {
            var query = orderDetails.AsQueryable();

            if (minUnitPrice.HasValue)
                query = query.Where(od => od.UnitPrice >= minUnitPrice.Value);

            if (maxUnitPrice.HasValue)
                query = query.Where(od => od.UnitPrice <= maxUnitPrice.Value);

            if (minQuantity.HasValue)
                query = query.Where(od => od.Quantity >= minQuantity.Value);

            if (maxQuantity.HasValue)
                query = query.Where(od => od.Quantity <= maxQuantity.Value);

            if (minDiscount.HasValue)
                query = query.Where(od => od.Discount >= minDiscount.Value);

            if (maxDiscount.HasValue)
                query = query.Where(od => od.Discount <= maxDiscount.Value);

            return query.ToList();
        }

        // Pagination logic
        private (int pageNumber, int pageSize, IEnumerable<OrderDetail> orderDetails) ApplyPagination(
            IEnumerable<OrderDetail> orderDetails,
            int? pageNumber,
            int? pageSize)
        {
            var actualPageNumber = pageNumber ?? 1;
            var actualPageSize = pageSize ?? orderDetails.Count();

            var pagedOrderDetails = orderDetails
                .Skip((actualPageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .ToList();

            return (actualPageNumber, actualPageSize, pagedOrderDetails);
        }

        // Invalid Page Response Helper
        private PagedApiResponse<OrderDetailDto> InvalidPageResponse(string message)
        {
            return new PagedApiResponse<OrderDetailDto>
            {
                Success = false,
                Message = message,
                Errors = new ErrorResponse("VALIDATION_ERROR", message)
            };
        }

    }
}
