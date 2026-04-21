using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Razor.InProc;
using Jolt.VirtualDocuments.Mapping;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Jolt.Roslyn.InProc;

internal sealed partial class InProcRoslynCodeService
{
    private const int MaxCompilationCacheEntries = 16;
    private static readonly CSharpParseOptions ParseOptions = new(languageVersion: LanguageVersion.CSharp14);
    private static readonly Regex UsingDirectivePattern = new(
        @"^\s*@using\s+(?<ns>[^\r\n]+)\s*$",
        RegexOptions.Multiline);
    private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
    private static readonly SymbolDisplayFormat SignatureParameterDisplayFormat = new(
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    private readonly JazorVueParser _parser = new();
    private readonly RazorDesignTimeCodeProjectionService _razorProjectionService;
    private readonly ImmutableArray<MetadataReference> _metadataReferences;
    private readonly Lock _compilationCacheGate = new();
    private readonly Dictionary<string, CachedCompilationContext> _compilationCache = new(StringComparer.Ordinal);
    private long _compilationCacheClock;

    public InProcRoslynCodeService(RazorDesignTimeCodeProjectionService? razorProjectionService = null)
    {
        _razorProjectionService = razorProjectionService ?? new RazorDesignTimeCodeProjectionService();
        _metadataReferences = CreateMetadataReferences();
    }

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetHoverAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
            return ValueTask.FromResult<LspHoverResult?>(null);

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            if (TryCreateFallbackHover(context, cancellationToken, out var fallbackHover))
            {
                return ValueTask.FromResult<LspHoverResult?>(fallbackHover);
            }

            return ValueTask.FromResult<LspHoverResult?>(null);
        }

        var range = TryMapSpanToOriginalRange(context, GetPreferredSpan(context, symbol));
        return ValueTask.FromResult<LspHoverResult?>(new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = CreateHoverMarkdown(symbol)
            },
            Range = range
        });
    }

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetCompletionItemsAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        var prefix = GetCompletionPrefix(context);
        var originalPrefix = GetCompletionPrefix(document.Text, LspProtocolHelpers.GetOffset(document.Text, position));
        if (!string.IsNullOrEmpty(originalPrefix))
        {
            prefix = originalPrefix;
        }
        var members = TryLookupMemberCompletion(context, prefix);
        IReadOnlyList<LspCompletionItem> fallbackItems = Array.Empty<LspCompletionItem>();
        if (members.Length == 0)
        {
            members = LookupVisibleSymbols(context, prefix);
            if (members.Length == 0)
            {
                members = LookupDeclaredTypeMemberSymbols(context, prefix, cancellationToken);
                if (members.Length == 0)
                {
                    fallbackItems = LookupFallbackMemberCompletionItems(context, prefix, cancellationToken);
                }
            }
        }

        if (members.Length == 0 && fallbackItems.Count > 0)
        {
            return ValueTask.FromResult(fallbackItems);
        }

        IReadOnlyList<LspCompletionItem> items = members
            .GroupBy(static symbol => symbol.Name, StringComparer.Ordinal)
            .Select(static group => group.First())
            .Select(CreateCompletionItem)
            .ToArray();
        return ValueTask.FromResult(items);
    }

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, originalPosition: null, openDocuments: null, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(CreateDocumentSymbols(context, cancellationToken));
    }

    public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, originalPosition: null, openDocuments: null, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(CreateSemanticTokens(context, cancellationToken));
    }

    public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments: null, cancellationToken, out var context))
            return ValueTask.FromResult<LspSignatureHelp?>(null);

        if (!TryCreateSignatureHelp(context, cancellationToken, out var signatureHelp))
            return ValueTask.FromResult<LspSignatureHelp?>(null);

        return ValueTask.FromResult<LspSignatureHelp?>(signatureHelp);
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetDefinitionAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            var fallbackDefinitions = GetFallbackDefinitionLocations(context, cancellationToken);
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(fallbackDefinitions);
        }

        var locations = new List<LspLocation>();
        foreach (var location in symbol.Locations)
        {
            if (!TryMapLocationToOriginal(context, location, out var mappedLocation))
                continue;

            locations.Add(mappedLocation);
        }

        if (locations.Count == 0)
        {
            var fallbackDefinitions = GetFallbackDefinitionLocations(context, cancellationToken);
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(fallbackDefinitions);
        }

        return ValueTask.FromResult<IReadOnlyList<LspLocation>>(DeduplicateLocations(locations));
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetTypeDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetTypeDefinitionAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspLocation>> GetTypeDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        var symbol = TryResolveSymbol(context);
        var typeSymbol = ResolveTypeDefinitionSymbol(symbol);
        if (typeSymbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        return ValueTask.FromResult(CreateSymbolLocations(context, typeSymbol));
    }

    public ValueTask<IReadOnlyList<LspCallHierarchyItem>> PrepareCallHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => PrepareCallHierarchyAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspCallHierarchyItem>> PrepareCallHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyItem>>(Array.Empty<LspCallHierarchyItem>());
        }

        var symbol = TryResolveSymbol(context);
        if (symbol is null
            || !TryCreateCallHierarchyItem(context, symbol, out var item))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyItem>>(Array.Empty<LspCallHierarchyItem>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyItem>>([item]);
    }

    public ValueTask<IReadOnlyList<LspCallHierarchyIncomingCall>> GetIncomingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetIncomingCallsAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspCallHierarchyIncomingCall>> GetIncomingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyIncomingCall>>(Array.Empty<LspCallHierarchyIncomingCall>());
        }

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyIncomingCall>>(Array.Empty<LspCallHierarchyIncomingCall>());
        }

        var groupedCalls = new Dictionary<string, CallHierarchyRangeGroup>(StringComparer.Ordinal);
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var calledSymbol = GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(invocation, cancellationToken));
                if (!IsCallHierarchyTargetMatch(calledSymbol, symbol))
                {
                    continue;
                }

                var fromRange = TryMapSpanToOriginalRange(projectedDocument, GetInvocationSelectionSpan(invocation));
                if (fromRange is null)
                {
                    continue;
                }

                var callerSymbol = projectedDocument.SemanticModel.GetEnclosingSymbol(invocation.SpanStart, cancellationToken);
                if (callerSymbol is null
                    || SymbolEqualityComparer.Default.Equals(callerSymbol.OriginalDefinition, symbol.OriginalDefinition)
                    || !TryCreateCallHierarchyItem(context, callerSymbol, out var callerItem))
                {
                    continue;
                }

                AddRangeToCallHierarchyGroup(groupedCalls, callerItem, fromRange);
            }

            foreach (var objectCreation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var calledSymbol = GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(objectCreation, cancellationToken));
                if (!IsCallHierarchyTargetMatch(calledSymbol, symbol))
                {
                    continue;
                }

                var fromRange = TryMapSpanToOriginalRange(projectedDocument, objectCreation.Type.Span);
                if (fromRange is null)
                {
                    continue;
                }

                var callerSymbol = projectedDocument.SemanticModel.GetEnclosingSymbol(objectCreation.SpanStart, cancellationToken);
                if (callerSymbol is null
                    || SymbolEqualityComparer.Default.Equals(callerSymbol.OriginalDefinition, symbol.OriginalDefinition)
                    || !TryCreateCallHierarchyItem(context, callerSymbol, out var callerItem))
                {
                    continue;
                }

                AddRangeToCallHierarchyGroup(groupedCalls, callerItem, fromRange);
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyIncomingCall>>(
            groupedCalls.Values
                .Select(static group => new LspCallHierarchyIncomingCall
                {
                    From = group.Item,
                    FromRanges = DeduplicateRanges(group.Ranges)
                })
                .ToArray());
    }

    public ValueTask<IReadOnlyList<LspCallHierarchyOutgoingCall>> GetOutgoingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetOutgoingCallsAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspCallHierarchyOutgoingCall>> GetOutgoingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyOutgoingCall>>(Array.Empty<LspCallHierarchyOutgoingCall>());
        }

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyOutgoingCall>>(Array.Empty<LspCallHierarchyOutgoingCall>());
        }

        var groupedCalls = new Dictionary<string, CallHierarchyRangeGroup>(StringComparer.Ordinal);
        foreach (var declaration in symbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryFindProjectedDocument(context, declaration.SyntaxTree, out var projectedDocument))
            {
                continue;
            }

            var declarationNode = declaration.GetSyntax(cancellationToken);
            foreach (var invocation in declarationNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var calledSymbol = GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(invocation, cancellationToken));
                if (calledSymbol is null
                    || !TryCreateCallHierarchyItem(context, calledSymbol, out var toItem))
                {
                    continue;
                }

                var fromRange = TryMapSpanToOriginalRange(projectedDocument, GetInvocationSelectionSpan(invocation));
                if (fromRange is null)
                {
                    continue;
                }

                AddRangeToCallHierarchyGroup(groupedCalls, toItem, fromRange);
            }

            foreach (var objectCreation in declarationNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var calledSymbol = GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(objectCreation, cancellationToken));
                if (calledSymbol is null
                    || !TryCreateCallHierarchyItem(context, calledSymbol, out var toItem))
                {
                    continue;
                }

                var fromRange = TryMapSpanToOriginalRange(projectedDocument, objectCreation.Type.Span);
                if (fromRange is null)
                {
                    continue;
                }

                AddRangeToCallHierarchyGroup(groupedCalls, toItem, fromRange);
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LspCallHierarchyOutgoingCall>>(
            groupedCalls.Values
                .Select(static group => new LspCallHierarchyOutgoingCall
                {
                    To = group.Item,
                    FromRanges = DeduplicateRanges(group.Ranges)
                })
                .ToArray());
    }

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> PrepareTypeHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => PrepareTypeHierarchyAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> PrepareTypeHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        var typeSymbol = ResolveHierarchyTypeSymbol(TryResolveSymbol(context));
        if (typeSymbol is null
            || !TryCreateTypeHierarchyItem(context, typeSymbol, out var item))
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>([item]);
    }

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySuperTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetTypeHierarchySuperTypesAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySuperTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        var typeSymbol = ResolveHierarchyTypeSymbol(TryResolveSymbol(context));
        if (typeSymbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        var items = new List<LspTypeHierarchyItem>();
        if (typeSymbol.BaseType is { SpecialType: not SpecialType.System_Object } baseType
            && TryCreateTypeHierarchyItem(context, baseType, out var baseItem))
        {
            items.Add(baseItem);
        }

        foreach (var interfaceType in typeSymbol.Interfaces)
        {
            if (TryCreateTypeHierarchyItem(context, interfaceType, out var interfaceItem))
            {
                items.Add(interfaceItem);
            }
        }

        return ValueTask.FromResult(DeduplicateTypeHierarchyItems(items));
    }

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySubTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetTypeHierarchySubTypesAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySubTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        var typeSymbol = ResolveHierarchyTypeSymbol(TryResolveSymbol(context));
        if (typeSymbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspTypeHierarchyItem>>(Array.Empty<LspTypeHierarchyItem>());
        }

        var items = new List<LspTypeHierarchyItem>();
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);
            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var candidate = projectedDocument.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) as INamedTypeSymbol;
                if (candidate is null
                    || SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, typeSymbol.OriginalDefinition)
                    || !IsNamedTypeImplementation(candidate, typeSymbol)
                    || !TryCreateTypeHierarchyItem(context, candidate, out var candidateItem))
                {
                    continue;
                }

                items.Add(candidateItem);
            }
        }

        return ValueTask.FromResult(DeduplicateTypeHierarchyItems(items));
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetImplementationAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        return ValueTask.FromResult(
            FindImplementationLocations(
                context,
                symbol,
                cancellationToken));
    }

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, originalPosition: null, openDocuments: null, cancellationToken, out var context))
            return ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        IReadOnlyList<LspDiagnostic> diagnostics = context.Compilation.GetDiagnostics(cancellationToken)
            .Where(diagnostic => diagnostic.Location.IsInSource && diagnostic.Location.SourceTree == context.SyntaxTree)
            .Select(diagnostic =>
            {
                var range = TryMapSpanToOriginalRange(context, diagnostic.Location.SourceSpan);
                return range is null
                    ? null
                    : new LspDiagnostic
                    {
                        Range = range,
                        Severity = diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => 1,
                            DiagnosticSeverity.Warning => 2,
                            _ => 3
                        },
                        Code = diagnostic.Id,
                        Source = "Roslyn",
                        Message = diagnostic.GetMessage()
                    };
            })
            .Where(static diagnostic => diagnostic is not null)
            .Cast<LspDiagnostic>()
            .ToArray();
        return ValueTask.FromResult(diagnostics);
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        CancellationToken cancellationToken)
        => GetReferencesAsync(
            document,
            position,
            includeDeclaration,
            openDocuments: null,
            cancellationToken);

    public ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetDocumentHighlightsAsync(
            document,
            position,
            openDocuments: null,
            cancellationToken);

    public async ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        var references = await GetReferencesAsync(
            document,
            position,
            includeDeclaration: true,
            openDocuments,
            cancellationToken);
        if (references.Count == 0)
        {
            return Array.Empty<LspDocumentHighlight>();
        }

        var documentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        return references
            .Where(location => string.Equals(location.Uri, documentUri, StringComparison.Ordinal))
            .Select(static location => new LspDocumentHighlight
            {
                Range = location.Range,
                Kind = 1
            })
            .GroupBy(static highlight => string.Join(
                '|',
                highlight.Range.Start.Line,
                highlight.Range.Start.Character,
                highlight.Range.End.Line,
                highlight.Range.End.Character,
                highlight.Kind),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments, cancellationToken, out var context))
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(
                GetFallbackReferenceLocations(
                    context,
                    includeDeclaration,
                    cancellationToken));
        }

        var locations = new List<LspLocation>();
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            foreach (var token in projectedDocument.SyntaxTree.GetRoot(cancellationToken).DescendantTokens())
            {
                if (!token.IsKind(SyntaxKind.IdentifierToken))
                    continue;

                var candidate = TryResolveTokenSymbol(projectedDocument, token, cancellationToken);
                if (candidate is null || !SymbolEqualityComparer.Default.Equals(candidate, symbol))
                    continue;

                var isDeclaration = symbol.Locations.Any(location =>
                    location.IsInSource
                    && location.SourceTree == projectedDocument.SyntaxTree
                    && location.SourceSpan.IntersectsWith(token.Span));
                if (!includeDeclaration && isDeclaration)
                    continue;

                var range = TryMapSpanToOriginalRange(projectedDocument, token.Span);
                if (range is null)
                    continue;

                locations.Add(new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(projectedDocument.Document.DocumentPath),
                    Range = range
                });
            }
        }

        if (locations.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspLocation>>(DeduplicateLocations(locations));
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
        => await GetRenameAsync(
            document,
            position,
            newName,
            openDocuments: null,
            cancellationToken);

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return null;

        var locations = await GetReferencesAsync(
            document,
            position,
            includeDeclaration: true,
            openDocuments,
            cancellationToken);
        if (locations.Count == 0)
            return null;

        var sourceDocuments = await BuildSourceDocumentLookupAsync(document, openDocuments, cancellationToken);
        var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var locationGroup in locations.GroupBy(static location => location.Uri, StringComparer.Ordinal))
        {
            if (!sourceDocuments.TryGetValue(locationGroup.Key, out var sourceDocument))
                continue;

            var edits = locationGroup
                .Select(location => new LspTextEdit
                {
                    Range = location.Range,
                    NewText = newName
                })
                .OrderByDescending(edit => LspProtocolHelpers.GetOffset(sourceDocument.Text, edit.Range.Start))
                .ToArray();
            if (edits.Length == 0)
                continue;

            changes[locationGroup.Key] = edits;
        }

        if (changes.Count == 0)
            return null;

        return new LspWorkspaceEdit
        {
            Changes = changes
        };
    }

    private bool TryCreateContext(
        DocumentSnapshot document,
        LspPosition? originalPosition,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken,
        [NotNullWhen(true)] out RoslynCodeContext? context)
    {
        var projectedDocuments = BuildProjectedDocuments(document, openDocuments, cancellationToken, out var primaryDocument);
        if (projectedDocuments.Count == 0 || primaryDocument is null)
        {
            context = null;
            return false;
        }

        var projectedPosition = new LspPosition();
        if (originalPosition is not null &&
            !TryMapToProjectedPositionWithBoundaryFallback(
                primaryDocument.ProjectionMap,
                document.Text,
                originalPosition,
                primaryDocument.ProjectedText,
                out projectedPosition))
        {
            if (document.DocumentKind == DocumentKind.CSharp
                && PathsEqual(primaryDocument.Document.DocumentPath, document.DocumentPath))
            {
                // C# primary documents use whole-document identity projection. If a
                // position lands on an edge case that fails strict segment lookup
                // (for example EOF-adjacent offsets), fall back to direct identity
                // mapping instead of dropping the entire request.
                projectedPosition = originalPosition;
                goto createCompilation;
            }

            if (!TryCreateFallbackProjectedDocument(document, out var fallbackDocument))
            {
                context = null;
                return false;
            }

            if (!TryMapToProjectedPositionWithBoundaryFallback(
                    fallbackDocument.ProjectionMap,
                    document.Text,
                    originalPosition,
                    fallbackDocument.ProjectedText,
                    out projectedPosition))
            {
                context = null;
                return false;
            }

            for (var index = 0; index < projectedDocuments.Count; index++)
            {
                if (PathsEqual(projectedDocuments[index].Document.DocumentPath, document.DocumentPath))
                {
                    projectedDocuments[index] = fallbackDocument;
                    break;
                }
            }

            primaryDocument = fallbackDocument;
        }

    createCompilation:
        var compilationContext = GetOrCreateCompilationContext(projectedDocuments);
        var primaryContext = compilationContext.ProjectedDocuments.First(projectedDocument =>
            PathsEqual(projectedDocument.Document.DocumentPath, document.DocumentPath));

        context = new RoslynCodeContext(
            document,
            primaryContext.ProjectedText,
            primaryContext.ProjectionMap,
            primaryContext.SyntaxTree,
            compilationContext.Compilation,
            primaryContext.SemanticModel,
            compilationContext.ContextsByTree,
            originalPosition is null
                ? 0
                : LspProtocolHelpers.GetOffset(primaryContext.ProjectedText, projectedPosition),
            projectedPosition);
        return true;
    }

    private sealed record RoslynCodeContext(
        DocumentSnapshot Document,
        string ProjectedText,
        ProjectionMap ProjectionMap,
        SyntaxTree SyntaxTree,
        CSharpCompilation Compilation,
        SemanticModel SemanticModel,
        IReadOnlyDictionary<SyntaxTree, ProjectedDocumentContext> ProjectedDocuments,
        int ProjectedOffset,
        LspPosition ProjectedPosition);

    private sealed class CachedCompilationContext
    {
        public CachedCompilationContext(
            CSharpCompilation compilation,
            IReadOnlyList<ProjectedDocumentContext> projectedDocuments,
            IReadOnlyDictionary<SyntaxTree, ProjectedDocumentContext> contextsByTree,
            long lastUsedTick)
        {
            Compilation = compilation;
            ProjectedDocuments = projectedDocuments;
            ContextsByTree = contextsByTree;
            LastUsedTick = lastUsedTick;
        }

        public CSharpCompilation Compilation { get; }

        public IReadOnlyList<ProjectedDocumentContext> ProjectedDocuments { get; }

        public IReadOnlyDictionary<SyntaxTree, ProjectedDocumentContext> ContextsByTree { get; }

        public long LastUsedTick { get; set; }
    }

    private sealed record ProjectedDocumentContext(
        DocumentSnapshot Document,
        string ProjectedText,
        ProjectionMap ProjectionMap,
        SyntaxTree SyntaxTree,
        SemanticModel SemanticModel);

    private sealed record CallHierarchyRangeGroup(
        LspCallHierarchyItem Item,
        List<LspRange> Ranges);

    private static StringComparer PathComparer
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private readonly record struct FallbackMemberAccess(
        string OwnerName,
        string MemberName);

    private sealed record FallbackMemberDeclaration(
        ProjectedDocumentContext Document,
        TextSpan IdentifierSpan,
        string MemberName,
        SymbolKind Kind,
        string Display);

    private sealed record FallbackMemberLocation(
        ProjectedDocumentContext Document,
        TextSpan IdentifierSpan,
        bool IsDeclaration);
}
