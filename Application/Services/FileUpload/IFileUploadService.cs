namespace Tbilink_Back.Services.Interfaces
{
public interface IFileUploadService
{
    Task<string> UploadFile(IFormFile file, string folder);
}
}
