﻿using EventBus.Abstractions;
using EventBus.Event;
using Microsoft.Extensions.Logging;

namespace EventBus;

public sealed class NullEventPublisher : IEventPublisher
{
    public NullEventPublisher(ILogger<NullEventPublisher> logger)
    {
        logger.LogInformation("NullEventPublisher is used");
    }
    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
    {
        return Task.CompletedTask;
    }
}
