using FishingMap.API.Interfaces;
using FishingMap.Data.Entities;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FishingMap.Domain.DTO.Users;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [HttpPut("{id}/details")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateUserDetails(int id, [FromForm]UserDetailsUpdate userDetails)
        {
            var currentUser = await _authService.GetCurrentUser(HttpContext);
            if (currentUser == null || currentUser.Id != id)
            {
                return Unauthorized();
            }

            var updatedUser = await _userService.UpdateUserDetails(currentUser.Id, userDetails);
            
            return Ok(updatedUser);
        }

        [HttpPut("{id}/password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> updateUserPassword(int id, [FromForm]UserPasswordUpdate userPasswordUpdate)
        {
            var currentUser = await _authService.GetCurrentUser(HttpContext);
            if (currentUser == null || currentUser.Id != id) 
            {
                return Unauthorized();
            }

            var userCredentials = await _userService.GetUserCredentials(currentUser.Id);
            if (!_authService.ValidateUserPassword(userCredentials, userPasswordUpdate.CurrentPassword))
            {
                return Unauthorized();
            }

            var passwordUpdated = await _userService.UpdateUserPassword(currentUser.Id, userPasswordUpdate.NewPassword);
            if (!passwordUpdated)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("registerUser")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterUser([FromBody]UserAdd user)
        {
            var newUser = await _userService.AddUser(user);
            return Created("success", newUser);
        }

        [HttpPost("registerAdmin")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserAdd admin)
        {
            var newAdmin = await _userService.AddAdministrator(admin);
            return Created("success", newAdmin);
        }
    }
}
