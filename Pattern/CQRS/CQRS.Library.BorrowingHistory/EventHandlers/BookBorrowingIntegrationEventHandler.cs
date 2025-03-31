using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;
using CQRS.Library.BorrowingHistoryService.Infrastructure.Entity;
using CQRS.Library.IntegrationEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;

public class BookBorrowingIntegrationEventHandler(BorrowingHistoryDbContext context, ILogger<BookBorrowingIntegrationEventHandler> logger) : IRequestHandler<BookBorrowedIntegrationEvent>, IRequestHandler<BookReturnedIntegrationEvent>
{
    public async Task Handle(BookBorrowedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book borrowed event: {bookId}", request.BookId);
        var book = await context.Books.Where(i => i.Id == request.BookId).SingleOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Book not found");
        var borrower = await context.Borrowers.Where(i => i.Id == request.BorrowerId).SingleOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Borrower not found");

        context.BorrowingHistoryItems.Add(new BorrowingHistoryItem()
        {
            Id = request.BorrowingId,
            BookId = request.BookId,
            BorrowerId = request.BorrowerId,
            BorrowedAt = request.BorrowedAt,
            ValidUntil = request.ValidUntil,
            HasReturned = false,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            BorrowerName = borrower.Name,
            BorrowerAddress = borrower.Address,
            BorrowerEmail = borrower.Email,
            BorrowerPhoneNumber = borrower.PhoneNumber
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(BookReturnedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book borrowed event: {bookId}", request.BookId);

        await context.BorrowingHistoryItems.Where(i => i.Id == request.BorrowingId)
                                            .ExecuteUpdateAsync(setter => setter
                                                                                .SetProperty(i => i.HasReturned, true)
                                                                                .SetProperty(i => i.ReturnedAt, request.ReturnedAt), cancellationToken: cancellationToken);
    }
}
