using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeMaterialProcessingPersistence
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);

    public static async Task<SaveLifeMaterialProcessingLease?> TryClaimAsync(
        ISqlSugarClient db,
        long materialId,
        DateTime now,
        Func<string> ownerGenerator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(ownerGenerator);
        var owner = ownerGenerator();
        if (string.IsNullOrWhiteSpace(owner) || owner.Length > 64)
            throw new ArgumentException("材料处理租约所有者无效", nameof(ownerGenerator));
        var expires = now.Add(LeaseDuration);
        var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
            .SetColumns(item => new SaveLifeEnrollmentMaterial
            {
                ExtractStatus = "processing",
                ExtractError = null,
                IndexStatus = "pending",
                IndexError = null,
                NextProcessRetryTime = null,
                ProcessingLeaseOwner = owner,
                ProcessingLeaseExpiresTime = expires,
                UpdateTime = now
            })
            .Where(item => item.Id == materialId && !item.IsDelete && item.ProcessingRevision > 0)
            .Where(BuildProcessablePredicate(now))
            .ExecuteCommandAsync(cancellationToken);
        if (affected != 1)
            return null;

        var material = await db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(item =>
                item.Id == materialId &&
                !item.IsDelete &&
                item.ProcessingLeaseOwner == owner &&
                item.ProcessingLeaseExpiresTime == expires)
            .FirstAsync(cancellationToken);
        return material == null ? null : SaveLifeMaterialProcessingLease.FromMaterial(material, owner, expires);
    }

    public static Expression<Func<SaveLifeEnrollmentMaterial, bool>> BuildProcessablePredicate(DateTime now)
    {
        return item =>
            (item.ProcessingLeaseOwner == null || item.ProcessingLeaseOwner == string.Empty ||
                item.ProcessingLeaseExpiresTime == null || item.ProcessingLeaseExpiresTime <= now) &&
            (item.ExtractStatus == "pending" ||
                item.IndexStatus == "pending" ||
                ((item.ExtractStatus == "failed" || item.IndexStatus == "failed") &&
                    (item.NextProcessRetryTime == null || item.NextProcessRetryTime <= now)) ||
                (item.IndexStatus == "skipped" &&
                    (item.IndexError == SaveLifeEnrollmentMaterialHelper.MissingMaterialEmbeddingConfigurationMessage ||
                        item.IndexError == SaveLifeEnrollmentMaterialHelper.MissingTextEmbeddingConfigurationMessage) &&
                    (item.NextProcessRetryTime == null || item.NextProcessRetryTime <= now)) ||
                item.ExtractStatus == "processing");
    }

    public static async Task<bool> IsCurrentAsync(
        ISqlSugarClient db,
        SaveLifeMaterialProcessingLease lease,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await CurrentQuery(db, lease, now).AnyAsync(cancellationToken);
    }

    public static async Task<SaveLifeMaterialProcessingLease?> TrySetContentSha256Async(
        ISqlSugarClient db,
        SaveLifeMaterialProcessingLease lease,
        string contentSha256,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(lease);
        var normalized = contentSha256.Trim().ToUpperInvariant();
        if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
            throw new ArgumentException("材料内容摘要必须是 64 位十六进制 SHA-256", nameof(contentSha256));

        if (!string.IsNullOrWhiteSpace(lease.ContentSha256))
        {
            return string.Equals(lease.ContentSha256, normalized, StringComparison.OrdinalIgnoreCase) &&
                await IsCurrentAsync(db, lease, now, cancellationToken)
                ? lease
                : null;
        }

        var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
            .SetColumns(item => new SaveLifeEnrollmentMaterial
            {
                ContentSha256 = normalized,
                UpdateTime = now
            })
            .Where(item =>
                item.Id == lease.MaterialId &&
                !item.IsDelete &&
                item.ProcessingRevision == lease.Revision &&
                item.ProcessingLeaseOwner == lease.Owner &&
                item.ProcessingLeaseExpiresTime != null &&
                item.ProcessingLeaseExpiresTime > now &&
                item.FileId == lease.FileId &&
                item.PatientProfileId == lease.PatientProfileId &&
                item.ExternalUserId == lease.ExternalUserId &&
                item.ContentSha256 == null)
            .ExecuteCommandAsync(cancellationToken);
        return affected == 1 ? lease with { ContentSha256 = normalized } : null;
    }

    public static async Task<bool> TryCommitAsync(
        ISqlSugarClient db,
        SaveLifeMaterialProcessingLease lease,
        SaveLifeEnrollmentMaterial state,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
            .SetColumns(item => new SaveLifeEnrollmentMaterial
            {
                ExtractedText = state.ExtractedText,
                ExtractStatus = state.ExtractStatus,
                ExtractError = state.ExtractError,
                ExtractedTime = state.ExtractedTime,
                ExtractModel = state.ExtractModel,
                IndexStatus = state.IndexStatus,
                IndexError = state.IndexError,
                IndexedTime = state.IndexedTime,
                ProcessRetryCount = state.ProcessRetryCount,
                NextProcessRetryTime = state.NextProcessRetryTime,
                ProcessingLeaseOwner = null,
                ProcessingLeaseExpiresTime = null,
                UpdateTime = now
            })
            .Where(item =>
                item.Id == lease.MaterialId &&
                !item.IsDelete &&
                item.ProcessingRevision == lease.Revision &&
                item.ProcessingLeaseOwner == lease.Owner &&
                item.ProcessingLeaseExpiresTime != null &&
                item.ProcessingLeaseExpiresTime > now &&
                item.FileId == lease.FileId &&
                item.PatientProfileId == lease.PatientProfileId &&
                item.ExternalUserId == lease.ExternalUserId &&
                item.ContentSha256 == lease.ContentSha256)
            .ExecuteCommandAsync(cancellationToken);
        return affected == 1;
    }

    public static async Task<SaveLifeEnrollmentMaterial> RequeueAsync(
        ISqlSugarClient db,
        long materialId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
            .SetColumns(item => new SaveLifeEnrollmentMaterial
            {
                ExtractStatus = "pending",
                ExtractError = null,
                IndexStatus = "pending",
                IndexError = null,
                ProcessRetryCount = 0,
                NextProcessRetryTime = null,
                ProcessingRevision = item.ProcessingRevision + 1,
                ProcessingLeaseOwner = null,
                ProcessingLeaseExpiresTime = null,
                UpdateTime = now
            })
            .Where(item => item.Id == materialId && !item.IsDelete && item.ProcessingRevision > 0)
            .ExecuteCommandAsync(cancellationToken);
        if (affected != 1)
            throw Oops.Oh("报名材料不存在或已删除").StatusCode(404);

        return await db.Queryable<SaveLifeEnrollmentMaterial>()
            .Where(item => item.Id == materialId)
            .FirstAsync(cancellationToken)
            ?? throw new InvalidOperationException("材料重排后无法读取");
    }

    private static ISugarQueryable<SaveLifeEnrollmentMaterial> CurrentQuery(
        ISqlSugarClient db,
        SaveLifeMaterialProcessingLease lease,
        DateTime now)
    {
        return db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(item =>
                item.Id == lease.MaterialId &&
                !item.IsDelete &&
                item.ProcessingRevision == lease.Revision &&
                item.ProcessingLeaseOwner == lease.Owner &&
                item.ProcessingLeaseExpiresTime != null &&
                item.ProcessingLeaseExpiresTime > now &&
                item.FileId == lease.FileId &&
                item.PatientProfileId == lease.PatientProfileId &&
                item.ExternalUserId == lease.ExternalUserId &&
                item.ContentSha256 == lease.ContentSha256);
    }
}

internal static class SaveLifeMaterialIndexPublicationPersistence
{
    public static async Task<bool> TryPublishAsync(
        ISqlSugarClient db,
        SaveLifeMaterialProcessingLease lease,
        Func<Task> publish,
        CancellationToken cancellationToken = default)
    {
        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            if (!await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
                    db,
                    lease,
                    DateTime.Now,
                    cancellationToken))
                return false;

            await publish();
            return true;
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "材料索引发布事务失败");
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
}

internal static class SaveLifeMaterialDeletionPersistence
{
    public static async Task<SaveLifeMaterialDeletionResult> DeleteAsync(
        ISqlSugarClient db,
        SaveLifeMaterialDeletionRequest request,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var material = await db.Queryable<SaveLifeEnrollmentMaterial>()
                .ClearFilter()
                .Where(item =>
                    item.Id == request.MaterialId &&
                    item.ExternalUserId == request.ExternalUserId &&
                    item.PatientProfileId == request.PatientProfileId &&
                    item.EnrollmentRecordId == request.EnrollmentRecordId)
                .FirstAsync(cancellationToken)
                ?? throw Oops.Oh("材料不存在或不属于当前用户").StatusCode(404);
            var wasAlreadyDeleted = material.IsDelete;
            if (!wasAlreadyDeleted)
            {
                var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
                    .SetColumns(item => new SaveLifeEnrollmentMaterial
                    {
                        IsDelete = true,
                        DeleteTime = now,
                        ExtractedText = null,
                        ExtractStatus = "deleted",
                        ExtractError = null,
                        IndexStatus = "deleted",
                        IndexError = null,
                        NextProcessRetryTime = null,
                        ProcessingRevision = item.ProcessingRevision + 1,
                        ProcessingLeaseOwner = null,
                        ProcessingLeaseExpiresTime = null,
                        UpdateTime = now
                    })
                    .Where(item =>
                        item.Id == material.Id &&
                        !item.IsDelete &&
                        item.ProcessingRevision == material.ProcessingRevision)
                    .ExecuteCommandAsync(cancellationToken);
                if (affected != 1)
                    throw new InvalidOperationException("材料删除与其他状态修改冲突");
            }

            var fileOutboxId = await SaveLifeMaterialUploadPersistence.QueueFileCleanupAsync(
                db,
                material.TenantId,
                material.FileId,
                now,
                idGenerator,
                cancellationToken);
            var indexOutboxId = await SaveLifeMaterialUploadPersistence.QueueMaterialIndexCleanupAsync(
                db,
                material.TenantId,
                material.Id,
                now,
                idGenerator,
                cancellationToken);
            return new SaveLifeMaterialDeletionResult(wasAlreadyDeleted, fileOutboxId, indexOutboxId);
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "材料删除事务失败");
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
}

internal sealed record SaveLifeMaterialProcessingLease(
    long MaterialId,
    long Revision,
    string Owner,
    DateTime LeaseExpiresTime,
    long FileId,
    long? PatientProfileId,
    string ExternalUserId,
    string? ContentSha256)
{
    public static SaveLifeMaterialProcessingLease FromMaterial(
        SaveLifeEnrollmentMaterial material,
        string owner,
        DateTime expires) => new(
            material.Id,
            material.ProcessingRevision,
            owner,
            expires,
            material.FileId,
            material.PatientProfileId,
            material.ExternalUserId,
            material.ContentSha256);

    public static SaveLifeMaterialProcessingLease? TryFromMaterial(SaveLifeEnrollmentMaterial material)
    {
        if (string.IsNullOrWhiteSpace(material.ProcessingLeaseOwner) ||
            material.ProcessingLeaseExpiresTime == null ||
            material.ProcessingRevision <= 0)
            return null;

        return FromMaterial(
            material,
            material.ProcessingLeaseOwner,
            material.ProcessingLeaseExpiresTime.Value);
    }
}

internal sealed record SaveLifeMaterialDeletionRequest(
    long? TenantId,
    string ExternalUserId,
    long PatientProfileId,
    long EnrollmentRecordId,
    long MaterialId);

internal sealed record SaveLifeMaterialDeletionResult(
    bool WasAlreadyDeleted,
    long FileCleanupOutboxId,
    long IndexCleanupOutboxId);

internal sealed class SaveLifeMaterialProcessingSupersededException : InvalidOperationException
{
    public SaveLifeMaterialProcessingSupersededException()
        : base("材料处理租约或代次已失效")
    {
    }
}
