using CvManager.Application.Common;

namespace CvManager.Application.Dtos.Cv;

public class UserCvsDto
{
    public string ProfileUserId { get; set; } = string.Empty;
    public string ProfileEmail { get; set; } = string.Empty;
    public bool IsOwn { get; set; }
    public bool CanEdit { get; set; }
    public PagedResult<CvListItemDto> Page { get; set; } = new([], 0, 1);
}
