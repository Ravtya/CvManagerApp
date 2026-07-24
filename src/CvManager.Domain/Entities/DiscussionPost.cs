using NpgsqlTypes;

namespace CvManager.Domain.Entities;

public class DiscussionPost
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public Position Position { get; set; } = null!;
}
