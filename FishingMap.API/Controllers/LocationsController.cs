using FishingMap.Domain.Data.DTO.LocationObjects;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<LocationSummary>> Get([FromQuery] string search = "", [FromQuery] List<int> sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var locations = await _locationService.GetLocations(search, sIds, radius, orgLat, orgLng);
            return locations;
        }

        [HttpGet("markers")]
        public async Task<IEnumerable<LocationMarker>> Markers([FromQuery] string search = "", [FromQuery] List<int> sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var markers = await _locationService.GetMarkers(search, sIds, radius, orgLat, orgLng);
            return markers;
        }

        [HttpGet("summary")]
        public async Task<IEnumerable<LocationSummary>> LocationsSummary([FromQuery] string search = "", [FromQuery] List<int> sIds = null, [FromQuery] double? radius = null, [FromQuery] double? orgLat = null, [FromQuery] double? orgLng = null)
        {
            var locations = await _locationService.GetLocationsSummary(search, sIds, radius, orgLat, orgLng);
            return locations;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<LocationDTO> Get(int id)
        {
            var location = await _locationService.GetLocation(id);
            return location;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<LocationDTO> Post([FromForm]LocationAdd location)
        {
            var loc = await _locationService.AddLocation(location);
            return loc;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<LocationDTO> Put(int id, [FromForm]LocationUpdate location)
        {
            var loc = await _locationService.UpdateLocation(id, location);
            return loc;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task Delete(int id)
        {
            await _locationService.DeleteLocation(id);
        }
    }
}
