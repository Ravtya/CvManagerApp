using System.Text.Json.Serialization;
using CvManager.Application.Validation;

namespace CvManager.Application.Dtos.Support;

public class TicketDto
{
    public string ReportedBy { get; set; } =  string.Empty;
    public string Position { get; set; } =  string.Empty;
    public string Link { get; set; } =  string.Empty;
    
    [LocalizedRequired]
    public string Summary { get; set; } =  string.Empty;
    
    public PriorityType Priority { get; set; } = PriorityType.Low;
    public List<string> AdminEmails { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PriorityType
{
    Low,
    Average,
    High
}