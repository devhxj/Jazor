using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentMaterialRequestGateTests
{
    [Fact]
    public async Task AcquireAsync_SerializesSameAccountWithoutBlockingDifferentAccount()
    {
        var gate = new SaveLifeAgentMaterialRequestGate(
            processLimit: 2,
            queueLimit: 4,
            queueTimeout: TimeSpan.FromSeconds(1),
            accountRequestsPerWindow: 10,
            rateWindow: TimeSpan.FromMinutes(1));
        await using var first = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);
        var sameAccount = gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);

        await using var otherAccount = await gate.AcquireAsync(
            "account-b",
            TestContext.Current.CancellationToken);

        Assert.False(sameAccount.IsCompleted);
        await first.DisposeAsync();
        await using var second = await sameAccount;
    }

    [Fact]
    public async Task AcquireAsync_WhenQueueIsFull_RejectsWithoutLeakingAccount()
    {
        var gate = new SaveLifeAgentMaterialRequestGate(
            processLimit: 1,
            queueLimit: 1,
            queueTimeout: TimeSpan.FromSeconds(1),
            accountRequestsPerWindow: 10,
            rateWindow: TimeSpan.FromMinutes(1));
        await using var active = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);
        var queued = gate.AcquireAsync(
            "account-b",
            TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            gate.AcquireAsync("sensitive-account-c", TestContext.Current.CancellationToken));

        Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
        Assert.DoesNotContain("sensitive-account-c", error.InnerException?.Message ?? string.Empty);
        await active.DisposeAsync();
        await using var acquired = await queued;
    }

    [Fact]
    public async Task AcquireAsync_WhenQueueWaitExpires_ReturnsRetryableFailure()
    {
        var gate = new SaveLifeAgentMaterialRequestGate(
            processLimit: 1,
            queueLimit: 1,
            queueTimeout: TimeSpan.FromMilliseconds(50),
            accountRequestsPerWindow: 10,
            rateWindow: TimeSpan.FromMinutes(1));
        await using var active = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            gate.AcquireAsync("account-b", TestContext.Current.CancellationToken));

        Assert.True(error.Retryable);
        Assert.Contains("queue_timeout", error.InnerException?.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AcquireAsync_WhenAccountRateIsExceeded_RejectsNextRequest()
    {
        var gate = new SaveLifeAgentMaterialRequestGate(
            processLimit: 1,
            queueLimit: 1,
            queueTimeout: TimeSpan.FromSeconds(1),
            accountRequestsPerWindow: 2,
            rateWindow: TimeSpan.FromMinutes(1));
        await using (var first = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken))
        {
        }
        await using (var second = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken))
        {
        }

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            gate.AcquireAsync("account-a", TestContext.Current.CancellationToken));

        Assert.Contains("rate_limit", error.InnerException?.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AcquireAsync_WhenQueuedCallerCancels_ReleasesAccountForNextRequest()
    {
        var gate = new SaveLifeAgentMaterialRequestGate(
            processLimit: 1,
            queueLimit: 2,
            queueTimeout: TimeSpan.FromSeconds(5),
            accountRequestsPerWindow: 10,
            rateWindow: TimeSpan.FromMinutes(1));
        await using var active = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        var queued = gate.AcquireAsync("account-a", cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
        await active.DisposeAsync();
        await using var next = await gate.AcquireAsync(
            "account-a",
            TestContext.Current.CancellationToken);
    }
}
