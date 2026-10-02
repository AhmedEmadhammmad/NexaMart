using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;
using NexaMart.Infrastructure.Data.Context;

namespace NexaMart.Infrastructure.Data.Seed;

/// <summary>
/// Database initializer seeding core staff accounts, 20 catalog categories, and 500 realistic products.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NexaMartDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NexaMartDbContext>>();

        // Ensure database exists and migrations are applied
        await context.Database.MigrateAsync();

        // Ensure all existing product ratings and review counts are zeroed out as required
        try
        {
            await context.Database.ExecuteSqlRawAsync("UPDATE Products SET AverageRating = 0.0, ReviewCount = 0; DELETE FROM Reviews; UPDATE Users SET IsEmailConfirmed = 1 WHERE IsEmailConfirmed = 0;");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not execute rating reset or user confirmation SQL directly: {Message}", ex.Message);
        }

        // 1. Seed Core Administrative and Customer Accounts (SuperAdmin, Admin, Customer only)
        var supportUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "support@nexamart.com");
        if (supportUser != null)
        {
            context.Users.Remove(supportUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Removed obsolete Support Specialist account.");
        }

        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Seeding default core staff and sample users...");

            var users = new List<ApplicationUser>
            {
                new()
                {
                    FullName = "Super Administrator",
                    Email = "superadmin@nexamart.com",
                    PasswordHash = passwordHasher.HashPassword("SuperAdmin@123"),
                    PhoneNumber = "+201000000001",
                    RoleType = UserRoleType.SuperAdmin,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    FullName = "System Administrator",
                    Email = "admin@nexamart.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    PhoneNumber = "+201000000002",
                    RoleType = UserRoleType.Admin,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    FullName = "Abdo Hassan",
                    Email = "customer@nexamart.com",
                    PasswordHash = passwordHasher.HashPassword("Customer@123"),
                    PhoneNumber = "+201111111111",
                    RoleType = UserRoleType.Customer,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("Default users seeded successfully (SuperAdmin, Admin, Customer).");
        }

        // 2. Seed 20 Core Categories
        if (!await context.Categories.AnyAsync())
        {
            logger.LogInformation("Seeding 20 core categories...");

            var categoryDefinitions = new (string Name, string Description, string ImageUrl, int DisplayOrder)[]
            {
                ("Smartphones & Tablets", "Latest flagship phones, 5G tablets, and mobile devices", "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=600&q=80", 1),
                ("Laptops & Computers", "High-performance ultrabooks, workstations, and desktop accessories", "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=600&q=80", 2),
                ("Smart Watches & Wearables", "Health tracking smartwatches, bands, and fitness trackers", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=600&q=80", 3),
                ("Audio & Headphones", "Noise-cancelling wireless headphones, earbuds, and speakers", "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80", 4),
                ("Cameras & Photography", "Mirrorless cameras, studio lenses, and cinema equipment", "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=600&q=80", 5),
                ("Gaming Consoles & Gear", "Next-gen consoles, RGB mechanical keyboards, and mice", "https://images.unsplash.com/photo-1612287233207-6b60c4bc75df?auto=format&fit=crop&w=600&q=80", 6),
                ("Television & Home Theater", "4K OLED displays, cinematic soundbars, and streaming setups", "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?auto=format&fit=crop&w=600&q=80", 7),
                ("Smart Home & IoT", "Automated lighting, smart plugs, sensors, and security hubs", "https://images.unsplash.com/photo-1558002038-1055907df827?auto=format&fit=crop&w=600&q=80", 8),
                ("Men's Fashion", "Premium menswear, jackets, formal apparel, and streetwear", "https://images.unsplash.com/photo-1516257984-b1b4d707412e?auto=format&fit=crop&w=600&q=80", 9),
                ("Women's Fashion", "Modern chic dresses, coats, everyday wear, and designer outfits", "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=600&q=80", 10),
                ("Footwear & Shoes", "Running sneakers, leather boots, loafers, and athletic shoes", "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=600&q=80", 11),
                ("Watches & Jewelry", "Luxury chronographs, gold jewelry, and bespoke bracelets", "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?auto=format&fit=crop&w=600&q=80", 12),
                ("Kitchen & Dining", "Espresso makers, chef cookware sets, and smart kitchen tools", "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=600&q=80", 13),
                ("Home Appliances", "Robot vacuums, air purifiers, and cordless washing gear", "https://images.unsplash.com/photo-1585659722983-3a675dabf23d?auto=format&fit=crop&w=600&q=80", 14),
                ("Furniture & Decor", "Ergonomic office chairs, standing desks, and ambient lamps", "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?auto=format&fit=crop&w=600&q=80", 15),
                ("Fitness & Exercise", "Adjustable dumbbells, yoga accessories, and cardio gear", "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=600&q=80", 16),
                ("Outdoor & Camping", "Waterproof backpacking tents, hiking gear, and trail essentials", "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4?auto=format&fit=crop&w=600&q=80", 17),
                ("Beauty & Personal Care", "Organic skincare serums, hair styling tools, and perfumes", "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=600&q=80", 18),
                ("Books & Stationery", "Bestselling hardcovers, luxury fountain pens, and leather journals", "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=600&q=80", 19),
                ("Automotive Accessories", "Wireless dashcams, fast car chargers, and detailing kits", "https://images.unsplash.com/photo-1563720223185-11003d516935?auto=format&fit=crop&w=600&q=80", 20)
            };

            var categories = categoryDefinitions.Select(c => new Category
            {
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("20 categories seeded successfully.");
        }

        // 3. Seed 500 Products (25 Products per Category)
        if (!await context.Products.AnyAsync())
        {
            logger.LogInformation("Seeding 500 products (25 products across each of the 20 categories)...");

            var categories = await context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            var random = new Random(42); // Seeded random for reproducible pricing and inventory
            var products = new List<Product>();

            var adjectives = new[] { "Ultra", "Pro", "Elite", "Prime", "Nexus", "Aero", "Pulse", "Vanguard", "Nova", "Titan" };
            var features = new[] { "Wireless", "Compact", "Ergonomic", "Smart", "Precision", "Adaptive", "High-Speed", "Eco-Friendly" };

            foreach (var category in categories)
            {
                var categoryPrefix = GetCategoryProductPrefix(category.Name);

                for (int i = 1; i <= 25; i++)
                {
                    var adj = adjectives[random.Next(adjectives.Length)];
                    var feat = features[random.Next(features.Length)];
                    var productName = $"{adj} {categoryPrefix} {feat} Model-{i + 100}";
                    var price = Math.Round((decimal)(random.Next(250, 18500) + random.NextDouble()), 2);
                    var stock = random.Next(15, 250);
                    var rating = 0.0m;
                    var reviews = 0;

                    products.Add(new Product
                    {
                        CategoryId = category.Id,
                        Name = productName,
                        Description = $"Experience top-tier quality with {productName}. Designed for performance, durability, and daily reliability in our {category.Name} collection.",
                        ImageUrl = category.ImageUrl,
                        Price = price,
                        StockQuantity = stock,
                        AverageRating = rating,
                        ReviewCount = reviews,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                    });
                }
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            logger.LogInformation("500 products seeded successfully!");
        }
    }

    private static string GetCategoryProductPrefix(string categoryName)
    {
        return categoryName switch
        {
            "Smartphones & Tablets" => "Phone / Tablet",
            "Laptops & Computers" => "Computer Unit",
            "Smart Watches & Wearables" => "Smartwatch Edition",
            "Audio & Headphones" => "Acoustic Headset",
            "Cameras & Photography" => "Digital Camera Lens",
            "Gaming Consoles & Gear" => "Gaming Controller",
            "Television & Home Theater" => "4K OLED Display",
            "Smart Home & IoT" => "Automation Node",
            "Men's Fashion" => "Tailored Garment",
            "Women's Fashion" => "Chic Ensemble",
            "Footwear & Shoes" => "Sneaker Pair",
            "Watches & Jewelry" => "Timepiece",
            "Kitchen & Dining" => "Culinary Tool",
            "Home Appliances" => "Purifier Machine",
            "Furniture & Decor" => "Studio Desk / Chair",
            "Fitness & Exercise" => "Training Weight",
            "Outdoor & Camping" => "Expedition Pack",
            "Beauty & Personal Care" => "Organic Serum",
            "Books & Stationery" => "Hardcover Edition",
            "Automotive Accessories" => "Vehicle Dashcam",
            _ => "Premium Goods"
        };
    }
}
