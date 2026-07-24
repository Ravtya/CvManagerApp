using System.ComponentModel.DataAnnotations;
using CvManager.Application.Resources;

namespace CvManager.Application.Validation;

public sealed class LocalizedRequiredAttribute : RequiredAttribute
{
    public LocalizedRequiredAttribute(string resourceName = nameof(ValidationResources.FieldRequired))
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = resourceName;
    }
}

public sealed class LocalizedMaxLengthAttribute : MaxLengthAttribute
{
    public LocalizedMaxLengthAttribute(int length) : base(length)
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = nameof(ValidationResources.StringMaxLength);
    }
}

public sealed class LocalizedMinLengthAttribute : MinLengthAttribute
{
    public LocalizedMinLengthAttribute(int length) : base(length)
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = nameof(ValidationResources.StringMinLength);
    }
}

public sealed class LocalizedRangeAttribute : RangeAttribute
{
    public LocalizedRangeAttribute(int minimum, int maximum, string resourceName = nameof(ValidationResources.Range))
        : base(minimum, maximum)
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = resourceName;
    }
}

public sealed class LocalizedRegularExpressionAttribute : RegularExpressionAttribute
{
    public LocalizedRegularExpressionAttribute(string pattern, 
        string resourceName = nameof(ValidationResources.InvalidValue)) : base(pattern)
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = resourceName;
    }
}

public sealed class LocalizedCompareAttribute : CompareAttribute
{
    public LocalizedCompareAttribute(string otherProperty, 
        string resourceName = nameof(ValidationResources.PasswordsDoNotMatch)) : base(otherProperty)
    {
        ErrorMessageResourceType = typeof(ValidationResources);
        ErrorMessageResourceName = resourceName;
    }
}
