using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;

namespace Jazor.Npm.Editor;

internal static class NpmEditorIndex
{
    private static readonly ConditionalWeakTable<Workspace, ConcurrentDictionary<(ProjectId Project, string Namespace), Entry>> Entries = new();

    internal static async Task<IReadOnlyList<string>> ReadAsync(
        Document document, string npmNamespace, CancellationToken cancellationToken)
    {
        var options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!options.TryGetValue("build_property._JazorNpmEditorTool", out var host) || string.IsNullOrEmpty(host) ||
            !options.TryGetValue("build_property.MSBuildProjectFullPath", out var project) || string.IsNullOrEmpty(project))
            return Array.Empty<string>();
        options.TryGetValue("build_property.JazorPnpmStoreDirectory", out var store);
        options.TryGetValue("build_property.NetCoreRoot", out var netCoreRoot);
        if (!string.IsNullOrEmpty(store))
            store = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project)!, store));

        var workspace = Entries.GetValue(document.Project.Solution.Workspace, _ => new());
        var entry = workspace.GetOrAdd((document.Project.Id, npmNamespace), _ => new Entry());
        var pathComparer = Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var additional = document.Project.AdditionalDocuments.Where(input => input.FilePath is not null)
            .ToLookup(input => Path.GetFullPath(input.FilePath!), pathComparer);
        await entry.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (entry.Snapshot is { } cached && cached.Host == host && cached.Project == project && cached.Store == store &&
                InputsMatch(cached.Inputs))
                return cached.Names;

            var response = await NpmHostProcess.RunAsync(host, netCoreRoot, project, "--sdk-npm-completion",
                string.Join("\n", project, npmNamespace, store), cancellationToken).ConfigureAwait(false);
            if (response is null)
                return Array.Empty<string>();

            using var reader = new StringReader(response);
            var names = (reader.ReadLine() ?? "").Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var inputs = new List<Input> { ReadInput(host, false) };
            while (reader.ReadLine() is { } path)
            {
                var linked = path.StartsWith("*", StringComparison.Ordinal);
                inputs.Add(ReadInput(linked ? path.Substring(1) : path, linked));
            }
            entry.Snapshot = new Snapshot(host, project, store, names, inputs);
            return names;
        }
        finally
        {
            entry.Gate.Release();
        }

        Input ReadInput(string path, bool linked)
        {
            var workspaceInput = additional[Path.GetFullPath(path)].FirstOrDefault();
            // Workspace snapshots can change independently of disk metadata. Check contents
            // only for linked declarations and AdditionalDocuments already loaded by Roslyn.
            var checksum = workspaceInput is not null && workspaceInput.TryGetText(out var source)
                ? Convert.ToBase64String(source.GetChecksum().ToArray()) : null;
            var info = new FileInfo(path);
            string? content = null;
            if (linked && info.Exists)
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var hash = SHA256.Create();
                content = Convert.ToBase64String(hash.ComputeHash(stream));
            }
            return new Input(path, linked, info.LastWriteTimeUtc, info.Exists ? info.Length : -1, checksum, content);
        }

        bool InputsMatch(IReadOnlyList<Input> inputs)
        {
            foreach (var input in inputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (ReadInput(input.Path, input.Linked) != input)
                    return false;
            }
            return true;
        }
    }

    private sealed class Entry
    {
        internal SemaphoreSlim Gate { get; } = new(1, 1);
        internal Snapshot? Snapshot { get; set; }
    }

    private sealed record Input(string Path, bool Linked, DateTime LastWrite, long Length, string? WorkspaceChecksum, string? Content);
    private sealed record Snapshot(string Host, string Project, string? Store, string[] Names, IReadOnlyList<Input> Inputs);
}
