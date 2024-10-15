using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace MainGameServer
{
    public class Consumer
    {
        public static void StartChat()
        {
            Console.WriteLine("Welcome to the private chat!");

            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "participant", durable: true, exclusive: true);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, EventArgs) =>
            {
                var body = EventArgs.Body.ToArray();

                //Get the message([])
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Received message: {message}");
            };

            channel.BasicConsume(queue: "participant", autoAck: true, consumer: consumer);
        }
}
}
