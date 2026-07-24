using CvManager.Application.Dtos.Profile;

namespace CvManager.Application.Dtos.Cv;

public class CvDetailsDto
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public string ProfileUserId { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public uint ProfileRowVersion { get; set; }

    public bool Exists => Id > 0;

    public bool CanEdit { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
    public bool CanLike { get; set; }
    public List<ProfileAttributeFieldDto> Attributes { get; set; } = [];
    public List<ProjectDto> Projects { get; set; } = [];
}
