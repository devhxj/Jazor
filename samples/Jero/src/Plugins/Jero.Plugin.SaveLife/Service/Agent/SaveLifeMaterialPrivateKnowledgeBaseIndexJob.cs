using Furion.Schedule;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Jero.Plugin.SaveLife.Service;

[JobDetail("SaveLifeMaterialPrivateKnowledgeBaseIndexJob", Description = "医线生机报名材料私有知识库入库", GroupName = "default", Concurrent = false)]
[PeriodMinutes(5, TriggerId = "SaveLifeMaterialPrivateKnowledgeBaseIndexDailyTrigger", Description = "每5分钟处理报名材料私有知识库入库", RunOnStart = true)]
public sealed class SaveLifeMaterialPrivateKnowledgeBaseIndexJob : IJob
{
    private const int DefaultBatchSize = 100;

    private readonly ISaveLifeMaterialIndexProcessor _processor;
    private readonly ILogger<SaveLifeMaterialPrivateKnowledgeBaseIndexJob> _logger;

    public SaveLifeMaterialPrivateKnowledgeBaseIndexJob(
        ISaveLifeMaterialIndexProcessor processor,
        ILogger<SaveLifeMaterialPrivateKnowledgeBaseIndexJob> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var result = await _processor.ProcessPendingAsync(DefaultBatchSize, stoppingToken);
        _logger.LogInformation(
            "SaveLife material private knowledge-base indexing completed. Total={Total}, Synced={Synced}, Skipped={Skipped}, Failed={Failed}",
            result.TotalCount,
            result.SyncedCount,
            result.SkippedCount,
            result.FailedCount);
    }
}

public interface ISaveLifeMaterialIndexProcessor
{
    Task<SaveLifeMaterialIndexBatchResult> ProcessPendingAsync(int limit, CancellationToken cancellationToken = default);

    Task<SaveLifeMaterialIndexItemResult> ProcessMaterialAsync(long materialId, CancellationToken cancellationToken = default);

    Task QueueMaterialAsync(long materialId, CancellationToken cancellationToken = default);
}

public sealed class SaveLifeMaterialIndexProcessor : ISaveLifeMaterialIndexProcessor, ITransient
{
    private const int MaxBatchSize = 500;

    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly SqlSugarRepository<SaveLifeAgentVectorIndex> _indexRep;
    private readonly ISaveLifeFileAccessService _fileAccessService;
    private readonly ISaveLifeAgentRetrievalService _retrievalService;
    private readonly ISaveLifeAgentConfigProvider _configProvider;
    private readonly ISaveLifeMaterialTextExtractionService _textExtractionService;
    private readonly ISaveLifeKnowledgeBaseIndexTaskDispatcher _taskDispatcher;

    public SaveLifeMaterialIndexProcessor(
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        SqlSugarRepository<SaveLifeAgentVectorIndex> indexRep,
        ISaveLifeFileAccessService fileAccessService,
        ISaveLifeAgentRetrievalService retrievalService,
        ISaveLifeAgentConfigProvider configProvider,
        ISaveLifeMaterialTextExtractionService textExtractionService,
        ISaveLifeKnowledgeBaseIndexTaskDispatcher taskDispatcher)
    {
        _materialRep = materialRep;
        _indexRep = indexRep;
        _fileAccessService = fileAccessService;
        _retrievalService = retrievalService;
        _configProvider = configProvider;
        _textExtractionService = textExtractionService;
        _taskDispatcher = taskDispatcher;
    }

    public async Task<SaveLifeMaterialIndexBatchResult> ProcessPendingAsync(int limit, CancellationToken cancellationToken = default)
    {
        var batchSize = Math.Clamp(limit, 1, MaxBatchSize);
        var options = await _configProvider.GetOptionsAsync();
        if (options.HasMaterialEmbeddingConfiguration)
        {
            await RequeueOutdatedMaterialIndexesAsync(
                options.MaterialEmbeddingModel.Trim(),
                options.MaterialEmbeddingDimensions,
                batchSize,
                cancellationToken);
        }

        var now = DateTime.Now;
        var materials = await _materialRep.AsQueryable()
            .Where(BuildMaterialProcessablePredicate(now))
            .OrderBy(u => u.UpdateTime, OrderByType.Asc)
            .OrderBy(u => u.CreateTime, OrderByType.Asc)
            .Take(batchSize)
            .ToListAsync();

        var result = new SaveLifeMaterialIndexBatchResult { TotalCount = materials.Count };
        foreach (var material in materials)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var itemResult = await ProcessMaterialAsync(material.Id, cancellationToken);
            result.Add(itemResult);
        }

        return result;
    }

    public async Task QueueMaterialAsync(long materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var material = await _materialRep.GetByIdAsync(materialId)
            ?? throw Oops.Oh("报名材料不存在").StatusCode(404);

        if (material.IsDelete)
        {
            await DeleteMaterialVectorIndexAsync(material);
            return;
        }
        if (!HasPrivatePatientScope(material))
            throw Oops.Oh("报名材料缺少账号或患者归属，不能进入私有知识库").StatusCode(400);

        material = await SaveLifeMaterialProcessingPersistence.RequeueAsync(
            _materialRep.Context,
            material.Id,
            DateTime.Now,
            cancellationToken);

        var index = await LoadMaterialVectorIndexAsync(material);
        if (index == null)
        {
            await _taskDispatcher.DispatchMaterialAsync(materialId);
            return;
        }

        index.SyncStatus = "pending";
        index.SyncError = null;
        index.RetryCount = 0;
        await _indexRep.AsUpdateable(index)
            .UpdateColumns(u => new { u.SyncStatus, u.SyncError, u.RetryCount })
            .ExecuteCommandAsync();

        await _taskDispatcher.DispatchMaterialAsync(materialId);
    }

    public async Task<SaveLifeMaterialIndexItemResult> ProcessMaterialAsync(long materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var material = await _materialRep.GetByIdAsync(materialId);
        if (material == null)
            return SaveLifeMaterialIndexItemResult.Skipped(materialId, "报名材料不存在");

        if (material.IsDelete)
        {
            await DeleteMaterialVectorIndexAsync(material);
            return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料已删除");
        }

        var lease = await TryClaimMaterialAsync(material.Id, cancellationToken);
        if (lease == null)
            return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料未到处理时间或已被其他任务领取");
        material = await _materialRep.AsQueryable()
            .Where(item =>
                item.Id == lease.MaterialId &&
                item.ProcessingRevision == lease.Revision &&
                item.ProcessingLeaseOwner == lease.Owner)
            .FirstAsync(cancellationToken)
            ?? throw new SaveLifeMaterialProcessingSupersededException();

        if (!HasPrivatePatientScope(material))
        {
            await MarkMaterialPermanentlySkippedAsync(
                material,
                lease,
                "报名材料缺少账号或患者归属，不能进入私有知识库",
                cancellationToken: cancellationToken);
            return SaveLifeMaterialIndexItemResult.Skipped(material.Id, material.IndexError);
        }

        try
        {
            await EnsureLeaseCurrentAsync(lease, cancellationToken);
            var options = await _configProvider.GetOptionsAsync();
            var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(material.FileSuffix ?? material.FileName);
            var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(suffix, material.ContentType);
            var isImageOrVideo = SaveLifeEnrollmentMaterialHelper.IsImageOrVideo(suffix, contentType);
            var missingConfigurationError = ResolveMissingEmbeddingConfigurationError(options);
            if (!string.IsNullOrWhiteSpace(missingConfigurationError))
            {
                if (!await MarkMaterialSkippedAsync(
                    material,
                    lease,
                    "skipped",
                    missingConfigurationError,
                    null,
                    missingConfigurationError,
                    cancellationToken))
                    return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
                return SaveLifeMaterialIndexItemResult.Skipped(material.Id, material.IndexError);
            }

            lease = await EnsureMaterialContentSha256Async(
                material,
                lease,
                cancellationToken);
            material.ContentSha256 = lease.ContentSha256;

            var extraction = await ExtractMaterialTextAsync(material, lease, cancellationToken);
            await EnsureLeaseCurrentAsync(lease, cancellationToken);
            if (!string.IsNullOrWhiteSpace(extraction.ExtractedText))
                material.ExtractedText = extraction.ExtractedText;

            if (string.Equals(extraction.Status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                var error = extraction.Error ?? "报名材料解析失败";
                if (SaveLifeMaterialProcessingFailurePolicy.IsPermanentFailure(error))
                {
                    if (!await MarkMaterialPermanentlySkippedAsync(
                        material,
                        lease,
                        error,
                        extraction.ExtractModel,
                        cancellationToken))
                        return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
                    return SaveLifeMaterialIndexItemResult.Skipped(material.Id, material.IndexError);
                }

                if (!await MarkMaterialFailedAsync(material, lease, error, extraction.ExtractModel, cancellationToken))
                    return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
                return SaveLifeMaterialIndexItemResult.Failed(material.Id, material.IndexError);
            }

            if (!CanIndexExtraction(extraction))
            {
                var error = extraction.Error ?? "病例材料未提取到可检索内容";
                if (!await MarkMaterialSkippedAsync(
                    material,
                    lease,
                    extraction.Status,
                    extraction.Error,
                    extraction.ExtractModel,
                    error,
                    cancellationToken))
                    return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
                return SaveLifeMaterialIndexItemResult.Skipped(material.Id, material.IndexError);
            }

            await EnsureLeaseCurrentAsync(lease, cancellationToken);
            await _retrievalService.SyncMaterialIndexAsync(
                material,
                () => OpenMaterialFileStreamAsync(material, lease, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();

            material.ExtractStatus = extraction.Status;
            material.ExtractError = extraction.Error;
            material.ExtractedTime = DateTime.Now;
            if (!isImageOrVideo || string.IsNullOrWhiteSpace(material.ExtractModel))
                material.ExtractModel = extraction.ExtractModel;
            material.IndexStatus = "synced";
            material.IndexError = null;
            material.IndexedTime = DateTime.Now;
            ApplyMaterialRetryState(material, true, null);

            if (!await UpdateMaterialProcessingColumnsAsync(material, lease, cancellationToken))
                throw new SaveLifeMaterialProcessingSupersededException();
            return SaveLifeMaterialIndexItemResult.Synced(material.Id);
        }
        catch (SaveLifeMaterialProcessingSupersededException)
        {
            return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "报名材料后台处理失败";
            if (SaveLifeMaterialProcessingFailurePolicy.IsPermanentFailure(error))
            {
                if (!await MarkMaterialPermanentlySkippedAsync(material, lease, error, cancellationToken: cancellationToken))
                    return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
                return SaveLifeMaterialIndexItemResult.Skipped(material.Id, material.IndexError);
            }

            if (!await MarkMaterialFailedAsync(material, lease, error, cancellationToken: cancellationToken))
                return SaveLifeMaterialIndexItemResult.Skipped(material.Id, "报名材料处理代次已失效");
            return SaveLifeMaterialIndexItemResult.Failed(material.Id, material.IndexError);
        }
    }

    internal static bool CanIndexExtraction(SaveLifeMaterialTextExtractionResult extraction)
    {
        ArgumentNullException.ThrowIfNull(extraction);
        return string.Equals(extraction.Status, "synced", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool HasPrivatePatientScope(SaveLifeEnrollmentMaterial material)
    {
        ArgumentNullException.ThrowIfNull(material);
        return !string.IsNullOrWhiteSpace(material.ExternalUserId) &&
            material.PatientProfileId is > 0;
    }

    private Task<SaveLifeMaterialProcessingLease?> TryClaimMaterialAsync(
        long materialId,
        CancellationToken cancellationToken)
    {
        return SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            _materialRep.Context,
            materialId,
            DateTime.Now,
            () => Guid.NewGuid().ToString("N"),
            cancellationToken);
    }

    private static Expression<Func<SaveLifeEnrollmentMaterial, bool>> BuildMaterialProcessablePredicate(DateTime now)
    {
        return SaveLifeMaterialProcessingPersistence.BuildProcessablePredicate(now);
    }

    private static Expression<Func<SaveLifeAgentVectorIndex, bool>> BuildOutdatedMaterialIndexPredicate(
        string materialEmbeddingModel,
        int materialEmbeddingDimensions)
    {
        return u =>
            !u.IsDelete &&
            u.ResourceType == "material" &&
            u.SyncStatus == "synced" &&
            (u.EmbeddingKind == null ||
                u.EmbeddingKind != "multimodal" ||
                u.EmbeddingConfiguredModel == null ||
                u.EmbeddingConfiguredModel != materialEmbeddingModel ||
                u.EmbeddingDimensions == null ||
                u.EmbeddingDimensions != materialEmbeddingDimensions);
    }

    private async Task RequeueOutdatedMaterialIndexesAsync(
        string materialEmbeddingModel,
        int materialEmbeddingDimensions,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await SaveLifeMaterialIndexVersionPersistence.RequeueOutdatedAsync(
            _materialRep.Context,
            materialEmbeddingModel,
            materialEmbeddingDimensions,
            limit,
            DateTime.Now,
            cancellationToken);
    }

    private static string? ResolveMissingEmbeddingConfigurationError(SaveLifeAgentOptions options)
    {
        if (!options.HasMaterialEmbeddingConfiguration)
            return SaveLifeEnrollmentMaterialHelper.MissingMaterialEmbeddingConfigurationMessage;

        return null;
    }

    private async Task<SaveLifeMaterialTextExtractionResult> ExtractMaterialTextAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _textExtractionService.ExtractAsync(
                material,
                () => OpenMaterialFileStreamAsync(material, lease, cancellationToken),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SaveLifeMaterialTextExtractionResult
            {
                Status = "failed",
                Error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512)
            };
        }
    }

    private async Task<SaveLifeMaterialProcessingLease> EnsureMaterialContentSha256Async(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(lease.ContentSha256))
            return lease;

        await using var stream = await OpenMaterialFileStreamAsync(
            material,
            lease,
            cancellationToken);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        long total = 0;
        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(
                    buffer.AsMemory(0, buffer.Length),
                    cancellationToken);
                if (read == 0)
                    break;

                total = checked(total + read);
                if (total > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes)
                    throw Oops.Oh("病例资料文件超过多模态解析限制").StatusCode(413);
                hash.AppendData(buffer, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }

        if (total == 0)
            throw Oops.Oh("病例资料文件为空").StatusCode(400);

        var contentSha256 = Convert.ToHexString(hash.GetHashAndReset());
        var updatedLease = await SaveLifeMaterialProcessingPersistence.TrySetContentSha256Async(
            _materialRep.Context,
            lease,
            contentSha256,
            DateTime.Now,
            cancellationToken);
        return updatedLease ?? throw new SaveLifeMaterialProcessingSupersededException();
    }

    private async Task<bool> MarkMaterialFailedAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        string message,
        string? extractModel = null,
        CancellationToken cancellationToken = default)
    {
        var error = Limit(message, 512);
        material.ExtractStatus = "failed";
        material.ExtractError = error;
        material.ExtractedTime = DateTime.Now;
        material.ExtractModel = extractModel ?? material.ExtractModel;
        material.IndexStatus = "failed";
        material.IndexError = error;
        material.IndexedTime = DateTime.Now;
        ApplyMaterialRetryState(material, false, error);

        return await UpdateMaterialProcessingColumnsAsync(material, lease, cancellationToken);
    }

    private async Task<bool> MarkMaterialPermanentlySkippedAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        string message,
        string? extractModel = null,
        CancellationToken cancellationToken = default)
    {
        var error = Limit(message, 512);
        material.ExtractStatus = "skipped";
        material.ExtractError = error;
        material.ExtractedTime = DateTime.Now;
        material.ExtractModel = extractModel ?? material.ExtractModel;
        material.IndexStatus = "skipped";
        material.IndexError = error;
        material.IndexedTime = DateTime.Now;
        material.ProcessRetryCount = 0;
        material.NextProcessRetryTime = null;

        return await UpdateMaterialProcessingColumnsAsync(material, lease, cancellationToken);
    }

    private async Task<bool> MarkMaterialSkippedAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        string? extractStatus,
        string? extractError,
        string? extractModel,
        string indexError,
        CancellationToken cancellationToken = default)
    {
        var error = Limit(indexError, 512);
        material.ExtractStatus = string.IsNullOrWhiteSpace(extractStatus) ? "skipped" : extractStatus;
        material.ExtractError = Limit(extractError, 512);
        material.ExtractedTime = DateTime.Now;
        material.ExtractModel = extractModel ?? material.ExtractModel;
        material.IndexStatus = "skipped";
        material.IndexError = error;
        material.IndexedTime = DateTime.Now;
        ApplyMaterialRetryState(material, false, error);

        return await UpdateMaterialProcessingColumnsAsync(material, lease, cancellationToken);
    }

    private static void ApplyMaterialRetryState(SaveLifeEnrollmentMaterial material, bool isSynced, string? error)
    {
        if (isSynced || string.IsNullOrWhiteSpace(error))
        {
            material.ProcessRetryCount = 0;
            material.NextProcessRetryTime = null;
            return;
        }

        var now = DateTime.Now;
        material.ProcessRetryCount += 1;
        material.NextProcessRetryTime = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(
            material.ProcessRetryCount,
            error,
            now);
    }

    private Task<bool> UpdateMaterialProcessingColumnsAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        CancellationToken cancellationToken)
    {
        return SaveLifeMaterialProcessingPersistence.TryCommitAsync(
            _materialRep.Context,
            lease,
            material,
            DateTime.Now,
            cancellationToken);
    }

    private async Task<Stream> OpenMaterialFileStreamAsync(
        SaveLifeEnrollmentMaterial material,
        SaveLifeMaterialProcessingLease lease,
        CancellationToken cancellationToken)
    {
        await EnsureLeaseCurrentAsync(lease, cancellationToken);
        return await _fileAccessService.OpenReadAsync(material.FileId);
    }

    private async Task EnsureLeaseCurrentAsync(
        SaveLifeMaterialProcessingLease lease,
        CancellationToken cancellationToken)
    {
        if (!await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
                _materialRep.Context,
                lease,
                DateTime.Now,
                cancellationToken))
            throw new SaveLifeMaterialProcessingSupersededException();
    }

    private async Task<SaveLifeAgentVectorIndex?> LoadMaterialVectorIndexAsync(SaveLifeEnrollmentMaterial material)
    {
        return await _indexRep.GetFirstAsync(u =>
            u.ResourceType == "material" &&
            u.ResourceId == material.Id &&
            u.ExternalUserId == material.ExternalUserId);
    }

    private async Task DeleteMaterialVectorIndexAsync(SaveLifeEnrollmentMaterial material)
    {
        var index = await LoadMaterialVectorIndexAsync(material);
        if (index != null)
            await _indexRep.DeleteAsync(index);
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

public sealed class SaveLifeMaterialIndexBatchResult
{
    public int TotalCount { get; set; }

    public int SyncedCount { get; private set; }

    public int SkippedCount { get; private set; }

    public int FailedCount { get; private set; }

    public void Add(SaveLifeMaterialIndexItemResult result)
    {
        if (result.Status == "synced")
        {
            SyncedCount++;
            return;
        }

        if (result.Status == "failed")
        {
            FailedCount++;
            return;
        }

        SkippedCount++;
    }
}

public sealed class SaveLifeMaterialIndexItemResult
{
    public long MaterialId { get; private init; }

    public string Status { get; private init; } = "skipped";

    public string? Message { get; private init; }

    public static SaveLifeMaterialIndexItemResult Synced(long materialId)
    {
        return new SaveLifeMaterialIndexItemResult
        {
            MaterialId = materialId,
            Status = "synced"
        };
    }

    public static SaveLifeMaterialIndexItemResult Skipped(long materialId, string? message)
    {
        return new SaveLifeMaterialIndexItemResult
        {
            MaterialId = materialId,
            Status = "skipped",
            Message = message
        };
    }

    public static SaveLifeMaterialIndexItemResult Failed(long materialId, string? message)
    {
        return new SaveLifeMaterialIndexItemResult
        {
            MaterialId = materialId,
            Status = "failed",
            Message = message
        };
    }
}
