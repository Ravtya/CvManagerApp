using CvManager.Application.Dtos.Attributes;

namespace CvManager.Application.Dtos.Profile;

public class ProfileDto
{
    public string ProfileUserId { get; set; } = string.Empty;
    public string ProfileEmail { get; set; } = string.Empty;
    public uint RowVersion { get; set; }
    public bool IsOwnProfile { get; set; }

    public Dictionary<int, ProfileAttributeFieldDto> MeFieldsById { get; set; } = new();
    public Dictionary<int, ProfileAttributeFieldDto> InfoFieldsById { get; set; } = new();
    public IReadOnlyList<AttributeCategoryDto> AttributeCategories { get; set; } = [];
    public List<int> RemoveInfoValueIds { get; set; } = [];
    public List<ProjectDto> Projects { get; set; } = [];
    public List<int> RemoveProjectIds { get; set; } = [];
}
