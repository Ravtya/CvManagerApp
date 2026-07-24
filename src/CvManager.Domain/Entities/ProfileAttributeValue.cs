using NpgsqlTypes;

namespace CvManager.Domain.Entities;

public class ProfileAttributeValue
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public int AttributeDefinitionId { get; set; }

    public string? StringValue { get; set; }
    public string? TextValue { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool? BooleanValue { get; set; }
    public int? DropdownOptionId { get; set; }

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public UserProfile UserProfile { get; set; } = null!;
    public AttributeDefinition Attribute { get; set; } = null!;
    public AttributeOption? DropdownOption { get; set; }
}
