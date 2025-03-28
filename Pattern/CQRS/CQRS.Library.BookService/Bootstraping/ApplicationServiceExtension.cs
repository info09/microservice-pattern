using CQRS.Library.BookService.Infrastructure.Data;
using Pattern.Shared;

namespace CQRS.Library.BookService.Bootstraping
{
    public static class ApplicationServiceExtension
    {
        public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddOpenApi();
            builder.AddNpgsqlDbContext<BookDbContext>(Consts.DefaultDatabase);

            
            return builder;
        }
    }
}
