using CvManager.Application.Dtos.Profile;

namespace CvManager.Application.Dtos.Cv;

public class CvSaveDto
{
    public int PositionId { get; set; }
    public string ProfileUserId { get; set; } = string.Empty;
    public uint ProfileRowVersion { get; set; }
    public List<ProfileAttributeFieldDto> Attributes { get; set; } = [];
}
