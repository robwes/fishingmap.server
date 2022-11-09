using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody]UserRegister user)
        {
            var newUser = await _userService.AddUser(user);
            return Created("success", newUser);
        }

        [HttpPost("RegisterAdmin")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserRegister admin)
        {
            var newAdmin = await _userService.AddAdministrator(admin);
            return Created("success", newAdmin);
        }
    }
}
