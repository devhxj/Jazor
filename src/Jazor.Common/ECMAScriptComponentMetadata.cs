using ECMAScript;
using Microsoft.CodeAnalysis;

namespace Jazor.Common;

/// <summary>
/// Reads canonical external component binding metadata from Roslyn symbols.
/// 从 Roslyn 符号读取统一的外部组件绑定元数据。
/// </summary>
public static class ECMAScriptComponentMetadata
{
    private const string AttributeMetadataName = "ECMAScript.ECMAScriptAttribute";

    public readonly record struct ComponentImport(string ImportSpecifier, string? ExportName);

    /// <summary>Determines whether one attribute instance is a valid component binding.</summary>
    public static bool IsComponentAttribute(AttributeData attribute)
    {
        if (attribute is null)
            throw new ArgumentNullException(nameof(attribute));

        return IsCanonicalAttribute(attribute) && TryRead(attribute, out _);
    }

    /// <summary>Reads a canonical component import descriptor.</summary>
    public static bool TryGetComponentImport(AttributeData attribute, out ComponentImport descriptor)
    {
        if (attribute is null)
            throw new ArgumentNullException(nameof(attribute));

        if (IsCanonicalAttribute(attribute) && TryRead(attribute, out descriptor))
            return true;

        descriptor = default;
        return false;
    }

    private static bool IsCanonicalAttribute(AttributeData attribute)
        => string.Equals(
            attribute.AttributeClass?.ToDisplayString(),
            AttributeMetadataName,
            StringComparison.Ordinal);

    private static bool TryRead(AttributeData attribute, out ComponentImport descriptor)
    {
        descriptor = default;
        if (attribute.ConstructorArguments.Length < 2 ||
            attribute.ConstructorArguments[0].Value is not string importSpecifier ||
            attribute.ConstructorArguments[1].Value is not int transform ||
            transform != (int)Transform.Component ||
            string.IsNullOrWhiteSpace(importSpecifier))
        {
            return false;
        }

        string? exportName = null;
        if (attribute.ConstructorArguments.Length >= 3)
        {
            var exportArgument = attribute.ConstructorArguments[2];
            if (exportArgument.Value is not null && exportArgument.Value is not string)
                return false;

            exportName = exportArgument.Value as string;
            if (exportName is not null && string.IsNullOrWhiteSpace(exportName))
                return false;
        }

        descriptor = new ComponentImport(
            ECMAScriptModulePath.ValidateExternalImportSpecifier(importSpecifier),
            exportName?.Trim());
        return true;
    }
}
