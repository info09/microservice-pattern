using MediatR;

namespace EventBus.Event;

public class IntegrationEvent : IRequest
{
    public Guid EventId { get; set; }
    public DateTime EventCreatedDate { get; set; }

    public IntegrationEvent()
    {
        EventId = Guid.CreateVersion7();
        EventCreatedDate = DateTime.UtcNow;
    }
}
