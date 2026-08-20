using System.Collections.Generic;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Composes the current-component and Vue child-slot projections over the product-neutral
/// compiler host contract. RenderTreeBuilder itself is owned by the direct render emitter.
/// 这里不再把 builder 协议交给通用 walker；direct emitter 是唯一的 RenderTree-to-h 边界。
/// </summary>
internal sealed class VueSemanticWalkerHost : CompositeSemanticWalkerHost
{
    public VueSemanticWalkerHost(
        INamedTypeSymbol componentType,
        string stateIdentifier = "state",
        string propsIdentifier = "props",
        IReadOnlyDictionary<string, string>? parameterRuntimeNames = null,
        IReadOnlyDictionary<ISymbol, string>? memberRuntimeNames = null,
        bool parameterPropertiesUseState = false,
        Func<IParameterReferenceOperation, SenseArgument, Expression?>? parameterReferenceRewriter = null,
        Func<ILocalReferenceOperation, SenseArgument, Expression?>? localReferenceRewriter = null,
        Func<IPropertyReferenceOperation, SenseArgument, Expression?>? propertyReferenceRewriter = null,
        Action<Expression, DirectBinderValueKind>? directBinderHandlerObserver = null,
        SemanticWalkerHost? tableCellHost = null)
        : base(BuildHosts(
            componentType,
            stateIdentifier,
            propsIdentifier,
            parameterRuntimeNames,
            memberRuntimeNames,
            parameterPropertiesUseState,
            parameterReferenceRewriter,
            localReferenceRewriter,
            propertyReferenceRewriter,
            directBinderHandlerObserver,
            tableCellHost))
    {
    }

    private static SemanticWalkerHost[] BuildHosts(
        INamedTypeSymbol componentType,
        string stateIdentifier,
        string propsIdentifier,
        IReadOnlyDictionary<string, string>? parameterRuntimeNames,
        IReadOnlyDictionary<ISymbol, string>? memberRuntimeNames,
        bool parameterPropertiesUseState,
        Func<IParameterReferenceOperation, SenseArgument, Expression?>? parameterReferenceRewriter,
        Func<ILocalReferenceOperation, SenseArgument, Expression?>? localReferenceRewriter,
        Func<IPropertyReferenceOperation, SenseArgument, Expression?>? propertyReferenceRewriter,
        Action<Expression, DirectBinderValueKind>? directBinderHandlerObserver,
        SemanticWalkerHost? tableCellHost)
    {
        var hosts = new List<SemanticWalkerHost>();
        if (tableCellHost is not null)
            hosts.Add(tableCellHost);
        hosts.Add(new CurrentComponentSemanticWalkerHost(
            componentType,
            stateIdentifier,
            propsIdentifier,
            parameterRuntimeNames,
            memberRuntimeNames,
            parameterPropertiesUseState,
            parameterReferenceRewriter,
            localReferenceRewriter,
            propertyReferenceRewriter,
            directBinderHandlerObserver));
        hosts.Add(ChildrenToSlotSemanticWalkerHost.Instance);
        return hosts.ToArray();
    }
}
