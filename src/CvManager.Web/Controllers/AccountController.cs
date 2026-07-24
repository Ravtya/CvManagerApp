using System.Security.Claims;
using System.Text;
using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Resources;
using CvManager.Infrastructure.Services;
using CvManager.Web.Models.Account;
using CvManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace CvManager.Web.Controllers;

[Authorize]
public class AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
    AccountService accountService, EmailService emailService) : AppController
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => AuthView(returnUrl);

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null) => AuthView(returnUrl);

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        returnUrl = GetSafeReturnUrl(returnUrl);
        return ChallengeExternalLogin(provider, returnUrl);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return AuthView(returnUrl, model);

        var signInResult = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);
        if (!signInResult.Succeeded)
        {
            Error(LoginFailureCode(signInResult));
            if (signInResult.IsNotAllowed)
                TempData["ResendEmail"] = model.Email;
            return AuthView(returnUrl, model);
        }

        return LocalRedirect(GetSafeReturnUrl(returnUrl));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return AuthView(returnUrl, model);

        var newUser = AccountService.CreateUser(model.Email);

        var registerResult = await accountService.RegisterWithPasswordAsync(newUser, model.Password);
        if (!registerResult.Succeeded)
        {
            ShowIdentityErrors(registerResult);
            return AuthView(returnUrl, model);
        }

        await emailService.SendAsync(newUser, HttpContext);
        Success(SuccessMessageCodes.ConfirmationSent);
        TempData["ResendEmail"] = model.Email;
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            return InvalidConfirmationLink();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return InvalidConfirmationLink();

        string decodedCode;
        try
        {
            decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return InvalidConfirmationLink();
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedCode);
        if (!result.Succeeded)
            return InvalidConfirmationLink();

        Success(SuccessMessageCodes.EmailConfirmed);
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        if (!EmailRules.IsValid(email))
            return RedirectToAction(nameof(Login));

        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
            await emailService.SendAsync(user, HttpContext);

        Success(SuccessMessageCodes.ConfirmationSent);
        TempData["ResendEmail"] = email;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = (await userManager.GetUserAsync(User))!;
        var changePassword = new ChangePasswordViewModel { HasPassword = await userManager.HasPasswordAsync(user) };
        return SettingsView(user, changePassword);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        [Bind(Prefix = nameof(AccountSettingsViewModel.ChangePassword))]
        ChangePasswordViewModel model)
    {
        var user = (await userManager.GetUserAsync(User))!;

        model.HasPassword = await userManager.HasPasswordAsync(user);
        ValidateCurrentPasswordRequired(model);

        if (!ModelState.IsValid)
            return SettingsView(user, model);

        var updatePasswordResult = await UpdatePasswordAsync(user, model);
        if (!updatePasswordResult.Succeeded)
        {
            ShowIdentityErrors(updatePasswordResult);
            return SettingsView(user, model);
        }

        return await PasswordChangedAsync(user, model.HasPassword);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalSignInFailed);

        var externalSignInResult =
            await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (externalSignInResult.Succeeded)
            return LocalRedirect(GetSafeReturnUrl(returnUrl));

        if (externalSignInResult.IsLockedOut)
            return RedirectToLogin(returnUrl, AuthErrorCodes.AccountLockedOut);

        if (externalSignInResult.IsNotAllowed)
        {
            var existing = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existing is null)
                return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalSignInFailed);

            return await ConfirmEmailFromExternalAndSignInAsync(existing, returnUrl);
        }

        return await LinkOrRegisterExternalAsync(info, returnUrl);
    }

    private async Task<IActionResult> LinkOrRegisterExternalAsync(ExternalLoginInfo info, string? returnUrl)
    {
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is null)
            return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalEmailMissing);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return await RegisterExternalUserAsync(email, info, returnUrl);

        return await LinkExternalLoginAsync(user, info, returnUrl);
    }

    private async Task<IActionResult> RegisterExternalUserAsync(string email, ExternalLoginInfo info, string? returnUrl)
    {
        var user = AccountService.CreateUser(email, emailConfirmed: true);
        var registerResult = await accountService.RegisterWithExternalLoginAsync(user, info);
        if (!registerResult.Succeeded)
            return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalAccountCreateFailed);

        return await SignInUserAsync(user, returnUrl);
    }

    private async Task<IActionResult> LinkExternalLoginAsync(IdentityUser user, ExternalLoginInfo info,
        string? returnUrl)
    {
        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
            return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalAccountLinkFailed);

        return await ConfirmEmailFromExternalAndSignInAsync(user, returnUrl);
    }

    private async Task<IActionResult> ConfirmEmailFromExternalAndSignInAsync(IdentityUser user, string? returnUrl)
    {
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return RedirectToLogin(returnUrl, AuthErrorCodes.ExternalAccountLinkFailed);
        }

        return await SignInUserAsync(user, returnUrl);
    }

    private ChallengeResult ChallengeExternalLogin(string provider, string returnUrl)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    private void ValidateCurrentPasswordRequired(ChangePasswordViewModel model)
    {
        if (model.HasPassword && string.IsNullOrWhiteSpace(model.CurrentPassword))
            ModelState.AddModelError(
                $"{nameof(AccountSettingsViewModel.ChangePassword)}.{nameof(ChangePasswordViewModel.CurrentPassword)}",
                ValidationResources.CurrentPasswordRequired);
    }

    private async Task<IActionResult> PasswordChangedAsync(IdentityUser user, bool hadPassword)
    {
        await signInManager.RefreshSignInAsync(user);
        Success(hadPassword ? SuccessMessageCodes.PasswordChanged : SuccessMessageCodes.PasswordSet);
        return RedirectToAction(nameof(Settings));
    }

    private static string LoginFailureCode(SignInResult result) =>
        result.IsLockedOut
            ? AuthErrorCodes.AccountLockedOut
            : result.IsNotAllowed
                ? AuthErrorCodes.EmailNotConfirmed
                : AuthErrorCodes.IncorrectEmailOrPassword;

    private void ShowIdentityErrors(IdentityResult result) => Errors(result.Errors.Select(e => e.Description).ToList());

    private Task<IdentityResult> UpdatePasswordAsync(IdentityUser user, ChangePasswordViewModel model) =>
        model.HasPassword
            ? userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword)
            : userManager.AddPasswordAsync(user, model.NewPassword);

    private static AccountSettingsViewModel SettingsModel(IdentityUser user, ChangePasswordViewModel changePassword) =>
        new()
        {
            Email = user.Email ?? user.UserName ?? string.Empty,
            ChangePassword = changePassword
        };

    private ViewResult SettingsView(IdentityUser user, ChangePasswordViewModel changePassword) =>
        View(nameof(Settings), SettingsModel(user, changePassword));

    private ViewResult AuthView(string? returnUrl, object? model = null)
    {
        ViewData["ReturnUrl"] = GetSafeReturnUrl(returnUrl);
        return model is null ? View() : View(model);
    }

    private async Task<IActionResult> SignInUserAsync(IdentityUser user, string? returnUrl = null)
    {
        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(GetSafeReturnUrl(returnUrl));
    }

    private RedirectToActionResult InvalidConfirmationLink() =>
        RedirectToLogin(null, AuthErrorCodes.InvalidConfirmationLink);

    private RedirectToActionResult RedirectToLogin(string? returnUrl, string? errorKey = null)
    {
        if (errorKey is not null)
            Error(errorKey);

        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    private string GetSafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}