using Microsoft.Extensions.DependencyInjection;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Application.Services;

namespace NexaMart.Application.Extensions;

/// <summary>
/// Extension methods for registering core Application layer services into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISuperAdminService, SuperAdminService>();

        return services;
    }
}
