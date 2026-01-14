using DocumentFormat.OpenXml.Spreadsheet;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly CloudinaryService _cloudinaryService;

    public OrderService(
        AppDbContext context,
        ICartService cartService,
        IEmailSenderService emailSender,
        IConfiguration configuration,
        IWebHostEnvironment env,
        CloudinaryService cloudinaryService) 
    {
        _context = context;
        _cartService = cartService;
        _emailSender = emailSender;
        _configuration = configuration;
        _env = env;
        _cloudinaryService = cloudinaryService;
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

            // Nếu user đã đăng nhập và chưa có email từ form, lấy email từ database
            if (!string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(finalReceiverEmail))
            {
                var user = await _context.AspNetUsers.FindAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    finalReceiverEmail = user.Email;
                }
            }

            if (model.ShippingAddressId.HasValue)
            {
                var addr = await _context.ShippingAddresses.FindAsync(model.ShippingAddressId.Value);
                if (addr != null)
                {
                    addressSnapshot = addr.FullAddress;
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

            var variantIds = cart.Items.Select(ci => ci.ProductVariantId).ToList();
            var variants = await _context.ProductVariant
                .Include(v => v.Product)
                .Include(v => v.Images)
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
                    ProductVariant = variant,
                    ProductNameAtPurchase = variant.Product.Name,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = variant.Price
                });
                subTotalAmount += (variant.Price * ci.Quantity);
            }
            Promotion? promotion = null;
            decimal discountAmount = 0;

            if (!string.IsNullOrWhiteSpace(model.PromotionCode))
            {
                promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p =>
                        p.Code == model.PromotionCode &&
                        p.IsEnabled &&
                        (!p.StartDate.HasValue || p.StartDate <= DateTime.Now) &&
                        (!p.EndDate.HasValue || p.EndDate >= DateTime.Now));

                if (promotion == null)
                    throw new Exception("Mã khuyến mãi không hợp lệ hoặc đã hết hạn.");

                if (promotion.MinOrderValue.HasValue &&
                    subTotalAmount < promotion.MinOrderValue.Value)
                    throw new Exception("Đơn hàng chưa đạt giá trị tối thiểu.");

                if (promotion.UsageLimit.HasValue &&
                    promotion.UsageCount >= promotion.UsageLimit.Value)
                    throw new Exception("Mã khuyến mãi đã hết lượt sử dụng.");

                discountAmount = promotion.DiscountType switch
                {
                    PromotionType.Percentage => subTotalAmount * promotion.DiscountValue / 100,
                    PromotionType.Fixed => promotion.DiscountValue,
                    _ => 0
                };

                if (promotion.MaxDiscountAmount.HasValue)
                    discountAmount = Math.Min(discountAmount, promotion.MaxDiscountAmount.Value);

                discountAmount = Math.Min(discountAmount, subTotalAmount);

                promotion.UsageCount++;
            }


            var order = new Order
            {
                // OrderId sẽ tự động được tạo trong constructor của Order
                UserId = userId,
                GuestKey = guestKey,
                ReceiverName = finalReceiverName,
                ReceiverPhone = finalReceiverPhone,
                ReceiverEmail = finalReceiverEmail,
                ShippingAddressSnapshot = addressSnapshot,
                ShippingAddressId = shippingAddressId,
                Status = "pending",
                ShippingFee = CalculateShippingFee(addressSnapshot),
                PromotionId = promotion?.Id,
                TotalAmount = subTotalAmount,
                DiscountAmount = discountAmount,
                ShippingMethod = model.ShippingMethod,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow,
                OrderDate = DateTime.Now
            };


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cartService.ClearCartAsync(cart.CartId);
            await transaction.CommitAsync();

            // GỬI EMAIL XÁC NHẬN ĐƠN HÀNG (cho cả user đăng nhập và guest)
            if (!string.IsNullOrEmpty(finalReceiverEmail))
            {
                try
                {
                    await SendOrderConfirmationEmail(order, finalReceiverEmail);
                    Console.WriteLine($"[INFO] Order confirmation email sent to: {finalReceiverEmail}");
                }
                catch (Exception emailEx)
                {
                    // Log lỗi nhưng không fail toàn bộ đơn hàng
                    Console.WriteLine($"[WARNING] Failed to send order confirmation email: {emailEx.Message}");
                }
            }
            else
            {
                Console.WriteLine("[INFO] No email address provided, skipping order confirmation email.");
            }

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

        var query = _context.Orders.AsQueryable();

        // Sửa logic: Nếu có cả 2, lấy đơn hàng của cả 2
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(guestKey))
        {
            query = query.Where(o => o.UserId == userId || o.GuestKey == guestKey);
        }
        // Chỉ có userId
        else if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }
        // Chỉ có guestKey
        else if (!string.IsNullOrEmpty(guestKey))
        {
            query = query.Where(o => o.GuestKey == guestKey);
        }
        else
        {
            // Không có định danh, trả về rỗng
            // Console.WriteLine("[DEBUG] Không có userId hoặc guestKey");
            return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = new List<OrderResponse>() });
        }

        // Log SQL query (nếu cần)
        var sqlQuery = query.ToQueryString();
        //  Console.WriteLine($"[DEBUG] SQL Query: {sqlQuery}");

        var orders = await query
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Images)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Console.WriteLine($"[DEBUG] Tìm thấy {orders.Count} đơn hàng trong database");

        var result = orders.Select(MapToResponse).ToList();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = result });
    }

    public async Task<ActionResult<ResponseResult>> GetOrder(string orderId)
    {
        var order = await _context.Orders
           .Include(o => o.OrderItems)
               .ThenInclude(oi => oi.ProductVariant)
                   .ThenInclude(pv => pv.Images)
           .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return new NotFoundResult();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = MapToResponse(order) });
    }

    public async Task<ActionResult<ResponseResult>> GetOrderAdmin(string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Images)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return new NotFoundResult();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = MapToResponse(order) });
    }

    public async Task<ActionResult<ResponseResult>> UpdateOrderStatus(string orderId, string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return new NotFoundResult();
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Updated Status Success!" });
    }

    public async Task<ActionResult<ResponseResult>> CancelOrder(string orderId, string? userId, string? guestKey)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
            .FirstOrDefaultAsync(o => o.OrderId == orderId &&
                ((userId != null && o.UserId == userId) || (guestKey != null && o.GuestKey == guestKey)));

        if (order == null || order.Status != "pending")
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Order cannot be cancelled." });

        foreach (var item in order.OrderItems)
        {
            if (item.ProductVariant != null) item.ProductVariant.Stock += item.Quantity;
        }

        order.Status = "cancelled";
        await _context.SaveChangesAsync();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Cancelled" });
    }

    public async Task<ActionResult<ResponseResult>> GetAllOrder()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Images)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = orders.Select(MapToResponse) });
    }

    private async Task SendOrderConfirmationEmail(Order order, string recipientEmail)
    {
        var subject = $"Xác nhận đơn hàng #{order.OrderId}";

        // Tạo nội dung chi tiết sản phẩm với hình ảnh
        var orderItemsHtml = string.Join("", order.OrderItems.Select(item =>
        {
            var imageUrl = item.ProductVariant?.Images?.FirstOrDefault()?.ImageUrl ?? "";
            var displayImage = !string.IsNullOrEmpty(imageUrl)
                ? $"<img src='{imageUrl}' alt='{item.ProductNameAtPurchase}' style='width: 60px; height: 60px; object-fit: cover; border-radius: 5px;' />"
                : "<div style='width: 60px; height: 60px; background-color: #f0f0f0; display: flex; align-items: center; justify-content: center; border-radius: 5px; font-size: 10px; color: #999;'>No Image</div>";

            return $@"<tr>
                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>
                    <div style='display: flex; align-items: center; gap: 10px;'>
                        {displayImage}
                        <span>{item.ProductNameAtPurchase}</span>
                    </div>
                </td>
                <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:N0}đ</td>
                <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{(item.PriceAtPurchase * item.Quantity):N0}đ</td>
            </tr>";
        }));

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6F4E37; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .order-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .order-info h3 {{ margin-top: 0; color: #6F4E37; border-bottom: 2px solid #6F4E37; padding-bottom: 10px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 15px 0; background-color: white; }}
        th {{ background-color: #6F4E37; color: white; padding: 12px; text-align: left; }}
        .total {{ background-color: #6F4E37; color: white; padding: 15px; text-align: right; font-size: 18px; font-weight: bold; border-radius: 5px; margin-top: 15px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>☕ Cảm ơn bạn đã đặt hàng!</h1>
        </div>
        
        <div class='content'>
            <div class='order-info'>
                <h3>📋 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> {order.OrderId}</p>
                <p><strong>Ngày đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Trạng thái:</strong> <span style='color: #ff9800; font-weight: bold;'>Đang chờ xử lý</span></p>
            </div>

            <div class='order-info'>
                <h3>👤 Thông tin người nhận</h3>
                <p><strong>Tên người nhận:</strong> {order.ReceiverName}</p>
                <p><strong>Số điện thoại:</strong> {order.ReceiverPhone}</p>
                <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddressSnapshot}</p>
                {(!string.IsNullOrEmpty(order.ShippingMethod) ? $"<p><strong>Phương thức vận chuyển:</strong> {order.ShippingMethod}</p>" : "")}
            </div>

            <div class='order-info'>
                <h3>🛒 Chi tiết sản phẩm</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Sản phẩm</th>
                            <th style='text-align: center; width: 80px;'>Số lượng</th>
                            <th style='text-align: right; width: 100px;'>Đơn giá</th>
                            <th style='text-align: right; width: 100px;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {orderItemsHtml}
                        <tr>
                            <td colspan='3' style='padding: 10px; text-align: right; font-weight: bold;'>Tạm tính:</td>
                            <td style='padding: 10px; text-align: right; font-weight: bold;'>{order.TotalAmount:N0}đ</td>
                        </tr>
                        <tr>
                            <td colspan='3' style='padding: 10px; text-align: right; font-weight: bold;'>Phí vận chuyển:</td>
                            <td style='padding: 10px; text-align: right; font-weight: bold;'>{order.ShippingFee:N0}đ</td>
                        </tr>
                        {(order.DiscountAmount > 0 ? $@"
                        <tr>
                            <td colspan='3' style='padding: 10px; text-align: right; font-weight: bold;'>Giảm giá:</td>
                            <td style='padding: 10px; text-align: right; color: #dc3545; font-weight: bold;'>-{order.DiscountAmount:N0}đ</td>
                        </tr>" : "")}
                    </tbody>
                </table>
            </div>

            <div class='total'>
                TỔNG CỘNG: {order.FinalAmount:N0}đ
            </div>

            <div class='order-info'>
                <p style='margin: 10px 0;'>📦 Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ liên hệ với bạn sớm nhất!</p>
                <p style='margin: 10px 0;'>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ: <strong style='color: #6F4E37;'>{order.ReceiverPhone}</strong></p>
            </div>
        </div>

        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Coffee Manager. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

        // Lấy thông tin email từ configuration
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "tranhoangngoc112@gmail.com";
        var fromPassword = _configuration["EmailSettings:FromPassword"] ?? "mffilftdavfmvvyg";

        await _emailSender.SendMailAsync(fromEmail, fromPassword, recipientEmail, subject, body);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            ShippingFee = order.ShippingFee,
            ReceiverEmail = order.ReceiverEmail,
            ReceiverName = order.ReceiverName,
            ReceiverPhone = order.ReceiverPhone,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            ShippingAddress = order.ShippingAddressSnapshot,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            ItemCount = order.OrderItems.Count,
            ShippingMethod = order.ShippingMethod,
            PromotionCode = order.Promotion?.Code,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductName = oi.ProductNameAtPurchase,
                ProductVariantId = oi.ProductVariantId,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase,
                Subtotal = oi.PriceAtPurchase * oi.Quantity,
                ImageUrl = oi.ProductVariant?.Images.FirstOrDefault()?.ImageUrl
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
    // Thêm method này vào OrderService.cs

    // Thêm tham số guestKey vào hàm
    public async Task<ActionResult<ResponseResult>> RequestReturnOrder(string orderId, string? userId, string? guestKey, ReturnOrderInputModel input)
    {
        try
        {
            // 1. SỬA LOGIC CHECK QUYỀN SỞ HỮU
            // Kiểm tra đơn hàng tồn tại VÀ (thuộc về UserId HOẶC thuộc về GuestKey)
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId &&
                    ((userId != null && o.UserId == userId) || (guestKey != null && o.GuestKey == guestKey)));

            if (order == null)
                // Thông báo chung chung để bảo mật, hoặc cụ thể là "Không tìm thấy đơn hàng hoặc bạn không có quyền."
                return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng hợp lệ." });

            // 2. Kiểm tra xem đơn hàng này đã từng yêu cầu trả hàng chưa
            var existingRequest = await _context.OrderReturns
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (existingRequest != null)
                return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Đơn hàng này đang được xử lý khiếu nại." });

            // 3. Xử lý Upload ảnh lên Cloudinary
            var uploadedImageUrls = new List<string>();

            if (input.Images != null && input.Images.Any())
            {
                if (input.Images.Count > 5)
                    return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = "Chỉ được upload tối đa 5 ảnh." });

                foreach (var file in input.Images)
                {
                    if (file.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(fileExtension))
                            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = $"File {file.FileName} không phải là ảnh hợp lệ." });

                        if (file.Length > 5 * 1024 * 1024)
                            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = $"File {file.FileName} vượt quá 5MB." });

                        try
                        {
                            // Gọi service Cloudinary để upload
                            var (url, publicId) = await _cloudinaryService.UploadImageAsync(file);
                            if (!string.IsNullOrEmpty(url)) uploadedImageUrls.Add(url);
                        }
                        catch (Exception uploadEx)
                        {
                            Console.WriteLine($"[ERROR] Upload image failed: {uploadEx.Message}");
                            // Tùy chọn: Có thể return lỗi luôn hoặc bỏ qua ảnh lỗi
                            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = $"Lỗi upload ảnh: {uploadEx.Message}" });
                        }
                    }
                }
            }

            // 4. Tạo record OrderReturn
            var returnRequest = new OrderReturn
            {
                OrderId = orderId,
                Reason = input.Reason,
                ProofImages = uploadedImageUrls.Any() ? string.Join(";", uploadedImageUrls) : "",
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            // 5. Cập nhật trạng thái Order
            order.Status = "return_requested"; // Đổi status

            // 6. Lưu Database
            _context.OrderReturns.Add(returnRequest);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseResult
            {
                IsSuccess = true,
                Message = "Đã gửi yêu cầu hoàn trả thành công.",
                Data = new
                {
                    OrderId = orderId,
                    ReturnRequestId = returnRequest.Id,
                    UploadedImagesCount = uploadedImageUrls.Count
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] RequestReturnOrder Service: {ex.Message}");
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = $"Lỗi xử lý: {ex.Message}" });
        }
    }

}