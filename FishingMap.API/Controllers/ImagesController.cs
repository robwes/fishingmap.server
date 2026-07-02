using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        // Only these top-level file share folders hold public images (see AzureFileService.AddFile)
        private static readonly string[] AllowedFolders = { "locations/", "species/" };

        private readonly IFileService _fileService;

        public ImagesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // GET api/<ImagesController>/locations/5/{file}
        [HttpGet("{*filePath}")]
        public async Task<IActionResult> Get(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)
                || filePath.Contains("..")
                || !AllowedFolders.Any(folder => filePath.StartsWith(folder, StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound();
            }

            var file = await _fileService.GetFile(filePath);
            if (file == null)
            {
                return NotFound();
            }

            return new FileStreamResult(file, file.ContentType);
        }
    }
}
