using CvManager.Application.Dtos.Profile;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Profile;

namespace CvManager.Infrastructure.Cv;

internal static class CvProfileProjection
{
    public static List<ProfileAttributeFieldDto> ProjectAttributes(Position position, UserProfile profile)
    {
        var valuesByAttrId = ProfileAttributes.ToAttributeDictionary(profile);

        return position.Attributes
            .OrderBy(a => a.Attribute.Category.Name)
            .ThenBy(a => a.Attribute.Name)
            .Select(posAttr =>
            {
                valuesByAttrId.TryGetValue(posAttr.AttributeDefinitionId, out var value);
                return ProfileAttributes.MapField(posAttr.Attribute, value);
            })
            .ToList();
    }

    public static List<ProjectDto> ProjectProjects(Position position, UserProfile profile)
    {
        var filterTagIds = position.Tags.Select(f => f.TagId).ToHashSet();
        var query = profile.Projects.AsEnumerable();
        if (filterTagIds.Count > 0)
            query = query.Where(p => filterTagIds.All(tagId => p.Tags.Any(a => a.TagId == tagId)));
        return query.OrderByDescending(p => p.StartDate).Take(Math.Max(0, position.MaxProjectsInCv))
            .Select(ProjectSync.MapDto).ToList();
    }
}