using Furion.TaskQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeKnowledgeBaseIndexTaskDispatcher
{
    ValueTask DispatchKnowledgeAsync(long knowledgeId);

    ValueTask DispatchTrialAsync(long trialId);

    ValueTask<bool> DispatchMaterialAsync(long materialId);
}

public sealed class SaveLifeKnowledgeBaseIndexTaskDispatcher : ISaveLifeKnowledgeBaseIndexTaskDispatcher, ITransient
{
    internal const string PublicChannel = "savelife-public-knowledge-base-index";
    private const string MaterialChannel = "savelife-material-private-knowledge-base-index";

    private readonly ITaskQueue _taskQueue;
    private readonly ILogger<SaveLifeKnowledgeBaseIndexTaskDispatcher> _logger;

    public SaveLifeKnowledgeBaseIndexTaskDispatcher(
        ITaskQueue taskQueue,
        ILogger<SaveLifeKnowledgeBaseIndexTaskDispatcher> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public async ValueTask DispatchKnowledgeAsync(long knowledgeId)
    {
        try
        {
            await _taskQueue.EnqueueAsync(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ISaveLifePublicKnowledgeBaseIndexProcessor>()
                    .ProcessKnowledgeAsync(knowledgeId, cancellationToken);
            }, wrapper => wrapper
                .WithChannel(PublicChannel)
                .WithTaskId($"knowledge:{knowledgeId}")
                .WithConcurrent(false)
                .WithDisableRetry(true));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispatch SaveLife knowledge index task. KnowledgeId={KnowledgeId}", knowledgeId);
        }
    }

    public async ValueTask DispatchTrialAsync(long trialId)
    {
        try
        {
            await _taskQueue.EnqueueAsync(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ISaveLifePublicKnowledgeBaseIndexProcessor>()
                    .ProcessTrialAsync(trialId, cancellationToken);
            }, wrapper => wrapper
                .WithChannel(PublicChannel)
                .WithTaskId($"trial:{trialId}")
                .WithConcurrent(false)
                .WithDisableRetry(true));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispatch SaveLife trial index task. TrialId={TrialId}", trialId);
        }
    }

    public async ValueTask<bool> DispatchMaterialAsync(long materialId)
    {
        try
        {
            await _taskQueue.EnqueueAsync(async (serviceProvider, cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ISaveLifeMaterialIndexProcessor>()
                    .ProcessMaterialAsync(materialId, cancellationToken);
            }, wrapper => wrapper
                .WithChannel(MaterialChannel)
                .WithTaskId($"material:{materialId}")
                .WithConcurrent(false)
                .WithDisableRetry(true));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispatch SaveLife material index task. MaterialId={MaterialId}", materialId);
            return false;
        }
    }
}
