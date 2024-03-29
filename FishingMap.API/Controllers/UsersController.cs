﻿using FishingMap.API.Interfaces;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<UserDTO>> UpdateUserDetails(int id, [FromForm]UserDetailsUpdate userDetails)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUser(HttpContext);
                if (currentUser == null || currentUser.Id != id)
                {
                    return Unauthorized();
                }

                var updatedUser = await _userService.UpdateUserDetails(currentUser.Id, userDetails);
                return Ok(updatedUser);
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

        [HttpPut("{id}/password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> updateUserPassword(int id, [FromForm]UserPasswordUpdate userPasswordUpdate)
        {
            try {                 
                var currentUser = await _authService.GetCurrentUser(HttpContext);
                if (currentUser == null || currentUser.Id != id)
                {
                    return Unauthorized();
                }

                var userCredentials = await _userService.GetUserCredentials(currentUser.Id);
                if (userCredentials == null)
                {
                    return NotFound();
                }

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
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("registerUser")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDTO>> RegisterUser([FromBody]UserAdd user)
        {
            try
            {
                var newUser = await _userService.AddUser(user);
                return Created("", newUser);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("registerAdmin")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserAdd admin)
        {
            try
            {
                var newAdmin = await _userService.AddAdministrator(admin);
                return Created("", newAdmin);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
