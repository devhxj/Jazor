using ECMAScript.Contract;
using Microsoft.CodeAnalysis;

namespace Jazor.Common;

/// <summary>
/// Identifies framework-neutral external library component metadata on Roslyn symbols.
/// 从 Roslyn 符号识别框架中性的外部库组件元数据。
/// </summary>
/// <remarks>
/// <para>
/// The shared rule intentionally recognizes <see cref="LibraryComponentAttribute"/> and
/// framework-specific attributes derived from it. Shared analysis can use this helper without
/// taking a dependency on Vue, React, or another framework; each adapter still owns its own
/// import interpretation and rendering protocol.
/// </para>
/// <para>
/// 共享规则有意识别 <see cref="LibraryComponentAttribute"/> 及其框架专属派生特性。
/// 共享分析可以通过此 helper 完成识别，而无需依赖 Vue、React 或其他框架；每个适配器
/// 仍独立拥有 import 解释和渲染协议。
/// </para>
/// </remarks>
public static class LibraryComponentMetadata
{
    private static readonly string AttributeMetadataName = typeof(LibraryComponentAttribute).FullName!;

    /// <summary>
    /// Determines whether an attribute type is the neutral library component contract or a
    /// framework-specific attribute derived from it.
    /// 判断特性类型是否为中性库组件契约或其框架专属派生特性。
    /// </summary>
    /// <param name="attributeType">The candidate attribute type. 候选特性类型。</param>
    /// <returns><see langword="true"/> when the type participates in the neutral component import contract; otherwise <see langword="false"/>.</returns>
    public static bool IsLibraryComponentAttribute(INamedTypeSymbol? attributeType)
    {
        for (INamedTypeSymbol? current = attributeType; current is not null; current = current.BaseType)
        {
            if (string.Equals(
                    current.ToDisplayString(),
                    AttributeMetadataName,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
