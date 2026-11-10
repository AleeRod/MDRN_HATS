using Microsoft.EntityFrameworkCore;
using MDRN_HATS.Application.Contracts;
using MDRN_HATS.Application.Exceptions;
using MDRN_HATS.Application.Interfaces;
using MDRN_HATS.Domain.Entities;
using MDRN_HATS.Infrastructure.Persistence;

namespace MDRN_HATS.Infrastructure.Services;

public sealed class ProductService(AppDbContext dbContext) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Active)
            .Include(product => product.Variants)
            .Include(product => product.Images)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(item => item.Id == id && item.Active)
            .Include(item => item.Variants)
            .Include(item => item.Images)
            .SingleOrDefaultAsync(cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureSkusAvailableAsync(request.Variants, null, cancellationToken);
        var product = new Product();
        ApplyDetails(product, request.Name, request.Description, request.BasePrice);
        AddVariants(product, request.Variants);
        AddImages(product, request.Images);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(item => item.Variants)
            .Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == id && item.Active, cancellationToken);
        if (product is null)
        {
            return null;
        }

        await EnsureSkusAvailableAsync(request.Variants, id, cancellationToken);
        var variantsBySku = product.Variants.ToDictionary(
            variant => variant.Sku,
            StringComparer.OrdinalIgnoreCase);
        var incomingSkus = request.Variants
            .Select(variant => variant.Sku.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var variant in product.Variants.Where(variant => !incomingSkus.Contains(variant.Sku)).ToList())
        {
            dbContext.ProductVariants.Remove(variant);
        }

        dbContext.ProductImages.RemoveRange(product.Images);
        product.Images.Clear();
        ApplyDetails(product, request.Name, request.Description, request.BasePrice);
        foreach (var input in request.Variants)
        {
            if (variantsBySku.TryGetValue(input.Sku.Trim(), out var existingVariant))
            {
                existingVariant.Color = input.Color.Trim();
                existingVariant.Size = input.Size.Trim();
                existingVariant.Stock = input.Stock;
                existingVariant.Sku = input.Sku.Trim();
                continue;
            }

            product.Variants.Add(new ProductVariant
            {
                Color = input.Color.Trim(),
                Size = input.Size.Trim(),
                Stock = input.Stock,
                Sku = input.Sku.Trim()
            });
        }

        AddImages(product, request.Images);
        product.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(item => item.Id == id && item.Active, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Active = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureSkusAvailableAsync(
        IReadOnlyList<ProductVariantInput> variants,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var skus = variants.Select(variant => variant.Sku.Trim()).ToArray();
        if (skus.Length == 0)
        {
            return;
        }

        var conflict = await dbContext.ProductVariants
            .AnyAsync(
                variant => skus.Contains(variant.Sku) &&
                           (productId == null || variant.ProductId != productId),
                cancellationToken);
        if (conflict)
        {
            throw new AppException("A product variant with one of these SKUs already exists.", 409);
        }
    }

    private static void ApplyDetails(
        Product product,
        string name,
        string description,
        decimal basePrice)
    {
        product.Name = name.Trim();
        product.Description = description.Trim();
        product.BasePrice = basePrice;
    }

    private static void AddVariants(Product product, IReadOnlyList<ProductVariantInput> variants)
    {
        foreach (var variant in variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Color = variant.Color.Trim(),
                Size = variant.Size.Trim(),
                Stock = variant.Stock,
                Sku = variant.Sku.Trim()
            });
        }
    }

    private static void AddImages(Product product, IReadOnlyList<ProductImageInput> images)
    {
        foreach (var image in images)
        {
            product.Images.Add(new ProductImage
            {
                Url = image.Url.Trim(),
                AltText = image.AltText.Trim(),
                SortOrder = image.SortOrder
            });
        }
    }

    private static ProductResponse Map(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.BasePrice,
            product.Variants
                .OrderBy(variant => variant.Size)
                .ThenBy(variant => variant.Color)
                .Select(variant => new ProductVariantInput(
                    variant.Color, variant.Size, variant.Stock, variant.Sku))
                .ToList(),
            product.Images
                .OrderBy(image => image.SortOrder)
                .Select(image => new ProductImageInput(image.Url, image.AltText, image.SortOrder))
                .ToList());
}
