using FishingMap.Domain.Data.DTO.PermitObjects;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermitsController : ControllerBase
    {
        private readonly IPermitsService _permitService;

        public PermitsController(IPermitsService permitsService)
        {
            _permitService = permitsService;
        }

        // GET: api/<PermitsController>
        [HttpGet]
        public async Task<IEnumerable<Permit>> Get([FromQuery] string search = "")
        {
            var permits = await _permitService.GetPermits(search);
            return permits;
        }

        // GET api/<PermitsController>/5
        [HttpGet("{id}")]
        public async Task<Permit> Get(int id)
        {
            var permit = await _permitService.GetPermit(id);
            return permit;
        }

        // POST api/<PermitsController>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<Permit> Post([FromForm] Permit permit)
        {
            var newPermit = await _permitService.AddPermit(permit);
            return newPermit;
        }

        // PUT api/<PermitsController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<Permit> Put(int id, [FromForm] Permit permit)
        {
            var updatedPermit = await _permitService.UpdatePermit(id, permit);
            return updatedPermit;
        }

        // DELETE api/<PermitsController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task Delete(int id)
        {
            await _permitService.DeletePermit(id);
        }
    }
}
