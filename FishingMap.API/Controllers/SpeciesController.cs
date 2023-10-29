using FishingMap.Domain.Data.DTO.SpeciesObjects;
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
        public async Task<Species> Post([FromForm]SpeciesAdd species)
        {
            var sp = await _speciesService.AddSpecies(species);
            return sp;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<Species> Put(int id, [FromForm]SpeciesUpdate species)
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
