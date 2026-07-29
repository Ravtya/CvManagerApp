using System.Security.Claims;
using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos;
using CvManager.Infrastructure.Positions;
using CvManager.Web.Extensions;
using CvManager.Web.Ui;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

public abstract class AppController : Controller
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected PositionViewer CurrentViewer() => new(
        User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null,
        User.CanManageRecruiting());

    protected void Success(string messageKey, params object[] args) => FlashMessages.Success(this, messageKey, args);

    protected void Error(string messageKey, params object[] args) => FlashMessages.Error(this, messageKey, args);

    protected void Errors(IReadOnlyList<string> messages) => FlashMessages.SetErrors(this, messages);

    private void ApplyServiceErrors<T>(ServiceResult<T> result)
    {
        var pageErrors = new List<string>();
        foreach (var error in result.Errors)
        {
            var message = UiMessages.FormatError(error);
            if (string.IsNullOrEmpty(error.Field))
                pageErrors.Add(message);
            else
                ModelState.AddModelError(error.Field, message);
        }

        if (pageErrors.Count > 0)
            Errors(pageErrors);
    }

    protected async Task<IActionResult> RunAndRedirectAsync<T>(
        Func<Task<ServiceResult<T>>> action,
        Func<IActionResult> redirect,
        Func<T, string>? successMessage = null,
        Func<T, IActionResult>? successRedirect = null)
    {
        if (!ModelState.IsValid)
        {
            Error(CommonErrorCodes.FormInvalid);
            return redirect();
        }

        var result = await action();
        if (!result.IsSuccess)
        {
            Errors(UiMessages.FormatErrors(result.Errors));
            return redirect();
        }

        var value = result.Value!;
        var message = successMessage?.Invoke(value);
        if (message is not null)
            Success(message);

        return successRedirect is not null ? successRedirect(value) : redirect();
    }

    protected async Task<IActionResult> RunJsonAsync<T>(
        Func<Task<ServiceResult<T>>> action,
        Func<T, Task>? afterSuccess = null,
        Func<T, object>? jsonSuccess = null)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = UiMessages.Text(CommonErrorCodes.FormInvalid) });

        var result = await action();
        if (!result.IsSuccess)
            return Json(new
            {
                success = false,
                message = result.Errors.Count > 0
                    ? UiMessages.FormatError(result.Errors[0])
                    : UiMessages.Text(CommonErrorCodes.FormInvalid),
            });

        var value = result.Value!;
        if (afterSuccess is not null)
            await afterSuccess(value);

        return Json(jsonSuccess?.Invoke(value) ?? new { success = true });
    }

    protected async Task<IActionResult> SaveAndRedirectAsync<TForm>(
        TForm form,
        Func<Task<ServiceResult<int>>> create,
        Func<Task<ServiceResult<int>>> update,
        Func<int, IActionResult> detailsRedirect,
        Func<Task> populateLookups)
        where TForm : FormDtoBase
    {
        if (!ModelState.IsValid)
            return await ReRender();

        var result = form.IsNew ? await create() : await update();

        if (!result.IsSuccess)
        {
            if (!form.IsNew && result.HasCode(CommonErrorCodes.ConcurrencyConflict))
            {
                Error(CommonErrorCodes.ConcurrencyConflict);
                return detailsRedirect(form.Id!.Value);
            }

            ApplyServiceErrors(result);
            return await ReRender();
        }

        Success(form.IsNew ? SuccessMessageCodes.RecordCreated : SuccessMessageCodes.RecordUpdated);
        return detailsRedirect(result.Value);

        async Task<IActionResult> ReRender()
        {
            await populateLookups();
            return View("Details", form);
        }
    }

    protected async Task<IActionResult> BatchAndRedirectAsync(
        Func<Task<BatchResult>> action,
        string actionName = "Index",
        string? controllerName = null,
        object? routeValues = null)
    {
        var result = await action();

        var messages = UiMessages.FormatErrors(result.Errors);
        if (messages.Count > 10)
        {
            var moreCount = messages.Count - 10;
            messages = [..messages.Take(10), UiMessages.Text(BatchMessageCodes.MoreErrors, moreCount)];
        }

        if (messages.Count > 0)
            Errors(messages);

        if (result.HasSuccess)
        {
            var key = result.IsFullSuccess ? BatchMessageCodes.SuccessFull : BatchMessageCodes.SuccessPartial;
            object[] args = result.IsFullSuccess
                ? [result.SuccessCount]
                : [result.SuccessCount, result.TotalCount];
            Success(key, args);
        }

        return RedirectToAction(actionName, controllerName, routeValues);
    }
}