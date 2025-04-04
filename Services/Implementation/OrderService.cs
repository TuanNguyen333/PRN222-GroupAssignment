using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using FluentValidation;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Dto.Order;
using BusinessObjects.Entities;
using OfficeOpenXml;

namespace Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderForCreationDto> _creationValidator;
        private readonly IValidator<OrderForUpdateDto> _updateValidator;
        private const int DEFAULT_PAGE_SIZE = 10;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<OrderForCreationDto> creationValidator, IValidator<OrderForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _creationValidator = creationValidator ?? throw new ArgumentNullException(nameof(creationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<ApiResponse<OrderDto>> GetByIdAsync(int id)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(id);
                if (order == null)
                    return ApiResponse<OrderDto>.ErrorResponse($"Order with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Order does not exist"));

                var orderDto = _mapper.Map<OrderDto>(order);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Failed to retrieve order",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<PagedApiResponse<OrderDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page number must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number"));

                if (pageSize < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page size must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size"));

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(pageNumber, pageSize);
                var allOrders = await _unitOfWork.OrderRepository.GetAllAsync(1, int.MaxValue);
                var totalItems = allOrders.Count();

                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
                return PagedApiResponse<OrderDto>.SuccessPagedResponse(
                    orderDtos,
                    pageNumber,
                    pageSize,
                    totalItems,
                    "Orders retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Failed to retrieve orders",
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        public async Task<ApiResponse<OrderDto>> CreateAsync(OrderForCreationDto order)
        {
            try
            {
                if (order == null)
                    return ApiResponse<OrderDto>.ErrorResponse("Order cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var validationResult = await _creationValidator.ValidateAsync(order);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<OrderDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                await _unitOfWork.BeginTransactionAsync();

                var orderEntity = _mapper.Map<Order>(order);
                // Add userId to order entity if applicable (assuming Order has a UserId property)
                // orderEntity.UserId = userId;

                // Increment the ID by 1
                var lastOrder = await _unitOfWork.OrderRepository.GetAllAsync(1, int.MaxValue);
                orderEntity.OrderId = lastOrder.Max(o => o.OrderId) + 1;

                _unitOfWork.OrderRepository.Add(orderEntity);
                await _unitOfWork.CommitTransactionAsync();

                var orderDto = _mapper.Map<OrderDto>(orderEntity);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<OrderDto>.ErrorResponse("Failed to create order",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<OrderDto>> UpdateAsync(int id, OrderForUpdateDto order)
        {
            try
            {
                if (order == null)
                    return ApiResponse<OrderDto>.ErrorResponse("Order cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var validationResult = await _updateValidator.ValidateAsync(order);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return ApiResponse<OrderDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                var existingOrder = await _unitOfWork.OrderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                    return ApiResponse<OrderDto>.ErrorResponse($"Order with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Order does not exist"));

                // Optionally verify userId matches if Order has user ownership
                // if (existingOrder.UserId != userId)
                //     return ApiResponse<OrderDto>.ErrorResponse("Unauthorized",
                //         new BusinessObject.Base.ErrorResponse("UNAUTHORIZED", "User not authorized to update this order"));

                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(order, existingOrder);
                _unitOfWork.OrderRepository.Update(existingOrder);

                await _unitOfWork.CommitTransactionAsync();

                var orderDto = _mapper.Map<OrderDto>(existingOrder);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order updated successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<OrderDto>.ErrorResponse("Failed to update order",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id, int userId)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(id);
                if (order == null)
                    return ApiResponse<string>.ErrorResponse($"Order with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Order does not exist"));

                // Optionally verify userId matches if Order has user ownership
                // if (order.UserId != userId)
                //     return ApiResponse<string>.ErrorResponse("Unauthorized",
                //         new BusinessObject.Base.ErrorResponse("UNAUTHORIZED", "User not authorized to delete this order"));

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.OrderRepository.Delete(order);
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.SuccessResponse(null, "Order deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.ErrorResponse("Failed to delete order",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<PagedApiResponse<OrderDto>> GetAllAsync(
           int? pageNumber = null,
           int? pageSize = null,
           decimal? minFreight = null,
           decimal? maxFreight = null,
           DateTime? minOrderDate = null,
           DateTime? maxOrderDate = null)
        {
            try
            {
                pageNumber = pageNumber ?? 1;
                pageSize = pageSize ?? DEFAULT_PAGE_SIZE;

                if (pageNumber < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page number must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number"));

                if (pageSize < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page size must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size"));

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(pageNumber.Value, pageSize.Value);
                var allOrders = await _unitOfWork.OrderRepository.GetAllAsync(1, int.MaxValue);

                // Apply filters
                if (minOrderDate.HasValue)
                    allOrders = allOrders.Where(o => o.OrderDate >= minOrderDate.Value);
                if (maxOrderDate.HasValue)
                    allOrders = allOrders.Where(o => o.OrderDate <= maxOrderDate.Value);
                if (minFreight.HasValue)
                    allOrders = allOrders.Where(o => o.Freight >= minFreight.Value);
                if (maxFreight.HasValue)
                    allOrders = allOrders.Where(o => o.Freight <= maxFreight.Value);

                var totalItems = allOrders.Count();

                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
                return PagedApiResponse<OrderDto>.SuccessPagedResponse(
                    orderDtos,
                    pageNumber.Value,
                    pageSize.Value,
                    totalItems,
                    "Orders retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Failed to retrieve orders",
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        private async Task<IEnumerable<Order>> GetAllOrdersWithPaginationFallback()
        {
            try
            {
                pageNumber = pageNumber ?? 1;
                pageSize = pageSize ?? DEFAULT_PAGE_SIZE;

                if (pageNumber < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page number must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number"));

                if (pageSize < 1)
                    return (PagedApiResponse<OrderDto>)PagedApiResponse<OrderDto>.ErrorResponse("Page size must be greater than 0",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size"));

                // Get filtered data with total count
                var (filteredOrders, totalCount) = await _unitOfWork.OrderRepository.GetOrdersByUserIdAsync(
                    userId,
                    pageNumber.Value,
                    pageSize.Value,
                    minFreight,
                    maxFreight,
                    minOrderDate,
                    maxOrderDate);

                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(filteredOrders);
                return PagedApiResponse<OrderDto>.SuccessPagedResponse(
                    orderDtos,
                    pageNumber.Value,
                    pageSize.Value,
                    totalCount,
                    "Orders retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Failed to retrieve orders",
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }
        
        
        public async Task<MemoryStream> ExportSalesToExcelAsync(DateTime startDate, DateTime endDate)
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Fetch sales data based on date range
            var salesData = await GetSalesDataAsync(startDate, endDate);

            // Create Excel file
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("SalesData");

            // Add headers
            worksheet.Cells[1, 1].Value = "Order ID";
            worksheet.Cells[1, 2].Value = "Order Date";
            worksheet.Cells[1, 3].Value = "Freight";
            worksheet.Cells[1, 4].Value = "Member ID";
            worksheet.Cells[1, 5].Value = "Required Date";
            worksheet.Cells[1, 6].Value = "Shipped Date";

            // Set column widths
            worksheet.Column(2).Width = 15; // Order Date
            worksheet.Column(5).Width = 15; // Required Date
            worksheet.Column(6).Width = 15; // Shipped Date

            // Add data
            for (int i = 0; i < salesData.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = salesData[i].OrderId;
                worksheet.Cells[i + 2, 2].Value = salesData[i].OrderDate;
                worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[i + 2, 3].Value = salesData[i].Freight;
                worksheet.Cells[i + 2, 4].Value = salesData[i].MemberId;
                worksheet.Cells[i + 2, 5].Value = salesData[i].RequiredDate;
                worksheet.Cells[i + 2, 5].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[i + 2, 6].Value = salesData[i].ShippedDate;
                worksheet.Cells[i + 2, 6].Style.Numberformat.Format = "yyyy-mm-dd";
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }
        private async Task<List<OrderDto>> GetSalesDataAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(1, int.MaxValue);
            var filteredOrders = orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Freight = o.Freight,
                    MemberId = o.MemberId,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate
                })
                .ToList(); 
            return filteredOrders;
        }

    }
}