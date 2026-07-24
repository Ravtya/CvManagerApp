using System.Text;
using System.Text.Encodings.Web;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Web.Options;
using CvManager.Web.Controllers;
using CvManager.Web.Ui;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CvManager.Web.Services;

public class EmailService(UserManager<IdentityUser> userManager, LinkGenerator linkGenerator,
    IOptions<SendGridOptions> sendGridOptions)
{
    public async Task SendAsync(IdentityUser user, HttpContext httpContext)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var url = linkGenerator.GetUriByAction(httpContext, nameof(AccountController.ConfirmEmail), "Account",
            new { userId = user.Id, code })!;
        var opts = sendGridOptions.Value;
        var subject = UiMessages.Text(EmailMessageCodes.ConfirmationSubject);
        var html = string.Format(UiMessages.Text(EmailMessageCodes.ConfirmationBody), HtmlEncoder.Default.Encode(url));

        _ = Task.Run(() => new SendGridClient(opts.ApiKey).SendEmailAsync(
            MailHelper.CreateSingleEmail(new EmailAddress(opts.FromEmail, opts.FromName),
                new EmailAddress(user.Email!), subject, plainTextContent: null, html)));
    }
}