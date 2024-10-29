

using Client.Models;

namespace Client
{
    public class GameClientManager
    {
        private readonly GameClient gameClient = new GameClient();
        private readonly User user = new User();
        private string jwtToken = string.Empty;
        private bool isPlaying = true;

        public GameClientManager()
        {
            gameClient.OnLoginSuccess += HandleLoginSuccess;
            gameClient.OnRegistrationSuccess += HandleRegistrationSuccess;
            gameClient.OnServerConnected += HandleServerConnected;
        }

        public async Task StartAsync()
        {
            Greeting();
            while (isPlaying)
            {
                string? input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input)) continue;

                switch (input)
                {
                    case "login":
                        await LoginAsync();
                        break;
                    case "register":
                        await RegisterAsync();
                        break;
                    case "connect":
                        await ConnectToServerAsync();
                        break;
                    case "exit":
                        ExitGame();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Available commands: 'login', 'register', 'connect', 'exit'.");
                        break;
                }
            }
        }

        private void Greeting()
        {
            Console.WriteLine("Welcome! You need to register or login to access the game.");
            Console.WriteLine("Available commands: 'login', 'register', 'connect', 'exit'.");
        }

        private async Task LoginAsync()
        {
            Console.WriteLine("Enter username:");
            user.Name = Console.ReadLine()!;

            Console.WriteLine("Enter password:");
            user.Password = Console.ReadLine()!;

            await gameClient.LoginAsync(user);
        }

        private async Task RegisterAsync()
        {
            Console.WriteLine("Choose a username:");
            user.Name = Console.ReadLine()!;

            Console.WriteLine("Choose a password:");
            user.Password = Console.ReadLine()!;

            await gameClient.RegisterAsync(user);
        }

        private async Task ConnectToServerAsync()
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("You need to login first.");
                return;
            }

            var serverInfo = await gameClient.GetGameServerAsync(jwtToken);
            await gameClient.ConnectToGameServerAsync(serverInfo.IP, serverInfo.Port);
        }

        private void HandleLoginSuccess(string token)
        {
            jwtToken = token;
            Console.WriteLine("Login successful! You are now logged in.");
        }

        private void HandleRegistrationSuccess(string username)
        {
            Console.WriteLine($"Registration successful! Welcome, {username}. You are now logged in.");
        }

        private void HandleServerConnected()
        {
            Console.WriteLine("Connected to the game server! You can now play the game.");
        }

        private void ExitGame()
        {
            Console.WriteLine("Goodbye! Thanks for playing.");
            isPlaying = false;
        }
    }

}
