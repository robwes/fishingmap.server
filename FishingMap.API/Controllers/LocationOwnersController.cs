using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationOwnersController : ControllerBase
    {
        private readonly ILocationOwnersService _service;

        public LocationOwnersController(ILocationOwnersService service)
        {
            _service = service;
        }

        // GET: api/LocationOwners
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LocationOwners/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LocationOwners
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/LocationOwners/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
