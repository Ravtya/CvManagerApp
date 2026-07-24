using CvManager.Domain.Enums;

namespace CvManager.Domain.Rules;

public static class AccessRuleOperatorRules
{
    public static IReadOnlyList<AccessRuleOperator> GetAllowedOperators(AttributeDataType type) =>
        type switch
        {
            AttributeDataType.Numeric or AttributeDataType.Date =>
            [
                AccessRuleOperator.Equals,
                AccessRuleOperator.NotEquals,
                AccessRuleOperator.GreaterThan,
                AccessRuleOperator.GreaterThanOrEqual,
                AccessRuleOperator.LessThan,
                AccessRuleOperator.LessThanOrEqual
            ],
            AttributeDataType.Boolean =>
            [
                AccessRuleOperator.IsChecked,
                AccessRuleOperator.IsNotChecked
            ],
            AttributeDataType.Dropdown or AttributeDataType.String =>
            [
                AccessRuleOperator.Equals,
                AccessRuleOperator.NotEquals
            ],
            _ => []
        };

    public static bool SupportsAccessRules(AttributeDataType type) =>
        GetAllowedOperators(type).Count > 0;

    public static bool IsOperatorAllowed(AttributeDataType type, AccessRuleOperator op) =>
        GetAllowedOperators(type).Contains(op);
}
