using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Positions;

public class PositionListItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public PositionAccessMode AccessMode { get; init; }
    public int CvCount { get; init; }
}

