namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAtomicUpsert
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T?>> loadExisting,
        Func<T> createNew,
        Action<T> applyChanges,
        Func<T, Task> insert,
        Func<T, Task> update)
        where T : class
    {
        var existing = await loadExisting();
        if (existing != null)
        {
            applyChanges(existing);
            await update(existing);
            return existing;
        }

        var candidate = createNew();
        applyChanges(candidate);
        try
        {
            await insert(candidate);
            return candidate;
        }
        catch
        {
            var winner = await loadExisting();
            if (winner == null)
                throw;

            applyChanges(winner);
            await update(winner);
            return winner;
        }
    }
}
