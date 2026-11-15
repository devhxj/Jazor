using Microsoft.CodeAnalysis;
using System.Text;

namespace Jazor.Compiler;

/// <summary>Structural metadata identity across the original and recovered compilations.</summary>
internal static class LibrarySymbolIdentity
{
    internal static string TypeName(INamedTypeSymbol type)
        => type.ContainingType is { } owner
            ? TypeName(owner) + "+" + type.MetadataName
            : (type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToDisplayString() + ".") + type.MetadataName;

    internal static string Member(ISymbol symbol)
    {
        symbol = symbol.OriginalDefinition;
        var builder = new StringBuilder(symbol.Kind.ToString()).Append(':').Append(symbol.IsStatic ? "static:" : "instance:").Append(symbol.MetadataName);
        if (symbol is IMethodSymbol method)
        {
            builder.Append('`').Append(method.Arity).Append('(');
            AppendParameters(builder, method.Parameters);
            builder.Append(")->").Append((int)method.RefKind).Append(':').Append(Type(method.ReturnType));
        }
        else if (symbol is IPropertySymbol property)
        {
            builder.Append('(');
            AppendParameters(builder, property.Parameters);
            builder.Append(")->").Append((int)property.RefKind).Append(':').Append(Type(property.Type));
        }
        else if (symbol is IFieldSymbol field)
            builder.Append(':').Append(Type(field.Type));
        else if (symbol is IEventSymbol @event)
            builder.Append(':').Append(Type(@event.Type));
        return builder.ToString();
    }

    private static void AppendParameters(StringBuilder builder, IEnumerable<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
            builder.Append((int)parameter.RefKind).Append(':').Append(Type(parameter.Type)).Append(';');
    }

    private static string Type(ITypeSymbol type)
        => type switch
        {
            ITypeParameterSymbol parameter => (parameter.TypeParameterKind == TypeParameterKind.Method ? "!!" : "!" + TypeName(parameter.ContainingType) + ":") + parameter.Ordinal,
            IArrayTypeSymbol array => Type(array.ElementType) + "[" + array.Rank + "]",
            IPointerTypeSymbol pointer => Type(pointer.PointedAtType) + "*",
            INamedTypeSymbol named => (named.SpecialType != SpecialType.None ? "special:" + named.SpecialType :
                named.ContainingAssembly.Identity + ":" + TypeName(named.OriginalDefinition)) +
                (named.TypeArguments.Length == 0 ? string.Empty : "<" + string.Join(",", named.TypeArguments.Select(Type)) + ">") +
                (named.ContainingType is null ? string.Empty : "^" + Type(named.ContainingType)),
            _ => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        };
}
