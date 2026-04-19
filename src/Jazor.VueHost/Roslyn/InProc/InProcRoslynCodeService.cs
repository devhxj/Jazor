using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Razor.InProc;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.Roslyn.InProc;

internal sealed class InProcRoslynCodeService
{
    private static readonly CSharpParseOptions ParseOptions = new(languageVersion: LanguageVersion.Preview);
    private static readonly ImmutableArray<MetadataReference> MetadataReferences = CreateMetadataReferences();
    private static readonly Regex UsingDirectivePattern = new(
        @"^\s*@using\s+(?<ns>[^\r\n]+)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);
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

    public InProcRoslynCodeService(RazorDesignTimeCodeProjectionService? razorProjectionService = null)
    {
        _razorProjectionService = razorProjectionService ?? new RazorDesignTimeCodeProjectionService();
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

    private static IReadOnlyList<LspLocation> FindImplementationLocations(
        RoslynCodeContext context,
        ISymbol symbol,
        CancellationToken cancellationToken)
    {
        var locations = new List<LspLocation>();
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            foreach (var token in projectedDocument.SyntaxTree.GetRoot(cancellationToken).DescendantTokens())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!token.IsKind(SyntaxKind.IdentifierToken))
                {
                    continue;
                }

                var candidate = TryResolveTokenSymbol(projectedDocument, token, cancellationToken);
                if (candidate is null
                    || !IsDeclarationToken(candidate, projectedDocument, token.Span)
                    || !IsImplementationSymbol(candidate, symbol))
                {
                    continue;
                }

                var range = TryMapSpanToOriginalRange(projectedDocument, token.Span);
                if (range is null)
                {
                    continue;
                }

                locations.Add(new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(projectedDocument.Document.DocumentPath),
                    Range = range
                });
            }
        }

        return DeduplicateLocations(locations);
    }

    private static bool IsDeclarationToken(
        ISymbol symbol,
        ProjectedDocumentContext projectedDocument,
        TextSpan tokenSpan)
        => symbol.Locations.Any(location =>
            location.IsInSource
            && location.SourceTree == projectedDocument.SyntaxTree
            && location.SourceSpan.IntersectsWith(tokenSpan));

    private static bool IsImplementationSymbol(ISymbol candidateSymbol, ISymbol targetSymbol)
    {
        var candidate = candidateSymbol.OriginalDefinition;
        var target = targetSymbol.OriginalDefinition;

        return (candidate, target) switch
        {
            (IMethodSymbol candidateMethod, IMethodSymbol targetMethod)
                => IsMethodImplementation(candidateMethod, targetMethod),
            (IPropertySymbol candidateProperty, IPropertySymbol targetProperty)
                => IsPropertyImplementation(candidateProperty, targetProperty),
            (IEventSymbol candidateEvent, IEventSymbol targetEvent)
                => IsEventImplementation(candidateEvent, targetEvent),
            (INamedTypeSymbol candidateType, INamedTypeSymbol targetType)
                => IsNamedTypeImplementation(candidateType, targetType),
            _ => false
        };
    }

    private static bool IsMethodImplementation(
        IMethodSymbol candidateMethod,
        IMethodSymbol targetMethod)
    {
        if (targetMethod.MethodKind == MethodKind.Constructor
            || targetMethod.MethodKind == MethodKind.StaticConstructor)
        {
            return false;
        }

        if (targetMethod.ContainingType.TypeKind == TypeKind.Interface)
        {
            if (candidateMethod.ExplicitInterfaceImplementations.Any(implemented =>
                    SymbolEqualityComparer.Default.Equals(
                        implemented.OriginalDefinition,
                        targetMethod.OriginalDefinition)))
            {
                return true;
            }

            var mapped = candidateMethod.ContainingType.FindImplementationForInterfaceMember(targetMethod);
            return mapped is IMethodSymbol mappedMethod
                && SymbolEqualityComparer.Default.Equals(
                    mappedMethod.OriginalDefinition,
                    candidateMethod.OriginalDefinition);
        }

        for (var overridden = candidateMethod.OverriddenMethod;
             overridden is not null;
             overridden = overridden.OverriddenMethod)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    overridden.OriginalDefinition,
                    targetMethod.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPropertyImplementation(
        IPropertySymbol candidateProperty,
        IPropertySymbol targetProperty)
    {
        if (targetProperty.ContainingType.TypeKind == TypeKind.Interface)
        {
            if (candidateProperty.ExplicitInterfaceImplementations.Any(implemented =>
                    SymbolEqualityComparer.Default.Equals(
                        implemented.OriginalDefinition,
                        targetProperty.OriginalDefinition)))
            {
                return true;
            }

            var mapped = candidateProperty.ContainingType.FindImplementationForInterfaceMember(targetProperty);
            return mapped is IPropertySymbol mappedProperty
                && SymbolEqualityComparer.Default.Equals(
                    mappedProperty.OriginalDefinition,
                    candidateProperty.OriginalDefinition);
        }

        for (var overridden = candidateProperty.OverriddenProperty;
             overridden is not null;
             overridden = overridden.OverriddenProperty)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    overridden.OriginalDefinition,
                    targetProperty.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEventImplementation(
        IEventSymbol candidateEvent,
        IEventSymbol targetEvent)
    {
        if (targetEvent.ContainingType.TypeKind == TypeKind.Interface)
        {
            if (candidateEvent.ExplicitInterfaceImplementations.Any(implemented =>
                    SymbolEqualityComparer.Default.Equals(
                        implemented.OriginalDefinition,
                        targetEvent.OriginalDefinition)))
            {
                return true;
            }

            var mapped = candidateEvent.ContainingType.FindImplementationForInterfaceMember(targetEvent);
            return mapped is IEventSymbol mappedEvent
                && SymbolEqualityComparer.Default.Equals(
                    mappedEvent.OriginalDefinition,
                    candidateEvent.OriginalDefinition);
        }

        for (var overridden = candidateEvent.OverriddenEvent;
             overridden is not null;
             overridden = overridden.OverriddenEvent)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    overridden.OriginalDefinition,
                    targetEvent.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNamedTypeImplementation(
        INamedTypeSymbol candidateType,
        INamedTypeSymbol targetType)
    {
        if (targetType.TypeKind == TypeKind.Interface)
        {
            return candidateType.TypeKind != TypeKind.Interface
                && candidateType.AllInterfaces.Any(implementedInterface =>
                    SymbolEqualityComparer.Default.Equals(
                        implementedInterface.OriginalDefinition,
                        targetType.OriginalDefinition));
        }

        if (!targetType.IsAbstract)
        {
            return false;
        }

        for (var baseType = candidateType.BaseType;
             baseType is not null;
             baseType = baseType.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    baseType.OriginalDefinition,
                    targetType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<LspLocation> GetFallbackDefinitionLocations(
        RoslynCodeContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetFallbackMemberAccess(context, cancellationToken, out var memberAccess))
        {
            return Array.Empty<LspLocation>();
        }

        var declarations = FindFallbackMemberDeclarations(context, memberAccess, cancellationToken);
        if (declarations.Count == 0)
        {
            return Array.Empty<LspLocation>();
        }

        var locations = new List<LspLocation>();
        foreach (var declaration in declarations)
        {
            var range = TryMapSpanToOriginalRange(declaration.Document, declaration.IdentifierSpan);
            if (range is null)
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(declaration.Document.Document.DocumentPath),
                Range = range
            });
        }

        return DeduplicateLocations(locations);
    }

    private static IReadOnlyList<LspLocation> GetFallbackReferenceLocations(
        RoslynCodeContext context,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        if (!TryGetFallbackMemberAccess(context, cancellationToken, out var memberAccess))
        {
            return Array.Empty<LspLocation>();
        }

        var declarations = FindFallbackMemberDeclarations(context, memberAccess, cancellationToken);
        var declarationLocations = declarations
            .Select(declaration => new FallbackMemberLocation(declaration.Document, declaration.IdentifierSpan, IsDeclaration: true))
            .ToList();
        var referenceLocations = FindFallbackMemberReferences(context, memberAccess, cancellationToken);

        var locations = new List<LspLocation>();
        foreach (var location in declarationLocations.Concat(referenceLocations))
        {
            if (!includeDeclaration && location.IsDeclaration)
            {
                continue;
            }

            var range = TryMapSpanToOriginalRange(location.Document, location.IdentifierSpan);
            if (range is null)
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(location.Document.Document.DocumentPath),
                Range = range
            });
        }

        return DeduplicateLocations(locations);
    }

    private static bool TryCreateFallbackHover(
        RoslynCodeContext context,
        CancellationToken cancellationToken,
        out LspHoverResult hover)
    {
        hover = null!;
        if (!TryGetFallbackMemberAccess(context, cancellationToken, out var memberAccess))
        {
            return false;
        }

        var declaration = FindFallbackMemberDeclarations(context, memberAccess, cancellationToken)
            .FirstOrDefault();
        if (declaration is null)
        {
            return false;
        }

        var range = TryMapSpanToOriginalRange(declaration.Document, declaration.IdentifierSpan);
        var detail = string.IsNullOrWhiteSpace(declaration.Display)
            ? $"{memberAccess.OwnerName}.{memberAccess.MemberName}"
            : declaration.Display;
        hover = new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"```csharp\n{detail}\n```"
            },
            Range = range
        };
        return true;
    }

    private static IReadOnlyList<LspCompletionItem> LookupFallbackMemberCompletionItems(
        RoslynCodeContext context,
        string prefix,
        CancellationToken cancellationToken)
    {
        if (!TryGetFallbackMemberAccess(context, cancellationToken, out var memberAccess))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var declarations = FindFallbackMemberDeclarations(
            context,
            new FallbackMemberAccess(memberAccess.OwnerName, memberAccess.MemberName),
            cancellationToken);
        if (declarations.Count == 0)
        {
            declarations = FindFallbackMemberDeclarationsByPrefix(context, memberAccess.OwnerName, prefix, cancellationToken);
        }

        return declarations
            .Where(declaration => string.IsNullOrEmpty(prefix)
                || declaration.MemberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .GroupBy(static declaration => declaration.MemberName, StringComparer.Ordinal)
            .Select(static group => group.First())
            .Select(static declaration => new LspCompletionItem
            {
                Label = declaration.MemberName,
                Kind = declaration.Kind switch
                {
                    SymbolKind.Method => 2,
                    SymbolKind.Property => 10,
                    SymbolKind.Field => 5,
                    _ => 6
                },
                Detail = declaration.Display,
                Documentation = "Fallback"
            })
            .ToArray();
    }

    private static IReadOnlyList<FallbackMemberDeclaration> FindFallbackMemberDeclarationsByPrefix(
        RoslynCodeContext context,
        string ownerName,
        string memberPrefix,
        CancellationToken cancellationToken)
    {
        return FindFallbackMemberDeclarationsCore(
            context,
            ownerName,
            candidateName =>
                string.IsNullOrWhiteSpace(memberPrefix)
                || candidateName.StartsWith(memberPrefix, StringComparison.OrdinalIgnoreCase),
            cancellationToken);
    }

    private static IReadOnlyList<FallbackMemberDeclaration> FindFallbackMemberDeclarations(
        RoslynCodeContext context,
        FallbackMemberAccess memberAccess,
        CancellationToken cancellationToken)
        => FindFallbackMemberDeclarationsCore(
            context,
            memberAccess.OwnerName,
            candidateName => string.Equals(candidateName, memberAccess.MemberName, StringComparison.Ordinal),
            cancellationToken);

    private static IReadOnlyList<FallbackMemberDeclaration> FindFallbackMemberDeclarationsCore(
        RoslynCodeContext context,
        string ownerName,
        Func<string, bool> memberNamePredicate,
        CancellationToken cancellationToken)
    {
        var declarations = new List<FallbackMemberDeclaration>();
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);
            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (!string.Equals(typeDeclaration.Identifier.ValueText, ownerName, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var member in typeDeclaration.Members)
                {
                    switch (member)
                    {
                        case FieldDeclarationSyntax fieldDeclaration:
                            foreach (var variable in fieldDeclaration.Declaration.Variables)
                            {
                                if (!memberNamePredicate(variable.Identifier.ValueText))
                                {
                                    continue;
                                }

                                declarations.Add(new FallbackMemberDeclaration(
                                    projectedDocument,
                                    variable.Identifier.Span,
                                    variable.Identifier.ValueText,
                                    SymbolKind.Field,
                                    $"{fieldDeclaration.Declaration.Type} {variable.Identifier.ValueText}"));
                            }
                            break;
                        case PropertyDeclarationSyntax propertyDeclaration when memberNamePredicate(propertyDeclaration.Identifier.ValueText):
                            declarations.Add(new FallbackMemberDeclaration(
                                projectedDocument,
                                propertyDeclaration.Identifier.Span,
                                propertyDeclaration.Identifier.ValueText,
                                SymbolKind.Property,
                                $"{propertyDeclaration.Type} {propertyDeclaration.Identifier.ValueText}"));
                            break;
                        case MethodDeclarationSyntax methodDeclaration when memberNamePredicate(methodDeclaration.Identifier.ValueText):
                            declarations.Add(new FallbackMemberDeclaration(
                                projectedDocument,
                                methodDeclaration.Identifier.Span,
                                methodDeclaration.Identifier.ValueText,
                                SymbolKind.Method,
                                $"{methodDeclaration.ReturnType} {methodDeclaration.Identifier.ValueText}({string.Join(", ", methodDeclaration.ParameterList.Parameters.Select(static parameter => parameter.ToString()))})"));
                            break;
                    }
                }
            }
        }

        return declarations;
    }

    private static IReadOnlyList<FallbackMemberLocation> FindFallbackMemberReferences(
        RoslynCodeContext context,
        FallbackMemberAccess memberAccess,
        CancellationToken cancellationToken)
    {
        var locations = new List<FallbackMemberLocation>();
        foreach (var projectedDocument in context.ProjectedDocuments.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);
            foreach (var memberAccessSyntax in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            {
                if (memberAccessSyntax.Expression is not IdentifierNameSyntax ownerIdentifier
                    || !string.Equals(ownerIdentifier.Identifier.ValueText, memberAccess.OwnerName, StringComparison.Ordinal)
                    || !string.Equals(memberAccessSyntax.Name.Identifier.ValueText, memberAccess.MemberName, StringComparison.Ordinal))
                {
                    continue;
                }

                locations.Add(new FallbackMemberLocation(
                    projectedDocument,
                    memberAccessSyntax.Name.Identifier.Span,
                    IsDeclaration: false));
            }
        }

        return locations;
    }

    private static bool TryGetFallbackMemberAccess(
        RoslynCodeContext context,
        CancellationToken cancellationToken,
        out FallbackMemberAccess memberAccess)
    {
        var projectedDocument = new ProjectedDocumentContext(
            context.Document,
            context.ProjectedText,
            context.ProjectionMap,
            context.SyntaxTree,
            context.SemanticModel);
        var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);
        var maxOffset = Math.Max(0, projectedDocument.ProjectedText.Length - 1);
        foreach (var offset in EnumerateCandidateOffsets(context.ProjectedOffset, maxOffset))
        {
            var token = root.FindToken(offset);
            var memberAccessSyntax = token.Parent?.AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>()
                .FirstOrDefault(candidate => candidate.Name.Span.IntersectsWith(token.Span));
            if (memberAccessSyntax is null
                || memberAccessSyntax.Expression is not IdentifierNameSyntax ownerIdentifier)
            {
                continue;
            }

            var ownerName = ownerIdentifier.Identifier.ValueText;
            var memberName = memberAccessSyntax.Name.Identifier.ValueText;
            if (string.IsNullOrWhiteSpace(ownerName)
                || string.IsNullOrWhiteSpace(memberName))
            {
                continue;
            }

            memberAccess = new FallbackMemberAccess(ownerName, memberName);
            return true;
        }

        memberAccess = default;
        return false;
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
        out RoslynCodeContext context)
    {
        var projectedDocuments = BuildProjectedDocuments(document, openDocuments, cancellationToken, out var primaryDocument);
        if (projectedDocuments.Count == 0 || primaryDocument is null)
        {
            context = null!;
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

            if (!TryCreateFallbackProjectedDocument(document, out var fallbackDocument)
                || !TryMapToProjectedPositionWithBoundaryFallback(
                    fallbackDocument.ProjectionMap,
                    document.Text,
                    originalPosition,
                    fallbackDocument.ProjectedText,
                    out projectedPosition))
            {
                context = null!;
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
        var compilation = CSharpCompilation.Create(
            assemblyName: "__JazorVueHostRoslyn",
            syntaxTrees: projectedDocuments.Select(static projectedDocument => projectedDocument.SyntaxTree),
            references: MetadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var projectedContexts = projectedDocuments
            .Select(projectedDocument => projectedDocument with
            {
                SemanticModel = compilation.GetSemanticModel(projectedDocument.SyntaxTree, ignoreAccessibility: true)
            })
            .ToArray();
        var contextsByTree = projectedContexts.ToDictionary(
            static projectedDocument => projectedDocument.SyntaxTree,
            static projectedDocument => projectedDocument);
        var primaryContext = projectedContexts.First(projectedDocument =>
            PathsEqual(projectedDocument.Document.DocumentPath, document.DocumentPath));

        context = new RoslynCodeContext(
            document,
            primaryContext.ProjectedText,
            primaryContext.ProjectionMap,
            primaryContext.SyntaxTree,
            compilation,
            primaryContext.SemanticModel,
            contextsByTree,
            originalPosition is null
                ? 0
                : LspProtocolHelpers.GetOffset(primaryContext.ProjectedText, projectedPosition),
            projectedPosition);
        return true;
    }

    private static bool TryMapToProjectedPositionWithBoundaryFallback(
        ProjectionMap projectionMap,
        string sourceText,
        LspPosition sourcePosition,
        string projectedText,
        out LspPosition projectedPosition)
    {
        if (projectionMap.TryMapToProjectedPosition(sourceText, sourcePosition, projectedText, out projectedPosition))
        {
            return true;
        }

        var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);
        if (sourceOffset <= 0)
        {
            projectedPosition = new LspPosition();
            return false;
        }

        var maxDelta = sourceOffset;
        for (var delta = 1; delta <= maxDelta; delta++)
        {
            var probeSourceOffset = sourceOffset - delta;
            if (!projectionMap.TryMapToProjectedOffset(probeSourceOffset, out var probeProjectedOffset))
            {
                continue;
            }

            var adjustedProjectedOffset = Math.Min(probeProjectedOffset + delta, projectedText.Length);
            projectedPosition = LspProtocolHelpers.GetPosition(projectedText, adjustedProjectedOffset);
            return true;
        }

        projectedPosition = new LspPosition();
        return false;
    }

    private List<ProjectedDocumentContext> BuildProjectedDocuments(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken,
        out ProjectedDocumentContext? primaryProjectedDocument)
    {
        var projectedDocuments = new List<ProjectedDocumentContext>();
        var seenPaths = new HashSet<string>(PathComparer);

        primaryProjectedDocument = null;
        foreach (var sourceDocument in EnumerateRoslynSourceDocuments(primaryDocument, openDocuments, cancellationToken))
        {
            AddProjectedDocument(sourceDocument, projectedDocuments, seenPaths, out var projectedDocument);
            if (projectedDocument is not null && PathsEqual(sourceDocument.DocumentPath, primaryDocument.DocumentPath))
            {
                primaryProjectedDocument = projectedDocument;
            }
        }

        return projectedDocuments;
    }

    private void AddProjectedDocument(
        DocumentSnapshot document,
        ICollection<ProjectedDocumentContext> projectedDocuments,
        ISet<string> seenPaths,
        out ProjectedDocumentContext? projectedDocument)
    {
        projectedDocument = null;
        if (!seenPaths.Add(GetComparablePath(document.DocumentPath)))
            return;

        if (document.DocumentKind == DocumentKind.CSharp)
        {
            var projectionMap = ProjectionMap.CreateWholeDocument(
                document.DocumentPath,
                document.DocumentPath,
                document.Text.Length,
                document.Text.Length);
            var csharpSyntaxTree = CSharpSyntaxTree.ParseText(
                document.Text,
                ParseOptions,
                path: document.DocumentPath,
                encoding: Encoding.UTF8);
            projectedDocument = new ProjectedDocumentContext(
                document,
                document.Text,
                projectionMap,
                csharpSyntaxTree,
                SemanticModel: null!);
            projectedDocuments.Add(projectedDocument);
            return;
        }

        var parsed = _parser.Parse(document.DocumentPath, document.Text);
        if (string.IsNullOrWhiteSpace(parsed.Code) || parsed.CodeStartIndex < 0)
            return;

        var projection = CreateProjection(document, parsed);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            projection.SourceText,
            ParseOptions,
            path: projection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        projectedDocument = new ProjectedDocumentContext(
            document,
            projection.SourceText,
            projection.ProjectionMap,
            syntaxTree,
            SemanticModel: null!);
        projectedDocuments.Add(projectedDocument);
    }

    private bool TryCreateFallbackProjectedDocument(
        DocumentSnapshot document,
        out ProjectedDocumentContext projectedDocument)
    {
        var parsed = _parser.Parse(document.DocumentPath, document.Text);
        if (string.IsNullOrWhiteSpace(parsed.Code) || parsed.CodeStartIndex < 0)
        {
            projectedDocument = null!;
            return false;
        }

        var projection = CreateFallbackProjection(document, parsed);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            projection.SourceText,
            ParseOptions,
            path: projection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        projectedDocument = new ProjectedDocumentContext(
            document,
            projection.SourceText,
            projection.ProjectionMap,
            syntaxTree,
            SemanticModel: null!);
        return true;
    }

    internal (string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) CreateProjection(DocumentSnapshot document, JazorVueDocument parsed)
    {
        if (_razorProjectionService.TryCreateProjection(document, out var razorProjection))
        {
            return (
                razorProjection.ProjectedDocumentPath,
                razorProjection.SourceText,
                razorProjection.ProjectionMap);
        }

        return CreateFallbackProjection(document, parsed);
    }

    internal ValueTask<IReadOnlyList<DocumentSnapshot>> GetSourceDocumentsAsync(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<DocumentSnapshot>>(
            EnumerateRoslynSourceDocuments(primaryDocument, openDocuments, cancellationToken).ToArray());
    }

    internal static (string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) CreateFallbackProjection(DocumentSnapshot document, JazorVueDocument parsed)
    {
        var projectedPath = "virtual:" + document.DocumentPath + ".inproc.g.cs";
        var sourceText = BuildProjectedSource(document.DocumentPath, document.Text, parsed);
        var projectionMap = new ProjectionMap(
            document.DocumentPath,
            projectedPath,
            TryCreateCodeProjectionSegment(parsed, sourceText, out var segment)
                ? [segment]
                : Array.Empty<ProjectionSegment>());

        return (projectedPath, sourceText, projectionMap);
    }

    internal static string BuildProjectedSource(string documentPath, string sourceText, JazorVueDocument parsed)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Threading.Tasks;");
        foreach (var import in UsingDirectivePattern.Matches(sourceText)
                     .Select(static match => match.Groups["ns"].Value.Trim())
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.Ordinal))
        {
            builder.Append("using ")
                .Append(import.TrimEnd(';'))
                .AppendLine(";");
        }

        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.VueHost.RoslynProjection;");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class PropAttribute : global::System.Attribute { }");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class StateAttribute : global::System.Attribute { }");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class ComputedAttribute : global::System.Attribute { }");
        builder.Append("internal partial class ")
            .Append(CreateContainerName(documentPath))
            .AppendLine();
        builder.AppendLine("{");
        builder.AppendLine(parsed.Code);
        builder.AppendLine("}");
        return builder.ToString();
    }

    internal static bool TryCreateCodeProjectionSegment(
        JazorVueDocument parsed,
        string projectedSource,
        out ProjectionSegment segment)
    {
        if (parsed.CodeStartIndex < 0 || parsed.CodeLength <= 0 || string.IsNullOrWhiteSpace(parsed.Code))
        {
            segment = null!;
            return false;
        }

        var projectedCodeStart = projectedSource.IndexOf(parsed.Code, StringComparison.Ordinal);
        if (projectedCodeStart < 0)
        {
            segment = null!;
            return false;
        }

        segment = new ProjectionSegment(
            parsed.CodeStartIndex,
            parsed.Code.Length,
            projectedCodeStart,
            parsed.Code.Length);
        return true;
    }

    private static string CreateContainerName(string documentPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(documentPath);
        var sanitized = string.Concat((fileName ?? "Document").Select(character =>
            char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
        if (string.IsNullOrWhiteSpace(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            sanitized = "_" + sanitized;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(documentPath));
        var hash = Convert.ToHexString(bytes.AsSpan(0, 4));
        return "__JazorDocument_" + sanitized + "_" + hash;
    }

    private static ImmutableArray<ISymbol> LookupVisibleSymbols(RoslynCodeContext context, string prefix)
    {
        return context.SemanticModel.LookupSymbols(context.ProjectedOffset)
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol.Name))
            .Where(symbol => string.IsNullOrEmpty(prefix) || symbol.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Where(IsCompletionSymbolSupported)
            .OrderBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    private static ImmutableArray<ISymbol> LookupDeclaredTypeMemberSymbols(
        RoslynCodeContext context,
        string prefix,
        CancellationToken cancellationToken)
    {
        var root = context.SyntaxTree.GetRoot(cancellationToken);
        var typeDeclaration = root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();
        if (typeDeclaration is null)
        {
            return ImmutableArray<ISymbol>.Empty;
        }

        var symbols = new List<ISymbol>();
        foreach (var member in typeDeclaration.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (member)
            {
                case FieldDeclarationSyntax fieldDeclaration:
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        var symbol = context.SemanticModel.GetDeclaredSymbol(variable, cancellationToken);
                        if (symbol is not null)
                        {
                            symbols.Add(symbol);
                        }
                    }
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    {
                        var symbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);
                        if (symbol is not null)
                        {
                            symbols.Add(symbol);
                        }
                    }
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    {
                        var symbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);
                        if (symbol is not null)
                        {
                            symbols.Add(symbol);
                        }
                    }
                    break;
            }
        }

        return symbols
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol.Name))
            .Where(symbol => string.IsNullOrEmpty(prefix) || symbol.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Where(IsCompletionSymbolSupported)
            .GroupBy(static symbol => symbol.Name, StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    private static ImmutableArray<ISymbol> TryLookupMemberCompletion(RoslynCodeContext context, string prefix)
    {
        var root = context.SyntaxTree.GetRoot();
        var token = root.FindToken(Math.Max(0, context.ProjectedOffset - 1));
        var memberAccess = token.Parent?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
        if (memberAccess is null || memberAccess.OperatorToken.Span.End > context.ProjectedOffset)
            return ImmutableArray<ISymbol>.Empty;

        var memberContainer = TryResolveMemberCompletionContainer(context, memberAccess.Expression);
        if (memberContainer is null)
            return ImmutableArray<ISymbol>.Empty;

        return memberContainer.GetMembers()
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol.Name))
            .Where(symbol => string.IsNullOrEmpty(prefix) || symbol.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Where(IsCompletionSymbolSupported)
            .OrderBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    private static INamespaceOrTypeSymbol? TryResolveMemberCompletionContainer(
        RoslynCodeContext context,
        ExpressionSyntax expression)
        => TryResolveMemberCompletionContainer(context.SemanticModel, expression);

    private static INamespaceOrTypeSymbol? TryResolveMemberCompletionContainer(
        SemanticModel semanticModel,
        ExpressionSyntax expression)
    {
        var type = semanticModel.GetTypeInfo(expression).Type;
        if (type is not null)
        {
            return type;
        }

        var symbol = GetPrimarySymbol(semanticModel.GetSymbolInfo(expression));
        if (symbol is null
            && expression is IdentifierNameSyntax identifierName
            && !string.IsNullOrWhiteSpace(identifierName.Identifier.ValueText))
        {
            symbol = semanticModel.LookupSymbols(
                    expression.SpanStart,
                    name: identifierName.Identifier.ValueText)
                .FirstOrDefault();
        }

        return symbol switch
        {
            INamespaceOrTypeSymbol namespaceOrType => namespaceOrType,
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            IMethodSymbol method => method.ReturnType,
            _ => null
        };
    }

    private static bool IsCompletionSymbolSupported(ISymbol symbol)
        => symbol.Kind is SymbolKind.Local
            or SymbolKind.Field
            or SymbolKind.Property
            or SymbolKind.Method
            or SymbolKind.Parameter;

    private static LspCompletionItem CreateCompletionItem(ISymbol symbol)
        => new()
        {
            Label = symbol.Name,
            Kind = symbol.Kind switch
            {
                SymbolKind.Method => 2,
                SymbolKind.Property => 10,
                SymbolKind.Field => 5,
                SymbolKind.Parameter => 6,
                _ => 6
            },
            Detail = symbol.ToDisplayString(SymbolDisplayFormat),
            Documentation = symbol.Kind.ToString()
        };

    private static ISymbol? TryResolveSymbol(RoslynCodeContext context)
    {
        return TryResolveSymbolAtPosition(
            new ProjectedDocumentContext(
                context.Document,
                context.ProjectedText,
                context.ProjectionMap,
                context.SyntaxTree,
                context.SemanticModel),
            context.ProjectedOffset,
            CancellationToken.None);
    }

    private static ISymbol? TryResolveSymbolAtPosition(
        ProjectedDocumentContext projectedDocument,
        int projectedOffset,
        CancellationToken cancellationToken)
    {
        var root = projectedDocument.SyntaxTree.GetRoot(cancellationToken);
        var maxOffset = Math.Max(0, projectedDocument.ProjectedText.Length - 1);
        var seenTokens = new HashSet<TextSpan>();

        foreach (var offset in EnumerateCandidateOffsets(projectedOffset, maxOffset))
        {
            var token = root.FindToken(offset);
            if (!seenTokens.Add(token.Span))
            {
                continue;
            }

            if (TryResolveTokenSymbolAtCursor(projectedDocument, token, cancellationToken, out var symbol))
            {
                return symbol;
            }
        }

        return null;
    }

    private static ISymbol? TryResolveTokenSymbol(
        ProjectedDocumentContext projectedDocument,
        SyntaxToken token,
        CancellationToken cancellationToken)
    {
        for (var node = token.Parent; node is not null; node = node.Parent)
        {
            var symbolInfo = projectedDocument.SemanticModel.GetSymbolInfo(node, cancellationToken);
            var symbol = projectedDocument.SemanticModel.GetDeclaredSymbol(node, cancellationToken)
                ?? GetPrimarySymbol(symbolInfo);
            if (symbol is not null)
                return symbol;
        }

        return null;
    }

    private static bool TryResolveTokenSymbolAtCursor(
        ProjectedDocumentContext projectedDocument,
        SyntaxToken token,
        CancellationToken cancellationToken,
        out ISymbol symbol)
    {
        symbol = null!;
        if (token.Parent is null)
        {
            return false;
        }

        foreach (var node in token.Parent.AncestorsAndSelf())
        {
            var resolvedSymbol = node switch
                {
                IdentifierNameSyntax identifierName when identifierName.Identifier.Span.IntersectsWith(token.Span)
                    => GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(identifierName, cancellationToken)),
                GenericNameSyntax genericName when genericName.Identifier.Span.IntersectsWith(token.Span)
                    => GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(genericName, cancellationToken)),
                MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Span.IntersectsWith(token.Span)
                    => TryResolveMemberAccessSymbol(projectedDocument, memberAccess, cancellationToken),
                VariableDeclaratorSyntax variableDeclarator when variableDeclarator.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken),
                BaseTypeDeclarationSyntax typeDeclaration when typeDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken),
                DelegateDeclarationSyntax delegateDeclaration when delegateDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(delegateDeclaration, cancellationToken),
                MethodDeclarationSyntax methodDeclaration when methodDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken),
                ConstructorDeclarationSyntax constructorDeclaration when constructorDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(constructorDeclaration, cancellationToken),
                PropertyDeclarationSyntax propertyDeclaration when propertyDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken),
                EventDeclarationSyntax eventDeclaration when eventDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken),
                EnumMemberDeclarationSyntax enumMember when enumMember.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(enumMember, cancellationToken),
                ParameterSyntax parameterSyntax when parameterSyntax.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(parameterSyntax, cancellationToken),
                _ => null
            };

            if (resolvedSymbol is not null)
            {
                symbol = resolvedSymbol;
                return true;
            }
        }

        return false;
    }

    private static TextSpan GetPreferredSpan(RoslynCodeContext context, ISymbol symbol)
    {
        var sourceLocation = symbol.Locations.FirstOrDefault(location => location.IsInSource && location.SourceTree == context.SyntaxTree);
        if (sourceLocation is not null)
            return sourceLocation.SourceSpan;

        var root = context.SyntaxTree.GetRoot();
        return root.FindToken(Math.Max(0, context.ProjectedOffset - 1)).Span;
    }

    private static LspRange? TryMapSpanToOriginalRange(RoslynCodeContext context, TextSpan span)
    {
        return TryMapSpanToOriginalRange(
            new ProjectedDocumentContext(
                context.Document,
                context.ProjectedText,
                context.ProjectionMap,
                context.SyntaxTree,
                context.SemanticModel),
            span);
    }

    private static LspRange? TryMapSpanToOriginalRange(ProjectedDocumentContext projectedDocument, TextSpan span)
    {
        var projectedRange = LspProtocolHelpers.ToRange(projectedDocument.ProjectedText, span.Start, span.Length);
        return projectedDocument.ProjectionMap.TryMapToOriginalRange(
            projectedDocument.ProjectedText,
            projectedRange,
            projectedDocument.Document.Text,
            out var originalRange)
            ? originalRange
            : null;
    }

    private static IReadOnlyList<LspLocation> DeduplicateLocations(IEnumerable<LspLocation> locations)
    {
        var uniqueLocations = new List<LspLocation>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var location in locations)
        {
            var key = $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}";
            if (!seen.Add(key))
                continue;

            uniqueLocations.Add(location);
        }

        return uniqueLocations;
    }

    private static ISymbol? ResolveTypeDefinitionSymbol(ISymbol? symbol)
    {
        if (symbol is null)
        {
            return null;
        }

        symbol = symbol switch
        {
            IAliasSymbol alias => alias.Target,
            _ => symbol
        };

        return symbol switch
        {
            INamedTypeSymbol namedType => namedType.OriginalDefinition,
            IArrayTypeSymbol arrayType => arrayType.ElementType,
            IPointerTypeSymbol pointerType => pointerType.PointedAtType,
            ITypeParameterSymbol typeParameter => typeParameter,
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            IEventSymbol @event => @event.Type,
            IMethodSymbol method when method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                => method.ContainingType,
            IMethodSymbol method => method.ReturnType,
            _ => null
        };
    }

    private static INamedTypeSymbol? ResolveHierarchyTypeSymbol(ISymbol? symbol)
        => ResolveTypeDefinitionSymbol(symbol) switch
        {
            INamedTypeSymbol namedType => namedType.OriginalDefinition,
            _ => null
        };

    private static IReadOnlyList<LspLocation> CreateSymbolLocations(RoslynCodeContext context, ISymbol symbol)
    {
        var locations = new List<LspLocation>();
        foreach (var location in symbol.Locations)
        {
            if (TryMapLocationToOriginal(context, location, out var mappedLocation))
            {
                locations.Add(mappedLocation);
            }
        }

        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            if (!TryFindProjectedDocument(context, syntaxReference.SyntaxTree, out var projectedDocument))
            {
                continue;
            }

            var declarationNode = syntaxReference.GetSyntax(CancellationToken.None);
            var selectionSpan = GetSymbolSelectionSpan(declarationNode);
            var range = TryMapSpanToOriginalRange(projectedDocument, selectionSpan)
                ?? TryMapSpanToOriginalRange(projectedDocument, declarationNode.Span);
            if (range is null)
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(projectedDocument.Document.DocumentPath),
                Range = range
            });
        }

        return DeduplicateLocations(locations);
    }

    private static bool TryCreateCallHierarchyItem(
        RoslynCodeContext context,
        ISymbol symbol,
        out LspCallHierarchyItem item)
    {
        item = null!;
        if (!TryCreateHierarchyLocation(context, symbol, out var uri, out var range, out var selectionRange))
        {
            return false;
        }

        var name = string.IsNullOrWhiteSpace(symbol.Name)
            ? symbol.ToDisplayString(SymbolDisplayFormat)
            : symbol.Name;
        item = new LspCallHierarchyItem
        {
            Name = name,
            Kind = MapHierarchySymbolKind(symbol),
            Uri = uri,
            Range = range,
            SelectionRange = selectionRange,
            Detail = symbol.ToDisplayString(SymbolDisplayFormat)
        };
        return true;
    }

    private static bool TryCreateTypeHierarchyItem(
        RoslynCodeContext context,
        INamedTypeSymbol symbol,
        out LspTypeHierarchyItem item)
    {
        item = null!;
        if (!TryCreateHierarchyLocation(context, symbol, out var uri, out var range, out var selectionRange))
        {
            return false;
        }

        item = new LspTypeHierarchyItem
        {
            Name = symbol.Name,
            Kind = MapHierarchySymbolKind(symbol),
            Uri = uri,
            Range = range,
            SelectionRange = selectionRange,
            Detail = symbol.ToDisplayString(SymbolDisplayFormat)
        };
        return true;
    }

    private static bool TryCreateHierarchyLocation(
        RoslynCodeContext context,
        ISymbol symbol,
        out string uri,
        out LspRange range,
        out LspRange selectionRange)
    {
        uri = string.Empty;
        range = null!;
        selectionRange = null!;

        foreach (var location in symbol.Locations)
        {
            if (!TryMapLocationToOriginal(context, location, out var mappedLocation))
            {
                continue;
            }

            uri = mappedLocation.Uri;
            range = mappedLocation.Range;
            selectionRange = mappedLocation.Range;
            break;
        }

        if (string.IsNullOrEmpty(uri))
        {
            return false;
        }

        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            if (!TryFindProjectedDocument(context, syntaxReference.SyntaxTree, out var projectedDocument))
            {
                continue;
            }

            var declarationNode = syntaxReference.GetSyntax(CancellationToken.None);
            var selectionSpan = GetSymbolSelectionSpan(declarationNode);
            var mappedSelectionRange = TryMapSpanToOriginalRange(projectedDocument, selectionSpan)
                ?? TryMapSpanToOriginalRange(projectedDocument, declarationNode.Span);
            if (mappedSelectionRange is null)
            {
                continue;
            }

            var selectionUri = LspProtocolHelpers.ToDocumentUri(projectedDocument.Document.DocumentPath);
            if (string.Equals(selectionUri, uri, StringComparison.Ordinal))
            {
                selectionRange = mappedSelectionRange;
                return true;
            }
        }

        return true;
    }

    private static TextSpan GetSymbolSelectionSpan(SyntaxNode declarationNode)
        => declarationNode switch
        {
            BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration.Identifier.Span,
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.Span,
            ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.Span,
            PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.Span,
            EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.Span,
            VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Identifier.Span,
            DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.Span,
            EnumMemberDeclarationSyntax enumMemberDeclaration => enumMemberDeclaration.Identifier.Span,
            ParameterSyntax parameterSyntax => parameterSyntax.Identifier.Span,
            _ => declarationNode.Span
        };

    private static int MapHierarchySymbolKind(ISymbol symbol)
        => symbol switch
        {
            INamedTypeSymbol namedType => namedType.TypeKind switch
            {
                TypeKind.Class => 5,
                TypeKind.Interface => 11,
                TypeKind.Struct => 23,
                TypeKind.Enum => 10,
                TypeKind.Delegate => 12,
                _ => 5
            },
            IMethodSymbol => 6,
            IPropertySymbol => 7,
            IFieldSymbol => 8,
            IEventSymbol => 24,
            IParameterSymbol => 13,
            _ => 13
        };

    private static bool IsCallHierarchyTargetMatch(ISymbol? calledSymbol, ISymbol targetSymbol)
    {
        if (calledSymbol is null)
        {
            return false;
        }

        var normalizedCalled = calledSymbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.ReducedExtension } reducedMethod => reducedMethod.ReducedFrom ?? reducedMethod,
            _ => calledSymbol
        };
        var calledDefinition = normalizedCalled.OriginalDefinition;
        var targetDefinition = targetSymbol.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(calledDefinition, targetDefinition))
        {
            return true;
        }

        if (calledDefinition is IMethodSymbol calledMethod
            && calledMethod.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
            && targetDefinition is INamedTypeSymbol targetType)
        {
            return SymbolEqualityComparer.Default.Equals(
                calledMethod.ContainingType.OriginalDefinition,
                targetType.OriginalDefinition);
        }

        return false;
    }

    private static TextSpan GetInvocationSelectionSpan(InvocationExpressionSyntax invocation)
        => invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Span,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Span,
            GenericNameSyntax genericName => genericName.Identifier.Span,
            IdentifierNameSyntax identifierName => identifierName.Identifier.Span,
            _ => invocation.Expression.Span
        };

    private static void AddRangeToCallHierarchyGroup(
        IDictionary<string, CallHierarchyRangeGroup> groups,
        LspCallHierarchyItem item,
        LspRange range)
    {
        var key = CreateHierarchyItemKey(item.Uri, item.SelectionRange);
        if (!groups.TryGetValue(key, out var group))
        {
            group = new CallHierarchyRangeGroup(item, new List<LspRange>());
            groups[key] = group;
        }

        group.Ranges.Add(range);
    }

    private static LspRange[] DeduplicateRanges(IEnumerable<LspRange> ranges)
    {
        var uniqueRanges = new List<LspRange>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var range in ranges)
        {
            var key = CreateHierarchyItemKey(string.Empty, range);
            if (!seen.Add(key))
            {
                continue;
            }

            uniqueRanges.Add(range);
        }

        return uniqueRanges.ToArray();
    }

    private static IReadOnlyList<LspTypeHierarchyItem> DeduplicateTypeHierarchyItems(IEnumerable<LspTypeHierarchyItem> items)
    {
        var uniqueItems = new List<LspTypeHierarchyItem>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var key = CreateHierarchyItemKey(item.Uri, item.SelectionRange);
            if (!seen.Add(key))
            {
                continue;
            }

            uniqueItems.Add(item);
        }

        return uniqueItems;
    }

    private static string CreateHierarchyItemKey(string uri, LspRange range)
        => string.Concat(
            uri,
            "|",
            range.Start.Line,
            ":",
            range.Start.Character,
            "-",
            range.End.Line,
            ":",
            range.End.Character);

    private static bool TryMapLocationToOriginal(
        RoslynCodeContext context,
        Location location,
        out LspLocation mappedLocation)
    {
        mappedLocation = null!;
        if (!location.IsInSource || location.SourceTree is null)
            return false;

        if (!TryFindProjectedDocument(context, location.SourceTree, out var projectedDocument))
            return false;

        var range = TryMapSpanToOriginalRange(projectedDocument, location.SourceSpan);
        if (range is null)
            return false;

        mappedLocation = new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(projectedDocument.Document.DocumentPath),
            Range = range
        };
        return true;
    }

    private static bool TryFindProjectedDocument(
        RoslynCodeContext context,
        SyntaxTree sourceTree,
        out ProjectedDocumentContext projectedDocument)
    {
        if (context.ProjectedDocuments.TryGetValue(sourceTree, out projectedDocument!))
        {
            return true;
        }

        var sourceTreePath = GetComparablePath(sourceTree.FilePath);
        foreach (var candidate in context.ProjectedDocuments.Values)
        {
            if (PathsEqual(candidate.SyntaxTree.FilePath, sourceTreePath)
                || PathsEqual(candidate.Document.DocumentPath, sourceTreePath)
                || PathsEqual(candidate.ProjectionMap.ProjectedDocumentPath, sourceTreePath))
            {
                projectedDocument = candidate;
                return true;
            }
        }

        projectedDocument = null!;
        return false;
    }

    private IEnumerable<DocumentSnapshot> EnumerateRoslynSourceDocuments(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        var seenPaths = new HashSet<string>(PathComparer);

        foreach (var sourceDocument in EnumerateTrackedRoslynDocuments(primaryDocument, openDocuments))
        {
            if (seenPaths.Add(GetComparablePath(sourceDocument.DocumentPath)))
            {
                yield return sourceDocument;
            }
        }

        var trackedDocuments = openDocuments?
            .Where(static document => document.DocumentKind is DocumentKind.Jazor or DocumentKind.CSharp)
            .ToArray() ?? [];
        var searchRoots = EnumerateRoslynSearchRoots(primaryDocument, trackedDocuments).ToArray();
        foreach (var searchRoot in searchRoots)
        {
            var discoveredWorkspaceDocuments = 0;
            foreach (var resolvedDocument in EnumerateWorkspaceRoslynDocuments(
                         [searchRoot],
                         trackedDocuments,
                         seenPaths,
                         cancellationToken))
            {
                discoveredWorkspaceDocuments++;
                yield return resolvedDocument;
            }

            if (discoveredWorkspaceDocuments > 0)
            {
                yield break;
            }
        }
    }

    private IEnumerable<DocumentSnapshot> EnumerateWorkspaceRoslynDocuments(
        IReadOnlyList<string> searchRoots,
        IReadOnlyList<DocumentSnapshot> trackedDocuments,
        ISet<string> seenPaths,
        CancellationToken cancellationToken)
    {
        foreach (var searchPattern in new[] { "*.cs", "*.jazor" })
        {
            foreach (var filePath in VueHostWorkspaceResolver.EnumerateWorkspaceFiles(
                         searchRoots,
                         searchPattern,
                         cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!seenPaths.Add(GetComparablePath(filePath)))
                {
                    continue;
                }

                var resolvedDocument = ResolveWorkspaceRoslynDocument(filePath, trackedDocuments);
                if (resolvedDocument is null
                    || resolvedDocument.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
                {
                    continue;
                }

                yield return resolvedDocument;
            }
        }
    }

    private static IEnumerable<string> EnumerateRoslynSearchRoots(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot> trackedDocuments)
    {
        var searchRoots = new HashSet<string>(PathComparer);

        foreach (var root in VueHostWorkspaceResolver.GetWorkspaceSearchRoots(
                     primaryDocument.DocumentPath,
                     secondaryDocumentPath: null,
                     trackedDocuments))
        {
            searchRoots.Add(GetComparablePath(root));
        }

        foreach (var trackedDocument in trackedDocuments)
        {
            if (!Path.IsPathRooted(trackedDocument.DocumentPath))
            {
                continue;
            }

            foreach (var root in VueHostWorkspaceResolver.GetWorkspaceSearchRoots(
                         primaryDocument.DocumentPath,
                         trackedDocument.DocumentPath,
                         trackedDocuments))
            {
                searchRoots.Add(GetComparablePath(root));
            }
        }

        return searchRoots;
    }

    private static IEnumerable<DocumentSnapshot> EnumerateTrackedRoslynDocuments(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments)
    {
        yield return primaryDocument;
        if (openDocuments is null)
        {
            yield break;
        }

        foreach (var openDocument in openDocuments)
        {
            if (openDocument.DocumentKind is DocumentKind.Jazor or DocumentKind.CSharp)
            {
                yield return openDocument;
            }
        }
    }

    private static DocumentSnapshot? ResolveWorkspaceRoslynDocument(
        string filePath,
        IReadOnlyList<DocumentSnapshot> trackedDocuments)
    {
        var comparablePath = GetComparablePath(filePath);
        var trackedDocument = trackedDocuments.FirstOrDefault(document =>
            PathsEqual(document.DocumentPath, comparablePath));
        if (trackedDocument is not null)
        {
            return trackedDocument;
        }

        var documentKind = VueHostWorkspaceResolver.MapDocumentKind(filePath);
        if (documentKind is not (DocumentKind.Jazor or DocumentKind.CSharp) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            return new DocumentSnapshot(
                comparablePath,
                documentKind,
                File.ReadAllText(filePath),
                version: null);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private ValueTask<IReadOnlyDictionary<string, DocumentSnapshot>> BuildSourceDocumentLookupAsync(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        var lookup = new Dictionary<string, DocumentSnapshot>(StringComparer.Ordinal);
        foreach (var sourceDocument in EnumerateRoslynSourceDocuments(primaryDocument, openDocuments, cancellationToken))
        {
            lookup[LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath)] = sourceDocument;
        }

        return ValueTask.FromResult<IReadOnlyDictionary<string, DocumentSnapshot>>(lookup);
    }

    private static string GetComparablePath(string documentPath)
    {
        var normalizedPath = VueHostWorkspaceResolver.NormalizePath(documentPath);
        return string.IsNullOrWhiteSpace(normalizedPath)
            ? documentPath
            : normalizedPath;
    }

    private static bool PathsEqual(string left, string right)
        => PathComparer.Equals(GetComparablePath(left), GetComparablePath(right));

    private static string CreateHoverMarkdown(ISymbol symbol)
    {
        var signature = symbol.ToDisplayString(SymbolDisplayFormat);
        return $"```csharp\n{signature}\n```\n\nkind: `{symbol.Kind}`";
    }

    private static IReadOnlyList<LspDocumentSymbol> CreateDocumentSymbols(
        RoslynCodeContext context,
        CancellationToken cancellationToken)
    {
        var root = context.SyntaxTree.GetRoot(cancellationToken);
        // The projected source wraps user code in a generated container type. For
        // document symbols we only surface the user's top-level @code members and
        // intentionally ignore generated scaffolding and nested local declarations.
        var typeDeclaration = root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();
        if (typeDeclaration is null)
        {
            return Array.Empty<LspDocumentSymbol>();
        }

        var symbols = new List<LspDocumentSymbol>();
        foreach (var member in typeDeclaration.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (member)
            {
                case FieldDeclarationSyntax fieldDeclaration:
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        if (TryCreateDocumentSymbol(
                            context,
                            fieldDeclaration.Span,
                            variable.Identifier.Span,
                            context.SemanticModel.GetDeclaredSymbol(variable, cancellationToken),
                            fallbackName: variable.Identifier.ValueText,
                            fallbackKind: 8,
                            out var fieldSymbol))
                        {
                            symbols.Add(fieldSymbol);
                        }
                    }
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    if (TryCreateDocumentSymbol(
                        context,
                        propertyDeclaration.Span,
                        propertyDeclaration.Identifier.Span,
                        context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken),
                        fallbackName: propertyDeclaration.Identifier.ValueText,
                        fallbackKind: 7,
                        out var propertySymbol))
                    {
                        symbols.Add(propertySymbol);
                    }
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    if (TryCreateDocumentSymbol(
                        context,
                        methodDeclaration.Span,
                        methodDeclaration.Identifier.Span,
                        context.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken),
                        fallbackName: methodDeclaration.Identifier.ValueText,
                        fallbackKind: 6,
                        out var methodSymbol))
                    {
                        symbols.Add(methodSymbol);
                    }
                    break;
            }
        }

        return symbols
            .OrderBy(static symbol => symbol.Range.Start.Line)
            .ThenBy(static symbol => symbol.Range.Start.Character)
            .ToArray();
    }

    private static bool TryCreateDocumentSymbol(
        RoslynCodeContext context,
        TextSpan rangeSpan,
        TextSpan selectionSpan,
        ISymbol? declaredSymbol,
        string fallbackName,
        int fallbackKind,
        out LspDocumentSymbol symbol)
    {
        symbol = null!;
        var range = TryMapSpanToOriginalRange(context, rangeSpan);
        var selectionRange = TryMapSpanToOriginalRange(context, selectionSpan);
        if (range is null && selectionRange is null)
        {
            return false;
        }

        var resolvedRange = range ?? selectionRange;
        var resolvedSelectionRange = selectionRange ?? range;
        if (resolvedRange is null || resolvedSelectionRange is null)
        {
            return false;
        }

        symbol = new LspDocumentSymbol
        {
            Name = declaredSymbol?.Name ?? fallbackName,
            Detail = declaredSymbol?.ToDisplayString(SymbolDisplayFormat),
            Kind = MapDocumentSymbolKind(declaredSymbol, fallbackKind),
            Range = resolvedRange,
            SelectionRange = resolvedSelectionRange
        };
        return true;
    }

    private static IEnumerable<int> EnumerateCandidateOffsets(int projectedOffset, int maxOffset)
    {
        var seenOffsets = new HashSet<int>();
        foreach (var candidate in new[]
                 {
                     projectedOffset,
                     projectedOffset - 1,
                     projectedOffset + 1,
                     projectedOffset - 2,
                     projectedOffset + 2,
                     projectedOffset - 3,
                     projectedOffset + 3
                 })
        {
            var normalized = Math.Max(0, Math.Min(candidate, maxOffset));
            if (seenOffsets.Add(normalized))
            {
                yield return normalized;
            }
        }
    }

    private static ISymbol? GetPrimarySymbol(SymbolInfo symbolInfo)
        => symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

    private static ISymbol? TryResolveMemberAccessSymbol(
        ProjectedDocumentContext projectedDocument,
        MemberAccessExpressionSyntax memberAccess,
        CancellationToken cancellationToken)
    {
        var symbol = GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(memberAccess.Name, cancellationToken))
            ?? GetPrimarySymbol(projectedDocument.SemanticModel.GetSymbolInfo(memberAccess, cancellationToken));
        if (symbol is not null)
        {
            return symbol;
        }

        var container = TryResolveMemberCompletionContainer(projectedDocument.SemanticModel, memberAccess.Expression);
        if (container is null)
        {
            return null;
        }

        var memberName = memberAccess.Name.Identifier.ValueText;
        if (string.IsNullOrWhiteSpace(memberName))
        {
            return null;
        }

        return container.GetMembers(memberName).FirstOrDefault();
    }

    private static int MapDocumentSymbolKind(ISymbol? symbol, int fallbackKind)
        => symbol?.Kind switch
        {
            SymbolKind.Method => 6,
            SymbolKind.Property => 7,
            SymbolKind.Field => 8,
            SymbolKind.NamedType => 5,
            SymbolKind.Event => 24,
            _ => fallbackKind
        };

    private static IReadOnlyList<LspSemanticToken> CreateSemanticTokens(
        RoslynCodeContext context,
        CancellationToken cancellationToken)
    {
        var tokens = new List<LspSemanticToken>();
        // Only projected tokens that map back into user source survive this pass,
        // which keeps generated Razor scaffolding out of the final semantic stream.
        foreach (var token in context.SyntaxTree.GetRoot(cancellationToken).DescendantTokens())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryCreateSemanticToken(context, token, cancellationToken, out var semanticToken))
            {
                continue;
            }

            tokens.Add(semanticToken);
        }

        return tokens;
    }

    private static bool TryCreateSemanticToken(
        RoslynCodeContext context,
        SyntaxToken token,
        CancellationToken cancellationToken,
        out LspSemanticToken semanticToken)
    {
        semanticToken = null!;
        var range = TryMapSpanToOriginalRange(context, token.Span);
        if (range is null
            || range.Start.Line != range.End.Line
            || range.End.Character <= range.Start.Character
            || !TryGetSemanticTokenClassification(context, token, cancellationToken, out var tokenType, out var tokenModifiers))
        {
            return false;
        }

        semanticToken = new LspSemanticToken
        {
            Line = range.Start.Line,
            Character = range.Start.Character,
            Length = range.End.Character - range.Start.Character,
            TokenType = tokenType,
            TokenModifiers = tokenModifiers
        };
        return true;
    }

    private static bool TryGetSemanticTokenClassification(
        RoslynCodeContext context,
        SyntaxToken token,
        CancellationToken cancellationToken,
        out string tokenType,
        out string[] tokenModifiers)
    {
        tokenModifiers = [];

        if (token.IsKind(SyntaxKind.StringLiteralToken)
            || token.IsKind(SyntaxKind.CharacterLiteralToken)
            || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
        {
            tokenType = "string";
            return true;
        }

        if (token.IsKind(SyntaxKind.NumericLiteralToken))
        {
            tokenType = "number";
            return true;
        }

        if (SyntaxFacts.IsKeywordKind(token.Kind()) || SyntaxFacts.IsContextualKeyword(token.Kind()))
        {
            tokenType = "keyword";
            return true;
        }

        if (!token.IsKind(SyntaxKind.IdentifierToken))
        {
            tokenType = string.Empty;
            return false;
        }

        var symbol = TryResolveTokenSymbol(
            new ProjectedDocumentContext(
                context.Document,
                context.ProjectedText,
                context.ProjectionMap,
                context.SyntaxTree,
                context.SemanticModel),
            token,
            cancellationToken);
        if (symbol is null)
        {
            tokenType = string.Empty;
            return false;
        }

        tokenType = symbol.Kind switch
        {
            SymbolKind.NamedType => "class",
            SymbolKind.Method => "method",
            SymbolKind.Property => "property",
            SymbolKind.Parameter => "parameter",
            SymbolKind.Field or SymbolKind.Local => "variable",
            _ => string.Empty
        };
        if (string.IsNullOrEmpty(tokenType))
        {
            return false;
        }

        var modifiers = new List<string>(3);
        if (IsDeclarationToken(context, symbol, token))
        {
            modifiers.Add("declaration");
        }

        if (symbol.IsStatic)
        {
            modifiers.Add("static");
        }

        if (symbol is IFieldSymbol { IsReadOnly: true }
            || symbol is IPropertySymbol { SetMethod: null })
        {
            modifiers.Add("readonly");
        }

        tokenModifiers = modifiers.ToArray();
        return true;
    }

    private static bool IsDeclarationToken(
        RoslynCodeContext context,
        ISymbol symbol,
        SyntaxToken token)
        => symbol.Locations.Any(location =>
            location.IsInSource
            && location.SourceTree == context.SyntaxTree
            && location.SourceSpan.IntersectsWith(token.Span));

    private static bool TryCreateSignatureHelp(
        RoslynCodeContext context,
        CancellationToken cancellationToken,
        out LspSignatureHelp signatureHelp)
    {
        var root = context.SyntaxTree.GetRoot(cancellationToken);
        var token = root.FindToken(Math.Max(0, context.ProjectedOffset - 1));
        foreach (var node in token.Parent?.AncestorsAndSelf() ?? [])
        {
            if (node is InvocationExpressionSyntax invocation
                && invocation.ArgumentList.Span.Contains(context.ProjectedOffset)
                && TryCreateInvocationSignatureHelp(context, invocation, out signatureHelp))
            {
                return true;
            }

            if (node is ObjectCreationExpressionSyntax objectCreation
                && objectCreation.ArgumentList is not null
                && objectCreation.ArgumentList.Span.Contains(context.ProjectedOffset)
                && TryCreateObjectCreationSignatureHelp(context, objectCreation, out signatureHelp))
            {
                return true;
            }
        }

        signatureHelp = null!;
        return false;
    }

    private static bool TryCreateInvocationSignatureHelp(
        RoslynCodeContext context,
        InvocationExpressionSyntax invocation,
        out LspSignatureHelp signatureHelp)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        var methods = symbolInfo.CandidateSymbols
            .OfType<IMethodSymbol>()
            .Concat(context.SemanticModel.GetMemberGroup(invocation.Expression).OfType<IMethodSymbol>())
            .Prepend(symbolInfo.Symbol as IMethodSymbol)
            .Where(static method => method is not null)
            .Cast<IMethodSymbol>()
            .GroupBy(static method => method.ToDisplayString(SymbolDisplayFormat), StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
        if (methods.Length == 0)
        {
            signatureHelp = null!;
            return false;
        }

        signatureHelp = CreateSignatureHelp(
            methods,
            symbolInfo.Symbol as IMethodSymbol,
            invocation.ArgumentList,
            context.ProjectedOffset);
        return true;
    }

    private static bool TryCreateObjectCreationSignatureHelp(
        RoslynCodeContext context,
        ObjectCreationExpressionSyntax objectCreation,
        out LspSignatureHelp signatureHelp)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(objectCreation);
        var methods = symbolInfo.CandidateSymbols
            .OfType<IMethodSymbol>()
            .Prepend(symbolInfo.Symbol as IMethodSymbol)
            .Where(static method => method is not null)
            .Cast<IMethodSymbol>()
            .GroupBy(static method => method.ToDisplayString(SymbolDisplayFormat), StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
        if (methods.Length == 0 || objectCreation.ArgumentList is null)
        {
            signatureHelp = null!;
            return false;
        }

        signatureHelp = CreateSignatureHelp(
            methods,
            symbolInfo.Symbol as IMethodSymbol,
            objectCreation.ArgumentList,
            context.ProjectedOffset);
        return true;
    }

    private static LspSignatureHelp CreateSignatureHelp(
        IReadOnlyList<IMethodSymbol> methods,
        IMethodSymbol? activeMethod,
        BaseArgumentListSyntax argumentList,
        int projectedOffset)
    {
        var signatures = methods
            .Select(CreateSignatureInformation)
            .ToArray();
        var activeSignature = activeMethod is null
            ? 0
            : Array.FindIndex(methods.ToArray(), method => SymbolEqualityComparer.Default.Equals(method, activeMethod));
        if (activeSignature < 0)
        {
            activeSignature = 0;
        }

        var activeParameter = GetActiveParameterIndex(argumentList, projectedOffset);
        if (signatures.Length > 0 && signatures[activeSignature].Parameters is { Length: > 0 } parameters)
        {
            activeParameter = Math.Min(activeParameter, parameters.Length - 1);
        }
        else
        {
            activeParameter = 0;
        }

        return new LspSignatureHelp
        {
            Signatures = signatures,
            ActiveSignature = activeSignature,
            ActiveParameter = activeParameter
        };
    }

    private static LspSignatureInformation CreateSignatureInformation(IMethodSymbol method)
        => new()
        {
            Label = method.ToDisplayString(SymbolDisplayFormat),
            Parameters = method.Parameters
                .Select(static parameter => new LspParameterInformation
                {
                    Label = parameter.ToDisplayString(SignatureParameterDisplayFormat)
                })
                .ToArray()
        };

    private static int GetActiveParameterIndex(BaseArgumentListSyntax argumentList, int projectedOffset)
    {
        var activeParameter = 0;
        foreach (var argument in argumentList.Arguments)
        {
            if (projectedOffset > argument.FullSpan.End)
            {
                activeParameter++;
                continue;
            }

            break;
        }

        return Math.Max(activeParameter, 0);
    }

    private static string GetCompletionPrefix(RoslynCodeContext context)
    {
        return GetCompletionPrefix(context.ProjectedText, context.ProjectedOffset);
    }

    private static string GetCompletionPrefix(string text, int offset)
    {
        var normalizedOffset = Math.Max(0, Math.Min(offset, text.Length));
        var start = normalizedOffset;
        while (start > 0)
        {
            var character = text[start - 1];
            if (!char.IsLetterOrDigit(character) && character != '_')
                break;

            start--;
        }

        return text[start..normalizedOffset];
    }

    private static ImmutableArray<MetadataReference> CreateMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            return ImmutableArray<MetadataReference>.Empty;

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path))
            .Cast<MetadataReference>()
            .ToImmutableArray();
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
