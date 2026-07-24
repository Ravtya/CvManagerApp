using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Persistence;

public static class Batch
{
    public static Task<BatchResult> RunBatchAsync<T>(IEnumerable<T>? ids, Func<List<T>, Task<BatchResult>> run)
    {
        var idList = ids?.Distinct().ToList() ?? [];
        return idList.Count == 0
            ? Task.FromResult(BatchResult.FailCode(CommonErrorCodes.NothingSelected))
            : run(idList);
    }

    public static (List<TKey> Candidates, List<ServiceError> Errors) SelectCandidates<TKey, T>(
        IReadOnlyList<TKey> idList,
        IReadOnlyDictionary<TKey, T> foundById,
        Func<T, string> label,
        Func<T, bool> canDelete,
        Func<T, string> denyCode) where TKey : notnull
    {
        var errors = new List<ServiceError>();
        var candidates = new List<TKey>();

        foreach (var id in idList)
        {
            if (!foundById.TryGetValue(id, out var item))
            {
                errors.Add(ServiceError.ItemError(id.ToString()!, CommonErrorCodes.NotFound));
                continue;
            }

            if (!canDelete(item))
            {
                errors.Add(ServiceError.ItemError(label(item), denyCode(item)));
                continue;
            }

            candidates.Add(id);
        }

        return (candidates, errors);
    }

    public static Task<BatchResult> RunExecuteDeleteAsync<TKey, TItem>(
        IEnumerable<TKey>? ids,
        Func<List<TKey>, Task<Dictionary<TKey, TItem>>> loadById,
        Func<TItem, string> label,
        Func<TItem, bool> canDelete,
        Func<TItem, string> denyCode,
        Func<List<TKey>, Task> executeDelete) where TKey : notnull =>
        RunBatchAsync(ids, async idList =>
        {
            var foundById = await loadById(idList);
            var (candidates, errors) = SelectCandidates(idList, foundById, label, canDelete, denyCode);

            try
            {
                if (candidates.Count > 0)
                    await executeDelete(candidates);
            }
            catch (DbUpdateException)
            {
                return BatchResult.FromCounts(idList.Count, 0,
                    [..errors, ServiceError.CodeOnly(CommonErrorCodes.InUse)]);
            }

            return BatchResult.FromCounts(idList.Count, candidates.Count, errors);
        });

    public static Task ExecuteDeleteByIdsAsync<TEntity>(DbSet<TEntity> set, List<int> ids)
        where TEntity : class =>
        set.Where(e => ids.Contains(EF.Property<int>(e, "Id"))).ExecuteDeleteAsync();
}
