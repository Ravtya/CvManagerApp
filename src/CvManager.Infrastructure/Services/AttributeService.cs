using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Attributes;
using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Attributes;
using CvManager.Infrastructure.Persistence;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class AttributeService(AppDbContext context)
{
    public Task<PagedResult<AttributeListItemDto>> GetAttributesAsync(string? search = null, int page = 1)
    {
        var query = context.AttributeDefinitions.AsNoTracking()
            .WhereILikeAny(search, prefix: false,
                a => a.Name,
                a => a.Description,
                a => a.Category.Name);
        return Paging.ToPagedAsync(ProjectListItems(query), page);
    }

    public async Task<AttributeFormDto> GetCreateFormAsync()
    {
        var form = new AttributeFormDto();
        await PopulateCategoryOptionsAsync(form);
        return form;
    }

    public async Task<AttributeFormDto?> GetAttributeByIdAsync(int id)
    {
        var form = await ProjectForm(context.AttributeDefinitions.AsNoTracking()).SingleOrDefaultAsync(f => f.Id == id);
        if (form is not null) await PopulateCategoryOptionsAsync(form);
        return form;
    }

    public async Task<ServiceResult<int>> CreateAsync(AttributeFormDto request)
    {
        var attribute = new AttributeDefinition
        {
            DataType = request.DataType,
            IsBuiltIn = request.IsBuiltIn
        };

        if (ApplyForm(attribute, request) is { } fail)
            return fail;

        return await SaveNewAttributeAsync(attribute, request.IsBuiltIn);
    }

    public async Task<ServiceResult<int>> UpdateAsync(AttributeFormDto request)
    {
        var attribute = await context.AttributeDefinitions.Include(a => a.Options)
            .SingleOrDefaultAsync(a => a.Id == request.Id);
        if (attribute is null) return ServiceResult<int>.FailCode(CommonErrorCodes.NotFound);

        if (EfSave.IsRowVersionMismatch(attribute.RowVersion, request.RowVersion))
            return ServiceResult<int>.FailCode(CommonErrorCodes.ConcurrencyConflict);

        if (ApplyForm(attribute, request) is { } fail) return fail;

        EfSave.SetRowVersion(context, attribute, request.RowVersion, e => e.Name);
        return await EfSave.TrySaveAsync(context, () => attribute.Id);
    }

    public Task<BatchResult> DeleteManyAsync(IEnumerable<int>? ids) =>
        Batch.RunExecuteDeleteAsync(
            ids,
            LoadAttributesForDeleteAsync,
            a => a.Name,
            a => !a.IsBuiltIn && !a.IsUsedByPosition,
            a => a.IsBuiltIn
                ? AttributeErrorCodes.DeleteBuiltInNotAllowed
                : AttributeErrorCodes.DeleteInUseByPosition,
            candidates => Batch.ExecuteDeleteByIdsAsync(context.AttributeDefinitions, candidates));

    public async Task<IReadOnlyList<AttributeSuggestItemDto>> SuggestAsync(string? q, int? categoryId = null,
        bool excludeBuiltIn = false, int skip = 0, int take = 20)
    {
        var query = ApplySuggestFilters(context.AttributeDefinitions.AsNoTracking(), q, categoryId, excludeBuiltIn);
        return await ProjectSuggestItems(query).Skip(skip).Take(take).ToListAsync();
    }

    public async Task PopulateCategoryOptionsAsync(AttributeFormDto form)
    {
        form.CategoryOptions = await AttributeLookups.GetCategoriesAsync(context);
        if (form is { CategoryId: 0, CategoryOptions.Count: > 0 })
            form.CategoryId = form.CategoryOptions[0].Id;
    }

    private static IQueryable<AttributeListItemDto> ProjectListItems(IQueryable<AttributeDefinition> query) =>
        query
            .OrderBy(a => a.Category.Name)
            .ThenBy(a => a.Name)
            .Select(a => new AttributeListItemDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                CategoryName = a.Category.Name,
                DataType = a.DataType,
                IsBuiltIn = a.IsBuiltIn
            });

    private static IQueryable<AttributeFormDto> ProjectForm(IQueryable<AttributeDefinition> query) =>
        query.Select(a => new AttributeFormDto
        {
            Id = a.Id,
            RowVersion = a.RowVersion,
            Name = a.Name,
            Description = a.Description,
            CategoryId = a.AttributeCategoryId,
            DataType = a.DataType,
            IsBuiltIn = a.IsBuiltIn,
            Options = a.Options
                .OrderBy(o => o.Value)
                .Select(o => new AttributeOptionDto { Id = o.Id, Value = o.Value })
                .ToList()
        });

    private static IQueryable<AttributeDefinition> ApplySuggestFilters(IQueryable<AttributeDefinition> query,
        string? q, int? categoryId, bool excludeBuiltIn)
    {
        if (excludeBuiltIn)
            query = query.Where(a => !a.IsBuiltIn);
        if (categoryId is > 0)
            query = query.Where(a => a.AttributeCategoryId == categoryId);
        return query.WhereILikeAny(q, prefix: true, a => a.Name);
    }

    private static IQueryable<AttributeSuggestItemDto> ProjectSuggestItems(IQueryable<AttributeDefinition> query) =>
        query
            .OrderBy(a => a.Name)
            .Select(a => new AttributeSuggestItemDto
            {
                Id = a.Id,
                Name = a.Name,
                DataType = a.DataType,
                CategoryId = a.AttributeCategoryId,
            });

    private static ServiceResult<int>? ApplyForm(AttributeDefinition attribute, AttributeFormDto request)
    {
        ApplyEditableFields(attribute, request);
        var error = SyncDropdownOptions(attribute, request);
        return error is null ? null : ServiceResult<int>.Fail(error);
    }

    private static void ApplyEditableFields(AttributeDefinition attribute, AttributeFormDto request)
    {
        attribute.Name = request.Name.Trim();
        attribute.Description = request.Description?.Trim() ?? string.Empty;
        attribute.AttributeCategoryId = request.CategoryId;
    }

    private static ServiceError? SyncDropdownOptions(AttributeDefinition attribute, AttributeFormDto request)
    {
        if (attribute.DataType != AttributeDataType.Dropdown)
            return null;

        var incoming = request.Options
            .Select(o => (o.Id, Value: o.Value.Trim()))
            .Where(o => o.Value.Length > 0)
            .DistinctBy(o => o.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (incoming.Count == 0)
            return ServiceError.FieldError(nameof(AttributeFormDto.Options), AttributeErrorCodes.DropdownAtLeastOneOption);

        var existing = attribute.Options.ToDictionary(o => o.Id);

        foreach (var (id, value) in incoming)
        {
            if (id is int oid && existing.Remove(oid, out var option))
                option.Value = value;
            else
                attribute.Options.Add(new AttributeOption { Value = value });
        }

        foreach (var option in existing.Values)
            attribute.Options.Remove(option);

        return null;
    }

    private async Task<ServiceResult<int>> SaveNewAttributeAsync(AttributeDefinition attribute, bool isBuiltIn)
    {
        context.AttributeDefinitions.Add(attribute);

        await using var transaction = isBuiltIn
            ? await context.Database.BeginTransactionAsync()
            : null;

        var saveResult = await EfSave.TrySaveAsync(context, () => attribute.Id);
        if (!saveResult.IsSuccess)
            return saveResult;

        if (transaction is not null)
        {
            await ProfileBuiltInAttributes.AddBuiltInAttributeToAllProfilesAsync(context, attribute.Id);
            var seedResult = await EfSave.TrySaveAsync(context, () => attribute.Id);
            if (!seedResult.IsSuccess)
                return seedResult;

            await transaction.CommitAsync();
        }

        return saveResult;
    }

    private async Task<Dictionary<int, AttributeDeleteInfo>> LoadAttributesForDeleteAsync(List<int> idList)
    {
        var attrs = await context.AttributeDefinitions
            .AsNoTracking()
            .Where(a => idList.Contains(a.Id))
            .Select(a => new AttributeDeleteInfo(
                a.Id,
                a.Name,
                a.IsBuiltIn,
                a.PositionAttributes.Any()))
            .ToListAsync();
        return attrs.ToDictionary(a => a.Id);
    }

    private sealed record AttributeDeleteInfo(int Id, string Name, bool IsBuiltIn, bool IsUsedByPosition);
}