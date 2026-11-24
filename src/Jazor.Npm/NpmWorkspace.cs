namespace Jazor.Npm;

internal static class NpmWorkspace
{
    internal static StringComparer PathComparer { get; } = Path.DirectorySeparatorChar == '\\'
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    internal static string? FindProjectManifest(string projectFile, Func<string, bool> exists) =>
        AncestorManifests(Path.GetDirectoryName(Path.GetFullPath(projectFile))!).FirstOrDefault(exists);

    internal static (string? Manifest, IReadOnlyList<(string Manifest, NpmSourceRequest Site)> Conflicts) Select(
        string projectFile, IReadOnlyList<NpmSourceRequest> requests, Func<string, bool> exists)
    {
        // Linked sources have always used the consumer's workspace when it has one.
        if (FindProjectManifest(projectFile, exists) is { } projectManifest)
            return (projectManifest, []);

        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFile))!;
        var candidates = new Dictionary<string, NpmSourceRequest>(PathComparer);
        foreach (var site in requests)
        {
            var path = site.Location.SourceTree?.FilePath;
            if (SourceManifestCandidates(projectDirectory, path).FirstOrDefault(exists) is { } manifest &&
                !candidates.ContainsKey(manifest))
                candidates.Add(manifest, site);
        }
        if (candidates.Count < 2)
            return (candidates.Keys.SingleOrDefault(), []);
        return (null, candidates.OrderBy(candidate => candidate.Key, PathComparer)
            .Select(candidate => (candidate.Key, candidate.Value)).ToArray());
    }

    internal static IEnumerable<string> PotentialManifests(string projectFile, IEnumerable<string> sourceFiles)
    {
        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFile))!;
        return AncestorManifests(projectDirectory)
            .Concat(sourceFiles.SelectMany(file => SourceManifestCandidates(projectDirectory, file)))
            .Distinct(PathComparer);
    }

    private static IEnumerable<string> SourceManifestCandidates(string projectDirectory, string? file)
    {
        if (string.IsNullOrEmpty(file) || !Path.IsPathRooted(file))
            yield break;
        var source = Path.GetDirectoryName(Path.GetFullPath(file!))!;
        var projectPrefix = projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!source.StartsWith(projectPrefix, comparison) || PathComparer.Equals(source, projectDirectory))
            yield break;
        for (var directory = new DirectoryInfo(source);
            directory is not null && !PathComparer.Equals(directory.FullName, projectDirectory); directory = directory.Parent)
            yield return Path.Combine(directory.FullName, "package.json");
    }

    private static IEnumerable<string> AncestorManifests(string directory)
    {
        for (var parent = new DirectoryInfo(directory); parent is not null; parent = parent.Parent)
            yield return Path.Combine(parent.FullName, "package.json");
    }
}
