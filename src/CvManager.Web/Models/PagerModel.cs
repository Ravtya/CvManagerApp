using CvManager.Application.Common;

namespace CvManager.Web.Models;

public sealed class PagerModel
{
    public required int Page { get; init; }
    public required int TotalCount { get; init; }
    public int PageSize => PagedResult<object>.Size;
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
