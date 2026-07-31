namespace CvManager.Infrastructure.Support;

public class SupportOptions
{
    public const string SectionName = "Support";
    
    public List<string> AdminEmails { get; set; } = [];
    public string AccessToken { get; set; } = string.Empty;
}