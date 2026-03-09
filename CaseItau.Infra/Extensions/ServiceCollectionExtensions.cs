using CaseItau.Domain.Interfaces;
using CaseItau.Infra.Data;
using CaseItau.Infra.Interfaces;
using CaseItau.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CaseItau.Infra.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQL Server DbContext and all Infrastructure repositories.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration used to resolve the connection string.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IFundoRepository, FundoRepository>();

        return services;
    }

    public static IHostBuilder MigrateDatabase(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
                logger.LogInformation("Database migration applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }
        });
        return hostBuilder;
    }

    public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                    .AddSource("*")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint); // OTLP HTTP endpoint
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });
        return services;
    }
}
