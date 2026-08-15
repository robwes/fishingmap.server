using FishingMap.Domain.DTO.Regulations;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegulationsController : ControllerBase
    {
        private readonly IRegulationsService _regulationsService;

        public RegulationsController(IRegulationsService regulationsService)
        {
            _regulationsService = regulationsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SpeciesRegulationDTO>>> Get()
        {
            var regulations = await _regulationsService.GetRegulations();
            return Ok(regulations);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SpeciesRegulationDTO>> Get(int id)
        {
            var regulation = await _regulationsService.GetRegulation(id);
            if (regulation == null)
            {
                return NotFound();
            }

            return Ok(regulation);
        }

        [HttpGet("location/{locationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LocationSpeciesRuleDTO>>> GetForLocation(int locationId)
        {
            var rules = await _regulationsService.GetEffectiveRulesForLocation(locationId);
            return Ok(rules);
        }

        // Mirrors location/{locationId} rather than living under SpeciesController, so both
        // scope-shaped lookups sit next to each other on the resource they return.
        [HttpGet("species/{speciesId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SpeciesRegulationScopeDTO>>> GetForSpecies(int speciesId)
        {
            var regulations = await _regulationsService.GetRegulationsForSpecies(speciesId);
            return Ok(regulations);
        }

        // Whether a water inherits its region's rules for one species. A PUT rather than a
        // POST/DELETE pair because the resource is the decision itself, which is set to one
        // of two values; the body carries which. See decision 11 in robwes/fishingmap.web#13.
        [HttpPut("location/{locationId}/species/{speciesId}/follows-region")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PutFollowsRegion(int locationId, int speciesId, [FromBody] FollowsRegionUpdate input)
        {
            await _regulationsService.SetFollowsRegion(locationId, speciesId, input.Follows);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesRegulationDTO>> Post([FromBody] SpeciesRegulationAdd regulation)
        {
            var added = await _regulationsService.AddRegulation(regulation);
            return CreatedAtAction(nameof(Get), new { id = added.Id }, added);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesRegulationDTO>> Put(int id, [FromBody] SpeciesRegulationUpdate regulation)
        {
            var updated = await _regulationsService.UpdateRegulation(id, regulation);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _regulationsService.DeleteRegulation(id);
            return Ok();
        }
    }
}
