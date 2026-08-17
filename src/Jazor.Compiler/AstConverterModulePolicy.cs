// File: AstConverterModulePolicy.cs
// Purpose: Provides the extension contract for module shape and declaration naming.
// 产品层通过此策略投影模块结构，而非继承或修改 AstConverter 的通用 C# lowering。
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// Defines product-neutral module projection decisions used by <see cref="AstConverter"/>.
/// </summary>
/// <remarks>
/// Compiler profiles describe compiler-owned output modes. Product integrations should extend
/// module membership, declaration naming, and runtime-class placement through this policy instead
/// of adding product-specific profile values or branches to <see cref="AstConverter"/>.
/// </remarks>
public abstract class AstConverterModulePolicy
{
    public static AstConverterModulePolicy Default { get; } = new DefaultModulePolicy();

    /// <summary>
    /// Returns the source types whose members and lexical declarations participate in one module.
    /// The sequence order is the deterministic declaration order used by the converter.
    /// </summary>
    public virtual IEnumerable<INamedTypeSymbol> EnumerateModuleTypes(INamedTypeSymbol moduleType)
    {
        yield return moduleType;
    }

    /// <summary>
    /// Returns whether a nested runtime class is emitted separately at module scope by the host.
    /// </summary>
    public virtual bool ShouldFlattenNestedRuntimeClass(
        INamedTypeSymbol moduleType,
        INamedTypeSymbol containingRuntimeClass,
        INamedTypeSymbol nestedRuntimeClass)
        => false;

    /// <summary>
    /// Optionally overrides the preferred local declaration name before collision resolution.
    /// </summary>
    public virtual string? GetPreferredModuleDeclaredName(ISymbol symbol)
        => null;

    /// <summary>
    /// Returns whether a projected module member should receive a named ES module export.
    /// The declaration itself is still emitted when it participates in the module; this hook
    /// only controls the public export surface. The default preserves the historical behavior
    /// of exporting every non-private projected member.
    /// </summary>
    public virtual bool ShouldExportModuleMember(
        INamedTypeSymbol moduleType,
        ISymbol member)
        => true;

    /// <summary>
    /// Allows a host to include an additional top-level accessibility in its projected module.
    /// </summary>
    public virtual bool IsAdditionalTopLevelAccessibilityAllowed(Accessibility accessibility)
        => false;

    private sealed class DefaultModulePolicy : AstConverterModulePolicy
    {
    }
}
