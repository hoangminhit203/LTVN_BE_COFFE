using Microsoft.EntityFrameworkCore;

public class OrderItemService : IOrderItemService
{
    private readonly AppDbContext _context;

    public OrderItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        return await _context.OrderItems
            .Include(o => o.Product)
            .Include(o => o.Order)
            .ToListAsync();
    }

    public async Task<OrderItem?> GetByIdAsync(int id)
    {
        return await _context.OrderItems
            .Include(o => o.Product)
            .Include(o => o.Order)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrderItem> CreateAsync(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    public async Task<OrderItem?> UpdateAsync(int id, OrderItem orderItem)
    {
        var existing = await _context.OrderItems.FindAsync(id);
        if (existing == null) return null;

        existing.ProductId = orderItem.ProductId;
        existing.Quantity = orderItem.Quantity;
        existing.PriceAtPurchase = orderItem.PriceAtPurchase;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item == null) return false;

        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
