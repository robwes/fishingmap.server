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
        public async Task<IEnumerable<SpeciesDTO>> Get([FromQuery] string search = "")
        {
            var species = await _speciesService.GetSpecies(search);
            return species;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<SpeciesDTO> Get(int id)
        {
            var species = await _speciesService.GetSpeciesById(id);
            return species;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<SpeciesDTO> Post([FromForm]SpeciesAdd species)
        {
            var sp = await _speciesService.AddSpecies(species);
            return sp;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<SpeciesDTO> Put(int id, [FromForm]SpeciesUpdate species)
        {
            var sp = await _speciesService.UpdateSpecies(id, species);
            return sp;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task Delete(int id)
        {
            await _speciesService.DeleteSpecies(id);
        }
    }
}
