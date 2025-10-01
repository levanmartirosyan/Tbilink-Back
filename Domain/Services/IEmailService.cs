using Tbilink_Back.DTOs;
using Tbilink_Back.Models;

namespace Tbilink_Back.Services.Interfaces
{
    public interface IEmailService
    {
        Task<ServiceResponse<bool>> SendEmail(string toEmail, string subject, string verificationCode);
    }
}
