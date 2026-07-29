using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Profile;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Attributes;
using CvManager.Infrastructure.Persistence;
using CvManager.Infrastructure.Profile;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class ProfileService(AppDbContext context)
{
    public async Task<ProfileDto?> GetProfileAsync(string profileUserId)
    {
        var profile = await LoadProfileAsync(profileUserId);
        return profile is null ? null : await MapProfileAsync(profile);
    }

    public async Task CreateProfileAsync(string userId)
    {
        var profile = new UserProfile { UserId = userId };
        await ProfileBuiltInAttributes.AddBuiltInValuesToProfileAsync(context, profile);
        context.UserProfiles.Add(profile);
    }

    public async Task<ProfileAttributeFieldDto?> GetInfoFieldTemplateAsync(int definitionId)
    {
        var attribute = await AttributeLookups.GetNonBuiltInDefinitionAsync(context, definitionId);
        return attribute is null ? null : ProfileAttributes.MapField(attribute);
    }

    public async Task<ServiceResult<uint>> SaveProfileAsync(ProfileDto model)
    {
        var profile = await LoadProfileForUpdateAsync(model.ProfileUserId);
        if (profile is null)
            return ServiceResult<uint>.FailCode(CommonErrorCodes.NotFound);

        if (EfSave.IsRowVersionMismatch(profile.RowVersion, model.RowVersion))
            return ServiceResult<uint>.FailCode(CommonErrorCodes.ConcurrencyConflict);

        await ApplyChangesAsync(profile, model);

        EfSave.SetRowVersion(context, profile, model.RowVersion, e => e.UserId);
        return await EfSave.TrySaveAsync(context, () => profile.RowVersion);
    }

    private async Task<ProfileDto> MapProfileAsync(UserProfile profile)
    {
        var (meFields, infoFields) = MapAttributeFields(profile);
        var categories = await AttributeLookups.GetCategoriesAsync(context);
        var projects = await ProjectSync.GetForProfileAsync(context, profile.Id);
        var profileEmail = await GetUserEmail(profile);

        return new ProfileDto
        {
            ProfileUserId = profile.UserId,
            ProfileEmail = profileEmail,
            RowVersion = profile.RowVersion,
            MeFieldsById = meFields,
            InfoFieldsById = infoFields,
            AttributeCategories = categories,
            Projects = projects,
        };
    }

    private async Task<string> GetUserEmail(UserProfile profile)
    {
        return await context.Users.AsNoTracking()
            .Where(u => u.Id == profile.UserId)
            .Select(u => u.Email ?? u.UserName ?? string.Empty)
            .FirstOrDefaultAsync() ?? string.Empty;
    }

    private static (Dictionary<int, ProfileAttributeFieldDto> Me, Dictionary<int, ProfileAttributeFieldDto> Info)
        MapAttributeFields(UserProfile profile)
    {
        var me = new Dictionary<int, ProfileAttributeFieldDto>();
        var info = new Dictionary<int, ProfileAttributeFieldDto>();

        foreach (var value in profile.AttributeValues)
        {
            var field = ProfileAttributes.MapField(value.Attribute, value);
            if (value.Attribute.IsBuiltIn)
                me[field.AttributeDefinitionId] = field;
            else
                info[field.AttributeDefinitionId] = field;
        }

        return (me, info);
    }

    private async Task ApplyChangesAsync(UserProfile profile, ProfileDto model)
    {
        RemoveInfoValues(profile, model.RemoveInfoValueIds);
        ProfileAttributes.Upsert(profile, WithDefinitionIds(model.MeFieldsById));
        ProfileAttributes.Upsert(profile, WithDefinitionIds(model.InfoFieldsById));
        await ProjectSync.SyncAsync(context, profile.Id, model.Projects, model.RemoveProjectIds);
    }

    private static IEnumerable<ProfileAttributeFieldDto> WithDefinitionIds(Dictionary<int, ProfileAttributeFieldDto> fields)
    {
        foreach (var (id, field) in fields)
        {
            field.AttributeDefinitionId = id;
            yield return field;
        }
    }

    private static void RemoveInfoValues(UserProfile profile, IReadOnlyList<int> valueIds)
    {
        var toRemove = profile.AttributeValues
            .Where(v => !v.Attribute.IsBuiltIn && valueIds.Contains(v.Id))
            .ToList();

        foreach (var value in toRemove)
            profile.AttributeValues.Remove(value);
    }

    private Task<UserProfile?> LoadProfileForUpdateAsync(string profileUserId) =>
        context.UserProfiles
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(p => p.UserId == profileUserId);

    private Task<UserProfile?> LoadProfileAsync(string profileUserId) =>
        context.UserProfiles
            .AsNoTracking()
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .ThenInclude(a => a.Category)
            .Include(p => p.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .ThenInclude(a => a.Options)
            .FirstOrDefaultAsync(p => p.UserId == profileUserId);
}