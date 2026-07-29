namespace CvManager.Infrastructure.Persistence;

public static class CollectionSync
{
    public static void SyncByKey<TItem, TKey>(ICollection<TItem> collection, IEnumerable<TKey> targetKeys,
        Func<TItem, TKey> keySelector, Func<TKey, TItem> createItem) where TKey : notnull
    {
        var target = targetKeys as ISet<TKey> ?? targetKeys.ToHashSet();
        var existing = collection.Select(keySelector).ToHashSet();

        foreach (var item in collection.Where(i => !target.Contains(keySelector(i))).ToList())
            collection.Remove(item);

        foreach (var key in target.Where(k => !existing.Contains(k)))
            collection.Add(createItem(key));
    }
}
