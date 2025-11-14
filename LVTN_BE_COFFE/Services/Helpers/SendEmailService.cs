using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using System.Net;
using System.Net.Mail;
using static LVTN_BE_COFFE.Domain.Common.Strings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LVTN_BE_COFFE.Services.Helpers
{
    public class SendEmailService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendEmailService> _logger;

        public SendEmailService(IConfiguration configuration, ILogger<SendEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResponseResult> SendMailAsync(string fromEmail, string fromPassWord, string toEmail, string sendMailTitle, string sendMailBody)
        {
            var result = new ResponseResult { IsSuccess = false };
            
            try
            {
                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassWord))
                {
                    result.Message = "Email credentials are missing";
                    return result;
                }

                var fromAddress = new MailAddress(fromEmail, "CoffeManager");
                var toAddress = new MailAddress(toEmail);
                
                using var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, fromPassWord),
                    Timeout = 30000 // 30 seconds timeout
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = sendMailTitle,
                    Body = sendMailBody,
                    IsBodyHtml = true
                };

                _logger.LogInformation($"Attempting to send email from {fromEmail} to {toEmail}");
                
                await smtp.SendMailAsync(message);
                
                result.IsSuccess = true;
                result.Message = Messages.EmailSentSuccess;
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP Error sending email to {toEmail}: {smtpEx.Message}");
                result.Message = $"SMTP Error: {smtpEx.Message}. StatusCode: {smtpEx.StatusCode}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General error sending email to {toEmail}: {ex.Message}");
                result.Message = string.Format(Messages.ErrorSendingEmail, ex.Message);
            }

            return result;
        }

        public async Task<ResponseResult> SendMailAsyncWithSmtp(string fromEmail, string toEmail, string sendMailTitle, string sendMailBody, SmtpClient smtp)
        {
            var result = new ResponseResult { IsSuccess = false };

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
            {
                result.Message = Messages.InvalidEmailAddress;
                return result;
            }

            try
            {
                var fromAddress = new MailAddress(fromEmail, "CoffeManager");
                var toAddress = new MailAddress(toEmail);

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = sendMailTitle,
                    Body = sendMailBody,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);
                result.IsSuccess = true;
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {toEmail}: {ex.Message}");
                result.Message = $"Lỗi: {ex.Message}";
            }

            return result;
        }
    }
}
