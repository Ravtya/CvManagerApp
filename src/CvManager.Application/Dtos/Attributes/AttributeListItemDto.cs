using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Attributes;

public class AttributeListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public AttributeDataType DataType { get; init; }
    public bool IsBuiltIn { get; init; }
}
