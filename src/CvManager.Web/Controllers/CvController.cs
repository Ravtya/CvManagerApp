using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Cv;
using CvManager.Infrastructure.Services;
using CvManager.Web.Extensions;
using CvManager.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[Authorize]
public class CvController(CvService cvService) : AppController
{
    [HttpGet]
    public async Task<IActionResult> UserCvs(string? userId = null, int page = 1)
    {
        var access = TryAccess(userId);
        if (access is null) return Forbid();
        var model = await cvService.GetUserCvsAsync(access.ProfileUserId, page);
        if (model is null) return NotFound();
        model.IsOwn = access.IsOwn;
        model.CanEdit = access.CanEdit;
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> PositionCvs(int positionId, int page = 1)
    {
        ViewBag.PositionId = positionId;
        return View(await cvService.GetPositionCvsAsync(positionId, page));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await cvService.GetDetailsAsync(id, CurrentUserId);
        var access = model is null ? null : TryAccess(model.ProfileUserId);
        if (model is null || access is null) return NotFound();
        model.CanEdit = access.CanEdit;
        model.CanLike = User.CanManageRecruiting();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int positionId)
    {
        var model = await cvService.GetCreateAsync(positionId, CurrentUserId);
        if (model is null) return NotFound();
        return model.Id > 0
            ? RedirectToAction(nameof(Details), new { id = model.Id })
            : View(nameof(Details), model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CvSaveDto model)
    {
        if (TryAccess(model.ProfileUserId) is not { CanEdit: true }) return Forbid();
        return await RunAndRedirectAsync(
            () => cvService.SaveAsync(model),
            () => RedirectToAction(nameof(Create), new { positionId = model.PositionId }),
            r => r.Created ? SuccessMessageCodes.CvCreated : SuccessMessageCodes.CvSaved,
            successRedirect: r => RedirectToAction(nameof(Details), new { id = r.CvId }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public Task<IActionResult> ToggleLike(int id) =>
        RunJsonAsync(
            () => cvService.ToggleLikeAsync(id, CurrentUserId),
            jsonSuccess: r => new { success = true, liked = r.Liked, likeCount = r.LikeCount });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Delete(List<int>? selectedIds, string? profileUserId) =>
        BatchAndRedirectAsync(
            () => cvService.DeleteManyAsync(selectedIds, CurrentUserId, User.IsAdmin()),
            nameof(UserCvs),
            routeValues: string.IsNullOrEmpty(profileUserId) ? null : new { userId = profileUserId });

    private Access? TryAccess(string? userId)
    {
        var profileUserId = string.IsNullOrEmpty(userId) ? CurrentUserId : userId;
        var isOwn = string.Equals(profileUserId, CurrentUserId, StringComparison.Ordinal);
        return isOwn || User.CanManageRecruiting()
            ? new Access(profileUserId, isOwn, isOwn || User.IsAdmin())
            : null;
    }

    private record Access(string ProfileUserId, bool IsOwn, bool CanEdit);
}