using EventBus.Event;

namespace EventBus;

public interface IIntegrationEventFactory
{
    IntegrationEvent? CreateEvent(string typeName, string value);
}
