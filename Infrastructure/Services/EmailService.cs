using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Tbilink_Back.Models;
using Tbilink_Back.Services.Interfaces;

namespace Tbilink_Back.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<ServiceResponse<bool>> SendEmail(string toEmail, string subject, string body)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.User, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.User, "Tbilink"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                response.IsSuccess = true;
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = $"Email sending failed: {ex.Message}";
            }

            return response;
        }
    }
}
