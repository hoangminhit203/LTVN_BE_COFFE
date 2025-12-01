using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(AppDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<ActionResult<OrderResponse>?> CreateOrder(string userId, OrderCreateVModel model)
    {
        var cart = await _cartService.GetCartByUserAsync(userId);
        if (cart == null || !cart.Items.Any())
            return new BadRequestObjectResult("Cart is empty");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Lấy địa chỉ shipping
            string finalAddressString = string.Empty;

            if (model.ShippingAddressId.HasValue)
            {
                var addr = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.Id == model.ShippingAddressId.Value && a.UserId == userId);
                if (addr == null) throw new Exception("Invalid shipping address");
                finalAddressString = addr.FullAddress;
            }
            else if (!string.IsNullOrEmpty(model.ShippingAddress))
            {
                var newAddr = new ShippingAddress
                {
                    UserId = userId,
                    FullAddress = model.ShippingAddress,
                    IsDefault = false
                };
                _context.ShippingAddresses.Add(newAddr);
                finalAddressString = newAddr.FullAddress;
            }
            else
            {
                var user = await _context.Users.Include(u => u.ShippingAddresses)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync(u => u.Id == userId);
                var defaultAddr = user?.ShippingAddresses?.FirstOrDefault();
                if (defaultAddr == null) throw new Exception("No shipping address available");
                finalAddressString = defaultAddr.FullAddress;
            }

            // Tạo order
            var order = new Order
            {
                UserId = userId,
                Status = "pending",
                ShippingMethod = model.ShippingMethod,
                ShippingAddress = new ShippingAddress
                {
                    FullAddress = finalAddressString,
                    UserId = userId
                },
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Lấy danh sách ProductVariant liên quan
            var variantIds = cart.Items.Select(ci => ci.ProductVariantId).ToList();
            var variants = await _context.ProductVariant
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var ci in cart.Items)
            {
                if (!variants.TryGetValue(ci.ProductVariantId, out var variant))
                    throw new Exception($"Product variant {ci.ProductVariantId} not found");

                if (variant.Stock < ci.Quantity)
                    throw new Exception($"Not enough stock for {variant.Product.Name}");

                variant.Stock -= ci.Quantity;
                _context.ProductVariant.Update(variant);

                var item = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductVariantId = variant.Id,
                    ProductNameAtPurchase = variant.Product.Name,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = variant.Price
                };
                orderItems.Add(item);

                totalAmount += item.Subtotal;
            }

            _context.OrderItems.AddRange(orderItems);

            order.TotalAmount = totalAmount;
            _context.Orders.Update(order);

            await _cartService.ClearCartAsync(cart.CartId, userId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new BadRequestObjectResult(ex.Message);
        }
    }

    public async Task<ActionResult<OrderResponse>?> GetOrder(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
            return new NotFoundObjectResult("Order not found");

        return MapToResponse(order);
    }

    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUser(string userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
            .ToListAsync();

        return orders.Select(MapToResponse).ToList();
    }

    public async Task<ActionResult<bool>> UpdateOrderStatus(int orderId, string status)
    {
        if (string.IsNullOrEmpty(status))
            return new BadRequestObjectResult("Status cannot be empty");

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null) return new NotFoundResult();

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return new StatusCodeResult(500);
        }
    }

    public async Task<ActionResult<bool>> CancelOrder(int orderId, string userId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        if (order == null) return new NotFoundResult();

        if (order.Status == "completed")
            return new BadRequestObjectResult("Cannot cancel a completed order");

        order.Status = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            FinalAmount = order.FinalAmount,
            ShippingAddress = order.ShippingAddress?.FullAddress ?? "",
            ShippingMethod = order.ShippingMethod,
            Status = order.Status,
            PromotionId = order.PromotionId,
            VoucherCode = order.VoucherCode,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ItemCount = order.ItemCount,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductVariantId = oi.ProductVariantId,
                ProductName = oi.ProductNameAtPurchase,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase,
                Subtotal = oi.Subtotal
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }

   
}
