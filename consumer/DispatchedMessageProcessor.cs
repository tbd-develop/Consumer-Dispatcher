using consumer.Infrastructure;
using System.Threading.Tasks.Dataflow;

namespace consumer;

public abstract class DispatchedMessageProcessor<TKey, TValue> : IMessageProcessor
{
    private readonly ActionBlock<(TKey, TValue?)> _action;

    protected DispatchedMessageProcessor()
    {
        _action = new ActionBlock<(TKey, TValue?)>(async (value) =>
        {
            await Handle(value.Item1, value.Item2);
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 8
        });
    }

    Task IMessageProcessor.Process(byte[] key, string message)
    {
        return Process(
            Convert(key),
            Convert(message));
    }

    Task<bool> Process(TKey key, TValue? value)
    {
        return Task.FromResult(_action.Post((key, value)));
    }

    public abstract Task Handle(TKey key, TValue? value);
    public abstract TKey Convert(byte[] key);
    public abstract TValue? Convert(string message);
}