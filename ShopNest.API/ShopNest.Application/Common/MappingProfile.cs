using AutoMapper;
using ShopNest.Application.Dtos;
using ShopNest.Domain.Entities;

namespace ShopNest.Application.Common;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        
        CreateMap<ProductImage, ProductImageDto>();
        
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Product.Price * src.Quantity));

        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(x => x.Product.Price * x.Quantity)));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

        CreateMap<Payment, PaymentDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment));
    }
}
