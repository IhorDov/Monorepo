using GameServerAPI.Models;
using GameServerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameServerAPI.Controllers
{
    //[Authorize(Roles = "Admin")]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerRegisterGame : ControllerBase
    {
        private readonly IGameServerService _gameServerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor injection for GameServerService
        public PlayerRegisterGame(IGameServerService gameServerService, IHttpContextAccessor httpContextAccessor)
        {
            _gameServerService = gameServerService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Endpoint to return IP and Port
        [HttpGet("server-info")]
        public ActionResult<GameServerInfo> GetServerInfo()
        {
            // Extract the token from the Authorization header
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();
            Console.WriteLine($"token from PlayerRegisterGame controller: {token}");

            // Verify the token and check the role
            //var result = _gameServerService.VerifyJWT(token).Result;
            //if (result == "Failed")
            //{
            //    return Unauthorized("Invalid token");
            //}

            // Use the GameServerService to get IP and Port
            var serverInfo = _gameServerService.GetGameServer();

            return Ok(serverInfo); // Return the server info
        }
    }
}

