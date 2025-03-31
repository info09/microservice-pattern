﻿
using CQRS.Library.BorrowingHistoryService.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pattern.Shared.Pagination;

namespace CQRS.Library.BorrowingHistoryService.Apis;

public static class BorrowingHistoryApi
{
    public static IEndpointRouteBuilder MapBorrowingHistoryApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1")
              .MapBorrowingHistoryApi()
              .WithTags("BorrowingHistory Api");

        return builder;
    }

    public static RouteGroupBuilder MapBorrowingHistoryApi(this RouteGroupBuilder group)
    {
        group.MapGet("history/items", FindBorrowingHistory);

        return group;
    }

    private static async Task<Results<Ok<PaginationResponse<BorrowingHistoryItem>>, BadRequest>> FindBorrowingHistory(
        [AsParameters] ApiServices services, 
        [AsParameters] PaginationRequest request, 
        [AsParameters] FindBorrowingHistoryFilters filters, 
        string? sortBy)
    {
        ValidityStatus validityStatus = ValidityStatus.GetValidityStatus(filters.ValidityStatus);

        var query = services.Context.BorrowingHistoryItems.AsQueryable();
        if (filters.BorrowerId.HasValue)
        {
            query = query.Where(x => x.BorrowerId == filters.BorrowerId);
        }

        if (filters.BookId.HasValue)
        {
            query = query.Where(x => x.BookId == filters.BookId);
        }

        if (filters.BorrowingOnly)
        {
            query = query.Where(i => !i.HasReturned);
        }

        if (validityStatus != ValidityStatus.All)
        {
            var now = DateTime.UtcNow;
            if(validityStatus == ValidityStatus.Valid)
            {
                query = query.Where(i => i.ValidUntil > now);
            }
            else if (validityStatus == ValidityStatus.Expired)
            {
                query = query.Where(i => i.ValidUntil <= now);
            }
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = BuildOrderBy(query, sortBy);
        }

        if (!string.IsNullOrEmpty(filters.Query))
        {
            query = query.Where(x => x.BorrowerName.Contains(filters.Query) || x.BookTitle.Contains(filters.Query));
        }

        var totalItems = await query.LongCountAsync();
        var result = new PaginationResponse<BorrowingHistoryItem>(request.PageIndex, request.PageSize, totalItems, await query.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToListAsync());

        return TypedResults.Ok(result);
    }

    private static IQueryable<BorrowingHistoryItem> BuildOrderBy(IQueryable<BorrowingHistoryItem> query, string sortBy)
    {
        if (sortBy == "borrowedAt")
        {
            return query.OrderBy(x => x.BorrowedAt);
        }
        else if (sortBy == "borrowedAtDesc")
        {
            return query.OrderByDescending(x => x.BorrowedAt);
        }
        else if (sortBy == "borrowerName")
        {
            return query.OrderBy(x => x.BorrowerName);
        }
        else if (sortBy == "borrowerNameDesc")
        {
            return query.OrderByDescending(x => x.BorrowerName);
        }
        else if (sortBy == "bookTitle")
        {
            return query.OrderBy(x => x.BookTitle);
        }
        else if (sortBy == "bookTitleDesc")
        {
            return query.OrderByDescending(x => x.BookTitle);
        }

        return query;
    }
}

public record FindBorrowingHistoryFilters(Guid? BorrowerId = null, Guid? BookId = null, string Query = "", string ValidityStatus = "all", bool BorrowingOnly = false);

public class ValidityStatus(string id)
{
    public string Id => id;

    public static readonly ValidityStatus All = new("all");
    public static readonly ValidityStatus Valid = new("valid");
    public static readonly ValidityStatus Expired = new("expired");

    public static ValidityStatus GetValidityStatus(string status)
    {
        return status switch
        {
            "valid" => Valid,
            "expired" => Expired,
            _ => All
        };
    }
}