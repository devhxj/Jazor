using System.Text.Json;
using System.Threading.Channels;
using Jazor.NodeHost;

namespace Jolt.Vite;

public static class JoltViteToolchain
{
    public const int DefaultPort = 15658;

    public static Task BuildLibraryAsync(string projectDirectory, string generatedDirectory,
        string outputDirectory, IReadOnlyList<string> modulePaths, CancellationToken cancellationToken)
        => NodeToolchain.BuildJoltLibraryAsync(projectDirectory, generatedDirectory, outputDirectory, modulePaths, cancellationToken);

    public static async Task<bool> BuildAsync(string projectDirectory, Func<CancellationToken, Task<bool>> compile,
        string? storeDirectory = null, CancellationToken cancellationToken = default)
    {
        projectDirectory = Path.GetFullPath(projectDirectory);
        await RestoreAsync(projectDirectory, storeDirectory, cancellationToken);
        if (!await compile(cancellationToken))
            return false;

        await NodeToolchain.BuildAsync(projectDirectory, ssr: false, cancellationToken, jolt: true);
        return true;
    }

    public static async Task<bool> BuildSsrAsync(string projectDirectory, string serverEntry,
        Func<CancellationToken, Task<bool>> compile, string? storeDirectory = null,
        CancellationToken cancellationToken = default)
    {
        projectDirectory = Path.GetFullPath(projectDirectory);
        var entry = Path.GetFullPath(serverEntry, projectDirectory);
        var relativeEntry = Path.GetRelativePath(projectDirectory, entry);
        if (Path.IsPathRooted(relativeEntry) || relativeEntry == ".." ||
            relativeEntry.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !File.Exists(entry))
            throw new ArgumentException($"SSR entry '{serverEntry}' must name a file in the Jolt project.", nameof(serverEntry));
        if (!File.Exists(Path.Combine(projectDirectory, "index.html")))
            throw new InvalidOperationException("The Jolt SSR client build requires an index.html at the project root.");

        await RestoreAsync(projectDirectory, storeDirectory, cancellationToken);
        if (!await compile(cancellationToken))
            return false;

        var buildId = "ssr-" + Guid.NewGuid().ToString("N");
        // Keep the staged output at the same depth as dist for relative source map paths.
        var staging = Path.Combine(projectDirectory, ".jolt-" + buildId);
        var previousOutput = Path.Combine(projectDirectory, ".jazor", buildId);
        var destination = Path.Combine(projectDirectory, "dist");
        try
        {
            // Publish the client and server as one generation after both builds succeed.
            await NodeToolchain.BuildJoltSsrAsync(projectDirectory, entry, staging, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(destination))
                Directory.Move(destination, previousOutput);
            try
            {
                Directory.Move(staging, destination);
            }
            catch
            {
                if (Directory.Exists(previousOutput))
                    Directory.Move(previousOutput, destination);
                throw;
            }
            if (Directory.Exists(previousOutput))
                Directory.Delete(previousOutput, recursive: true);
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
        }
        return true;
    }

    public static async Task<JoltViteServer> ServeAsync(string projectDirectory, Func<CancellationToken, Task<bool>> compile,
        string? storeDirectory = null, string host = "127.0.0.1", int port = DefaultPort,
        CancellationToken cancellationToken = default, Action<Exception>? reportError = null)
    {
        projectDirectory = Path.GetFullPath(projectDirectory);
        await RestoreAsync(projectDirectory, storeDirectory, cancellationToken);
        var manifest = Path.Combine(projectDirectory, "package.json");
        var lockfile = Path.Combine(projectDirectory, "pnpm-lock.yaml");
        var dependencies = await ReadDependenciesAsync(cancellationToken);
        var sourceChanged = 0;

        var changes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
        });
        var watcher = new FileSystemWatcher(projectDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            Filters = { "*.jazor", "*.vue", "*.ts", "*.tsx", "*.mts", "*.cts" },
        };
        var dependencyWatcher = new FileSystemWatcher(projectDirectory)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            Filters = { "package.json", "pnpm-lock.yaml" },
        };
        void OnChange(object sender, FileSystemEventArgs args)
        {
            if (IsSource(args.FullPath) || args is RenamedEventArgs renamed && IsSource(renamed.OldFullPath))
            {
                Interlocked.Exchange(ref sourceChanged, 1);
                changes.Writer.TryWrite(true);
            }
        }
        watcher.Changed += OnChange;
        watcher.Created += OnChange;
        watcher.Deleted += OnChange;
        watcher.Renamed += OnChange;
        dependencyWatcher.Changed += OnDependencyChange;
        dependencyWatcher.Created += OnDependencyChange;
        dependencyWatcher.Deleted += OnDependencyChange;
        dependencyWatcher.Renamed += OnDependencyChange;
        watcher.Error += OnError;
        dependencyWatcher.Error += OnError;

        void OnDependencyChange(object sender, FileSystemEventArgs args) => changes.Writer.TryWrite(true);

        void OnError(object sender, ErrorEventArgs args)
        {
            reportError?.Invoke(args.GetException());
            Interlocked.Exchange(ref sourceChanged, 1);
            changes.Writer.TryWrite(true);
        }

        NodeViteServer? vite = null;
        try
        {
            watcher.EnableRaisingEvents = true;
            dependencyWatcher.EnableRaisingEvents = true;
            if (!await compile(cancellationToken))
                throw new InvalidOperationException("The initial Jolt compilation failed.");

            vite = new NodeViteServer(projectDirectory);
            var result = await vite.StartAsync(projectDirectory, host, port, "/", cancellationToken, jolt: true);
            using var response = JsonDocument.Parse(result);
            var origin = new Uri(response.RootElement.GetProperty("origin").GetString()!);
            return new JoltViteServer(origin, vite, [watcher, dependencyWatcher], changes.Reader, RebuildAsync, reportError);
        }
        catch
        {
            watcher.Dispose();
            dependencyWatcher.Dispose();
            if (vite is not null)
                await vite.DisposeAsync();
            throw;
        }

        async Task RebuildAsync(CancellationToken stop)
        {
            var recompile = Interlocked.Exchange(ref sourceChanged, 0) != 0;
            var current = await ReadDependenciesAsync(stop);
            if (current != dependencies)
            {
                // Windows queues recursive events before applying the source filter.
                // Pause that watcher during pnpm's bulk writes. The compile below reads all sources.
                watcher.EnableRaisingEvents = false;
                try
                {
                    await RestoreAsync(projectDirectory, storeDirectory, stop);
                    // pnpm can rewrite the lockfile. Its watcher events must observe the restored inputs.
                    dependencies = await ReadDependenciesAsync(stop);
                }
                catch (Exception error) when (!stop.IsCancellationRequested && reportError is not null)
                {
                    reportError(error);
                }
                finally
                {
                    watcher.EnableRaisingEvents = true;
                }
                recompile = true;
            }
            if (recompile)
                await compile(stop);
        }

        async Task<(string? Manifest, string? Lockfile)> ReadDependenciesAsync(CancellationToken stop)
            => (await ReadInputAsync(manifest, stop), await ReadInputAsync(lockfile, stop));

        bool IsSource(string path)
        {
            if (Path.GetExtension(path).ToLowerInvariant() is not (".jazor" or ".vue" or ".ts" or ".tsx" or ".mts" or ".cts"))
                return false;
            var relative = Path.GetRelativePath(projectDirectory, path);
            return !relative.Split(Path.DirectorySeparatorChar).Any(part =>
                part is "node_modules" or ".jolt" or "dist" or ".git" or ".jazor");
        }
    }

    private static async Task<string?> ReadInputAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await File.ReadAllTextAsync(path, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    public static Task<string> RestoreAsync(string projectDirectory, string? storeDirectory = null,
        CancellationToken cancellationToken = default)
        => NodeToolchain.RestoreAsync(projectDirectory,
            storeDirectory ?? Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
                ?? Path.Combine(projectDirectory, ".jazor", "pnpm-store"), cancellationToken);

    public static async Task<Dictionary<string, JoltCompiledStyle>> CompileScopedStylesAsync(string projectDirectory,
        IReadOnlyDictionary<string, string> styles, CancellationToken cancellationToken)
        => JsonSerializer.Deserialize<Dictionary<string, JoltCompiledStyle>>(
            await NodeToolchain.CompileJoltStylesAsync(projectDirectory, styles, cancellationToken),
            JsonSerializerOptions.Web)!;
}

public sealed record JoltCompiledStyle(string Code, IReadOnlyList<string> Assets);

public sealed class JoltViteServer : IAsyncDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly NodeViteServer _vite;
    private readonly FileSystemWatcher[] _watchers;
    private readonly Task _watch;

    internal JoltViteServer(Uri origin, NodeViteServer vite, FileSystemWatcher[] watchers, ChannelReader<bool> changes,
        Func<CancellationToken, Task> rebuild, Action<Exception>? reportError)
    {
        Origin = origin;
        _vite = vite;
        _watchers = watchers;
        _watch = RunAsync(changes, rebuild, reportError, _stop.Token);
    }

    public Uri Origin { get; }

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        try
        {
            await _watch;
        }
        finally
        {
            foreach (var watcher in _watchers)
                watcher.Dispose();
            _stop.Dispose();
            await _vite.DisposeAsync();
        }
    }

    private static async Task RunAsync(ChannelReader<bool> changes, Func<CancellationToken, Task> rebuild,
        Action<Exception>? reportError, CancellationToken stop)
    {
        try
        {
            while (await changes.WaitToReadAsync(stop))
            {
                // Editors often save via several writes or a rename. Compile each batch serially.
                await Task.Delay(75, stop);
                changes.TryRead(out _);
                try
                {
                    await rebuild(stop);
                }
                catch (Exception ex) when (!stop.IsCancellationRequested && reportError is not null)
                {
                    reportError(ex);
                }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested)
        {
        }
    }
}
