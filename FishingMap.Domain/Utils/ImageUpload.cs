using Microsoft.AspNetCore.Http;

namespace FishingMap.Domain.Utils
{
    public static class ImageUpload
    {
        public const long MaxSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        /// <summary>
        /// Throws ArgumentException (→ 400 via ApiExceptionFilter) for files that
        /// aren't a supported image type or exceed the size cap.
        /// </summary>
        public static void Validate(IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new ArgumentException("Image file is empty.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    $"Unsupported image type '{extension}'. Allowed: {string.Join(", ", AllowedExtensions)}.");
            }

            if (file.Length > MaxSizeBytes)
            {
                throw new ArgumentException(
                    $"Image is too large ({file.Length / (1024 * 1024)} MB). Maximum is {MaxSizeBytes / (1024 * 1024)} MB.");
            }
        }
    }
}
