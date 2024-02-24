using Microsoft.AspNetCore.Http;

namespace FishingMap.Domain.Interfaces
{
    public interface IFileService
    {
        Task<string> AddFile(IFormFile image, string folder);
        Task DeleteFile(string path);
        Task DeleteFolder(string path);
        Task<ContentTypeStream?> GetFile(string filePath);
    }
}
