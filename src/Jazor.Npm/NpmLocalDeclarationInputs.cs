using System.Text.Json;

namespace Jazor.Npm;

internal static class NpmLocalDeclarationInputs
{
    internal static async Task WriteAsync(string projectDirectory, string output, CancellationToken cancellationToken)
    {
        var packages = new HashSet<string>(PathComparer);
        var inputs = new SortedSet<string>(StringComparer.Ordinal);
        // Workspace ownership can change with the C# buffer still open. Track the exact
        // candidate manifests and locks, including missing paths between project and workspace.
        var candidates = new List<string>();
        string? activeManifest = null;
        for (var directory = new DirectoryInfo(projectDirectory); directory is not null; directory = directory.Parent)
        {
            var manifest = Path.Combine(directory.FullName, "package.json");
            candidates.Add(manifest);
            candidates.Add(Path.Combine(directory.FullName, "pnpm-lock.yaml"));
            var hasManifest = File.Exists(manifest);
            var workspace = Path.Combine(directory.FullName, "pnpm-workspace.yaml");
            if (File.Exists(workspace))
                candidates.Add(workspace);
            else if (!hasManifest)
                continue;
            inputs.UnionWith(candidates);
            candidates.Clear();
            if (hasManifest)
                activeManifest ??= manifest;
        }
        if (activeManifest is not null)
            Visit(activeManifest, includeDeclarations: false);
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
            if (!packages.Add(directory) || !File.Exists(path))
                return;
            if (includeDeclarations)
            {
                inputs.Add(path);
                foreach (var file in FindDeclarations(directory, cancellationToken))
                    inputs.Add(file);
            }

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
