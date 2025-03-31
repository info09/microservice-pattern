using EventSourcing.Banking.AccountService.Apis;
using EventSourcing.Banking.AccountService.Bootstraping;
using EventSourcing.Banking.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

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

app.MapAccountApi();

await app.MigrateEventStoreDatabaseAsync();

app.Run();

