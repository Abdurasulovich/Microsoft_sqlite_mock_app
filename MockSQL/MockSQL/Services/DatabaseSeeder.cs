using MockSQL.Data;
using Microsoft.EntityFrameworkCore;
using MockSQL.Entities;

namespace MockSQL.Services;

public class DatabaseSeeder
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DatabaseSeeder(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// DB ni yaratadi va agar bo'sh bo'lsa mock data bilan to'ldiradi.
    /// MauiProgram.cs da chaqiriladi.
    /// </summary>
    public async Task SeedAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        // Migrate yoki yaratish
        await db.Database.EnsureCreatedAsync();

        // Allaqachon data bor bo'lsa seed qilmaymiz
        if (await db.Categories.AnyAsync()) return;

        // ── Categories ──────────────────────────────────────────────────────
        var categories = new List<Category>
        {
            new() { Name = "Elektronika",    Description = "Telefonlar, noutbuklar, gadgetlar" },
            new() { Name = "Kiyim-kechak",   Description = "Erkaklar va ayollar kiyimi" },
            new() { Name = "Oziq-ovqat",     Description = "Mahalliy va import mahsulotlar" },
            new() { Name = "Sport anjomlari",Description = "Fitnes, outdoor, sport" },
            new() { Name = "Uy-ro'zg'or",   Description = "Mebel, dekor, oshxona" },
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // ── Products ─────────────────────────────────────────────────────────
        var products = new List<Product>
        {
            // Elektronika
            new() { Name = "Samsung Galaxy S24",     Price = 12_500_000, Stock = 15, CategoryId = categories[0].Id, Description = "256GB, 8GB RAM, 6.2 dyuym" },
            new() { Name = "Apple iPhone 15",        Price = 15_000_000, Stock = 10, CategoryId = categories[0].Id, Description = "128GB, A16 Bionic chip" },
            new() { Name = "Lenovo IdeaPad 5",       Price = 9_800_000,  Stock = 8,  CategoryId = categories[0].Id, Description = "Ryzen 5, 16GB RAM, 512GB SSD" },
            new() { Name = "AirPods Pro 2",          Price = 3_200_000,  Stock = 25, CategoryId = categories[0].Id, Description = "Faol shovqin o'chirish" },
            new() { Name = "Xiaomi Redmi Note 13",   Price = 4_500_000,  Stock = 30, CategoryId = categories[0].Id, Description = "128GB, 6GB RAM, 200MP kamera" },

            // Kiyim
            new() { Name = "Nike Air Force 1",       Price = 1_200_000,  Stock = 50, CategoryId = categories[1].Id, Description = "Oq rang, 40-45 razmer" },
            new() { Name = "Adidas Hoodie",          Price = 850_000,    Stock = 40, CategoryId = categories[1].Id, Description = "Qora, M/L/XL" },
            new() { Name = "Levi's 501 Jeans",       Price = 980_000,    Stock = 35, CategoryId = categories[1].Id, Description = "Classic straight fit" },

            // Oziq-ovqat
            new() { Name = "Zaitun moyi 1L",         Price = 85_000,     Stock = 100, CategoryId = categories[2].Id, Description = "Extra virgin, Ispaniya" },
            new() { Name = "Uy qovoq murabbo",       Price = 35_000,     Stock = 60,  CategoryId = categories[2].Id, Description = "Tabiiy, 0.5 kg" },
            new() { Name = "Organik asal 500g",      Price = 120_000,    Stock = 45,  CategoryId = categories[2].Id, Description = "O'zbekiston, tog' asali" },

            // Sport
            new() { Name = "Yoga mat",               Price = 250_000,    Stock = 20, CategoryId = categories[3].Id, Description = "6mm, slip-proof" },
            new() { Name = "Dumbbell 10kg (juft)",   Price = 480_000,    Stock = 15, CategoryId = categories[3].Id, Description = "Rezina qoplamali" },
            new() { Name = "Futbol to'p Adidas",     Price = 320_000,    Stock = 25, CategoryId = categories[3].Id, Description = "5-razmer, FIFA sertifikat" },

            // Uy-ro'zg'or
            new() { Name = "IKEA KALLAX tokcha",     Price = 1_100_000,  Stock = 12, CategoryId = categories[4].Id, Description = "4 katak, oq" },
            new() { Name = "Oshxona blender",        Price = 650_000,    Stock = 18, CategoryId = categories[4].Id, Description = "800W, 1.5L, 3 tezlik" },
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        // ── Orders ────────────────────────────────────────────────────────────
        var orders = new List<Order>
        {
            new()
            {
                CustomerName  = "Alisher Toshmatov",
                CustomerEmail = "alisher@example.com",
                Status        = OrderStatus.Delivered,
                OrderDate     = DateTime.UtcNow.AddDays(-10),
                Items = new List<OrderItem>
                {
                    new() { ProductId = products[0].Id, Quantity = 1, UnitPrice = products[0].Price },
                    new() { ProductId = products[3].Id, Quantity = 2, UnitPrice = products[3].Price },
                }
            },
            new()
            {
                CustomerName  = "Malika Yusupova",
                CustomerEmail = "malika@example.com",
                Status        = OrderStatus.Processing,
                OrderDate     = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem>
                {
                    new() { ProductId = products[5].Id, Quantity = 1, UnitPrice = products[5].Price },
                    new() { ProductId = products[6].Id, Quantity = 2, UnitPrice = products[6].Price },
                    new() { ProductId = products[7].Id, Quantity = 1, UnitPrice = products[7].Price },
                }
            },
            new()
            {
                CustomerName  = "Bobur Xasanov",
                CustomerEmail = "bobur@example.com",
                Status        = OrderStatus.Pending,
                OrderDate     = DateTime.UtcNow.AddHours(-3),
                Items = new List<OrderItem>
                {
                    new() { ProductId = products[11].Id, Quantity = 1, UnitPrice = products[11].Price },
                    new() { ProductId = products[12].Id, Quantity = 1, UnitPrice = products[12].Price },
                }
            },
            new()
            {
                CustomerName  = "Javlonbek Djalekeev",
                CustomerEmail = "djalekeev1@example.com",
                Status        = OrderStatus.Shipped,
                OrderDate     = DateTime.UtcNow.AddDays(-5),
                Items = new List<OrderItem>
                {
                    new() { ProductId = products[14].Id, Quantity = 1, UnitPrice = products[14].Price },
                    new() { ProductId = products[15].Id, Quantity = 1, UnitPrice = products[15].Price },
                }
            },
        };

        // TotalAmount ni hisoblash
        foreach (var order in orders)
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }
}
