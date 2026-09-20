using ECMAScript;
using Microsoft.CodeAnalysis;

namespace Jazor.Common;

/// <summary>
/// Reads canonical external component binding metadata from Roslyn symbols.
/// 从 Roslyn 符号读取统一的外部组件绑定元数据。
/// </summary>
/// <remarks>
/// 组件身份由约定判定（派生 <c>ComponentBase</c> 且实现 Vue marker），不由特性表达。
/// 因此这里只读取绑定本身：<c>[ECMAScript("specifier")]</c> 的 import specifier。
/// 导出名由名字机制给出，调用方通过 <c>Jazor.Compiler.Util.GetConfigOrSymbolName</c> 解析。
/// </remarks>
public static class ECMAScriptComponentMetadata
{
    private const string AttributeMetadataName = "ECMAScript.ECMAScriptAttribute";

    public readonly record struct ComponentImport(string ImportSpecifier);

    /// <summary>Determines whether one attribute instance is a valid component binding.</summary>
    public static bool IsComponentAttribute(AttributeData attribute)
    {
        if (attribute is null)
            throw new ArgumentNullException(nameof(attribute));

        return TryRead(attribute, out _);
    }

    /// <summary>Reads a canonical component import descriptor.</summary>
    public static bool TryGetComponentImport(AttributeData attribute, out ComponentImport descriptor)
    {
        if (attribute is null)
            throw new ArgumentNullException(nameof(attribute));

        return TryRead(attribute, out descriptor);
    }

    private static bool TryRead(AttributeData attribute, out ComponentImport descriptor)
    {
        descriptor = default;
        if (!string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                AttributeMetadataName,
                StringComparison.Ordinal))
        {
            return false;
        }

        // 无参形式是环境宿主契约，不构成组件绑定。
        if (attribute.ConstructorArguments.Length != 1 ||
            attribute.ConstructorArguments[0].Value is not string importSpecifier ||
            string.IsNullOrWhiteSpace(importSpecifier))
        {
            return false;
        }

        descriptor = new ComponentImport(
            ECMAScriptModulePath.ValidateExternalImportSpecifier(importSpecifier));
        return true;
    }
}
