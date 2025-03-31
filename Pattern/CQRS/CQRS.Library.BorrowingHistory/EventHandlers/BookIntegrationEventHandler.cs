using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;
using CQRS.Library.IntegrationEvents;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;

public class BookIntegrationEventHandler(BorrowingHistoryDbContext context, ILogger<BookIntegrationEventHandler> logger) : IRequestHandler<BookCreatedIntegrationEvent>, IRequestHandler<BookUpdatedIntegrationEvent>
{
    public async Task Handle(BookCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book created event: {bookId}", request.BookId);
        context.Books.Add(new Infrastructure.Entity.Book
        {
            Id = request.BookId,
            Author = request.Author,
            Title = request.Title
        });
        await context.SaveChangesAsync();
    }

    public async Task Handle(BookUpdatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book updated event: {bookId}", request.BookId);
        await context.Books.Where(i => i.Id == request.BookId)
                            .ExecuteUpdateAsync(setter => setter.SetProperty(b => b.Title, request.Title)
                                                                .SetProperty(b => b.Author, request.Author), cancellationToken: cancellationToken);
        await context.BorrowingHistoryItems.Where(i => i.BookId == request.BookId)
                                            .ExecuteUpdateAsync(setter => setter.SetProperty(i => i.BookTitle, request.Title)
                                                                                .SetProperty(i => i.BookAuthor, request.Author), cancellationToken: cancellationToken);
    }
}
