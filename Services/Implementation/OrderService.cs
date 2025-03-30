using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Dto.Order;
using BusinessObjects.Entities;

namespace Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const int DEFAULT_PAGE_SIZE = 10;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

        public async Task<ApiResponse<OrderDto>> CreateAsync(OrderForCreationDto order, int userId)
        {
            try
            {
                if (order == null)
                    return ApiResponse<OrderDto>.ErrorResponse("Order cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                await _unitOfWork.BeginTransactionAsync();

                var orderEntity = _mapper.Map<Order>(order);
                // Add userId to order entity if applicable (assuming Order has a UserId property)
                // orderEntity.UserId = userId;

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

        public async Task<ApiResponse<OrderDto>> UpdateAsync(int id, OrderForUpdateDto order, int userId)
        {
            try
            {
                if (order == null)
                    return ApiResponse<OrderDto>.ErrorResponse("Order cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

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
    }
}