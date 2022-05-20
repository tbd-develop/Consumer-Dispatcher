using System.Text;
using Confluent.Kafka;
using consumer.Configuration;
using Microsoft.Extensions.Hosting;

namespace consumer;

public class ConsumerService : IHostedService
{
    private readonly KafkaConfiguration _kafka;
    private readonly Dispatcher _dispatcher;

    public ConsumerService(KafkaConfiguration kafka, Dispatcher dispatcher)
    {
        _kafka = kafka;
        _dispatcher = dispatcher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<byte[], string>(_kafka.Consumer)
                .SetKeyDeserializer(new KeyDeserializer())
                .SetValueDeserializer(new ValueDeserializer())
                .Build();

            consumer.Subscribe("customers.leads");

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = consumer.Consume(cancellationToken);

                if (result is null) continue;

                var hasCompleted = await _dispatcher.Dispatch(new DispatchMessage
                {
                    Topic = result.Topic,
                    Key = result.Message.Key,
                    Message = result.Message.Value
                });

                if (hasCompleted)
                {
                    consumer.Commit(result);
                }
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class KeyDeserializer : IDeserializer<byte[]>
{
    public byte[] Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return data.ToArray();
    }
}

public class ValueDeserializer : IDeserializer<string>
{
    public string Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return Encoding.UTF8.GetString(data);
    }
}