using Microsoft.EntityFrameworkCore;
using Pattern.Library.BookApi.Infrastructures.Entity;

namespace Pattern.Library.BookApi.Infrastructures.Data
{
    public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
    {
        public DbSet<Book> Books { get; set; } = default!;
    }
}
