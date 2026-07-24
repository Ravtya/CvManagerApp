using CvManager.Domain.Enums;
using NpgsqlTypes;

namespace CvManager.Domain.Entities;

public class Position
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;

    public uint RowVersion { get; set; }

    public int MaxProjectsInCv { get; set; }

    public PositionAccessMode AccessMode { get; set; }

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public ICollection<PositionAttribute> Attributes { get; set; } = [];
    public ICollection<PositionTag> Tags { get; set; } = [];
    public ICollection<Cv> Cvs { get; set; } = [];
    public ICollection<DiscussionPost> DiscussionPosts { get; set; } = [];
}
