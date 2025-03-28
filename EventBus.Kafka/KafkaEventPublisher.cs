using Confluent.Kafka;
using EventBus.Abstractions;
using EventBus.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.Kafka;

public class KafkaEventPublisher(string topic, IProducer<string, MessageEnvelop> producer, ILogger logger) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
    {
        var json = JsonSerializer.Serialize(@event);
        logger.LogInformation("Publishing event {type} to topic {topic}: {event} ...", @event.GetType(), topic, json);

		try
		{
			await producer.ProduceAsync(topic, new Message<string, MessageEnvelop>
            {
                Key = @event.GetType().FullName!,
                Value = new MessageEnvelop(typeof(TEvent), json)
            });
        }
		catch (Exception ex)
		{
			logger.LogError(ex, "Error publishing event {@event}", @event.EventId);
		}
    }
}
