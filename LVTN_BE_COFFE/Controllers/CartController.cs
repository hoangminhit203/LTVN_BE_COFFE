using LVTN_BE_COFFE.Domain.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new System.Exception("User not authenticated");

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserAsync(userId);
            if (cart == null) return Ok(new { message = "Cart empty" });
            return Ok(cart);
        }

        [HttpPost("clear")]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserAsync(userId);
            if (cart == null) return BadRequest("Cart not found");

            var ok = await _cartService.ClearCartAsync(cart.CartId, userId);
            return ok ? Ok() : StatusCode(500, "Failed to clear cart");
        }
    }
}
