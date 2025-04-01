
using EventSourcing.Banking.AccountService.Infrastructure.Entity;
using EventSourcing.Banking.Infrastructure;
using EventSourcing.Banking.Infrastructure.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace EventSourcing.Banking.AccountService.Apis;

public static class AccountApiExetensions
{
    public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/eventsourcing/v1")
              .MapAccountApi()
              .WithTags("Event Sourcing Account Api");

        return builder;
    }

    public static RouteGroupBuilder MapAccountApi(this RouteGroupBuilder group)
    {
        group.MapPost("accounts", AccountApi.OpenAccount);
        group.MapGet("accounts/{id:guid}", AccountApi.GetAccountById);
        group.MapPut("accounts/{id:guid}/deposit", AccountApi.Deposit);
        group.MapPut("accounts/{id:guid}/withdraw", AccountApi.Withdraw);

        return group;
    }
}
public static class AccountApi
{
    internal static async Task<Results<Ok, BadRequest, NotFound>> Deposit([AsParameters] ApiServices services, Guid id, DepositRequest request)
    {
        if (request == null || id == Guid.Empty)
            return TypedResults.BadRequest();

        var account = await services.EventStore.FindAsync<Account>(id, typeResolver: TypeResolver, cancellationToken: services.CancellationToken);
        if (account == null)
        {
            return TypedResults.NotFound();
        }

        account.Deposit(request.Amount);

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        return TypedResults.Ok();
    }

    internal static async Task<Results<Ok<Account>, BadRequest, NotFound>> GetAccountById([AsParameters] ApiServices services, Guid id)
    {
        if (id == Guid.Empty)
            return TypedResults.BadRequest();

        var account = await services.EventStore.FindAsync<Account>(id, typeResolver: TypeResolver, cancellationToken: services.CancellationToken);
        if (account == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(account);
    }

    internal static async Task<Results<Ok, BadRequest>> OpenAccount([AsParameters] ApiServices services, OpenAccountRequest request)
    {
        if (request == null)
            return TypedResults.BadRequest();

        if (request.Id == Guid.Empty)
            request.Id = Guid.CreateVersion7();

        var account = Account.Create(request.Id, request.AccountNumber, request.Currency, request.Balance, request.CreditLimit);
        var events = new List<Event>();
        foreach (var item in account.PendingChanges)
        {
            events.Add(new Event
            {
                Id = item.EventId,
                StreamId = account.Id,
                Data = JsonSerializer.Serialize(item, item.GetType()),
                Type = item.GetType().FullName ?? throw new Exception($"Could not get fullname of type {item.GetType()}"),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await services.EventStore.AppendAsync(account.Id, Banking.Infrastructure.StreamStates.New, events, cancellationToken: services.CancellationToken);
        return TypedResults.Ok();
    }

    internal static async Task<Results<Ok, BadRequest, NotFound>> Withdraw([AsParameters] ApiServices services, Guid id, WithdrawRequest withdraw)
    {
        if (withdraw == null || id == Guid.Empty)
            return TypedResults.BadRequest();

        var account = await services.EventStore.FindAsync<Account>(id, typeResolver: TypeResolver, cancellationToken: services.CancellationToken);
        if (account == null)
        {
            return TypedResults.NotFound();
        }

        account.Withdraw(withdraw.Amount);

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        return TypedResults.Ok();
    }

    private static Type TypeResolver(string typeName)
    {
        var type = Type.GetType(typeName);
        return type == null ? throw new InvalidOperationException($"Type '{typeName}' not found.") : type;
    }
}

public class OpenAccountRequest
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }

}

public record DepositRequest
{
    public decimal Amount { get; set; }
}

public record WithdrawRequest
{
    public decimal Amount { get; set; }
}