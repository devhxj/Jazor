using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ClrRuntimeSelection
{
    public static bool HasRuntimeContent(TypeDeclarationSyntax rootDeclaration)
    {
        if (string.Equals(rootDeclaration.Identifier.ValueText, "RuntimeModule", StringComparison.Ordinal))
            return true;

        foreach (var member in rootDeclaration.Members)
        {
            var attr = SharedGeneration.FindAttribute(member.AttributeLists, "Jazor");
            if (attr is null)
                continue;

            var arguments = attr.ArgumentList?.Arguments;
            if (arguments is null || arguments.Value.Count == 0)
                continue;

            var opText = arguments.Value[0].Expression.ToString();
            if (opText.EndsWith($".{nameof(Op.Import)}", StringComparison.Ordinal) ||
                string.Equals(opText, nameof(Op.Import), StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static bool ShouldInclude(INamedTypeSymbol rootType, ISymbol symbol)
    {
        if (SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, rootType.OriginalDefinition))
            return true;

        if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType?.OriginalDefinition, rootType.OriginalDefinition) &&
            !IsNestedUnder(symbol, rootType))
            return false;

        return rootType.Name switch
        {
            "RuntimeModule" => ShouldIncludeRuntimeModuleSymbol(symbol),
            _ => ShouldIncludeOrdinaryModuleSymbol(symbol)
        };
    }

    private static bool ShouldIncludeOrdinaryModuleSymbol(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol method => IsImportMethod(method) || (IsRuntimeHelperAccessibility(method.DeclaredAccessibility) && !method.IsExtern),
            IFieldSymbol field => IsRuntimeHelperAccessibility(field.DeclaredAccessibility) || field.AssociatedSymbol is IPropertySymbol,
            IPropertySymbol property => IsRuntimeHelperAccessibility(property.DeclaredAccessibility),
            INamedTypeSymbol => false,
            _ => IsRuntimeHelperAccessibility(symbol.DeclaredAccessibility)
        };

    private static bool ShouldIncludeRuntimeModuleSymbol(ISymbol symbol)
        => symbol switch
        {
            INamedTypeSymbol type => type.TypeKind == TypeKind.Class && !type.IsRecord,
            IMethodSymbol method => method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor,
            IPropertySymbol => true,
            IFieldSymbol field => field.AssociatedSymbol is IPropertySymbol || field.DeclaredAccessibility == Accessibility.Private,
            _ => symbol.DeclaredAccessibility == Accessibility.Private
        };

    private static bool IsImportMethod(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not ("JazorAttribute" or "Jazor"))
                continue;

            if (attribute.ConstructorArguments.Length == 0)
                continue;

            var argument = attribute.ConstructorArguments[0];
            if (TryReadOp(argument, out var op) && op == Op.Import)
                return true;

            var raw = argument.Value?.ToString();
            if (string.Equals(raw?.Split('.').Last(), nameof(Op.Import), StringComparison.Ordinal))
                return true;
        }

        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MemberDeclarationSyntax declaration)
                continue;

            var attr = SharedGeneration.FindAttribute(declaration.AttributeLists, "Jazor");
            if (attr is null)
                continue;

            var arguments = attr.ArgumentList?.Arguments;
            if (arguments is null || arguments.Value.Count == 0)
                continue;

            var opText = arguments.Value[0].Expression.ToString();
            if (opText.EndsWith($".{nameof(Op.Import)}", StringComparison.Ordinal) ||
                string.Equals(opText, nameof(Op.Import), StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static bool TryReadOp(TypedConstant argument, out Op op)
    {
        if (argument.Value is not null)
        {
            try
            {
                op = (Op)Convert.ToInt32(argument.Value);
                return true;
            }
            catch
            {
            }
        }

        op = default;
        return false;
    }

    private static bool IsNestedUnder(ISymbol symbol, INamedTypeSymbol rootType)
    {
        for (var current = symbol.ContainingType; current is not null; current = current.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, rootType.OriginalDefinition))
                return true;
        }

        return false;
    }

    private static bool IsRuntimeHelperAccessibility(Accessibility accessibility)
        => accessibility is Accessibility.Private
            or Accessibility.Internal
            or Accessibility.ProtectedAndInternal
            or Accessibility.ProtectedOrInternal;
}
