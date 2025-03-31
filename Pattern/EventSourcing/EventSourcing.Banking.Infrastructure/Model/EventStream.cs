namespace EventSourcing.Banking.Infrastructure.Model;

public class EventStream
{
    public Guid Id { get; set; }
    public long CurrentVersion { get; set; }
    public ICollection<Event> Events { get; set; } = [];
}
