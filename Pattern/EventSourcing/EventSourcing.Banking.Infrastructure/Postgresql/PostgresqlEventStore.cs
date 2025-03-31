using EventSourcing.Banking.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Banking.Infrastructure.Postgresql;

public class PostgresqlEventStore : IEventStore
{
    private readonly EventStoreDbContext dbContext;

    public PostgresqlEventStore(EventStoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<long> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        if (events == null || !events.Any())
        {
            return -1;
        }

        EventStream? stream;

        if (state == StreamStates.New)
        {
            stream = new EventStream
            {
                Id = streamId,
                CurrentVersion = 0
            };
            dbContext.EventStreams.Add(stream);
        }
        else
        {
            stream = await dbContext.EventStreams.FindAsync([streamId], cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Stream '{streamId}' not found.");
        }

        var lastId = Guid.Empty;

        foreach (var evt in events)
        {
            var @event = new Event
            {
                Id = lastId,
                StreamId = streamId,
                Data = evt.Data,
                Type = evt.Type,
                CreatedAtUtc = evt.CreatedAtUtc,
                MetaData = evt.MetaData ?? string.Empty,
                Version = evt.Version
            };

            dbContext.Events.Add(@event);

            stream.CurrentVersion = evt.Version;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return stream.CurrentVersion;
    }

    public Task<List<Event>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Events.ToListAsync(cancellationToken);
    }

    public Task<List<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default)
    {
        if (afterVersion == null)
        {
            return dbContext.Events.Where(e => e.StreamId == streamId).OrderBy(e => e.Version).ToListAsync(cancellationToken);
        }
        else
        {
            return dbContext.Events.Where(e => e.StreamId == streamId && e.Version > afterVersion).OrderBy(e => e.Version).ToListAsync(cancellationToken);
        }
    }
}
