using FluentValidation;
using MDRN_HATS.Application.Contracts;

namespace MDRN_HATS.Application.Validation;

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
    }
}
