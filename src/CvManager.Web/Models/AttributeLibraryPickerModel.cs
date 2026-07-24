using CvManager.Application.Dtos.Attributes;

namespace CvManager.Web.Models;

public sealed class AttributeLibraryPickerModel
{
    public required IReadOnlyList<AttributeCategoryDto> Categories { get; init; }
    public required string SuggestUrl { get; init; }
    public required string RowUrl { get; init; }
    public required string Placeholder { get; init; }
    public required string AllCategoriesLabel { get; init; }
    public bool ExcludeBuiltIn { get; init; }
}
