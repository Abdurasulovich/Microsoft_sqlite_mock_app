using MockSQL.Data;
using MockSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MockSQL.Services;

public interface IProductService : IRepository<Product>
{
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<List<Product>> SearchAsync(string query);
    Task<bool> UpdateStockAsync(int productId, int newStock);
}

public class ProductService : IProductService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProductService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string query)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var lower = query.ToLower();
        return await db.Products
            .Where(p => p.Name.ToLower().Contains(lower) ||
                        (p.Description != null && p.Description.ToLower().Contains(lower)))
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await using var db = await _factory.CreateDbContextAsync();
        product.CreatedAt = DateTime.UtcNow;
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Products.FindAsync(product.Id);
        if (existing is null) return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> UpdateStockAsync(int productId, int newStock)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var product = await db.Products.FindAsync(productId);
        if (product is null) return false;

        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }
}
