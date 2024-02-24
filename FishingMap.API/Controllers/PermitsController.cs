using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<PermitDTO>> Get([FromQuery] string search = "")
        {
            var permits = await _permitService.GetPermits(search);
            return Ok(permits);
        }

        // GET api/<PermitsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermitDTO>> Get(int id)
        {
            var permit = await _permitService.GetPermit(id);
            if (permit == null)
            {
                return NotFound();
            }

            return Ok(permit);
        }

        // POST api/<PermitsController>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PermitDTO>> Post([FromForm] PermitDTO permit)
        {
            var newPermit = await _permitService.AddPermit(permit);
            return Created("success", newPermit);
        }

        // PUT api/<PermitsController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PermitDTO>> Put(int id, [FromForm] PermitDTO permit)
        {
            var updatedPermit = await _permitService.UpdatePermit(id, permit);
            if (updatedPermit == null)
            {
                return NotFound();
            }

            return Ok(updatedPermit);
        }

        // DELETE api/<PermitsController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _permitService.DeletePermit(id);
            return Ok();
        }
    }
}
