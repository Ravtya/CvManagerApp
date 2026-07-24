using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Profile;

internal static class ProfileBuiltInAttributes
{
    public static async Task AddBuiltInValuesToProfileAsync(AppDbContext context, UserProfile profile)
    {
        var existingDefIds = profile.AttributeValues
            .Select(v => v.AttributeDefinitionId)
            .ToHashSet();

        var missingIds = await context.AttributeDefinitions
            .Where(a => a.IsBuiltIn && !existingDefIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();

        foreach (var attrId in missingIds)
        {
            profile.AttributeValues.Add(new ProfileAttributeValue
            {
                UserProfileId = profile.Id,
                AttributeDefinitionId = attrId,
            });
        }
    }

    public static async Task AddBuiltInAttributeToAllProfilesAsync(AppDbContext context, int attributeDefinitionId)
    {
        var missingProfileIds = await context.UserProfiles
            .Where(p => p.AttributeValues.All(v => v.AttributeDefinitionId != attributeDefinitionId))
            .Select(p => p.Id)
            .ToListAsync();

        if (missingProfileIds.Count == 0)
            return;

        context.ProfileAttributeValues.AddRange(missingProfileIds.Select(profileId =>
            new ProfileAttributeValue
            {
                UserProfileId = profileId,
                AttributeDefinitionId = attributeDefinitionId
            }));
    }
}
