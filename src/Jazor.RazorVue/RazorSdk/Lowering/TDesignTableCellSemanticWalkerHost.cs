using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Claims only TDesign table-cell union conversions whose value is an inline generic
/// RenderFragment. The nested builder protocol is then delegated to RenderEmitter while every
/// other operation remains on SemanticWalker's ordinary C# lowering path.
/// 这是 TDesign callback 协议的产品边界，不是通用 union 或 RenderFragment fallback。
/// </summary>
internal sealed class TDesignTableCellSemanticWalkerHost(
    Compilation compilation,
    INamedTypeSymbol componentSymbol,
    IReadOnlyDictionary<ISymbol, string>? declaredNames,
    VueInjectRegistry injectRegistry,
    VueRenderRuntimeFeatures runtimeFeatures) : SemanticWalkerHost
{
    public override Expression? RewriteConversionPreorder(
        IConversionOperation operation,
        SenseArgument argument)
        => RenderEmitter.TryEmitTDesignTableCell(
            compilation,
            componentSymbol,
            declaredNames,
            injectRegistry,
            operation,
            argument,
            runtimeFeatures);
}
