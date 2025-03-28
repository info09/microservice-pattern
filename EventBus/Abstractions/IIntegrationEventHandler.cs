using EventBus.Event;

namespace EventBus.Abstractions;

public interface IIntegrationEventHandler<T> where T : IntegrationEvent
{
    Task Handle(T @event);
}
