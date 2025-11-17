using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICartItemService
    {
        Task<ActionResult<CartItemResponse>> AddItem(string userId, CartItemCreateVModel request);
        Task<ActionResult<CartItemResponse>> UpdateItem(string userId, CartItemUpdateVModel request);
        Task<ActionResult<bool>> RemoveItem(string userId, int cartItemId);
        Task<ActionResult<IEnumerable<CartItemResponse>>> GetItemsByUserId(string userId);
    }
}
