using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MDRN_HATS.Application.Validation;

namespace MDRN_HATS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        return services;
    }
}
