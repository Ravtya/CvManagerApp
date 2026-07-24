using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Positions;
using CvManager.Domain.Entities;
using CvManager.Domain.Enums;
using CvManager.Domain.Rules;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Positions;

public record PositionViewer(string? UserId, bool CanManageRecruiting);

public static class PositionAccess
{
    public static async Task<IQueryable<Position>> VisibleAsync(AppDbContext context,
        IQueryable<Position> query, PositionViewer viewer)
    {
        if (viewer.CanManageRecruiting)
            return query;

        if (string.IsNullOrEmpty(viewer.UserId))
            return query.Where(p => p.AccessMode == PositionAccessMode.Public);

        var eligibleRestrictedIds = await GetAccessibleRestrictedIdsAsync(context, viewer.UserId);
        return query.Where(p => p.AccessMode == PositionAccessMode.Public || eligibleRestrictedIds.Contains(p.Id));
    }

    private static async Task<List<int>> GetAccessibleRestrictedIdsAsync(AppDbContext context, string userId)
    {
        var profile = await context.UserProfiles.AsNoTracking()
            .Include(p => p.AttributeValues)
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null) return [];

        var restricted = await context.Positions.AsNoTracking()
            .Where(p => p.AccessMode == PositionAccessMode.Restricted)
            .Include(p => p.Attributes.Where(a => a.HasAccessRule))
            .ThenInclude(a => a.Attribute)
            .ToListAsync();

        var values = ProfileAttributes.ToAttributeDictionary(profile);
        return restricted.Where(p => CvAccessEvaluator.HasAccess(p, values)).Select(p => p.Id).ToList();
    }

    public static ServiceError? Apply(Position position, PositionFormDto request)
    {
        if (request.AccessMode == PositionAccessMode.Public)
        {
            ClearAllRules(position);
            return null;
        }

        if (ApplyRules(position, request.AttributesById) is { } fail)
            return fail;

        return position.Attributes.Any(a => a.HasAccessRule)
            ? null
            : RulesError(PositionErrorCodes.RestrictedRequiresRule);
    }

    private static ServiceError? ApplyRules(Position position, Dictionary<int, PositionAttributeDto> byId)
    {
        foreach (var row in position.Attributes)
        {
            var attr = byId[row.AttributeDefinitionId];
            if (!attr.HasAccessRule)
            {
                ClearRule(row);
                continue;
            }

            if (ApplyAttributeRule(row, attr) is { } fail)
                return fail;
        }

        return null;
    }

    private static ServiceError? ApplyAttributeRule(PositionAttribute row, PositionAttributeDto attr)
    {
        if (!AccessRuleOperatorRules.SupportsAccessRules(attr.DataType))
            return RulesError(PositionErrorCodes.RuleUnsupportedType);

        if (!AccessRuleOperatorRules.IsOperatorAllowed(attr.DataType, attr.AccessRuleOperator))
            return RulesError(PositionErrorCodes.RuleOperatorNotAllowed);

        var valueError = ApplyComparisonValue(row, attr);
        return valueError is null ? null : RulesError(valueError);
    }

    private static string? ApplyComparisonValue(PositionAttribute row, PositionAttributeDto attr)
    {
        row.HasAccessRule = true;
        row.Operator = attr.AccessRuleOperator;
        row.ComparisonString = null;
        row.ComparisonNumeric = null;
        row.ComparisonDate = null;
        row.ComparisonOptionId = null;

        return attr.DataType switch
        {
            AttributeDataType.Boolean => null,
            AttributeDataType.Numeric => SetRequired(attr.ComparisonNumeric, v => row.ComparisonNumeric = v),
            AttributeDataType.Date => SetRequired(attr.ComparisonDate, v => row.ComparisonDate = v),
            AttributeDataType.Dropdown => SetRequired(attr.ComparisonOptionId, v => row.ComparisonOptionId = v),
            AttributeDataType.String => SetRequired(attr.ComparisonString, v => row.ComparisonString = v),
            _ => PositionErrorCodes.RuleUnsupportedType
        };
    }

    private static string? SetRequired<T>(T? value, Action<T> assign) where T : struct
    {
        if (!value.HasValue)
            return PositionErrorCodes.RuleRequiresValue;
        assign(value.Value);
        return null;
    }

    private static string? SetRequired(string? value, Action<string> assign)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return PositionErrorCodes.RuleRequiresValue;
        assign(trimmed);
        return null;
    }

    private static void ClearAllRules(Position position)
    {
        foreach (var row in position.Attributes)
            ClearRule(row);
    }

    private static void ClearRule(PositionAttribute row)
    {
        row.HasAccessRule = false;
        row.Operator = default;
        row.ComparisonString = null;
        row.ComparisonNumeric = null;
        row.ComparisonDate = null;
        row.ComparisonOptionId = null;
    }

    private static ServiceError RulesError(string code) => ServiceError.CodeOnly(code);
}
