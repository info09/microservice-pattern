using CQRS.Library.BookService.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pattern.Shared.Pagination;

namespace CQRS.Library.BookService.Apis
{
    public static class BookApi
    {
        public static IEndpointRouteBuilder MapBookApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/cqrs/v1").MapGroupBookApi().WithTags("Book Api");
            return builder;
        }

        public static RouteGroupBuilder MapGroupBookApi(this RouteGroupBuilder group)
        {
            group.MapGet("books", async ([AsParameters] ApiServices service) =>
            {
                return await service.Context.Books.ToListAsync();
            });

            group.MapGet("books/{id}", async ([FromRoute] Guid id, [AsParameters] ApiServices service) =>
            {
                return await service.Context.Books.FindAsync(id);
            });

            group.MapGet("books/paging", async ([AsParameters] ApiServices service, [AsParameters]PaginationRequest request) =>
            {
                var data = await service.Context.Books.Skip(request.PageIndex).Take(request.PageSize).ToListAsync();
                var total = await service.Context.Books.CountAsync();

                return new PaginationResponse<Book>(request.PageIndex, request.PageSize, total, data);
            });

            group.MapPost("books", CreateBook);
            group.MapPut("books/{id:guid}", UpdateBook);

            group.MapDelete("books/{id:guid}", DeleteBook);

            return group;
        }

        private static async Task<Results<Ok, BadRequest>> DeleteBook([AsParameters] ApiServices services, Guid id)
        {
            var existingBook = await services.Context.Books.FindAsync(id);
            if (existingBook == null)
                return TypedResults.BadRequest();

            services.Context.Books.Remove(existingBook);
            await services.Context.SaveChangesAsync();

            return TypedResults.Ok();
        }

        private static async Task<Results<Ok<Book>, BadRequest>> UpdateBook([AsParameters] ApiServices services, Guid id, Book book)
        {
            var existingBook = await services.Context.Books.FindAsync(id);
            if (existingBook == null)
                return TypedResults.BadRequest();

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;

            services.Context.Update(existingBook);
            await services.Context.SaveChangesAsync();

            return TypedResults.Ok(existingBook);
        }

        private static async Task<Results<Ok<Book>, BadRequest>> CreateBook([AsParameters] ApiServices services, Book book)
        {
            if (book == null)
                return TypedResults.BadRequest();

            if (book.Id == Guid.Empty)
                book.Id = Guid.CreateVersion7();

            await services.Context.Books.AddAsync(book);
            await services.Context.SaveChangesAsync();

            return TypedResults.Ok(book);
        }
    }
}
