using CvManager.Application.Dtos.Profile;

namespace CvManager.Web.Models;

public sealed class AttributeFieldModel
{
    public required ProfileAttributeFieldDto Field { get; init; }
    public string? FieldName { get; init; }
    public bool ShowLabel { get; init; } = true;
    public bool IsEdit => !string.IsNullOrEmpty(FieldName);
}