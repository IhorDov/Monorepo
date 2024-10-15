namespace ChatApi.Services
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message);
    }
}
