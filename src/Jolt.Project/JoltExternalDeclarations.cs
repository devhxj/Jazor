using System.Security.Cryptography;

namespace Jolt.Project;

internal sealed class JoltExternalDeclarations(string project) : IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new(PathComparer);
    private Dictionary<string, string?> _active = new(PathComparer);
    private readonly Dictionary<string, string?> _next = new(PathComparer);
    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    internal Action? Changed { get; set; }

    internal void Begin() => _next.Clear();

    internal void Observe(IReadOnlyDictionary<string, string?> inputs)
    {
        foreach (var (path, hash) in inputs)
        {
            if (!path.EndsWith(".d.ts", StringComparison.Ordinal) &&
                !path.EndsWith(".d.mts", StringComparison.Ordinal) &&
                !path.EndsWith(".d.cts", StringComparison.Ordinal) &&
                Path.GetFileName(path) is not ("pnpm-lock.yaml" or "pnpm-workspace.yaml"))
                continue;
            var relative = Path.GetRelativePath(project, path);
            if (Path.IsPathRooted(relative) || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                _next[path] = hash;
        }
    }

    internal void Complete(bool prune)
    {
        bool changed;
        lock (_gate)
        {
            if (!prune)
                foreach (var (path, hash) in _active)
                    _next.TryAdd(path, hash);

            _active = new(_next, PathComparer);
            var directories = _active.Keys.Select(Path.GetDirectoryName).OfType<string>()
                .Where(Directory.Exists).ToHashSet(PathComparer);
            foreach (var directory in _watchers.Keys.Where(directory => !directories.Contains(directory)).ToArray())
            {
                _watchers[directory].Dispose();
                _watchers.Remove(directory);
            }
            foreach (var directory in directories.Where(directory => !_watchers.ContainsKey(directory)))
            {
                var watcher = new FileSystemWatcher(directory) { NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite };
                watcher.Changed += OnChange;
                watcher.Created += OnChange;
                watcher.Deleted += OnChange;
                watcher.Renamed += OnChange;
                watcher.Error += (_, _) => CheckAll();
                _watchers.Add(directory, watcher);
                watcher.EnableRaisingEvents = true;
            }

            // Cover changes between the checker snapshot and watcher registration.
            changed = CheckAllInputs();
        }
        if (changed)
            Changed?.Invoke();
    }

    private void OnChange(object sender, FileSystemEventArgs args)
    {
        bool changed;
        lock (_gate)
        {
            changed = Check(args.FullPath);
            if (args is RenamedEventArgs renamed)
                changed |= Check(renamed.OldFullPath);
        }
        if (changed)
            Changed?.Invoke();
    }

    private void CheckAll()
    {
        bool changed;
        lock (_gate)
            changed = CheckAllInputs();
        if (changed)
            Changed?.Invoke();
    }

    private bool CheckAllInputs()
    {
        var changed = false;
        foreach (var path in _active.Keys.ToArray())
            changed |= Check(path);
        return changed;
    }

    private bool Check(string path)
    {
        if (!_active.TryGetValue(path, out var before))
            return false;
        string? hash;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            hash = Convert.ToHexString(SHA256.HashData(stream));
        }
        catch (FileNotFoundException) { hash = null; }
        catch (DirectoryNotFoundException) { hash = null; }
        catch (IOException) { return false; }
        if (hash == before)
            return false;
        _active[path] = hash;
        return true;
    }

    public void Dispose()
        => Reset();

    internal void Reset()
    {
        lock (_gate)
        {
            _active.Clear();
            foreach (var watcher in _watchers.Values)
                watcher.Dispose();
            _watchers.Clear();
        }
    }
}
