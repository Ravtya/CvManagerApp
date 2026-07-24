using CvManager.Domain.Enums;

namespace CvManager.Domain.Entities;

public class PositionAttribute
{
    public int PositionId { get; set; }
    public int AttributeDefinitionId { get; set; }

    public bool HasAccessRule { get; set; }

    public AccessRuleOperator Operator { get; set; }

    public string? ComparisonString { get; set; }
    public decimal? ComparisonNumeric { get; set; }
    public DateOnly? ComparisonDate { get; set; }
    public int? ComparisonOptionId { get; set; }

    public Position Position { get; set; } = null!;
    public AttributeDefinition Attribute { get; set; } = null!;
    public AttributeOption? ComparisonOption { get; set; }
}
