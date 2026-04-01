using MockSQL.Data;
using MockSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MockSQL.Services;

public interface IOrderService : IRepository<Order>
{
    Task<List<Order>> GetByStatusAsync(OrderStatus status);
    Task<Order?> GetWithItemsAsync(int orderId);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus);
}

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public OrderService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        await using var db = await _factory.CreateDbContextAsync();
        order.OrderDate = DateTime.UtcNow;
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> UpdateAsync(Order order)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        if (existing is null) return null;

        existing.CustomerName = order.CustomerName;
        existing.CustomerEmail = order.CustomerEmail;
        existing.Status = order.Status;
        existing.TotalAmount = order.TotalAmount;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var order = await db.Orders.FindAsync(orderId);
        if (order is null) return false;

        order.Status = newStatus;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var order = await db.Orders.FindAsync(id);
        if (order is null) return false;

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return true;
    }
}
