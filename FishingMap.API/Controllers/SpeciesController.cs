using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeciesController : ControllerBase
    {
        private readonly ISpeciesService _speciesService;

        public SpeciesController(ISpeciesService service)
        {
            _speciesService = service;
        }

        // GET: api/<controller>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<Species>> Get([FromQuery] string search = "")
        {
            var species = await _speciesService.GetSpecies(search);
            return species;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<Species> Get(int id)
        {
            var species = await _speciesService.GetSpeciesById(id);
            return species;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<Species> Post([FromForm]SpeciesUpdate species)
        {
            var sp = await _speciesService.AddSpecies(species);
            return sp;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<Species> Put(int id, [FromForm]SpeciesUpdate species)
        {
            var sp = await _speciesService.UpdateSpecies(id, species);
            return sp;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _speciesService.DeleteSpecies(id);
        }

        [HttpPost("UploadImage")]
        public async Task UploadImage(List<IFormFile> images)
        {
            var targetFilePath = "C:\\Users\\rwest\\source\\repos\\fishingmap\\fishingmap.server\\FishingMap.Domain\\Images\\";

            foreach (var image in images)
            {
                // For the file name of the uploaded file stored
                // server-side, use Path.GetRandomFileName to generate a safe
                // random file name.
                
                var trustedFileNameForFileStorage = Path.ChangeExtension(Path.GetRandomFileName(), Path.GetExtension(image.FileName));
                var filePath = Path.Combine(
                    targetFilePath, trustedFileNameForFileStorage);

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(fileStream);
                }
            }
        }
    }
}
