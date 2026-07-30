using System.Security.Cryptography;
using CvManager.Application.Dtos.Positions;
using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class PositionExportService(AppDbContext context)
{
    public async Task<PositionTokenDto?> EnsureTokenAsync(int positionId)
    {
        var position = await context.Positions.FindAsync(positionId);
        if (position is null) return null;
        if (string.IsNullOrEmpty(position.ApiToken))
        {
            position.ApiToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            await context.SaveChangesAsync();
        }

        return new PositionTokenDto(position.Id, position.Title, position.ApiToken);
    }

    public async Task<PositionExportDto?> GetByTokenAsync(string token)
    {
        var position = await LoadPositionAsync(token);
        if (position is null) return null;
        var values = await LoadValuesAsync(position);
        var attributes = position.Attributes.Select(x => Aggregate(x.Attribute, values)).ToList();
        return new PositionExportDto(position.Id, position.Title, position.Cvs.Count, attributes);
    }

    private async Task<Position?> LoadPositionAsync(string token) =>
        await context.Positions.AsNoTracking()
            .Include(x => x.Attributes)
            .ThenInclude(x => x.Attribute)
            .Include(x => x.Cvs)
            .SingleOrDefaultAsync(x => x.ApiToken == token);

    private async Task<List<ProfileAttributeValue>> LoadValuesAsync(Position position)
    {
        var profileIds = position.Cvs.Select(x => x.UserProfileId).ToList();
        var attributeIds = position.Attributes.Select(x => x.AttributeDefinitionId).ToList();
        if (profileIds.Count == 0 || attributeIds.Count == 0) return [];
        return await context.ProfileAttributeValues.AsNoTracking()
            .Include(x => x.DropdownOption)
            .Where(x => profileIds.Contains(x.UserProfileId) && attributeIds.Contains(x.AttributeDefinitionId))
            .ToListAsync();
    }

    private static PositionAttributeExportDto Aggregate(AttributeDefinition definition, List<ProfileAttributeValue> values)
    {
        var filled = values
            .Where(v => v.AttributeDefinitionId == definition.Id && ProfileAttributes.HasValue(v, definition.DataType))
            .ToList();
        return new PositionAttributeExportDto(definition.Id, definition.Name,
            definition.DataType.ToString(), filled.Count, CalculateStats(definition.DataType, filled));
    }

    private static Dictionary<string, string> CalculateStats(AttributeDataType type, List<ProfileAttributeValue> filled)
    {
        if (filled.Count == 0) return [];
        return type switch
        {
            AttributeDataType.Numeric => CalculateNumericStats(filled),
            AttributeDataType.Boolean => CalculateBoolStats(filled),
            AttributeDataType.Date => CalculateDateStats(filled),
            AttributeDataType.Dropdown => CalculateDropdownStats(filled),
            _ => [],
        };
    }

    private static Dictionary<string, string> CalculateNumericStats(List<ProfileAttributeValue> filled)
    {
        var nums = filled.Select(v => v.NumericValue!.Value).ToList();
        return new Dictionary<string, string>
        {
            ["min"] = nums.Min().ToString("0.##"),
            ["max"] = nums.Max().ToString("0.##"),
            ["avg"] = decimal.Round(nums.Average(), 2).ToString("0.##"),
        };
    }

    private static Dictionary<string, string> CalculateBoolStats(List<ProfileAttributeValue> filled)
    {
        var trues = filled.Count(v => v.BooleanValue!.Value);
        var percent = decimal.Round(100m * trues / filled.Count, 1);
        return new Dictionary<string, string> { ["truePercent"] = $"{percent}%" };
    }

    private static Dictionary<string, string> CalculateDateStats(List<ProfileAttributeValue> filled)
    {
        var dates = filled.Select(v => v.DateValue!.Value).ToList();
        return new Dictionary<string, string>
        {
            ["min"] = dates.Min().ToString("yyyy-MM-dd"),
            ["max"] = dates.Max().ToString("yyyy-MM-dd"),
            ["avg"] = DateOnly.FromDayNumber((int)Math.Round(dates.Average(d => d.DayNumber))).ToString("yyyy-MM-dd"),
        };
    }

    private static Dictionary<string, string> CalculateDropdownStats(List<ProfileAttributeValue> filled) =>
        filled
            .GroupBy(v => v.DropdownOption!.Value)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(3)
            .ToDictionary(g => g.Key, g => g.Count().ToString());
}
