using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationsService _locationService;

        public LocationsController(ILocationsService service)
        {
            _locationService = service;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationSummary>>> Get([FromQuery] string search = "", [FromQuery] List<int>? sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var locations = await _locationService.GetLocations(search, sIds, radius, orgLat, orgLng);
            return Ok(locations);
        }

        [HttpGet("markers")]
        public async Task<ActionResult<IEnumerable<LocationMarker>>> Markers([FromQuery] string search = "", [FromQuery] List<int>? sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var markers = await _locationService.GetMarkers(search, sIds, radius, orgLat, orgLng);
            return Ok(markers);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<LocationSummary>>> LocationsSummary([FromQuery] string search = "", [FromQuery] List<int>? sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var locations = await _locationService.GetLocationsSummary(search, sIds, radius, orgLat, orgLng);
            return Ok(locations);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> Get(int id)
        {
            var location = await _locationService.GetLocation(id);
            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> Post([FromForm]LocationAdd location)
        {
            try
            {
                var loc = await _locationService.AddLocation(location);
                return Created("success", loc);
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

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> Put(int id, [FromForm]LocationUpdate location)
        {
            try
            {
                var loc = await _locationService.UpdateLocation(id, location);
                if (loc == null)
                {
                    return NotFound();
                }

                return Ok(loc);
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

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _locationService.DeleteLocation(id);
            return Ok();
        }
    }
}
