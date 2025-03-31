using CQRS.Library.BorrowerService.Infrastructure.Entity;
using CQRS.Library.IntegrationEvents;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pattern.Shared.Pagination;

namespace CQRS.Library.BorrowerService.Apis;

public static class BorrowerApi
{
    public static IEndpointRouteBuilder MapBorrowerApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1").MapGroupBorrowerApi().WithTags("Book Api");
        return builder;
    }

    public static RouteGroupBuilder MapGroupBorrowerApi(this RouteGroupBuilder group)
    {
        group.MapGet("borrowers", async ([AsParameters] ApiServices service) =>
        {
            return await service.Context.Borrowers.ToListAsync();
        });

        group.MapGet("borrowers/{id}", async ([FromRoute] Guid id, [AsParameters] ApiServices service) =>
        {
            return await service.Context.Borrowers.FindAsync(id);
        });

        group.MapGet("borrowers/paging", async ([AsParameters] ApiServices service, [AsParameters] PaginationRequest request) =>
        {
            var data = await service.Context.Borrowers.Skip(request.PageIndex).Take(request.PageSize).ToListAsync();
            var total = await service.Context.Borrowers.CountAsync();

            return new PaginationResponse<Borrower>(request.PageIndex, request.PageSize, total, data);
        });

        group.MapPost("borrowers", CreateBorrower);
        group.MapPut("borrowers/{id:guid}", UpdateBorrower);

        group.MapDelete("borrowers/{id:guid}", DeleteBorrower);

        return group;
    }

    private static async Task<Results<Ok, BadRequest>> DeleteBorrower([AsParameters] ApiServices services, Guid id)
    {
        var existingBorrower = await services.Context.Borrowers.FindAsync(id);
        if (existingBorrower == null)
            return TypedResults.BadRequest();

        services.Context.Borrowers.Remove(existingBorrower);
        await services.Context.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<Borrower>, BadRequest>> UpdateBorrower([AsParameters] ApiServices services, Guid id, Borrower borrower)
    {
        var existingBorrower = await services.Context.Borrowers.FindAsync(id);
        if (existingBorrower == null)
            return TypedResults.BadRequest();

        existingBorrower.Name = borrower.Name;
        existingBorrower.Address = borrower.Address;
        existingBorrower.PhoneNumber = borrower.PhoneNumber;
        existingBorrower.Email = borrower.Email;

        services.Context.Update(existingBorrower);
        await services.Context.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BorrowerUpdatedIntegrationEvent
        {
            BorrowerId = borrower.Id,
            Name = borrower.Name,
            Address = borrower.Address,
            PhoneNumber = borrower.PhoneNumber,
            Email = borrower.Email
        });

        return TypedResults.Ok(existingBorrower);
    }

    private static async Task<Results<Ok<Borrower>, BadRequest>> CreateBorrower([AsParameters] ApiServices services, Borrower borrower)
    {
        if (borrower == null)
            return TypedResults.BadRequest();

        if (borrower.Id == Guid.Empty)
            borrower.Id = Guid.CreateVersion7();

        await services.Context.Borrowers.AddAsync(borrower);
        await services.Context.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BorrowerCreatedIntegrationEvent
        {
            BorrowerId = borrower.Id,
            Name = borrower.Name,
            Address = borrower.Address,
            PhoneNumber = borrower.PhoneNumber,
            Email = borrower.Email
        });

        return TypedResults.Ok(borrower);
    }
}
