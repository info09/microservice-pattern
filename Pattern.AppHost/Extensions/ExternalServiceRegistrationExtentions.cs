using Confluent.Kafka;
using Confluent.Kafka.Admin;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Pattern.Shared;

using Projects;

namespace Pattern.AppHost.Extensions;

public static class ExternalServiceRegistrationExtentions
{
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis");
        var kafka = builder.AddKafka("kafka");
        var mongoDb = builder.AddMongoDB("mongodb");
        var postgres = builder.AddPostgres("postgresql");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            cache = cache.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithRedisInsight();
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
            mongoDb = mongoDb.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithMongoExpress();
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithPgWeb();
        }

        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });

        #region CQRS
        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        var bookService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BookService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BookService>())
            .WithReference(kafka)
            .WithReference(bookDb, Consts.DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowerDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowerService>();
        var borrowerService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowerService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, Consts.DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        var borrowingService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, Consts.DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        var borrowingHistoryService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingHistoryService>()
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                                string.Join(',', GetTopicName<CQRS_Library_BookService>(), GetTopicName<CQRS_Library_BorrowerService>(), GetTopicName<CQRS_Library_BorrowingService>()))
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb, Consts.DefaultDatabase)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka);

        bookService.WithParentRelationship(borrowingHistoryService);
        borrowerService.WithParentRelationship(borrowingHistoryService);
        borrowingService.WithParentRelationship(borrowingHistoryService);
        #endregion

        #region EventSourcing
        var esAccountDb = postgres.AddDefaultDatabase<Projects.EventSourcing_Banking_AccountService>();
        var esAccountService = builder.AddProjectWithPostfix<Projects.EventSourcing_Banking_AccountService>()
            .WithReference(esAccountDb, Consts.DefaultDatabase)
            .WaitFor(esAccountDb);
        #endregion
        return builder;
    }

    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();
        TopicSpecification[] topics = [
            new() { Name = GetTopicName<Projects.CQRS_Library_BookService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowerService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowingService>(), NumPartitions = 1, ReplicationFactor = 1 },
            //new() { Name = GetTopicName<Projects.Saga_OnlineStore_CatalogService>(), NumPartitions = 1, ReplicationFactor = 1 },
            //new() { Name = GetTopicName<Projects.Saga_OnlineStore_PaymentService>(), NumPartitions = 1, ReplicationFactor = 1 },
            //new() { Name = GetTopicName<Projects.Saga_OnlineStore_InventoryService>(), NumPartitions = 1, ReplicationFactor = 1 },
            //new() { Name = GetTopicName<Projects.Saga_OnlineStore_OrderService>(), NumPartitions = 1, ReplicationFactor = 1 }
        ];

        logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));
        var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = connectionString
        }).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions { });
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "An error occurred creating topics: {topics}", string.Join(", ", ex.Results.Select(r => r.Topic).ToArray()));
        }
    }

    private static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";
}
