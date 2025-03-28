using CQRS.Library.BookService.Infrastructure.Data;
using EventBus;
using EventBus.Abstractions;
using EventBus.Kafka;
using Pattern.Shared;

namespace CQRS.Library.BookService.Bootstraping;

public static class ApplicationServiceExtension
{
    public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BookDbContext>(Consts.DefaultDatabase);

        builder.AddKafkaProducerExtension("kafka");
        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if(!string.IsNullOrEmpty(kafkaTopic))
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
