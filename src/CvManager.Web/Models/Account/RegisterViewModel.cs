using System.ComponentModel.DataAnnotations;
using CvManager.Application.Resources;
using CvManager.Application.Validation;

namespace CvManager.Web.Models.Account;

public class RegisterViewModel : AuthCredentialsViewModel
{
    [LocalizedRequired(nameof(ValidationResources.ConfirmPasswordRequired))]
    [DataType(DataType.Password)]
    [LocalizedCompare(nameof(Password))]
    [Display(Name = nameof(ValidationResources.ConfirmPassword), ResourceType = typeof(ValidationResources))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
