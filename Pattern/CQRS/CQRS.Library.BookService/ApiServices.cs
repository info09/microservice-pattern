using CQRS.Library.BookService.Infrastructure.Data;
using EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Library.BookService
{
    public class ApiServices(BookDbContext context, IEventPublisher eventPublisher)
    {
        public BookDbContext Context => context;
        public IEventPublisher EventPublisher => eventPublisher;

    }
}
