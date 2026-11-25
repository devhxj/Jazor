using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeMaterialUploadPersistence
{
    private const int MaximumAttempts = 40;

    public static async Task<SaveLifeMaterialUploadReplayResult> TryReplayAsync(
        ISqlSugarClient db,
        SaveLifeMaterialUploadReplayRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(request);
        ValidateScope(request.ExternalUserId, request.ClientCommandId, request.Payload);
        var command = await FindCommandAsync(
            db,
            request.TenantId,
            request.ExternalUserId,
            request.ClientCommandId,
            request.Payload,
            cancellationToken);
        if (command == null)
            return SaveLifeMaterialUploadReplayResult.NotFound();

        var replay = await ReplayAsync(
            db,
            command,
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(request.Payload),
            cancellationToken);
        return SaveLifeMaterialUploadReplayResult.Found(replay.Material, replay.OutboxId);
    }

    public static async Task<long> QueueFileCleanupAsync(
        ISqlSugarClient db,
        long? tenantId,
        long fileId,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        return await QueueCleanupAsync(
            db,
            tenantId,
            SaveLifeMaterialWorkTypes.CleanupFile,
            fileId,
            now,
            idGenerator,
            cancellationToken);
    }

    public static async Task<long> QueueMaterialIndexCleanupAsync(
        ISqlSugarClient db,
        long? tenantId,
        long materialId,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        return await QueueCleanupAsync(
            db,
            tenantId,
            SaveLifeMaterialWorkTypes.CleanupMaterialIndex,
            materialId,
            now,
            idGenerator,
            cancellationToken);
    }

    private static async Task<long> QueueCleanupAsync(
        ISqlSugarClient db,
        long? tenantId,
        string workType,
        long aggregateId,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(idGenerator);
        if (aggregateId <= 0)
            throw new ArgumentOutOfRangeException(nameof(aggregateId));
        var existing = await db.Queryable<SaveLifeMaterialWorkOutbox>()
            .ClearFilter()
            .Where(item =>
                item.WorkType == workType &&
                item.AggregateId == aggregateId)
            .FirstAsync(cancellationToken);
        if (existing != null)
        {
            await ReactivateCompletedCleanupAsync(db, existing, now, cancellationToken);
            return existing.Id;
        }

        var outbox = new SaveLifeMaterialWorkOutbox
        {
            Id = idGenerator(),
            TenantId = tenantId,
            WorkType = workType,
            AggregateId = aggregateId,
            Status = SaveLifeMaterialWorkStatuses.Pending,
            CreateTime = now
        };
        try
        {
            await db.Insertable(outbox).ExecuteCommandAsync(cancellationToken);
            return outbox.Id;
        }
        catch (Exception error) when (Contains(error, SaveLifeMaterialUploadMigration.OutboxAggregateUniqueIndexName))
        {
            existing = await db.Queryable<SaveLifeMaterialWorkOutbox>()
                .ClearFilter()
                .Where(item =>
                    item.WorkType == workType &&
                    item.AggregateId == aggregateId)
                .FirstAsync(cancellationToken);
            return existing?.Id ??
                throw new InvalidOperationException("文件清理任务唯一冲突后未找到已有任务", error);
        }
    }

    private static async Task ReactivateCompletedCleanupAsync(
        ISqlSugarClient db,
        SaveLifeMaterialWorkOutbox existing,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (existing.Status != SaveLifeMaterialWorkStatuses.Completed)
            return;

        await db.Updateable<SaveLifeMaterialWorkOutbox>()
            .SetColumns(item => new SaveLifeMaterialWorkOutbox
            {
                Status = SaveLifeMaterialWorkStatuses.Pending,
                LeaseOwner = null,
                LeaseExpiresTime = null,
                AttemptCount = 0,
                NextAttemptTime = null,
                LastErrorCode = null,
                UpdateTime = now
            })
            .Where(item => item.Id == existing.Id && item.Status == SaveLifeMaterialWorkStatuses.Completed)
            .ExecuteCommandAsync(cancellationToken);
    }

    public static async Task<SaveLifeMaterialUploadCommitResult> CommitAsync(
        ISqlSugarClient db,
        SaveLifeMaterialUploadRequest request,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(idGenerator);
        Validate(request);
        var payloadHash = SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(request.Payload);

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                var transaction = await db.Ado.UseTranAsync(async () =>
                {
                    var existing = await FindCommandAsync(
                        db,
                        request.TenantId,
                        request.ExternalUserId,
                        request.ClientCommandId,
                        request.Payload,
                        cancellationToken);
                    if (existing != null)
                        return await ReplayAsync(db, existing, payloadHash, cancellationToken);

                    await EnsureWritableScopeAsync(db, request, cancellationToken);

                    var command = new SaveLifeMaterialUploadCommand
                    {
                        Id = idGenerator(),
                        TenantId = request.TenantId,
                        ExternalUserId = request.ExternalUserId,
                        PatientProfileId = request.Payload.PatientProfileId,
                        EnrollmentRecordId = request.Payload.EnrollmentRecordId,
                        ClientCommandId = request.ClientCommandId,
                        PayloadHashAlgorithm = payloadHash.Algorithm,
                        PayloadHash = payloadHash.Value,
                        ContentSha256 = request.Payload.ContentSha256,
                        Status = "processing",
                        CreateTime = now
                    };
                    if (await db.Insertable(command).ExecuteCommandAsync(cancellationToken) != 1)
                        throw new InvalidOperationException("材料上传命令写入失败");

                    var active = await db.Queryable<SaveLifeEnrollmentMaterial>()
                        .ClearFilter()
                        .Where(item =>
                            item.ExternalUserId == request.ExternalUserId &&
                            item.PatientProfileId == request.Payload.PatientProfileId &&
                            item.EnrollmentRecordId == request.Payload.EnrollmentRecordId &&
                            !item.IsDelete)
                        .ToListAsync(cancellationToken);
                    var slot = SaveLifeEnrollmentMaterialHelper.ResolveNextUploadSlot(
                        active.Select(item => item.UploadSlot ?? 0));
                    var material = request.Material;
                    material.Id = material.Id > 0 ? material.Id : idGenerator();
                    material.TenantId = request.TenantId;
                    material.ExternalUserId = request.ExternalUserId;
                    material.PatientProfileId = request.Payload.PatientProfileId;
                    material.EnrollmentRecordId = request.Payload.EnrollmentRecordId;
                    material.ContentSha256 = request.Payload.ContentSha256;
                    material.SizeBytes = request.Payload.ActualSizeBytes;
                    material.UploadSlot = slot;
                    material.CreateTime = now;
                    if (await db.Insertable(material).ExecuteCommandAsync(cancellationToken) != 1)
                        throw new InvalidOperationException("材料记录写入失败");

                    var outbox = new SaveLifeMaterialWorkOutbox
                    {
                        Id = idGenerator(),
                        TenantId = request.TenantId,
                        WorkType = SaveLifeMaterialWorkTypes.IndexMaterial,
                        AggregateId = material.Id,
                        Status = SaveLifeMaterialWorkStatuses.Pending,
                        CreateTime = now
                    };
                    if (await db.Insertable(outbox).ExecuteCommandAsync(cancellationToken) != 1)
                        throw new InvalidOperationException("材料派发任务写入失败");

                    var resultJson = JsonSerializer.Serialize(new { materialId = material.Id, outboxId = outbox.Id });
                    var completed = await db.Updateable<SaveLifeMaterialUploadCommand>()
                        .SetColumns(item => new SaveLifeMaterialUploadCommand
                        {
                            Status = "completed",
                            MaterialId = material.Id,
                            OutboxId = outbox.Id,
                            ResultJson = resultJson,
                            UpdateTime = now
                        })
                        .Where(item => item.Id == command.Id && item.Status == "processing" && !item.IsDelete)
                        .ExecuteCommandAsync(cancellationToken);
                    if (completed != 1)
                        throw new InvalidOperationException("材料上传命令结果写入失败");

                    return new SaveLifeMaterialUploadCommitResult(material, outbox.Id, IsReplay: false);
                }, _ => { });
                if (!transaction.IsSuccess)
                    Rethrow(transaction.ErrorException, "材料上传事务失败");
                return transaction.Data;
            }
            catch (Exception error) when (attempt < MaximumAttempts && IsRetryableContention(error))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(10 * attempt, 100)), cancellationToken);
            }
        }

        throw new InvalidOperationException("材料上传命令在最大并发重试次数内未完成");
    }

    private static async Task<SaveLifeMaterialUploadCommitResult> ReplayAsync(
        ISqlSugarClient db,
        SaveLifeMaterialUploadCommand command,
        SaveLifeAgentPayloadHash payloadHash,
        CancellationToken cancellationToken)
    {
        if (command.PayloadHashAlgorithm != payloadHash.Algorithm || command.PayloadHash != payloadHash.Value)
            throw Oops.Oh("客户端命令 Id 已被其他材料载荷使用").StatusCode(409);
        if (command.Status != "completed" || command.MaterialId is not > 0 || command.OutboxId is not > 0)
            throw Oops.Oh("材料上传命令仍在处理中，请稍后使用相同命令 Id 重试").StatusCode(409);

        var material = await db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(item => item.Id == command.MaterialId.Value)
            .FirstAsync(cancellationToken)
            ?? throw new InvalidOperationException("已完成材料上传命令缺少材料记录");
        return new SaveLifeMaterialUploadCommitResult(material, command.OutboxId.Value, IsReplay: true);
    }

    private static async Task<SaveLifeMaterialUploadCommand?> FindCommandAsync(
        ISqlSugarClient db,
        long? tenantId,
        string externalUserId,
        string clientCommandId,
        SaveLifeMaterialUploadPayload payload,
        CancellationToken cancellationToken)
    {
        return await db.Queryable<SaveLifeMaterialUploadCommand>()
            .ClearFilter()
            .Where(item =>
                item.ExternalUserId == externalUserId &&
                item.PatientProfileId == payload.PatientProfileId &&
                item.EnrollmentRecordId == payload.EnrollmentRecordId &&
                item.ClientCommandId == clientCommandId)
            .FirstAsync(cancellationToken);
    }

    private static async Task EnsureWritableScopeAsync(
        ISqlSugarClient db,
        SaveLifeMaterialUploadRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await db.Queryable<SaveLifePatientProfile>()
            .ClearFilter()
            .Where(item =>
                item.Id == request.Payload.PatientProfileId &&
                item.OwnerExternalUserId == request.ExternalUserId &&
                !item.IsDelete)
            .FirstAsync(cancellationToken);
        if (patient == null)
            throw Oops.Oh("患者不存在或不属于当前账号").StatusCode(409);

        if (request.Payload.EnrollmentRecordId <= 0)
        {
            if (request.Material.ProjectId != 0)
                throw new InvalidOperationException("个人材料不能绑定项目");
            return;
        }

        var enrollment = await db.Queryable<SaveLifeEnrollmentRecord>()
            .ClearFilter()
            .Where(item =>
                item.Id == request.Payload.EnrollmentRecordId &&
                item.ExternalUserId == request.ExternalUserId &&
                item.PatientProfileId == request.Payload.PatientProfileId &&
                item.ProjectId == request.Material.ProjectId &&
                !item.IsDelete)
            .FirstAsync(cancellationToken);
        if (enrollment == null)
            throw Oops.Oh("报名记录不存在或不属于当前患者").StatusCode(409);
        if (enrollment.Status is "approved" or "cancelled" or "closed")
            throw Oops.Oh("当前报名状态不允许上传材料").StatusCode(409);
    }

    private static void Validate(SaveLifeMaterialUploadRequest request)
    {
        ValidateScope(request.ExternalUserId, request.ClientCommandId, request.Payload);
        if (request.Material.FileId <= 0)
            throw Oops.Oh("材料上传命令缺少文件记录").StatusCode(400);
    }

    private static void ValidateScope(
        string externalUserId,
        string clientCommandId,
        SaveLifeMaterialUploadPayload payload)
    {
        if (string.IsNullOrWhiteSpace(externalUserId) || externalUserId.Length > 64 ||
            string.IsNullOrWhiteSpace(clientCommandId) || clientCommandId.Length > 128 ||
            payload.PatientProfileId <= 0 || payload.EnrollmentRecordId < 0 ||
            payload.ActualSizeBytes <= 0 || payload.ActualSizeBytes > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes ||
            payload.ContentSha256.Length != 64)
            throw Oops.Oh("材料上传命令参数不完整或格式不正确").StatusCode(400);
    }

    private static bool IsRetryableContention(Exception? error)
    {
        return Contains(error, "database is locked") ||
            Contains(error, "database is busy") ||
            Contains(error, SaveLifeMaterialUploadMigration.CommandScopeUniqueIndexName) ||
            Contains(error, SaveLifeMaterialUploadMigration.ActiveSlotUniqueIndexName);
    }

    private static bool Contains(Exception? error, string value)
    {
        for (var current = error; current != null; current = current.InnerException)
        {
            if (current.Message.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    [DoesNotReturn]
    private static void Rethrow(Exception? exception, string fallbackMessage)
    {
        if (exception == null)
            throw new InvalidOperationException(fallbackMessage);
        ExceptionDispatchInfo.Capture(exception).Throw();
        throw new InvalidOperationException(fallbackMessage);
    }
}

internal sealed record SaveLifeMaterialUploadRequest(
    long? TenantId,
    string ExternalUserId,
    string ClientCommandId,
    SaveLifeMaterialUploadPayload Payload,
    SaveLifeEnrollmentMaterial Material);

internal sealed record SaveLifeMaterialUploadReplayRequest(
    long? TenantId,
    string ExternalUserId,
    string ClientCommandId,
    SaveLifeMaterialUploadPayload Payload);

internal sealed record SaveLifeMaterialUploadCommitResult(
    SaveLifeEnrollmentMaterial Material,
    long OutboxId,
    bool IsReplay);

internal sealed record SaveLifeMaterialUploadReplayResult(
    bool Exists,
    SaveLifeEnrollmentMaterial? Material,
    long? OutboxId)
{
    public static SaveLifeMaterialUploadReplayResult NotFound() => new(false, null, null);

    public static SaveLifeMaterialUploadReplayResult Found(
        SaveLifeEnrollmentMaterial material,
        long outboxId) => new(true, material, outboxId);
}

internal static class SaveLifeMaterialWorkTypes
{
    public const string IndexMaterial = "index_material";
    public const string CleanupFile = "cleanup_file";
    public const string CleanupMaterialIndex = "cleanup_material_index";
}

internal static class SaveLifeMaterialWorkStatuses
{
    public const string Pending = "pending";
    public const string Dispatching = "dispatching";
    public const string Dispatched = "dispatched";
    public const string Completed = "completed";
}
