using CvManager.Application.Dtos.Profile;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Attributes;

namespace CvManager.Infrastructure.Profile;

public static class ProfileAttributes
{
    public static Dictionary<int, ProfileAttributeValue> ToAttributeDictionary(UserProfile profile) =>
        profile.AttributeValues.ToDictionary(v => v.AttributeDefinitionId);

    public static ProfileAttributeFieldDto MapField(AttributeDefinition attribute, ProfileAttributeValue? value = null) =>
        new()
        {
            ValueId = value?.Id ?? 0,
            AttributeDefinitionId = attribute.Id,
            Name = attribute.Name,
            CategoryId = attribute.AttributeCategoryId,
            CategoryName = attribute.Category.Name,
            DataType = attribute.DataType,
            StringValue = value?.StringValue,
            TextValue = value?.TextValue,
            ImageUrl = value?.ImageUrl,
            NumericValue = value?.NumericValue,
            DateValue = value?.DateValue,
            PeriodStart = value?.PeriodStart,
            PeriodEnd = value?.PeriodEnd,
            BooleanValue = value?.BooleanValue ?? false,
            DropdownOptionId = value?.DropdownOptionId,
            DropdownOptions = AttributeLookups.MapOptions(attribute.Options)
        };

    public static void Upsert(UserProfile profile, IEnumerable<ProfileAttributeFieldDto> inputs,
        bool skipEmpty = false, bool removeEmpty = false)
    {
        var byId = ToAttributeDictionary(profile);

        foreach (var input in inputs)
        {
            byId.TryGetValue(input.AttributeDefinitionId, out var value);

            if (!HasValue(input))
            {
                if (removeEmpty && value is not null)
                    profile.AttributeValues.Remove(value);
                else if (!skipEmpty)
                    ApplyValue(GetOrAdd(profile, byId, input.AttributeDefinitionId), input);
                continue;
            }

            ApplyValue(GetOrAdd(profile, byId, input.AttributeDefinitionId), input);
        }
    }

    private static ProfileAttributeValue GetOrAdd(UserProfile profile, Dictionary<int, ProfileAttributeValue> byId,
        int definitionId)
    {
        if (byId.TryGetValue(definitionId, out var value))
            return value;

        value = new ProfileAttributeValue
        {
            UserProfileId = profile.Id,
            AttributeDefinitionId = definitionId,
        };
        profile.AttributeValues.Add(value);
        byId[definitionId] = value;
        return value;
    }

    private static bool HasValue(ProfileAttributeFieldDto input) =>
        !string.IsNullOrWhiteSpace(input.StringValue)
        || !string.IsNullOrWhiteSpace(input.TextValue)
        || !string.IsNullOrWhiteSpace(input.ImageUrl)
        || input.NumericValue is not null
        || input.DateValue is not null
        || input.PeriodStart is not null
        || input.PeriodEnd is not null
        || input.DropdownOptionId is not null
        || input.BooleanValue;

    private static void ApplyValue(ProfileAttributeValue entity, ProfileAttributeFieldDto input)
    {
        entity.StringValue = input.StringValue;
        entity.TextValue = input.TextValue;
        entity.ImageUrl = input.ImageUrl;
        entity.NumericValue = input.NumericValue;
        entity.DateValue = input.DateValue;
        entity.PeriodStart = input.PeriodStart;
        entity.PeriodEnd = input.PeriodEnd;
        entity.BooleanValue = input.BooleanValue;
        entity.DropdownOptionId = input.DropdownOptionId;
    }
}
