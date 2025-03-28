using CQRS.Library.BookService.Infrastructure.Data;

namespace CQRS.Library.BookService
{
    public class ApiServices(BookDbContext context)
    {
        public BookDbContext Context => context;
    }
}
