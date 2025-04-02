using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Dto.OrderDetail;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDetailDto> GetOrderDetailAsync(int orderId, int productId)
        {
            var orderDetail = await _unitOfWork.OrderDetailRepository.GetByOrderAndProductIdAsync(orderId, productId);
            if (orderDetail == null)
            {
                return null;
            }
            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return orderDetailDto;
        }
    }
}