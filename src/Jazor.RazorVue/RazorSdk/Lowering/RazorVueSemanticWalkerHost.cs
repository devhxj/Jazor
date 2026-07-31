using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Composes the Razor component, RenderTreeBuilder, and Vue child-slot projections over the
/// product-neutral compiler host contract.
/// </summary>
internal sealed class RazorVueSemanticWalkerHost : CompositeSemanticWalkerHost
{
    public RazorVueSemanticWalkerHost(
        INamedTypeSymbol componentType,
        string stateIdentifier = "state",
        string propsIdentifier = "props",
        IReadOnlyDictionary<string, string>? parameterRuntimeNames = null,
        IReadOnlyDictionary<ISymbol, string>? memberRuntimeNames = null,
        Func<IParameterReferenceOperation, SenseArgument, Expression?>? parameterReferenceRewriter = null,
        Func<ILocalReferenceOperation, SenseArgument, Expression?>? localReferenceRewriter = null)
        : base(
            new CurrentComponentSemanticWalkerHost(
                componentType,
                stateIdentifier,
                propsIdentifier,
                parameterRuntimeNames,
                memberRuntimeNames,
                parameterReferenceRewriter,
                localReferenceRewriter),
            new RenderTreeBuilderSemanticWalkerHost(),
            ChildrenToSlotSemanticWalkerHost.Instance)
    {
    }
}
