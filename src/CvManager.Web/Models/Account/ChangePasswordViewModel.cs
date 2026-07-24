using System.ComponentModel.DataAnnotations;
using CvManager.Application.Resources;
using CvManager.Application.Validation;
using CvManager.Domain;

namespace CvManager.Web.Models.Account;

public class ChangePasswordViewModel
{
    public bool HasPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = nameof(ValidationResources.CurrentPassword), ResourceType = typeof(ValidationResources))]
    public string? CurrentPassword { get; set; }

    [LocalizedRequired(nameof(ValidationResources.PasswordRequired))]
    [LocalizedMinLength(FieldLengths.PasswordMin)]
    [LocalizedMaxLength(FieldLengths.PasswordMax)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(ValidationResources.NewPassword), ResourceType = typeof(ValidationResources))]
    public string NewPassword { get; set; } = string.Empty;

    [LocalizedRequired(nameof(ValidationResources.ConfirmPasswordRequired))]
    [DataType(DataType.Password)]
    [LocalizedCompare(nameof(NewPassword))]
    [Display(Name = nameof(ValidationResources.ConfirmPassword), ResourceType = typeof(ValidationResources))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
