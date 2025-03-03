using Pattern.Library.BookApi.Apis;
using Pattern.Library.BookApi.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapBookApi();

await app.MigrateApiDbContextAsync();

app.Run();

