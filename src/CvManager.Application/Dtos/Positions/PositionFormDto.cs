using CvManager.Application.Dtos.Attributes;
using CvManager.Application.Validation;
using CvManager.Domain;
using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Positions;

public class PositionFormDto : FormDtoBase
{
    [LocalizedRequired]
    [LocalizedMaxLength(FieldLengths.Name)]
    public string Title { get; set; } = string.Empty;

    [LocalizedRequired]
    [LocalizedMaxLength(FieldLengths.Description)]
    public string ShortDescription { get; set; } = string.Empty;

    [LocalizedRange(0, FieldLengths.MaxProjectsInCv)]
    public int MaxProjectsInCv { get; set; } = 4;

    public PositionAccessMode AccessMode { get; set; } = PositionAccessMode.Public;

    [LocalizedMaxLength(FieldLengths.TagsString)]
    public string? TagsString { get; set; }

    public Dictionary<int, PositionAttributeDto> AttributesById { get; set; } = new();

    public IReadOnlyList<AttributeCategoryDto> AttributeCategories { get; set; } = [];

    public int? MyCvId { get; set; }
    public bool CanFillCv { get; set; }
}
