using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IEnumerable<SpeciesDTO>>> Get([FromQuery] string search = "")
        {
            var species = await _speciesService.GetSpecies(search);
            return Ok(species);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpeciesDTO>> Get(int id)
        {
            var species = await _speciesService.GetSpeciesById(id);
            if (species == null)
            {
                return NotFound();
            }

            return Ok(species);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesDTO>> Post([FromForm]SpeciesAdd species)
        {
            var addedSpecies = await _speciesService.AddSpecies(species);
            return CreatedAtAction(nameof(Get), new { id = addedSpecies.Id }, addedSpecies);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesDTO>> Put(int id, [FromForm]SpeciesUpdate species)
        {
            var updatedSpecies = await _speciesService.UpdateSpecies(id, species);
            return Ok(updatedSpecies);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _speciesService.DeleteSpecies(id);
            return Ok();
        }

        [HttpPatch("{id}/info")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesDTO>> PatchInfo(int id, [FromBody] SpeciesInfoPatch patch)
        {
            var species = await _speciesService.UpdateSpeciesInfo(id, patch);
            return Ok(species);
        }

        [HttpPost("{id}/images")]
        [Authorize(Roles = "Administrator")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImageDTO>> PostImage(int id, [FromForm] IFormFile image)
        {
            var imageDto = await _speciesService.AddImageToSpecies(id, image);
            return CreatedAtAction(nameof(Get), new { id }, imageDto);
        }

        [HttpDelete("{id}/images/{imageId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            await _speciesService.RemoveImageFromSpecies(id, imageId);
            return NoContent();
        }
    }
}
