using Client;
using Client.Models;
using System.Net.Sockets;
using System.Text;

var gameClient = new GameClient();
var user = new User();

Greeting();

bool isPlaying = true;
string jwtToken = null!;

await MainLoop();

async Task MainLoop()
{
    while (isPlaying)
    {
        var input = Console.ReadLine()?.ToLower();
        if (input == null) continue;

        switch (input)
        {
            case "login":
                await LoginAsync();

                if (!string.IsNullOrEmpty(jwtToken))
                {
                    await SendJwtToMainGameServer(jwtToken);
                }
                else
                {
                    Console.WriteLine("You need to register first.");
                }
                break;

            case "register":
                await RegisterAsync();
                break;

            case "connect":
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    await ConnectToGameServer();
                }
                else
                {
                    Console.WriteLine("You need to login first.");
                }
                break;

            case "exit":
                Console.WriteLine("Goodbye! Thanks for playing.");
                isPlaying = false;
                break;

            default:
                Console.WriteLine("Invalid command. Please enter 'login', 'register', 'connect', or 'exit'.");
                break;
        }
    }
}

async Task LoginAsync()
{
    Console.WriteLine("Enter your username:");
    user.Name = Console.ReadLine()!;

    Console.WriteLine("Enter your password:");
    user.Password = Console.ReadLine()!;

    if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Password))
    {
        try
        {
            jwtToken = await gameClient.Login(user);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("Login successful! You are now logged in.");
                await PromptForConnect();
            }
            else
            {
                Console.WriteLine("Login failed. Invalid username or password.");
            }
        }
        catch (LoginException ex)
        {
            Console.WriteLine($"{ex.Message} \n{ex.StackTrace}");
        }
    }
}

async Task RegisterAsync()
{
    Console.WriteLine("Choose a username:");
    user.Name = Console.ReadLine()!;

    Console.WriteLine("Choose a password:");
    user.Password = Console.ReadLine()!;

    if (user.Name != null && user.Password != null)
    {
        try
        {
            var registrationResponse = await gameClient.HandlePostRegister(user);
            Console.WriteLine("Registration successful!");

            // Automatically log the user in after successful registration
            jwtToken = await gameClient.Login(user);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("Login successful! You are now logged in.");
                await PromptForConnect();
            }
        }
        catch (RegistrationException ex)
        {
            Console.WriteLine($"{ex.Message} \n{ex.StackTrace}");
        }
        catch (LoginException ex)
        {
            Console.WriteLine("Login failed after registration. Please try logging in manually.");
            Console.WriteLine($"{ex.Message} \n{ex.StackTrace}");
        }
    }
}

async Task PromptForConnect()
{
    bool isValidInput = false;

    while (!isValidInput)
    {
        Console.WriteLine("Do you want to connect to the game server? (yes/no)");
        var connectInput = Console.ReadLine()?.ToLower();

        if (connectInput == "yes" && !string.IsNullOrEmpty(jwtToken))
        {
            await ConnectToGameServer();
            isValidInput = true; // Exit loop after handling the "yes" case
        }
        else if (connectInput == "no")
        {
            Console.WriteLine("You can use the 'connect' command later if you change your mind.");
            Console.WriteLine("You need to write 'exit' to leave the game server.");
            isValidInput = true; // Exit loop after handling the "no" case
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
        }
    }
}

async Task SendJwtToMainGameServer(string jwtToken)
{
    try
    {
        var serverInfo = await gameClient.GetGameServer(jwtToken);
        await SendDataToServer(serverInfo.IP, serverInfo.Port, jwtToken);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send JWT to GameServer: {ex.Message} \n{ex.StackTrace}");
    }
}

async Task SendDataToServer(string serverIP, int serverPort, string jwtToken)
{
    try
    {
        using (var tcpClient = new TcpClient(serverIP, serverPort))
        {
            Console.WriteLine($"Connecting to GameServer at {serverIP}:{serverPort}...");

            NetworkStream stream = tcpClient.GetStream();
            var jwtBytes = Encoding.UTF8.GetBytes(jwtToken);
            var message = new byte[jwtBytes.Length + 4];

            BitConverter.GetBytes(jwtBytes.Length).CopyTo(message, 0);
            jwtBytes.CopyTo(message, 4);

            await stream.WriteAsync(message, 0, message.Length);
            Console.WriteLine("JWT sent to the GameServer. Waiting for server response...");

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Received response from server: {serverResponse}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending data to server: {ex.Message}");
    }
}

async Task ConnectToGameServer()
{
    try
    {
        var serverInfo = await gameClient.GetGameServer(jwtToken);
        var success = await GameClient.ConnectToGameServer(serverInfo.IP, serverInfo.Port);
        if (success)
        {
            var game = new Game1(serverInfo.IP, serverInfo.Port);
            game.StartGame();
            Console.WriteLine("Connected to GameServer! You can now play the game.");
        }
        else
        {
            Console.WriteLine("Failed to connect to the GameServer.");
        }
    }
    catch (ServerConnectionException ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine("You can write 'exit' to leave the game server.");
    }
}

void Greeting()
{
    Console.WriteLine("Welcome! You need to register or login to access the game.");
    Console.WriteLine("Available commands: 'login', 'register', 'connect', 'exit'");
}

Console.ReadKey();
