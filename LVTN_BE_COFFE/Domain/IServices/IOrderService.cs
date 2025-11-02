using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IOrderService
    {
        Task<ActionResult<OrderResponse>> CreateOrderAsync(OrderCreateVModel request);
        Task<ActionResult<OrderResponse>> UpdateOrderAsync(OrderUpdateVModel request);
        Task<ActionResult<bool>> DeleteOrderAsync(int orderId);
        Task<ActionResult<OrderResponse>> GetOrderByIdAsync(int orderId);
        Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersAsync(OrderFilterVModel filter);
    }
}
