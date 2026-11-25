using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeAgentActiveConversationPersistence
{
    private const int MaximumCreateAttempts = 3;

    public static async Task<SaveLifeAgentConversation?> FindActiveAsync(
        ISqlSugarClient db,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var normalizedUserId = NormalizeExternalUserId(externalUserId);
        var rows = await db.Queryable<SaveLifeAgentConversation>()
            .ClearFilter()
            .Where(item =>
                item.ExternalUserId == normalizedUserId &&
                item.Status == "active" &&
                !item.IsDelete)
            .OrderBy(item => item.UpdateTime, OrderByType.Desc)
            .OrderBy(item => item.CreateTime, OrderByType.Desc)
            .OrderBy(item => item.Id, OrderByType.Desc)
            .Take(1)
            .ToListAsync(cancellationToken);
        return rows.FirstOrDefault();
    }

    public static async Task<SaveLifeAgentConversation> CreateOrGetActiveAsync(
        ISqlSugarClient db,
        SaveLifeAgentConversation candidate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(candidate);
        candidate.ExternalUserId = NormalizeExternalUserId(candidate.ExternalUserId);
        if (!string.Equals(candidate.Status, "active", StringComparison.Ordinal))
            throw new ArgumentException("候选会话必须处于 active 状态", nameof(candidate));

        for (var attempt = 0; attempt < MaximumCreateAttempts; attempt++)
        {
            var existing = await FindActiveAsync(
                db,
                candidate.ExternalUserId,
                cancellationToken);
            if (existing != null)
                return existing;

            try
            {
                var inserted = await db.Insertable(candidate).ExecuteCommandAsync(cancellationToken);
                if (inserted != 1)
                    throw new InvalidOperationException("活跃会话创建未写入预期记录");

                return candidate;
            }
            catch (Exception exception) when (IsActiveAccountUniqueViolation(exception))
            {
                existing = await FindActiveAsync(
                    db,
                    candidate.ExternalUserId,
                    cancellationToken);
                if (existing != null)
                    return existing;
            }
        }

        throw new InvalidOperationException("活跃会话状态持续变化，请重试");
    }

    private static bool IsActiveAccountUniqueViolation(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
        {
            if (current.Message.Contains(
                    SaveLifeAgentConversationMigration.ActiveAccountUniqueIndexName,
                    StringComparison.OrdinalIgnoreCase) &&
                current.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeExternalUserId(string externalUserId)
    {
        var normalized = externalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("账号标识不能为空", nameof(externalUserId));

        return normalized;
    }
}
