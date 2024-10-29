using Client.Models;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    public class GameClient
    {
        private readonly HttpClient client;
        private const string baseUrl = "http://localhost:10001/api/auth/";

        public event Action<string>? OnLoginSuccess;
        public event Action<string>? OnRegistrationSuccess;
        public event Action? OnServerConnected;

        public GameClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            client = new HttpClient(handler);
        }

        public async Task RegisterAsync(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync($"{baseUrl}register", content);
                if (response.IsSuccessStatusCode)
                {
                    OnRegistrationSuccess?.Invoke(user.Name);
                }
                else
                {
                    Console.WriteLine($"Registration failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
            }
        }

        public async Task LoginAsync(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync($"{baseUrl}login", content);
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    OnLoginSuccess?.Invoke(token);
                }
                else
                {
                    Console.WriteLine($"Login failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
            }
        }

        public async Task<GameServerInfo> GetGameServerAsync(string jwtToken)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");

            try
            {
                var response = await client.GetAsync("http://localhost:10003/api/PlayerRegisterGame/server-info");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serverInfo = JsonConvert.DeserializeObject<GameServerInfo>(result);
                    if (serverInfo != null)
                    {
                        return serverInfo;
                    }
                    throw new Exception("Failed to deserialize game server info.");
                }
                throw new ServerConnectionException($"Failed to retrieve game server: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                throw new ServerConnectionException($"Error retrieving game server: {ex.Message}");
            }
        }

        public async Task ConnectToGameServerAsync(string ipAddress, int port)
        {
            try
            {
                using (var client = new TcpClient(ipAddress, port))
                {
                    Console.WriteLine($"Connecting to GameServer at {ipAddress}:{port}...");
                    OnServerConnected?.Invoke();
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes("Player was connected");
                    await stream.WriteAsync(data, 0, data.Length);
                    byte[] buffer = new byte[256];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Server response: {response}");
                }
            }
            catch (SocketException ex)
            {
                throw new ServerConnectionException($"Error connecting to game server: {ex.Message}");
            }
        }
    }

}
