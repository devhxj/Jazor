using System.Text.Json;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler;

internal static class JoltTypeScriptType
{
    internal static string Map(ITypeSymbol? symbol) => Map(symbol, new(SymbolEqualityComparer.Default));

    internal static string TypeParameterName(ITypeParameterSymbol parameter) => "$" + parameter.Name;

    internal static string Parameters(IMethodSymbol method) => Parameters(method, new(SymbolEqualityComparer.Default));

    internal static string ReturnType(IMethodSymbol method) => ReturnType(method, new(SymbolEqualityComparer.Default));

    private static string Parameters(IMethodSymbol method, HashSet<ITypeSymbol> recordPath) =>
        string.Join(", ", method.Parameters.Select((parameter, index) =>
            $"arg{index}{(parameter.HasExplicitDefaultValue ? "?" : "")}: {Map(parameter.Type, recordPath)}"));

    private static string ReturnType(IMethodSymbol method, HashSet<ITypeSymbol> recordPath)
    {
        var result = Map(method.ReturnType, recordPath);
        var writable = method.Parameters.Where(parameter => parameter.RefKind is RefKind.Ref or RefKind.Out)
            .Select(parameter => Map(parameter.Type, recordPath)).ToList();
        if (writable.Count == 0)
            return result;
        if (!method.ReturnsVoid)
            writable.Insert(0, result);
        var protocol = "[" + string.Join(", ", writable) + "]";
        return method.IsAsync ? $"Promise<{protocol}>" : protocol;
    }

    private static string Map(ITypeSymbol? symbol, HashSet<ITypeSymbol> recordPath)
    {
        var mapped = symbol switch
        {
            ITypeParameterSymbol parameter => TypeParameterName(parameter),
            IArrayTypeSymbol { Rank: 1 } array => $"Array<{Map(array.ElementType, recordPath)}>",
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable =>
                Map(nullable.TypeArguments[0], recordPath) + " | null",
            INamedTypeSymbol { DelegateInvokeMethod: { } method } => $"(({Parameters(method, recordPath)}) => {ReturnType(method, recordPath)})",
            INamedTypeSymbol { IsTupleType: true } tuple => "{ " + string.Join("; ", tuple.TupleElements
                .Select(element => $"{JsonSerializer.Serialize(Util.GetConfigOrSymbolName(element))}: {Map(element.Type, recordPath)}")) + " }",
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumeration => Util.IsStringEnumType(enumeration)
                ? "string" : Map(enumeration.EnumUnderlyingType, recordPath),
            INamedTypeSymbol { IsRecord: true } record => MapRecord(record, recordPath),
            INamedTypeSymbol named => MapNamed(named, recordPath),
            _ => "unknown",
        };
        return symbol?.NullableAnnotation == NullableAnnotation.Annotated && mapped != "unknown" && !mapped.EndsWith(" | null", StringComparison.Ordinal)
            ? mapped + " | null"
            : mapped;
    }

    private static string MapNamed(INamedTypeSymbol type, HashSet<ITypeSymbol> recordPath) => type.SpecialType switch
    {
        SpecialType.System_String or SpecialType.System_Char => "string",
        SpecialType.System_Boolean => "boolean",
        SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or
            SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Single or SpecialType.System_Double or
            SpecialType.System_Decimal => "number",
        SpecialType.System_Int64 or SpecialType.System_UInt64 => "bigint",
        SpecialType.System_Void => "void",
        _ => type.OriginalDefinition.ToDisplayString() switch
        {
            "JoltAuthoring.RenderedFragment" => "VNodeChild",
            "System.Half" or "ECMAScript.Number" => "number",
            "System.Int128" or "System.UInt128" or "System.Numerics.BigInteger" or "ECMAScript.BigInt" => "bigint",
            "System.Threading.Tasks.Task" or "System.Threading.Tasks.ValueTask" => "Promise<void>",
            "System.Threading.Tasks.Task<TResult>" => $"Promise<{Map(type.TypeArguments[0], recordPath)}>",
            "System.Collections.Generic.List<T>" or "System.Collections.Generic.IList<T>" or "System.Collections.Generic.ICollection<T>" =>
                $"Array<{Map(type.TypeArguments[0], recordPath)}>",
            "System.Collections.Generic.IReadOnlyList<T>" or "System.Collections.Generic.IReadOnlyCollection<T>" =>
                $"ReadonlyArray<{Map(type.TypeArguments[0], recordPath)}>",
            _ => "unknown",
        },
    };

    private static string MapRecord(INamedTypeSymbol record, HashSet<ITypeSymbol> recordPath)
    {
        // Inline the JavaScript object shape. Stop at a recursive edge, including
        // generic recursion whose type arguments change at each step.
        if (!recordPath.Add(record.OriginalDefinition))
            return "unknown";

        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var current = record; current is { IsRecord: true }; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member.DeclaredAccessibility != Accessibility.Public || !StructuralRecordSupport.IsStructuralRecordMember(member))
                    continue;

                var name = Util.GetConfigOrSymbolName(member);
                switch (member)
                {
                    case IPropertySymbol property when !property.GetAttributes().Any(attribute =>
                        attribute.AttributeClass?.ToDisplayString() == "ECMAScript.SpreadAttribute"):
                        // Static null and omitted constructor defaults may erase the key.
                        var required = property.IsRequired || property.DeclaringSyntaxReferences.Any(reference =>
                            reference.GetSyntax() is ParameterSyntax { Default: null });
                        fields.TryAdd(name, Field(name, property.Type, required, recordPath));
                        break;
                    case IFieldSymbol { IsImplicitlyDeclared: false } field:
                        fields.TryAdd(name, Field(name, field.Type, field.IsRequired, recordPath));
                        break;
                }
            }
        }
        recordPath.Remove(record.OriginalDefinition);
        return "{ " + string.Join("; ", fields.Values) + " }";
    }

    private static string Field(string name, ITypeSymbol type, bool required, HashSet<ITypeSymbol> recordPath)
    {
        var mapped = Map(type, recordPath);
        var optional = !required || mapped.EndsWith(" | null", StringComparison.Ordinal);
        return $"{JsonSerializer.Serialize(name)}{(optional ? "?" : "")}: {mapped}";
    }
}
