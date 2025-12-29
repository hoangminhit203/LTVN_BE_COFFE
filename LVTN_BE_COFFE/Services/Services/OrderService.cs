
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(AppDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<ActionResult<ResponseResult>> CreateOrder(string? userId, string? guestKey, OrderCreateVModel model)
    {
        var cart = await _cartService.GetCartAsync(userId, guestKey);
        if (cart == null || !cart.Items.Any())
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Giỏ hàng trống." });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            string finalReceiverName = model.ReceiverName;
            string finalReceiverPhone = model.ReceiverPhone;
            string? finalReceiverEmail = model.ReceiverEmail;
            string addressSnapshot = "";
            int? shippingAddressId = model.ShippingAddressId;

            if (model.ShippingAddressId.HasValue)
            {
                var addr = await _context.ShippingAddresses.FindAsync(model.ShippingAddressId.Value);
                if (addr != null)
                {
                    addressSnapshot = addr.FullAddress;
                    // Nếu model gửi lên trống thì mới lấy từ địa chỉ mặc định
                    if (string.IsNullOrEmpty(finalReceiverName)) finalReceiverName = addr.ReceiverName;
                    if (string.IsNullOrEmpty(finalReceiverPhone)) finalReceiverPhone = addr.Phone;
                }
            }
            else
            {
                addressSnapshot = model.ShippingAddress ?? "";
            }

            if (string.IsNullOrEmpty(addressSnapshot))
                throw new Exception("Địa chỉ giao hàng không được để trống.");
            // ----------------------------------------------

            // (Giữ nguyên logic load variants và tính tiền...)
            var variantIds = cart.Items.Select(ci => ci.ProductVariantId).ToList();
            var variants = await _context.ProductVariant
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            var orderItems = new List<OrderItem>();
            decimal subTotalAmount = 0;

            foreach (var ci in cart.Items)
            {
                if (!variants.TryGetValue(ci.ProductVariantId, out var variant))
                    throw new Exception($"Sản phẩm {ci.ProductVariantId} không tồn tại.");

                if (variant.Stock < ci.Quantity)
                    throw new Exception($"Sản phẩm '{variant.Product.Name}' không đủ hàng.");

                variant.Stock -= ci.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductVariantId = variant.Id,
                    ProductNameAtPurchase = variant.Product.Name,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = variant.Price
                });
                subTotalAmount += (variant.Price * ci.Quantity);
            }

            var order = new Order
            {
                UserId = userId,
                GuestKey = guestKey,

                // Gán dữ liệu thực tế khách vừa nhập
                ReceiverName = finalReceiverName,
                ReceiverPhone = finalReceiverPhone,
                ReceiverEmail = finalReceiverEmail,

                ShippingAddressSnapshot = addressSnapshot,
                ShippingAddressId = shippingAddressId,
                Status = "pending",
                TotalAmount = subTotalAmount,
                ShippingFee = CalculateShippingFee(addressSnapshot),
                DiscountAmount = 0,
                VoucherCode = model.VoucherCode,
                ShippingMethod = model.ShippingMethod,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cartService.ClearCartAsync(cart.CartId);
            await transaction.CommitAsync();

            return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = MapToResponse(order) });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }

    public async Task<ActionResult<ResponseResult>> GetOrdersByIdentity(string? userId, string? guestKey)
    {
        var orders = await _context.Orders
            .Where(o => (userId != null && o.UserId == userId) || (guestKey != null && o.GuestKey == guestKey))
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = orders.Select(MapToResponse) });
    }

    public async Task<ActionResult<ResponseResult>> GetOrder(int orderId, string? userId, string? guestKey)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId &&
                ((userId != null && o.UserId == userId) || (guestKey != null && o.GuestKey == guestKey)));

        if (order == null) return new NotFoundResult();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = MapToResponse(order) });
    }

    public async Task<ActionResult<ResponseResult>> UpdateOrderStatus(int orderId, string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return new NotFoundResult();

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Updated" });
    }

    public async Task<ActionResult<ResponseResult>> CancelOrder(int orderId, string? userId, string? guestKey)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
            .FirstOrDefaultAsync(o => o.OrderId == orderId &&
                ((userId != null && o.UserId == userId) || (guestKey != null && o.GuestKey == guestKey)));

        if (order == null || order.Status != "pending")
            return new BadRequestObjectResult("Order cannot be cancelled.");

        foreach (var item in order.OrderItems)
        {
            if (item.ProductVariant != null) item.ProductVariant.Stock += item.Quantity;
        }

        order.Status = "cancelled";
        await _context.SaveChangesAsync();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Cancelled" });
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            ShippingFee = order.ShippingFee,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            ShippingAddress = order.ShippingAddressSnapshot,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            ItemCount = order.OrderItems.Count,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductName = oi.ProductNameAtPurchase,
                ProductVariantId=oi.ProductVariantId,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase,
                Subtotal = oi.PriceAtPurchase * oi.Quantity
            }).ToList()
        };
    }

    private decimal CalculateShippingFee(string address)
    {
        if (string.IsNullOrEmpty(address)) return 0;

        string addressLower = address.ToLower();
        if (addressLower.Contains("hồ chí minh") || addressLower.Contains("tphcm"))
            return 15000;
        if (addressLower.Contains("hà nội"))
            return 25000;

        return 35000;
    }

}