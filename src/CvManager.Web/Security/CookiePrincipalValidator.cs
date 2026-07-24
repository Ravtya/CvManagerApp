using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace CvManager.Web.Security;

public static class CookiePrincipalValidator
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
        var user = context.Principal is null ? null : await userManager.GetUserAsync(context.Principal);

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        var cookieRoles = context.Principal!.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);
        if (cookieRoles.SetEquals(await userManager.GetRolesAsync(user)))
            return;

        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
        context.ReplacePrincipal(await signInManager.CreateUserPrincipalAsync(user));
        context.ShouldRenew = true;
    }
}