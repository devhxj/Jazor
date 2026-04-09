using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp.Routing;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class FrontendLaneService : ILspLane
{
    private const string MissingTemplateImportDiagnosticCode = "JAZORVUEFRONTEND001";
    private static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
    private static readonly Regex TagCompletionPrefixPattern = new(
        @"</?(?<name>[A-Za-z0-9_]*)$",
        RegexOptions.Compiled);
    private readonly JazorLspDocumentService _documentService;
    private readonly IDenoFrontendHost? _denoFrontendHost;

    public FrontendLaneService(
        JazorLspDocumentService documentService,
        IDenoFrontendHost? denoFrontendHost = null)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _denoFrontendHost = denoFrontendHost;
    }

    public LaneKind LaneKind => LaneKind.Frontend;

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        var denoDiagnostics = await TryGetDenoDiagnosticsAsync(document, cancellationToken);
        if (denoDiagnostics.Count > 0)
        {
            diagnostics.AddRange(denoDiagnostics);
        }

        diagnostics.AddRange(await CreateUnresolvedMarkupComponentDiagnosticsAsync(document, cancellationToken));
        return diagnostics
            .GroupBy(static diagnostic =>
                $"{diagnostic.Code}:{diagnostic.Range.Start.Line}:{diagnostic.Range.Start.Character}:{diagnostic.Range.End.Line}:{diagnostic.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryGetDenoHoverAsync(document, position, cancellationToken);
        if (denoResult is not null)
        {
            return denoResult;
        }

        var hover = await _documentService.GetHoverAsync(document, position, cancellationToken);
        return hover ?? CreateFilesystemBackedHover(document, position);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var denoResult = await TryGetDenoCompletionItemsAsync(document, position, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        var fallbackItems = await _documentService.GetCompletionItemsAsync(document, position, cancellationToken);
        return CreateFilesystemBackedCompletionItems(document, position, fallbackItems);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryGetDenoDefinitionsAsync(document, position, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        var locations = await _documentService.GetDefinitionAsync(document, position, cancellationToken);
        return locations.Count > 0
            ? locations
            : CreateFilesystemBackedDefinitions(document, position);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryGetDenoReferencesAsync(document, position, includeDeclaration, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        var locations = await _documentService.GetReferencesAsync(document, position, includeDeclaration, cancellationToken);
        return locations.Count > 0
            ? locations
            : CreateFilesystemBackedReferences(document, position, includeDeclaration);
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryGetDenoRenameAsync(document, position, newName, cancellationToken);
        if (denoResult is not null)
        {
            return denoResult;
        }

        return await _documentService.GetRenameAsync(document, position, newName, cancellationToken);
    }

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => GetTemplateCodeActionsAsync(document, diagnostics, projectionTarget, cancellationToken);

    private async ValueTask<IReadOnlyList<LspCodeAction>> GetTemplateCodeActionsAsync(
        DocumentSnapshot document,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget)
            && !ContainsFrontendTemplateDiagnostic(diagnostics))
        {
            return Array.Empty<LspCodeAction>();
        }

        var actions = new List<LspCodeAction>();
        var fallbackActions = await _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken);
        if (fallbackActions.Count > 0)
        {
            actions.AddRange(fallbackActions);
        }

        return actions;
    }

    private static bool ContainsFrontendTemplateDiagnostic(IReadOnlyList<LspDiagnostic> diagnostics)
        => diagnostics.Any(diagnostic =>
            string.Equals(diagnostic.Code, MissingTemplateImportDiagnosticCode, StringComparison.Ordinal));

    private static bool IsTemplateTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Frontend
            || projectionTarget.RegionKind == DocumentRegionKind.Template;

    private static IReadOnlyList<LspCompletionItem> CreateFilesystemBackedCompletionItems(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<LspCompletionItem> fallbackItems)
    {
        if (fallbackItems.Count > 0
            || !TryGetTagCompletionPrefix(document.Text, position, out var tagPrefix))
        {
            return fallbackItems;
        }

        var items = new List<LspCompletionItem>(fallbackItems);
        var seenLabels = new HashSet<string>(
            fallbackItems.Select(static item => item.Label),
            StringComparer.Ordinal);

        foreach (var suggestion in EnumerateNearbyVueComponentSuggestions(document.DocumentPath))
        {
            if (!seenLabels.Add(suggestion.ComponentName))
            {
                continue;
            }

            if (!suggestion.ComponentName.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(new LspCompletionItem
            {
                Label = suggestion.ComponentName,
                Kind = 7,
                Detail = suggestion.ImportPath,
                Documentation = $"Vue component discovered on disk at `{suggestion.ImportPath}`."
            });
        }

        return items;
    }

    private static LspHoverResult? CreateFilesystemBackedHover(
        DocumentSnapshot document,
        LspPosition position)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol)
            || !TryResolveNearbyVueComponent(document.DocumentPath, symbol.ComponentName, out var resolvedComponent))
        {
            return null;
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{symbol.ComponentName}` resolved to nearby Vue component `{resolvedComponent.ImportPath}`."
            },
            Range = symbol.Range
        };
    }

    private static IReadOnlyList<LspLocation> CreateFilesystemBackedDefinitions(
        DocumentSnapshot document,
        LspPosition position)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol)
            || !TryResolveNearbyVueComponent(document.DocumentPath, symbol.ComponentName, out var resolvedComponent))
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(resolvedComponent.AbsolutePath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            }
        ];
    }

    private static IReadOnlyList<LspLocation> CreateFilesystemBackedReferences(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return Array.Empty<LspLocation>();
        }

        var references = new List<LspLocation>();
        if (includeDeclaration
            && TryResolveNearbyVueComponent(document.DocumentPath, symbol.ComponentName, out var resolvedComponent))
        {
            references.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(resolvedComponent.AbsolutePath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            });
        }

        references.AddRange(FindComponentTagLocations(document, symbol.ComponentName));
        return references;
    }

    private static bool TryGetTagCompletionPrefix(string text, LspPosition position, out string tagPrefix)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        var prefix = text[..Math.Min(offset, text.Length)];
        var match = TagCompletionPrefixPattern.Match(prefix);
        if (!match.Success)
        {
            tagPrefix = string.Empty;
            return false;
        }

        tagPrefix = match.Groups["name"].Value;
        return true;
    }

    private static bool TryFindComponentTagSymbol(string text, LspPosition position, out ComponentTagSymbol symbol)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in ComponentTagPattern.Matches(text))
        {
            var group = match.Groups["name"];
            if (offset < group.Index || offset > group.Index + group.Length)
            {
                continue;
            }

            symbol = new ComponentTagSymbol(
                group.Value,
                new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(text, group.Index),
                    End = LspProtocolHelpers.GetPosition(text, group.Index + group.Length)
                });
            return true;
        }

        symbol = default;
        return false;
    }

    private async ValueTask<IReadOnlyList<LspCompletionItem>> TryGetDenoCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspCompletionItem>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateCompletionItemsAsync(document, position, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspCompletionItem>();
        }
    }

    private async ValueTask<LspHoverResult?> TryGetDenoHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateHoverAsync(document, position, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoDefinitionsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDefinitionAsync(document, position, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateReferencesAsync(
                document,
                position,
                includeDeclaration,
                cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<LspWorkspaceEdit?> TryGetDenoRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateRenameAsync(document, position, newName, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> TryGetDenoDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspDiagnostic>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDiagnosticsAsync(document, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspDiagnostic>();
        }
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> CreateUnresolvedMarkupComponentDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        foreach (Match match in ComponentTagPattern.Matches(document.Text))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var group = match.Groups["name"];
            if (!group.Success
                || await _documentService.IsVueComponentResolvableAsync(document, group.Value, cancellationToken))
            {
                continue;
            }

            diagnostics.Add(new LspDiagnostic
            {
                Range = new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                    End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
                },
                Severity = 2,
                Code = MissingTemplateImportDiagnosticCode,
                Source = "Jazor.VueHost.Frontend",
                Message = $"Razor component '{group.Value}' could not be resolved to a nearby Vue file."
            });
        }

        return diagnostics;
    }

    private static bool TryResolveNearbyVueComponent(
        string documentPath,
        string componentName,
        out ResolvedVueComponent resolvedComponent)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (!string.IsNullOrWhiteSpace(documentDirectory))
        {
            foreach (var candidate in GetImportPathCandidates(documentDirectory, componentName))
            {
                if (!File.Exists(candidate.AbsolutePath))
                {
                    continue;
                }

                resolvedComponent = new ResolvedVueComponent(candidate.AbsolutePath, candidate.ImportPath);
                return true;
            }
        }

        resolvedComponent = default;
        return false;
    }

    private static IEnumerable<(string ComponentName, string ImportPath)> EnumerateNearbyVueComponentSuggestions(
        string documentPath)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in GetSearchDirectories(documentDirectory))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(directory, "*.vue", SearchOption.TopDirectoryOnly))
            {
                var absolutePath = Path.GetFullPath(filePath);
                if (!seenPaths.Add(absolutePath))
                {
                    continue;
                }

                var componentName = Path.GetFileNameWithoutExtension(absolutePath);
                if (string.IsNullOrWhiteSpace(componentName)
                    || !char.IsUpper(componentName[0]))
                {
                    continue;
                }

                yield return (componentName, ToImportPath(documentDirectory, absolutePath));
            }
        }
    }

    private static IEnumerable<string> GetSearchDirectories(string documentDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parentDirectory = Directory.GetParent(documentDirectory)?.FullName;
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

            var fullPath = Path.GetFullPath(directory);
            if (seen.Add(fullPath))
            {
                yield return fullPath;
            }
        }
    }

    private static IEnumerable<(string AbsolutePath, string ImportPath)> GetImportPathCandidates(
        string documentDirectory,
        string componentName)
    {
        var directFileName = componentName + ".vue";
        var parentDirectory = Directory.GetParent(documentDirectory)?.FullName;
        var rawCandidates = new List<string>
        {
            Path.Combine(documentDirectory, directFileName),
            Path.Combine(documentDirectory, "Components", directFileName),
            Path.Combine(documentDirectory, "components", directFileName)
        };

        if (!string.IsNullOrWhiteSpace(parentDirectory))
        {
            rawCandidates.Add(Path.Combine(parentDirectory, directFileName));
            rawCandidates.Add(Path.Combine(parentDirectory, "Components", directFileName));
            rawCandidates.Add(Path.Combine(parentDirectory, "components", directFileName));
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var absolutePath in rawCandidates)
        {
            var normalizedAbsolutePath = Path.GetFullPath(absolutePath);
            if (!seen.Add(normalizedAbsolutePath))
            {
                continue;
            }

            yield return (normalizedAbsolutePath, ToImportPath(documentDirectory, normalizedAbsolutePath));
        }
    }

    private static string ToImportPath(string documentDirectory, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(documentDirectory, absolutePath)
            .Replace('\\', '/');
        if (relativePath.StartsWith(".", StringComparison.Ordinal))
        {
            return relativePath;
        }

        return "./" + relativePath;
    }

    private static IReadOnlyList<LspLocation> FindComponentTagLocations(
        DocumentSnapshot document,
        string componentName)
    {
        var locations = new List<LspLocation>();
        foreach (Match match in ComponentTagPattern.Matches(document.Text))
        {
            var group = match.Groups["name"];
            if (!string.Equals(group.Value, componentName, StringComparison.Ordinal))
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                Range = new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                    End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
                }
            });
        }

        return locations;
    }

    private readonly record struct ComponentTagSymbol(
        string ComponentName,
        LspRange Range);

    private readonly record struct ResolvedVueComponent(
        string AbsolutePath,
        string ImportPath);
}
