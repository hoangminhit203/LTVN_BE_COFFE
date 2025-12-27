using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly AppDbContext _context;

        public PaymentController(IVnPayService vnPayService, AppDbContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        [HttpPost("create-vnpay-url/{orderId}")]
        public async Task<IActionResult> CreatePaymentUrlVnpay(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Đơn hàng không tồn tại");
            }
            // Tạo model thông tin thanh toán đơn hàng
            var paymentInfo = new PaymentInfomationModel
            {
                OrderId = orderId,
                OrderType = order.Status ?? "pending",
                Amount = (double)order.TotalAmount,
                OrderDescription = $"Thanh toán đơn hàng #{orderId}",
                Name = order.User?.UserName ?? "Khách"
            };
            // Tạo URL thanh toán VNPAY
            var url = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext, orderId);
            return Ok(new { PaymentUrl = url });
        }

        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VnPayCallback()
        {
            var result = await _vnPayService.ProcessVnPayCallbackAsync(Request.Query);
            if (!result)
                return BadRequest("Payment callback failed or order not found.");
            return Ok(new { Message = "Payment status updated successfully." });
        }
    }
}