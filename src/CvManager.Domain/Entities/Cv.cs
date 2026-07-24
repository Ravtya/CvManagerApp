namespace CvManager.Domain.Entities;

public class Cv
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public int PositionId { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public ICollection<CvLike> Likes { get; set; } = [];
}
