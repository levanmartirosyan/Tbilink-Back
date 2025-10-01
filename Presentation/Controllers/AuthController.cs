using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tbilink_Back.Data;
using Tbilink_Back.DTOs;
using Tbilink_Back.Models;
using Tbilink_Back.Services.Helpers;
using Tbilink_Back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Tbilink_Back.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IEmailService emailService, IAuthService authService)
        {
            _context = context;
            _emailService = emailService;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(LoginDTO loginDTO)
        {
            var result = await _authService.Login(loginDTO);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ServiceResponse<int>
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid request payload"
                });
            }

            var response = await _authService.Register(registerDTO);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> RequestVerification([FromBody] RequestVerificationDTO request)
        {
            var response = await _authService.RequestVerification(request.Email);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO request)
        {
            var response = await _authService.VerifyEmail(request.Email, request.Code);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("verify-username")]
        public async Task<IActionResult> VerifyUsername([FromBody] VerifyUsernameDTO usernameDTO)
        {
            var response = await _authService.VerifyUsername(usernameDTO);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
