using GameServerAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GameServerAPI.Services
{
    public class GameServerService : IGameServerService
    {
        public GameServerInfo GetGameServer()
        {
            // Define the IP and port to be returned
            string ipAddress = "localhost";
            int port = 7216;

            // Start the Console App to run the game server
            //StartGameServer(ipAddress, port);

            // Return the server info
            return new GameServerInfo
            {
                IPAddress = ipAddress,
                Port = port
            };
        }
    }
}
