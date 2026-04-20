namespace Jolt.DevServer;

internal sealed class DependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _dependenciesByModule = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _dependentsByDependency = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();
    private readonly ModuleResolver? _moduleResolver;

    public DependencyGraph(ModuleResolver? moduleResolver = null)
    {
        _moduleResolver = moduleResolver;
    }

    public void Record(string modulePath, IReadOnlyList<string> dependencies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);
        ArgumentNullException.ThrowIfNull(dependencies);

        lock (_gate)
        {
            RemoveCore(modulePath);

            var normalizedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in dependencies)
            {
                if (TryNormalizeDependency(modulePath, dependency, out var normalizedDependency))
                {
                    normalizedDependencies.Add(normalizedDependency);
                }
            }

            _dependenciesByModule[modulePath] = normalizedDependencies;
            foreach (var dependency in normalizedDependencies)
            {
                if (!_dependentsByDependency.TryGetValue(dependency, out var dependents))
                {
                    dependents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _dependentsByDependency[dependency] = dependents;
                }

                dependents.Add(modulePath);
            }
        }
    }

    public IReadOnlyList<string> GetDependencies(string modulePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);

        lock (_gate)
        {
            return _dependenciesByModule.TryGetValue(modulePath, out var dependencies)
                ? dependencies.Order(StringComparer.OrdinalIgnoreCase).ToArray()
                : [];
        }
    }

    public IReadOnlyList<string> GetDependents(string modulePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);

        lock (_gate)
        {
            return _dependentsByDependency.TryGetValue(modulePath, out var dependents)
                ? dependents.Order(StringComparer.OrdinalIgnoreCase).ToArray()
                : [];
        }
    }

    public IReadOnlyList<string> GetAllAffectedModules(string changedModulePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(changedModulePath);

        lock (_gate)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();
            queue.Enqueue(changedModulePath);

            while (queue.Count > 0)
            {
                var modulePath = queue.Dequeue();
                if (!_dependentsByDependency.TryGetValue(modulePath, out var dependents))
                {
                    continue;
                }

                foreach (var dependent in dependents)
                {
                    if (!visited.Add(dependent))
                    {
                        continue;
                    }

                    queue.Enqueue(dependent);
                }
            }

            return visited.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }

    public void Remove(string modulePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);

        lock (_gate)
        {
            RemoveCore(modulePath);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _dependenciesByModule.Clear();
            _dependentsByDependency.Clear();
        }
    }

    private void RemoveCore(string modulePath)
    {
        if (!_dependenciesByModule.Remove(modulePath, out var oldDependencies))
        {
            return;
        }

        foreach (var dependency in oldDependencies)
        {
            if (!_dependentsByDependency.TryGetValue(dependency, out var dependents))
            {
                continue;
            }

            dependents.Remove(modulePath);
            if (dependents.Count == 0)
            {
                _dependentsByDependency.Remove(dependency);
            }
        }
    }

    private bool TryNormalizeDependency(
        string modulePath,
        string dependency,
        out string normalizedDependency)
    {
        normalizedDependency = string.Empty;
        if (string.IsNullOrWhiteSpace(dependency) || IsExternalSpecifier(dependency) || IsBareSpecifier(dependency))
        {
            return false;
        }

        if (_moduleResolver is null)
        {
            normalizedDependency = dependency;
            return true;
        }

        if (Path.IsPathFullyQualified(dependency) && File.Exists(dependency))
        {
            normalizedDependency = Path.GetFullPath(dependency);
            return true;
        }

        var resolved = _moduleResolver.Resolve(dependency, modulePath);
        if (!resolved.Found || resolved.IsVirtual)
        {
            return false;
        }

        normalizedDependency = resolved.AbsolutePath;
        return true;
    }

    private static bool IsExternalSpecifier(string dependency)
        => dependency.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || dependency.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || dependency.StartsWith("//", StringComparison.Ordinal)
            || dependency.StartsWith("data:", StringComparison.OrdinalIgnoreCase);

    private static bool IsBareSpecifier(string dependency)
        => !dependency.StartsWith(".", StringComparison.Ordinal)
            && !dependency.StartsWith("/", StringComparison.Ordinal)
            && !Path.IsPathFullyQualified(dependency);
}
