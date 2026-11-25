using Furion.Schedule;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeMaterialFileCleanup
{
    Task DeleteAsync(long fileId);
}

public interface ISaveLifeMaterialIndexCleanup
{
    Task DeleteAsync(long materialId, CancellationToken cancellationToken = default);
}

public sealed class SaveLifeMaterialIndexCleanup(
    SqlSugarRepository<SaveLifeAgentVectorIndex> indexRep)
    : ISaveLifeMaterialIndexCleanup, ITransient
{
    public async Task DeleteAsync(long materialId, CancellationToken cancellationToken = default)
    {
        await indexRep.Context.Deleteable<SaveLifeAgentVectorIndex>()
            .Where(item => item.ResourceType == "material" && item.ResourceId == materialId)
            .ExecuteCommandAsync(cancellationToken);
    }
}

public sealed class SaveLifeMaterialFileCleanup(SysFileService fileService)
    : ISaveLifeMaterialFileCleanup, ITransient
{
    public async Task DeleteAsync(long fileId)
    {
        await fileService.TryDeleteFile(fileId);
    }
}

public interface ISaveLifeMaterialWorkOutboxProcessor
{
    Task<SaveLifeMaterialOutboxBatchResult> ProcessPendingAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<bool> ProcessOneAsync(long outboxId, CancellationToken cancellationToken = default);
}

public sealed class SaveLifeMaterialWorkOutboxProcessor : ISaveLifeMaterialWorkOutboxProcessor, ITransient
{
    private const int MaximumBatchSize = 200;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);

    private readonly SqlSugarRepository<SaveLifeMaterialWorkOutbox> _outboxRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly ISaveLifeKnowledgeBaseIndexTaskDispatcher _dispatcher;
    private readonly ISaveLifeMaterialFileCleanup _fileCleanup;
    private readonly ISaveLifeMaterialIndexCleanup _indexCleanup;

    public SaveLifeMaterialWorkOutboxProcessor(
        SqlSugarRepository<SaveLifeMaterialWorkOutbox> outboxRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        ISaveLifeKnowledgeBaseIndexTaskDispatcher dispatcher,
        ISaveLifeMaterialFileCleanup fileCleanup,
        ISaveLifeMaterialIndexCleanup indexCleanup)
    {
        _outboxRep = outboxRep;
        _materialRep = materialRep;
        _dispatcher = dispatcher;
        _fileCleanup = fileCleanup;
        _indexCleanup = indexCleanup;
    }

    public async Task<SaveLifeMaterialOutboxBatchResult> ProcessPendingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var batchSize = Math.Clamp(limit, 1, MaximumBatchSize);
        var items = await _outboxRep.AsQueryable()
            .ClearFilter()
            .Where(item =>
                !item.IsDelete &&
                ((item.Status == SaveLifeMaterialWorkStatuses.Pending &&
                  (item.NextAttemptTime == null || item.NextAttemptTime <= now)) ||
                 (item.Status == SaveLifeMaterialWorkStatuses.Dispatching &&
                  item.LeaseExpiresTime != null && item.LeaseExpiresTime <= now)))
            .OrderBy(item => item.NextAttemptTime, OrderByType.Asc)
            .OrderBy(item => item.CreateTime, OrderByType.Asc)
            .Take(batchSize)
            .ToListAsync();
        var result = new SaveLifeMaterialOutboxBatchResult { TotalCount = items.Count };
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ProcessOneAsync(item.Id, cancellationToken))
                result.CompletedCount++;
            else
                result.FailedCount++;
        }
        return result;
    }

    public async Task<bool> ProcessOneAsync(
        long outboxId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var owner = Guid.NewGuid().ToString("N");
        var expires = now.Add(LeaseDuration);
        var claimed = await _outboxRep.Context.Updateable<SaveLifeMaterialWorkOutbox>()
            .SetColumns(item => new SaveLifeMaterialWorkOutbox
            {
                Status = SaveLifeMaterialWorkStatuses.Dispatching,
                LeaseOwner = owner,
                LeaseExpiresTime = expires,
                NextAttemptTime = null,
                UpdateTime = now
            })
            .Where(item =>
                item.Id == outboxId &&
                !item.IsDelete &&
                ((item.Status == SaveLifeMaterialWorkStatuses.Pending &&
                  (item.NextAttemptTime == null || item.NextAttemptTime <= now)) ||
                 (item.Status == SaveLifeMaterialWorkStatuses.Dispatching &&
                  item.LeaseExpiresTime != null && item.LeaseExpiresTime <= now)))
            .ExecuteCommandAsync(cancellationToken);
        if (claimed != 1)
            return false;

        var item = await _outboxRep.AsQueryable()
            .ClearFilter()
            .Where(candidate => candidate.Id == outboxId && candidate.LeaseOwner == owner)
            .FirstAsync(cancellationToken)
            ?? throw new InvalidOperationException("已领取材料任务无法重读");
        try
        {
            var completedStatus = item.WorkType switch
            {
                SaveLifeMaterialWorkTypes.IndexMaterial =>
                    await _dispatcher.DispatchMaterialAsync(item.AggregateId)
                        ? SaveLifeMaterialWorkStatuses.Dispatched
                        : null,
                SaveLifeMaterialWorkTypes.CleanupFile =>
                    await CleanupFileAsync(item.AggregateId)
                        ? SaveLifeMaterialWorkStatuses.Completed
                        : null,
                SaveLifeMaterialWorkTypes.CleanupMaterialIndex =>
                    await CleanupMaterialIndexAsync(item.AggregateId, cancellationToken)
                        ? SaveLifeMaterialWorkStatuses.Completed
                        : null,
                _ => throw new InvalidOperationException("未知材料 outbox 任务类型")
            };
            if (completedStatus == null)
            {
                await MarkFailedAsync(item, owner, "dispatch_unavailable", now, cancellationToken);
                return false;
            }

            var completed = await _outboxRep.Context.Updateable<SaveLifeMaterialWorkOutbox>()
                .SetColumns(candidate => new SaveLifeMaterialWorkOutbox
                {
                    Status = completedStatus,
                    LeaseOwner = null,
                    LeaseExpiresTime = null,
                    LastErrorCode = null,
                    NextAttemptTime = null,
                    UpdateTime = DateTime.Now
                })
                .Where(candidate =>
                    candidate.Id == item.Id &&
                    candidate.Status == SaveLifeMaterialWorkStatuses.Dispatching &&
                    candidate.LeaseOwner == owner &&
                    !candidate.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            return completed == 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            var errorCode = item.WorkType switch
            {
                SaveLifeMaterialWorkTypes.CleanupFile => "file_cleanup_failed",
                SaveLifeMaterialWorkTypes.CleanupMaterialIndex => "index_cleanup_failed",
                _ => "dispatch_failed"
            };
            await MarkFailedAsync(item, owner, errorCode, now, cancellationToken);
            return false;
        }
    }

    private async Task<bool> CleanupFileAsync(long fileId)
    {
        var hasActiveReference = await _materialRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.FileId == fileId && !item.IsDelete)
            .AnyAsync();
        if (hasActiveReference)
            return true;

        await _fileCleanup.DeleteAsync(fileId);
        return true;
    }

    private async Task<bool> CleanupMaterialIndexAsync(
        long materialId,
        CancellationToken cancellationToken)
    {
        await _indexCleanup.DeleteAsync(materialId, cancellationToken);
        return true;
    }

    private async Task MarkFailedAsync(
        SaveLifeMaterialWorkOutbox item,
        string owner,
        string errorCode,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var attemptCount = checked(item.AttemptCount + 1);
        var delaySeconds = Math.Min(300, 1 << Math.Min(attemptCount, 8));
        await _outboxRep.Context.Updateable<SaveLifeMaterialWorkOutbox>()
            .SetColumns(candidate => new SaveLifeMaterialWorkOutbox
            {
                Status = SaveLifeMaterialWorkStatuses.Pending,
                LeaseOwner = null,
                LeaseExpiresTime = null,
                AttemptCount = attemptCount,
                NextAttemptTime = now.AddSeconds(delaySeconds),
                LastErrorCode = errorCode,
                UpdateTime = now
            })
            .Where(candidate =>
                candidate.Id == item.Id &&
                candidate.Status == SaveLifeMaterialWorkStatuses.Dispatching &&
                candidate.LeaseOwner == owner &&
                !candidate.IsDelete)
            .ExecuteCommandAsync(cancellationToken);
    }
}

public sealed class SaveLifeMaterialOutboxBatchResult
{
    public int TotalCount { get; set; }

    public int CompletedCount { get; set; }

    public int FailedCount { get; set; }
}

[JobDetail(
    "SaveLifeMaterialWorkOutboxJob",
    Description = "医线生机材料可靠任务派发与清理",
    GroupName = "default",
    Concurrent = false)]
[PeriodMinutes(
    1,
    TriggerId = "SaveLifeMaterialWorkOutboxTrigger",
    Description = "每分钟恢复材料派发和文件清理任务",
    RunOnStart = true)]
public sealed class SaveLifeMaterialWorkOutboxJob(
    ISaveLifeMaterialWorkOutboxProcessor processor,
    ILogger<SaveLifeMaterialWorkOutboxJob> logger) : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var result = await processor.ProcessPendingAsync(100, stoppingToken);
        logger.LogInformation(
            "SaveLife material outbox completed. Total={Total}, Completed={Completed}, Failed={Failed}",
            result.TotalCount,
            result.CompletedCount,
            result.FailedCount);
    }
}
