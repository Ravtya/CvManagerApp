using System.Diagnostics;
using CvManager.Infrastructure.Services;
using CvManager.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

public class HomeController(PositionService positionService) : AppController
{
    public async Task<IActionResult> Index() => View(await positionService.GetHomePageAsync(CurrentViewer()));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
        { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}