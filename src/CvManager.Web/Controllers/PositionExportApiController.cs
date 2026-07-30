using CvManager.Application.Dtos.Positions;
using CvManager.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/positions/export")]
public class PositionExportApiController(PositionExportService exportService) : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<ActionResult<PositionExportDto>> Get(string token)
    {
        var result = await exportService.GetByTokenAsync(token);
        return result is null ? NotFound() : result;
    }
}