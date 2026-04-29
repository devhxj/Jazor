using Jazor.Vue;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Workspace;

internal sealed class JazorRelatedDocumentResolver
{
    private readonly IJoltWorkspaceStore _workspaceStore;
    private readonly JazorVueParser _parser = new();

    public JazorRelatedDocumentResolver(IJoltWorkspaceStore workspaceStore)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
    }

    public async Task<IReadOnlyList<DocumentSnapshot>> ResolveAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        CancellationToken cancellationToken)
        => await ResolveAsync(jazorDocument, explicitPaths, openDocuments: null, cancellationToken);

    internal async Task<IReadOnlyList<DocumentSnapshot>> ResolveAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(jazorDocument);
        ArgumentNullException.ThrowIfNull(explicitPaths);
        cancellationToken.ThrowIfCancellationRequested();

        openDocuments ??= await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var candidatePaths = await CollectCandidatePathsAsync(
            jazorDocument,
            explicitPaths,
            openDocuments,
            cancellationToken);

        var documents = new List<DocumentSnapshot>();
        var seenPaths = new HashSet<string>(WorkspacePathComparison.StringComparer);

        foreach (var candidatePath in candidatePaths)
        {
            var document = await JoltWorkspaceResolver.ResolveDocumentAsync(candidatePath, openDocuments, cancellationToken);
            if (document is null || !IsSupportedVolarDocument(document))
            {
                continue;
            }

            if (!seenPaths.Add(JoltWorkspaceResolver.NormalizePath(document.DocumentPath)))
            {
                continue;
            }

            documents.Add(document);
        }

        return documents;
    }

    internal async ValueTask<bool> ReferencesPathAsync(
        DocumentSnapshot jazorDocument,
        string candidatePath,
        IReadOnlyList<string> explicitPaths,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(jazorDocument);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);
        ArgumentNullException.ThrowIfNull(explicitPaths);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedCandidatePath = JoltWorkspaceResolver.NormalizePath(candidatePath);
        var candidatePaths = await CollectCandidatePathsAsync(
            jazorDocument,
            explicitPaths,
            openDocuments,
            cancellationToken);
        foreach (var relatedPath in candidatePaths)
        {
            if (string.Equals(
                    JoltWorkspaceResolver.NormalizePath(relatedPath),
                    normalizedCandidatePath,
                    WorkspacePathComparison.StringComparison))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportedVolarDocument(DocumentSnapshot document)
        => document.DocumentKind is DocumentKind.Vue
            or DocumentKind.JavaScript
            or DocumentKind.TypeScript
            or DocumentKind.Css;

    private async Task<LinkedHashSet<string>> CollectCandidatePathsAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        var parsed = JazorVueParser.Parse(jazorDocument.DocumentPath, jazorDocument.Text);
        openDocuments ??= await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var candidatePaths = new LinkedHashSet<string>(WorkspacePathComparison.StringComparer);

        foreach (var explicitPath in explicitPaths)
        {
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                candidatePaths.Add(explicitPath);
            }
        }

        foreach (var importDescriptor in parsed.Imports)
        {
            foreach (var candidatePath in JoltWorkspaceResolver.GetImportPathCandidates(
                         jazorDocument.DocumentPath,
                         importDescriptor.Source))
            {
                candidatePaths.Add(candidatePath);
            }
        }

        foreach (var componentName in GetReferencedVueComponents(jazorDocument.Text))
        {
            if (JoltWorkspaceResolver.TryResolveTrackedNearbyVueComponent(
                    jazorDocument.DocumentPath,
                    componentName,
                    openDocuments,
                    out var trackedNearby))
            {
                candidatePaths.Add(trackedNearby.AbsolutePath);
                continue;
            }

            if (JoltWorkspaceResolver.TryResolveNearbyVueComponent(
                    jazorDocument.DocumentPath,
                    componentName,
                    out var nearbyComponentPath,
                    out _))
            {
                candidatePaths.Add(nearbyComponentPath);
                continue;
            }

            if (JoltWorkspaceResolver.TryResolveTrackedVueComponent(
                    jazorDocument.DocumentPath,
                    componentName,
                    openDocuments,
                    out var tracked))
            {
                candidatePaths.Add(tracked.AbsolutePath);
                continue;
            }

            if (JoltWorkspaceResolver.ResolveWorkspaceVueComponent(
                    jazorDocument.DocumentPath,
                    componentName,
                    openDocuments,
                    cancellationToken) is { } workspaceResolved)
            {
                candidatePaths.Add(workspaceResolved.AbsolutePath);
            }
        }

        foreach (var candidatePath in JoltWorkspaceResolver.GetCoLocatedAssetPaths(jazorDocument.DocumentPath))
        {
            candidatePaths.Add(candidatePath);
        }

        return candidatePaths;
    }

    private static string[] GetReferencedVueComponents(string text)
        => JazorMarkupPatterns.ComponentTagPattern.Matches(text)
            .Select(static match => match.Groups["name"].Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private sealed class LinkedHashSet<T> where T : notnull
    {
        private readonly HashSet<T> _set;
        private readonly List<T> _items = [];

        public LinkedHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        public void Add(T value)
        {
            if (_set.Add(value))
            {
                _items.Add(value);
            }
        }

        public IEnumerator<T> GetEnumerator()
            => _items.GetEnumerator();
    }
}
