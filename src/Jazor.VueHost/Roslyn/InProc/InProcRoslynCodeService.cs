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
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments: null, cancellationToken, out var context))
            return ValueTask.FromResult<LspHoverResult?>(null);

        var symbol = TryResolveSymbol(context);
        if (symbol is null)
            return ValueTask.FromResult<LspHoverResult?>(null);

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
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryCreateContext(document, position, openDocuments: null, cancellationToken, out var context))
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        var prefix = GetCompletionPrefix(context);
        var members = TryLookupMemberCompletion(context, prefix);
        if (members.Length == 0)
            members = LookupVisibleSymbols(context, prefix);

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
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        var locations = new List<LspLocation>();
        foreach (var location in symbol.Locations)
        {
            if (!TryMapLocationToOriginal(context, location, out var mappedLocation))
                continue;

            locations.Add(mappedLocation);
        }

        return ValueTask.FromResult<IReadOnlyList<LspLocation>>(DeduplicateLocations(locations));
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
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

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
            !primaryDocument.ProjectionMap.TryMapToProjectedPosition(
                document.Text,
                originalPosition,
                primaryDocument.ProjectedText,
                out projectedPosition))
        {
            if (!TryCreateFallbackProjectedDocument(document, out var fallbackDocument)
                || !fallbackDocument.ProjectionMap.TryMapToProjectedPosition(
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
    {
        var type = context.SemanticModel.GetTypeInfo(expression).Type;
        if (type is not null)
        {
            return type;
        }

        return context.SemanticModel.GetSymbolInfo(expression).Symbol switch
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
        var candidateOffsets = new[]
        {
            Math.Max(0, Math.Min(projectedOffset, maxOffset)),
            Math.Max(0, Math.Min(projectedOffset - 1, maxOffset))
        };
        var seenTokens = new HashSet<TextSpan>();

        foreach (var offset in candidateOffsets)
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
            var symbol = projectedDocument.SemanticModel.GetDeclaredSymbol(node, cancellationToken)
                ?? projectedDocument.SemanticModel.GetSymbolInfo(node, cancellationToken).Symbol;
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
                    => projectedDocument.SemanticModel.GetSymbolInfo(identifierName, cancellationToken).Symbol,
                GenericNameSyntax genericName when genericName.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetSymbolInfo(genericName, cancellationToken).Symbol,
                MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetSymbolInfo(memberAccess.Name, cancellationToken).Symbol
                        ?? projectedDocument.SemanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol,
                VariableDeclaratorSyntax variableDeclarator when variableDeclarator.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken),
                MethodDeclarationSyntax methodDeclaration when methodDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken),
                PropertyDeclarationSyntax propertyDeclaration when propertyDeclaration.Identifier.Span.IntersectsWith(token.Span)
                    => projectedDocument.SemanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken),
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

    private static bool TryMapLocationToOriginal(
        RoslynCodeContext context,
        Location location,
        out LspLocation mappedLocation)
    {
        mappedLocation = null!;
        if (!location.IsInSource || location.SourceTree is null)
            return false;

        if (!context.ProjectedDocuments.TryGetValue(location.SourceTree, out var projectedDocument))
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
        foreach (var searchPattern in new[] { "*.cs", "*.jazor" })
        {
            foreach (var filePath in VueHostWorkspaceResolver.EnumerateWorkspaceFiles(
                         EnumerateRoslynSearchRoots(primaryDocument, trackedDocuments),
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
        if (range is null)
        {
            return false;
        }

        var selectionRange = TryMapSpanToOriginalRange(context, selectionSpan) ?? range;
        symbol = new LspDocumentSymbol
        {
            Name = declaredSymbol?.Name ?? fallbackName,
            Detail = declaredSymbol?.ToDisplayString(SymbolDisplayFormat),
            Kind = MapDocumentSymbolKind(declaredSymbol, fallbackKind),
            Range = range,
            SelectionRange = selectionRange
        };
        return true;
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
        var offset = Math.Max(0, Math.Min(context.ProjectedOffset, context.ProjectedText.Length));
        var start = offset;
        while (start > 0)
        {
            var character = context.ProjectedText[start - 1];
            if (!char.IsLetterOrDigit(character) && character != '_')
                break;

            start--;
        }

        return context.ProjectedText[start..offset];
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

    private static StringComparer PathComparer
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
