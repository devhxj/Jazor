using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeKnowledgeBaseIndexJobTests
{
    [Fact]
    public async Task PublicJob_WhenAnItemFails_ReportsFailureToScheduler()
    {
        var batch = new SaveLifePublicKnowledgeBaseIndexBatchResult();
        batch.Add(SaveLifePublicKnowledgeBaseIndexItemResult.Failed(
            "trial",
            123,
            "Embedding API Key 未配置"));
        var processor = new StubPublicProcessor(batch);
        var job = new SaveLifePublicKnowledgeBaseIndexJob(
            processor,
            NullLogger<SaveLifePublicKnowledgeBaseIndexJob>.Instance);
        var context = CreateContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ExecuteAsync(job, context));

        Assert.Contains("Failed=1", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Embedding API Key 未配置", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Failed=1", GetResult(context), StringComparison.Ordinal);
        Assert.Contains("Embedding API Key 未配置", GetResult(context), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicJob_WhenBatchSucceeds_StoresSummaryInExecutionRecord()
    {
        var batch = new SaveLifePublicKnowledgeBaseIndexBatchResult
        {
            KnowledgeTotalCount = 1,
            TrialTotalCount = 1
        };
        batch.Add(SaveLifePublicKnowledgeBaseIndexItemResult.Synced("knowledge", 1));
        batch.Add(SaveLifePublicKnowledgeBaseIndexItemResult.Removed("trial", 2));
        var job = new SaveLifePublicKnowledgeBaseIndexJob(
            new StubPublicProcessor(batch),
            NullLogger<SaveLifePublicKnowledgeBaseIndexJob>.Instance);
        var context = CreateContext();

        await ExecuteAsync(job, context);

        Assert.Equal(
            "Total=2, Synced=1, Removed=1, Skipped=0, Failed=0",
            GetResult(context));
    }

    private static object CreateContext()
    {
        var contextType = typeof(SaveLifePublicKnowledgeBaseIndexJob)
            .GetMethod(nameof(SaveLifePublicKnowledgeBaseIndexJob.ExecuteAsync))!
            .GetParameters()[0]
            .ParameterType;
        return RuntimeHelpers.GetUninitializedObject(contextType);
    }

    private static async Task ExecuteAsync(SaveLifePublicKnowledgeBaseIndexJob job, object context)
    {
        var task = (Task)typeof(SaveLifePublicKnowledgeBaseIndexJob)
            .GetMethod(nameof(SaveLifePublicKnowledgeBaseIndexJob.ExecuteAsync))!
            .Invoke(job, [context, CancellationToken.None])!;
        await task;
    }

    private static string? GetResult(object context)
    {
        return context.GetType().GetProperty("Result")!.GetValue(context)?.ToString();
    }

    private sealed class StubPublicProcessor(SaveLifePublicKnowledgeBaseIndexBatchResult batch)
        : ISaveLifePublicKnowledgeBaseIndexProcessor
    {
        public Task<SaveLifePublicKnowledgeBaseIndexBatchResult> ProcessPendingAsync(
            int limit,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(batch);
        }

        public Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessKnowledgeAsync(
            long knowledgeId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SaveLifePublicKnowledgeBaseIndexItemResult> ProcessTrialAsync(
            long trialId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task QueueKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task QueueTrialAsync(long trialId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveKnowledgeAsync(long knowledgeId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveTrialAsync(long trialId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
