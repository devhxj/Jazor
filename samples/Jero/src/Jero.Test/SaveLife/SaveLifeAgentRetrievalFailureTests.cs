using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentRetrievalFailureTests
{
    [Fact]
    public async Task ExecuteAsync_WhenRetrievalDependencyFails_ThrowsTypedRetrievalFailure()
    {
        var dependencyError = new InvalidOperationException("vector index unavailable");

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            SaveLifeAgentRetrievalFailureBoundary.ExecuteAsync<int>(() =>
                Task.FromException<int>(dependencyError),
                TestContext.Current.CancellationToken));

        Assert.Equal(SaveLifeAgentFailureCodes.RetrievalUnavailable, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Retrieval, error.Stage);
        Assert.True(error.Retryable);
        Assert.Same(dependencyError, error.InnerException);
        Assert.Equal(SaveLifeAgentFailureContract.PublicMessage, error.Message);
    }

    [Fact]
    public void RequireEmbedding_WhenProviderReturnsEmpty_ThrowsTypedRetrievalFailure()
    {
        var error = Assert.Throws<SaveLifeAgentUnavailableException>(() =>
            SaveLifeAgentRetrievalFailureBoundary.RequireEmbedding([], "text"));

        Assert.Equal(SaveLifeAgentFailureCodes.RetrievalUnavailable, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Retrieval, error.Stage);
        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCallerCancels_PropagatesCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            SaveLifeAgentRetrievalFailureBoundary.ExecuteAsync<int>(() =>
                Task.FromCanceled<int>(cancellation.Token), cancellation.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WhenDependencyCancelsWithoutCallerCancellation_ThrowsTypedRetrievalFailure()
    {
        using var dependencyCancellation = new CancellationTokenSource();
        dependencyCancellation.Cancel();

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            SaveLifeAgentRetrievalFailureBoundary.ExecuteAsync<int>(() =>
                Task.FromCanceled<int>(dependencyCancellation.Token),
                TestContext.Current.CancellationToken));

        Assert.Equal(SaveLifeAgentFailureCodes.RetrievalUnavailable, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Retrieval, error.Stage);
        Assert.IsAssignableFrom<OperationCanceledException>(error.InnerException);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRetrievalHasNoMatches_ReturnsNormalEmptyResult()
    {
        var result = await SaveLifeAgentRetrievalFailureBoundary.ExecuteAsync(() =>
            Task.FromResult(new SaveLifeAgentRetrievalResult()),
            TestContext.Current.CancellationToken);

        Assert.Empty(result.Knowledge);
        Assert.Empty(result.Trials);
        Assert.Empty(result.Materials);
    }
}
