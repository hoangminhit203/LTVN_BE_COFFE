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
    [Route("api/order")]
    [Authorize] // Bắt buộc đăng nhập cho toàn bộ Controller
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ============================================================
        // HÀM HỖ TRỢ: LẤY USER ID TỪ TOKEN
        // ============================================================
        private string GetUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User not authenticated");

            // Ưu tiên lấy 'sub', nếu không có thì lấy 'nameidentifier'
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID claim not found in token.");

            return userId;
        }

        // ============================================================
        // 1. TẠO ĐƠN HÀNG MỚI
        // POST: api/order
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> CreateOrder([FromBody] OrderCreateVModel model)
        {
            // Lưu ý: Dùng [FromBody] để nhận JSON phức tạp
            var userId = GetUserId();
            return await _orderService.CreateOrder(userId, model);
        }

        // ============================================================
        // 2. LẤY LỊCH SỬ ĐƠN HÀNG CỦA TÔI
        // GET: api/order/history
        // ============================================================
        [HttpGet("history")]
        public async Task<ActionResult<ResponseResult>> GetMyOrders()
        {
            var userId = GetUserId();
            return await _orderService.GetOrdersByUser(userId);
        }

        // ============================================================
        // 3. LẤY CHI TIẾT 1 ĐƠN HÀNG
        // GET: api/order/{id}
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetOrderById(int id)
        {
            var userId = GetUserId();
            return await _orderService.GetOrder(id, userId);
        }

        // ============================================================
        // 4. KHÁCH HÀNG HỦY ĐƠN
        // PUT: api/order/{id}/cancel
        // ============================================================
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ResponseResult>> CancelOrder(int id)
        {
            var userId = GetUserId();
            return await _orderService.CancelOrder(id, userId);
        }

        // ============================================================
        // 5. ADMIN/SHIPPER CẬP NHẬT TRẠNG THÁI (Pending -> Shipping...)
        // PUT: api/order/{id}/status?status=shipping
        // ============================================================
        [HttpPut("{id}/status")]
        // [Authorize(Roles = "Admin,Shipper")] // Nếu muốn chỉ Admin được gọi
        public async Task<ActionResult<ResponseResult>> UpdateStatus(int id, [FromQuery] string status)
        {
            // Hàm này không cần check UserId sở hữu đơn, vì Admin có quyền sửa mọi đơn
            return await _orderService.UpdateOrderStatus(id, status);
        }
    }
}