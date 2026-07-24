namespace CvManager.Domain.Entities;

public class PositionTag
{
    public int PositionId { get; set; }
    public int TagId { get; set; }

    public Position Position { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
