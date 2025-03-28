using CQRS.Library.BookService.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Library.BookService.Infrastructure.Data;

public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = default!;
}
