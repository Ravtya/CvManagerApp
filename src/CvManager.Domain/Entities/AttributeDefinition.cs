using CvManager.Domain.Enums;

namespace CvManager.Domain.Entities;

public class AttributeDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int AttributeCategoryId { get; set; }
    public AttributeCategory Category { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public uint RowVersion { get; set; }

    public bool IsBuiltIn { get; set; }

    public ICollection<AttributeOption> Options { get; set; } = [];
    public ICollection<ProfileAttributeValue> ProfileValues { get; set; } = [];
    public ICollection<PositionAttribute> PositionAttributes { get; set; } = [];
}
