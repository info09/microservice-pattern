using CQRS.Library.BorrowerService.Apis;
using CQRS.Library.BorrowerService.Bootstraping;
using CQRS.Library.BorrowerService.Infrastructure.Data;
using Pattern.DatabaseMigrationHelpers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationService();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DefaultFonts = false;
    });
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.MapBorrowerApi();

await app.MigrateDbContextAsync<BorrowerDbContext>();

app.Run();

