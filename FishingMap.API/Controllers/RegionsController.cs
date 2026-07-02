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
            var regions = await _regionsService.GetRegions();
            return Ok(regions);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RegionDTO>> Get(int id)
        {
            var region = await _regionsService.GetRegion(id);
            if (region == null)
            {
                return NotFound();
            }

            return Ok(region);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RegionDTO>> Post([FromBody] RegionAdd region)
        {
            var added = await _regionsService.AddRegion(region);
            return CreatedAtAction(nameof(Get), new { id = added.Id }, added);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RegionDTO>> Put(int id, [FromBody] RegionUpdate region)
        {
            var updated = await _regionsService.UpdateRegion(id, region);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _regionsService.DeleteRegion(id);
            return Ok();
        }
    }
}
