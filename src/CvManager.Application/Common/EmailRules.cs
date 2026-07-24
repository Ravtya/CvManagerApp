using System.Text.RegularExpressions;

namespace CvManager.Application.Common;

public static partial class EmailRules
{
    public const string Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";

    public static bool IsValid(string? email) => !string.IsNullOrWhiteSpace(email) && MyRegex().IsMatch(email);

    [GeneratedRegex(Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
}