using CvManager.Application.Dtos.Attributes;
using CvManager.Application.Validation;
using CvManager.Domain;
using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Positions;

public class PositionAttributeDto
{
    public int AttributeDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public AttributeDataType DataType { get; set; }

    public bool HasAccessRule { get; set; }
    public AccessRuleOperator AccessRuleOperator { get; set; } = AccessRuleOperator.Equals;

    [LocalizedMaxLength(FieldLengths.AttributeString)]
    public string? ComparisonString { get; set; }
    public decimal? ComparisonNumeric { get; set; }
    public DateOnly? ComparisonDate { get; set; }
    public int? ComparisonOptionId { get; set; }

    public List<AttributeOptionDto> DropdownOptions { get; set; } = [];
}
