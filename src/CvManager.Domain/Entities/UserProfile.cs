namespace CvManager.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public uint RowVersion { get; set; }

    public ICollection<ProfileAttributeValue> AttributeValues { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<Cv> Cvs { get; set; } = [];
}
