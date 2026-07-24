using CvManager.Application.Dtos.Positions;

namespace CvManager.Web.Models;

public enum HomePositionSecondaryColumn
{
    Access,
    CvCount
}

public sealed class HomePositionTableModel
{
    public required IReadOnlyList<PositionListItemDto> Items { get; init; }
    public required string TitleHeader { get; init; }
    public required string SecondaryHeader { get; init; }
    public required HomePositionSecondaryColumn Secondary { get; init; }
    public required string EmptyText { get; init; }
    public bool ShowDescription { get; init; }
}
