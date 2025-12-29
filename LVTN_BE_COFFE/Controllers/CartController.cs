using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;
        public CartController(ICartService cartService,AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }
        private (string? userId, string? guestKey) GetCartIdentity()
        {
            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }

            var guestKey = Request.Headers["X-Guest-Key"].FirstOrDefault();

            return (userId, guestKey);
        }

        [HttpGet]
        public async Task<ActionResult<CartResponse>> Get()
        {
            var (userId, guestKey) = GetCartIdentity();

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
                return BadRequest("User identification or Guest-Key is required.");

            var cart = await _cartService.GetCartAsync(userId, guestKey);

            if (cart == null) return Ok(new { message = "Cart empty", items = new List<object>() });

            return Ok(cart);
        }

        [HttpPost("clear")]
        public async Task<ActionResult> Clear()
        {
            var (userId, guestKey) = GetCartIdentity();

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.Status == "Active" &&
                    ((userId != null && c.UserId == userId) ||
                     (guestKey != null && c.GuestKey == guestKey)));

            if (cart == null) return BadRequest("Cart not found");

            var ok = await _cartService.ClearCartAsync(cart.Id);
            return ok ? Ok(new { message = "Cart cleared" }) : StatusCode(500, "Failed to clear cart");
        }
    }
}
