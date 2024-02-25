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
            try
            {
                var permits = await _permitService.GetPermits(search);
                return Ok(permits);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // GET api/<PermitsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermitDTO>> Get(int id)
        {
            try
            {
                var permit = await _permitService.GetPermit(id);
                if (permit == null)
                {
                    return NotFound();
                }

                return Ok(permit);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST api/<PermitsController>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PermitDTO>> Post([FromForm] PermitDTO permit)
        {
            try
            {
                var newPermit = await _permitService.AddPermit(permit);
                return CreatedAtAction(nameof(Get), new { id = newPermit.Id }, newPermit);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // PUT api/<PermitsController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PermitDTO>> Put(int id, [FromForm] PermitDTO permit)
        {
            try
            {
                var updatedPermit = await _permitService.UpdatePermit(id, permit);
                return Ok(updatedPermit);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // DELETE api/<PermitsController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _permitService.DeletePermit(id);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
