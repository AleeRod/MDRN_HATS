using FluentValidation;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Application.Validation;

public sealed class DropValidator : AbstractValidator<Drop>
{
    private const string SlugPattern = "^[a-z0-9-]+$";
    private const string ColorPattern = "^#[0-9A-Fa-f]{6}$";

    public DropValidator()
    {
        RuleFor(drop => drop.Slug)
            .NotEmpty()
            .MaximumLength(80)
            .Matches(SlugPattern);

        RuleFor(drop => drop.CollaboratorName)
            .MaximumLength(120);

        RuleFor(drop => drop.PrimaryColor).Must(BeValidOptionalColor);
        RuleFor(drop => drop.SecondaryColor).Must(BeValidOptionalColor);
        RuleFor(drop => drop.BackgroundColor).Must(BeValidOptionalColor);
        RuleFor(drop => drop.TextColor).Must(BeValidOptionalColor);
        RuleFor(drop => drop.ButtonColor).Must(BeValidOptionalColor);
    }

    private static bool BeValidOptionalColor(string? color) =>
        color is null || System.Text.RegularExpressions.Regex.IsMatch(color, ColorPattern);
}
