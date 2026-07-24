using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using CvManager.Application.Dtos.Discussion;
using CvManager.Domain.Entities;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Services;

public class DiscussionService(AppDbContext context)
{
    public async Task<IReadOnlyList<DiscussionPostDto>> GetPostsAsync(int positionId) =>
        await (
            from p in context.DiscussionPosts.AsNoTracking()
            where p.PositionId == positionId
            join u in context.Users.AsNoTracking() on p.AuthorUserId equals u.Id
            orderby p.CreatedAt, p.Id
            select new DiscussionPostDto
            {
                Id = p.Id,
                AuthorUserId = p.AuthorUserId,
                AuthorName = u.Email ?? u.UserName ?? p.AuthorUserId,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
            }).ToListAsync();

    public async Task<ServiceResult<(int Id, string Content)>> CreateAsync(
        int positionId, string authorUserId, string? content)
    {
        var text = NormalizeContent(content);
        if (text is null)
            return ServiceResult<(int, string)>.FailCode(DiscussionErrorCodes.ContentRequired);

        var post = new DiscussionPost
        {
            PositionId = positionId,
            AuthorUserId = authorUserId,
            Content = text,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        context.DiscussionPosts.Add(post);
        return await EfSave.TrySaveAsync(context, () => (post.Id, post.Content));
    }

    public async Task<ServiceResult<(int Id, string Content)>> UpdateAsync(
        int postId, string userId, bool isAdmin, string? content)
    {
        var (post, error) = await TryGetManagedAsync(postId, userId, isAdmin);
        if (error is not null)
            return ServiceResult<(int, string)>.FailCode(error);

        var text = NormalizeContent(content);
        if (text is null)
            return ServiceResult<(int, string)>.FailCode(DiscussionErrorCodes.ContentRequired);

        post!.Content = text;
        return await EfSave.TrySaveAsync(context, () => (post.Id, post.Content));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int postId, string userId, bool isAdmin)
    {
        var (post, error) = await TryGetManagedAsync(postId, userId, isAdmin);
        if (error is not null)
            return ServiceResult<bool>.FailCode(error);

        context.DiscussionPosts.Remove(post!);
        return await EfSave.TrySaveAsync(context, () => true);
    }

    private async Task<(DiscussionPost? Post, string? Error)> TryGetManagedAsync(
        int postId, string userId, bool isAdmin)
    {
        var post = await context.DiscussionPosts.FindAsync(postId);
        if (post is null)
            return (null, CommonErrorCodes.NotFound);
        if (!isAdmin && post.AuthorUserId != userId)
            return (null, DiscussionErrorCodes.NotAllowed);
        return (post, null);
    }

    private static string? NormalizeContent(string? content)
    {
        var text = (content ?? string.Empty).Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }
}