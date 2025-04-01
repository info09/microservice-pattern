﻿using EventSourcing.Banking.Infrastructure.Model;

namespace EventSourcing.Banking.Infrastructure;

public interface IEventStore
{
    Task<long> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    Task<List<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default);
}

public enum StreamStates
{
    New,
    Existing
}
