using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Libraries;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore; // Cần dòng này để dùng .Include()

namespace LVTN_BE_COFFE.Services.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor: Chỉ inject những thứ cần thiết, bỏ email
        public VnPayService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string CreatePaymentUrl(PaymentInfomationModel model, HttpContext context, int OrderId)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnpayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", OrderId.ToString());

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            return paymentUrl;
        }

        public async Task<bool> ProcessVnPayCallbackAsync(IQueryCollection query)
        {
            var hashSecret = _configuration["Vnpay:HashSecret"];
            var vnpayLib = new VnpayLibrary();

            // Lấy toàn bộ dữ liệu trả về
            var paymentResult = vnpayLib.GetFullResponseData(query, hashSecret);

            // Parse OrderId từ vnp_TxnRef
            var orderId = int.TryParse(paymentResult.OrderId, out var oid) ? oid : 0;

            // Truy vấn đơn hàng
            // LƯU Ý: Đã bỏ OrderAdd và UserGuest vì không có trong Entity Order bạn gửi
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Origin)
                .Include(o => o.OrderItems).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Acidity)
                .Include(o => o.OrderItems).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Weight)
                .Include(o => o.OrderItems).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Certifications)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId); // SỬA: Dùng o.OrderId thay vì o.Id

            if (order == null)
                return false;

            // Xử lý cập nhật trạng thái
            if (paymentResult.Success && paymentResult.VnPayResponseCode == "00")
            {
                // -- Thành công --
                // Vì Order entity chỉ có field Status (string), ta gán trực tiếp chuỗi
                order.Status = "paid";
                // order.IsPaid = true; // Field này không có trong Entity Order nên bỏ đi
            }
            else if (paymentResult.VnPayResponseCode == "24")
            {
                // -- Người dùng hủy --
                order.Status = "cancelled";
            }
            else
            {
                // -- Lỗi khác --
                order.Status = "failed";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}