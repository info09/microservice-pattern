using Confluent.Kafka;
using EventBus.Abstractions;
using EventBus.Event;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventBus.Kafka;

public class EventHandlingService : BackgroundService
{
    private readonly IConsumer<string, MessageEnvelop> consumer;
    private readonly EventHandlingWorkerOptions options;
    private readonly IIntegrationEventFactory integrationEventFactory;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger logger;

    public EventHandlingService(IConsumer<string, MessageEnvelop> consumer, EventHandlingWorkerOptions options, IIntegrationEventFactory integrationEventFactory, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        this.consumer = consumer;
        this.options = options;
        this.integrationEventFactory = integrationEventFactory;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = loggerFactory.CreateLogger(options.ServiceName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topics [{topics}]...", string.Join(',', options.Topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer!.Subscribe(options.Topics);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumerResult = consumer!.Consume(100);
                    if (consumer != null)
                    {
                        using IServiceScope scope = serviceScopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await ProcessMessageAsync(mediator, consumerResult.Message.Value, stoppingToken);
                    }
                    else
                    {
                        await Task.Delay(100, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error comsuming message");
            }
        }
    }

    private async Task ProcessMessageAsync(IMediator mediator, MessageEnvelop message, CancellationToken stoppingToken)
    {
        var integrationEvent = this.integrationEventFactory.CreateEvent(message.MessageTypeName, message.Message);
        if (integrationEvent is not null)
        {
            if (options.AcceptEvent(integrationEvent))
            {
                logger.LogInformation("Processing message {t}: {message}", message.MessageTypeName, message.Message);
                await mediator.Send(integrationEvent, stoppingToken);
            }
        }
        else
        {
            logger.LogWarning("Event type not found: {t}", message.MessageTypeName);
        }
    }
}

public class EventHandlingWorkerOptions
{
    public string KafkaGroupId { get; set; } = "event-handling";
    public List<string> Topics { get; set; } = [];
    public IIntegrationEventFactory IntegrationEventFactory { get; set; } = EventBus.IntegrationEventFactory.Instance;
    public string ServiceName { get; set; } = "EventHandlingService";
    public Func<IntegrationEvent, bool> AcceptEvent { get; set; } = _ => true;
}