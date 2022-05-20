using System.Text.Json;

namespace consumer.Defaults;

public abstract class GuidKeyMessageProcessor<TMessage> : DispatchedMessageProcessor<Guid, TMessage>
{
    public override Guid Convert(byte[] key)
    {
        return new Guid(key);
    }

    public override TMessage? Convert(string message)
    {
        return JsonSerializer.Deserialize<TMessage>(message);
    }
}