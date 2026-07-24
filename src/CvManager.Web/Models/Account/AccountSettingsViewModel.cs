namespace CvManager.Web.Models.Account;

public class AccountSettingsViewModel
{
    public string Email { get; set; } = string.Empty;
    public ChangePasswordViewModel ChangePassword { get; set; } = new();
}
