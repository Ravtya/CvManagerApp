using CvManager.Application.Dtos.Profile;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Profile;

public static class ProjectSync
{
    public static ProjectDto MapDto(Project project) =>
        new()
        {
            Id = project.Id,
            Name = project.Name,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Description = project.Description,
            TagsString = TagSync.FormatTagsString(project.Tags.Select(a => a.Tag.Name)),
        };

    public static async Task<List<ProjectDto>> GetForProfileAsync(AppDbContext context, int userProfileId)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Tags)
            .ThenInclude(a => a.Tag)
            .Where(p => p.UserProfileId == userProfileId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        return projects.Select(MapDto).ToList();
    }

    public static async Task SyncAsync(
        AppDbContext context,
        int userProfileId,
        IReadOnlyList<ProjectDto> projects,
        IReadOnlyList<int> removeProjectIds)
    {
        if (removeProjectIds.Count > 0)
        {
            var toRemove = await context.Projects
                .Where(p => p.UserProfileId == userProfileId && removeProjectIds.Contains(p.Id))
                .ToListAsync();
            context.Projects.RemoveRange(toRemove);
        }

        var named = projects.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();
        if (named.Count == 0)
            return;

        var parsed = named
            .Select(p => (Input: p, TagNames: TagSync.ParseTagsString(p.TagsString)))
            .ToList();

        var tagsByKey = await TagSync.GetOrCreateByKeyAsync(
            context,
            parsed.SelectMany(x => x.TagNames).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        var inputIds = named.Where(p => p.Id > 0).Select(p => p.Id).ToList();
        var existing = inputIds.Count == 0
            ? new Dictionary<int, Project>()
            : await context.Projects
                .Include(p => p.Tags)
                .Where(p => p.UserProfileId == userProfileId && inputIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

        foreach (var (input, tagNames) in parsed)
        {
            if (!TryGetOrAddProject(context, userProfileId, existing, input, out var project))
                continue;

            ApplyFields(project, input);
            CollectionSync.SyncByKey(
                project.Tags,
                tagNames.Select(n => tagsByKey[n].Id),
                a => a.TagId,
                tagId => new TagAssignment { TagId = tagId });
        }
    }

    private static bool TryGetOrAddProject(
        AppDbContext context,
        int userProfileId,
        Dictionary<int, Project> existing,
        ProjectDto input,
        out Project project)
    {
        if (input.Id > 0)
            return existing.TryGetValue(input.Id, out project!);

        project = new Project { UserProfileId = userProfileId };
        context.Projects.Add(project);
        return true;
    }

    private static void ApplyFields(Project project, ProjectDto input)
    {
        project.Name = input.Name;
        project.StartDate = input.StartDate == default
            ? DateOnly.FromDateTime(DateTime.UtcNow)
            : input.StartDate;
        project.EndDate = input.EndDate;
        project.Description = input.Description;
    }
}
