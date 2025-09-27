using LVTN_BE_COFFE.Domain.Model;
using System.Net.Mail;
namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IEmailSenderService
    {
        Task<ResponseResult> SendMailAsync(string fromEmail, string fromPassWord, string toEmail, string sendMailTitle, string sendMailBody);
        Task<ResponseResult> SendMailAsyncWithSmtp(string fromEmail, string toEmail, string sendMailTitle, string sendMailBody, SmtpClient smtp);
    }
}
