using Jazor.NodeHost;
using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore;

/// <summary>
/// Executes generated Vue roots through a bounded, generation-aware pool of persistent NodeApi environments.
/// pool 的 generation 包含 SSR 入口和依赖声明/锁；变化后轮换环境以清除 ESM cache。
/// </summary>
internal sealed class SsrRenderer : IJazorSsrRenderer, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    private readonly SsrArtifactLocator _artifactLocator;
    private readonly int _workerCount;
    private readonly SemaphoreSlim _generationGate = new(1, 1);
    private readonly SemaphoreSlim _renderCapacity;
    private SsrArtifactStamp? _poolStamp;
    private SsrArtifactGeneration? _poolGeneration;
    private SsrWorkerPool? _pool;
    private volatile bool _disposed;
    private int _disposeStarted;

    public SsrRenderer(
        SsrArtifactLocator artifactLocator,
        IOptions<JazorSsrOptions> options)
    {
        _artifactLocator = artifactLocator ?? throw new ArgumentNullException(nameof(artifactLocator));
        ArgumentNullException.ThrowIfNull(options);
        _workerCount = options.Value.WorkerCount;
        if (_workerCount <= 0)
            throw new InvalidOperationException("Jazor SSR WorkerCount must be greater than zero.");
        _renderCapacity = new SemaphoreSlim(_workerCount, _workerCount);
    }

    /// <inheritdoc />
    public async Task<JazorSsrRenderResult> RenderAsync(
        JazorSsrRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _renderCapacity.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var modulePath = SsrArtifactLocator.NormalizeRelativePath(request.ModulePath, "module path");
            var state = JazorSsrStateEnvelope.Create(request);
            var serializedState = JsonSerializer.Serialize(state, JsonOptions);
            var serializedProps = JsonSerializer.Serialize(request.Props, JsonOptions);
            var serializedProviders = JsonSerializer.Serialize(state.Providers, JsonOptions);
            using var stateDocument = JsonDocument.Parse(serializedState);
            var payload = new SsrRenderPayload(
                modulePath,
                stateDocument.RootElement);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var artifacts = _artifactLocator.Resolve();
                var pool = await GetPoolAsync(artifacts, cancellationToken).ConfigureAwait(false);

                try
                {
                    var html = await pool.RenderAsync(payload, cancellationToken).ConfigureAwait(false);
                    return new JazorSsrRenderResult(modulePath, html, serializedProps, serializedProviders, serializedState);
                }
                catch (SsrGenerationRetiredException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Project inputs may change between Resolve() and the pool lease.
                    // 新 generation 已接管时重新解析，绝不把新请求送回 retired worker。
                }
            }
        }
        finally
        {
            _renderCapacity.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
            return;

        _disposed = true;
        // Stop admitting work, then drain every permit before touching a worker. Existing renders
        // complete normally; queued calls observe ObjectDisposedException after acquiring a permit.
        // 释放宿主时先 drain 全局并发槽，避免 disposal 在正常 SSR 中途终止环境。
        for (var index = 0; index < _workerCount; index++)
            await _renderCapacity.WaitAsync().ConfigureAwait(false);

        await _generationGate.WaitAsync().ConfigureAwait(false);
        SsrWorkerPool? pool;
        try
        {
            pool = _pool;
            _pool = null;
            _poolStamp = null;
            _poolGeneration = null;
        }
        finally
        {
            _generationGate.Release();
        }

        if (pool is not null)
            await pool.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<SsrWorkerPool> GetPoolAsync(
        SsrArtifacts artifacts,
        CancellationToken cancellationToken)
    {
        var stamp = SsrArtifactStamp.Capture(artifacts);
        await _generationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        SsrWorkerPool? retiredPool = null;
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_pool is not null && Equals(_poolStamp, stamp))
                return _pool;

            // Content hashing only occurs after a cheap file-stamp change. Normal warm renders
            // therefore pay metadata probes, while timestamp-only rewrites keep the live pool.
            // 包版本变化也必须轮换 worker，否则 Node 的 ESM cache 会继续持有旧依赖。
            var generation = SsrArtifactGeneration.Create(artifacts);
            if (_pool is not null && Equals(_poolGeneration, generation))
            {
                _poolStamp = stamp;
                return _pool;
            }

            retiredPool = _pool;
            _pool = new SsrWorkerPool(generation, artifacts, _workerCount, JsonOptions);
            _poolStamp = stamp;
            _poolGeneration = generation;
            return _pool;
        }
        finally
        {
            _generationGate.Release();
            if (retiredPool is not null)
                await retiredPool.RetireAsync().ConfigureAwait(false);
        }
    }

    private sealed class SsrWorkerPool(
        SsrRenderer.SsrArtifactGeneration generation,
        SsrArtifacts artifacts,
        int workerCount,
        JsonSerializerOptions jsonOptions) : IAsyncDisposable
    {
        private readonly SsrArtifactGeneration _generation = generation;
        private readonly SsrArtifacts _artifacts = artifacts;
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
        private readonly SemaphoreSlim _capacity = new SemaphoreSlim(workerCount, workerCount);
        private readonly object _gate = new();
        private readonly Queue<SsrWorker> _idleWorkers = new();
        private readonly HashSet<SsrWorker> _workers = [];
        private bool _retired;
        private bool _disposed;

        public async Task<string> RenderAsync(
            SsrRenderPayload payload,
            CancellationToken cancellationToken)
        {
            await _capacity.WaitAsync(cancellationToken).ConfigureAwait(false);
            SsrWorker? worker = null;
            try
            {
                lock (_gate)
                {
                    if (_retired || _disposed)
                        throw new SsrGenerationRetiredException();

                    if (_idleWorkers.Count > 0)
                    {
                        worker = _idleWorkers.Dequeue();
                    }
                    else
                    {
                        worker = new SsrWorker(_artifacts, _jsonOptions);
                        _workers.Add(worker);
                    }
                }

                await worker.EnsureStartedAsync(cancellationToken).ConfigureAwait(false);
                return await worker.RenderAsync(payload, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (worker is not null)
                    await ReturnAsync(worker).ConfigureAwait(false);
                _capacity.Release();
            }
        }

        public async ValueTask RetireAsync()
        {
            List<SsrWorker> idle;
            lock (_gate)
            {
                if (_retired)
                    return;

                _retired = true;
                idle = [.. _idleWorkers];
                _idleWorkers.Clear();
                foreach (var worker in idle)
                    _workers.Remove(worker);
            }

            await DisposeWorkersAsync(idle).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            List<SsrWorker> workers;
            lock (_gate)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _retired = true;
                workers = [.. _workers];
                _workers.Clear();
                _idleWorkers.Clear();
            }

            await DisposeWorkersAsync(workers).ConfigureAwait(false);
        }

        private async ValueTask ReturnAsync(SsrWorker worker)
        {
            var keep = false;
            lock (_gate)
            {
                if (!_retired && !_disposed && worker.IsHealthy)
                {
                    _idleWorkers.Enqueue(worker);
                    keep = true;
                }
                else
                {
                    _workers.Remove(worker);
                }
            }

            if (!keep)
                await worker.DisposeAsync().ConfigureAwait(false);
        }

        private static async ValueTask DisposeWorkersAsync(IEnumerable<SsrWorker> workers)
        {
            foreach (var worker in workers)
                await worker.DisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class SsrWorker : IAsyncDisposable
    {
        private readonly SsrArtifacts _artifacts;
        private readonly JsonSerializerOptions _jsonOptions;
        private NodeEnvironment? _environment;
        private bool _healthy = true;

        public SsrWorker(SsrArtifacts artifacts, JsonSerializerOptions jsonOptions)
        {
            _artifacts = artifacts;
            _jsonOptions = jsonOptions;
        }

        public bool IsHealthy => _healthy && _environment is not null;

        public Task EnsureStartedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _environment ??= new NodeEnvironment(_artifacts.RootPath);
            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(SsrRenderPayload payload, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var request = JsonSerializer.Serialize(new SsrExecutionRequest(
                    _artifacts.RootPath, payload.ModulePath, payload.State), _jsonOptions);
                // Cancellation retires this environment and joins its JS thread
                // before releasing capacity. Never replay a render whose hooks
                // may already have produced side effects.
                var html = await NodeToolchain.RenderAsync(_environment!, _artifacts.RootPath, request, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return html;
            }
            catch (Microsoft.JavaScript.NodeApi.JSException exception)
            {
                _healthy = false;
                throw new InvalidOperationException("Jazor SSR execution failed. " + exception.Message, exception);
            }
            catch
            {
                _healthy = false;
                throw;
            }
        }

        public ValueTask DisposeAsync()
        {
            _healthy = false;
            Interlocked.Exchange(ref _environment, null)?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
    private sealed record SsrRenderPayload(
        string ModulePath,
        JsonElement State);

    // The runner protocol is a JavaScript-owned ABI. Keep field names explicit so the
    // host-wide CLR naming policy never becomes an accidental transport convention.
    private sealed record SsrExecutionRequest(
        [property: JsonPropertyName("projectDirectory")] string ProjectDirectory,
        [property: JsonPropertyName("modulePath")] string ModulePath,
        [property: JsonPropertyName("state")] JsonElement State);

    private sealed record SsrArtifactStamp(
        string RootPath,
        FileStamp SsrEntry,
        FileStamp Package,
        FileStamp? Lock,
        FileStamp SourceTree)
    {
        public static SsrArtifactStamp Capture(SsrArtifacts artifacts)
            => new(
                NormalizeRoot(artifacts.RootPath),
                FileStamp.Capture(artifacts.SsrEntryPath),
                FileStamp.Capture(Path.Combine(artifacts.RootPath, "package.json")),
                artifacts.IsBundled ? null : FileStamp.Capture(Path.Combine(artifacts.RootPath, "pnpm-lock.yaml")),
                FileStamp.CaptureSourceTree(artifacts.RootPath));
    }

    private sealed record SsrArtifactGeneration(
        string RootPath,
        string SsrEntryHash,
        string PackageHash,
        string? LockHash,
        string SourceTreeHash)
    {
        public string Id => SsrEntryHash[..12] + ":" +
                            PackageHash[..12] + ":" + (LockHash?[..12] ?? "bundled") + ":" + SourceTreeHash[..12];

        public static SsrArtifactGeneration Create(SsrArtifacts artifacts)
            => new(
                NormalizeRoot(artifacts.RootPath),
                ComputeFileHash(artifacts.SsrEntryPath),
                ComputeFileHash(Path.Combine(artifacts.RootPath, "package.json")),
                // The locator marks a bundled runtime explicitly. Source projects still
                // require their restore lock; published local chunks do not have one.
                artifacts.IsBundled ? null : ComputeFileHash(Path.Combine(artifacts.RootPath, "pnpm-lock.yaml")),
                ComputeSourceTreeHash(artifacts.RootPath));
    }

    private sealed record FileStamp(long Length, long LastWriteTimeUtcTicks)
    {
        public static FileStamp Capture(string path)
        {
            var file = new FileInfo(path);
            file.Refresh();
            if (!file.Exists)
                throw new FileNotFoundException("Jazor SSR generation input was not found.", path);
            return new FileStamp(file.Length, file.LastWriteTimeUtc.Ticks);
        }

        public static FileStamp CaptureSourceTree(string rootPath)
        {
            long length = 0;
            long latest = 0;
            foreach (var path in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
                         .Where(static path => !path.Contains(Path.DirectorySeparatorChar + "node_modules" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                            && !path.Contains(Path.DirectorySeparatorChar + "dist" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                            && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                            && !path.Contains(Path.DirectorySeparatorChar + ".jazor" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
            {
                var file = new FileInfo(path);
                length = unchecked(length + file.Length);
                latest = Math.Max(latest, file.LastWriteTimeUtc.Ticks);
            }

            return new FileStamp(length, latest);
        }
    }

    private static string NormalizeRoot(string path)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        return OperatingSystem.IsWindows() ? fullPath.ToUpperInvariant() : fullPath;
    }

    private static string ComputeFileHash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string ComputeSourceTreeHash(string rootPath)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var path in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
                     .Where(static path => !path.Contains(Path.DirectorySeparatorChar + "node_modules" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                        && !path.Contains(Path.DirectorySeparatorChar + "dist" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                            && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                            && !path.Contains(Path.DirectorySeparatorChar + ".jazor" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(rootPath, path).Replace('\\', '/');
            hash.AppendData(Encoding.UTF8.GetBytes(relative));
            hash.AppendData([0]);
            hash.AppendData(File.ReadAllBytes(path));
            hash.AppendData([0]);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private sealed class SsrGenerationRetiredException : InvalidOperationException;
}
