using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace FishingMap.API.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<string> AddImage(IFormFile image, string folder)
        {
            var imagesFolder = Path.Combine(_environment.ContentRootPath, "StaticFiles", "Images");

            var folderFullPath = Path.Combine(imagesFolder, folder);
            Directory.CreateDirectory(folderFullPath);

            // For the file name of the uploaded file stored
            // server-side, use Path.GetRandomFileName to generate a safe
            // random file name.
            var trustedFileNameForFileStorage = Path.ChangeExtension(Path.GetRandomFileName(), Path.GetExtension(image.FileName));
            var filePath = Path.Combine(
                folderFullPath, trustedFileNameForFileStorage);

            using (var fileStream = System.IO.File.Create(filePath))
            {
                await image.CopyToAsync(fileStream);
                return Path.Combine(folder, trustedFileNameForFileStorage).Replace("\\", "/");
            }
        }

        public void DeleteImage(string path)
        {
            var fullPathToFile = Path.Combine(_environment.ContentRootPath, "StaticFiles/Images", path);
            System.IO.File.Delete(fullPathToFile);
        }
    }
}
