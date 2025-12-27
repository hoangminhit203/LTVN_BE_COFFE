using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Http;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInfomationModel model, HttpContext context, int orderId);
        Task<bool> ProcessVnPayCallbackAsync(IQueryCollection query);
    }
}
