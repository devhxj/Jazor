using System.Text.Json;

namespace Jazor.Npm;

internal static class NpmLocalDeclarationInputs
{
    internal static async Task WriteAsync(string projectFile, IReadOnlyList<string> sourceFiles, string output,
        CancellationToken cancellationToken)
    {
        var packages = new HashSet<string>(PathComparer);
        var declarations = new HashSet<string>(PathComparer);
        var inputs = new SortedSet<string>(StringComparer.Ordinal);
        var ancestors = NpmWorkspace.PotentialManifests(projectFile, []).ToHashSet(PathComparer);
        var projectDirectory = Path.GetDirectoryName(projectFile)!;
        // An edit can activate an existing source-local workspace before the first npm
        // import. Registering every possible missing file makes Roslyn load phantom
        // documents in each C# folder, even when they all share one npm workspace.
        var candidates = NpmWorkspace.PotentialManifests(projectFile, sourceFiles)
            .Where(path => !ancestors.Contains(path) &&
                (File.Exists(path) || File.Exists(Path.Combine(Path.GetDirectoryName(path)!, "pnpm-workspace.yaml"))))
            .ToList();
        var pendingAncestors = new List<string>();
        for (var parent = new DirectoryInfo(projectDirectory); parent is not null; parent = parent.Parent)
        {
            var manifest = Path.Combine(parent.FullName, "package.json");
            pendingAncestors.Add(manifest);
            if (!File.Exists(manifest) && !File.Exists(Path.Combine(parent.FullName, "pnpm-workspace.yaml")))
                continue;
            candidates.AddRange(pendingAncestors);
            pendingAncestors.Clear();
        }
        candidates.Add(Path.Combine(projectDirectory, "package.json"));
        foreach (var manifest in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = Path.GetDirectoryName(manifest)!;
            inputs.Add(manifest);
            inputs.Add(Path.Combine(directory, "pnpm-lock.yaml"));
            var workspace = Path.Combine(directory, "pnpm-workspace.yaml");
            if (File.Exists(workspace))
                inputs.Add(workspace);
            Visit(manifest, includeDeclarations: false);
        }
        var text = string.Join(Environment.NewLine, inputs);
        if (File.Exists(output) && await File.ReadAllTextAsync(output, cancellationToken) == text)
            return;
        var temporary = output + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, text, cancellationToken);
            File.Move(temporary, output, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }

        void Visit(string path, bool includeDeclarations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = Path.GetDirectoryName(path)!;
            if (!File.Exists(path))
                return;
            if (includeDeclarations && declarations.Add(directory))
            {
                inputs.Add(path);
                foreach (var file in FindDeclarations(directory, cancellationToken))
                    inputs.Add(file);
            }

            if (!packages.Add(directory))
                return;

            using var json = ReadDesignTimeManifest(path);
            if (json is null || json.RootElement.ValueKind != JsonValueKind.Object)
                return;
            foreach (var section in new[] { "dependencies", "devDependencies", "peerDependencies", "optionalDependencies" })
            {
                if (!json.RootElement.TryGetProperty(section, out var dependencies) || dependencies.ValueKind != JsonValueKind.Object)
                    continue;
                foreach (var dependency in dependencies.EnumerateObject())
                {
                    if (dependency.Value.ValueKind != JsonValueKind.String)
                        continue;
                    var specifier = dependency.Value.GetString()!;
                    if (specifier.StartsWith("workspace:", StringComparison.Ordinal))
                    {
                        // pnpm has already resolved workspace aliases and globs. Register
                        // the physical declaration paths so Roslyn watches edits at the source.
                        for (var owner = new DirectoryInfo(directory); owner is not null; owner = owner.Parent)
                        {
                            var installed = new DirectoryInfo(Path.Combine(owner.FullName, "node_modules", dependency.Name));
                            if (!installed.Exists)
                                continue;
                            if (installed.ResolveLinkTarget(returnFinalTarget: true) is DirectoryInfo target)
                                Visit(Path.Combine(target.FullName, "package.json"), includeDeclarations: true);
                            break;
                        }
                        continue;
                    }
                    // link: resolves to the source directory. pnpm can materialize a
                    // file: dependency as a separate installation, so watch that copy.
                    var linked = specifier.StartsWith("link:", StringComparison.Ordinal);
                    if (specifier.Length > 5 && (linked || specifier.StartsWith("file:", StringComparison.Ordinal)))
                    {
                        var localDirectory = linked
                            ? Path.GetFullPath(specifier[5..], directory)
                            : Path.GetFullPath(Path.Combine("node_modules", dependency.Name), directory);
                        Visit(Path.Combine(localDirectory, "package.json"), includeDeclarations: true);
                    }
                }
            }
        }
    }

    private static JsonDocument? ReadDesignTimeManifest(string path)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            // Keep the manifest in Roslyn's inputs so a later valid save can restore
            // completion in the open project, even if its first snapshot was incomplete.
            return null;
        }
    }

    private static IEnumerable<string> FindDeclarations(string directory, CancellationToken cancellationToken)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.d.*"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.EndsWith(".d.ts", StringComparison.Ordinal) || file.EndsWith(".d.mts", StringComparison.Ordinal) ||
                file.EndsWith(".d.cts", StringComparison.Ordinal))
                yield return file;
        }
        foreach (var child in new DirectoryInfo(directory).EnumerateDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (child.Name is "node_modules" or ".git" or ".jazor" ||
                (child.Attributes & FileAttributes.ReparsePoint) != 0)
                continue;
            foreach (var file in FindDeclarations(child.FullName, cancellationToken))
                yield return file;
        }
    }

    private static StringComparer PathComparer =>
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
