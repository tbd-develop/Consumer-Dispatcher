using consumer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace consumer;

public class DispatchMessage
{
    public string Topic { get; set; } = null!;
    public byte[] Key { get; set; } = null!;
    public string? Message { get; set; }
}

public class Dispatcher
{
    private readonly DispatchConfiguration _configuration;
    private readonly IServiceProvider _provider;

    public Dispatcher(DispatchConfiguration configuration, IServiceProvider provider)
    {
        _configuration = configuration;
        _provider = provider;
    }

    public async Task<bool> Dispatch(DispatchMessage dispatch)
    {
        if (_configuration.Values.ContainsKey(dispatch.Topic))
        {
            using var scope = _provider.CreateScope();

            var processor =
                scope.ServiceProvider.GetRequiredService(_configuration.Values[dispatch.Topic]) as IMessageProcessor;

            await processor.Process(dispatch.Key, dispatch.Message);

            return true;
        }

        return false;
    }
}