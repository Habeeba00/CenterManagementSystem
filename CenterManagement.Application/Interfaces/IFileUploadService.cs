using Microsoft.AspNetCore.Http;

namespace CenterManagement.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadPhotoAsync(IFormFile file, string subfolder);
        void DeleteFile(string relativePath);
    }
}
