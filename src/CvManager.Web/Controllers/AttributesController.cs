using CvManager.Application.Dtos.Attributes;
using CvManager.Infrastructure.Services;
using CvManager.Web.Ui;
using CvManager.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[Authorize]
public class AttributesController(AttributeService attributeService) : AppController
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewBag.Search = search;
        return View(await attributeService.GetAttributesAsync(search, page));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> Create() => View(nameof(Details), await attributeService.GetCreateFormAsync());

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public async Task<IActionResult> Details(int id)
    {
        var attribute = await attributeService.GetAttributeByIdAsync(id);
        if (attribute is null)
            return NotFound();

        return View(attribute);
    }

    [HttpGet]
    public async Task<IActionResult> Suggest(string? q, int? categoryId, bool excludeBuiltIn = false, int skip = 0)
    {
        var items = await attributeService.SuggestAsync(q, categoryId, excludeBuiltIn, skip);
        return Json(items.Select(i => new
        {
            i.Id,
            i.Name,
            DataType = UiMessages.DataType(i.DataType),
            i.CategoryId,
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public Task<IActionResult> Save(AttributeFormDto model) =>
        SaveAndRedirectAsync(
            model,
            () => attributeService.CreateAsync(model),
            () => attributeService.UpdateAsync(model),
            id => RedirectToAction(nameof(Details), new { id }),
            () => attributeService.PopulateCategoryOptionsAsync(model));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOrAdmin)]
    public Task<IActionResult> Delete(List<int>? selectedIds) =>
        BatchAndRedirectAsync(() => attributeService.DeleteManyAsync(selectedIds));
}