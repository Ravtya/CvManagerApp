using System.Text.Json;
using CvManager.Application.Common;
using CvManager.Application.Dtos.Support;
using Microsoft.Extensions.Options;

namespace CvManager.Infrastructure.Support;

public class SupportService(IOptions<SupportOptions> options)
{
    private readonly SupportOptions _options = options.Value;
    private const string Path = "C:\\Projects\\CourseProject\\Tickets";

    public async Task<ServiceResult<string>> CreateTicketAsync(TicketDto ticket)
    {
        ticket.AdminEmails = _options.AdminEmails;
        var json = JsonSerializer.Serialize(ticket);
        Directory.CreateDirectory(Path);
        var fileName = $"ticket-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N[..8]}.json";
        var filePath = System.IO.Path.Combine(Path, fileName);
        await File.WriteAllTextAsync(filePath, json);
        return ServiceResult<string>.Ok(filePath);
    }
}