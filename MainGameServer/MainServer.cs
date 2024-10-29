using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using System.Timers;
using System.Net.Http;
using MainGameServer.Models;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace MainGameServer
{
    internal class MainServer
    {
        private readonly HttpClient httpClient;
        private System.Timers.Timer timer;
        private TcpListener tcpListener;
        private List<TCPGameClient> clients = new List<TCPGameClient>();
        private GameWorld world;
        private float snapshotSpeed = 3;
        private CommandExecutor commandExecutor = new CommandExecutor();
        
        public MainServer()
        {
            // Create a custom HttpClientHandler to handle proxy and certificate issues
            var httpClientHandler = new HttpClientHandler
            {
                UseProxy = false,  // Bypass proxy for localhost
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator  // Accept self-signed certificates
            };

            // Pass the custom handler to HttpClient
            httpClient = new HttpClient(httpClientHandler);

            commandExecutor.RegisterCommand("tick", SetTickRate);
            timer = new System.Timers.Timer();
            timer.Interval = 1000f / snapshotSpeed;
            timer.Elapsed += TimerElapsed;
            tcpListener = new TcpListener(IPAddress.Any, 7216);
            tcpListener.Start();
            Console.WriteLine("Main server started listening on TCP port 7216");
            Console.WriteLine($"Setting tick rate to {snapshotSpeed}");
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
            Start();
            world = new GameWorld();
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();
           }

        private void ReadInput()
        {
            while (true)
            {
                var input = Console.ReadLine();
                commandExecutor.ExecuteCommand(input);
            }
        }

        private void SetTickRate(string[] args)
        {
            if (args.Length == 1 && int.TryParse(args[0], out int tickRate))
            {
                Console.WriteLine($"Setting tick rate to {tickRate}");
                snapshotSpeed = tickRate;
                timer.Interval = 1000f / snapshotSpeed;
            }
            else
            {
                Console.WriteLine("Invalid arguments for tick");
            }
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void AcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    TCPGameClient client = new TCPGameClient(OnDataReceived, tcpClient);
                    clients.Add(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private void OnDataReceived(byte[] receivedData, TCPGameClient client)
        {
            try
            {
                MessageType messageType = (MessageType)receivedData[0];
                byte[] dataToDeserialize = receivedData.Skip(1).ToArray();
                switch (messageType)
                {
                    case MessageType.MovementUpdate:
                        MovementUpdate mov = MessagePackSerializer.Deserialize<MovementUpdate>(dataToDeserialize);
                        world.UpdateBallMovement(mov);
                        break;
                    case MessageType.Join:
                        // The first joiner is the owner of the ball.
                        SendDataToClient(new JoinAnswer { BallOwner = (clients.Count == 1) }, client);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing received data: {ex.Message}");
                throw;
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (clients.Count == 0)
            {
                return;
            }
            foreach (var client in clients)
            {
                SendDataToClient(world.GetWorldStateSnapShot(), client);
            }
        }

        public void SendDataToClient(NetworkMessage message, TCPGameClient client)
        {
            byte[] messageBytes = new byte[1024];
            byte messageTypeByte = message.GetMessageTypeAsByte;
            switch (message.MessageType)
            {
                case MessageType.SnapShot:
                    messageBytes = MessagePackSerializer.Serialize((SnapShot)message);
                    break;
                case MessageType.JoinAnswer:
                    messageBytes = MessagePackSerializer.Serialize((JoinAnswer)message);
                    break;
                default:
                    break;
            }
            byte[] combinedBytes = new byte[1 + messageBytes.Length];
            combinedBytes[0] = messageTypeByte;
            Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);
            client.SendDataToServer(combinedBytes);
        }

        public async Task<bool> ConnectToClient(string serverIP, int serverPort)
        {
            try
            {
                // Create a TcpClient to connect to the game server
                TcpClient tcpClient = new TcpClient();

                // Connect to the server's IP and port
                await tcpClient.ConnectAsync(IPAddress.Parse(serverIP), serverPort);

                Console.WriteLine($"Connected to server at {serverIP}:{serverPort}");

                // Initialize any additional logic here (like sending initial data, etc.)

                return true; // Return success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                return false; // Return failure
            }
        }


        public async Task<GameServerInfo> GetGameServer(string jwt)
        {
            // Ensure headers are cleared before setting new ones
            httpClient.DefaultRequestHeaders.Clear();

            // Log the token for debugging purposes (be careful with logging sensitive data)
            Console.WriteLine($"JWT Token used: {jwt}");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
            
            // Log the request being sent (optional)
            Console.WriteLine("Sending request to retrieve game server info...");

            // Make the GET request to the server-info endpoint
            var response = await httpClient.GetAsync("http://localhost:10003/api/PlayerRegisterGame/server-info");

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("RESPONSE was Success!!!!!!!!!!!!");
                // Read the response content as a string
                var result = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response to a GameServerInfo object
                var serverInfo = JsonConvert.DeserializeObject<GameServerInfo>(result);

                // Return the deserialized object, ensuring it's not null
                if (serverInfo != null)
                {
                    return serverInfo;
                }
                else
                {
                    throw new Exception("Failed to deserialize GameServer info");
                }
            }
            else
            {
                // Log and throw an error if the status code is not successful
                Console.WriteLine($"Failed to retrieve GameServer info. Status Code: {response.StatusCode}");
                throw new Exception($"Error retrieving GameServer info: {response.StatusCode}");
            }
        }

    }
}
