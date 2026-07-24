namespace CvManager.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<TagAssignment> Assignments { get; set; } = [];
    public ICollection<PositionTag> PositionTags { get; set; } = [];
}
