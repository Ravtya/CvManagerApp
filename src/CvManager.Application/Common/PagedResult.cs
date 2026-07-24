namespace CvManager.Application.Common;

public sealed class PagedResult<T>(IReadOnlyList<T> items, int totalCount, int page)
{
    public const int Size = 20;

    public IReadOnlyList<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page < 1 ? 1 : page;
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)Size);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
