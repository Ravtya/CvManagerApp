namespace CvManager.Infrastructure.Salesforce;

public class SalesforceOptions
{
    public const string SectionName = "Salesforce";

    public string TokenUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
