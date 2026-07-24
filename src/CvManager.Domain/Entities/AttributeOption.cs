namespace CvManager.Domain.Entities;

public class AttributeOption
{
    public int Id { get; set; }
    public int AttributeDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;

    public AttributeDefinition Attribute { get; set; } = null!;
    public ICollection<ProfileAttributeValue> ProfileValues { get; set; } = [];
    public ICollection<PositionAttribute> PositionAttributes { get; set; } = [];
}
