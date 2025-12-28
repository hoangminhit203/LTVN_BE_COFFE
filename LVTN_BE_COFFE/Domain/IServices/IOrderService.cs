using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IOrderService
    {
        /// <summary>
        /// Tạo đơn hàng mới từ giỏ hàng
        /// </summary>
        Task<ActionResult<ResponseResult>> CreateOrder(string userId, OrderCreateVModel model);

        /// <summary>
        /// Lấy danh sách đơn hàng của một User cụ thể
        /// </summary>
        Task<ActionResult<ResponseResult>> GetOrdersByUser(string userId);

        /// <summary>
        /// Lấy chi tiết một đơn hàng (Check cả UserId để bảo mật)
        /// </summary>
        Task<ActionResult<ResponseResult>> GetOrder(int orderId, string userId);

        /// <summary>
        /// Admin/Shipper cập nhật trạng thái đơn hàng (Pending -> Shipping -> Completed...)
        /// </summary>
        Task<ActionResult<ResponseResult>> UpdateOrderStatus(int orderId, string status);

        /// <summary>
        /// Khách hàng hủy đơn (Chỉ hủy được khi còn Pending)
        /// </summary>
        Task<ActionResult<ResponseResult>> CancelOrder(int orderId, string userId);

        // Nếu sau này bạn muốn làm chức năng Admin quản lý tất cả đơn hàng thì mở comment này ra:
        // Task<ActionResult<ResponseResult>> GetAllOrders(OrderFilterVModel filter);
    }
}