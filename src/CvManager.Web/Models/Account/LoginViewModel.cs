using System.ComponentModel.DataAnnotations;
using CvManager.Application.Resources;

namespace CvManager.Web.Models.Account;

public class LoginViewModel : AuthCredentialsViewModel
{
    [Display(Name = nameof(ValidationResources.RememberMe), ResourceType = typeof(ValidationResources))]
    public bool RememberMe { get; set; }
}
