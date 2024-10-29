using LoginApi.Models;
using LoginApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
        //private readonly UserService _userService;
        //public UserAuthController(UserService userService)
        //{
        //    _userService = userService;
        //}
    public class UserAuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserAuthController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                await _userService.RegisterUser(userDto);
                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            // Call the service to authenticate the user and get the token
            var token = await _userService.LoginUser(userDto);

            if (token == null)
            {
                // Return 401 Unauthorized if authentication fails
                return Unauthorized("Invalid username or password");
            }
                // Return a 200 OK response with the JWT token if authentication is successful
                return Ok(new { Token = token });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsers();
            return Ok(result);
        }

        //[HttpDelete("deleteall")]
        //public async Task<IActionResult> DeleteAllUsers()
        //{
        //    await _userService.DeleteAll();

        //    return Ok("All users deleted successfully");
        //}

        //[HttpDelete("delete/{id}")]
        //public async Task<IActionResult> DeleteUser(int id)
        //{
        //    await _userService.DeleteUser(id);

        //    return Ok("User deleted successfully");
        //}

    }
}
