using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowingHistoryService;

public class ApiServices(BorrowingHistoryDbContext context)
{
    public BorrowingHistoryDbContext Context => context;
}
