using CQRS.Library.BorrowerService.Infrastructure.Data;
using EventBus;
using EventBus.Abstractions;
using EventBus.Kafka;
using Pattern.Shared;

namespace CQRS.Library.BorrowerService.Bootstraping;

public static class ApplicationServiceExtension
{
    public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowerDbContext>(Consts.DefaultDatabase);

        builder.AddKafkaProducerExtension("kafka");
        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrEmpty(kafkaTopic))
        {
            builder.AddKafkaEventPublisher(kafkaTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        return builder;
    }
}
