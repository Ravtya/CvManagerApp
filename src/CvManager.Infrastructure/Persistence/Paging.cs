using CvManager.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Persistence;

public static class Paging
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(IQueryable<T> query, int page)
    {
        var total = await query.CountAsync();
        var p = page < 1 ? 1 : page;
        if (total == 0)
            return EmptyPage<T>(p);

        var items = await query
            .Skip((p - 1) * PagedResult<T>.Size)
            .Take(PagedResult<T>.Size)
            .ToListAsync();

        return new PagedResult<T>(items, total, p);
    }

    public static PagedResult<T> EmptyPage<T>(int page) =>
        new([], 0, page < 1 ? 1 : page);
}