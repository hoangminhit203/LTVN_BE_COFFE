using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Authorization;
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

        // Bắt đầu giao dịch (Transaction)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Xử lý địa chỉ: Đảm bảo finalAddress LUÔN LÀ string (địa chỉ đầy đủ)
            string finalAddressString;

            if (model.ShippingAddressId.HasValue)
            {
                //Dùng địa chỉ đã có (từ ID)
                var addr = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.Id == model.ShippingAddressId.Value && a.UserId == userId);

                if (addr == null)
                    throw new Exception("Invalid shipping address");

                finalAddressString = addr.FullAddress;
            }
            else if (!string.IsNullOrEmpty(model.ShippingAddress))
            {
                // Dùng địa chỉ mới nhập (từ string)
                // Có thể thêm code tạo mới địa chỉ nếu muốn lưu vào DB,
                // nhưng ở đây ta chỉ cần lấy chuỗi địa chỉ

                var newAddr = new ShippingAddress // Vẫn giữ logic tạo mới và lưu vào DB
                {
                    UserId = userId,
                    FullAddress = model.ShippingAddress,
                    IsDefault = false
                };
                _context.ShippingAddresses.Add(newAddr);

                // LƯU Ý: Không cần SaveChangesAsync ở đây vì ta sẽ lưu tất cả một lần cuối.
                // Tuy nhiên, nếu bạn cần ID của newAddr cho mục đích khác, hãy gọi nó.
                // Tạm thời, ta chỉ lấy chuỗi địa chỉ

                finalAddressString = newAddr.FullAddress;
            }
            else
            {
                // Case 3: Dùng địa chỉ mặc định của user
                var user = await _context.Users.Include(u => u.ShippingAddresses) // INCLUDE nếu ShippingAddresses là Navigation Property
                                               .AsNoTracking() // Tăng hiệu suất khi chỉ đọc
                                               .FirstOrDefaultAsync(u => u.Id == userId);

                var defaultAddress = user?.ShippingAddresses?.FirstOrDefault();

                if (defaultAddress == null)
                {
                    throw new Exception("No shipping address available");
                }

                finalAddressString = defaultAddress.FullAddress; // Lấy thuộc tính chuỗi (string)
            }

            // Tạo order
            var order = new Order
            {
                UserId = userId,
                Status = "pending",
                ShippingAddress = new ShippingAddress
                {
                    FullAddress = finalAddressString,
                    UserId = userId
                },
                TotalAmount = cart.TotalPrice,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // lưu để lấy Order.Id

            // Tạo order items và giảm tồn kho
            // Tránh N+1 Query
            var productIds = cart.Items.Select(item => item.ProductId).ToList();
            var productsDictionary = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var orderItems = new List<OrderItem>();
            decimal calculatedTotal = 0;

            foreach (var cartItem in cart.Items)
            {
                if (!productsDictionary.TryGetValue(cartItem.ProductId, out var product))
                    throw new Exception($"Product {cartItem.ProductId} not found");

                if (product.Stock < cartItem.Quantity)
                    throw new Exception($"Not enough inventory for {product.Name}");

                // Cập nhật tồn kho (Stock)
                product.Stock -= cartItem.Quantity;
                _context.Products.Update(product); // Đánh dấu là đã thay đổi

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Subtotal // Giả sử Subtotal đã có giá đúng
                };

                orderItems.Add(orderItem);
                calculatedTotal += orderItem.PriceAtPurchase;
            }

            _context.OrderItems.AddRange(orderItems);

            // Cập nhật tổng tiền order
            order.TotalAmount = calculatedTotal;
            _context.Orders.Update(order);

            // Xóa cart
            await _cartService.ClearCartAsync(cart.CartId, userId);

            // LƯU TẤT CẢ thay đổi còn lại VÀO DATABASE
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(order);
        }
        catch (Exception ex)
        {
            // Rollback khi có lỗi
            await transaction.RollbackAsync();
            return new BadRequestObjectResult(ex.Message);
        }
    }


    public Task<ActionResult<bool>> DeleteOrder(int orderId, string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<ActionResult<OrderResponse>?> GetOrder(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
            return new NotFoundObjectResult("Order not found");

        return MapToResponse(order);
    }

    public async Task<ActionResult<PaginationModel<OrderResponse>>> GetOrdersByUser(string userId, OrderFilterVModel filter)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(o => o.Status == filter.Status);

        var total = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new OkObjectResult(new PaginationModel<OrderResponse>
        {
            TotalRecords = total,
            Records = orders.Select(MapToResponse).ToList()
        });
    }

    [Authorize(Roles = "Admin")] // Bắt buộc người dùng phải có vai trò "Admin"
    public async Task<ActionResult<bool>> UpdateOrderStatus(int orderId, string status)
    {
        // 1. Kiểm tra tính hợp lệ của trạng thái mới (Optional nhưng nên có)
        if (string.IsNullOrEmpty(status))
        {
            return new BadRequestObjectResult("Status cannot be empty.");
        }

        // 2. Tìm đơn hàng trong DB
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return new NotFoundResult(); // Trả về 404 nếu không tìm thấy
        }

        // 3. Kiểm tra trạng thái mới có hợp lệ trong hệ thống không
        // (Ví dụ: "pending", "processing", "shipped", "delivered", "cancelled")
        // Bạn nên có một hàm hoặc Enum để kiểm tra việc này.

        // 4. Cập nhật trạng thái
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian thay đổi

        // 5. Lưu thay đổi vào DB
        try
        {
            await _context.SaveChangesAsync();
            return new OkObjectResult(true);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và trả về lỗi máy chủ
            // _logger.LogError(ex, "Error updating order status for ID: {OrderId}", orderId);
            return new StatusCodeResult(500);
        }
    }
    public static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress.FullAddress,
            ShippingMethod = order.ShippingMethod,
            Status = order.Status,
            PromotionId = order.PromotionId,
            VoucherCode = order.VoucherCode,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase,
                ProductName = oi.Product?.Name
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }
}
