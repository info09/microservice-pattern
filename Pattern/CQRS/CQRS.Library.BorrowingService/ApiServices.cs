using CQRS.Library.BorrowingService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowingService;

public class ApiServices(BorrowingDbContext context, IEventPublisher eventPublisher)
{
    public BorrowingDbContext Context => context;
    public IEventPublisher EventPublisher => eventPublisher;
}
