using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Descriptor;

internal static class RazorVueCascadingValueComponent
{
    private const string MetadataName = "Microsoft.AspNetCore.Components.CascadingValue`1";
    private const string DisplayName = "Microsoft.AspNetCore.Components.CascadingValue<TValue>";
    private static readonly SymbolDisplayFormat TypeKeyDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    public static bool IsCascadingValueComponent(INamedTypeSymbol? componentType)
    {
        if (componentType is null)
            return false;

        var original = componentType.OriginalDefinition;
        return string.Equals(original.ToDisplayString(), DisplayName, StringComparison.Ordinal) ||
               string.Equals(original.MetadataName, "CascadingValue`1", StringComparison.Ordinal) &&
               string.Equals(original.ContainingNamespace?.ToDisplayString(), "Microsoft.AspNetCore.Components", StringComparison.Ordinal);
    }

    public static bool IsCascadingValueComponentFullName(string? componentFullName)
    {
        if (componentFullName is not { } value || string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim();
        return normalized.StartsWith("Microsoft.AspNetCore.Components.CascadingValue<", StringComparison.Ordinal) ||
               string.Equals(normalized, "Microsoft.AspNetCore.Components.CascadingValue<TValue>", StringComparison.Ordinal);
    }

    public static INamedTypeSymbol? GetDefinition(Compilation compilation)
        => compilation.GetTypeByMetadataName(MetadataName);

    public static string GetTypeKey(INamedTypeSymbol componentType)
    {
        if (componentType.TypeArguments.Length == 0)
            return "System.Object";

        return componentType.TypeArguments[0]
            .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            .ToDisplayString(TypeKeyDisplayFormat);
    }

    public static string GetTypeKey(string componentFullName)
    {
        var value = ExtractTypeArgument(componentFullName);
        return NormalizeTypeKey(value);
    }

    private static string ExtractTypeArgument(string componentFullName)
    {
        const string prefix = "Microsoft.AspNetCore.Components.CascadingValue<";
        if (!componentFullName.StartsWith(prefix, StringComparison.Ordinal) ||
            !componentFullName.EndsWith(">", StringComparison.Ordinal))
        {
            return componentFullName;
        }

        return componentFullName.Substring(prefix.Length, componentFullName.Length - prefix.Length - 1);
    }

    private static string NormalizeTypeKey(string typeName)
        => typeName.Trim().TrimEnd('?') switch
        {
            "string" => "System.String",
            "bool" => "System.Boolean",
            "byte" => "System.Byte",
            "sbyte" => "System.SByte",
            "short" => "System.Int16",
            "ushort" => "System.UInt16",
            "int" => "System.Int32",
            "uint" => "System.UInt32",
            "long" => "System.Int64",
            "ulong" => "System.UInt64",
            "float" => "System.Single",
            "double" => "System.Double",
            "decimal" => "System.Decimal",
            "object" => "System.Object",
            var normalized => normalized
        };
}
