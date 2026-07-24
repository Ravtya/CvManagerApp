using System.Security.Claims;
using CvManager.Application.Common;

namespace CvManager.Web.Extensions;

public static class PrincipalExtensions
{
    public static bool CanManageRecruiting(this ClaimsPrincipal user) =>
        user.IsInRole(RoleNames.Administrator) || user.IsInRole(RoleNames.Recruiter);

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(RoleNames.Administrator);

    public static bool IsRecruiter(this ClaimsPrincipal user) =>
        user.IsInRole(RoleNames.Recruiter);
}
