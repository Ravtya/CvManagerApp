using CvManager.Domain.Entities;
using CvManager.Domain.Enums;

namespace CvManager.Domain.Rules;

public static class CvAccessEvaluator
{
    public static bool HasAccess(Position position, IReadOnlyDictionary<int, ProfileAttributeValue> valuesByAttributeId)
    {
        if (position.AccessMode == PositionAccessMode.Public) return true;
        var rules = position.Attributes.Where(a => a.HasAccessRule).ToList();
        if (rules.Count == 0) return false;
        return rules.All(rule =>
        {
            valuesByAttributeId.TryGetValue(rule.AttributeDefinitionId, out var value);
            var dataType = rule.Attribute.DataType;
            return MatchesRule(rule, value, dataType);
        });
    }

    private static bool MatchesRule(PositionAttribute rule, ProfileAttributeValue? value, AttributeDataType? dataType) =>
        dataType switch
        {
            AttributeDataType.Numeric => CompareOrdered(rule.Operator, value?.NumericValue, rule.ComparisonNumeric),
            AttributeDataType.Date => CompareOrdered(rule.Operator, value?.DateValue, rule.ComparisonDate),
            AttributeDataType.Boolean => CompareBoolean(rule.Operator, value?.BooleanValue),
            AttributeDataType.Dropdown => CompareDropdown(rule.Operator, value?.DropdownOptionId, rule.ComparisonOptionId),
            AttributeDataType.String => CompareString(rule.Operator, value?.StringValue, rule.ComparisonString),
            _ => false
        };

    private static bool CompareOrdered<T>(AccessRuleOperator op, T? actual, T? expected)
        where T : struct, IComparable<T>
    {
        if (actual is null || expected is null)
            return false;

        var cmp = actual.Value.CompareTo(expected.Value);
        return op switch
        {
            AccessRuleOperator.Equals => cmp == 0,
            AccessRuleOperator.NotEquals => cmp != 0,
            AccessRuleOperator.GreaterThan => cmp > 0,
            AccessRuleOperator.GreaterThanOrEqual => cmp >= 0,
            AccessRuleOperator.LessThan => cmp < 0,
            AccessRuleOperator.LessThanOrEqual => cmp <= 0,
            _ => false
        };
    }

    private static bool CompareBoolean(AccessRuleOperator op, bool? actual) =>
        op switch
        {
            AccessRuleOperator.IsChecked => actual == true,
            AccessRuleOperator.IsNotChecked => actual != true,
            _ => false
        };

    private static bool CompareDropdown(AccessRuleOperator op, int? actualOptionId, int? expectedOptionId)
    {
        if (expectedOptionId is null)
            return false;

        return op switch
        {
            AccessRuleOperator.Equals => actualOptionId == expectedOptionId,
            AccessRuleOperator.NotEquals => actualOptionId != expectedOptionId,
            _ => false
        };
    }

    private static bool CompareString(AccessRuleOperator op, string? actual, string? expected)
    {
        var left = (actual ?? string.Empty).Trim();
        var right = (expected ?? string.Empty).Trim();

        return op switch
        {
            AccessRuleOperator.Equals => string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            AccessRuleOperator.NotEquals => !string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
