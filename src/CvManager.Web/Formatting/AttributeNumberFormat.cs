using System.Globalization;

namespace CvManager.Web.Formatting;

public static class AttributeNumberFormat
{
    public static string Format(decimal? value) =>
        value?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;
}