using Tbilink_Back.DTOs;
using Tbilink_Back.Models;

namespace Tbilink_Back.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ServiceResponse<int>> Register(RegisterDTO registerDTO);
        public Task<ServiceResponse<string>> Login(LoginDTO loginDTO);
        public Task<ServiceResponse<string>> RequestVerification(string email);
        public Task<ServiceResponse<string>> VerifyEmail(string email, string code);
        public Task<ServiceResponse<bool>> VerifyUsername(VerifyUsernameDTO userameDTO);
    }
}
