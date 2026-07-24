namespace CvManager.Application.Dtos.Discussion;

public class DiscussionPostDto
{
    public int Id { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
