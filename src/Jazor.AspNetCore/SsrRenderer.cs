using DenoHost.Core;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore;

/// <summary>
/// Executes generated Vue roots through a bounded, generation-aware pool of persistent Deno workers.
/// pool 的 generation 以 artifact publish manifest 为边界，不能把运行中 module rewrite 当作支持的热更新协议。
/// </summary>
internal sealed class SsrRenderer : IJazorSsrRenderer, IAsyncDisposable
{
    private const string RunnerResourceName = "Jazor.AspNetCore.Runtime.ssr-runner.mjs";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    private readonly SsrArtifactLocator _artifactLocator;
    private readonly int _workerCount;
    private readonly Lock _runnerGate = new();
    private readonly SemaphoreSlim _generationGate = new(1, 1);
    private readonly SemaphoreSlim _renderCapacity;
    private string? _preparedRunnerRoot;
    private string? _preparedRunnerPath;
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
            var serializedProps = JsonSerializer.Serialize(request.Props, JsonOptions);
            var serializedProviders = JsonSerializer.Serialize(request.Providers ?? [], JsonOptions);
            using var propsDocument = JsonDocument.Parse(serializedProps);
            using var providersDocument = JsonDocument.Parse(serializedProviders);
            var payload = new SsrRenderPayload(
                modulePath,
                propsDocument.RootElement,
                providersDocument.RootElement);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var artifacts = _artifactLocator.Resolve();
                var runnerPath = EnsureRunner(artifacts.RootPath);
                var pool = await GetPoolAsync(artifacts, runnerPath, cancellationToken).ConfigureAwait(false);

                try
                {
                    var html = await pool.RenderAsync(payload, cancellationToken).ConfigureAwait(false);
                    return new JazorSsrRenderResult(modulePath, html, serializedProps, serializedProviders);
                }
                catch (SsrGenerationRetiredException) when (!cancellationToken.IsCancellationRequested)
                {
                    // A build may publish a new manifest between Resolve() and the pool lease.
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
        // 释放宿主时先 drain 全局并发槽，避免 disposal 在正常 SSR 中途终止进程。
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
        string runnerPath,
        CancellationToken cancellationToken)
    {
        var stamp = SsrArtifactStamp.Capture(artifacts, runnerPath);
        await _generationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        SsrWorkerPool? retiredPool = null;
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_pool is not null && Equals(_poolStamp, stamp))
                return _pool;

            // Content hashing only occurs after a cheap file-stamp change. Normal warm renders
            // therefore pay metadata probes, while timestamp-only rewrites keep the live pool.
            // 以 manifest/import-map 内容作为 generation，而不是用易漂移的构建时间戳。
            var generation = SsrArtifactGeneration.Create(artifacts, runnerPath);
            if (_pool is not null && Equals(_poolGeneration, generation))
            {
                _poolStamp = stamp;
                return _pool;
            }

            retiredPool = _pool;
            _pool = new SsrWorkerPool(generation, artifacts, runnerPath, _workerCount, JsonOptions);
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

    private string EnsureRunner(string artifactRoot)
    {
        lock (_runnerGate)
        {
            if (string.Equals(_preparedRunnerRoot, artifactRoot, StringComparison.Ordinal) &&
                _preparedRunnerPath is not null &&
                File.Exists(_preparedRunnerPath))
            {
                return _preparedRunnerPath;
            }

            var runnerPath = Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs");
            var runnerSource = ReadRunnerSource();
            Directory.CreateDirectory(Path.GetDirectoryName(runnerPath)!);
            // The runner is packaged with this assembly; rewrite only when an upgraded host changed it.
            if (!File.Exists(runnerPath) || !string.Equals(File.ReadAllText(runnerPath), runnerSource, StringComparison.Ordinal))
                File.WriteAllText(runnerPath, runnerSource, Utf8WithoutBom);

            _preparedRunnerRoot = artifactRoot;
            _preparedRunnerPath = runnerPath;
            return runnerPath;
        }
    }

    private static string ReadRunnerSource()
    {
        var assembly = typeof(SsrRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(RunnerResourceName)
            ?? throw new InvalidOperationException("Jazor SSR runner resource was not embedded in Jazor.AspNetCore.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    private sealed class SsrWorkerPool(
        SsrRenderer.SsrArtifactGeneration generation,
        SsrArtifacts artifacts,
        string runnerPath,
        int workerCount,
        JsonSerializerOptions jsonOptions) : IAsyncDisposable
    {
        private readonly SsrArtifactGeneration _generation = generation;
        private readonly SsrArtifacts _artifacts = artifacts;
        private readonly string _runnerPath = runnerPath;
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
                        worker = new SsrWorker(_generation, _artifacts, _runnerPath, _jsonOptions);
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
        private const string ProtocolPrefix = "__JAZOR_SSR__:";

        private readonly SsrArtifactGeneration _generation;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DenoProcess _process;
        private readonly object _responseGate = new();
        private readonly object _errorGate = new();
        private readonly StringBuilder _standardError = new();
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource<string>? _pendingResponse;
        private long _pendingRequestId;
        private long _nextRequestId;
        private int _started;
        private int _disposed;
        private volatile bool _healthy = true;

        public SsrWorker(
            SsrArtifactGeneration generation,
            SsrArtifacts artifacts,
            string runnerPath,
            JsonSerializerOptions jsonOptions)
        {
            _generation = generation;
            _jsonOptions = jsonOptions;
            _process = new DenoProcess(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = artifacts.RootPath
                },
                [
                    "run",
                    "--no-config",
                    "--no-npm",
                    "--no-remote",
                    "--no-prompt",
                    "--allow-read=" + artifacts.RootPath,
                    "--import-map",
                    artifacts.SsrImportMapPath,
                    runnerPath
                ]);
            _process.OutputDataReceived += HandleOutput;
            _process.ErrorDataReceived += HandleError;
            _process.ProcessExited += HandleExit;
        }

        public bool IsHealthy => _healthy && Volatile.Read(ref _disposed) == 0 && _process.IsRunning;

        public async Task EnsureStartedAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
            {
                try
                {
                    // DenoHost publishes its managed process state after startup. Do not cancel
                    // inside that bookkeeping window: once it returns, cancellation cleanup can
                    // always stop the actual OS process through the supported DenoProcess API.
                    // cold-start 期间先完成进程托管，再在 ready wait 响应请求取消，避免 orphan process。
                    await _process.StartAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    _healthy = false;
                    throw;
                }
            }

            try
            {
                await _ready.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                _healthy = false;
                throw;
            }
        }

        public async Task<string> RenderAsync(
            SsrRenderPayload payload,
            CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            if (!IsHealthy)
                throw CreateProcessFailure("Jazor SSR worker is not running.");

            var requestId = Interlocked.Increment(ref _nextRequestId);
            var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_responseGate)
            {
                if (_pendingResponse is not null)
                    throw new InvalidOperationException("Jazor SSR worker received overlapping requests.");

                _pendingRequestId = requestId;
                _pendingResponse = completion;
            }

            var requestJson = JsonSerializer.Serialize(
                new SsrExecutionRequest(requestId, payload.ModulePath, payload.Props, payload.Providers),
                _jsonOptions);
            try
            {
                await _process.SendInputAsync(requestJson, cancellationToken).ConfigureAwait(false);
                return await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _healthy = false;
                await StopAsync(graceful: false).ConfigureAwait(false);
                throw;
            }
            catch
            {
                if (!completion.Task.IsCompleted || !_process.IsRunning)
                    _healthy = false;
                throw;
            }
            finally
            {
                lock (_responseGate)
                {
                    if (ReferenceEquals(_pendingResponse, completion))
                    {
                        _pendingResponse = null;
                        _pendingRequestId = 0;
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _healthy = false;
            CompletePending(CreateProcessFailure("Jazor SSR worker was disposed."));
            await StopAsync(graceful: true).ConfigureAwait(false);
            _process.OutputDataReceived -= HandleOutput;
            _process.ErrorDataReceived -= HandleError;
            _process.ProcessExited -= HandleExit;
            _process.Dispose();
        }

        private void HandleOutput(object? sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
        {
            var line = eventArgs.Data;
            if (line is null || !line.StartsWith(ProtocolPrefix, StringComparison.Ordinal))
                return;

            try
            {
                using var response = JsonDocument.Parse(line[ProtocolPrefix.Length..]);
                var root = response.RootElement;
                if (root.TryGetProperty("kind", out var kindElement) &&
                    string.Equals(kindElement.GetString(), "ready", StringComparison.Ordinal))
                {
                    _ready.TrySetResult();
                    return;
                }

                if (!root.TryGetProperty("id", out var idElement) || !idElement.TryGetInt64(out var requestId))
                    throw new InvalidOperationException("Jazor SSR worker response did not contain a request id.");

                TaskCompletionSource<string>? completion;
                lock (_responseGate)
                {
                    completion = requestId == _pendingRequestId ? _pendingResponse : null;
                }

                if (completion is null)
                    return;

                if (root.TryGetProperty("html", out var htmlElement) &&
                    htmlElement.ValueKind == JsonValueKind.String &&
                    htmlElement.GetString() is { } html)
                {
                    completion.TrySetResult(html);
                    return;
                }

                var error = root.TryGetProperty("error", out var errorElement) &&
                            errorElement.ValueKind == JsonValueKind.String
                    ? errorElement.GetString()
                    : null;
                completion.TrySetException(new InvalidOperationException(
                    "Jazor SSR render failed for artifact generation '" + _generation.Id + "'." +
                    (string.IsNullOrWhiteSpace(error) ? string.Empty : Environment.NewLine + error)));
            }
            catch (Exception exception)
            {
                _healthy = false;
                _ready.TrySetException(exception);
                CompletePending(exception);
            }
        }

        private void HandleError(object? sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data is null)
                return;

            lock (_errorGate)
                _standardError.AppendLine(eventArgs.Data);
        }

        private void HandleExit(object? sender, ProcessExitedEventArgs eventArgs)
        {
            _healthy = false;
            var exception = CreateProcessFailure(
                "Jazor SSR Deno worker exited with code " + eventArgs.ExitCode + ".");
            _ready.TrySetException(exception);
            CompletePending(exception);
        }

        private void CompletePending(Exception exception)
        {
            TaskCompletionSource<string>? completion;
            lock (_responseGate)
                completion = _pendingResponse;
            completion?.TrySetException(exception);
        }

        private InvalidOperationException CreateProcessFailure(string message)
        {
            string standardError;
            lock (_errorGate)
                standardError = _standardError.ToString();

            return new InvalidOperationException(
                message + (standardError.Length == 0 ? string.Empty : Environment.NewLine + standardError));
        }

        private async Task StopAsync(bool graceful)
        {
            try
            {
                if (_process.IsRunning)
                {
                    await _process.StopAsync(
                        graceful ? TimeSpan.FromSeconds(2) : TimeSpan.Zero,
                        CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (InvalidOperationException)
            {
                // Deno may exit between IsRunning and StopAsync while cancellation/disposal owns cleanup.
            }
        }
    }

    private sealed record SsrRenderPayload(
        string ModulePath,
        JsonElement Props,
        JsonElement Providers);

    // The runner protocol is a JavaScript-owned ABI. Keep field names explicit so the
    // host-wide CLR naming policy never becomes an accidental transport convention.
    private sealed record SsrExecutionRequest(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("modulePath")] string ModulePath,
        [property: JsonPropertyName("props")] JsonElement Props,
        [property: JsonPropertyName("providers")] JsonElement Providers);

    private sealed record SsrArtifactStamp(
        string RootPath,
        FileStamp ArtifactManifest,
        FileStamp SsrImportMap,
        FileStamp Runner)
    {
        public static SsrArtifactStamp Capture(SsrArtifacts artifacts, string runnerPath)
            => new(
                NormalizeRoot(artifacts.RootPath),
                FileStamp.Capture(artifacts.ArtifactManifestPath),
                FileStamp.Capture(artifacts.SsrImportMapPath),
                FileStamp.Capture(runnerPath));
    }

    private sealed record SsrArtifactGeneration(
        string RootPath,
        string ArtifactManifestHash,
        string SsrImportMapHash,
        string RunnerHash)
    {
        public string Id => ArtifactManifestHash[..12] + ":" + SsrImportMapHash[..12];

        public static SsrArtifactGeneration Create(SsrArtifacts artifacts, string runnerPath)
            => new(
                NormalizeRoot(artifacts.RootPath),
                ComputeFileHash(artifacts.ArtifactManifestPath),
                ComputeFileHash(artifacts.SsrImportMapPath),
                ComputeFileHash(runnerPath));
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

    private sealed class SsrGenerationRetiredException : InvalidOperationException;
}
