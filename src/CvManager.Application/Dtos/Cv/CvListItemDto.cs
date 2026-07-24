namespace CvManager.Application.Dtos.Cv;

public class CvListItemDto
{
    public int Id { get; init; }
    public string PositionTitle { get; init; } = string.Empty;
    public string CandidateEmail { get; init; } = string.Empty;
    public DateTimeOffset PublishedAt { get; init; }
    public int LikeCount { get; init; }
}
