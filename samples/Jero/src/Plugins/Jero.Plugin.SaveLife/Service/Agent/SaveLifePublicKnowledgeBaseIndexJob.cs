using Furion.Schedule;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Jero.Plugin.SaveLife.Service;

[JobDetail("SaveLifePublicKnowledgeBaseIndexJob", Description = "医线生机公开知识库入库", GroupName = "default", Concurrent = false)]
[PeriodMinutes(5, TriggerId = "SaveLifePublicKnowledgeBaseIndexDailyTrigger", Description = "每5分钟处理疾病知识和临床试验项目公开知识库入库", RunOnStart = true)]
public sealed class SaveLifePublicKnowledgeBaseIndexJob : IJob
{
    private const int DefaultBatchSize = 100;
    private const int MaxExecutionResultLength = 128;

    private readonly ISaveLifePublicKnowledgeBaseIndexProcessor _processor;
    private readonly ILogger<SaveLifePublicKnowledgeBaseIndexJob> _logger;

    public SaveLifePublicKnowledgeBaseIndexJob(
        ISaveLifePublicKnowledgeBaseIndexProcessor processor,
        ILogger<SaveLifePublicKnowledgeBaseIndexJob> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var result = await _processor.ProcessPendingAsync(DefaultBatchSize, stoppingToken);
        var summary = $"Total={result.TotalCount}, Synced={result.SyncedCount}, Removed={result.RemovedCount}, Skipped={result.SkippedCount}, Failed={result.FailedCount}";
        context.Result = summary;
        _logger.LogInformation(
            "SaveLife public knowledge-base indexing completed. Total={Total}, Synced={Synced}, Removed={Removed}, Skipped={Skipped}, Failed={Failed}",
            result.TotalCount,
            result.SyncedCount,
            result.RemovedCount,
            result.SkippedCount,
            result.FailedCount);

        if (result.FailedCount > 0)
        {
            var details = result.FailureMessages.Count == 0
                ? string.Empty
                : $" Details: {string.Join(" | ", result.FailureMessages)}";
            context.Result = LimitExecutionResult($"{summary};{details}");
            throw new InvalidOperationException(
                $"SaveLife public knowledge-base indexing failed. Total={result.TotalCount}, Failed={result.FailedCount}.{details}");
        }
    }

    private static string LimitExecutionResult(string value)
    {
        return value.Length <= MaxExecutionResultLength
            ? value
            : value[..MaxExecutionResultLength];
    }
}

public interface ISaveLifePublicKnowledgeBaseIndexProcessor
{
    Task<SaveLifePublicKnowledgeBaseIndexBatchResult> ProcessPendingAsync(int limit, CancellationToken cancellationToken = default);

    Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default);

    Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessTrialAsync(long trialId, CancellationToken cancellationToken = default);

    Task QueueKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default);

    Task QueueTrialAsync(long trialId, CancellationToken cancellationToken = default);

    Task RemoveKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default);

    Task RemoveTrialAsync(long trialId, CancellationToken cancellationToken = default);
}

public sealed class SaveLifePublicKnowledgeBaseIndexProcessor : ISaveLifePublicKnowledgeBaseIndexProcessor, ITransient
{
    private const int MaxBatchSize = 500;
    private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(30);

    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeAgentVectorIndex> _indexRep;
    private readonly ISaveLifeAgentRetrievalService _retrievalService;
    private readonly ISaveLifeKnowledgeBaseIndexTaskDispatcher _taskDispatcher;

    public SaveLifePublicKnowledgeBaseIndexProcessor(
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeAgentVectorIndex> indexRep,
        ISaveLifeAgentRetrievalService retrievalService,
        ISaveLifeKnowledgeBaseIndexTaskDispatcher taskDispatcher)
    {
        _knowledgeRep = knowledgeRep;
        _trialRep = trialRep;
        _indexRep = indexRep;
        _retrievalService = retrievalService;
        _taskDispatcher = taskDispatcher;
    }

    public async Task<SaveLifePublicKnowledgeBaseIndexBatchResult> ProcessPendingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var batchSize = Math.Clamp(limit, 1, MaxBatchSize);
        var resourceBatchSize = Math.Max(1, batchSize);
        var now = DateTime.Now;
        var knowledge = await _knowledgeRep.AsQueryable()
            .Where(BuildKnowledgeProcessablePredicate(now))
            .OrderBy(u => u.UpdateTime, OrderByType.Asc)
            .OrderBy(u => u.CreateTime, OrderByType.Asc)
            .Take(resourceBatchSize)
            .ToListAsync();
        var trials = await _trialRep.AsQueryable()
            .Where(BuildTrialProcessablePredicate(now))
            .OrderBy(u => u.UpdateTime, OrderByType.Asc)
            .OrderBy(u => u.CreateTime, OrderByType.Asc)
            .Take(resourceBatchSize)
            .ToListAsync();

        var result = new SaveLifePublicKnowledgeBaseIndexBatchResult
        {
            KnowledgeTotalCount = knowledge.Count,
            TrialTotalCount = trials.Count
        };

        foreach (var item in knowledge)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(await ProcessKnowledgeAsync(item.Id, cancellationToken));
        }

        foreach (var item in trials)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(await ProcessTrialAsync(item.Id, cancellationToken));
        }

        return result;
    }

    public async Task QueueKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var knowledge = await _knowledgeRep.GetByIdAsync(knowledgeId);
        if (knowledge == null)
        {
            await DeletePublicVectorIndexAsync("knowledge", knowledgeId);
            return;
        }

        if (!knowledge.IsPublished)
        {
            await RemoveKnowledgeAsync(knowledgeId, cancellationToken);
            return;
        }

        await MarkKnowledgeStateAsync(knowledge, false, null);
        await MarkPublicVectorIndexPendingAsync("knowledge", knowledgeId);
        await _taskDispatcher.DispatchKnowledgeAsync(knowledgeId);
    }

    public async Task QueueTrialAsync(long trialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trial = await _trialRep.GetByIdAsync(trialId);
        if (trial == null)
        {
            await DeletePublicVectorIndexAsync("trial", trialId);
            return;
        }

        await MarkTrialStateAsync(trial, false, null);
        await MarkPublicVectorIndexPendingAsync("trial", trialId);
        await _taskDispatcher.DispatchTrialAsync(trialId);
    }

    public async Task RemoveKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await DeletePublicVectorIndexAsync("knowledge", knowledgeId);
        var knowledge = await _knowledgeRep.GetByIdAsync(knowledgeId);
        if (knowledge != null)
            await MarkKnowledgeStateAsync(knowledge, false, null);
    }

    public async Task RemoveTrialAsync(long trialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await DeletePublicVectorIndexAsync("trial", trialId);
        var trial = await _trialRep.GetByIdAsync(trialId);
        if (trial != null)
            await MarkTrialStateAsync(trial, false, null);
    }

    public async Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessKnowledgeAsync(
        long knowledgeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var knowledge = await _knowledgeRep.GetByIdAsync(knowledgeId);
        if (knowledge == null)
        {
            await DeletePublicVectorIndexAsync("knowledge", knowledgeId);
            return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("knowledge", knowledgeId, "疾病知识不存在");
        }

        if (!knowledge.IsPublished)
        {
            await RemoveKnowledgeAsync(knowledge.Id, cancellationToken);
            return SaveLifePublicKnowledgeBaseIndexItemResult.Removed("knowledge", knowledge.Id);
        }

        var claimedKnowledge = await TryClaimKnowledgeAsync(knowledge.Id, cancellationToken);
        if (claimedKnowledge == null)
            return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("knowledge", knowledge.Id, "疾病知识未到处理时间或已被其他任务领取");
        knowledge = claimedKnowledge;

        try
        {
            var signature = BuildKnowledgeSignature(knowledge);
            await _retrievalService.SyncKnowledgeIndexAsync(knowledge);
            cancellationToken.ThrowIfCancellationRequested();
            if (!await IsKnowledgeSyncStillCurrentAsync(knowledge.Id, signature, cancellationToken))
            {
                return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("knowledge", knowledge.Id, "疾病知识在入库期间发生变更，已重新排队");
            }

            return SaveLifePublicKnowledgeBaseIndexItemResult.Synced("knowledge", knowledge.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "疾病知识入库失败";
            var latest = await _knowledgeRep.GetByIdAsync(knowledge.Id);
            if (latest == null || latest.KnowledgeBaseError != error)
                await MarkKnowledgeStateAsync(latest ?? knowledge, false, error);
            return SaveLifePublicKnowledgeBaseIndexItemResult.Failed("knowledge", knowledge.Id, latest?.KnowledgeBaseError ?? error);
        }
    }

    public async Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessTrialAsync(
        long trialId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trial = await _trialRep.GetByIdAsync(trialId);
        if (trial == null)
        {
            await DeletePublicVectorIndexAsync("trial", trialId);
            return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("trial", trialId, "临床试验项目不存在");
        }

        var claimedTrial = await TryClaimTrialAsync(trial.Id, cancellationToken);
        if (claimedTrial == null)
            return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("trial", trial.Id, "临床试验项目未到处理时间或已被其他任务领取");
        trial = claimedTrial;

        try
        {
            var signature = BuildTrialSignature(trial);
            await _retrievalService.SyncTrialIndexAsync(trial);
            cancellationToken.ThrowIfCancellationRequested();
            if (!await IsTrialSyncStillCurrentAsync(trial.Id, signature, cancellationToken))
            {
                return SaveLifePublicKnowledgeBaseIndexItemResult.Skipped("trial", trial.Id, "临床试验项目在入库期间发生变更，已重新排队");
            }

            return SaveLifePublicKnowledgeBaseIndexItemResult.Synced("trial", trial.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "临床试验项目入库失败";
            var latest = await _trialRep.GetByIdAsync(trial.Id);
            if (latest == null || latest.KnowledgeBaseError != error)
                await MarkTrialStateAsync(latest ?? trial, false, error);
            return SaveLifePublicKnowledgeBaseIndexItemResult.Failed("trial", trial.Id, latest?.KnowledgeBaseError ?? error);
        }
    }

    private async Task<SaveLifeDiseaseKnowledge?> TryClaimKnowledgeAsync(
        long knowledgeId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.Now;
        var affected = await _knowledgeRep.AsUpdateable()
            .SetColumns(u => new SaveLifeDiseaseKnowledge
            {
                KnowledgeBaseLastAttemptTime = now,
                KnowledgeBaseNextRetryTime = now.Add(ProcessingLease)
            })
            .Where(u => u.Id == knowledgeId)
            .Where(BuildKnowledgeProcessablePredicate(now))
            .ExecuteCommandAsync();
        return affected > 0 ? await _knowledgeRep.GetByIdAsync(knowledgeId) : null;
    }

    private async Task<SaveLifeClinicalTrial?> TryClaimTrialAsync(
        long trialId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.Now;
        var affected = await _trialRep.AsUpdateable()
            .SetColumns(u => new SaveLifeClinicalTrial
            {
                KnowledgeBaseLastAttemptTime = now,
                KnowledgeBaseNextRetryTime = now.Add(ProcessingLease)
            })
            .Where(u => u.Id == trialId)
            .Where(BuildTrialProcessablePredicate(now))
            .ExecuteCommandAsync();
        return affected > 0 ? await _trialRep.GetByIdAsync(trialId) : null;
    }

    private static Expression<Func<SaveLifeDiseaseKnowledge, bool>> BuildKnowledgeProcessablePredicate(DateTime now)
    {
        return u => ((u.IsPublished == true && SqlFunc.IsNull(u.IsInKnowledgeBase, false) == false) ||
                (u.IsPublished == false && (SqlFunc.IsNull(u.IsInKnowledgeBase, false) == true || u.KnowledgeBaseError != null))) &&
            (u.KnowledgeBaseNextRetryTime == null || u.KnowledgeBaseNextRetryTime <= now);
    }

    private static Expression<Func<SaveLifeClinicalTrial, bool>> BuildTrialProcessablePredicate(DateTime now)
    {
        return u => SqlFunc.IsNull(u.IsInKnowledgeBase, false) == false &&
            (u.KnowledgeBaseNextRetryTime == null || u.KnowledgeBaseNextRetryTime <= now);
    }

    private async Task MarkPublicVectorIndexPendingAsync(string resourceType, long resourceId)
    {
        var index = await LoadPublicVectorIndexAsync(resourceType, resourceId);
        if (index == null)
            return;

        index.SyncStatus = "pending";
        index.SyncError = null;
        index.RetryCount = 0;
        await _indexRep.AsUpdateable(index)
            .UpdateColumns(u => new { u.SyncStatus, u.SyncError, u.RetryCount })
            .ExecuteCommandAsync();
    }

    private async Task DeletePublicVectorIndexAsync(string resourceType, long resourceId)
    {
        var index = await LoadPublicVectorIndexAsync(resourceType, resourceId);
        if (index != null)
            await _indexRep.DeleteAsync(index);
    }

    private async Task<SaveLifeAgentVectorIndex?> LoadPublicVectorIndexAsync(string resourceType, long resourceId)
    {
        return await _indexRep.GetFirstAsync(u =>
            u.ResourceType == resourceType &&
            u.ResourceId == resourceId &&
            u.ExternalUserId == null);
    }

    private async Task MarkKnowledgeStateAsync(SaveLifeDiseaseKnowledge knowledge, bool isInKnowledgeBase, string? error)
    {
        knowledge.IsInKnowledgeBase = isInKnowledgeBase;
        knowledge.KnowledgeBaseError = error;
        ApplyKnowledgeBaseRetryState(knowledge, isInKnowledgeBase, error);
        await _knowledgeRep.AsUpdateable(knowledge)
            .UpdateColumns(u => new
            {
                u.IsInKnowledgeBase,
                u.KnowledgeBaseError,
                u.KnowledgeBaseRetryCount,
                u.KnowledgeBaseLastAttemptTime,
                u.KnowledgeBaseNextRetryTime
            })
            .ExecuteCommandAsync();
    }

    private async Task MarkTrialStateAsync(SaveLifeClinicalTrial trial, bool isInKnowledgeBase, string? error)
    {
        trial.IsInKnowledgeBase = isInKnowledgeBase;
        trial.KnowledgeBaseError = error;
        ApplyKnowledgeBaseRetryState(trial, isInKnowledgeBase, error);
        await _trialRep.AsUpdateable(trial)
            .UpdateColumns(u => new
            {
                u.IsInKnowledgeBase,
                u.KnowledgeBaseError,
                u.KnowledgeBaseRetryCount,
                u.KnowledgeBaseLastAttemptTime,
                u.KnowledgeBaseNextRetryTime
            })
            .ExecuteCommandAsync();
    }

    private static void ApplyKnowledgeBaseRetryState(
        SaveLifeDiseaseKnowledge knowledge,
        bool isInKnowledgeBase,
        string? error)
    {
        if (isInKnowledgeBase || string.IsNullOrWhiteSpace(error))
        {
            knowledge.KnowledgeBaseRetryCount = 0;
            knowledge.KnowledgeBaseLastAttemptTime = null;
            knowledge.KnowledgeBaseNextRetryTime = null;
            return;
        }

        var now = DateTime.Now;
        knowledge.KnowledgeBaseRetryCount += 1;
        knowledge.KnowledgeBaseLastAttemptTime = now;
        knowledge.KnowledgeBaseNextRetryTime = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(
            knowledge.KnowledgeBaseRetryCount,
            error,
            now);
    }

    private static void ApplyKnowledgeBaseRetryState(
        SaveLifeClinicalTrial trial,
        bool isInKnowledgeBase,
        string? error)
    {
        if (isInKnowledgeBase || string.IsNullOrWhiteSpace(error))
        {
            trial.KnowledgeBaseRetryCount = 0;
            trial.KnowledgeBaseLastAttemptTime = null;
            trial.KnowledgeBaseNextRetryTime = null;
            return;
        }

        var now = DateTime.Now;
        trial.KnowledgeBaseRetryCount += 1;
        trial.KnowledgeBaseLastAttemptTime = now;
        trial.KnowledgeBaseNextRetryTime = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(
            trial.KnowledgeBaseRetryCount,
            error,
            now);
    }

    private async Task<bool> IsKnowledgeSyncStillCurrentAsync(
        long knowledgeId,
        string signature,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var latest = await _knowledgeRep.GetByIdAsync(knowledgeId);
        if (latest == null || !latest.IsPublished)
        {
            await RemoveKnowledgeAsync(knowledgeId, cancellationToken);
            return false;
        }

        if (BuildKnowledgeSignature(latest) == signature)
            return true;

        await QueueKnowledgeAsync(knowledgeId, cancellationToken);
        return false;
    }

    private async Task<bool> IsTrialSyncStillCurrentAsync(
        long trialId,
        string signature,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var latest = await _trialRep.GetByIdAsync(trialId);
        if (latest == null)
        {
            await RemoveTrialAsync(trialId, cancellationToken);
            return false;
        }

        if (BuildTrialSignature(latest) == signature)
            return true;

        await QueueTrialAsync(trialId, cancellationToken);
        return false;
    }

    private static string BuildKnowledgeSignature(SaveLifeDiseaseKnowledge knowledge)
    {
        return string.Join("\u001F", new[]
        {
            knowledge.IsPublished ? "published" : "offline",
            knowledge.Title,
            knowledge.DiseaseName,
            knowledge.Author,
            knowledge.Summary,
            knowledge.Content,
            knowledge.Tags,
            knowledge.SortOrder.ToString()
        });
    }

    private static string BuildTrialSignature(SaveLifeClinicalTrial trial)
    {
        return string.Join("\u001F", new[]
        {
            trial.Status,
            trial.Title,
            trial.DiseaseArea,
            trial.Indication,
            trial.ClinicalPhase,
            trial.Sponsor,
            trial.Location,
            trial.Sites,
            trial.Requirements,
            trial.InclusionCriteria,
            trial.ExclusionCriteria,
            trial.Description,
            trial.Benefits,
            trial.IsPinned.ToString(),
            trial.SortOrder.ToString()
        });
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

public sealed class SaveLifePublicKnowledgeBaseIndexBatchResult
{
    public int KnowledgeTotalCount { get; set; }

    public int TrialTotalCount { get; set; }

    public int TotalCount => KnowledgeTotalCount + TrialTotalCount;

    public int SyncedCount { get; private set; }

    public int RemovedCount { get; private set; }

    public int SkippedCount { get; private set; }

    public int FailedCount { get; private set; }

    public IReadOnlyList<string> FailureMessages => _failureMessages;

    private readonly List<string> _failureMessages = [];

    public void Add(SaveLifePublicKnowledgeBaseIndexItemResult result)
    {
        if (result.Status == "synced")
        {
            SyncedCount++;
            return;
        }

        if (result.Status == "removed")
        {
            RemovedCount++;
            return;
        }

        if (result.Status == "failed")
        {
            FailedCount++;
            if (!string.IsNullOrWhiteSpace(result.Message) && _failureMessages.Count < 5)
                _failureMessages.Add($"{result.ResourceType}:{result.ResourceId} {result.Message}");
            return;
        }

        SkippedCount++;
    }
}

public sealed class SaveLifePublicKnowledgeBaseIndexItemResult
{
    public string ResourceType { get; private init; } = string.Empty;

    public long ResourceId { get; private init; }

    public string Status { get; private init; } = "skipped";

    public string? Message { get; private init; }

    public static SaveLifePublicKnowledgeBaseIndexItemResult Synced(string resourceType, long resourceId)
    {
        return new SaveLifePublicKnowledgeBaseIndexItemResult
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            Status = "synced"
        };
    }

    public static SaveLifePublicKnowledgeBaseIndexItemResult Removed(string resourceType, long resourceId)
    {
        return new SaveLifePublicKnowledgeBaseIndexItemResult
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            Status = "removed"
        };
    }

    public static SaveLifePublicKnowledgeBaseIndexItemResult Skipped(string resourceType, long resourceId, string? message)
    {
        return new SaveLifePublicKnowledgeBaseIndexItemResult
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            Status = "skipped",
            Message = message
        };
    }

    public static SaveLifePublicKnowledgeBaseIndexItemResult Failed(string resourceType, long resourceId, string? message)
    {
        return new SaveLifePublicKnowledgeBaseIndexItemResult
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            Status = "failed",
            Message = message
        };
    }
}
