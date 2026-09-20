using FirstAPIProject.Application.Common.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public async Task<string> SaveAvatarAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
            {
                throw new ArgumentException("Invalid image file format. Only JPG, PNG, and WEBP are allowed.");
            }

            if (fileStream.Length > MaxFileSize)
            {
                throw new ArgumentException("File size exceeds the 5MB limit.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var destinationPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(destinationStream, cancellationToken);
            }

            return $"/uploads/avatars/{uniqueFileName}";
        }
    }
}
