using CvManager.Application.Resources;
using CvManager.Application.Validation;
using CvManager.Domain;
using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Attributes;

public class AttributeFormDto : FormDtoBase
{
    [LocalizedRequired]
    [LocalizedMaxLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [LocalizedMaxLength(FieldLengths.Description)]
    public string? Description { get; set; }

    [LocalizedRange(1, int.MaxValue, nameof(ValidationResources.FieldRequired))]
    public int CategoryId { get; set; }

    public List<AttributeCategoryDto> CategoryOptions { get; set; } = [];

    public AttributeDataType DataType { get; set; } = AttributeDataType.String;

    public bool IsBuiltIn { get; set; }

    public List<AttributeOptionDto> Options { get; set; } = [];
}