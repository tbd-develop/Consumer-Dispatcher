using System.Text.Json;
using consumer.Defaults;
using consumer.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

new HostBuilder()
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<CustomerLeadsProcessor>();

        services.AddDispatchedConsumer(builder =>
        {
            builder.For("customers.leads").Use<CustomerLeadsProcessor>();
        });
    })
    .Build()
    .Run();

public class CustomerLeadsProcessor : GuidKeyMessageProcessor<Lead>
{
    public override Task Handle(Guid key, Lead? value)
    {
        Console.WriteLine($"{key} created for {value.FirstName} {value.LastName}");

        return Task.CompletedTask;
    }
}

public class Lead
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsInterested { get; set; }
}