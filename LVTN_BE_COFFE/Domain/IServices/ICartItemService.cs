using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICartItemService
    {
        Task<ActionResult<CartItemResponse>> AddItemAsync(CartItemCreateVModel request);
        Task<ActionResult<CartItemResponse>> UpdateItemAsync(CartItemUpdateVModel request);
        Task<ActionResult<bool>> RemoveItemAsync(int cartItemId);
        Task<ActionResult<IEnumerable<CartItemResponse>>> GetItemsByCartAsync(int cartId);
    }
}
