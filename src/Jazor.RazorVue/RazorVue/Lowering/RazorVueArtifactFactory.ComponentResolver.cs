using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory
{
    private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
    {
        var components = CollectComponents(renderTree);
        if (components.Count == 0)
            return ImmutableDictionary<string, VueComponentDescriptor>.Empty;

        var registry = context.CreateComponentRegistry();
        var resolutionContext = new VueComponentResolutionContext(
            snapshot.Descriptor.ResolutionNamespace,
            snapshot.ImportedNamespaces);
        var builder = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);

        foreach (var component in components)
        {
            var result = ResolveComponentDescriptor(registry, resolutionContext, component);
            if (result.Status != VueComponentResolutionStatus.Resolved || result.Descriptor is null)
                throw CreateResolutionIssueException(result, snapshot.Descriptor.FullName, component);

            builder[component.ComponentName] = result.Descriptor;
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

        return registry.Resolve(resolutionName, resolutionContext);
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


    private static HashSet<RazorVueComponentNode> CollectComponents(RazorVueRenderFragment fragment)
    {
        var result = new HashSet<RazorVueComponentNode>();
        foreach (var child in fragment.Children)
            CollectComponents(child, result);
        return result;
    }

    private static void CollectComponents(RazorVueRenderNode node, HashSet<RazorVueComponentNode> components)
    {
        switch (node)
        {
            case RazorVueComponentNode component:
                components.Add(component);
                foreach (var child in component.Children.Children)
                    CollectComponents(child, components);
                break;
            case RazorVueElementNode element:
                foreach (var child in element.Children.Children)
                    CollectComponents(child, components);
                break;
            case RazorVueConditionalNode conditional:
                foreach (var child in conditional.WhenTrue.Children)
                    CollectComponents(child, components);
                foreach (var child in conditional.WhenFalse.Children)
                    CollectComponents(child, components);
                break;
            case RazorVueForEachNode loop:
                foreach (var child in loop.Body.Children)
                    CollectComponents(child, components);
                break;
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
