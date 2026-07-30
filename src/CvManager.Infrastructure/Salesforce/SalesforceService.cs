using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Profile;
using Microsoft.Extensions.Options;
using NetCoreForce.Client;
using NetCoreForce.Models;

namespace CvManager.Infrastructure.Salesforce;

public class SalesforceService(IOptions<SalesforceOptions> options)
{
    private readonly SalesforceOptions _options = options.Value;
    private const string ExternalFieldName = "CvManagerUserId__c";

    public async Task<ServiceResult<bool>> ExportAsync(SalesforceExportDto form)
    {
        try
        {
            var client = await CreateClientAsync();
            var accountId = await UpsertAccount(form, client);
            await UpsertContactAsync(form, accountId, client);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception)
        {
            return ServiceResult<bool>.FailCode(SalesforceErrorCodes.ExportFailed);
        }
    }

    private static async Task UpsertContactAsync(SalesforceExportDto form, string accountId, ForceClient client)
    {
        var contact = new SfContact
        {
            AccountId = accountId,
            FirstName = form.FirstName,
            LastName = form.LastName,
            Email = form.Email,
            Phone = form.Phone,
            Title = form.Title,
            Description = form.Description
        };
        await client.InsertOrUpdateRecord(SfContact.SObjectTypeName, ExternalFieldName, form.ProfileUserId, contact);
    }

    private static async Task<string> UpsertAccount(SalesforceExportDto form, ForceClient client)
    {
        var account = new SfAccount
        {
            Name = form.AccountName,
            Website = form.AccountWebsite,
            Phone = form.AccountPhone,
            Industry = form.AccountIndustry,
            Description = form.AccountDescription,
        };
        var result = await client.InsertOrUpdateRecord(SfAccount.SObjectTypeName, ExternalFieldName, form.ProfileUserId,
            account);
        return result.Id;
    }

    private async Task<ForceClient> CreateClientAsync()
    {
        var auth = new AuthenticationClient();
        await auth.ClientCredentialsAsync(_options.ClientId, _options.ClientSecret, _options.TokenUrl);
        return new ForceClient(auth.AccessInfo.InstanceUrl, auth.ApiVersion, auth.AccessInfo.AccessToken);
    }
}