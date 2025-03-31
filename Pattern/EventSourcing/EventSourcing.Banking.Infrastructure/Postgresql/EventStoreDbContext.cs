using EventSourcing.Banking.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Banking.Infrastructure.Postgresql;

public class EventStoreDbContext : DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<EventStream> EventStreams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
