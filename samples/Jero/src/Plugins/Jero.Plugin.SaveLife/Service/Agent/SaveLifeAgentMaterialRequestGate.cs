namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentMaterialRequestGate
{
    Task<IAsyncDisposable> AcquireAsync(
        string externalUserId,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeAgentMaterialRequestGate : ISaveLifeAgentMaterialRequestGate, ISingleton
{
    private readonly Dictionary<string, AccountGate> _accounts = new(StringComparer.Ordinal);
    private readonly object _accountLock = new();
    private readonly Dictionary<string, RateWindow> _rateWindows = new(StringComparer.Ordinal);
    private readonly object _rateLock = new();
    private readonly SemaphoreSlim _processGate;
    private readonly int _queueLimit;
    private readonly TimeSpan _queueTimeout;
    private readonly int _accountRequestsPerWindow;
    private readonly TimeSpan _rateWindow;
    private readonly Func<DateTimeOffset> _utcNow;
    private int _waitingCount;

    public SaveLifeAgentMaterialRequestGate()
        : this(
            SaveLifeMaterialResourceBudget.MaterialReviewProcessConcurrency,
            SaveLifeMaterialResourceBudget.MaterialReviewQueueLength,
            TimeSpan.FromMilliseconds(SaveLifeMaterialResourceBudget.MaterialReviewQueueTimeoutMilliseconds),
            SaveLifeMaterialResourceBudget.MaterialReviewAccountRequestsPerMinute,
            TimeSpan.FromMinutes(1))
    {
    }

    internal SaveLifeAgentMaterialRequestGate(
        int processLimit,
        int queueLimit,
        TimeSpan queueTimeout,
        int accountRequestsPerWindow,
        TimeSpan rateWindow,
        Func<DateTimeOffset>? utcNow = null)
    {
        if (processLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(processLimit));
        if (queueLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(queueLimit));
        if (queueTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(queueTimeout));
        if (accountRequestsPerWindow <= 0)
            throw new ArgumentOutOfRangeException(nameof(accountRequestsPerWindow));
        if (rateWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(rateWindow));

        _processGate = new SemaphoreSlim(processLimit, processLimit);
        _queueLimit = queueLimit;
        _queueTimeout = queueTimeout;
        _accountRequestsPerWindow = accountRequestsPerWindow;
        _rateWindow = rateWindow;
        _utcNow = utcNow ?? TimeProvider.System.GetUtcNow;
    }

    public async Task<IAsyncDisposable> AcquireAsync(
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        var accountKey = externalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(accountKey) || accountKey.Length > 256)
            throw Rejected("account_invalid", retryable: false);
        EnforceRateLimit(accountKey);

        var waiting = Interlocked.Increment(ref _waitingCount);
        if (waiting > _queueLimit)
        {
            Interlocked.Decrement(ref _waitingCount);
            throw Rejected("queue_full");
        }

        var accountGate = AddAccountReference(accountKey);
        var accountAcquired = false;
        var processAcquired = false;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_queueTimeout);
        try
        {
            await accountGate.Semaphore.WaitAsync(timeout.Token);
            accountAcquired = true;
            await _processGate.WaitAsync(timeout.Token);
            processAcquired = true;
            return new Lease(this, accountKey, accountGate);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw Rejected("queue_timeout");
        }
        finally
        {
            Interlocked.Decrement(ref _waitingCount);
            if (!processAcquired)
            {
                if (accountAcquired)
                    accountGate.Semaphore.Release();
                ReleaseAccountReference(accountKey, accountGate);
            }
        }
    }

    private AccountGate AddAccountReference(string accountKey)
    {
        lock (_accountLock)
        {
            if (!_accounts.TryGetValue(accountKey, out var gate))
            {
                gate = new AccountGate();
                _accounts.Add(accountKey, gate);
            }

            gate.ReferenceCount++;
            return gate;
        }
    }

    private void Release(string accountKey, AccountGate accountGate)
    {
        _processGate.Release();
        accountGate.Semaphore.Release();
        ReleaseAccountReference(accountKey, accountGate);
    }

    private void ReleaseAccountReference(string accountKey, AccountGate accountGate)
    {
        lock (_accountLock)
        {
            accountGate.ReferenceCount--;
            if (accountGate.ReferenceCount != 0)
                return;

            if (_accounts.TryGetValue(accountKey, out var current) &&
                ReferenceEquals(current, accountGate))
            {
                _accounts.Remove(accountKey);
            }
        }
    }

    private void EnforceRateLimit(string accountKey)
    {
        lock (_rateLock)
        {
            var now = _utcNow();
            if (!_rateWindows.TryGetValue(accountKey, out var window) ||
                now - window.Start >= _rateWindow)
            {
                _rateWindows[accountKey] = new RateWindow(now, 1);
                RemoveExpiredRateWindows(now);
                return;
            }

            if (window.Count >= _accountRequestsPerWindow)
                throw Rejected("rate_limit");
            _rateWindows[accountKey] = window with { Count = window.Count + 1 };
        }
    }

    private void RemoveExpiredRateWindows(DateTimeOffset now)
    {
        if (_rateWindows.Count <= 1024)
            return;

        foreach (var key in _rateWindows
            .Where(item => now - item.Value.Start >= _rateWindow)
            .Select(item => item.Key)
            .Take(256)
            .ToList())
        {
            _rateWindows.Remove(key);
        }
    }

    private static SaveLifeAgentUnavailableException Rejected(
        string reason,
        bool retryable = true) =>
        new(
            SaveLifeAgentFailureCodes.MaterialNotReady,
            SaveLifeAgentFailureStages.Material,
            new InvalidOperationException($"Patient material request rejected: {reason}."),
            retryable);

    private sealed class AccountGate
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int ReferenceCount;
    }

    private sealed record RateWindow(DateTimeOffset Start, int Count);

    private sealed class Lease(
        SaveLifeAgentMaterialRequestGate owner,
        string accountKey,
        AccountGate accountGate) : IAsyncDisposable
    {
        private SaveLifeAgentMaterialRequestGate? _owner = owner;

        public ValueTask DisposeAsync()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            current?.Release(accountKey, accountGate);
            return ValueTask.CompletedTask;
        }
    }
}
