using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public interface IOrderService
{
    // Tạo đơn hàng từ giỏ hàng của user
    Task<ActionResult<OrderResponse>?> CreateOrder(string userId, OrderCreateVModel model);

    // Lấy danh sách đơn của user (có phân trang)
    Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUser(string userId);

    // Lấy chi tiết 1 đơn hàng
    Task<ActionResult<OrderResponse>?> GetOrder(int orderId, string userId);

    // Admin: Lấy toàn bộ đơn (phân trang)
    //Task<ActionResult<PaginationModel<OrderResponse>>> GetAllOrders(OrderFilterVModel filter);

    // Admin: Cập nhật trạng thái đơn
    Task<ActionResult<bool>> UpdateOrderStatus(int orderId, string status);

    // Xóa đơn hàng
    Task<ActionResult<bool>> CancelOrder(int orderId, string userId);
}
