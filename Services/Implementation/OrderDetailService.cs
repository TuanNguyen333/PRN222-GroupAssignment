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
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
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
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order detail not found",
                    new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

            // Fetch related Order and Product by their IDs
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderDetail.OrderId);
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(orderDetail.ProductId);

            if (order == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order not found",
                    new ErrorResponse("NOT_FOUND", "Order does not exist"));

            if (product == null)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Product not found",
                    new ErrorResponse("NOT_FOUND", "Product does not exist"));

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

        public async Task<ApiResponse<OrderDetailForCreationResponseDto>> CreateAsync(
            OrderDetailForCreationDto orderDetail)
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
                return ApiResponse<OrderDetailDto>.ErrorResponse("Order detail not found",
                    new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

            var validationResult = await _updateValidator.ValidateAsync(orderDetail);
            if (!validationResult.IsValid)
                return ApiResponse<OrderDetailDto>.ErrorResponse("Validation failed",
                    new ErrorResponse("VALIDATION_ERROR",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

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
                return ApiResponse<string>.ErrorResponse("Order detail not found",
                    new ErrorResponse("NOT_FOUND", "Order detail does not exist"));

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
                int actualPageSize = pageSize ?? 10; // Setting a reasonable default

                // Use a very large page size to get all records in one request
                // This is a workaround since we can't modify the repository
                var allOrderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(1, int.MaxValue);

                // Apply filters to all data
                var filteredOrderDetails = ApplyFilters(allOrderDetails, minUnitPrice, maxUnitPrice, minQuantity,
                    maxQuantity, minDiscount, maxDiscount);

                // Get the total count after filtering
                var totalItems = filteredOrderDetails.Count();

                // Apply pagination to the filtered results
                var pagedOrderDetails = filteredOrderDetails
                    .Skip((actualPageNumber - 1) * actualPageSize)
                    .Take(actualPageSize)
                    .ToList();

                // Map each OrderDetail to OrderDetailDto manually
                var orderDetailDtos = new List<OrderDetailDto>();
                foreach (var orderDetail in pagedOrderDetails)
                {
                    var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderDetail.OrderId);
                    var product = await _unitOfWork.ProductRepository.GetByIdAsync(orderDetail.ProductId);

                    var orderDetailDto = new OrderDetailDto
                    {
                        OrderId = orderDetail.OrderId,
                        OrderDto = _mapper.Map<OrderDto>(order),
                        ProductId = orderDetail.ProductId,
                        ProductDto = _mapper.Map<ProductDto>(product),
                        UnitPrice = orderDetail.UnitPrice,
                        Quantity = orderDetail.Quantity,
                        Discount = orderDetail.Discount
                    };

                    orderDetailDtos.Add(orderDetailDto);
                }

                return PagedApiResponse<OrderDetailDto>.SuccessPagedResponse(
                    orderDetailDtos,
                    actualPageNumber,
                    actualPageSize,
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

        public async Task<IActionResult> ExportOrderDetailsToExcel(
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

                // Fetch order details
                var allOrderDetails =
                    await _unitOfWork.OrderDetailRepository.GetAllAsync(actualPageNumber, actualPageSize);

                // Apply filters
                var filteredOrderDetails = ApplyFilters(allOrderDetails, minUnitPrice, maxUnitPrice, minQuantity,
                    maxQuantity, minDiscount, maxDiscount);

                // Map each OrderDetail to OrderDetailDto
                var orderDetailDtos = new List<OrderDetailDto>();
                foreach (var orderDetail in filteredOrderDetails)
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

                // Create the Excel file
                var excelPackage = new ExcelPackage();
                var worksheet = excelPackage.Workbook.Worksheets.Add("OrderDetails");

                // Set the header row
                worksheet.Cells[1, 1].Value = "OrderId";
                worksheet.Cells[1, 2].Value = "MemberId";
                worksheet.Cells[1, 3].Value = "OrderDate";
                worksheet.Cells[1, 4].Value = "RequiredDate";
                worksheet.Cells[1, 5].Value = "ShippedDate";
                worksheet.Cells[1, 6].Value = "Freight";
                worksheet.Cells[1, 7].Value = "ProductId";
                worksheet.Cells[1, 8].Value = "CategoryId";
                worksheet.Cells[1, 9].Value = "ProductName";
                worksheet.Cells[1, 10].Value = "Weight";
                worksheet.Cells[1, 11].Value = "UnitPrice";
                worksheet.Cells[1, 12].Value = "UnitsInStock";
                worksheet.Cells[1, 13].Value = "Quantity";
                worksheet.Cells[1, 14].Value = "Discount";

                // Set column widths
                worksheet.Column(3).Width = 15; // OrderDate
                worksheet.Column(4).Width = 15; // RequiredDate
                worksheet.Column(5).Width = 15; // ShippedDate
                worksheet.Column(9).Width = 20; // ProductName

                // Add data rows
                int row = 2;
                foreach (var dto in orderDetailDtos)
                {
                    worksheet.Cells[row, 1].Value = dto.OrderId;
                    worksheet.Cells[row, 2].Value = dto.OrderDto?.MemberId;
                    worksheet.Cells[row, 3].Value = dto.OrderDto?.OrderDate;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Cells[row, 4].Value = dto.OrderDto?.RequiredDate;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Cells[row, 5].Value = dto.OrderDto?.ShippedDate;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Cells[row, 6].Value = dto.OrderDto?.Freight;
                    worksheet.Cells[row, 7].Value = dto.ProductId;
                    worksheet.Cells[row, 8].Value = dto.ProductDto?.CategoryId;
                    worksheet.Cells[row, 9].Value = dto.ProductDto?.ProductName;
                    worksheet.Cells[row, 10].Value = dto.ProductDto?.Weight;
                    worksheet.Cells[row, 11].Value = dto.UnitPrice;
                    worksheet.Cells[row, 12].Value = dto.ProductDto?.UnitsInStock;
                    worksheet.Cells[row, 13].Value = dto.Quantity;
                    worksheet.Cells[row, 14].Value = dto.Discount;
                    row++;
                }

                // Save the Excel file to memory
                var stream = new MemoryStream();
                excelPackage.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"OrderDetails_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Create FileContentResult directly without using the File() method
                return new FileContentResult(stream.ToArray(), contentType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                    {
                        Success = false,
                        Message = "Failed to export order details to Excel",
                        Errors = new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                    })
                    { StatusCode = 500 };
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

        public async Task<PagedApiResponse<OrderDetailDto>> GetByOrderIdAsync(int orderId, int? pageNumber = null,
            int? pageSize = null)
        {
            try
            {
                // Validate input
                if (pageNumber.HasValue && pageNumber < 1)
                    return InvalidPageResponse("Page number must be greater than 0");

                if (pageSize.HasValue && pageSize < 1)
                    return InvalidPageResponse("Page size must be greater than 0");

                // Get all order details for the specified order
                var orderDetails = await _unitOfWork.OrderDetailRepository.GetByOrderIdAsync(orderId);
                if (orderDetails == null)
                {
                    return new PagedApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = "Order details not found",
                        Errors = new ErrorResponse("NOT_FOUND", "Order details do not exist")
                    };
                }

                var totalItems = orderDetails.Count();

                // Apply pagination
                var actualPageNumber = pageNumber ?? 1;
                var actualPageSize = pageSize ?? totalItems;

                var pagedOrderDetails = orderDetails
                    .Skip((actualPageNumber - 1) * actualPageSize)
                    .Take(actualPageSize);

                // Map to DTOs
                var orderDetailDtos = pagedOrderDetails.Select(od => new OrderDetailDto
                {
                    OrderId = od.OrderId,
                    OrderDto = _mapper.Map<OrderDto>(od.Order),
                    ProductId = od.ProductId,
                    ProductDto = _mapper.Map<ProductDto>(od.Product),
                    UnitPrice = od.UnitPrice,
                    Quantity = od.Quantity,
                    Discount = od.Discount
                });

                return PagedApiResponse<OrderDetailDto>.SuccessPagedResponse(
                    orderDetailDtos,
                    actualPageNumber,
                    actualPageSize,
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

        private PagedApiResponse<OrderDetailDto> InvalidPageResponse(string message)
        {
            return new PagedApiResponse<OrderDetailDto>
            {
                Success = false,
                Message = message,
                Errors = new ErrorResponse("VALIDATION_ERROR", "Invalid pagination parameters")
            };
        }

        public async Task<MemoryStream> ExportOrderDetailsToExcelAsync(DateTime startDate, DateTime endDate)
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Fetch order details based on date range
            var orderDetails = await GetOrderDetailsDataAsync(startDate, endDate);

            // Create Excel file
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("OrderDetails");

            // Add headers
            worksheet.Cells[1, 1].Value = "Order ID";
            worksheet.Cells[1, 2].Value = "Product ID";
            worksheet.Cells[1, 3].Value = "Unit Price";
            worksheet.Cells[1, 4].Value = "Quantity";
            worksheet.Cells[1, 5].Value = "Discount";
            worksheet.Cells[1, 6].Value = "Order Date";
            worksheet.Cells[1, 7].Value = "Required Date";
            worksheet.Cells[1, 8].Value = "Shipped Date";
            worksheet.Cells[1, 9].Value = "Freight";
            worksheet.Cells[1, 10].Value = "Member ID";
            worksheet.Cells[1, 11].Value = "Product Name";
            worksheet.Cells[1, 12].Value = "Category ID";
            worksheet.Cells[1, 13].Value = "Weight";
            worksheet.Cells[1, 14].Value = "Product Unit Price";
            worksheet.Cells[1, 15].Value = "Units In Stock";

            // Set column widths
            worksheet.Column(6).Width = 15; // Order Date
            worksheet.Column(7).Width = 15; // Required Date
            worksheet.Column(8).Width = 15; // Shipped Date

            // Add data
            for (int i = 0; i < orderDetails.Count; i++)
            {
                var orderDetail = orderDetails[i];
                worksheet.Cells[i + 2, 1].Value = orderDetail.OrderId;
                worksheet.Cells[i + 2, 2].Value = orderDetail.ProductId;
                worksheet.Cells[i + 2, 3].Value = orderDetail.UnitPrice;
                worksheet.Cells[i + 2, 4].Value = orderDetail.Quantity;
                worksheet.Cells[i + 2, 5].Value = orderDetail.Discount;
                worksheet.Cells[i + 2, 6].Value = orderDetail.OrderDto?.OrderDate;
                worksheet.Cells[i + 2, 6].Style.Numberformat.Format = "dd-mm-yyyy";
                worksheet.Cells[i + 2, 7].Value = orderDetail.OrderDto?.RequiredDate;
                worksheet.Cells[i + 2, 7].Style.Numberformat.Format = "dd-mm-yyyy";
                worksheet.Cells[i + 2, 8].Value = orderDetail.OrderDto?.ShippedDate;
                worksheet.Cells[i + 2, 8].Style.Numberformat.Format = "dd-mm-yyyy";
                worksheet.Cells[i + 2, 9].Value = orderDetail.OrderDto?.Freight;
                worksheet.Cells[i + 2, 10].Value = orderDetail.OrderDto?.MemberId;
                worksheet.Cells[i + 2, 11].Value = orderDetail.ProductDto?.ProductName;
                worksheet.Cells[i + 2, 12].Value = orderDetail.ProductDto?.CategoryId;
                worksheet.Cells[i + 2, 13].Value = orderDetail.ProductDto?.Weight;
                worksheet.Cells[i + 2, 14].Value = orderDetail.ProductDto?.UnitPrice;
                worksheet.Cells[i + 2, 15].Value = orderDetail.ProductDto?.UnitsInStock;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private async Task<List<OrderDetailDto>> GetOrderDetailsDataAsync(DateTime startDate, DateTime endDate)
        {
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(1, int.MaxValue);
            var filteredOrderDetails = orderDetails
                .Where(od => od.Order != null && od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
                .Select(od => new OrderDetailDto
                {
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    UnitPrice = od.UnitPrice,
                    Quantity = od.Quantity,
                    Discount = od.Discount,
                    OrderDto = od.Order != null
                        ? new OrderDto
                        {
                            OrderId = od.Order.OrderId,
                            OrderDate = od.Order.OrderDate,
                            Freight = od.Order.Freight,
                            MemberId = od.Order.MemberId,
                            RequiredDate = od.Order.RequiredDate,
                            ShippedDate = od.Order.ShippedDate
                        }
                        : null,
                    ProductDto = new ProductDto
                    {
                        ProductId = od.Product.ProductId,
                        ProductName = od.Product.ProductName,
                        CategoryId = od.Product.CategoryId,
                        Weight = od.Product.Weight,
                        UnitPrice = od.Product.UnitPrice,
                        UnitsInStock = od.Product.UnitsInStock
                    }
                })
                .ToList();
            return filteredOrderDetails;
        }
    }
}