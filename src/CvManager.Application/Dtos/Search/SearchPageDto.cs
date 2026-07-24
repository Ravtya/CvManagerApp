namespace CvManager.Application.Dtos.Search;

public class SearchPageDto
{
    public string Query { get; init; } = string.Empty;
    public IReadOnlyList<SearchHitDto> Positions { get; init; } = [];
    public IReadOnlyList<SearchHitDto> Cvs { get; init; } = [];
    public IReadOnlyList<SearchHitDto> Discussions { get; init; } = [];
    public bool CanSeeCvs { get; init; }
}

public class SearchHitDto
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public int? LikeCount { get; init; }
    public int? PositionId { get; init; }
}
