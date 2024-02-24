using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public ImagesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // GET api/<ImagesController>/5
        [HttpGet("{*filePath}")]
        public async Task<FileStreamResult?> Get(string filePath)
        {
            var file = await _fileService.GetFile(filePath);

            if (file == null) 
            {
                return null;
            }

            return new FileStreamResult(file, file.ContentType);
        }
    }
}
