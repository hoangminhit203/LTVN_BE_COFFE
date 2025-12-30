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
        [HttpGet("check-stock")]
        public async Task<ActionResult<StockCheckResponse>> CheckStock()
        {
            var (userId, guestKey) = GetCartIdentity();

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
                return BadRequest("User identification or Guest-Key is required.");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.Status == "Active" &&
                    ((userId != null && c.UserId == userId) ||
                     (guestKey != null && c.GuestKey == guestKey)));

            if (cart == null || !cart.CartItems.Any())
            {
                return Ok(new StockCheckResponse
                {
                    IsAvailable = true,
                    Message = "Giỏ hàng trống",
                    Items = new List<StockCheckItemResponse>()
                });
            }

            var stockCheckItems = new List<StockCheckItemResponse>();
            var allAvailable = true;

            foreach (var item in cart.CartItems)
            {
                var isAvailable = item.ProductVariant.Stock >= item.Quantity;
                if (!isAvailable) allAvailable = false;

                stockCheckItems.Add(new StockCheckItemResponse
                {
                    CartItemId = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductVariant?.Product?.Name ?? "Unknown",
                    RequestedQuantity = item.Quantity,
                    AvailableStock = item.ProductVariant.Stock,
                    IsAvailable = isAvailable,
                    Message = isAvailable
                        ? "Sản phẩm còn hàng"
                        : $"Chỉ còn {item.ProductVariant.Stock} sản phẩm"
                });
            }

            return Ok(new StockCheckResponse
            {
                IsAvailable = allAvailable,
                Message = allAvailable
                    ? "Tất cả sản phẩm đều có sẵn"
                    : "Một số sản phẩm không đủ số lượng",
                Items = stockCheckItems
            });
        }
    }
}
