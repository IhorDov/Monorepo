using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ChatApi.Services
{
    public class MessageProducer : IMessageProducer
    {
        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
                //HostName = "localhost",
                //UserName = "user",
                //Password = "user",
                //VirtualHost = "/"
            };

            var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "participant", durable: true, exclusive: true);

            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            channel.BasicPublish("", "participant", body: body);
        }

    }
}
