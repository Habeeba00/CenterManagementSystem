using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CenterManagement.Application.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadPhotoAsync(IFormFile file, string subfolder)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"File extension '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException(
                    "File size exceeds the 5 MB limit.");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", subfolder);

            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"uploads/{subfolder}/{fileName}";
        }

        public void DeleteFile(string relativePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // File is locked by another process (e.g., IIS static file handler).
                    // Swallow the error — the orphaned file is harmless and the business
                    // operation (photo replacement, profile update) must not be blocked.
                }
            }
        }
    }
}
