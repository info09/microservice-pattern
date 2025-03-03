using Pattern.Library.BookApi.Infrastructures.Data;

namespace Pattern.Library.BookApi.Bootstraping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddOpenApi();
            builder.AddNpgsqlDbContext<BookDbContext>("cqrs-book-db");

            return builder;
        }
    }
}
