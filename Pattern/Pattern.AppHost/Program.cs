using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImageTag("latest")
    .WithVolume("book-data", "/var/lib/postgresql/data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(builder =>
    {
        builder.WithImageTag("latest");
    });
var cache = builder.AddRedis("redis").WithLifetime(ContainerLifetime.Persistent).WithRedisInsight();
var kafka = builder.AddKafka("kafka").WithLifetime(ContainerLifetime.Persistent).WithKafkaUI();
var mongoDb = builder.AddMongoDB("mongodb").WithLifetime(ContainerLifetime.Persistent).WithMongoExpress().WithDataVolume(); // here we use MongoDB for both read/write model, but we can use different databases using replicas

builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
{
    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Creating topics...");

    var connectionString = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);
    using var adminClient = new AdminClientBuilder(new AdminClientConfig()
    {
        BootstrapServers = connectionString
    }).Build();

    try
    {
        await adminClient.CreateTopicsAsync(
            [
            new() { Name = "book", NumPartitions = 1, ReplicationFactor = 1 }, 
            new() { Name = "borrower", NumPartitions = 1, ReplicationFactor = 1 }, 
            new() { Name = "borrowing", NumPartitions = 1, ReplicationFactor = 1 }]);
    }
    catch (CreateTopicsException)
    {
        logger.LogError("An error occurred creating topics");
        throw;
    }
});

var bookDb = postgres.AddDatabase("cqrs-book-db");
builder.AddProject<Projects.Pattern_Library_BookApi>("cqrs-bookapi").WithReference(bookDb).WaitFor(bookDb);

builder.Build().Run();
