using CvManager.Application.Common;
using CvManager.Domain.Enums;
using System.Globalization;
using System.Resources;

namespace CvManager.Web.Ui;

public static class UiMessages
{
    private static readonly ResourceManager ServiceMessages =
        new("CvManager.Web.Resources.ServiceMessages", typeof(Program).Assembly);

    public static string Text(string messageKey, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(messageKey)) return messageKey;
        var template = ServiceMessages.GetString(messageKey, CultureInfo.CurrentUICulture);
        if (string.IsNullOrEmpty(template)) return messageKey;
        return args is { Length: > 0 }
            ? string.Format(CultureInfo.CurrentUICulture, template, args)
            : template;
    }

    public static string DataType(AttributeDataType type) => Text($"DataType_{type}");

    public static string AccessMode(PositionAccessMode mode) => Text($"AccessMode_{mode}");

    public static string FormatError(ServiceError error)
    {
        var text = Text(error.Code);
        return string.IsNullOrEmpty(error.Label) ? text : $"{error.Label}: {text}";
    }

    public static List<string> FormatErrors(IReadOnlyList<ServiceError> errors) =>
        errors.Select(FormatError).ToList();
}