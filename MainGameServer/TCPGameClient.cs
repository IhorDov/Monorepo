using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MainGameServer
{
    public class TCPGameClient
    {
        private readonly TcpClient tcpClient;
        private readonly NetworkStream networkStream;
        private readonly Action<byte[], TCPGameClient> onDataReceived;

        public TCPGameClient(Action<byte[], TCPGameClient> onDataReceived, TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            this.networkStream = tcpClient.GetStream();
            this.onDataReceived = onDataReceived;
            Task.Run(() => ReceiveDataFromServer());
        }

        public async void SendDataToServer(byte[] data)
        {
            try
            {
                await networkStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to server: {ex.Message}");
            }
        }

        private async void ReceiveDataFromServer()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, receivedData, bytesRead);
                        onDataReceived(receivedData, this);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data from server: {ex.Message}");
                    break;
                }
            }
        }
    }
}
