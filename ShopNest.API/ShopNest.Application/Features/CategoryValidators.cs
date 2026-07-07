using FluentValidation;
using ShopNest.Application.Dtos;

namespace ShopNest.Application.Features.Categories;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(120).WithMessage("Category name must not exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(250).WithMessage("Short description must not exceed 250 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be a positive integer.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(150).WithMessage("Meta Title must not exceed 150 characters.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(250).WithMessage("Meta Description must not exceed 250 characters.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(250).WithMessage("Meta Keywords must not exceed 250 characters.");
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(120).WithMessage("Category name must not exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(250).WithMessage("Short description must not exceed 250 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be a positive integer.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(150).WithMessage("Meta Title must not exceed 150 characters.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(250).WithMessage("Meta Description must not exceed 250 characters.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(250).WithMessage("Meta Keywords must not exceed 250 characters.");
    }
}
