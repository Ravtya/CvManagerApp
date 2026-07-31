using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Support;
using CvManager.Infrastructure.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManager.Web.Controllers;

[Authorize]
public class SupportController(SupportService supportService) : AppController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketDto ticket, string? returnUrl)
    {
        var roles = string.Join(", ", RoleNames.All.Where(User.IsInRole));
        ticket.ReportedBy = $"{User.Identity!.Name} ({roles})";
        ticket.Link = $"{Request.Scheme}://{Request.Host}{returnUrl}";

        return await RunAndRedirectAsync(
            () => supportService.SendTicketAsync(ticket),
            () => LocalRedirect(returnUrl ?? "/"),
            _ => SuccessMessageCodes.SupportTicketCreated);
    }
}
