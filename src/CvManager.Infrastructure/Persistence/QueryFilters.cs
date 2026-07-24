using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Persistence;

public static class QueryFilters
{
    public static IQueryable<T> WhereILikeAny<T>(this IQueryable<T> source, string? search, bool prefix,
        params Expression<Func<T, string?>>[] properties)
    {
        var pattern = BuildLikePattern(search, prefix);
        if (pattern is null || properties.Length == 0)
            return source;

        var predicate = PredicateBuilder.New<T>(false);
        foreach (var property in properties)
        {
            var prop = property;
            predicate = predicate.Or(e => EF.Functions.ILike(prop.Invoke(e)!, pattern, "\\"));
        }

        return source.AsExpandable().Where(predicate);
    }

    private static string? BuildLikePattern(string? search, bool prefix)
    {
        var trimmed = search?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        var escaped = trimmed
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

        return prefix ? escaped + "%" : "%" + escaped + "%";
    }
}