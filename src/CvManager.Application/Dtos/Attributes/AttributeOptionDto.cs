using CvManager.Application.Validation;
using CvManager.Domain;

namespace CvManager.Application.Dtos.Attributes;

public class AttributeOptionDto
{
    public int? Id { get; set; }

    [LocalizedRequired]
    [LocalizedMaxLength(FieldLengths.OptionValue)]
    public string Value { get; set; } = string.Empty;
}
