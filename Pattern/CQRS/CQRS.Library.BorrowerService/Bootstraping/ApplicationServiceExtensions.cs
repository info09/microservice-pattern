using CQRS.Library.BorrowerService.Infrastructure.Data;
using Pattern.Shared;

namespace CQRS.Library.BorrowerService.Bootstraping;

public static class ApplicationServiceExtension
{
    public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowerDbContext>(Consts.DefaultDatabase);


        return builder;
    }
}
