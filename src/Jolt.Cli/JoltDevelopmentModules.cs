using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Concurrent;

internal sealed class JoltDevelopmentModules(string projectDirectory, string generatedRoot, string configuration) : IDisposable
{
    private readonly ConcurrentDictionary<string, bool> _changes = new(PathComparer);
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new(PathComparer);
    private Dictionary<string, string?>? _previous;
    private string? _project;
    private bool _pendingBuild;
    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    internal Action? Changed { get; set; }

    internal async Task<bool> UpdateAsync(CancellationToken cancellationToken)
    {
        _project ??= Directory.EnumerateFiles(projectDirectory, "*.csproj").Single();
        var changes = new List<KeyValuePair<string, bool>>();
        foreach (var path in _changes.Keys)
            if (_changes.TryRemove(path, out var membership))
                changes.Add(new(path, membership));
        if (_previous is not null && changes.Count == 0)
            return !_pendingBuild;

        var refreshGraph = _previous is null || changes.Any(change => change.Value || Path.GetExtension(change.Key) == ".csproj");
        Dictionary<string, string?> snapshot;
        if (refreshGraph)
        {
            snapshot = await ReadGraphAsync(cancellationToken);
            WatchDirectories(snapshot.Keys);
        }
        else
        {
            snapshot = new(_previous!, PathComparer);
            foreach (var (path, _) in changes)
                if (snapshot.ContainsKey(path))
                    snapshot[path] = Hash(path);
        }
        if (_previous is null)
        {
            // The host's initial build has already emitted the ordinary C# modules.
            _previous = snapshot;
            return true;
        }
        if (!_pendingBuild && _previous.Count == snapshot.Count && _previous.All(entry => snapshot.TryGetValue(entry.Key, out var hash) && hash == entry.Value))
            return true;

        // Keep rebuild outputs away from the running host and exclude them from the source watcher.
        var (exitCode, _, _) = await RunAsync(["build", _project, "-c", configuration, "--nologo", "--artifacts-path",
            Path.Combine(generatedRoot, "development-build"), "-p:_JoltRefreshModules=true"], capture: false, cancellationToken);
        _pendingBuild = exitCode != 0;
        _previous = snapshot;
        return !_pendingBuild;
    }

    private async Task<Dictionary<string, string?>> ReadGraphAsync(CancellationToken cancellationToken)
    {
        // dotnet watch enumerates evaluated Compile items across the MSBuild ProjectReference graph.
        var (exitCode, output, error) = await RunAsync(["watch", "--project", _project!, "--configuration", configuration, "--list"],
            capture: true, cancellationToken);
        if (exitCode != 0)
            throw new InvalidOperationException($"dotnet watch --list exited with code {exitCode}.{Environment.NewLine}{error}{output}".TrimEnd());
        if (error.Length != 0)
            Console.Error.Write(error);
        return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(path => Path.IsPathFullyQualified(path) && Path.GetExtension(path) is ".cs" or ".csproj")
            .Distinct(PathComparer)
            .ToDictionary(path => path, Hash, PathComparer);
    }

    private static string? Hash(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private async Task<(int ExitCode, string Output, string Error)> RunAsync(string[] arguments, bool capture, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            UseShellExecute = false,
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = capture,
            RedirectStandardError = capture
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        using var child = Process.Start(start) ?? throw new InvalidOperationException("Could not refresh the Jazor C# modules.");
        try
        {
            var output = capture ? child.StandardOutput.ReadToEndAsync(cancellationToken) : Task.FromResult("");
            var error = capture ? child.StandardError.ReadToEndAsync(cancellationToken) : Task.FromResult("");
            await child.WaitForExitAsync(cancellationToken);
            return (child.ExitCode, await output, await error);
        }
        finally
        {
            if (!child.HasExited)
            {
                child.Kill(entireProcessTree: true);
                await child.WaitForExitAsync();
            }
        }
    }

    private void WatchDirectories(IEnumerable<string> paths)
    {
        foreach (var directory in paths.Select(Path.GetDirectoryName).OfType<string>().Distinct(PathComparer)
            .OrderBy(directory => directory.Length))
        {
            if (!Directory.Exists(directory) || _watchers.Keys.Any(root => IsWithin(root, directory)))
                continue;
            var watcher = new FileSystemWatcher(directory)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
            };
            watcher.Changed += (_, args) => Record(args.FullPath, membership: false);
            watcher.Created += (_, args) => Record(args.FullPath, membership: true);
            watcher.Deleted += (_, args) => Record(args.FullPath, membership: true);
            watcher.Renamed += (_, args) =>
            {
                Record(args.OldFullPath, membership: true);
                Record(args.FullPath, membership: true);
            };
            watcher.Error += (_, _) =>
            {
                _changes[directory] = true;
                Changed?.Invoke();
            };
            _watchers.Add(directory, watcher);
            watcher.EnableRaisingEvents = true;
        }
    }

    private void Record(string path, bool membership)
    {
        if (IsWithin(generatedRoot, path) ||
            path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "bin" or "obj" or "node_modules" or ".git"))
            return;
        var extension = Path.GetExtension(path);
        if (extension is not (".cs" or ".csproj") && !(membership && (extension.Length == 0 || Directory.Exists(path))))
            return;

        // Create, delete, and rename may change SDK glob membership. A normal
        // save rehashes just the changed inputs from the evaluated watch list.
        _changes.AddOrUpdate(path, membership, (_, previous) => previous || membership);
        Changed?.Invoke();
    }

    private static bool IsWithin(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return !Path.IsPathRooted(relative) && relative != ".." &&
            !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers.Values)
            watcher.Dispose();
    }
}
