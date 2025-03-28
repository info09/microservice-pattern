using CQRS.Library.BorrowerService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowerService;

public class ApiServices(BorrowerDbContext context, IEventPublisher eventPublisher)
{
    public BorrowerDbContext Context => context;
    public IEventPublisher EventPublisher => eventPublisher;
}
