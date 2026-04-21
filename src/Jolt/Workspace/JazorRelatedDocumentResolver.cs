using Jazor.Vue;
using Jazor.VueContracts.Protocol;

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
    {
        ArgumentNullException.ThrowIfNull(jazorDocument);
        ArgumentNullException.ThrowIfNull(explicitPaths);
        cancellationToken.ThrowIfCancellationRequested();

        var parsed = _parser.Parse(jazorDocument.DocumentPath, jazorDocument.Text);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var candidatePaths = new LinkedHashSet<string>(StringComparer.OrdinalIgnoreCase);

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

        var documents = new List<DocumentSnapshot>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidatePath in candidatePaths)
        {
            var document = await JoltWorkspaceResolver.ResolveDocumentAsync(candidatePath, openDocuments, cancellationToken);
            if (document is null || !IsSupportedFrontendDocument(document))
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

    private static bool IsSupportedFrontendDocument(DocumentSnapshot document)
        => document.DocumentKind is DocumentKind.Vue
            or DocumentKind.JavaScript
            or DocumentKind.TypeScript
            or DocumentKind.Css;

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
