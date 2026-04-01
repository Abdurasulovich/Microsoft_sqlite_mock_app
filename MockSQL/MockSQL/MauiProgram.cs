using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using MockSQL.Data;
using MockSQL.Services;

namespace MockSQL;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();
        // ── SQLite + EF Core ─────────────────────────────────────────────────
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, LogLevel.Information);
#endif
        });
        // ── Services ──────────────────────────────────────────────────────────
        builder.Services.AddSingleton<DatabaseSeeder>();
        builder.Services.AddSingleton<IProductService, ProductService>();
        builder.Services.AddSingleton<ICategoryService, CategoryService>();
        builder.Services.AddSingleton<IOrderService, OrderService>();
        // ── Pages / ViewModels ────────────────────────────────────────────────
        // builder.Services.AddTransient<MainPage>();
        // builder.Services.AddTransient<MainPageViewModel>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();
        // DB seed — app start bo'lganda bir marta ishlaydi
        Task.Run(async () =>
        {
            var seeder = app.Services.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        });
        return app;
    }
}