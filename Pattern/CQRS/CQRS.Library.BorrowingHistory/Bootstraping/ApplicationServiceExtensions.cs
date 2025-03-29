using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;
using CQRS.Library.IntegrationEvents;

using EventBus;
using EventBus.Kafka;

using Pattern.Shared;

namespace CQRS.Library.BorrowingHistoryService.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingHistoryDbContext>(Consts.DefaultDatabase);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrEmpty(eventConsumingTopics))
        {
            builder.AddKafkaEventConsumer(options =>
            {
                options.ServiceName = "BorrowingHistoryService";
                options.KafkaGroupId = "cqrs";
                options.Topics.AddRange(eventConsumingTopics.Split(","));
                options.IntegrationEventFactory = IntegrationEventFactory<BookCreatedIntegrationEvent>.Instance;
            });
        }

        return builder;
    }
}
