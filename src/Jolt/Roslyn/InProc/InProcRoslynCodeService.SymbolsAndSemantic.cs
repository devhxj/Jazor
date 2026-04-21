using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jolt.Razor.InProc;
using Jolt.VirtualDocuments.Mapping;
using Jolt.Workspace;

namespace Jolt.Roslyn.InProc;

internal sealed partial class InProcRoslynCodeService
{
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
        => symbol.Kind is SymbolKind.Namespace
            or SymbolKind.NamedType
            or SymbolKind.Local
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
                SymbolKind.Namespace => 9,
                SymbolKind.NamedType => 7,
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
        [NotNullWhen(true)] out ISymbol? symbol)
    {
        symbol = null;
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
        [NotNullWhen(true)] out LspCallHierarchyItem? item)
    {
        item = null;
        if (!TryCreateHierarchyLocation(context, symbol, out var uri, out var range, out var selectionRange))
        {
            return false;
        }
        if (range is null || selectionRange is null)
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
        [NotNullWhen(true)] out LspTypeHierarchyItem? item)
    {
        item = null;
        if (!TryCreateHierarchyLocation(context, symbol, out var uri, out var range, out var selectionRange))
        {
            return false;
        }
        if (range is null || selectionRange is null)
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
        out LspRange? range,
        out LspRange? selectionRange)
    {
        uri = string.Empty;
        range = null;
        selectionRange = null;

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
                if (range is null)
                {
                    uri = string.Empty;
                    return false;
                }

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
        [NotNullWhen(true)] out LspLocation? mappedLocation)
    {
        mappedLocation = null;
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
        [NotNullWhen(true)] out ProjectedDocumentContext? projectedDocument)
    {
        if (context.ProjectedDocuments.TryGetValue(sourceTree, out var cachedDocument))
        {
            projectedDocument = cachedDocument;
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

        projectedDocument = null;
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
            foreach (var filePath in JoltWorkspaceResolver.EnumerateWorkspaceFiles(
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

        foreach (var root in JoltWorkspaceResolver.GetWorkspaceSearchRoots(
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

            foreach (var root in JoltWorkspaceResolver.GetWorkspaceSearchRoots(
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

        var documentKind = JoltWorkspaceResolver.MapDocumentKind(filePath);
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
        var normalizedPath = JoltWorkspaceResolver.NormalizePath(documentPath);
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
        [NotNullWhen(true)] out LspDocumentSymbol? symbol)
    {
        symbol = null;
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
        [NotNullWhen(true)] out LspSemanticToken? semanticToken)
    {
        semanticToken = null;
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
        [NotNullWhen(true)] out LspSignatureHelp? signatureHelp)
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

        signatureHelp = null;
        return false;
    }

    private static bool TryCreateInvocationSignatureHelp(
        RoslynCodeContext context,
        InvocationExpressionSyntax invocation,
        [NotNullWhen(true)] out LspSignatureHelp? signatureHelp)
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
            signatureHelp = null;
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
        [NotNullWhen(true)] out LspSignatureHelp? signatureHelp)
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
            signatureHelp = null;
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
}
