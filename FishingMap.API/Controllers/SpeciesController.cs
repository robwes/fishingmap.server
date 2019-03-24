using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeciesController : ControllerBase
    {
        private readonly ISpeciesService _service;

        public SpeciesController(ISpeciesService service)
        {
            _service = service;
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IEnumerable<Species>> Get()
        {
            var species = await _service.GetSpecies();
            return species;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<Species> Get(int id)
        {
            var species = await _service.GetSpeciesById(id);
            return species;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<Species> Post([FromBody]Species species)
        {
            var sp = await _service.AddSpecies(species);
            return sp;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<Species> Put(int id, [FromBody]Species species)
        {
            var sp = await _service.UpdateSpecies(species);
            return sp;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
