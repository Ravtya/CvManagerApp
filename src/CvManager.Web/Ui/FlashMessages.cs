using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Ui;

public static class FlashMessages
{
    public const string SuccessKey = "Success";
    public const string ErrorsKey = "Errors";

    public static void Success(Controller controller, string messageKey, params object[] args) =>
        controller.TempData[SuccessKey] = UiMessages.Text(messageKey, args);

    public static void Error(Controller controller, string messageKey, params object[] args) =>
        SetErrors(controller, [UiMessages.Text(messageKey, args)]);

    public static void SetErrors(Controller controller, IReadOnlyList<string> messages) =>
        controller.TempData[ErrorsKey] = messages.ToList();

    public static IReadOnlyList<string>? ReadErrors(object? value) =>
        value is IEnumerable<string> seq
            ? seq.ToList() is { Count: > 0 } list ? list : null
            : null;
}
