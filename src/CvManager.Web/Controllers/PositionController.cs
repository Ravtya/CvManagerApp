using CvManager.Application.Dtos.Positions;
using CvManager.Infrastructure.Services;
using CvManager.Web.Extensions;
using CvManager.Web.Formatting;
using CvManager.Web.Hubs;
using CvManager.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CvManager.Web.Controllers;

public class PositionController(PositionService positionService, PositionExportService positionExportService,
    DiscussionService discussionService, IHubContext<DiscussionHub> discussionHub) : AppController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewBag.Search = search;
        return View(await positionService.GetPositionsAsync(search, page, CurrentViewer()));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> Create() => View(nameof(Details), await positionService.GetCreateFormAsync());

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var position = await positionService.GetPositionByIdAsync(id, CurrentViewer());
        if (position is null) return NotFound();
        ViewBag.DiscussionPosts = await discussionService.GetPostsAsync(id);
        return View(position);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportToken(int id)
    {
        var token = await positionExportService.EnsureTokenAsync(id);
        if (token is null) return NotFound();
        return View(token);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SuggestTags(string? q)
    {
        var names = await positionService.SuggestTagsAsync(q);
        return Json(names);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> AttributeRow(int defId)
    {
        var row = await positionService.GetAttributeFormAsync(defId);
        if (row is null) return NotFound();
        return PartialView("_PositionAttributeCard", row);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id)
    {
        var form = await positionService.GetDuplicateFormAsync(id);
        if (form is null) return NotFound();
        return View(nameof(Details), form);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Save(PositionFormDto model) =>
        SaveAndRedirectAsync(
            model,
            () => positionService.CreateAsync(model),
            () => positionService.UpdateAsync(model),
            id => RedirectToAction(nameof(Details), new { id }),
            () => positionService.PopulateFormLookupsAsync(model));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Delete(List<int>? selectedIds) =>
        BatchAndRedirectAsync(() => positionService.DeleteManyAsync(selectedIds));

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> AddDiscussionPost(int positionId, string? content) =>
        RunJsonAsync(
            () => discussionService.CreateAsync(positionId, CurrentUserId, content),
            afterSuccess: r => BroadcastDiscussionAsync(positionId, "post", new
            {
                id = r.Id,
                authorUserId = CurrentUserId,
                authorName = User.Identity?.Name ?? CurrentUserId,
                content = r.Content,
                contentHtml = Markdown.ToHtml(r.Content),
                createdAt = DateTimeOffset.UtcNow.ToString("o"),
            }));

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> EditDiscussionPost(int positionId, int postId, string? content) =>
        RunJsonAsync(
            () => discussionService.UpdateAsync(postId, CurrentUserId, User.IsAdmin(), content),
            afterSuccess: r => BroadcastDiscussionAsync(positionId, "updated", new
            {
                id = r.Id,
                content = r.Content,
                contentHtml = Markdown.ToHtml(r.Content),
            }));

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteDiscussionPost(int positionId, int postId) =>
        RunJsonAsync(
            () => discussionService.DeleteAsync(postId, CurrentUserId, User.IsAdmin()),
            afterSuccess: _ => BroadcastDiscussionAsync(positionId, "deleted", new { id = postId }));

    private Task BroadcastDiscussionAsync(int positionId, string method, object payload) =>
        discussionHub.Clients
            .Group(DiscussionHub.GroupName(positionId))
            .SendAsync(method, payload);
}