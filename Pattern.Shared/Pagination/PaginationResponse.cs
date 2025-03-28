namespace Pattern.Shared.Pagination;

public class PaginationResponse<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> items) where TEntity : class
{
    public int PageIndex => pageIndex;
    public int PageSize => pageSize;
    public long Count => count;
    public IEnumerable<TEntity> Items => items;
}
