namespace EventSourcing.Banking.SeedWork;

public interface IDomainEvent
{
    public Guid EventId { get; set; }
    public long Version { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
