using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;

        public CartItemController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] CartItemCreateVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartItemService.AddItemAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] CartItemUpdateVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartItemService.UpdateItemAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Xoá sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var result = await _cartItemService.RemoveItemAsync(cartItemId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm trong giỏ hàng
        /// </summary>
        [HttpGet("{cartId}")]
        public async Task<IActionResult> GetItemsByCart(int cartId)
        {
            var result = await _cartItemService.GetItemsByCartAsync(cartId);
            return Ok(result);
        }
    }
}
