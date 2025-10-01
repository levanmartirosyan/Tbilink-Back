using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Tbilink_Back.Data;
using Tbilink_Back.DTOs;
using Tbilink_Back.Models;
using Tbilink_Back.Services.Helpers;
using Tbilink_Back.Services.Interfaces;

namespace Tbilink_Back.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public AuthService(ApplicationDbContext dbContext, IConfiguration configuration, IEmailService emailService, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _emailService = emailService;
            _env = env;
        }

        public async Task<ServiceResponse<string>> Login(LoginDTO loginDTO)
        {
            var result = new ServiceResponse<string>();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());

            if (user == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User email not found";
            }
            else if (loginDTO.Password.Length < 8)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Password must be at least 8 symbols";
            }
            else if (!VerifyPasswordHash(loginDTO.Password, user.PasswordHash, user.PasswordSalt))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User password is incorrect";
            }
            else
            {
                result.IsSuccess = true;
                result.Data = GenerateToken(user);
            }

            return result;
        }

        public async Task<ServiceResponse<int>> Register(RegisterDTO registerDTO)
        {
            var result = new ServiceResponse<int>();

            if (string.IsNullOrWhiteSpace(registerDTO.FirstName) || registerDTO.FirstName.Length < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "First name is required (min length 1).";
                return result;
            }

            if (string.IsNullOrWhiteSpace(registerDTO.LastName) || registerDTO.LastName.Length < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Last name is required (min length 1).";
                return result;
            }

            if (!ValidationHelper.IsValidEmail(registerDTO.Email))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Invalid email format.";
                return result;
            }

            if (!ValidationHelper.IsValidPassword(registerDTO.Password))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Password must be at least 8 characters long, include 1 uppercase, 1 number, and 1 special character.";
                return result;
            }

            if (!ValidationHelper.PasswordCompare(registerDTO.Password, registerDTO.RepPassword))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Passwords do not match.";
                return result;
            }

            var exists = await UserExistCheck(registerDTO.Email);
            if (exists)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"User with email {registerDTO.Email} already exists!";
                return result;
            }

            CreatePasswordHash(registerDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                Role = "User",
                RegisterDate = DateTime.UtcNow,

                Country = registerDTO.Country,
                City = registerDTO.City,
                PhoneNumber = registerDTO.PhoneNumber,
                ProfilePhotoUrl = registerDTO.ProfilePhotoUrl,
                CoverPhotoUrl = registerDTO.CoverPhotoUrl,
                Description = registerDTO.Description,

                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,

                PostCount = 0,
                FollowersCount = 0,
                FollowingCount = 0,
                IsPublicProfile = true,
                ShowEmail = false,
                ShowPhone = false,
                AllowTagging = true,
                EmailNotifications = true,
                PushNotifications = true,
                SmsNotifications = false,
                MarketingEmails = false,
                Language = "en",
                TimeZone = "UTC",
                IsOnline = false,
                LastActive = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            result.Data = user.Id;
            result.IsSuccess = true;
            return result;
        }

        public async Task<ServiceResponse<string>> RequestVerification(string email)
        {
            var result = new ServiceResponse<string>();

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (string.IsNullOrWhiteSpace(email) || !emailRegex.IsMatch(email))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Invalid email format.";
                return result;
            }

            var existing = await _dbContext.EmailVerifications
                .FirstOrDefaultAsync(c => c.Email == email && !c.IsUsed);

            if (existing != null)
            {
                if (existing.ExpiresAt > DateTime.UtcNow)
                {
                    // Still valid → block
                    result.IsSuccess = false;
                    result.ErrorMessage = "A verification code has already been sent. Please wait until it expires.";
                    return result;
                }
                else
                {
                    _dbContext.EmailVerifications.Remove(existing);
                    await _dbContext.SaveChangesAsync();
                }
            }

            var code = OtpHelper.GenerateCode();
            var codeHash = OtpHelper.HashCode(code);

            var record = new EmailVerification
            {
                Email = email,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _dbContext.EmailVerifications.Add(record);
            await _dbContext.SaveChangesAsync();

            var templatePath = Path.Combine(_env.ContentRootPath, "Resources/Templates", "EmailVerification.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);
            var body = htmlTemplate.Replace("{{CODE}}", code);

            var emailResult = await _emailService.SendEmail(email, "Verify your email", body);

            if (!emailResult.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = emailResult.ErrorMessage;
                return result;
            }

            result.IsSuccess = true;
            result.Data = "Verification code sent";
            return result;
        }

        public async Task<ServiceResponse<string>> VerifyEmail(string email, string code)
        {
            var record = await _dbContext.EmailVerifications
                .Where(c => c.Email == email && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.ExpiresAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid or expired code."
                };

            if (record.CodeHash != OtpHelper.HashCode(code))
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid code."
                };

            record.IsUsed = true;
            await _dbContext.SaveChangesAsync();

            return new ServiceResponse<string>
            {
                IsSuccess = true,
                Data = "Email verified successfully!"
            };
        }
        public async Task<ServiceResponse<bool>> VerifyUsername(VerifyUsernameDTO usernameDTO)
        {
            var exists = await _dbContext.Users
                .AnyAsync(u => u.UserName == usernameDTO.UserName);

            if (exists)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    ErrorMessage = "Username is already in use."
                };
            }

            return new ServiceResponse<bool>
            {
                IsSuccess = true,
                Data = true
            };
        }


        private string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            SecurityToken token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }

        private async Task<bool> UserExistCheck(string userEmail)
        {
            var result = default(bool);

            result = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == userEmail.ToLower());

            return result;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
