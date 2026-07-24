using CvManager.Application.Dtos.Positions;

namespace CvManager.Application.Dtos.Home;

public class HomePageDto
{
    public int TotalPositions { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalRecruiters { get; set; }
    public int TotalCvs { get; set; }
    public int CvsPublishedLast24Hours { get; set; }
    public IReadOnlyList<PositionListItemDto> LatestPositions { get; set; } = [];
    public IReadOnlyList<PositionListItemDto> PopularPositions { get; set; } = [];
    public IReadOnlyList<TagCloudItemDto> TagCloud { get; set; } = [];
}
