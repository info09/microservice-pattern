using Pattern.Library.BookApi.Infrastructures.Data;

namespace Pattern.Library.BookApi
{
    public class ApiService(BookDbContext context)
    {
        public BookDbContext Context => context;
    }
}
