using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Cv;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Cv;
using CvManager.Infrastructure.Persistence;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;
using CvEntity = CvManager.Domain.Entities.Cv;

namespace CvManager.Infrastructure.Services;

public class CvService(AppDbContext context)
{
    public async Task<UserCvsDto?> GetUserCvsAsync(string profileUserId, int page = 1)
    {
        var header = await (
            from p in context.UserProfiles.AsNoTracking()
            where p.UserId == profileUserId
            join u in context.Users.AsNoTracking() on p.UserId equals u.Id
            select new { p.Id, Email = u.Email ?? u.UserName ?? string.Empty }
        ).FirstOrDefaultAsync();
        if (header is null)
            return null;

        var cvs = await CvAccess.ForProfileAsync(context, 
            context.Cvs.AsNoTracking().Where(c => c.UserProfileId == header.Id), profileUserId);

        return new UserCvsDto
        {
            ProfileUserId = profileUserId,
            ProfileEmail = header.Email,
            Page = await Paging.ToPagedAsync(
                cvs.OrderBy(c => c.Position.Title)
                    .Select(c => new CvListItemDto
                    {
                        Id = c.Id,
                        PositionTitle = c.Position.Title,
                        PublishedAt = c.PublishedAt,
                        LikeCount = c.Likes.Count,
                    }),
                page),
        };
    }

    public async Task<PagedResult<CvListItemDto>> GetPositionCvsAsync(int positionId, int page = 1)
    {
        var cvs = await CvAccess.ForPositionAsync(context, context.Cvs.AsNoTracking(), positionId);
        return await Paging.ToPagedAsync(
            from c in cvs
            join u in context.Users.AsNoTracking() on c.UserProfile.UserId equals u.Id
            orderby c.PublishedAt descending, c.Id
            select new CvListItemDto
            {
                Id = c.Id,
                CandidateEmail = u.Email ?? u.UserName ?? string.Empty,
                PublishedAt = c.PublishedAt,
                LikeCount = c.Likes.Count,
            },
            page);
    }

    public async Task<CvDetailsDto?> GetDetailsAsync(int cvId, string viewerUserId)
    {
        var ids = await context.Cvs.AsNoTracking()
            .Select(c => new { c.Id, c.UserProfileId, c.PositionId })
            .SingleOrDefaultAsync(c => c.Id == cvId);
        if (ids is null) return null;
        var loaded = await LoadFormPairAsync(ids.UserProfileId, ids.PositionId);
        if (loaded is null || !CvAccess.HasAccess(loaded.Value.Position, loaded.Value.Profile)) return null;
        return await MapDetailsAsync(loaded.Value.Profile, loaded.Value.Position, ids.Id, viewerUserId);
    }

    public async Task<CvDetailsDto?> GetCreateAsync(int positionId, string userId)
    {
        var profile = await context.UserProfiles.AsNoTracking()
            .IncludeFormGraph()
            .Include(p => p.Cvs.Where(c => c.PositionId == positionId))
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null)
            return null;

        var position = await context.Positions.AsNoTracking()
            .IncludeFormGraph()
            .FirstOrDefaultAsync(p => p.Id == positionId);
        if (position is null || !CvAccess.HasAccess(position, profile))
            return null;

        var existing = profile.Cvs.FirstOrDefault();
        if (existing is not null)
            return new CvDetailsDto { Id = existing.Id };

        var dto = await MapDetailsAsync(profile, position, cvId: 0, viewerUserId: null);
        dto.CanEdit = true;
        return dto;
    }

    public async Task<ServiceResult<(bool Liked, int LikeCount)>> ToggleLikeAsync(int cvId, string userId)
    {
        var existing = await context.CvLikes.FirstOrDefaultAsync(l => l.CvId == cvId && l.UserId == userId);

        if (existing is null)
            context.CvLikes.Add(new CvLike { CvId = cvId, UserId = userId });
        else
            context.CvLikes.Remove(existing);

        var liked = existing is null;
        var likeCount = await context.CvLikes.CountAsync(l => l.CvId == cvId);
        return await EfSave.TrySaveAsync(context, () => (liked, likeCount + (liked ? 1 : -1)));
    }

    public async Task<ServiceResult<(int CvId, bool Created)>> SaveAsync(CvSaveDto model)
    {
        var profile = await context.UserProfiles
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(p => p.UserId == model.ProfileUserId);
        if (profile is null) return ServiceResult<(int, bool)>.FailCode(CommonErrorCodes.NotFound);
        if (EfSave.IsRowVersionMismatch(profile.RowVersion, model.ProfileRowVersion))
            return ServiceResult<(int, bool)>.FailCode(CommonErrorCodes.ConcurrencyConflict);
        ProfileAttributes.Upsert(profile, model.Attributes);

        var cv = await context.Cvs.FirstOrDefaultAsync(c => c.UserProfileId == profile.Id && c.PositionId == model.PositionId);
        var created = cv is null;
        if (created)
        {
            cv = new CvEntity
            {
                UserProfileId = profile.Id,
                PositionId = model.PositionId,
                PublishedAt = DateTimeOffset.UtcNow,
            };
            context.Cvs.Add(cv);
        }

        EfSave.SetRowVersion(context, profile, model.ProfileRowVersion, e => e.UserId);
        return await EfSave.TrySaveAsync(context, () => (cv!.Id, created));
    }

    public Task<BatchResult> DeleteManyAsync(IEnumerable<int>? ids, string userId, bool isAdmin) =>
        Batch.RunExecuteDeleteAsync(
            ids,
            LoadForDeleteAsync,
            c => c.Position.Title,
            c => isAdmin || string.Equals(c.UserProfile.UserId, userId, StringComparison.Ordinal),
            _ => CvErrorCodes.NotOwner,
            candidates => Batch.ExecuteDeleteByIdsAsync(context.Cvs, candidates));

    private async Task<CvDetailsDto> MapDetailsAsync(UserProfile profile, Position position, int cvId, string? viewerUserId)
    {
        var meta = await context.Users.AsNoTracking()
            .Where(u => u.Id == profile.UserId)
            .Select(u => new
            {
                Email = u.Email ?? u.UserName ?? string.Empty,
                LikeCount = cvId > 0 ? context.CvLikes.Count(l => l.CvId == cvId) : 0,
                LikedByMe = cvId > 0
                            && viewerUserId != null
                            && context.CvLikes.Any(l => l.CvId == cvId && l.UserId == viewerUserId),
            })
            .FirstAsync();

        return new CvDetailsDto
        {
            Id = cvId,
            PositionId = position.Id,
            PositionTitle = position.Title,
            ProfileUserId = profile.UserId,
            CandidateEmail = meta.Email,
            ProfileRowVersion = profile.RowVersion,
            LikeCount = meta.LikeCount,
            LikedByMe = meta.LikedByMe,
            Attributes = CvProfileProjection.ProjectAttributes(position, profile),
            Projects = CvProfileProjection.ProjectProjects(position, profile),
        };
    }

    private async Task<(UserProfile Profile, Position Position)?> LoadFormPairAsync(int profileId, int positionId)
    {
        var profile = await context.UserProfiles.AsNoTracking().IncludeFormGraph().FirstOrDefaultAsync(p => p.Id == profileId);
        if (profile is null) return null;
        var position = await context.Positions.AsNoTracking().IncludeFormGraph().FirstOrDefaultAsync(p => p.Id == positionId);
        return position is null ? null : (profile, position);
    }

    private async Task<Dictionary<int, CvEntity>> LoadForDeleteAsync(List<int> ids)
    {
        var cvs = await context.Cvs
            .Include(c => c.UserProfile)
            .Include(c => c.Position)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
        return cvs.ToDictionary(c => c.Id);
    }
}