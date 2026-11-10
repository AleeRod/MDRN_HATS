namespace MDRN_HATS.Application.Contracts;

public sealed record ProductVariantInput(string Color, string Size, int Stock, string Sku);

public sealed record ProductImageInput(string Url, string AltText, int SortOrder);

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal BasePrice,
    IReadOnlyList<ProductVariantInput> Variants,
    IReadOnlyList<ProductImageInput> Images);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal BasePrice,
    IReadOnlyList<ProductVariantInput> Variants,
    IReadOnlyList<ProductImageInput> Images);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal BasePrice,
    IReadOnlyList<ProductVariantInput> Variants,
    IReadOnlyList<ProductImageInput> Images);
