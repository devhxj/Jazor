using Jolt.Compiler;
using Jolt.Npm;

internal static class NpmBindingsCommand
{
    internal const string Usage = "jolt bindings [project] --module specifier --export name [--export name ...] [--module specifier --export name ...] --output file.cs";

    internal static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args is ["--help"] or ["-h"])
        {
            Console.WriteLine(Usage);
            return 0;
        }

        var index = 0;
        var project = Path.GetFullPath(args.Length > 0 && !args[0].StartsWith('-') ? args[index++] : ".");
        string? module = null;
        string? output = null;
        var exports = new List<string>();
        var requests = new List<(string Module, List<string> Exports)>();
        for (; index + 1 < args.Length; index += 2)
        {
            var value = args[index + 1];
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith('-'))
                throw new ArgumentException(Usage);
            switch (args[index])
            {
                case "--module":
                    if (module is not null)
                    {
                        if (exports.Count == 0)
                            throw new ArgumentException(Usage);
                        requests.Add((module, exports));
                        exports = [];
                    }
                    module = value;
                    break;
                case "--export": exports.Add(value); break;
                case "--output" when output is null: output = value; break;
                default: throw new ArgumentException(Usage);
            }
        }
        if (index != args.Length || module is null || output is null || exports.Count == 0)
            throw new ArgumentException(Usage);
        requests.Add((module, exports));
        if (!File.Exists(Path.Combine(project, "package.json")))
            throw new FileNotFoundException($"No package.json found in '{project}'.");

        var provider = new NpmVueMetadataProvider();
        var document = Path.Combine(project, "package.json");
        var values = new List<NpmValueExport>();
        foreach (var request in requests.GroupBy(request => request.Module, StringComparer.Ordinal))
        {
            var requested = request.SelectMany(group => group.Exports).Distinct(StringComparer.Ordinal).ToArray();
            var projections = await provider.ReadValueProjectionsAsync(document, request.Key, requested, cancellationToken);
            foreach (var name in requested)
                if (projections.Errors.TryGetValue(name, out var error))
                    throw new InvalidOperationException(error);
            values.AddRange(projections.Values);
        }

        var source = JoltNpmBindings.GenerateCachedSource(project, values, cancellationToken);
        output = Path.GetFullPath(output);
        var bytes = System.Text.Encoding.UTF8.GetBytes(source);
        var previous = File.Exists(output) ? await File.ReadAllBytesAsync(output, cancellationToken) : null;
        if (previous is not null && bytes.AsSpan().SequenceEqual(previous))
        {
            Console.WriteLine($"Bindings are up to date in {output}");
            return 0;
        }

        var directory = Path.GetDirectoryName(output)!;
        Directory.CreateDirectory(directory);
        // A sibling file keeps the final rename on the same filesystem.
        var temporary = Path.Combine(directory, $".jolt-bindings-{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, output, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
        Console.WriteLine($"Generated {values.Count} npm binding(s) in {output}");
        return 0;
    }
}
