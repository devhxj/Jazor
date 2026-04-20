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
        [NotNullWhen(true)] out LspHoverResult? hover)
    {
        hover = null;
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

}
