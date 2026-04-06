using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Services;
using System.Threading.Tasks;

namespace SilesiaRental.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase {
        // Connect our service
        private readonly IUserService _userService;

        //Options from Program.cs
        public UsersController(IUserService userService) {
            _userService = userService;
        }

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            var result = await _userService.GetProfileInfo(username);

            if (!result.IsSuccess) return BadRequest(result.Message);

            return Ok(result.Payload);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(UserDTO userDTO) {
            var register = await _userService.RegisterUserAsync(userDTO);
            if (!register.IsSuccess) return BadRequest(register.Message);
            return Ok("User was registered");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser(UserDTO userDTO) {
            var result = await _userService.LoginAsync(userDTO);
            if(!result.IsSuccess) return BadRequest(result.Message);
            // here Payload - Token
            return Ok(new { token = result.Payload, message = "Login success" });
        }
    }
}
