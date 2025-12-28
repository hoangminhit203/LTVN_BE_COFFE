
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
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ICartService cartService, ILogger<OrderService> logger)
    {
        _context = context;
        _cartService = cartService;
        _logger = logger;   
    }

    // =========================================================================
    // 1. TẠO ĐƠN HÀNG (CORE LOGIC)
    // =========================================================================
    public async Task<ActionResult<ResponseResult>> CreateOrder(string userId, OrderCreateVModel model)
    {
        // 1. Kiểm tra giỏ hàng
        var cart = await _cartService.GetCartByUserAsync(userId);
        if (cart == null || !cart.Items.Any())
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Giỏ hàng trống." });

        // Bắt đầu Transaction (Quan trọng: Mọi thay đổi DB phải thành công cùng lúc)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 2. Xử lý Địa chỉ & Tính phí Ship
            int finalShippingAddressId = 0;
            string finalAddressString = string.Empty;

            // Trường hợp A: User chọn địa chỉ có sẵn
            if (model.ShippingAddressId.HasValue)
            {
                var existingAddr = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.Id == model.ShippingAddressId.Value && a.UserId == userId);

                if (existingAddr == null)
                    throw new Exception("Địa chỉ giao hàng không hợp lệ.");

                finalShippingAddressId = existingAddr.Id;
                finalAddressString = existingAddr.FullAddress;
            }
            // Trường hợp B: User nhập địa chỉ mới (string) -> Phải tạo mới trong DB để lấy ID
            else if (!string.IsNullOrEmpty(model.ShippingAddress))
            {
                var newAddr = new ShippingAddress
                {
                    UserId = userId,
                    FullAddress = model.ShippingAddress,
                    IsDefault = false, // Địa chỉ nhập nhanh thường không set default
                    ReceiverName = "N/A", // Cần bổ sung logic lấy tên người nhận nếu có
                    Phone = "N/A"
                };
                _context.ShippingAddresses.Add(newAddr);
                await _context.SaveChangesAsync(); // Lưu để lấy ID

                finalShippingAddressId = newAddr.Id;
                finalAddressString = newAddr.FullAddress;
            }
            // Trường hợp C: Lấy mặc định
            else
            {
                var defaultAddr = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDefault);

                if (defaultAddr == null)
                    throw new Exception("Vui lòng chọn hoặc nhập địa chỉ giao hàng.");

                finalShippingAddressId = defaultAddr.Id;
                finalAddressString = defaultAddr.FullAddress;
            }

            // Tính phí ship (Hàm riêng bên dưới)
            decimal shippingFee = CalculateShippingFee(finalAddressString);

            // 3. Chuẩn bị dữ liệu Order Items & Kiểm tra tồn kho
            // Lấy danh sách ProductVariant ID từ giỏ hàng
            var variantIds = cart.Items.Select(ci => ci.ProductVariantId).ToList();

            // Load Variants từ DB để check giá và tồn kho thực tế (Không tin tưởng dữ liệu từ FE gửi lên)
            var variants = await _context.ProductVariant
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            var orderItems = new List<OrderItem>();
            decimal subTotalAmount = 0;

            foreach (var ci in cart.Items)
            {
                if (!variants.TryGetValue(ci.ProductVariantId, out var variant))
                    throw new Exception($"Sản phẩm variant {ci.ProductVariantId} không tồn tại.");

                // Check tồn kho
                if (variant.Stock < ci.Quantity)
                    throw new Exception($"Sản phẩm '{variant.Product.Name}' không đủ hàng (Còn: {variant.Stock}).");

                // Trừ tồn kho
                variant.Stock -= ci.Quantity;
                _context.ProductVariant.Update(variant);

                // Tạo OrderItem
                var item = new OrderItem
                {
                    ProductVariantId = variant.Id,
                    ProductNameAtPurchase = variant.Product.Name,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = variant.Price // Lấy giá từ DB
                };
                // Subtotal của Item tự tính trong getter của OrderItem (hoặc tính tay ở đây nếu muốn)
                // Giả sử OrderItem có property Subtotal = Price * Quantity
                subTotalAmount += (item.PriceAtPurchase * item.Quantity);

                orderItems.Add(item);
            }

            // 4. Tính toán Voucher/Giảm giá (Nếu có)
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(model.VoucherCode))
            {
                // Gọi hàm check voucher ở đây (Logic này tùy bạn triển khai)
                // discountAmount = CheckVoucher(model.VoucherCode, subTotalAmount);
                discountAmount = 0; // Tạm thời để 0
            }

            // 5. Tạo Order Entity
            var order = new Order
            {
                UserId = userId,
                Status = "pending",
                ShippingMethod = model.ShippingMethod ?? "Standard",
                ShippingAddressId = finalShippingAddressId, // Quan trọng: Link tới bảng địa chỉ

                // Tiền nong
                TotalAmount = subTotalAmount, // Tổng tiền hàng
                ShippingFee = shippingFee,    // Phí ship
                DiscountAmount = discountAmount, // Giảm giá

                VoucherCode = model.VoucherCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderDate = DateTime.Now
            };

            // Entity Framework thông minh sẽ tự gán OrderId cho các OrderItem bên dưới khi Add order
            order.OrderItems = orderItems;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 6. Xóa giỏ hàng sau khi mua thành công
            await _cartService.ClearCartAsync(cart.CartId, userId);

            // Commit Transaction
            await transaction.CommitAsync();

            // Return kết quả
            return new OkObjectResult(new ResponseResult
            {
                IsSuccess = true,
                Message = "Đặt hàng thành công",
                Data = MapToResponse(order)
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Hoàn tác nếu lỗi
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }

    // =========================================================================
    // 2. LẤY CHI TIẾT ĐƠN HÀNG
    // =========================================================================
    public async Task<ActionResult<ResponseResult>> GetOrder(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress) // Include địa chỉ
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
            return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" });

        return new OkObjectResult(new ResponseResult
        {
            IsSuccess = true,
            Data = MapToResponse(order)
        });
    }

    // =========================================================================
    // 3. LẤY DANH SÁCH ĐƠN HÀNG CỦA USER
    // =========================================================================
    public async Task<ActionResult<ResponseResult>> GetOrdersByUser(string userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.ShippingAddress)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
            .OrderByDescending(o => o.CreatedAt) // Mới nhất lên đầu
            .ToListAsync();

        var responseData = orders.Select(MapToResponse).ToList();

        return new OkObjectResult(new ResponseResult
        {
            IsSuccess = true,
            Data = responseData
        });
    }

    // =========================================================================
    // 4. CẬP NHẬT TRẠNG THÁI (ADMIN/SHIPPER)
    // =========================================================================
    public async Task<ActionResult<ResponseResult>> UpdateOrderStatus(int orderId, string status)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null)
            return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" });

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Cập nhật trạng thái thành công" });
    }

    // =========================================================================
    // 5. HỦY ĐƠN HÀNG
    // =========================================================================
    public async Task<ActionResult<ResponseResult>> CancelOrder(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant) // Include để trả lại tồn kho
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
            return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" });

        // Chỉ cho hủy khi đơn hàng chưa hoàn thành hoặc đang giao
        if (order.Status == "completed" || order.Status == "shipping" || order.Status == "cancelled")
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Không thể hủy đơn hàng ở trạng thái này." });

        // Logic hoàn trả tồn kho (nếu cần thiết)
        foreach (var item in order.OrderItems)
        {
            var variant = item.ProductVariant;
            if (variant != null)
            {
                variant.Stock += item.Quantity; // Cộng lại kho
            }
        }

        order.Status = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Đã hủy đơn hàng thành công." });
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    // Hàm map Entity sang Response Model
    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,     // Tiền hàng
            ShippingFee = order.ShippingFee,     // Phí ship
            DiscountAmount = order.DiscountAmount, // Giảm giá
            FinalAmount = order.FinalAmount,     // Tổng thanh toán (đã tính trong Entity)

            ShippingAddress = order.ShippingAddress?.FullAddress ?? "N/A",
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
                Subtotal = oi.PriceAtPurchase * oi.Quantity
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }

    // Hàm tính phí ship đơn giản
    private decimal CalculateShippingFee(string address)
    {
        if (string.IsNullOrEmpty(address)) return 0;

        string addressLower = address.ToLower();
        if (addressLower.Contains("hồ chí minh") || addressLower.Contains("tphcm"))
            return 15000;
        if (addressLower.Contains("hà nội"))
            return 25000;

        return 35000; // Các tỉnh còn lại
    }
}