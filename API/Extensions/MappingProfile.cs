using AutoMapper;
using BusinessObjects.Dto.Category;
using BusinessObjects.Dto.Member;
using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Dto.Product;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Product
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));
            CreateMap<ProductForCreationDto, Product>();
            CreateMap<ProductForUpdateDto, Product>();
            #endregion

            #region Category
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));
            CreateMap<CategoryForCreationDto, Category>();
            CreateMap<CategoryForUpdateDto, Category>();
            #endregion

            #region Member
            CreateMap<Member, MemberDto>()
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId));
            CreateMap<MemberForCreationDto, Member>();
            CreateMap<MemberForUpdateDto, Member>();
            #endregion

            #region Order
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId));
            CreateMap<OrderForCreationDto, Order>();
            CreateMap<OrderForUpdateDto, Order>();
            #endregion
            
            #region OrderDetail
            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.OrderDto, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductDto, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount));
            #endregion
        }
    }
}