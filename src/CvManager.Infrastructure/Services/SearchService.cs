using CvManager.Application.Dtos.Search;
using CvManager.Infrastructure.Cv;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Positions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Linq.Expressions;

namespace CvManager.Infrastructure.Services;

public class SearchService(AppDbContext context)
{
    private const int MaxResultsPerGroup = 30;
    private const string FtsConfig = "simple";

    private static readonly Expression<Func<NpgsqlTsVector, string, bool>> MatchesVector =
        (vector, tsQuery) => vector.Matches(EF.Functions.ToTsQuery(FtsConfig, tsQuery));

    private static readonly Expression<Func<string, string, bool>> MatchesText =
        (text, tsQuery) => EF.Functions.ToTsVector(FtsConfig, text)
            .Matches(EF.Functions.ToTsQuery(FtsConfig, tsQuery));

    public async Task<SearchPageDto> SearchAsync(string? query, PositionViewer viewer)
    {
        var q = (query ?? string.Empty).Trim();
        var canSeeCvs = viewer.CanManageRecruiting || !string.IsNullOrEmpty(viewer.UserId);

        if (q.Length == 0)
        {
            return new SearchPageDto
            {
                Query = string.Empty,
                CanSeeCvs = canSeeCvs,
            };
        }

        var tsQuery = ToPrefixTsQuery(q);
        if (tsQuery is null)
        {
            return new SearchPageDto
            {
                Query = q,
                CanSeeCvs = canSeeCvs,
            };
        }

        return new SearchPageDto
        {
            Query = q,
            Positions = await SearchPositionsAsync(tsQuery, viewer),
            Cvs = canSeeCvs ? await SearchCvsAsync(tsQuery, viewer) : [],
            Discussions = await SearchDiscussionsAsync(tsQuery, viewer),
            CanSeeCvs = canSeeCvs,
        };
    }

    private async Task<IReadOnlyList<SearchHitDto>> SearchPositionsAsync(string tsQuery, PositionViewer viewer)
    {
        var positions = await PositionAccess.VisibleAsync(context, context.Positions.AsNoTracking(), viewer);
        var rows = await positions.AsExpandable()
            .Where(p => MatchesVector.Invoke(p.SearchVector, tsQuery)
                || p.Tags.Any(t => MatchesText.Invoke(t.Tag.Name, tsQuery)))
            .OrderBy(p => p.Title)
            .Take(MaxResultsPerGroup)
            .Select(p => new { p.Id, p.Title, p.ShortDescription })
            .ToListAsync();

        return rows.Select(p => new SearchHitDto
        {
            Id = p.Id.ToString(),
            Title = p.Title,
            Subtitle = string.IsNullOrWhiteSpace(p.ShortDescription) ? null : p.ShortDescription,
        }).ToList();
    }

    private async Task<IReadOnlyList<SearchHitDto>> SearchCvsAsync(string tsQuery, PositionViewer viewer)
    {
        IQueryable<Domain.Entities.Cv> cvs = context.Cvs.AsNoTracking();
        if (!viewer.CanManageRecruiting)
        {
            if (string.IsNullOrEmpty(viewer.UserId))
                return [];
            cvs = await CvAccess.ForProfileAsync(context, cvs, viewer.UserId);
        }

        var hits = await (
            from c in cvs.AsExpandable()
            join u in context.Users.AsNoTracking() on c.UserProfile.UserId equals u.Id
            where MatchesVector.Invoke(c.Position.SearchVector, tsQuery)
                || MatchesText.Invoke((u.Email ?? "") + " " + (u.UserName ?? ""), tsQuery)
                || c.UserProfile.AttributeValues.Any(v => MatchesVector.Invoke(v.SearchVector, tsQuery))
                || c.UserProfile.Projects.Any(p =>
                    MatchesVector.Invoke(p.SearchVector, tsQuery)
                    || p.Tags.Any(t => MatchesText.Invoke(t.Tag.Name, tsQuery)))
                || c.Position.Tags.Any(t => MatchesText.Invoke(t.Tag.Name, tsQuery))
            select new
            {
                c.Id,
                PositionTitle = c.Position.Title,
                Email = u.Email ?? u.UserName ?? string.Empty,
                LikeCount = c.Likes.Count,
            })
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return hits.Select(h => new SearchHitDto
        {
            Id = h.Id.ToString(),
            Title = string.IsNullOrWhiteSpace(h.Email) ? h.PositionTitle : $"{h.Email} — {h.PositionTitle}",
            LikeCount = h.LikeCount,
        }).ToList();
    }

    private async Task<IReadOnlyList<SearchHitDto>> SearchDiscussionsAsync(string tsQuery, PositionViewer viewer)
    {
        var visible = await PositionAccess.VisibleAsync(context, context.Positions.AsNoTracking(), viewer);
        var rows = await (
            from d in context.DiscussionPosts.AsNoTracking().AsExpandable()
            join p in visible on d.PositionId equals p.Id
            join u in context.Users.AsNoTracking() on d.AuthorUserId equals u.Id
            where MatchesVector.Invoke(d.SearchVector, tsQuery)
                || MatchesVector.Invoke(p.SearchVector, tsQuery)
                || MatchesText.Invoke((u.Email ?? "") + " " + (u.UserName ?? ""), tsQuery)
            orderby d.CreatedAt descending
            select new
            {
                d.Id,
                d.PositionId,
                PositionTitle = p.Title,
                Author = u.Email ?? u.UserName ?? string.Empty,
                d.Content,
            })
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return rows.Select(d => new SearchHitDto
        {
            Id = d.Id.ToString(),
            Title = d.PositionTitle,
            Subtitle = Truncate($"{d.Author}: {d.Content}", 160),
            PositionId = d.PositionId,
        }).ToList();
    }

    private static string? ToPrefixTsQuery(string q)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var c in q)
        {
            if (char.IsLetterOrDigit(c))
            {
                current.Append(char.ToLowerInvariant(c));
            }
            else if (current.Length > 0)
            {
                tokens.Add(current.ToString());
                current.Clear();
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.Count == 0
            ? null
            : string.Join(" & ", tokens.Select(t => t + ":*"));
    }

    private static string Truncate(string text, int maxLength)
    {
        var flat = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return flat.Length <= maxLength ? flat : flat[..(maxLength - 1)] + "…";
    }
}
