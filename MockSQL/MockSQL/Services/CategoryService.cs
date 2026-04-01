using Microsoft.EntityFrameworkCore;
using MockSQL.Data;
using MockSQL.Entities;

namespace MockSQL.Services;

public interface ICategoryService : IRepository<Category>
{
    Task<List<Category>> GetAllWithProductsAsync();
}

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CategoryService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetAllWithProductsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        await using var db = await _factory.CreateDbContextAsync();
        category.CreatedAt = DateTime.UtcNow;
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Categories.FindAsync(category.Id);
        if (existing is null) return null;

        existing.Name = category.Name;
        existing.Description = category.Description;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var category = await db.Categories.FindAsync(id);
        if (category is null) return false;

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }
}
