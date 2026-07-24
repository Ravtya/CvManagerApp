using CvManager.Application.Common;
using CvManager.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace CvManager.Infrastructure.Services;

public class AccountService(AppDbContext context, UserManager<IdentityUser> userManager, ProfileService profileService)
{
    public Task<IdentityResult> RegisterWithPasswordAsync(IdentityUser user, string password) =>
        RegisterUserAsync(user, password, login: null);

    public Task<IdentityResult> RegisterWithExternalLoginAsync(IdentityUser user, UserLoginInfo login) =>
        RegisterUserAsync(user, password: null, login);

    public static IdentityUser CreateUser(string email, bool emailConfirmed = false) =>
        new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            LockoutEnabled = true
        };

    private async Task<IdentityResult> RegisterUserAsync(IdentityUser user, string? password, UserLoginInfo? login)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var create = password is null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            return create;

        var role = await userManager.AddToRoleAsync(user, RoleNames.Candidate);
        if (!role.Succeeded)
            return role;

        if (login is not null)
        {
            var link = await userManager.AddLoginAsync(user, login);
            if (!link.Succeeded)
                return link;
        }

        await profileService.CreateProfileAsync(user.Id);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return IdentityResult.Success;
    }
}
