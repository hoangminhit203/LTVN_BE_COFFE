using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;
        public CartItemsController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        // Trong CartItemsController.cs
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


        [HttpPost]
        public async Task<ActionResult<CartItemResponse>> Add([FromBody] CartItemCreateVModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            try
            {
                var item = await _cartItemService.AddItem(userId, model);
                return Ok(item);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPut]
        public async Task<ActionResult<CartItemResponse>> Update([FromBody] CartItemUpdateVModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            try
            {
                var item = await _cartItemService.UpdateItem(userId, model);
                return Ok(item);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<CartItemResponse>> Delete(int id)
        {
            var userId = GetUserId();
            try
            {
                var ok = await _cartItemService.RemoveItem(userId, id);
                return Ok(new { success = ok });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet]
        public async Task<ActionResult<CartItemResponse>> Get()
        {
            var userId = GetUserId();
            var items = await _cartItemService.GetItemsByUserId(userId);
            return Ok(items);
        }
    }
}
