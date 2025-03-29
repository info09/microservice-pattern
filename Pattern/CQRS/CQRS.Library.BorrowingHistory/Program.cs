using CQRS.Library.BorrowingHistoryService.Bootstraping;
using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;

using Pattern.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationService();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



await app.MigrateDbContextAsync<BorrowingHistoryDbContext>();

app.Run();