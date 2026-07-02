using FishingMap.Domain.DTO.Images;
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

        [HttpGet("features")]
        public async Task<ActionResult<string>> Features([FromQuery] string search = "", [FromQuery] List<int>? sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var featureCollection = await _locationService.GetFeatures(search, sIds, radius, orgLat, orgLng);
            return Ok(featureCollection);
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
            var loc = await _locationService.AddLocation(location);
            return CreatedAtAction(nameof(Get), new { id = loc.Id }, loc);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> Put(int id, [FromForm]LocationUpdate location)
        {
            var loc = await _locationService.UpdateLocation(id, location);
            return Ok(loc);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _locationService.DeleteLocation(id);
            return Ok();
        }

        [HttpPatch("{id}/info")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> PatchInfo(int id, [FromBody] LocationInfoPatch patch)
        {
            var loc = await _locationService.UpdateLocationInfo(id, patch);
            return Ok(loc);
        }

        [HttpPatch("{id}/associations")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> PatchAssociations(int id, [FromBody] LocationAssociationsPatch patch)
        {
            var loc = await _locationService.UpdateLocationAssociations(id, patch);
            return Ok(loc);
        }

        [HttpPost("{id}/images")]
        [Authorize(Roles = "Administrator")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImageDTO>> PostImage(int id, [FromForm] IFormFile image)
        {
            var imageDto = await _locationService.AddImageToLocation(id, image);
            return CreatedAtAction(nameof(Get), new { id }, imageDto);
        }

        [HttpDelete("{id}/images/{imageId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            await _locationService.RemoveImageFromLocation(id, imageId);
            return NoContent();
        }

        [HttpPatch("{id}/geometry")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<LocationDTO>> PatchGeometry(int id, [FromBody] LocationGeometryPatch patch)
        {
            var loc = await _locationService.UpdateLocationGeometry(id, patch);
            return Ok(loc);
        }
    }
}
