using System.Text;
using System.Text.Json;
using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Support;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;

namespace CvManager.Infrastructure.Support;

public class SupportService(IOptions<SupportOptions> options)
{
    private readonly SupportOptions _options = options.Value;

    public async Task<ServiceResult<string>> SendTicketAsync(TicketDto ticket)
    {
        try
        {
            ticket.AdminEmails = _options.AdminEmails;
            var json = JsonSerializer.Serialize(ticket);
            var path = $"/ticket-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}.json";
            using var client = new DropboxClient(_options.RefreshToken,  _options.AppKey, _options.AppSecret);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await client.Files.UploadAsync(path, WriteMode.Add.Instance, body: stream);
            return ServiceResult<string>.Ok(path);
        }
        catch (Exception)
        {
            return ServiceResult<string>.FailCode(SupportErrorCodes.SendFailed);
        }
    }
}
