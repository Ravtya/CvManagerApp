using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Admin;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class AdminService(UserManager<IdentityUser> userManager, AppDbContext context)
{
    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(string? search = null, int page = 1)
    {
        var now = DateTimeOffset.UtcNow;

        var query = context.Users.AsNoTracking()
            .WhereILikeAny(search, prefix: false, u => u.Email, u => u.UserName)
            .OrderBy(u => u.Email)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? string.Empty,
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now,
                IsEmailConfirmed = u.EmailConfirmed
            });

        var pageResult = await Paging.ToPagedAsync(query, page);
        if (pageResult.Items.Count != 0)
        {
            var rolesByUserId = await LoadRolesAsync(pageResult.Items.Select(u => u.Id).ToList());
            foreach (var user in pageResult.Items)
                user.Roles = rolesByUserId[user.Id].ToList();
        }

        return pageResult;
    }

    public Task<BatchResult> ChangeRolesAsync(IEnumerable<string>? userIds, string? roleName, bool assign)
    {
        if (string.IsNullOrWhiteSpace(roleName) || !RoleNames.All.Contains(roleName))
            return Task.FromResult(BatchResult.FailCode(AdminErrorCodes.InvalidRole));

        Func<IdentityUser, string, Task<IdentityResult>> operation =
            assign ? userManager.AddToRoleAsync : userManager.RemoveFromRoleAsync;
        var errorCode = assign ? IdentityErrorCodes.UserAlreadyInRole : IdentityErrorCodes.UserNotInRole;

        return Batch.RunBatchAsync(userIds,
            ids => ProcessUsersAsync(ids, user => operation(user, roleName), errorCode));
    }

    public Task<BatchResult> SetBlockStateAsync(IEnumerable<string>? userIds, bool block) =>
        Batch.RunBatchAsync(userIds, ids => ProcessUsersAsync(ids, user =>
            userManager.SetLockoutEndDateAsync(user, block ? GetPermanentLockoutEnd() : null)));

    public Task<BatchResult> DeleteUsersAsync(IEnumerable<string>? userIds) =>
        Batch.RunBatchAsync(userIds, ids => ProcessUsersAsync(ids, userManager.DeleteAsync));

    private async Task<ILookup<string, string>> LoadRolesAsync(List<string> userIds)
    {
        var rows = await context.UserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(
                context.Roles.AsNoTracking(),
                ur => ur.RoleId,
                role => role.Id,
                (ur, role) => new { ur.UserId, role.Name })
            .ToListAsync();
        return rows.ToLookup(x => x.UserId, x => x.Name!);
    }

    private async Task<BatchResult> ProcessUsersAsync(List<string> ids,
        Func<IdentityUser, Task<IdentityResult>> operation, string? idempotentErrorCode = null)
    {
        var users = await LoadUsersByIdsAsync(ids);
        var foundById = users.ToDictionary(u => u.Id);
        var (candidates, errors) = Batch.SelectCandidates(
            ids,
            foundById,
            label: u => u.Email ?? u.UserName ?? string.Empty,
            canDelete: _ => true,
            denyCode: _ => CommonErrorCodes.NotFound);

        var usersToProcess = candidates.Select(id => foundById[id]).ToList();
        var successCount = await ExecuteOperationAsync(usersToProcess, operation, idempotentErrorCode, errors);
        return BatchResult.FromCounts(ids.Count, successCount, errors);
    }

    private async Task<List<IdentityUser>> LoadUsersByIdsAsync(List<string> ids) =>
        await userManager.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

    private static bool IsIdempotentSuccess(IdentityResult result, string? idempotentErrorCode) =>
        result.Succeeded || (idempotentErrorCode is not null && result.Errors.Any(e => e.Code == idempotentErrorCode));

    private static async Task<int> ExecuteOperationAsync(List<IdentityUser> users, Func<IdentityUser,
        Task<IdentityResult>> operation, string? idempotentErrorCode, List<ServiceError> errors)
    {
        var successCount = 0;
        foreach (var user in users)
        {
            var result = await operation(user);
            if (IsIdempotentSuccess(result, idempotentErrorCode))
                successCount++;
            else
                errors.Add(ServiceError.ItemError(user.Email ?? user.UserName ?? string.Empty,
                    AdminErrorCodes.OperationFailed));
        }

        return successCount;
    }

    private static DateTimeOffset GetPermanentLockoutEnd() => DateTimeOffset.MaxValue;
}