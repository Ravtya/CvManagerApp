using System.ComponentModel.DataAnnotations;
using CvManager.Application.Common;
using CvManager.Application.Resources;
using CvManager.Application.Validation;
using CvManager.Domain;

namespace CvManager.Web.Models.Account;

public abstract class AuthCredentialsViewModel
{
    [LocalizedRequired(nameof(ValidationResources.EmailRequired))]
    [LocalizedMaxLength(FieldLengths.Email)]
    [LocalizedRegularExpression(EmailRules.Pattern, nameof(ValidationResources.EmailInvalid))]
    [Display(Name = nameof(ValidationResources.Email), ResourceType = typeof(ValidationResources))]
    public string Email { get; set; } = string.Empty;

    [LocalizedRequired(nameof(ValidationResources.PasswordRequired))]
    [LocalizedMinLength(FieldLengths.PasswordMin)]
    [LocalizedMaxLength(FieldLengths.PasswordMax)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(ValidationResources.Password), ResourceType = typeof(ValidationResources))]
    public string Password { get; set; } = string.Empty;
}
