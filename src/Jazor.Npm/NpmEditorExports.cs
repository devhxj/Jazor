namespace Jazor.Npm;

public sealed partial class NpmValueMetadataProvider
{
    internal static async Task<int> WriteEditorExportsAsync(string request, string outputPath, TextWriter diagnostics,
        CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StringReader(request);
            var manifest = reader.ReadLine()!;
            var npmNamespace = reader.ReadLine()!;
            var store = reader.ReadLine();
            var root = Path.GetDirectoryName(manifest)!;
            var packages = ListPackageNames(root);
            var inputs = PackageInputs(root).ToHashSet(StringComparer.Ordinal);
            var provider = new NpmValueMetadataProvider(string.IsNullOrEmpty(store) ? null : store);
            var matches = await NpmExportSelection.FindModulesAsync(npmNamespace, packages, package =>
            {
                inputs.UnionWith(NpmPackageExports.ManifestCandidates(root, package));
                return provider.ListSpecifiersAsync(manifest, package, npmNamespace, cancellationToken);
            }, validatePackageRoot: true);

            string[] names = [];
            var linked = new HashSet<string>(StringComparer.Ordinal);
            if (matches.Count == 1)
            {
                var snapshot = await provider.ReadSnapshotAsync(manifest, matches[0], "editor", null, cancellationToken);
                inputs.UnionWith(snapshot.Inputs);
                if (snapshot.Realpaths is { } realpaths)
                    foreach (var (path, resolved) in realpaths)
                        if (IsInstalledPath(path) && !IsInstalledPath(resolved))
                        {
                            linked.Add(path);
                            linked.Add(resolved);
                        }
                foreach (var path in snapshot.Inputs)
                    if (!IsInstalledPath(path) && (path.EndsWith(".d.ts", StringComparison.Ordinal) ||
                        path.EndsWith(".d.mts", StringComparison.Ordinal) || path.EndsWith(".d.cts", StringComparison.Ordinal)))
                        linked.Add(path);
                inputs.UnionWith(linked);
                names = snapshot.Exports!.Distinct(StringComparer.Ordinal)
                    .GroupBy(NpmNames.ValueNameFor, StringComparer.Ordinal)
                    .Where(group => group.Key != "Exports" && IsIdentifier(group.Key) && group.Count() == 1)
                    .Select(group => group.Key)
                    .Concat(snapshot.Values!.Where(value => value.AttachedMemberSuffix is not null)
                        .Select(value => NpmNames.ValueNameFor(value.ExportName) + value.AttachedMemberSuffix))
                    .Order(StringComparer.Ordinal)
                    .ToArray();
            }

            await File.WriteAllLinesAsync(outputPath,
                [string.Join("\t", names), .. inputs.Order(StringComparer.Ordinal).Select(path => linked.Contains(path) ? "*" + path : path)], cancellationToken);
            return 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await diagnostics.WriteLineAsync(ex.Message);
            return 1;
        }
    }

    private static bool IsInstalledPath(string path) =>
        path.Replace('\\', '/').Contains("/node_modules/", StringComparison.OrdinalIgnoreCase);
}
