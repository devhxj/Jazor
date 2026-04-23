using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Jolt.DevServer;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Workspace;

internal static class JoltWorkspaceResolver
{
    private const int MaxWorkspaceCacheEntries = 1000;
    private const int MaxSolutionProjectRootCacheEntries = 128;
    private const int MaxPathSegmentDepth = 256;
    private static readonly ConcurrentDictionary<string, string[]> WorkspaceFileCache = new(WorkspacePathComparison.StringComparer);
    private static readonly object WorkspaceFileCacheSync = new();
    private static readonly Dictionary<string, long> WorkspaceFileCacheAges = new(WorkspacePathComparison.StringComparer);
    private static readonly ConcurrentDictionary<string, string[]> SolutionProjectRootCache = new(WorkspacePathComparison.StringComparer);
    private static readonly object SolutionProjectRootCacheSync = new();
    private static readonly Dictionary<string, long> SolutionProjectRootCacheAges = new(WorkspacePathComparison.StringComparer);
    private static readonly AsyncLocal<string[]?> WorkspaceFolderRoots = new();
    private static readonly string[] WorkspaceBoundaryDirectories =
    [
        ".git",
        ".hg",
        ".svn"
    ];
    private static readonly string[] WorkspaceBoundaryFiles =
    [
        JoltConfigFile.FileName,
        "package.json",
        "global.json",
        "Directory.Build.props",
        "Directory.Build.targets"
    ];
    private static readonly string[] WorkspaceBoundaryProjectPatterns =
    [
        "*.sln",
        "*.slnx",
        "*.csproj",
        "*.fsproj",
        "*.vbproj"
    ];
    private static readonly string[] SolutionBoundaryProjectPatterns =
    [
        "*.slnx"
    ];

    public static IDisposable PushWorkspaceFolderRoots(IEnumerable<string> workspaceFolderRoots)
    {
        ArgumentNullException.ThrowIfNull(workspaceFolderRoots);

        var previous = WorkspaceFolderRoots.Value;
        var normalizedRoots = workspaceFolderRoots
            .Where(static root => !string.IsNullOrWhiteSpace(root))
            .Select(root => Path.GetFullPath(root))
            .Distinct(WorkspacePathComparison.StringComparer)
            .ToArray();
        WorkspaceFolderRoots.Value = normalizedRoots.Length == 0
            ? null
            : normalizedRoots;
        return new WorkspaceFolderRootScope(previous);
    }

    public static void InvalidatePath(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
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
                RemoveWorkspaceCacheEntry(cacheKey);
            }
        }

        if (IsProjectScopeDefinitionPath(normalizedPath))
        {
            InvalidateSolutionProjectRootCaches(normalizedPath);
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
            if (segments.Count > MaxPathSegmentDepth)
            {
                throw new InvalidOperationException(
                    $"Path normalization exceeded the safety limit of {MaxPathSegmentDepth} segments for '{documentPath}'.");
            }
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
            ".css" => DocumentKind.Css,
            _ => DocumentKind.Unknown
        };

    public static DocumentKind? GetVolarDocumentKind(string documentPath)
        => MapDocumentKind(documentPath) switch
        {
            DocumentKind.Vue => DocumentKind.Vue,
            DocumentKind.JavaScript => DocumentKind.JavaScript,
            DocumentKind.TypeScript => DocumentKind.TypeScript,
            DocumentKind.Css => DocumentKind.Css,
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
            yield return documentPath + ".css";
        }

        var slashNormalized = documentPath.Replace('\\', '/');
        if (!string.Equals(documentPath, slashNormalized, StringComparison.Ordinal))
        {
            yield return slashNormalized;
        }

        if (Path.IsPathRooted(documentPath))
        {
            var fullPath = Path.GetFullPath(documentPath);
            if (!string.Equals(documentPath, fullPath, WorkspacePathComparison.StringComparison))
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

    public static IEnumerable<string> GetCoLocatedCodeBehindPaths(string jazorDocumentPath)
    {
        var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
        var fileName = Path.GetFileName(jazorDocumentPath);
        var componentName = Path.GetFileNameWithoutExtension(jazorDocumentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory)
            || string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(componentName))
        {
            yield break;
        }

        yield return Path.Combine(documentDirectory, fileName + ".cs");
        yield return Path.Combine(documentDirectory, componentName + ".cs");
    }

    public static bool TryResolveOwningJazorPath(string codeBehindPath, out string jazorDocumentPath)
    {
        jazorDocumentPath = string.Empty;
        if (string.IsNullOrWhiteSpace(codeBehindPath)
            || !codeBehindPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fullCodeBehindPath = Path.IsPathRooted(codeBehindPath)
            ? Path.GetFullPath(codeBehindPath)
            : codeBehindPath;
        if (fullCodeBehindPath.EndsWith(".jazor.cs", StringComparison.OrdinalIgnoreCase))
        {
            var candidate = fullCodeBehindPath[..^3];
            if (File.Exists(candidate))
            {
                jazorDocumentPath = Path.GetFullPath(candidate);
                return true;
            }

            return false;
        }

        var documentDirectory = Path.GetDirectoryName(fullCodeBehindPath);
        var componentName = Path.GetFileNameWithoutExtension(fullCodeBehindPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(componentName))
        {
            return false;
        }

        var coLocatedJazorPath = Path.Combine(documentDirectory, componentName + ".jazor");
        if (!File.Exists(coLocatedJazorPath))
        {
            return false;
        }

        jazorDocumentPath = Path.GetFullPath(coLocatedJazorPath);
        return true;
    }

    public static IEnumerable<string> GetImportPathCandidates(
        string jazorDocumentPath,
        string importSource)
    {
        if (!IsVolarImport(importSource))
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
        var seen = new HashSet<string>(WorkspacePathComparison.StringComparer);
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
                        WorkspacePathComparison.StringComparison));
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
        // 跟踪态文档解析必须限制在当前项目内，否则同一个 Jolt 实例里
        // 其他项目中同名的 Vue 组件会污染当前项目的解析结果。
        var owningProjectRoot = TryGetOwningProjectRoot(documentPath);
        if (Path.IsPathRooted(documentPath) && string.IsNullOrWhiteSpace(owningProjectRoot))
        {
            resolvedComponent = default;
            return false;
        }

        var tracked = openDocuments.FirstOrDefault(openDocument =>
            openDocument.DocumentKind == DocumentKind.Vue
            && (owningProjectRoot is null
                || IsPathWithinWorkspaceRoot(openDocument.DocumentPath, owningProjectRoot))
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

        // 这里会被补全和桥接逻辑复用，所以只能枚举当前项目内的打开文档，
        // 不能把同 solution 下其他项目的 Vue 文件一起带进来。
        var owningProjectRoot = TryGetOwningProjectRoot(documentPath);
        if (Path.IsPathRooted(documentPath) && string.IsNullOrWhiteSpace(owningProjectRoot))
        {
            yield break;
        }

        foreach (var openDocument in openDocuments.Where(candidate =>
                     candidate.DocumentKind == DocumentKind.Vue
                     && (owningProjectRoot is null
                         || IsPathWithinWorkspaceRoot(candidate.DocumentPath, owningProjectRoot))))
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

        var seen = new HashSet<string>(WorkspacePathComparison.StringComparer);
        foreach (var directory in GetNearbyVueSearchDirectories(documentDirectory))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in SafeEnumerate(
                         Directory.EnumerateFiles(directory, "*.vue", SearchOption.TopDirectoryOnly)))
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

    public static string? TryGetOwningProjectRoot(string documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath) || !Path.IsPathRooted(documentPath))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(documentPath);
        var workspaceRoot = TryGetScopedWorkspaceRootForDocument(fullPath);
        var solutionRoot = TryGetNearestSolutionRoot(fullPath, workspaceRoot);
        if (!string.IsNullOrWhiteSpace(solutionRoot))
        {
            // `slnx` 是唯一认可的解决方案边界。先定位 solution，再把隐式发现
            // 收敛到该 solution 中声明的 owning project。
            if (TryFindContainingProjectRoot(fullPath, GetSolutionProjectRoots(solutionRoot), out var projectRoot))
            {
                return projectRoot;
            }
        }

        return null;
    }

    public static string GetRequiredOwningProjectRoot(string documentPath)
    {
        var fullPath = Path.GetFullPath(documentPath);
        var workspaceRoot = TryGetScopedWorkspaceRootForDocument(fullPath);
        var solutionRoot = TryGetNearestSolutionRoot(fullPath, workspaceRoot);
        if (string.IsNullOrWhiteSpace(solutionRoot))
        {
            throw new InvalidOperationException(
                $"No solution .slnx was found for '{documentPath}'. Open the project from a solution directory that contains a .slnx file.");
        }

        if (TryFindContainingProjectRoot(fullPath, GetSolutionProjectRoots(solutionRoot), out var projectRoot))
        {
            return projectRoot;
        }

        throw new InvalidOperationException(
            $"The file '{documentPath}' is not contained in any project declared by solution '{solutionRoot}'.");
    }

    public static bool IsInSameProjectScope(string primaryDocumentPath, string candidateDocumentPath)
    {
        if (!Path.IsPathRooted(primaryDocumentPath) || !Path.IsPathRooted(candidateDocumentPath))
        {
            return true;
        }

        var primaryProjectRoot = TryGetOwningProjectRoot(primaryDocumentPath);
        if (string.IsNullOrWhiteSpace(primaryProjectRoot))
        {
            return string.Equals(
                NormalizeComparablePath(primaryDocumentPath),
                NormalizeComparablePath(candidateDocumentPath),
                WorkspacePathComparison.StringComparison);
        }

        return IsPathWithinWorkspaceRoot(candidateDocumentPath, primaryProjectRoot);
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

        var seen = new HashSet<string>(WorkspacePathComparison.StringComparer);
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
        if (Path.IsPathRooted(documentPath))
        {
            var owningProjectRoot = GetRequiredOwningProjectRoot(documentPath);
            // 递归扫描是跨项目串扰的主要来源。这里直接返回 owning project 根，
            // 让后续所有文件发现都天然落在项目范围内。
            yield return owningProjectRoot;
            yield break;
        }

        var directories = CollectSearchDirectories(documentPath, secondaryDocumentPath, openDocuments);
        var workspaceFolderRoots = GetScopedWorkspaceFolderRoots();
        if (workspaceFolderRoots.Count > 0)
        {
            foreach (var root in GetWorkspaceSearchRootsWithinFolders(directories, workspaceFolderRoots))
            {
                yield return root;
            }

            yield break;
        }

        foreach (var root in GetDefaultWorkspaceSearchRoots(directories))
        {
            yield return root;
        }
    }

    private static IReadOnlyList<string> CollectSearchDirectories(
        string documentPath,
        string? secondaryDocumentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var directories = new List<string>();
        var seen = new HashSet<string>(WorkspacePathComparison.StringComparer);
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

        return directories;
    }

    private static IReadOnlyList<string> GetScopedWorkspaceFolderRoots()
        => WorkspaceFolderRoots.Value ?? Array.Empty<string>();

    private static IEnumerable<string> GetDefaultWorkspaceSearchRoots(IReadOnlyList<string> directories)
    {
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

        var emitted = new HashSet<string>(WorkspacePathComparison.StringComparer);
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

    private static IEnumerable<string> GetWorkspaceSearchRootsWithinFolders(
        IReadOnlyList<string> directories,
        IReadOnlyList<string> workspaceFolderRoots)
    {
        var normalizedFolderRoots = workspaceFolderRoots
            .Where(static root => !string.IsNullOrWhiteSpace(root))
            .Select(static root => Path.GetFullPath(root))
            .Distinct(WorkspacePathComparison.StringComparer)
            .ToArray();
        if (normalizedFolderRoots.Length == 0)
        {
            yield break;
        }

        var boundedDirectories = directories
            .Select(directory => new
            {
                Directory = directory,
                Root = FindContainingWorkspaceFolderRoot(directory, normalizedFolderRoots)
            })
            .Where(static item => item.Root is not null)
            .Select(static item => new
            {
                item.Directory,
                Root = item.Root!
            })
            .ToArray();
        var primaryDirectory = directories.FirstOrDefault();
        var primaryRoot = string.IsNullOrWhiteSpace(primaryDirectory)
            ? null
            : FindContainingWorkspaceFolderRoot(primaryDirectory, normalizedFolderRoots);
        if (!string.IsNullOrWhiteSpace(primaryRoot))
        {
            boundedDirectories = boundedDirectories
                .Where(item => string.Equals(item.Root, primaryRoot, WorkspacePathComparison.StringComparison))
                .ToArray();
        }

        if (boundedDirectories.Length == 0)
        {
            foreach (var root in normalizedFolderRoots)
            {
                yield return root;
            }

            yield break;
        }

        if (boundedDirectories.Length > 1
            && TryGetCommonSearchAncestor(boundedDirectories.Select(static item => item.Directory).ToArray()) is { } commonAncestor)
        {
            var normalizedCommonAncestor = NormalizeComparablePath(commonAncestor);
            var isBoundedAncestor = boundedDirectories.All(item =>
                PathMatchesOrContains(normalizedCommonAncestor, NormalizeComparablePath(item.Root)));
            if (isBoundedAncestor)
            {
                yield return commonAncestor;
                yield break;
            }
        }

        var emitted = new HashSet<string>(WorkspacePathComparison.StringComparer);
        foreach (var bounded in boundedDirectories)
        {
            foreach (var ancestor in EnumerateSearchAncestors(bounded.Directory, bounded.Root))
            {
                if (emitted.Add(ancestor))
                {
                    yield return ancestor;
                }
            }
        }

        var relevantRoots = boundedDirectories
            .Select(static item => item.Root)
            .Distinct(WorkspacePathComparison.StringComparer)
            .ToArray();
        foreach (var root in relevantRoots)
        {
            if (emitted.Add(root))
            {
                yield return root;
            }
        }
    }

    public static IEnumerable<string> EnumerateWorkspaceFiles(
        IEnumerable<string> searchRoots,
        string searchPattern,
        CancellationToken cancellationToken)
    {
        var visitedFiles = new HashSet<string>(WorkspacePathComparison.StringComparer);
        foreach (var searchRoot in searchRoots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedRoot = NormalizePath(searchRoot);
            if (string.IsNullOrWhiteSpace(normalizedRoot))
            {
                continue;
            }

            if (IsSystemTempRoot(normalizedRoot))
            {
                continue;
            }

            var cacheKey = CreateCacheKey(normalizedRoot, searchPattern);
            if (!TryGetWorkspaceCacheEntry(cacheKey, out var files))
            {
                files = ScanWorkspaceFiles(searchRoot, searchPattern, cancellationToken);
                SetWorkspaceCacheEntry(cacheKey, files);
            }

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!SafeFileExists(filePath))
                {
                    RemoveWorkspaceCacheEntry(cacheKey);
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
                    WorkspacePathComparison.StringComparison));
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
            catch (IOException exception)
            {
                WriteDocumentResolutionWarning(probePath, exception);
                continue;
            }
            catch (UnauthorizedAccessException exception)
            {
                WriteDocumentResolutionWarning(probePath, exception);
                continue;
            }
            catch (NotSupportedException exception)
            {
                WriteDocumentResolutionWarning(probePath, exception);
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

        var visitedDirectories = new HashSet<string>(WorkspacePathComparison.StringComparer);
        var visitedFiles = new HashSet<string>(WorkspacePathComparison.StringComparer);
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

            foreach (var filePath in SafeEnumerate(files))
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

            foreach (var childDirectory in SafeEnumerate(directories))
            {
                if (!ShouldSkipWorkspaceDirectory(childDirectory))
                {
                    pendingDirectories.Push(childDirectory);
                }
            }
        }

        return results.ToArray();
    }

    private static bool IsVolarImport(string importSource)
        => GetVolarDocumentKind(importSource) is not null
            || importSource.StartsWith("./", StringComparison.Ordinal)
            || importSource.StartsWith("../", StringComparison.Ordinal)
            || importSource.StartsWith(".\\", StringComparison.Ordinal)
            || importSource.StartsWith("..\\", StringComparison.Ordinal);

    private static IEnumerable<string> EnumerateSearchAncestors(
        string directory,
        string? stopAtDirectory = null)
    {
        var current = Path.GetFullPath(directory);
        var normalizedStopAt = string.IsNullOrWhiteSpace(stopAtDirectory)
            ? null
            : NormalizeComparablePath(Path.GetFullPath(stopAtDirectory));
        var emittedStopDirectory = false;
        while (!string.IsNullOrWhiteSpace(current))
        {
            var normalizedCurrent = NormalizeComparablePath(current);
            if (normalizedStopAt is not null
                && !PathMatchesOrContains(normalizedCurrent, normalizedStopAt))
            {
                break;
            }

            if (string.Equals(current, Path.GetPathRoot(current), WorkspacePathComparison.StringComparison))
            {
                yield break;
            }

            yield return current;
            if (normalizedStopAt is not null
                && string.Equals(normalizedCurrent, normalizedStopAt, WorkspacePathComparison.StringComparison))
            {
                emittedStopDirectory = true;
                yield break;
            }

            if (normalizedStopAt is null && ContainsWorkspaceBoundaryMarker(current))
            {
                yield break;
            }

            if (normalizedStopAt is null && IsTooBroadTempAncestor(current))
            {
                yield break;
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent)
                || string.Equals(parent, current, WorkspacePathComparison.StringComparison))
            {
                yield break;
            }

            current = parent;
        }

        if (normalizedStopAt is not null && !emittedStopDirectory)
        {
            yield return Path.GetFullPath(stopAtDirectory!);
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

        return string.Equals(current, Path.GetPathRoot(current), WorkspacePathComparison.StringComparison)
            ? null
            : current;
    }

    private static string? GetCommonAncestor(string left, string right)
    {
        var candidate = Path.GetFullPath(left);
        var normalizedRight = NormalizePath(right);
        while (!string.IsNullOrWhiteSpace(candidate)
               && !string.Equals(candidate, Path.GetPathRoot(candidate), WorkspacePathComparison.StringComparison))
        {
            var normalizedCandidate = NormalizePath(candidate);
            if (normalizedRight.StartsWith(normalizedCandidate + "/", WorkspacePathComparison.StringComparison)
                || string.Equals(normalizedRight, normalizedCandidate, WorkspacePathComparison.StringComparison))
            {
                return candidate;
            }

            candidate = Directory.GetParent(candidate)?.FullName;
        }

        return null;
    }

    private static string? FindContainingWorkspaceFolderRoot(
        string directory,
        IReadOnlyList<string> workspaceFolderRoots)
    {
        var normalizedDirectory = NormalizeComparablePath(directory);
        string? bestMatch = null;
        foreach (var root in workspaceFolderRoots)
        {
            var normalizedRoot = NormalizeComparablePath(root);
            if (!PathMatchesOrContains(normalizedDirectory, normalizedRoot))
            {
                continue;
            }

            if (bestMatch is null || normalizedRoot.Length > bestMatch.Length)
            {
                bestMatch = normalizedRoot;
            }
        }

        return bestMatch;
    }

    private static string? TryGetScopedWorkspaceRootForDocument(string documentPath)
    {
        var scopedRoots = GetScopedWorkspaceFolderRoots();
        if (scopedRoots.Count == 0 || !Path.IsPathRooted(documentPath))
        {
            return null;
        }

        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return null;
        }

        return FindContainingWorkspaceFolderRoot(documentDirectory, scopedRoots);
    }

    private static string? TryGetNearestSolutionRoot(string documentPath, string? stopAtDirectory)
    {
        var current = Path.GetDirectoryName(Path.GetFullPath(documentPath));
        if (string.IsNullOrWhiteSpace(current))
        {
            return null;
        }

        var normalizedStopAt = string.IsNullOrWhiteSpace(stopAtDirectory)
            ? null
            : NormalizeComparablePath(Path.GetFullPath(stopAtDirectory));
        while (!string.IsNullOrWhiteSpace(current))
        {
            var normalizedCurrent = NormalizeComparablePath(current);
            if (normalizedStopAt is not null && !PathMatchesOrContains(normalizedCurrent, normalizedStopAt))
            {
                break;
            }

            if (ContainsSolutionBoundaryMarker(current))
            {
                return Path.GetFullPath(current);
            }

            if (string.Equals(current, Path.GetPathRoot(current), WorkspacePathComparison.StringComparison))
            {
                break;
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent)
                || string.Equals(parent, current, WorkspacePathComparison.StringComparison))
            {
                break;
            }

            current = parent;
        }

        return null;
    }

    private static bool IsPathWithinWorkspaceRoot(string path, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(workspaceRoot))
        {
            return false;
        }

        return PathMatchesOrContains(
            NormalizeComparablePath(path),
            NormalizeComparablePath(workspaceRoot));
    }

    private static string[] GetSolutionProjectRoots(string solutionRoot)
    {
        var normalizedRoot = NormalizeComparablePath(Path.GetFullPath(solutionRoot));
        if (!SolutionProjectRootCache.TryGetValue(normalizedRoot, out var roots))
        {
            roots = LoadSolutionProjectRoots(solutionRoot);
            SetSolutionProjectRootCacheEntry(normalizedRoot, roots);
        }
        else
        {
            TouchSolutionProjectRootCacheEntry(normalizedRoot);
        }

        return roots;
    }

    private static string[] LoadSolutionProjectRoots(string solutionRoot)
    {
        if (!Directory.Exists(solutionRoot))
        {
            return [];
        }

        // 以 solution 根为粒度缓存项目目录，避免高频 LSP 请求反复解析 `slnx`。
        var projectRoots = new HashSet<string>(WorkspacePathComparison.StringComparer);
        foreach (var solutionPath in SafeEnumerate(Directory.EnumerateFiles(solutionRoot, "*.slnx", SearchOption.TopDirectoryOnly)))
        {
            foreach (var projectRoot in ReadSlnxProjectRoots(solutionPath))
            {
                projectRoots.Add(projectRoot);
            }
        }

        return projectRoots
            .OrderBy(static root => root, WorkspacePathComparison.StringComparer)
            .ToArray();
    }

    private static IEnumerable<string> ReadSlnxProjectRoots(string solutionPath)
    {
        XDocument document;
        try
        {
            using var stream = File.OpenRead(solutionPath);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            document = XDocument.Load(reader, LoadOptions.None);
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (XmlException)
        {
            yield break;
        }

        var solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(solutionPath));
        if (string.IsNullOrWhiteSpace(solutionDirectory))
        {
            yield break;
        }

        foreach (var project in document.Descendants().Where(static element => element.Name.LocalName == "Project"))
        {
            var projectPath = project.Attribute("Path")?.Value;
            if (string.IsNullOrWhiteSpace(projectPath) || !IsDotNetProjectPath(projectPath))
            {
                continue;
            }

            var fullProjectPath = Path.GetFullPath(Path.Combine(solutionDirectory, projectPath));
            var projectDirectory = Path.GetDirectoryName(fullProjectPath);
            if (!string.IsNullOrWhiteSpace(projectDirectory))
            {
                yield return NormalizeComparablePath(projectDirectory);
            }
        }
    }

    private static bool TryFindContainingProjectRoot(
        string documentPath,
        IReadOnlyList<string> projectRoots,
        out string projectRoot)
    {
        projectRoot = string.Empty;
        var normalizedPath = NormalizeComparablePath(Path.GetFullPath(documentPath));
        foreach (var candidateRoot in projectRoots)
        {
            var normalizedRoot = NormalizeComparablePath(candidateRoot);
            if (!PathMatchesOrContains(normalizedPath, normalizedRoot))
            {
                continue;
            }

            if (projectRoot.Length == 0 || normalizedRoot.Length > projectRoot.Length)
            {
                projectRoot = normalizedRoot;
            }
        }

        return projectRoot.Length > 0;
    }

    private static bool ContainsWorkspaceBoundaryMarker(string directoryPath)
    {
        try
        {
            foreach (var markerDirectory in WorkspaceBoundaryDirectories)
            {
                if (Directory.Exists(Path.Combine(directoryPath, markerDirectory)))
                {
                    return true;
                }
            }

            foreach (var markerFile in WorkspaceBoundaryFiles)
            {
                if (File.Exists(Path.Combine(directoryPath, markerFile)))
                {
                    return true;
                }
            }

            foreach (var searchPattern in WorkspaceBoundaryProjectPatterns)
            {
                if (Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly).Any())
                {
                    return true;
                }
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return false;
    }

    private static bool ContainsSolutionBoundaryMarker(string directoryPath)
    {
        try
        {
            foreach (var searchPattern in SolutionBoundaryProjectPatterns)
            {
                if (Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly).Any())
                {
                    return true;
                }
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return false;
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

    private static bool TryGetWorkspaceCacheEntry(string cacheKey, out string[] files)
    {
        if (!WorkspaceFileCache.TryGetValue(cacheKey, out files!))
        {
            return false;
        }

        TouchWorkspaceCacheEntry(cacheKey);
        return true;
    }

    private static void SetWorkspaceCacheEntry(string cacheKey, string[] files)
    {
        WorkspaceFileCache[cacheKey] = files;
        TouchWorkspaceCacheEntry(cacheKey);

        string[] keysToTrim;
        lock (WorkspaceFileCacheSync)
        {
            if (WorkspaceFileCacheAges.Count <= MaxWorkspaceCacheEntries)
            {
                return;
            }

            keysToTrim = WorkspaceFileCacheAges
                .OrderBy(static pair => pair.Value)
                .Take(WorkspaceFileCacheAges.Count - MaxWorkspaceCacheEntries)
                .Select(static pair => pair.Key)
                .ToArray();

            foreach (var key in keysToTrim)
            {
                WorkspaceFileCacheAges.Remove(key);
            }
        }

        foreach (var key in keysToTrim)
        {
            WorkspaceFileCache.TryRemove(key, out _);
        }
    }

    private static void TouchWorkspaceCacheEntry(string cacheKey)
    {
        lock (WorkspaceFileCacheSync)
        {
            WorkspaceFileCacheAges[cacheKey] = Environment.TickCount64;
        }
    }

    private static void RemoveWorkspaceCacheEntry(string cacheKey)
    {
        WorkspaceFileCache.TryRemove(cacheKey, out _);
        lock (WorkspaceFileCacheSync)
        {
            WorkspaceFileCacheAges.Remove(cacheKey);
        }
    }

    private static void SetSolutionProjectRootCacheEntry(string cacheKey, string[] projectRoots)
    {
        SolutionProjectRootCache[cacheKey] = projectRoots;
        TouchSolutionProjectRootCacheEntry(cacheKey);

        string[] keysToTrim;
        lock (SolutionProjectRootCacheSync)
        {
            if (SolutionProjectRootCacheAges.Count <= MaxSolutionProjectRootCacheEntries)
            {
                return;
            }

            keysToTrim = SolutionProjectRootCacheAges
                .OrderBy(static pair => pair.Value)
                .Take(SolutionProjectRootCacheAges.Count - MaxSolutionProjectRootCacheEntries)
                .Select(static pair => pair.Key)
                .ToArray();

            foreach (var key in keysToTrim)
            {
                SolutionProjectRootCacheAges.Remove(key);
            }
        }

        foreach (var key in keysToTrim)
        {
            SolutionProjectRootCache.TryRemove(key, out _);
        }
    }

    private static void TouchSolutionProjectRootCacheEntry(string cacheKey)
    {
        lock (SolutionProjectRootCacheSync)
        {
            SolutionProjectRootCacheAges[cacheKey] = Environment.TickCount64;
        }
    }

    private static void InvalidateSolutionProjectRootCaches(string normalizedPath)
    {
        foreach (var cacheKey in SolutionProjectRootCache.Keys)
        {
            if (PathMatchesOrContains(normalizedPath, cacheKey)
                || PathMatchesOrContains(cacheKey, normalizedPath))
            {
                SolutionProjectRootCache.TryRemove(cacheKey, out _);
                lock (SolutionProjectRootCacheSync)
                {
                    SolutionProjectRootCacheAges.Remove(cacheKey);
                }
            }
        }
    }

    private static void ClearWorkspaceCache()
    {
        WorkspaceFileCache.Clear();
        lock (WorkspaceFileCacheSync)
        {
            WorkspaceFileCacheAges.Clear();
        }
    }

    private static bool IsSystemTempRoot(string normalizedRoot)
    {
        var normalizedSystemTemp = NormalizePath(Path.GetTempPath())
            .TrimEnd('/', '\\');
        var comparableRoot = normalizedRoot.TrimEnd('/', '\\');
        return string.Equals(
                   comparableRoot,
                   normalizedSystemTemp,
                   WorkspacePathComparison.StringComparison)
               || normalizedSystemTemp.StartsWith(
                   comparableRoot + "/",
                   WorkspacePathComparison.StringComparison);
    }

    private static bool SafeFileExists(string filePath)
    {
        try
        {
            return File.Exists(filePath);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsTooBroadTempAncestor(string directoryPath)
    {
        var normalizedSystemTemp = NormalizePath(Path.GetTempPath()).TrimEnd('/', '\\');
        var normalizedDirectory = NormalizePath(directoryPath).TrimEnd('/', '\\');
        if (string.IsNullOrWhiteSpace(normalizedSystemTemp)
            || string.IsNullOrWhiteSpace(normalizedDirectory))
        {
            return false;
        }

        if (!normalizedDirectory.StartsWith(normalizedSystemTemp + "/", WorkspacePathComparison.StringComparison))
        {
            return false;
        }

        var relativePath = normalizedDirectory[(normalizedSystemTemp.Length + 1)..];
        var segmentCount = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
        return segmentCount <= 1;
    }

    private static bool PathMatchesOrContains(string left, string right)
        => string.Equals(left, right, WorkspacePathComparison.StringComparison)
            || left.StartsWith(right + "/", WorkspacePathComparison.StringComparison);

    private static IEnumerable<string> SafeEnumerate(IEnumerable<string> values)
    {
        IEnumerator<string>? enumerator = null;
        try
        {
            enumerator = values.GetEnumerator();
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        using (enumerator)
        {
            while (true)
            {
                string current;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        yield break;
                    }

                    current = enumerator.Current;
                }
                catch (DirectoryNotFoundException)
                {
                    yield break;
                }
                catch (IOException)
                {
                    yield break;
                }
                catch (UnauthorizedAccessException)
                {
                    yield break;
                }

                yield return current;
            }
        }
    }

    private static void WriteDocumentResolutionWarning(string documentPath, Exception exception)
    {
        try
        {
            Console.Error.WriteLine(JsonSerializer.Serialize(new
            {
                eventType = "workspaceDocumentResolveFailed",
                documentPath,
                errorType = exception.GetType().FullName ?? exception.GetType().Name,
                message = exception.Message,
                timestamp = DateTimeOffset.UtcNow
            }));
        }
        catch (Exception)
        {
            // Resolution failure reporting must not change workspace lookup behavior.
        }
    }

    private static string NormalizeComparablePath(string path)
        => NormalizePath(path).TrimEnd('/', '\\');

    private static bool IsProjectScopeDefinitionPath(string path)
        => IsDotNetProjectPath(path)
            || path.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase);

    private static bool IsDotNetProjectPath(string path)
        => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase);

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

    private sealed class WorkspaceFolderRootScope(string[]? previousRoots) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            WorkspaceFolderRoots.Value = previousRoots;
            _disposed = true;
        }
    }
}

internal readonly record struct WorkspaceVueComponentResolution(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);
