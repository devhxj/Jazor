using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeMaterialIndexVersionPersistence
{
    public static async Task<int> RequeueOutdatedAsync(
        ISqlSugarClient db,
        string configuredModel,
        int expectedDimensions,
        int limit,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (string.IsNullOrWhiteSpace(configuredModel))
            throw new ArgumentException("病例向量配置模型不能为空", nameof(configuredModel));
        if (expectedDimensions <= 0)
            throw new ArgumentOutOfRangeException(nameof(expectedDimensions));
        var batchSize = Math.Clamp(limit, 1, 500);
        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var candidates = await db.Queryable<SaveLifeAgentVectorIndex, SaveLifeEnrollmentMaterial>(
                    (index, material) => index.ResourceId == material.Id)
                .Where((index, material) =>
                    !index.IsDelete &&
                    !material.IsDelete &&
                    index.ResourceType == "material" &&
                    index.SyncStatus == "synced" &&
                    (index.EmbeddingKind == null ||
                        index.EmbeddingKind != "multimodal" ||
                        index.EmbeddingConfiguredModel == null ||
                        index.EmbeddingConfiguredModel != configuredModel ||
                        index.EmbeddingDimensions == null ||
                        index.EmbeddingDimensions != expectedDimensions ||
                        index.SourceContentSha256 == null ||
                        material.ContentSha256 == null ||
                        index.SourceContentSha256 != material.ContentSha256))
                .OrderBy((index, material) => index.IndexedTime, OrderByType.Asc)
                .OrderBy((index, material) => index.CreateTime, OrderByType.Asc)
                .Take(batchSize)
                .Select((index, material) => new SaveLifeMaterialIndexVersionCandidate
                {
                    IndexId = index.Id,
                    MaterialId = material.Id
                })
                .ToListAsync(cancellationToken);
            if (candidates.Count == 0)
                return 0;

            var materialIds = candidates.Select(item => item.MaterialId).Distinct().ToList();
            var materialCount = await db.Updateable<SaveLifeEnrollmentMaterial>()
                .SetColumns(item => new SaveLifeEnrollmentMaterial
                {
                    IndexStatus = "pending",
                    IndexError = null,
                    ProcessRetryCount = 0,
                    NextProcessRetryTime = null,
                    ProcessingRevision = item.ProcessingRevision + 1,
                    ProcessingLeaseOwner = null,
                    ProcessingLeaseExpiresTime = null,
                    UpdateTime = now
                })
                .Where(item => materialIds.Contains(item.Id) && !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (materialCount != materialIds.Count)
                throw new InvalidOperationException("病例向量重排时材料状态发生变化");

            var indexIds = candidates.Select(item => item.IndexId).Distinct().ToList();
            var indexCount = await db.Updateable<SaveLifeAgentVectorIndex>()
                .SetColumns(item => new SaveLifeAgentVectorIndex
                {
                    SyncStatus = "pending",
                    SyncError = null,
                    RetryCount = 0,
                    UpdateTime = now
                })
                .Where(item => indexIds.Contains(item.Id) && !item.IsDelete && item.SyncStatus == "synced")
                .ExecuteCommandAsync(cancellationToken);
            if (indexCount != indexIds.Count)
                throw new InvalidOperationException("病例向量重排时索引状态发生变化");

            return materialIds.Count;
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "病例向量版本重排事务失败");
        return transaction.Data;
    }

    [DoesNotReturn]
    private static void Rethrow(Exception? error, string fallback)
    {
        if (error == null)
            throw new InvalidOperationException(fallback);
        ExceptionDispatchInfo.Capture(error).Throw();
        throw new InvalidOperationException(fallback);
    }

    private sealed class SaveLifeMaterialIndexVersionCandidate
    {
        public long IndexId { get; set; }

        public long MaterialId { get; set; }
    }
}
