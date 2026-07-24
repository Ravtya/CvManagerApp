using CvManager.Infrastructure.Services;
using CvManager.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminController(AdminService adminService) : AppController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewBag.Search = search;
        return View(await adminService.GetUsersAsync(search, page));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ChangeRoles(List<string>? userIds, string? role, bool assign) =>
        BatchAndRedirectAsync(() => adminService.ChangeRolesAsync(userIds, role, assign));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SetBlockState(List<string>? userIds, bool block) =>
        BatchAndRedirectAsync(() => adminService.SetBlockStateAsync(userIds, block));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteUsers(List<string>? userIds) =>
        BatchAndRedirectAsync(() => adminService.DeleteUsersAsync(userIds));
}
