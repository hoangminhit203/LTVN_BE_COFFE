using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CheckoutService
{
    private readonly AppDbContext _context;

    public CheckoutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CheckoutAsync(int userId, string shippingAddress, string? voucherCode = null)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            throw new Exception("Giỏ hàng trống!");

        // Tính tổng tiền
        var total = cart.CartItems.Sum(i => i.Quantity * i.Product.Price);

        // Áp dụng voucher (nếu có)
        Promotion? promo = null;
        if (!string.IsNullOrEmpty(voucherCode))
        {
            promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == voucherCode && p.IsActive);
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = shippingAddress,
            VoucherCode = promo?.Code,
            TotalAmount = total,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "pending",
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Chuyển từng item trong Cart → OrderItem
        foreach (var item in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.Product.Price
            };
            _context.OrderItems.Add(orderItem);
        }

        // Xóa giỏ hàng sau khi đặt hàng
        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();

        return order;
    }
}
