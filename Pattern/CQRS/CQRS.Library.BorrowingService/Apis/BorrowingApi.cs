﻿using CQRS.Library.BorrowingService.Infrastructure.Entity;
using CQRS.Library.IntegrationEvents;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pattern.Shared.Pagination;

namespace CQRS.Library.BorrowingService.Apis;

public static class BorrowingApi
{
    public static IEndpointRouteBuilder MapBorrowingApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1").MapGroupBorrowingApi().WithTags("Book Api");
        return builder;
    }

    public static RouteGroupBuilder MapGroupBorrowingApi(this RouteGroupBuilder group)
    {
        group.MapGet("borrowings", async ([AsParameters] ApiServices service) =>
        {
            return await service.Context.Borrowings.ToListAsync();
        });

        group.MapGet("borrowings/{id}", async ([FromRoute] Guid id, [AsParameters] ApiServices service) =>
        {
            return await service.Context.Borrowings.FindAsync(id);
        });

        group.MapGet("borrowings/paging", async ([AsParameters] ApiServices service, [AsParameters] PaginationRequest request) =>
        {
            var data = await service.Context.Borrowings.Skip(request.PageIndex).Take(request.PageSize).ToListAsync();
            var total = await service.Context.Borrowings.CountAsync();

            return new PaginationResponse<Borrowing>(request.PageIndex, request.PageSize, total, data);
        });

        group.MapPost("borrowings", BorrowBook);
        group.MapPut("borrowings/{id:guid}/return", ReturnBook);


        return group;
    }

    private static async Task<Results<Ok, NotFound>> ReturnBook([AsParameters] ApiServices services, Guid id)
    {
        var existingBorrowing = await services.Context.Borrowings.FindAsync(id);
        if(existingBorrowing == null)
        {
            return TypedResults.NotFound();
        }

        existingBorrowing.HasReturned = true;
        existingBorrowing.ReturnedAt = DateTime.UtcNow;

        await services.Context.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BookReturnedIntegrationEvent()
        {
            BorrowingId = existingBorrowing.Id,
            BorrowerId = existingBorrowing.BorrowerId,
            BookId = existingBorrowing.BookId,
            ReturnedAt = existingBorrowing.ReturnedAt.Value
        });

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<Borrowing>, BadRequest<string>>> BorrowBook([AsParameters] ApiServices services, Borrowing borrowing)
    {
        if(borrowing.BookId == Guid.Empty)
        {
            return TypedResults.BadRequest("BookId is required");
        }
        if (borrowing.BorrowerId == Guid.Empty)
        {
            return TypedResults.BadRequest("BorrowerId is required");
        }

        borrowing.Id = Guid.CreateVersion7();
        borrowing.BorrowedAt = DateTime.UtcNow;
        borrowing.ValidUntil = borrowing.ValidUntil.ToUniversalTime();
        borrowing.HasReturned = false;

        await services.Context.Borrowings.AddAsync(borrowing);
        await services.Context.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BookBorrowedIntegrationEvent()
        {
            BorrowingId = borrowing.Id,
            BorrowerId = borrowing.BorrowerId,
            BookId = borrowing.BookId,
            BorrowedAt = borrowing.BorrowedAt,
            ValidUntil = borrowing.ValidUntil
        });

        return TypedResults.Ok(borrowing);
    }
}
