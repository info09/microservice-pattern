﻿using Confluent.Kafka;
using EventBus.Abstractions;
using EventBus.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventBus.Kafka;

public static class KafkaEventBusExtensions
{
    public static IHostApplicationBuilder AddKafkaProducerExtension(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddKafkaProducer<string, MessageEnvelop>(connectionName, configureSettings: (settings) =>
        {
        }, configureBuilder: (builder) =>
        {
            builder.SetValueSerializer(new MessageEnvelopSerializer());
        });
        return builder;
    }

    public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder, string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentNullException(nameof(topic));
        }

        if(!string.IsNullOrWhiteSpace(topic))
        {
            builder.Services.AddTransient<IEventPublisher>(service => new KafkaEventPublisher(topic, service.GetRequiredService<IProducer<string, MessageEnvelop>>(), service.GetRequiredService<ILogger>()));
        }   
    }

    public static IHostApplicationBuilder AddKafkaMessageEnvelopConsumer(this IHostApplicationBuilder builder, string groupId, string connectionName = "kafka")
    {
        builder.AddKafkaConsumer<string, MessageEnvelop>(connectionName, configureSettings: (settings) => {
            settings.Config.GroupId = groupId;
            settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
        },
        configureBuilder: (builder) =>
        {
            builder.SetValueDeserializer(new MessageEnvelopDeserializer());
        }
        );

        return builder;
    }

    public static IHostApplicationBuilder AddKafkaEventConsumer(this IHostApplicationBuilder builder, Action<EventHandlingWorkerOptions>? configureOptions = null)
    {
        var options = new EventHandlingWorkerOptions();
        configureOptions?.Invoke(options);

        builder.AddKafkaMessageEnvelopConsumer(options.KafkaGroupId);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(services => options.IntegrationEventFactory);
        builder.Services.AddHostedService<EventHandlingService>();
        return builder;
    }

    public static bool IsEvent<T1>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1);
    }

    public static bool IsEvent<T1, T2>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2);
    }

    public static bool IsEvent<T1, T2, T3>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2) || @event.GetType() == typeof(T3);
    }

    public static bool IsEvent<T1, T2, T3, T4>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2) || @event.GetType() == typeof(T3) || @event.GetType() == typeof(T4);
    }
}
internal class MessageEnvelopDeserializer : IDeserializer<MessageEnvelop>
{
    public MessageEnvelop Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<MessageEnvelop>(data) ?? throw new Exception("Error deserialize data");
    }
}

internal class MessageEnvelopSerializer : ISerializer<MessageEnvelop>
{
    public byte[] Serialize(MessageEnvelop data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}