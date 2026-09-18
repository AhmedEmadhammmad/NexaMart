using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Infrastructure.Data.Context;
using NexaMart.Infrastructure.Repositories;
using NexaMart.Infrastructure.Security;

namespace NexaMart.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure, custom security, and application services into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Retrieve the connection string strictly from appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

        // Register EF Core DbContext with SQL Server
        services.AddDbContext<NexaMartDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(NexaMartDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3, 
                    maxRetryDelay: TimeSpan.FromSeconds(5), 
                    errorNumbersToAdd: null);
            }));

        // Register Custom Security Services (BCrypt Password Hasher & JWT Token Service)
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register Generic Repository and Unit of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
