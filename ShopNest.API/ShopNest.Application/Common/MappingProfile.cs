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
            .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Product.Brand != null ? src.Product.Brand.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product.Category != null ? src.Product.Category.Name : string.Empty))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.Images != null && src.Product.Images.Any() ? (src.Product.Images.FirstOrDefault(x => x.IsPrimary)!.Url ?? src.Product.Images.FirstOrDefault()!.Url) : null))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Product.Price + src.Product.DiscountValue))
            .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Product.Price * src.Quantity))
            .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => src.Product.StockQuantity > 0 ? "InStock" : "OutOfStock"))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.Product.StockQuantity));

        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Items.Sum(x => x.Product.Price * x.Quantity)))
            .ForMember(dest => dest.TotalDiscount, opt => opt.MapFrom(src => src.Items.Sum(x => x.Product.DiscountValue * x.Quantity)))
            .ForMember(dest => dest.AppliedCouponCode, opt => opt.MapFrom(src => src.AppliedCouponCode))
            .ForMember(dest => dest.CouponDiscount, opt => opt.MapFrom(src => 0m))
            .ForMember(dest => dest.ShippingCharges, opt => opt.MapFrom(src => (src.Items.Sum(x => x.Product.Price * x.Quantity) > 0 && src.Items.Sum(x => x.Product.Price * x.Quantity) < 100) ? 10m : 0m))
            .ForMember(dest => dest.EstimatedTax, opt => opt.MapFrom(src => src.Items.Sum(x => x.Product.Price * x.Quantity) * 0.10m))
            .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src =>
                src.Items.Sum(x => x.Product.Price * x.Quantity) +
                ((src.Items.Sum(x => x.Product.Price * x.Quantity) > 0 && src.Items.Sum(x => x.Product.Price * x.Quantity) < 100) ? 10m : 0m) +
                (src.Items.Sum(x => x.Product.Price * x.Quantity) * 0.10m)
            ));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Total == 0 ? src.UnitPrice * src.Quantity : src.Total))
            .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId))
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.Tax))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total == 0 ? src.UnitPrice * src.Quantity : src.Total));

        CreateMap<Payment, PaymentDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
            .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.BillingAddress))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
            .ForMember(dest => dest.CourierPartner, opt => opt.MapFrom(src => src.CourierPartner))
            .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost))
            .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.Tax))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.OrderNotes, opt => opt.MapFrom(src => src.OrderNotes))
            .ForMember(dest => dest.EstimatedDeliveryDate, opt => opt.MapFrom(src => src.EstimatedDeliveryDate))
            .ForMember(dest => dest.DeliveredDate, opt => opt.MapFrom(src => src.DeliveredDate));

        CreateMap<UserProfile, UserProfileDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.User.MobileNumber));

        CreateMap<UserAddress, UserAddressDto>();
        CreateMap<AddAddressRequest, UserAddress>();

        CreateMap<AppUser, AdminUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified));

        CreateMap<Role, AdminRoleDto>();

        CreateMap<Category, AdminCategoryDto>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children.Count));

        CreateMap<Category, CategoryNodeDto>();

        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<ProductVariant, AdminProductVariantDto>();

        CreateMap<Product, AdminProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Name : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(x => x.DisplayOrder)))
            .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants));
    }
}
