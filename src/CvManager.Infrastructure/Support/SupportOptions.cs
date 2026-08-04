namespace CvManager.Infrastructure.Support;

public class SupportOptions
{
    public const string SectionName = "Support";
    
    public List<string> AdminEmails { get; set; } = [];
    public string AppKey { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}