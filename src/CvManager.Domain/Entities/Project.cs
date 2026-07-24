using NpgsqlTypes;

namespace CvManager.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public UserProfile UserProfile { get; set; } = null!;

    public ICollection<TagAssignment> Tags { get; set; } = [];
}
