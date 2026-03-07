using CaseItau.Application.Interfaces;
using CaseItau.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CaseItau.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Application layer services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFundoService, FundoService>();
        return services;
    }
}
