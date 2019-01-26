using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
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
        public async Task<IEnumerable<Location>> Get([FromQuery] string search = "", [FromQuery] List<int> sIds = null, [FromQuery] double? inRange = null, [FromQuery] GeoPoint fromPos = null)
        {
            var locations = await _locationService.GetLocations(search, sIds, inRange, fromPos);
            return locations;
        }

        [HttpGet("markers")]
        public async Task<IEnumerable<LocationMarker>> Markers([FromQuery] string search = "")
        {
            var markers = await _locationService.GetMarkers(search);
            return markers;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<Location> Get(int id)
        {
            var location = await _locationService.GetLocation(id);
            return location;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<Location> Post([FromBody]Location location)
        {
            var loc = await _locationService.AddLocation(location);
            return loc;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<Location> Put(int id, [FromBody]Location location)
        {
            var loc = await _locationService.UpdateLocation(location);
            return loc;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _locationService.DeleteLocation(id);
        }
    }
}
