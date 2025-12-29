using DocumentFormat.OpenXml.Spreadsheet;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // 1. Thay [Authorize] bằng [AllowAnonymous] để Guest có thể thêm hàng
    [AllowAnonymous]
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;

        public CartItemsController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        // 2. Hàm lấy danh tính linh hoạt (UserId hoặc GuestKey)
        private (string? userId, string? guestKey) GetIdentity()
        {
            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }

            // Lấy GuestKey từ Header
            var guestKey = Request.Headers["X-Guest-Key"].FirstOrDefault();

            return (userId, guestKey);
        }

        [HttpPost]
        public async Task<ActionResult<CartItemResponse>> Add([FromBody] CartItemCreateVModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (userId, guestKey) = GetIdentity();

            // Nếu không có cả 2 định danh thì không cho thêm hàng
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
                return BadRequest("User identification or Guest-Key is required.");

            var result = await _cartItemService.AddItem(userId, guestKey, model);
            return result;
        }

        [HttpPut]
        public async Task<ActionResult<CartItemResponse>> Update([FromBody] CartItemUpdateVModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (userId, guestKey) = GetIdentity();

            var result = await _cartItemService.UpdateItem(userId, guestKey, model);
            return result;
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var (userId, guestKey) = GetIdentity();

            var result = await _cartItemService.RemoveItem(userId, guestKey, id);
            return result;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemResponse>>> Get()
        {
            var (userId, guestKey) = GetIdentity();

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey))
                return Ok(new List<CartItemResponse>());

            var result = await _cartItemService.GetItems(userId, guestKey);
            return result;
        }
    }
}