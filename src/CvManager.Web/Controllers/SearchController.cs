using CvManager.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[AllowAnonymous]
public class SearchController(SearchService searchService) : AppController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? query) =>
        View(await searchService.SearchAsync(query, CurrentViewer()));
}