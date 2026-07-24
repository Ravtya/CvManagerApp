using CvManager.Domain.Enums;

namespace CvManager.Application.Dtos.Attributes;

public sealed class AttributeSuggestItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AttributeDataType DataType { get; init; }
    public int CategoryId { get; init; }
}
