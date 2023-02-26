using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IFileService
    {
        Task<string> AddFile(IFormFile image, string folder);
        Task DeleteFile(string path);
        Task DeleteFolder(string path);
        Task<ContentTypeStream> GetFile(string filePath);
    }
}
