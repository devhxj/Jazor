using System.Text.Json;

internal static class JoltClrRuntime
{
    public static IReadOnlyDictionary<string, string> ReadModules(IEnumerable<string> roots)
    {
        var paths = new Queue<string>(roots.Distinct(StringComparer.Ordinal));
        var modules = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (paths.Count == 0)
            return modules;

        var assembly = typeof(JoltClrRuntime).Assembly;
        // Use the ECMAScript package's declared edges, including the cycles shared by
        // CLR helpers. Keeping this manifest with the tool also supports installed jolt.
        using var manifestStream = assembly.GetManifestResourceStream("Jolt.Clr.manifest.json")!;
        using var manifest = JsonDocument.Parse(manifestStream);
        var imports = manifest.RootElement.GetProperty("imports");
        while (paths.TryDequeue(out var path))
        {
            if (modules.ContainsKey(path))
                continue;
            var entry = imports.GetProperty(path);
            var resourceName = "Jolt.Clr/" + entry.GetProperty("path").GetString()!["clr/".Length..];
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidDataException($"The CLR runtime module '{path}' was not packaged.");
            using var reader = new StreamReader(stream);
            modules.Add(path, reader.ReadToEnd());
            foreach (var dependency in entry.GetProperty("moduleDependencies").EnumerateArray())
                paths.Enqueue(dependency.GetString()!);
        }
        return modules.ToDictionary(pair => ".jolt-runtime/" + pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }
}
