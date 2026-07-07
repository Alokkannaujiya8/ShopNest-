using FluentValidation;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Features.Products;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost Price must be positive.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be positive.");

        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Tax percentage must be positive.")
            .LessThanOrEqualTo(100).WithMessage("Tax percentage cannot exceed 100%.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleForEach(x => x.Variants).SetValidator(new UpsertVariantRequestValidator());
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost Price must be positive.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be positive.");

        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Tax percentage must be positive.")
            .LessThanOrEqualTo(100).WithMessage("Tax percentage cannot exceed 100%.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleForEach(x => x.Variants).SetValidator(new UpsertVariantRequestValidator());
    }
}

public sealed class UpsertVariantRequestValidator : AbstractValidator<UpsertVariantRequest>
{
    public UpsertVariantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Variant name is required.")
            .MaximumLength(100).WithMessage("Variant name must not exceed 100 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Variant SKU is required.")
            .MaximumLength(50).WithMessage("Variant SKU must not exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Variant price must be greater than zero.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Variant stock quantity must be positive.");
    }
}
