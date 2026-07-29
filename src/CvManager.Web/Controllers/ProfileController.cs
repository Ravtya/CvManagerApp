using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Profile;
using CvManager.Infrastructure.Services;
using CvManager.Web.Extensions;
using CvManager.Web.Ui;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[Authorize]
public class ProfileController(ProfileService profileService) : AppController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? userId)
    {
        var access = TryAccess(userId);
        if (access is null) return Forbid();
        var page = await profileService.GetProfileAsync(access.ProfileUserId);
        if (page is null) return NotFound();
        page.IsOwnProfile = access.IsOwn;
        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProfileDto model)
    {
        var access = TryAccess(model.ProfileUserId);
        if (access is null) return Forbid();
        return await RunAndRedirectAsync(
            () => profileService.SaveProfileAsync(model),
            () => RedirectToAction(nameof(Index), Route(access)),
            _ => SuccessMessageCodes.ProfileSaved);
    }

    [HttpGet]
    public async Task<IActionResult> InfoFieldRow(int defId)
    {
        var field = await profileService.GetInfoFieldTemplateAsync(defId);
        return field is null ? NotFound() : PartialView("_ProfileInfoFieldRow", field);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoSave([FromForm] ProfileDto model)
    {
        var access = TryAccess(model.ProfileUserId);
        if (access is null) return Json(new { success = false, error = "forbidden" });
        if (!ModelState.IsValid)
            return Json(new { success = false, message = UiMessages.Text(CommonErrorCodes.FormInvalid) });
        var result = await profileService.SaveProfileAsync(model);
        if (!result.IsSuccess)
        {
            return Json(new
            {
                success = false,
                conflict = result.HasCode(CommonErrorCodes.ConcurrencyConflict),
                message = UiMessages.FormatError(result.Errors[0])
            });
        }
        return Json(new { success = true, rowVersion = result.Value });
    }

    private Access? TryAccess(string? userId)
    {
        var profileUserId = string.IsNullOrEmpty(userId) ? CurrentUserId : userId;
        var isOwn = string.Equals(profileUserId, CurrentUserId, StringComparison.Ordinal);
        return isOwn || User.IsAdmin()
            ? new Access(profileUserId, isOwn)
            : null;
    }

    private static object? Route(Access access) => access.IsOwn ? null : new { userId = access.ProfileUserId };

    private record Access(string ProfileUserId, bool IsOwn);
}