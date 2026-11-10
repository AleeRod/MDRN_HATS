using MDRN_HATS.Application.Contracts;
using MDRN_HATS.Application.Validation;

namespace MDRN_HATS.Tests;

public sealed class ProductValidationTests
{
    [Fact]
    public void CreateRequest_AcceptsValidProduct()
    {
        var request = new CreateProductRequest(
            "Classic hat",
            "A simple product description.",
            49.99m,
            [new ProductVariantInput("Black", "M", 8, "HAT-BLK-M")],
            [new ProductImageInput("https://example.com/hat.jpg", "Black hat", 0)]);

        var result = new CreateProductRequestValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateRequest_RejectsInvalidPriceVariantAndImageUrl()
    {
        var request = new CreateProductRequest(
            "",
            "",
            -1,
            [new ProductVariantInput("", "", -1, "")],
            [new ProductImageInput("not-a-url", "", -1)]);

        var result = new CreateProductRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.BasePrice));
        Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(request.Variants)));
        Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(request.Images)));
    }
}
