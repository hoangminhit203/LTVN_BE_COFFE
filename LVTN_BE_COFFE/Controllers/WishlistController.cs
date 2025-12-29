using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }
        private string GetUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                throw new System.Exception("User not authenticated");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new System.Exception("User ID claim not found in token.");
            }
            return userId;
        }
        [HttpGet]
        public async Task<ActionResult<PaginationModel<WishlistResponseVModel>>> Get([FromQuery] WishlistFilterVModel filter)
        {
            var userId = GetUserId();
            var wishlist = await _wishlistService.GetUserWishlist(userId, filter);
            return Ok(wishlist);
        }
        [HttpPost]
        public async Task<ActionResult<WishlistResponseVModel>> AddToWishlist([FromBody] WishlistCreateVModel model)
        {
            var userId = GetUserId();
            var wishlistItem = await _wishlistService.AddToWishlist(userId, model);
            return Ok(wishlistItem);
        }
        [HttpPost]
        [Route("add-multiple")]
        public async Task<ActionResult<List<WishlistResponseVModel>>> AddMultipleToWishlist(int wishlistId)
        {
            var userId = GetUserId();
            var wishlistItems = await _wishlistService.AddToCard(userId, wishlistId);
            return Ok(wishlistItems);
        }

        [HttpDelete("{wishlistId}")]
        public async Task<ActionResult> RemoveFromWishlist(int wishlistId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.RemoveFromWishlistById(wishlistId, userId);
            if (!result)
            {
                return NotFound(new { message = "Wishlist item not found or does not belong to the user." });
            }
            return NoContent();
        }
        [HttpPost]
        [Route("check")]
        public async Task<ActionResult<bool>> IsProductInWishlist([FromBody] int productId)
        {
            var userId = GetUserId();
            var exists = await _wishlistService.IsProductInWishlist(userId, productId);
            return Ok(exists);
        }

    }
}
