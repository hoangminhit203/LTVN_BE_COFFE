using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
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

        [HttpGet]
        public async Task<ActionResult<CartResponse>> Get()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserAsync(userId);
            if (cart == null) return Ok(new { message = "Cart empty" });
            return Ok(cart);
        }

        [HttpPost("clear")]
        public async Task<ActionResult<CartResponse>> Clear()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserAsync(userId);
            if (cart == null) return BadRequest("Cart not found");

            var ok = await _cartService.ClearCartAsync(cart.CartId, userId);
            return ok ? Ok() : StatusCode(500, "Failed to clear cart");
        }
    }
}
