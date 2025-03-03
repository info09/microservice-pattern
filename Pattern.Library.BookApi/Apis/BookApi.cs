using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pattern.Library.BookApi.Infrastructures.Entity;

namespace Pattern.Library.BookApi.Apis
{
    public static class BookApi
    {
        public static IEndpointRouteBuilder MapBookApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/cqrs/v1").GroupBookApi().WithTags("Book Api");

            return builder;
        }

        public static RouteGroupBuilder GroupBookApi(this RouteGroupBuilder group)
        {
            group.MapGet("books", GetBooks);
            group.MapGet("books/{id:guid}", GetBookById);
            group.MapPost("books", CreateBook);
            group.MapPut("books/{id:guid}", UpdateBook);

            return group;
        }

        private static async Task<Results<NotFound, Ok>> UpdateBook([AsParameters] ApiService service, Guid id, Book book)
        {
            var existingBook = await service.Context.Books.FindAsync(id);
            if (existingBook == null) return TypedResults.NotFound();

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;

            service.Context.Books.Update(existingBook);
            await service.Context.SaveChangesAsync();

            return TypedResults.Ok();
        }

        private static async Task<Results<Created, BadRequest>> CreateBook([AsParameters] ApiService service, Book book)
        {
            if (book == null)
                return TypedResults.BadRequest();

            if (book.Id == Guid.Empty)
                book.Id = Guid.CreateVersion7();

            await service.Context.Books.AddAsync(book);
            await service.Context.SaveChangesAsync();

            return TypedResults.Created();
        }

        private static async Task<Results<Ok<Book>, NotFound>> GetBookById([AsParameters] ApiService service, Guid id)
        {
            var book = await service.Context.Books.SingleOrDefaultAsync(i => i.Id == id);
            if (book == null) return TypedResults.NotFound();

            return TypedResults.Ok(book);
        }

        private static async Task<Ok<List<Book>>> GetBooks([AsParameters] ApiService service)
        {
            var book = await service.Context.Books.ToListAsync();
            return TypedResults.Ok(book);
        }
    }
}
