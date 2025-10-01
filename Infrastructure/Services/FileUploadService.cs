using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tbilink_Back.Data;
using Tbilink_Back.Models;
using Tbilink_Back.Services.Interfaces;

namespace Tbilink_Back.Services.Implementations
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContext;

        public FileUploadService(IWebHostEnvironment env, IHttpContextAccessor httpContext)
        {
            _env = env;
            _httpContext = httpContext;
        }

        public async Task<string> UploadFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");

            var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/{folder}/{fileName}";
        }
    }
}
