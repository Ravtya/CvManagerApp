using CvManager.Application.Dtos.Attributes;
using CvManager.Application.Validation;
using CvManager.Domain;
using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Profile;

public class ProfileAttributeFieldDto
{
    public int ValueId { get; set; }
    public int AttributeDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public AttributeDataType DataType { get; set; }

    [LocalizedMaxLength(FieldLengths.AttributeString)]
    public string? StringValue { get; set; }

    [LocalizedMaxLength(FieldLengths.Text)]
    public string? TextValue { get; set; }

    [LocalizedMaxLength(FieldLengths.Description)]
    public string? ImageUrl { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool BooleanValue { get; set; }
    public int? DropdownOptionId { get; set; }

    public List<AttributeOptionDto> DropdownOptions { get; set; } = [];
}
