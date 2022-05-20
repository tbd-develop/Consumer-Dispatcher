using consumer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace consumer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDispatchedConsumer(this IServiceCollection services, Action<DispatchConfigurationBuilder> configure)
    {
        services.AddSingleton<Dispatcher>();

        services.AddSingleton(provider =>
        {
            KafkaConfiguration configuration = new KafkaConfiguration();

            provider.GetRequiredService<IConfiguration>().Bind("kafka", configuration);

            return configuration;
        });

        services.AddSingleton(ctx =>
        {
            var builder = new DispatchConfigurationBuilder();

            configure(builder);

            return builder.Build();
        });

        services.AddHostedService<ConsumerService>();

        return services;
    }
}

public class DispatchConfiguration
{
    public Dictionary<string, Type> Values { get; }

    public DispatchConfiguration(Dictionary<string, Type> values)
    {
        Values = values;
    }
}

public class DispatchConfigurationBuilder
{
    private readonly List<DispatchTopicConfiguration> _configurations;

    public DispatchConfigurationBuilder()
    {
        _configurations = new List<DispatchTopicConfiguration>();
    }

    public DispatchTopicConfiguration For(string topicName)
    {
        var result = new DispatchTopicConfiguration(this, topicName);

        _configurations.Add(result);

        return result;
    }

    public DispatchConfiguration Build()
    {
        return new DispatchConfiguration(_configurations.ToDictionary(k => k.TopicName, k => k.ProcessorType));
    }
}

public class DispatchTopicConfiguration
{
    private readonly DispatchConfigurationBuilder _parent;
    private readonly string _topicName = null!;
    private Type _processorType = null!;

    public Type ProcessorType => _processorType;
    public string TopicName => _topicName;

    public DispatchTopicConfiguration(DispatchConfigurationBuilder parent, string topicName)
    {
        _parent = parent;
        _topicName = topicName;
    }

    public DispatchConfigurationBuilder Use<TProcessor>()
    {
        _processorType = typeof(TProcessor);

        return _parent;
    }
}