using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Cv;

internal static class CvFormQuery
{
    public static IQueryable<UserProfile> IncludeFormGraph(this IQueryable<UserProfile> query) =>
        query
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .ThenInclude(a => a.Options)
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .ThenInclude(a => a.Category)
            .Include(p => p.Projects)
            .ThenInclude(p => p.Tags)
            .ThenInclude(a => a.Tag);

    public static IQueryable<Position> IncludeFormGraph(this IQueryable<Position> query) =>
        query
            .Include(p => p.Attributes)
            .ThenInclude(a => a.Attribute)
            .ThenInclude(a => a.Category)
            .Include(p => p.Attributes)
            .ThenInclude(a => a.Attribute)
            .ThenInclude(a => a.Options)
            .Include(p => p.Tags);
}