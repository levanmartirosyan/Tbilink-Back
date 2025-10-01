﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tbilink_Back.Services.Interfaces;

namespace Tbilink_Back.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost()]
        public async Task<IActionResult> UploadPhoto(IFormFile file, string path)
        {
            try
            {
                var url = await _fileUploadService.UploadFile(file, path);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
