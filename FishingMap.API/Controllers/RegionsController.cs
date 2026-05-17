using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly IRegionsService _regionsService;

        public RegionsController(IRegionsService regionsService)
        {
            _regionsService = regionsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RegionDTO>>> Get()
        {
            try
            {
                var regions = await _regionsService.GetRegions();
                return Ok(regions);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RegionDTO>> Get(int id)
        {
            try
            {
                var region = await _regionsService.GetRegion(id);
                if (region == null)
                {
                    return NotFound();
                }

                return Ok(region);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RegionDTO>> Post([FromBody] RegionAdd region)
        {
            try
            {
                var added = await _regionsService.AddRegion(region);
                return CreatedAtAction(nameof(Get), new { id = added.Id }, added);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RegionDTO>> Put(int id, [FromBody] RegionUpdate region)
        {
            try
            {
                var updated = await _regionsService.UpdateRegion(id, region);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _regionsService.DeleteRegion(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
