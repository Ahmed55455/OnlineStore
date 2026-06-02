using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed Categories and Products
    if (!context.Categories.Any())
    {
        var categories = new List<Category>
        {
            new Category { Name = "Electronics" },
            new Category { Name = "Clothing" },
            new Category { Name = "Books" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        var electronics = context.Categories.First(c => c.Name == "Electronics");
        var clothing    = context.Categories.First(c => c.Name == "Clothing");
        var books       = context.Categories.First(c => c.Name == "Books");

var products = new List<Product>
{
    new Product
    {
        Name        = "Laptop Pro",
        Description = "A powerful laptop for professionals.",
        Price       = 1200.00m,
        Stock       = 10,
        ImageUrl    = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400&h=300&fit=crop",
        CategoryId  = electronics.Id
    },
    new Product
    {
        Name        = "Wireless Headphones",
        Description = "Noise cancelling wireless headphones.",
        Price       = 250.00m,
        Stock       = 25,
        ImageUrl    = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=300&fit=crop",
        CategoryId  = electronics.Id
    },
    new Product
    {
        Name        = "Smart Watch",
        Description = "Track your fitness and stay connected.",
        Price       = 199.00m,
        Stock       = 15,
        ImageUrl    = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=300&fit=crop",
        CategoryId  = electronics.Id
    },
    new Product
    {
        Name        = "Classic T-Shirt",
        Description = "Comfortable cotton t-shirt.",
        Price       = 25.00m,
        Stock       = 100,
        ImageUrl    = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400&h=300&fit=crop",
        CategoryId  = clothing.Id
    },
    new Product
    {
        Name        = "Denim Jacket",
        Description = "Stylish denim jacket for all seasons.",
        Price       = 89.00m,
        Stock       = 40,
        ImageUrl    = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=400&h=300&fit=crop",
        CategoryId  = clothing.Id
    },
    new Product
    {
        Name        = "C# Programming Guide",
        Description = "Complete guide to C# programming.",
        Price       = 45.00m,
        Stock       = 60,
        ImageUrl    = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400&h=300&fit=crop",
        CategoryId  = books.Id
    },
    new Product
    {
        Name        = "ASP.NET Core in Action",
        Description = "Build modern web apps with ASP.NET Core.",
        Price       = 55.00m,
        Stock       = 35,
        ImageUrl    = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400&h=300&fit=crop",
        CategoryId  = books.Id
    }
};
        context.Products.AddRange(products);
        context.SaveChanges();
    }

    // Seed Admin
    if (!context.Admins.Any())
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("admin123"));
        var hash  = Convert.ToBase64String(bytes);

        context.Admins.Add(new OnlineStore.Models.Admin
        {
            Username     = "admin",
            PasswordHash = hash
        });
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();