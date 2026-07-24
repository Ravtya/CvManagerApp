using System.Globalization;
using System.Reflection;
using System.Resources;

namespace CvManager.Application.Resources;

public static class ValidationResources
{
    private static readonly ResourceManager Resources =
        new("CvManager.Application.Resources.ValidationResources", typeof(ValidationResources).GetTypeInfo().Assembly);

    public static string Text(string messageKey, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(messageKey)) return messageKey;
        var template = Resources.GetString(messageKey, CultureInfo.CurrentUICulture);
        if (string.IsNullOrEmpty(template)) return messageKey;
        return args is { Length: > 0 }
            ? string.Format(CultureInfo.CurrentUICulture, template, args)
            : template;
    }

    public static string FieldRequired => Text(nameof(FieldRequired));
    public static string StringMaxLength => Text(nameof(StringMaxLength));
    public static string StringMinLength => Text(nameof(StringMinLength));
    public static string Range => Text(nameof(Range));
    public static string InvalidValue => Text(nameof(InvalidValue));
    public static string EmailRequired => Text(nameof(EmailRequired));
    public static string EmailInvalid => Text(nameof(EmailInvalid));
    public static string PasswordRequired => Text(nameof(PasswordRequired));
    public static string ConfirmPasswordRequired => Text(nameof(ConfirmPasswordRequired));
    public static string PasswordsDoNotMatch => Text(nameof(PasswordsDoNotMatch));
    public static string CurrentPasswordRequired => Text(nameof(CurrentPasswordRequired));

    public static string Email => Text(nameof(Email));
    public static string Password => Text(nameof(Password));
    public static string ConfirmPassword => Text(nameof(ConfirmPassword));
    public static string RememberMe => Text(nameof(RememberMe));
    public static string CurrentPassword => Text(nameof(CurrentPassword));
    public static string NewPassword => Text(nameof(NewPassword));
}
