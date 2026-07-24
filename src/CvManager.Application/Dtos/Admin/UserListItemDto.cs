namespace CvManager.Application.Dtos.Admin;

public class UserListItemDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool IsLockedOut { get; init; }
    public bool IsEmailConfirmed { get; init; }
}
