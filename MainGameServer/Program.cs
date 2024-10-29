using MainGameServer;
using System.Net.Sockets;
using System.Net;
using System.Text;
using MainGameServer.Models;


GameServerInfo _gameServerInfo = new GameServerInfo();
MainServer _server = new MainServer();

try
{
    await ConnectClient();
    Consumer.StartChat();    
}
catch (Exception ex)
{
    Console.WriteLine($"You got that error when connect to Client {ex.Message}\n{ex.StackTrace}");
}

async Task ConnectClient()
{
    try
    {
        Console.WriteLine("Retrieving JWT token...");
        string? jwt = await ReceiveJWTFromClient();
        if (jwt == null)
        {
            throw new ArgumentNullException(nameof(jwt), "JWT token cannot be null");
        }
        Console.WriteLine("JWT token retrieved successfully: ", jwt);
        var serverInfo = await _server.GetGameServer(jwt);
        if (serverInfo == null)
        {
            Console.WriteLine("Failed to retrieve server info.");
            return;
        }
        var success = await _server.ConnectToClient(serverInfo.IPAddress, serverInfo.Port);
        if (success)
        {
            // Step 5: Start the game if connection is successful
           
            Console.WriteLine("Connected to Client!!!");
        }
        else
        {
            Console.WriteLine("Failed to connect to the Client.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"You got that exception when tried to connect the client: {ex.Message} \n{ex.StackTrace}");
    }
}

async Task<string?> ReceiveJWTFromClient()
{
    try
    {
        // Specify the IP address and port the server will listen on
        string serverIP = _gameServerInfo.IPAddress; // Replace with your server's actual IP address
        int serverPort = _gameServerInfo.Port;        // Replace with the actual port number

        // Create a TcpListener to listen for incoming connections
        var tcpListener = new TcpListener(IPAddress.Parse(serverIP), serverPort);

        // Start the listener
        tcpListener.Start();
        Console.WriteLine($"Main Game Server listening for connections on {serverIP}:{serverPort}...");

        // Accept incoming connection from a client
        TcpClient client = await tcpListener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected!");

        // Get the network stream to read data from the client
        NetworkStream stream = client.GetStream();

        // Read the length header (first 4 bytes)
        byte[] lengthBuffer = new byte[4];
        int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
        Console.WriteLine($"Read {bytesRead} bytes for JWT length header.");

        if (bytesRead == 4)
        {
            // Convert the length header to an integer
            int jwtLength = BitConverter.ToInt32(lengthBuffer, 0);
            Console.WriteLine($"JWT length header indicates {jwtLength} bytes.");

            // Prepare buffer to receive the JWT
            byte[] jwtBuffer = new byte[jwtLength];
            bytesRead = await stream.ReadAsync(jwtBuffer, 0, jwtBuffer.Length);
            Console.WriteLine($"Read {bytesRead} bytes for JWT.");

            if (bytesRead == jwtLength)
            {
                // Convert JWT byte array back to string
                string receivedJwt = Encoding.UTF8.GetString(jwtBuffer);
                Console.WriteLine("JWT received successfully.");

                // Send a response back to the client
                string responseMessage = "JWT received and processed successfully.";
                byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                Console.WriteLine("Response sent to client.");

                // Close the client connection
                client.Close();

                // Return the received JWT
                return receivedJwt;
            }
            else
            {
                Console.WriteLine("Error: JWT length does not match the expected size.");
            }
        }
        else
        {
            Console.WriteLine("Error: Failed to read JWT length header.");
        }

        // Close the client connection if something went wrong
        client.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while receiving the JWT: {ex.Message} \n{ex.StackTrace}");
    }

    // Return null if there was an error
    return null;
}





Console.ReadKey();
