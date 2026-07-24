using CvManager.Application.Dtos.Attributes;
using CvManager.Application.Dtos.Positions;
using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Attributes;

public static class AttributeLookups
{
    public static Task<List<AttributeCategoryDto>> GetCategoriesAsync(AppDbContext context) =>
        context.AttributeCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new AttributeCategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

    public static Task<AttributeDefinition?> GetNonBuiltInDefinitionAsync(AppDbContext context, int definitionId) =>
        context.AttributeDefinitions
            .AsNoTracking()
            .Include(a => a.Options)
            .Include(a => a.Category)
            .Where(a => a.Id == definitionId && !a.IsBuiltIn)
            .SingleOrDefaultAsync();

    public static List<AttributeOptionDto> MapOptions(IEnumerable<AttributeOption> options) =>
        options
            .OrderBy(o => o.Value)
            .Select(o => new AttributeOptionDto { Id = o.Id, Value = o.Value })
            .ToList();

    public static IQueryable<PositionAttributeDto> ProjectPositionAttribute(
        IQueryable<AttributeDefinition> query) =>
        query.Select(a => new PositionAttributeDto
        {
            AttributeDefinitionId = a.Id,
            Name = a.Name,
            CategoryId = a.AttributeCategoryId,
            CategoryName = a.Category.Name,
            DataType = a.DataType,
            AccessRuleOperator = AccessRuleOperator.Equals,
            DropdownOptions = a.DataType == AttributeDataType.Dropdown
                ? a.Options
                    .OrderBy(o => o.Value)
                    .Select(o => new AttributeOptionDto { Id = o.Id, Value = o.Value })
                    .ToList()
                : new List<AttributeOptionDto>()
        });

    public static async Task ApplyDefinitionMetaAsync(AppDbContext context, IEnumerable<PositionAttributeDto> items)
    {
        var list = items as IList<PositionAttributeDto> ?? items.ToList();
        if (list.Count == 0)
            return;

        var ids = list.Select(i => i.AttributeDefinitionId).ToList();
        var metadataById = await ProjectPositionAttribute(context.AttributeDefinitions.AsNoTracking()
            .Where(a => ids.Contains(a.Id))).ToDictionaryAsync(a => a.AttributeDefinitionId);

        foreach (var item in list)
        {
            if (!metadataById.TryGetValue(item.AttributeDefinitionId, out var metadata))
                continue;

            item.Name = metadata.Name;
            item.CategoryId = metadata.CategoryId;
            item.CategoryName = metadata.CategoryName;
            item.DataType = metadata.DataType;
            item.DropdownOptions = metadata.DropdownOptions;
        }
    }
}