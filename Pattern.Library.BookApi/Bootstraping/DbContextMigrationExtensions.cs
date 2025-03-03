using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Pattern.Library.BookApi.Infrastructures.Data;

namespace Pattern.Library.BookApi.Bootstraping
{
    public static class DbContextMigrationExtensions
    {
        public static async Task<IHost> MigrateApiDbContextAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            await host.MigrateDbContext<BookDbContext>(cancellationToken);
            return host;
        }

        private static async Task<IHost> MigrateDbContext<TContext>(this IHost host, CancellationToken cancellationToken = default) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider;
            var logger = service.GetRequiredService<ILogger<TContext>>();
            var context = service.GetService<TContext>();

            if(context is not null)
            {
                try
                {
                    var dbCreator = context.GetService<IRelationalDatabaseCreator>();
                    var strategy = context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        if (!await dbCreator.ExistsAsync(cancellationToken))
                        {
                            await dbCreator.CreateAsync(cancellationToken);
                        }
                    });

                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                    await strategy.ExecuteAsync(async () =>
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                    });
                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }

            return host;
        }
    }
}
