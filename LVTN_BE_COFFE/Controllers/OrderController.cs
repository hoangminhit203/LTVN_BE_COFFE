using DocumentFormat.OpenXml.Spreadsheet;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUserId()
        {
            // 1. Kiểm tra xem người dùng đã được xác thực chưa
            if (!User.Identity.IsAuthenticated)
            {
                // Hoặc trả về 401/403, nhưng ném Exception theo cách cũ để khớp với lỗi của bạn
                throw new System.Exception("User not authenticated");
            }

            // 2. Trích xuất ID từ Claim
            // Tùy thuộc vào cách bạn tạo Token, ID có thể là ClaimTypes.NameIdentifier HOẶC JwtRegisteredClaimNames.Sub.
            // Vì token của bạn dùng 'sub' làm ID, ta dùng nó:
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // 3. Nếu không tìm thấy, ném lỗi
                throw new System.Exception("User ID claim not found in token.");
            }

            return userId;
        }
        // GET: api/Order
        //[HttpGet]
        //public async Task<ActionResult<<OrderResponse>> GetAll([FromQuery] OrderFilterVModel filter)
        //{
        //    return await _orderService.GetAllOrders(filter);
        //}

        // GET: api/Order/{id}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrderUser()
        {
            var userId = GetUserId();
            return await _orderService.GetOrdersByUser(userId);
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<OrderResponse>?> CreateOrder([FromQuery] OrderCreateVModel model)
        {
            var userId = GetUserId();
            return await _orderService.CreateOrder(userId, model);
        }

        [HttpPut]
        public async Task<ActionResult<bool>> updateStatus(int orderId, string status)
        {
            var userId = GetUserId();
            return await _orderService.UpdateOrderStatus(orderId, status);
        }

    }
}
