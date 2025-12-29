using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IOrderService
    {
        Task<ActionResult<ResponseResult>> CreateOrder(string? userId, string? guestKey, OrderCreateVModel model);
        Task<ActionResult<ResponseResult>> GetOrdersByIdentity(string? userId, string? guestKey);
        Task<ActionResult<ResponseResult>> GetOrder(int orderId, string? userId, string? guestKey);
        Task<ActionResult<ResponseResult>> UpdateOrderStatus(int orderId, string status);
        Task<ActionResult<ResponseResult>> CancelOrder(int orderId, string? userId, string? guestKey);
        Task<ActionResult<ResponseResult>> GetAllOrder();
    }
}