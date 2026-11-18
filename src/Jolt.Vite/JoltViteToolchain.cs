using System.Text.Json;
using System.Threading.Channels;
using Jazor.NodeHost;

namespace Jolt.Vite;

public static class JoltViteToolchain
{
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

    public static async Task<JoltViteServer> ServeAsync(string projectDirectory, Func<CancellationToken, Task<bool>> compile,
        string? storeDirectory = null, string host = "127.0.0.1", int port = 5173,
        CancellationToken cancellationToken = default, Action<Exception>? reportError = null)
    {
        projectDirectory = Path.GetFullPath(projectDirectory);
        await RestoreAsync(projectDirectory, storeDirectory, cancellationToken);

        var changes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
        });
        var watcher = new FileSystemWatcher(projectDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
        };
        void OnChange(object sender, FileSystemEventArgs args)
        {
            if (IsSource(args.FullPath) || args is RenamedEventArgs renamed && IsSource(renamed.OldFullPath))
                changes.Writer.TryWrite(true);
        }
        watcher.Changed += OnChange;
        watcher.Created += OnChange;
        watcher.Deleted += OnChange;
        watcher.Renamed += OnChange;
        watcher.Error += (_, args) =>
        {
            reportError?.Invoke(args.GetException());
            changes.Writer.TryWrite(true);
        };

        NodeViteServer? vite = null;
        try
        {
            watcher.EnableRaisingEvents = true;
            if (!await compile(cancellationToken))
                throw new InvalidOperationException("The initial Jolt compilation failed.");

            vite = new NodeViteServer(projectDirectory);
            var result = await vite.StartAsync(projectDirectory, host, port, "/", cancellationToken, jolt: true);
            using var response = JsonDocument.Parse(result);
            var origin = new Uri(response.RootElement.GetProperty("origin").GetString()!);
            return new JoltViteServer(origin, vite, watcher, changes.Reader, compile, reportError);
        }
        catch
        {
            watcher.Dispose();
            if (vite is not null)
                await vite.DisposeAsync();
            throw;
        }

        bool IsSource(string path)
        {
            if (!path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
                return false;
            var relative = Path.GetRelativePath(projectDirectory, path);
            return !relative.Split(Path.DirectorySeparatorChar).Any(part =>
                part is "node_modules" or ".jolt" or "dist" or ".git" or ".jazor");
        }
    }

    private static Task<string> RestoreAsync(string projectDirectory, string? storeDirectory,
        CancellationToken cancellationToken)
        => NodeToolchain.RestoreAsync(projectDirectory,
            storeDirectory ?? Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
                ?? Path.Combine(projectDirectory, ".jazor", "pnpm-store"), cancellationToken);
}

public sealed class JoltViteServer : IAsyncDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly NodeViteServer _vite;
    private readonly FileSystemWatcher _watcher;
    private readonly Task _watch;

    internal JoltViteServer(Uri origin, NodeViteServer vite, FileSystemWatcher watcher, ChannelReader<bool> changes,
        Func<CancellationToken, Task<bool>> compile, Action<Exception>? reportError)
    {
        Origin = origin;
        _vite = vite;
        _watcher = watcher;
        _watch = RunAsync(changes, compile, reportError, _stop.Token);
    }

    public Uri Origin { get; }

    public async ValueTask DisposeAsync()
    {
        _watcher.Dispose();
        await _stop.CancelAsync();
        try
        {
            await _watch;
        }
        finally
        {
            _stop.Dispose();
            await _vite.DisposeAsync();
        }
    }

    private static async Task RunAsync(ChannelReader<bool> changes, Func<CancellationToken, Task<bool>> compile,
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
                    await compile(stop);
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
