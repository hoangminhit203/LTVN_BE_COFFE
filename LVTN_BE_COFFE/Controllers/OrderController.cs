using DocumentFormat.OpenXml.Spreadsheet;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // 1. Cho phép truy cập ẩn danh để Guest có thể đặt hàng
    [AllowAnonymous]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ============================================================
        // HÀM HỖ TRỢ: LẤY CẢ USER ID (TỪ TOKEN) VÀ GUEST KEY (TỪ HEADER)
        // ============================================================
        private (string? userId, string? guestKey) GetIdentity()
        {
            string? userId = null;
            
            // Debug: Log authentication status
            Console.WriteLine($"[DEBUG] User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            
            if (User.Identity?.IsAuthenticated == true)
            {
                // Log tất cả claims để debug
                Console.WriteLine("[DEBUG] User Claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  - {claim.Type}: {claim.Value}");
                }
                
                userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                Console.WriteLine($"[DEBUG] Extracted UserId: {userId}");
            }

            // Lấy GuestKey từ Header X-Guest-Key (do Frontend gửi lên)
            var guestKey = Request.Headers["X-Guest-Key"].FirstOrDefault();
            
            // Debug: Log tất cả headers
            Console.WriteLine("[DEBUG] Request Headers:");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"  - {header.Key}: {header.Value}");
            }
            
            Console.WriteLine($"[DEBUG] Final - UserId: {userId}, GuestKey: {guestKey}");

            return (userId, guestKey);
        }

        // ============================================================
        // 1. TẠO ĐƠN HÀNG MỚI
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> CreateOrder([FromBody] OrderCreateVModel model)
        {
            var (userId, guestKey) = GetIdentity();

            // Phải có ít nhất 1 trong 2 định danh
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
                return BadRequest(new ResponseResult { IsSuccess = false, Message = "Định danh người dùng không hợp lệ (Token hoặc GuestKey)." });

            return await _orderService.CreateOrder(userId, guestKey, model);
        }

        // ============================================================
        // 2. LẤY LỊCH SỬ ĐƠN HÀNG
        // ============================================================
        [HttpGet("history")]
        public async Task<ActionResult<ResponseResult>> GetMyOrders()
        {
            var (userId, guestKey) = GetIdentity();

            Console.WriteLine($"[DEBUG] GetMyOrders - UserId: '{userId}', GuestKey: '{guestKey}'");

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
            {
                Console.WriteLine("[WARNING] Cả UserId và GuestKey đều null/empty!");
                return Ok(new ResponseResult 
                { 
                    IsSuccess = true, 
                    Data = new List<object>(), 
                    Message = "Không có định danh người dùng. Vui lòng đăng nhập hoặc gửi X-Guest-Key header." 
                });
            }

            return await _orderService.GetOrdersByIdentity(userId, guestKey);
        }

        // ============================================================
        // 3. LẤY CHI TIẾT 1 ĐƠN HÀNG
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetOrderById(int id)
        {
            var (userId, guestKey) = GetIdentity();
            return await _orderService.GetOrder(id, userId, guestKey);
        }

        // ============================================================
        // 4. KHÁCH HÀNG HỦY ĐƠN
        // ============================================================
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ResponseResult>> CancelOrder(int id)
        {
            var (userId, guestKey) = GetIdentity();
            return await _orderService.CancelOrder(id, userId, guestKey);
        }

        // ============================================================
        // 5. ADMIN/SHIPPER CẬP NHẬT TRẠNG THÁI
        // ============================================================
        [HttpPut("{id}/status")]
        // [Authorize(Roles = "Admin,Shipper")] // Mở ra nếu bạn đã phân quyền
        public async Task<ActionResult<ResponseResult>> UpdateStatus(int id, [FromQuery] string status)
        {
            return await _orderService.UpdateOrderStatus(id, status);
        }
    }
}