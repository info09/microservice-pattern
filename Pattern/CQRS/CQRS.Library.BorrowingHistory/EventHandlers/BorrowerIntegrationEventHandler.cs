using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;
using CQRS.Library.IntegrationEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;

public class BorrowerIntegrationEventHandler(BorrowingHistoryDbContext context, ILogger<BorrowerIntegrationEventHandler> logger) : IRequestHandler<BorrowerCreatedIntegrationEvent>, IRequestHandler<BorrowerUpdatedIntegrationEvent>
{
    public async Task Handle(BorrowerUpdatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling borrower updated event: {borrowerId}", request.BorrowerId);

        await context.Borrowers.Where(i => i.Id == request.BorrowerId)
                         .ExecuteUpdateAsync(setter => setter.SetProperty(b => b.Name, request.Name)
                                                             .SetProperty(b => b.Address, request.Address)
                                                             .SetProperty(b => b.Email, request.Email)
                                                             .SetProperty(b => b.PhoneNumber, request.PhoneNumber), cancellationToken: cancellationToken);

        await context.BorrowingHistoryItems.Where(i => i.BorrowerId == request.BorrowerId)
                                     .ExecuteUpdateAsync(setter => setter.SetProperty(i => i.BorrowerName, request.Name)
                                                                     .SetProperty(i => i.BorrowerAddress, request.Address)
                                                                     .SetProperty(i => i.BorrowerEmail, request.Email)
                                                                     .SetProperty(i => i.BorrowerPhoneNumber, request.PhoneNumber), cancellationToken: cancellationToken);
    }

    public async Task Handle(BorrowerCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling borrower created event: {borrowerId}", request.BorrowerId);

        context.Borrowers.Add(new Infrastructure.Entity.Borrower
        {
            Id = request.BorrowerId,
            Name = request.Name,
            Address = request.Address,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        });

        await context.SaveChangesAsync();
    }
}
