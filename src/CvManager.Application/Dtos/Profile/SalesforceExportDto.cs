using System.ComponentModel.DataAnnotations;
using CvManager.Application.Validation;

namespace CvManager.Application.Dtos.Profile;

public class SalesforceExportDto
{
    public string ProfileUserId { get; set; } = string.Empty;

    [LocalizedRequired]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [LocalizedRequired]
    public string LastName { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    [LocalizedRequired]
    public string AccountName { get; set; } = string.Empty;

    public string? AccountWebsite { get; set; }
    public string? AccountPhone { get; set; }
    public string? AccountIndustry { get; set; }
    public string? AccountDescription { get; set; }
}
