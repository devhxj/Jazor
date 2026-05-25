using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory
{
    internal static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponentsForCanonicalization(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
        => ResolveComponents(context, snapshot, renderTree);

    private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
    {
        var components = CollectComponents(snapshot, renderTree);
        if (components.Count == 0)
            return ImmutableDictionary<string, VueComponentDescriptor>.Empty;

        var registry = context.CreateComponentRegistry();
        var injectRegistry = VueInjectRegistry.Resolve(context);
        var resolutionContext = new VueComponentResolutionContext(
            snapshot.Descriptor.ResolutionNamespace,
            snapshot.ImportedNamespaces);
        var builder = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);

        foreach (var component in components)
        {
            var result = ResolveComponentDescriptor(registry, resolutionContext, component);
            if (result.Status != VueComponentResolutionStatus.Resolved || result.Descriptor is null)
                throw CreateResolutionIssueException(result, snapshot.Descriptor.FullName, component);

            builder[component.ComponentName] = injectRegistry.ResolveImplementation(
                result.Descriptor,
                registry,
                component.Origins.IsDefaultOrEmpty ? null : component.Origins[0]);
        }

        return builder.ToImmutable();
    }

    private static VueComponentResolutionResult ResolveComponentDescriptor(
        VueComponentRegistry registry,
        VueComponentResolutionContext resolutionContext,
        RazorVueComponentNode component)
    {
        var resolutionName = string.IsNullOrWhiteSpace(component.ResolutionName)
            ? component.ComponentName
            : component.ResolutionName;
        resolutionName = NormalizeResolutionName(resolutionName);

        return registry.Resolve(resolutionName, resolutionContext);
    }

    private static string NormalizeResolutionName(string resolutionName)
    {
        if (string.IsNullOrWhiteSpace(resolutionName))
            return resolutionName;

        return resolutionName.StartsWith("global::", StringComparison.Ordinal)
            ? resolutionName.Substring("global::".Length)
            : resolutionName;
    }

    private static RazorVueCompilationIssueException CreateResolutionIssueException(
        VueComponentResolutionResult resolutionResult,
        string ownerComponentFullName,
        RazorVueComponentNode component)
    {
        var issue = resolutionResult.Issues.IsDefaultOrEmpty
            ? new RazorVueCompilationIssue(
                RazorVueIssueCode.ComponentNotFound,
                RazorVueIssueSeverity.Error,
                $"Component '{GetMissingComponentDisplayName(resolutionResult, component)}' is not visible in the current RazorVue resolution scope.",
                ImmutableArray<string>.Empty)
            : resolutionResult.Status == VueComponentResolutionStatus.NotFound
                ? new RazorVueCompilationIssue(
                    RazorVueIssueCode.ComponentNotFound,
                    RazorVueIssueSeverity.Error,
                    $"Component '{GetMissingComponentDisplayName(resolutionResult, component)}' is not visible in the current RazorVue resolution scope.",
                    ImmutableArray<string>.Empty)
                : resolutionResult.Issues[0];
        var origin = component.Origins.IsDefaultOrEmpty ? null : component.Origins[0];
        return new RazorVueCompilationIssueException(issue, ownerComponentFullName, origin);
    }

    private static string GetMissingComponentDisplayName(
        VueComponentResolutionResult resolutionResult,
        RazorVueComponentNode component)
        => string.IsNullOrWhiteSpace(component.ComponentFullName)
            ? resolutionResult.ComponentName
            : component.ComponentFullName;

    private static ImmutableDictionary<string, string> BuildComponentReferences(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableDictionary<string, string>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var item in resolvedComponents)
        {
            if (string.Equals(item.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
                builder[item.Key] = item.Value.ExportName;
            else
                builder[item.Key] = CreateComponentAlias(item.Key);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, ImmutableDictionary<string, string>> BuildComponentEmitsByRazorAlias(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var item in resolvedComponents)
        {
            var emitsBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
            foreach (var emit in item.Value.Emits)
            {
                if (!string.IsNullOrWhiteSpace(emit.RazorAlias))
                    emitsBuilder[emit.RazorAlias!] = ToVueEventHandlerName(emit.Name);
            }

            builder[item.Key] = emitsBuilder.ToImmutable();
        }

        return builder.ToImmutable();
    }

    private static string ToVueEventHandlerName(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return "on";

        if (IsVueEventHandlerName(eventName))
            return eventName;

        return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }

    private static bool IsVueEventHandlerName(string eventName)
    {
        if (!eventName.StartsWith("on", StringComparison.Ordinal) || eventName.Length <= 2)
            return false;

        var marker = eventName[2];
        return char.IsUpper(marker) || marker == ':';
    }


    private static HashSet<RazorVueComponentNode> CollectComponents(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment fragment)
    {
        var result = new HashSet<RazorVueComponentNode>();
        if (!fragment.Children.IsDefaultOrEmpty)
        {
            foreach (var child in fragment.Children)
                CollectComponents(snapshot, child, result);
        }
        return result;
    }

    private static void CollectComponents(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderNode node,
        HashSet<RazorVueComponentNode> components)
    {
        switch (node)
        {
            case RazorVueComponentNode component:
                components.Add(component);
                foreach (var slotTemplate in component.SlotTemplates)
                {
                    foreach (var child in slotTemplate.Children.Children)
                        CollectComponents(snapshot, child, components);
                }
                foreach (var child in component.Children.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueElementNode element:
                foreach (var child in element.Children.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueConditionalNode conditional:
                foreach (var child in conditional.WhenTrue.Children)
                    CollectComponents(snapshot, child, components);
                foreach (var child in conditional.WhenFalse.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueTemplateScopeNode templateScope:
                foreach (var child in templateScope.Children.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueForEachNode loop:
                foreach (var child in loop.Body.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueForNode loop:
                foreach (var child in loop.Body.Children)
                    CollectComponents(snapshot, child, components);
                break;
            case RazorVueImperativeBlockNode imperative:
                foreach (var operation in imperative.Operations)
                    CollectComponents(operation, snapshot.Compilation, snapshot.ComponentSymbol, imperative.Origins, components);
                break;
            case RazorVueLocalDeclarationNode:
                break;
        }
    }

    private static void CollectComponents(
        IOperation operation,
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        ImmutableArray<RazorVueSourceOrigin> origins,
        HashSet<RazorVueComponentNode> components)
    {
        if (RazorVueImperativeRenderFragmentCarrierHelper.TryEnumerateNestedImperativeRenderFragmentBodies(
                compilation,
                componentSymbol,
                operation,
                RazorVueOperationNormalizer.Unwrap,
                IsSourceStableMutableCarrierMember,
                out var nestedBodies))
        {
            foreach (var body in nestedBodies)
            {
                foreach (var nestedOperation in EnumerateOperationAndDescendants(body))
                {
                    if (nestedOperation is IInvocationOperation nestedInvocation &&
                        IsOpenComponentInvocation(nestedInvocation, componentSymbol, out var nestedComponentType, out var nestedResolutionName))
                    {
                        components.Add(new RazorVueComponentNode(
                            nestedComponentType.Name,
                            nestedComponentType.ToDisplayString(),
                            nestedResolutionName,
                            Key: null,
                            Attributes: ImmutableArray<RazorVueAttributeEntry>.Empty,
                            SlotTemplates: ImmutableArray<RazorVueComponentSlotTemplateNode>.Empty,
                            ImplicitDefaultSlotAssignments: ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode>.Empty,
                            AmbientDefaultSlotChildren: RazorVueRenderFragment.Empty,
                            Children: RazorVueRenderFragment.Empty,
                            Origins: origins));
                    }
                }
            }
        }

        foreach (var current in EnumerateOperationAndDescendants(operation))
        {
            if (current is not IInvocationOperation invocation)
                continue;

            if (!IsOpenComponentInvocation(invocation, componentSymbol, out var componentType, out var resolutionName))
                continue;

            components.Add(new RazorVueComponentNode(
                componentType.Name,
                componentType.ToDisplayString(),
                resolutionName,
                Key: null,
                Attributes: ImmutableArray<RazorVueAttributeEntry>.Empty,
                SlotTemplates: ImmutableArray<RazorVueComponentSlotTemplateNode>.Empty,
                ImplicitDefaultSlotAssignments: ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode>.Empty,
                AmbientDefaultSlotChildren: RazorVueRenderFragment.Empty,
                Children: RazorVueRenderFragment.Empty,
                Origins: origins));
        }
    }

    private static bool IsSourceStableMutableCarrierMember(Compilation compilation, ISymbol member)
        => RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(member) &&
           !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, member);

    private static bool IsOpenComponentInvocation(
        IInvocationOperation invocation,
        INamedTypeSymbol componentSymbol,
        out INamedTypeSymbol componentType,
        out string resolutionName)
    {
        componentType = default!;
        resolutionName = string.Empty;

        if (!string.Equals(invocation.TargetMethod.Name, "OpenComponent", StringComparison.Ordinal))
            return false;

        if (invocation.TargetMethod.TypeArguments.Length == 1 &&
            invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
        {
            componentType = genericComponentType;
            resolutionName = genericComponentType.ToDisplayString();
            return true;
        }

        if (invocation.Arguments.Length >= 2 &&
            invocation.SemanticModel?.Compilation is { } compilation &&
            RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                compilation,
                componentSymbol,
                invocation.Arguments[1].Value,
                out var explicitComponentType,
                out _))
        {
            componentType = explicitComponentType;
            resolutionName = explicitComponentType.ToDisplayString();
            return true;
        }

        return false;
    }

    private static IEnumerable<IOperation> EnumerateOperationAndDescendants(IOperation operation)
    {
        yield return operation;
        foreach (var child in operation.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateOperationAndDescendants(child))
                yield return nested;
        }
    }

    private static string CreateComponentAlias(string componentName)
        => componentName + "Component";

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";
}
