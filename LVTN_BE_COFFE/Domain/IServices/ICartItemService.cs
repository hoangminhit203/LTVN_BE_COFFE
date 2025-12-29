using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICartItemService
    {
        Task<ActionResult<CartItemResponse>> AddItem(string? userId, string? guestKey, CartItemCreateVModel request);
        Task<ActionResult<CartItemResponse>> UpdateItem(string? userId, string? guestKey, CartItemUpdateVModel request);
        Task<ActionResult<bool>> RemoveItem(string? userId, string? guestKey, int cartItemId);
        Task<ActionResult<IEnumerable<CartItemResponse>>> GetItems(string? userId, string? guestKey);
    }
}
