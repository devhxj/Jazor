using System.Collections.Concurrent;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Workspace;

internal static class VueHostWorkspaceResolver
{
    private static readonly ConcurrentDictionary<string, string[]> WorkspaceFileCache = new(StringComparer.OrdinalIgnoreCase);

    public static void InvalidatePath(string documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            WorkspaceFileCache.Clear();
            return;
        }

        var normalizedPath = NormalizePath(documentPath);
        var normalizedDirectory = NormalizePath(Path.GetDirectoryName(normalizedPath) ?? normalizedPath);

        foreach (var cacheKey in WorkspaceFileCache.Keys)
        {
            if (!TryParseCacheKey(cacheKey, out var normalizedRoot))
            {
                continue;
            }

            if (PathMatchesOrContains(normalizedRoot, normalizedPath)
                || PathMatchesOrContains(normalizedRoot, normalizedDirectory)
                || PathMatchesOrContains(normalizedPath, normalizedRoot)
                || PathMatchesOrContains(normalizedDirectory, normalizedRoot))
            {
                WorkspaceFileCache.TryRemove(cacheKey, out _);
            }
        }
    }

    public static string NormalizePath(string documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            return string.Empty;
        }

        var comparablePath = Path.IsPathRooted(documentPath)
            ? Path.GetFullPath(documentPath)
            : documentPath;
        var slashNormalized = comparablePath.Replace('\\', '/');
        var prefix = string.Empty;
        var workingPath = slashNormalized;

        if (workingPath.Length >= 2 && workingPath[1] == ':')
        {
            prefix = workingPath[..2];
            workingPath = workingPath[2..];
        }
        else if (workingPath.StartsWith("/", StringComparison.Ordinal))
        {
            prefix = "/";
            workingPath = workingPath.TrimStart('/');
        }

        var segments = new Stack<string>();
        foreach (var segment in workingPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count > 0 && !string.Equals(segments.Peek(), "..", StringComparison.Ordinal))
                {
                    segments.Pop();
                }
                else if (prefix.Length == 0)
                {
                    segments.Push(segment);
                }

                continue;
            }

            segments.Push(segment);
        }

        var normalized = string.Join("/", segments.Reverse());
        if (prefix.Length == 0)
        {
            return normalized;
        }

        if (normalized.Length == 0)
        {
            return prefix;
        }

        return prefix == "/"
            ? prefix + normalized
            : prefix + "/" + normalized;
    }

    public static DocumentKind MapDocumentKind(string documentPath)
        => Path.GetExtension(documentPath).ToLowerInvariant() switch
        {
            ".jazor" => DocumentKind.Jazor,
            ".cs" => DocumentKind.CSharp,
            ".vue" => DocumentKind.Vue,
            ".js" => DocumentKind.JavaScript,
            ".ts" => DocumentKind.TypeScript,
            _ => DocumentKind.Unknown
        };

    public static DocumentKind? GetFrontendDocumentKind(string documentPath)
        => MapDocumentKind(documentPath) switch
        {
            DocumentKind.Vue => DocumentKind.Vue,
            DocumentKind.JavaScript => DocumentKind.JavaScript,
            DocumentKind.TypeScript => DocumentKind.TypeScript,
            _ => null
        };

    public static IEnumerable<string> ExpandPathCandidates(string documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            yield break;
        }

        yield return documentPath;

        if (string.IsNullOrWhiteSpace(Path.GetExtension(documentPath)))
        {
            yield return documentPath + ".vue";
            yield return documentPath + ".ts";
            yield return documentPath + ".js";
        }

        var slashNormalized = documentPath.Replace('\\', '/');
        if (!string.Equals(documentPath, slashNormalized, StringComparison.Ordinal))
        {
            yield return slashNormalized;
        }

        if (Path.IsPathRooted(documentPath))
        {
            var fullPath = Path.GetFullPath(documentPath);
            if (!string.Equals(documentPath, fullPath, StringComparison.OrdinalIgnoreCase))
            {
                yield return fullPath;
            }

            var fullSlashNormalized = fullPath.Replace('\\', '/');
            if (!string.Equals(fullPath, fullSlashNormalized, StringComparison.Ordinal))
            {
                yield return fullSlashNormalized;
            }
        }
    }

    public static IEnumerable<string> GetCoLocatedAssetPaths(string documentPath)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            yield break;
        }

        foreach (var extension in new[] { ".css", ".js", ".ts" })
        {
            yield return Path.Combine(documentDirectory, fileNameWithoutExtension + extension);
        }
    }

    public static IEnumerable<string> GetImportPathCandidates(
        string jazorDocumentPath,
        string importSource)
    {
        if (!IsFrontendImport(importSource))
        {
            yield break;
        }

        if (Path.IsPathRooted(importSource))
        {
            foreach (var candidate in ExpandPathCandidates(importSource))
            {
                yield return candidate;
            }

            yield break;
        }

        var jazorDirectory = Path.GetDirectoryName(jazorDocumentPath);
        if (!string.IsNullOrWhiteSpace(jazorDirectory))
        {
            foreach (var candidate in ExpandPathCandidates(Path.Combine(jazorDirectory, importSource)))
            {
                yield return candidate;
            }
        }

        foreach (var candidate in ExpandPathCandidates(importSource))
        {
            yield return candidate;
        }
    }

    public static IEnumerable<string> GetNearbyVueComponentPathCandidates(
        string jazorDocumentPath,
        string componentName)
    {
        var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(componentName))
        {
            yield break;
        }

        foreach (var directory in GetNearbyVueSearchDirectories(documentDirectory))
        {
            yield return Path.Combine(directory, componentName + ".vue");
        }
    }

    public static IEnumerable<string> GetNearbyVueSearchDirectories(string documentDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parentDirectory = GetParentDirectoryPath(documentDirectory);
        foreach (var directory in new[]
                 {
                     documentDirectory,
                     Path.Combine(documentDirectory, "Components"),
                     Path.Combine(documentDirectory, "components"),
                     parentDirectory,
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "Components"),
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "components")
                 })
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            if (seen.Add(NormalizePath(directory)))
            {
                yield return Path.IsPathRooted(directory)
                    ? Path.GetFullPath(directory)
                    : directory;
            }
        }
    }

    public static bool TryResolveNearbyVueComponent(
        string documentPath,
        string componentName,
        out string componentPath,
        out string importPath)
    {
        componentPath = string.Empty;
        importPath = string.Empty;

        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return false;
        }

        foreach (var directory in GetNearbyVueSearchDirectories(documentDirectory))
        {
            var candidate = Path.Combine(directory, componentName + ".vue");
            if (!File.Exists(candidate))
            {
                continue;
            }

            componentPath = NormalizePath(candidate);
            importPath = ToImportPath(documentDirectory, candidate);
            return true;
        }

        return false;
    }

    public static bool TryResolveTrackedNearbyVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        out WorkspaceVueComponentResolution resolvedComponent)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (!string.IsNullOrWhiteSpace(documentDirectory))
        {
            foreach (var candidate in GetNearbyVueSearchDirectories(documentDirectory))
            {
                var expectedPath = NormalizePath(Path.Combine(candidate, componentName + ".vue"));
                var tracked = openDocuments.FirstOrDefault(openDocument =>
                    openDocument.DocumentKind == DocumentKind.Vue
                    && string.Equals(
                        NormalizePath(openDocument.DocumentPath),
                        expectedPath,
                        StringComparison.OrdinalIgnoreCase));
                if (tracked is not null)
                {
                    resolvedComponent = new WorkspaceVueComponentResolution(
                        componentName,
                        NormalizePath(tracked.DocumentPath),
                        ToImportPath(documentDirectory, tracked.DocumentPath));
                    return true;
                }
            }
        }

        resolvedComponent = default;
        return false;
    }

    public static bool TryResolveTrackedVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        out WorkspaceVueComponentResolution resolvedComponent)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        var tracked = openDocuments.FirstOrDefault(openDocument =>
            openDocument.DocumentKind == DocumentKind.Vue
            && string.Equals(
                Path.GetFileNameWithoutExtension(openDocument.DocumentPath),
                componentName,
                StringComparison.Ordinal));
        if (tracked is not null && !string.IsNullOrWhiteSpace(documentDirectory))
        {
            resolvedComponent = new WorkspaceVueComponentResolution(
                componentName,
                NormalizePath(tracked.DocumentPath),
                ToImportPath(documentDirectory, tracked.DocumentPath));
            return true;
        }

        resolvedComponent = default;
        return false;
    }

    public static IEnumerable<WorkspaceVueComponentResolution> EnumerateTrackedVueComponents(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Vue))
        {
            var componentName = Path.GetFileNameWithoutExtension(openDocument.DocumentPath);
            if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
            {
                continue;
            }

            yield return new WorkspaceVueComponentResolution(
                componentName,
                NormalizePath(openDocument.DocumentPath),
                ToImportPath(documentDirectory, openDocument.DocumentPath));
        }
    }

    public static IEnumerable<WorkspaceVueComponentResolution> EnumerateNearbyVueComponents(string documentPath)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in GetNearbyVueSearchDirectories(documentDirectory))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(directory, "*.vue", SearchOption.TopDirectoryOnly))
            {
                var normalizedPath = NormalizePath(filePath);
                if (!seen.Add(normalizedPath))
                {
                    continue;
                }

                var componentName = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
                {
                    continue;
                }

                yield return new WorkspaceVueComponentResolution(
                    componentName,
                    normalizedPath,
                    ToImportPath(documentDirectory, normalizedPath));
            }
        }
    }

    public static WorkspaceVueComponentResolution? ResolveWorkspaceVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return null;
        }

        foreach (var filePath in EnumerateWorkspaceFiles(
                     GetWorkspaceSearchRoots(documentPath, secondaryDocumentPath: null, openDocuments),
                     componentName + ".vue",
                     cancellationToken))
        {
            return new WorkspaceVueComponentResolution(
                componentName,
                filePath,
                ToImportPath(documentDirectory, filePath));
        }

        return null;
    }

    public static IEnumerable<WorkspaceVueComponentResolution> EnumerateWorkspaceVueComponents(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in EnumerateWorkspaceFiles(
                     GetWorkspaceSearchRoots(documentPath, secondaryDocumentPath: null, openDocuments),
                     "*.vue",
                     cancellationToken))
        {
            var componentName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
            {
                continue;
            }

            var normalizedPath = NormalizePath(filePath);
            if (!seen.Add(normalizedPath))
            {
                continue;
            }

            yield return new WorkspaceVueComponentResolution(
                componentName,
                normalizedPath,
                ToImportPath(documentDirectory, normalizedPath));
        }
    }

    public static IEnumerable<string> GetWorkspaceSearchRoots(
        string documentPath,
        string? secondaryDocumentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var directories = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in new[] { documentPath, secondaryDocumentPath }
                     .Concat(openDocuments
                         .Where(static document => document.DocumentKind is DocumentKind.Jazor or DocumentKind.CSharp or DocumentKind.Vue)
                         .Select(static document => document.DocumentPath)))
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
            {
                continue;
            }

            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            var normalizedDirectory = Path.GetFullPath(directory);
            if (seen.Add(normalizedDirectory))
            {
                directories.Add(normalizedDirectory);
            }
        }

        if (directories.Count == 0)
        {
            yield break;
        }

        if (directories.Count == 1)
        {
            foreach (var ancestor in EnumerateSearchAncestors(directories[0]))
            {
                yield return ancestor;
            }

            yield break;
        }

        if (TryGetCommonSearchAncestor(directories) is { } commonAncestor)
        {
            yield return commonAncestor;
            yield break;
        }

        var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in directories)
        {
            foreach (var ancestor in EnumerateSearchAncestors(directory))
            {
                if (emitted.Add(ancestor))
                {
                    yield return ancestor;
                }
            }
        }
    }

    public static IEnumerable<string> EnumerateWorkspaceFiles(
        IEnumerable<string> searchRoots,
        string searchPattern,
        CancellationToken cancellationToken)
    {
        var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var searchRoot in searchRoots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedRoot = NormalizePath(searchRoot);
            if (string.IsNullOrWhiteSpace(normalizedRoot))
            {
                continue;
            }

            var cacheKey = CreateCacheKey(normalizedRoot, searchPattern);
            if (!WorkspaceFileCache.TryGetValue(cacheKey, out var files))
            {
                files = ScanWorkspaceFiles(searchRoot, searchPattern, cancellationToken);
                WorkspaceFileCache[cacheKey] = files;
            }

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!File.Exists(filePath))
                {
                    WorkspaceFileCache.TryRemove(cacheKey, out _);
                    continue;
                }

                if (visitedFiles.Add(filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    public static async ValueTask<DocumentSnapshot?> ResolveDocumentAsync(
        string candidatePath,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        foreach (var probePath in ExpandPathCandidates(candidatePath))
        {
            var normalizedProbePath = NormalizePath(probePath);
            var trackedDocument = openDocuments.FirstOrDefault(document =>
                string.Equals(
                    NormalizePath(document.DocumentPath),
                    normalizedProbePath,
                    StringComparison.OrdinalIgnoreCase));
            if (trackedDocument is not null)
            {
                return trackedDocument;
            }
        }

        foreach (var probePath in ExpandPathCandidates(candidatePath))
        {
            if (!File.Exists(probePath))
            {
                continue;
            }

            var documentKind = MapDocumentKind(probePath);
            if (documentKind == DocumentKind.Unknown)
            {
                return null;
            }

            try
            {
                return new DocumentSnapshot(
                    NormalizePath(probePath),
                    documentKind,
                    await File.ReadAllTextAsync(probePath, cancellationToken),
                    version: null);
            }
            catch (FileNotFoundException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }
        }

        return null;
    }

    public static string ToImportPath(string documentDirectory, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(documentDirectory, absolutePath)
            .Replace('\\', '/');
        if (relativePath.StartsWith(".", StringComparison.Ordinal))
        {
            return relativePath;
        }

        return "./" + relativePath;
    }

    private static string[] ScanWorkspaceFiles(
        string searchRoot,
        string searchPattern,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(searchRoot))
        {
            return Array.Empty<string>();
        }

        var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<string>();
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(searchRoot);

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentDirectory = pendingDirectories.Pop();
            var normalizedDirectory = NormalizePath(currentDirectory);
            if (!visitedDirectories.Add(normalizedDirectory) || ShouldSkipWorkspaceDirectory(currentDirectory))
            {
                continue;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var filePath in files)
            {
                var normalizedPath = NormalizePath(filePath);
                if (visitedFiles.Add(normalizedPath))
                {
                    results.Add(normalizedPath);
                }
            }

            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in directories)
            {
                if (!ShouldSkipWorkspaceDirectory(childDirectory))
                {
                    pendingDirectories.Push(childDirectory);
                }
            }
        }

        return results.ToArray();
    }

    private static bool IsFrontendImport(string importSource)
        => GetFrontendDocumentKind(importSource) is not null
            || importSource.StartsWith("./", StringComparison.Ordinal)
            || importSource.StartsWith("../", StringComparison.Ordinal)
            || importSource.StartsWith(".\\", StringComparison.Ordinal)
            || importSource.StartsWith("..\\", StringComparison.Ordinal);

    private static IEnumerable<string> EnumerateSearchAncestors(string directory)
    {
        var current = Path.GetFullPath(directory);
        var depth = 0;
        while (!string.IsNullOrWhiteSpace(current) && depth < 3)
        {
            if (string.Equals(current, Path.GetPathRoot(current), StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            yield return current;
            depth++;

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent)
                || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            current = parent;
        }
    }

    private static string? TryGetCommonSearchAncestor(IReadOnlyList<string> directories)
    {
        if (directories.Count == 0)
        {
            return null;
        }

        var current = directories[0];
        for (var index = 1; index < directories.Count; index++)
        {
            current = GetCommonAncestor(current, directories[index]);
            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }
        }

        return string.Equals(current, Path.GetPathRoot(current), StringComparison.OrdinalIgnoreCase)
            ? null
            : current;
    }

    private static string? GetCommonAncestor(string left, string right)
    {
        var candidate = Path.GetFullPath(left);
        var normalizedRight = NormalizePath(right);
        while (!string.IsNullOrWhiteSpace(candidate)
               && !string.Equals(candidate, Path.GetPathRoot(candidate), StringComparison.OrdinalIgnoreCase))
        {
            var normalizedCandidate = NormalizePath(candidate);
            if (normalizedRight.StartsWith(normalizedCandidate + "/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedRight, normalizedCandidate, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }

            candidate = Directory.GetParent(candidate)?.FullName;
        }

        return null;
    }

    private static bool ShouldSkipWorkspaceDirectory(string directoryPath)
    {
        var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return directoryName switch
        {
            ".git" => true,
            ".hg" => true,
            ".svn" => true,
            ".vs" => true,
            ".idea" => true,
            "bin" => true,
            "obj" => true,
            "node_modules" => true,
            ".deno" => true,
            _ => false
        };
    }

    private static bool TryParseCacheKey(string cacheKey, out string normalizedRoot)
    {
        var separatorIndex = cacheKey.LastIndexOf('|');
        if (separatorIndex < 0)
        {
            normalizedRoot = string.Empty;
            return false;
        }

        normalizedRoot = cacheKey[..separatorIndex];
        return true;
    }

    private static string CreateCacheKey(string searchRoot, string searchPattern)
        => NormalizePath(searchRoot) + "|" + searchPattern;

    private static bool PathMatchesOrContains(string left, string right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase)
            || left.StartsWith(right + "/", StringComparison.OrdinalIgnoreCase);

    private static string? GetParentDirectoryPath(string documentDirectory)
    {
        if (Path.IsPathRooted(documentDirectory))
        {
            return Directory.GetParent(documentDirectory)?.FullName;
        }

        var normalized = documentDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (normalized.Length == 0)
        {
            return null;
        }

        return Path.GetDirectoryName(normalized);
    }
}

internal readonly record struct WorkspaceVueComponentResolution(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);
