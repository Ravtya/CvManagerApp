using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Home;
using CvManager.Application.Dtos.Positions;
using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Domain.Rules;
using CvManager.Infrastructure.Attributes;
using CvManager.Infrastructure.Cv;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Persistence;
using CvManager.Infrastructure.Positions;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class PositionService(AppDbContext context)
{
    public async Task<HomePageDto> GetHomePageAsync(PositionViewer viewer)
    {
        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var positions = await VisibleAsync(viewer);

        var roleCounts = await (
            from ur in context.UserRoles
            join r in context.Roles on ur.RoleId equals r.Id
            where r.Name == RoleNames.Candidate || r.Name == RoleNames.Recruiter
            group r by r.Name
            into g
            select new { Role = g.Key, Count = g.Count() }
        ).ToListAsync();

        var tagCloud = await context.Tags
            .AsNoTracking()
            .Select(t => new TagCloudItemDto
            {
                Name = t.Name,
                Count = t.Assignments.Count + t.PositionTags.Count,
            })
            .Where(t => t.Count > 0)
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Name)
            .Take(40)
            .ToListAsync();

        return new HomePageDto
        {
            TotalPositions = await positions.CountAsync(),
            TotalCandidates = roleCounts.FirstOrDefault(x => x.Role == RoleNames.Candidate)?.Count ?? 0,
            TotalRecruiters = roleCounts.FirstOrDefault(x => x.Role == RoleNames.Recruiter)?.Count ?? 0,
            TotalCvs = await context.Cvs.CountAsync(),
            CvsPublishedLast24Hours = await context.Cvs.CountAsync(c => c.PublishedAt >= since),
            LatestPositions = await ProjectListItems(positions.OrderByDescending(p => p.Id).Take(10)).ToListAsync(),
            PopularPositions = await ProjectListItems(positions.OrderByDescending(p => p.Cvs.Count()).Take(5))
                .ToListAsync(),
            TagCloud = tagCloud,
        };
    }

    public async Task<PagedResult<PositionListItemDto>> GetPositionsAsync(
        string? search,
        int page,
        PositionViewer viewer)
    {
        var query = await VisibleAsync(viewer);
        query = query
            .WhereILikeAny(search, prefix: false, p => p.Title, p => p.ShortDescription)
            .OrderBy(p => p.Title);
        return await Paging.ToPagedAsync(ProjectListItems(query), page);
    }

    public async Task<PositionFormDto> GetCreateFormAsync()
    {
        var form = new PositionFormDto();
        await PopulateFormLookupsAsync(form);
        return form;
    }

    public async Task<IReadOnlyList<string>> SuggestTagsAsync(string? query, int take = 20) =>
        await context.Tags.AsNoTracking().WhereILikeAny(query, prefix: true, t => t.Name)
            .OrderBy(t => t.Name).Take(take).Select(t => t.Name).ToListAsync();

    public async Task<PositionFormDto?> GetPositionByIdAsync(int id, PositionViewer viewer)
    {
        var position = await LoadPositionAsync(id);
        if (position is null)
            return null;

        UserProfile? profile = null;
        if (!string.IsNullOrEmpty(viewer.UserId))
        {
            profile = await context.UserProfiles.AsNoTracking()
                .Include(p => p.AttributeValues)
                .Include(p => p.Cvs.Where(c => c.PositionId == id))
                .FirstOrDefaultAsync(p => p.UserId == viewer.UserId);
        }

        bool? hasAccess = null;
        if (!viewer.CanManageRecruiting && position.AccessMode != PositionAccessMode.Public)
        {
            hasAccess = profile is not null && CvAccess.HasAccess(position, profile);
            if (hasAccess != true)
                return null;
        }

        var form = MapToFormDto(position);
        if (profile is not null)
        {
            hasAccess ??= CvAccess.HasAccess(position, profile);
            var cv = profile.Cvs.FirstOrDefault();
            form.MyCvId = hasAccess == true && cv is not null ? cv.Id : null;
            form.CanFillCv = hasAccess == true && cv is null;
        }

        if (viewer.CanManageRecruiting)
            await PopulateFormLookupsAsync(form, includeDefinitionMeta: false);

        return form;
    }

    public async Task<PositionAttributeDto?> GetAttributeFormAsync(int definitionId)
    {
        var item = await AttributeLookups.ProjectPositionAttribute(
                context.AttributeDefinitions.AsNoTracking().Where(a => a.Id == definitionId))
            .SingleOrDefaultAsync();

        if (item is null)
            return null;

        var operators = AccessRuleOperatorRules.GetAllowedOperators(item.DataType);
        item.AccessRuleOperator = operators.Count > 0 ? operators[0] : default;
        return item;
    }

    public async Task<PositionFormDto?> GetDuplicateFormAsync(int id)
    {
        var form = await GetPositionByIdAsync(
            id, new PositionViewer(null, CanManageRecruiting: true));
        if (form is null) return null;
        form.Id = null;
        form.RowVersion = 0;
        form.Title = $"Copy of {form.Title}";
        form.MyCvId = null;
        form.CanFillCv = false;
        return form;
    }

    public async Task<ServiceResult<int>> CreateAsync(PositionFormDto request)
    {
        var position = new Position();
        if (await ApplyFormChangesAsync(position, request) is { } fail)
            return fail;

        context.Positions.Add(position);
        return await EfSave.TrySaveAsync(context, () => position.Id);
    }

    public async Task<ServiceResult<int>> UpdateAsync(PositionFormDto request)
    {
        var position = await LoadPositionForUpdateAsync(request.Id);
        if (position is null) return ServiceResult<int>.FailCode(CommonErrorCodes.NotFound);

        if (EfSave.IsRowVersionMismatch(position.RowVersion, request.RowVersion))
            return ServiceResult<int>.FailCode(CommonErrorCodes.ConcurrencyConflict);

        if (await ApplyFormChangesAsync(position, request) is { } fail) return fail;

        EfSave.SetRowVersion(context, position, request.RowVersion, e => e.Title);
        return await EfSave.TrySaveAsync(context, () => position.Id);
    }

    public Task<BatchResult> DeleteManyAsync(IEnumerable<int>? ids) =>
        Batch.RunExecuteDeleteAsync(
            ids,
            LoadPositionsForDeleteAsync,
            p => p.Title,
            p => !p.HasCvs,
            _ => PositionErrorCodes.DeleteInUseByCv,
            candidates => Batch.ExecuteDeleteByIdsAsync(context.Positions, candidates));

    public async Task PopulateFormLookupsAsync(PositionFormDto form, bool includeDefinitionMeta = true)
    {
        form.AttributeCategories = await AttributeLookups.GetCategoriesAsync(context);
        if (includeDefinitionMeta)
            await AttributeLookups.ApplyDefinitionMetaAsync(context, form.AttributesById.Values);
    }

    private Task<IQueryable<Position>> VisibleAsync(PositionViewer viewer) =>
        PositionAccess.VisibleAsync(context, context.Positions.AsNoTracking(), viewer);

    private async Task<ServiceResult<int>?> ApplyFormChangesAsync(Position position, PositionFormDto request)
    {
        ApplyEditableFields(position, request);
        ApplyAttributes(position, request.AttributesById);
        await ApplyTagsAsync(position, request.TagsString);

        var rulesError = PositionAccess.Apply(position, request);
        return rulesError is not null ? ServiceResult<int>.Fail(rulesError) : null;
    }

    private static void ApplyEditableFields(Position position, PositionFormDto request)
    {
        position.Title = request.Title.Trim();
        position.ShortDescription = request.ShortDescription.Trim();
        position.AccessMode = request.AccessMode;
        position.MaxProjectsInCv = request.MaxProjectsInCv;
    }

    private static void ApplyAttributes(Position position, IReadOnlyDictionary<int, PositionAttributeDto> byId) =>
        CollectionSync.SyncByKey(position.Attributes, byId.Keys, a => a.AttributeDefinitionId,
            id => new PositionAttribute { AttributeDefinitionId = id });

    private async Task ApplyTagsAsync(Position position, string? tagsString)
    {
        var byKey = await TagSync.GetOrCreateByKeyAsync(context, TagSync.ParseTagsString(tagsString));
        CollectionSync.SyncByKey(position.Tags, byKey.Values, l => l.Tag, tag => new PositionTag { Tag = tag });
    }

    private static IQueryable<PositionListItemDto> ProjectListItems(IQueryable<Position> query) =>
        query.Select(p => new PositionListItemDto
        {
            Id = p.Id,
            Title = p.Title,
            ShortDescription = p.ShortDescription,
            AccessMode = p.AccessMode,
            CvCount = p.Cvs.Count(),
        });

    private Task<Position?> LoadPositionAsync(int id) =>
        context.Positions
            .AsNoTracking()
            .Include(p => p.Tags)
            .ThenInclude(f => f.Tag)
            .Include(p => p.Attributes)
            .ThenInclude(a => a.Attribute)
            .ThenInclude(a => a.Category)
            .Include(p => p.Attributes)
            .ThenInclude(a => a.Attribute)
            .ThenInclude(a => a.Options)
            .SingleOrDefaultAsync(p => p.Id == id);

    private Task<Position?> LoadPositionForUpdateAsync(int? id) =>
        context.Positions
            .Include(p => p.Attributes)
            .Include(p => p.Tags)
            .ThenInclude(t => t.Tag)
            .SingleOrDefaultAsync(p => p.Id == id);

    private async Task<Dictionary<int, PositionDeleteInfo>> LoadPositionsForDeleteAsync(List<int> idList)
    {
        var positions = await context.Positions
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .Select(p => new PositionDeleteInfo(p.Id, p.Title, p.Cvs.Any()))
            .ToListAsync();
        return positions.ToDictionary(p => p.Id);
    }

    private static PositionFormDto MapToFormDto(Position position) =>
        new()
        {
            Id = position.Id,
            RowVersion = position.RowVersion,
            Title = position.Title,
            ShortDescription = position.ShortDescription,
            AccessMode = position.AccessMode,
            MaxProjectsInCv = position.MaxProjectsInCv,
            TagsString = TagSync.FormatTagsString(position.Tags.Select(f => f.Tag.Name)),
            AttributesById = position.Attributes.Select(MapAttributeDto).ToDictionary(t => t.AttributeDefinitionId)
        };

    private static PositionAttributeDto MapAttributeDto(PositionAttribute a) =>
        new()
        {
            AttributeDefinitionId = a.AttributeDefinitionId,
            Name = a.Attribute.Name,
            CategoryId = a.Attribute.AttributeCategoryId,
            CategoryName = a.Attribute.Category.Name,
            DataType = a.Attribute.DataType,
            HasAccessRule = a.HasAccessRule,
            AccessRuleOperator = a.Operator,
            ComparisonString = a.ComparisonString,
            ComparisonNumeric = a.ComparisonNumeric,
            ComparisonDate = a.ComparisonDate,
            ComparisonOptionId = a.ComparisonOptionId,
            DropdownOptions = a.Attribute.DataType == AttributeDataType.Dropdown
                ? AttributeLookups.MapOptions(a.Attribute.Options)
                : [],
        };

    private sealed record PositionDeleteInfo(int Id, string Title, bool HasCvs);
}