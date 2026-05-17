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
            try
            {
                var regulations = await _regulationsService.GetRegulations();
                return Ok(regulations);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SpeciesRegulationDTO>> Get(int id)
        {
            try
            {
                var regulation = await _regulationsService.GetRegulation(id);
                if (regulation == null)
                {
                    return NotFound();
                }

                return Ok(regulation);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("location/{locationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LocationSpeciesRuleDTO>>> GetForLocation(int locationId)
        {
            try
            {
                var rules = await _regulationsService.GetEffectiveRulesForLocation(locationId);
                return Ok(rules);
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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SpeciesRegulationDTO>> Post([FromBody] SpeciesRegulationAdd regulation)
        {
            try
            {
                var added = await _regulationsService.AddRegulation(regulation);
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
        public async Task<ActionResult<SpeciesRegulationDTO>> Put(int id, [FromBody] SpeciesRegulationUpdate regulation)
        {
            try
            {
                var updated = await _regulationsService.UpdateRegulation(id, regulation);
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
                await _regulationsService.DeleteRegulation(id);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
