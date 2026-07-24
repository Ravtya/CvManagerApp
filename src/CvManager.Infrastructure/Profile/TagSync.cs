using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Profile;

public static class TagSync
{
    public static async Task<IReadOnlyDictionary<string, Tag>> GetOrCreateByKeyAsync(
        AppDbContext context, IReadOnlyList<string> names)
    {
        if (names.Count == 0)
            return new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);

        var keys = names.Select(static n => n.ToLowerInvariant()).ToHashSet(StringComparer.Ordinal);

        var existing = await context.Tags
            .Where(t => keys.Contains(t.Name.Trim().ToLower()))
            .ToListAsync();

        var byKey = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in names)
        {
            if (byKey.ContainsKey(name))
                continue;

            var tag = new Tag { Name = name };
            context.Tags.Add(tag);
            byKey[name] = tag;
        }

        return byKey;
    }

    public static List<string> ParseTagsString(string? tagsString) =>
        (tagsString ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    public static string FormatTagsString(IEnumerable<string> names) =>
        string.Join(", ", names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase));
}
