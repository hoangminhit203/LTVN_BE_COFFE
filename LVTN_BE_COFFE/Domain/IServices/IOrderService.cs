using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IOrderService
    {
        Task<ActionResult<ResponseResult>> CreateOrder(string? userId, string? guestKey, OrderCreateVModel model);
        Task<ActionResult<ResponseResult>> GetOrdersByIdentity(string? userId, string? guestKey);
        Task<ActionResult<ResponseResult>> GetOrder(string orderId);
        Task<ActionResult<ResponseResult>> UpdateOrderStatus(string orderId, string status);
        Task<ActionResult<ResponseResult>> CancelOrder(string orderId, string? userId, string? guestKey);
        Task<ActionResult<ResponseResult>> GetAllOrder();
        Task<ActionResult<ResponseResult>> GetOrderAdmin(string orderId);
        Task<ActionResult<ResponseResult>> RequestReturnOrder(string orderId, string? userId, string? guestKey, ReturnOrderInputModel input);
        Task<ActionResult<ResponseResult>> GetReturnRequests();
        Task<ActionResult<ResponseResult>> ProcessReturnRequest(int requestId, ProcessReturnModel model);
    }
}