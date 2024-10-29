using MessagePack;
using System.Net.Sockets;


namespace Client
{
    public class TCPGameClient
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Action<byte[]> OnDataReceived;
        private string _ip;
        private int _port;

        public TCPGameClient(Action<byte[]> onDataReceived, string ip, int port)
        {
            _ip = ip;
            _port = port;
            tcpClient = new TcpClient(_ip, _port);
            networkStream = tcpClient.GetStream();
            this.OnDataReceived = onDataReceived;
            Thread receiveThread = new Thread(() => ReceiveDataFromServer());
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        public async void SendDataToServer(NetworkMessage message)
        {
            await Task.Delay(Game1.LATENCY);
            byte[] messageBytes = new byte[1024];
            byte messageTypeByte = message.GetMessageTypeAsByte;
            switch (message.MessageType)
            {
                case MessageType.MovementUpdate:
                    messageBytes = MessagePackSerializer.Serialize((MovementUpdate)message);
                    break;
                default:
                    break;
            }
            byte[] combinedBytes = new byte[1 + messageBytes.Length];
            combinedBytes[0] = messageTypeByte;
            Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);
            await networkStream.WriteAsync(combinedBytes, 0, combinedBytes.Length);
        }

        private void ReceiveDataFromServer()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] serverResponse = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, serverResponse, 0, bytesRead);
                    OnDataReceived.Invoke(serverResponse);
                }
            }
        }
    }
}
