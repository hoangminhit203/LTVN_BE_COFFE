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

            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not authenticated");
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
