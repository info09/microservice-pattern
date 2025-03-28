using System.ComponentModel;

namespace Pattern.Shared.Pagination;

public class PaginationRequest(int pageIndex = 0, int pageSize = 20)
{
    [property: DefaultValue(0)]
    public int PageIndex { get; set; } = pageIndex;

    [property: DefaultValue(20)]
    public int PageSize { get; set; } = pageSize;
}
