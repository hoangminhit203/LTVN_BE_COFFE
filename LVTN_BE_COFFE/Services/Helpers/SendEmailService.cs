using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using System.Net;
using System.Net.Mail;
using static LVTN_BE_COFFE.Domain.Common.Strings;

namespace LVTN_BE_COFFE.Services.Helpers
{
    public class SendEmailService : IEmailSenderService
    {
        public async Task<ResponseResult> SendMailAsync(string fromEmail, string fromPassWord, string toEmail, string sendMailTitle, string sendMailBody)
        {
            var result = new ResponseResult { IsSuccess = false };
            if (!string.IsNullOrEmpty(fromEmail) && !string.IsNullOrEmpty(fromPassWord))
            {
                var fromAddress = new MailAddress(fromEmail, sendMailTitle);
                var toAddress = new MailAddress(toEmail, sendMailTitle);
                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, fromPassWord)
                })
                {
                    var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = sendMailTitle,
                        Body = sendMailBody,
                        IsBodyHtml = true
                    };
                    using (message)
                    {
                        try
                        {
                            await smtp.SendMailAsync(message);
                            result.IsSuccess = true;
                            result.Message = Messages.EmailSentSuccess;
                        }
                        catch (Exception ex)
                        {
                            result.Message = string.Format(Messages.ErrorSendingEmail, ex.Message);
                        }
                    }
                }
            }
            return result;
        }

        public async Task<ResponseResult> SendMailAsyncWithSmtp(string fromEmail, string toEmail, string sendMailTitle, string sendMailBody, SmtpClient smtp)
        {
            var result = new ResponseResult { IsSuccess = false };

            if (!string.IsNullOrEmpty(fromEmail) && !string.IsNullOrEmpty(toEmail))
            {
                var fromAddress = new MailAddress(fromEmail, sendMailTitle);
                var toAddress = new MailAddress(toEmail, sendMailTitle);

                var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = sendMailTitle,
                    Body = sendMailBody,
                    IsBodyHtml = true
                };

                using (message)
                {
                    try
                    {
                        await smtp.SendMailAsync(message);
                        result.IsSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Lỗi: {ex.Message}";
                    }
                }
            }
            else
            {
                result.Message = Messages.InvalidEmailAddress;
            }

            return result;
        }
    }
}
