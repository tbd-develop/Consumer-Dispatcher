namespace consumer.Infrastructure;

public interface IMessageProcessor
{
    Task Process(byte[] key, string message);
}