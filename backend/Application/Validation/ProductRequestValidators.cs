using FluentValidation;
using MDRN_HATS.Application.Contracts;

namespace MDRN_HATS.Application.Validation;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).NotNull().MaximumLength(4000);
        RuleFor(request => request.BasePrice).GreaterThan(0);
        RuleFor(request => request.Variants).NotNull();
        RuleFor(request => request.Variants)
            .Must(variants => variants is null ||
                variants.Select(variant => variant.Sku).Distinct(StringComparer.OrdinalIgnoreCase).Count() == variants.Count)
            .WithMessage("Variant SKUs must be unique within a product.");
        RuleFor(request => request.Images).NotNull();
        RuleForEach(request => request.Variants).ChildRules(variant =>
        {
            variant.RuleFor(item => item.Color).NotEmpty().MaximumLength(80);
            variant.RuleFor(item => item.Size).NotEmpty().MaximumLength(40);
            variant.RuleFor(item => item.Stock).GreaterThanOrEqualTo(0);
            variant.RuleFor(item => item.Sku).NotEmpty().MaximumLength(100);
        });
        RuleForEach(request => request.Images).ChildRules(image =>
        {
            image.RuleFor(item => item.Url)
                .NotEmpty()
                .MaximumLength(2048)
                .Must(IsAbsoluteUrl);
            image.RuleFor(item => item.AltText).MaximumLength(200);
            image.RuleFor(item => item.SortOrder).GreaterThanOrEqualTo(0);
        });
    }

    internal static bool IsAbsoluteUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).NotNull().MaximumLength(4000);
        RuleFor(request => request.BasePrice).GreaterThan(0);
        RuleFor(request => request.Variants).NotNull();
        RuleFor(request => request.Variants)
            .Must(variants => variants is null ||
                variants.Select(variant => variant.Sku).Distinct(StringComparer.OrdinalIgnoreCase).Count() == variants.Count)
            .WithMessage("Variant SKUs must be unique within a product.");
        RuleFor(request => request.Images).NotNull();
        RuleForEach(request => request.Variants).ChildRules(variant =>
        {
            variant.RuleFor(item => item.Color).NotEmpty().MaximumLength(80);
            variant.RuleFor(item => item.Size).NotEmpty().MaximumLength(40);
            variant.RuleFor(item => item.Stock).GreaterThanOrEqualTo(0);
            variant.RuleFor(item => item.Sku).NotEmpty().MaximumLength(100);
        });
        RuleForEach(request => request.Images).ChildRules(image =>
        {
            image.RuleFor(item => item.Url)
                .NotEmpty()
                .MaximumLength(2048)
                .Must(CreateProductRequestValidator.IsAbsoluteUrl);
            image.RuleFor(item => item.AltText).MaximumLength(200);
            image.RuleFor(item => item.SortOrder).GreaterThanOrEqualTo(0);
        });
    }
}
