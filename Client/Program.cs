using Client;
using Client.Models;
using System.Net.Sockets;
using System.Text;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var gameClientManager = new GameClientManager();
        await gameClientManager.StartAsync();
    }
}
