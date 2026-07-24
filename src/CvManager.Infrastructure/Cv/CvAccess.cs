using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Domain.Rules;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Positions;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;
using CvEntity = CvManager.Domain.Entities.Cv;

namespace CvManager.Infrastructure.Cv;

public static class CvAccess
{
    public static bool HasAccess(Position position, UserProfile profile) =>
        CvAccessEvaluator.HasAccess(position, ProfileAttributes.ToAttributeDictionary(profile));

    public static async Task<IQueryable<CvEntity>> ForProfileAsync(AppDbContext context,
        IQueryable<CvEntity> query, string profileUserId)
    {
        var visible = await PositionAccess.VisibleAsync(context, context.Positions.AsNoTracking(),
            new PositionViewer(profileUserId, CanManageRecruiting: false));
        var positionIds = await visible.Select(p => p.Id).ToListAsync();
        return query.Where(c => positionIds.Contains(c.PositionId));
    }

    public static async Task<IQueryable<CvEntity>> ForPositionAsync(AppDbContext context,
        IQueryable<CvEntity> query, int positionId)
    {
        var position = await context.Positions.AsNoTracking()
            .Include(p => p.Attributes.Where(a => a.HasAccessRule))
            .ThenInclude(a => a.Attribute)
            .FirstOrDefaultAsync(p => p.Id == positionId);
        if (position is null)
            return query.Where(_ => false);

        query = query.Where(c => c.PositionId == positionId);
        if (position.AccessMode == PositionAccessMode.Public)
            return query;

        var profiles = await context.UserProfiles.AsNoTracking()
            .Include(p => p.AttributeValues)
            .Where(p => p.Cvs.Any(c => c.PositionId == positionId))
            .ToListAsync();

        var eligible = profiles.Where(p => HasAccess(position, p)).Select(p => p.Id).ToList();
        return query.Where(c => eligible.Contains(c.UserProfileId));
    }
}