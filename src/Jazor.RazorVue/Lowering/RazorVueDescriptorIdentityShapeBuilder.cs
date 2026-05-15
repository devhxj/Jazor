using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueDescriptorIdentityShapeBuilder
{
    public static string BuildForRenderTree(
        VueComponentDescriptor ownerDescriptor,
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = CreateBaseDescriptorShape(ownerDescriptor);
        AppendResolvedComponentRuntimeShape(builder, CollectFromRenderTree(renderTree, resolvedComponents));
        return builder.ToString();
    }

    public static string BuildForCanonicalTemplate(
        VueComponentDescriptor ownerDescriptor,
        RazorVueCanonicalTemplateFragment template)
    {
        var builder = CreateBaseDescriptorShape(ownerDescriptor);
        AppendResolvedComponentRuntimeShape(builder, CollectFromCanonicalTemplate(template));
        return builder.ToString();
    }

    private static StringBuilder CreateBaseDescriptorShape(VueComponentDescriptor descriptor)
    {
        var builder = new StringBuilder();
        builder.AppendLine(descriptor.FullName);
        builder.AppendLine(descriptor.SourceKind.ToString());
        builder.AppendLine(descriptor.ImportSpecifier);
        builder.AppendLine(descriptor.ExportName);
        foreach (var routeTemplate in descriptor.RouteTemplates)
            builder.AppendLine("route:" + routeTemplate);
        builder.AppendLine("flags:" + descriptor.Flags);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
        {
            builder.AppendLine(
                prop.PublicName + "|" +
                prop.Name + "|" +
                prop.TypeName + "|" +
                prop.Required + "|" +
                prop.AcceptsBinding + "|" +
                (prop.DefaultExpression ?? string.Empty) + "|" +
                prop.DefaultSource + "|" +
                prop.Kind + "|" +
                prop.CaptureUnmatchedValues);
        }

        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            builder.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);

        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            builder.AppendLine(
                slot.PublicName + "|" +
                slot.Name + "|" +
                (slot.NamePattern ?? string.Empty) + "|" +
                slot.PatternOnly + "|" +
                slot.IsDefault + "|" +
                slot.Required + "|" +
                string.Join(",", slot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
        }

        foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
            builder.AppendLine("plugin:" + pluginRequirement);

        return builder;
    }

    private static void AppendResolvedComponentRuntimeShape(
        StringBuilder builder,
        ImmutableArray<ResolvedComponentRuntimeUsage> referencedComponents)
    {
        foreach (var component in referencedComponents)
        {
            builder.AppendLine("resolved:" + component.ComponentKey);
            builder.AppendLine("resolved:sourceKind=" + component.Descriptor.SourceKind);
            builder.AppendLine("resolved:import=" + component.Descriptor.ImportSpecifier);
            builder.AppendLine("resolved:export=" + component.Descriptor.ExportName);
            builder.AppendLine("resolved:container=" + (component.Descriptor.ContainerContractFullName ?? string.Empty));
            builder.AppendLine("resolved:flags=" + component.Descriptor.Flags);

            foreach (var usedProp in component.UsedProps.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    "resolved:prop:" +
                    usedProp.PublicName + "|" +
                    usedProp.Name + "|" +
                    usedProp.TypeName + "|" +
                    usedProp.Required + "|" +
                    usedProp.AcceptsBinding + "|" +
                    (usedProp.DefaultExpression ?? string.Empty) + "|" +
                    usedProp.DefaultSource + "|" +
                    usedProp.Kind + "|" +
                    usedProp.CaptureUnmatchedValues);
            }

            foreach (var usedEmit in component.UsedEmits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
                builder.AppendLine("resolved:emit:" + (usedEmit.RazorAlias ?? string.Empty) + "|" + usedEmit.Name + "|" + usedEmit.PayloadTypeName + "|" + usedEmit.Kind);

            foreach (var usedSlot in component.UsedSlots.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    "resolved:slot:" +
                    usedSlot.PublicName + "|" +
                    usedSlot.Name + "|" +
                    (usedSlot.NamePattern ?? string.Empty) + "|" +
                    usedSlot.PatternOnly + "|" +
                    usedSlot.IsDefault + "|" +
                    usedSlot.Required + "|" +
                    string.Join(",", usedSlot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
            }

            foreach (var style in component.Descriptor.StyleDependencies.OrderBy(static item => item, StringComparer.Ordinal))
                builder.AppendLine("resolved:style:" + style);
            foreach (var pluginRequirement in component.Descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
                builder.AppendLine("resolved:plugin:" + pluginRequirement);
        }
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> CollectFromRenderTree(
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableArray<ResolvedComponentRuntimeUsage>.Empty;

        var usageByComponent = new Dictionary<string, RuntimeUsageBuilder>(StringComparer.Ordinal);
        CollectFromRenderTree(renderTree, resolvedComponents, usageByComponent);
        return BuildResult(usageByComponent);
    }

    private static void CollectFromRenderTree(
        RazorVueRenderFragment fragment,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            CollectFromRenderTree(child, resolvedComponents, usageByComponent);
    }

    private static void CollectFromRenderTree(
        RazorVueRenderNode node,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        switch (node)
        {
            case RazorVueElementNode element:
                CollectFromRenderTree(element.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueComponentNode component:
                CollectFromRenderTreeComponent(component, resolvedComponents, usageByComponent);
                CollectFromRenderTree(component.Children, resolvedComponents, usageByComponent);
                foreach (var slotTemplate in component.SlotTemplates)
                    CollectFromRenderTree(slotTemplate.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueConditionalNode conditional:
                CollectFromRenderTree(conditional.WhenTrue, resolvedComponents, usageByComponent);
                CollectFromRenderTree(conditional.WhenFalse, resolvedComponents, usageByComponent);
                break;
            case RazorVueForEachNode loop:
                CollectFromRenderTree(loop.Body, resolvedComponents, usageByComponent);
                break;
            case RazorVueForNode loop:
                CollectFromRenderTree(loop.Body, resolvedComponents, usageByComponent);
                break;
        }
    }

    private static void CollectFromRenderTreeComponent(
        RazorVueComponentNode component,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (!resolvedComponents.TryGetValue(component.ComponentName, out var descriptor))
            return;

        var usage = GetOrAddUsage(usageByComponent, component.ComponentName, descriptor);
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode)
            {
                usage.MarkAllPropsAndEmits();
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out var slotResolution))
            {
                usage.MarkSlot(slotResolution.Descriptor);
                continue;
            }

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out var propResolution))
            {
                usage.MarkProp(propResolution.Descriptor);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Name) &&
                descriptor.Emits.Any(emit => string.Equals(emit.RazorAlias, attribute.Name, StringComparison.Ordinal)))
            {
                usage.MarkEmitByAlias(attribute.Name);
            }
        }

        if (!component.Children.Children.IsDefaultOrEmpty &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var childContentSlot))
        {
            usage.MarkSlot(childContentSlot.Descriptor);
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slotResolution))
                usage.MarkSlot(slotResolution.Descriptor);
        }
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateFragment template)
    {
        var usageByComponent = new Dictionary<string, RuntimeUsageBuilder>(StringComparer.Ordinal);
        CollectFromCanonicalTemplate(template, usageByComponent);
        return BuildResult(usageByComponent);
    }

    private static void CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateFragment fragment,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            CollectFromCanonicalTemplate(child, usageByComponent);
    }

    private static void CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateNode node,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        switch (node)
        {
            case RazorVueCanonicalElementNode element:
                CollectFromCanonicalTemplate(element.Children, usageByComponent);
                break;
            case RazorVueCanonicalComponentNode component:
                CollectFromCanonicalComponent(component, usageByComponent);
                CollectFromCanonicalTemplate(component.Children, usageByComponent);
                foreach (var slot in component.Slots)
                    CollectFromCanonicalTemplate(slot.Children, usageByComponent);
                break;
            case RazorVueCanonicalConditionalNode conditional:
                CollectFromCanonicalTemplate(conditional.WhenTrue, usageByComponent);
                CollectFromCanonicalTemplate(conditional.WhenFalse, usageByComponent);
                break;
            case RazorVueCanonicalForEachNode loop:
                CollectFromCanonicalTemplate(loop.Body, usageByComponent);
                break;
            case RazorVueCanonicalForNode loop:
                CollectFromCanonicalTemplate(loop.Body, usageByComponent);
                break;
        }
    }

    private static void CollectFromCanonicalComponent(
        RazorVueCanonicalComponentNode component,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (component.ResolvedDescriptor is null)
            return;

        var descriptor = component.ResolvedDescriptor;
        var usage = GetOrAddUsage(usageByComponent, component.ComponentFullName, descriptor);

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueCanonicalAttributeSpreadBinding)
            {
                usage.MarkAllPropsAndEmits();
                continue;
            }

            var attribute = (RazorVueCanonicalAttributeBinding)attributeEntry;
            switch (attribute.AttributeKind)
            {
                case RazorVueCanonicalAttributeKind.ComponentProp:
                    if (TryResolvePropByRuntimeName(descriptor, attribute.Name, out var prop))
                        usage.MarkProp(prop);
                    break;
                case RazorVueCanonicalAttributeKind.ComponentEvent:
                    if (TryResolveEmitByRuntimeName(descriptor, attribute.Name, out var emit))
                        usage.MarkEmit(emit);
                    break;
            }
        }

        foreach (var slot in component.Slots)
        {
            if (TryResolveSlotByRuntimeName(descriptor, slot.SlotName, out var resolvedSlot))
                usage.MarkSlot(resolvedSlot);
        }
    }

    private static RuntimeUsageBuilder GetOrAddUsage(
        Dictionary<string, RuntimeUsageBuilder> usageByComponent,
        string componentKey,
        VueComponentDescriptor descriptor)
    {
        if (usageByComponent.TryGetValue(componentKey, out var usage))
            return usage;

        usage = new RuntimeUsageBuilder(componentKey, descriptor);
        usageByComponent.Add(componentKey, usage);
        return usage;
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> BuildResult(Dictionary<string, RuntimeUsageBuilder> usageByComponent)
        => usageByComponent
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => pair.Value.Build())
            .ToImmutableArray();

    private static bool TryResolvePropByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VuePropDescriptor prop)
    {
        foreach (var candidate in descriptor.Props)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                prop = candidate;
                return true;
            }
        }

        prop = default!;
        return false;
    }

    private static bool TryResolveEmitByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VueEmitDescriptor emit)
    {
        foreach (var candidate in descriptor.Emits)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                emit = candidate;
                return true;
            }
        }

        emit = default!;
        return false;
    }

    private static bool TryResolveSlotByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VueSlotDescriptor slot)
    {
        foreach (var candidate in descriptor.Slots)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                slot = candidate;
                return true;
            }
        }

        if (VueSlotResolver.TryResolve(descriptor.Slots, runtimeName, out var resolution))
        {
            slot = resolution.Descriptor;
            return true;
        }

        slot = default!;
        return false;
    }

    private sealed record ResolvedComponentRuntimeUsage(
        string ComponentKey,
        VueComponentDescriptor Descriptor,
        ImmutableArray<VuePropDescriptor> UsedProps,
        ImmutableArray<VueEmitDescriptor> UsedEmits,
        ImmutableArray<VueSlotDescriptor> UsedSlots);

    private sealed class RuntimeUsageBuilder
    {
        private readonly Dictionary<string, VuePropDescriptor> _props = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueEmitDescriptor> _emits = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueSlotDescriptor> _slots = new(StringComparer.Ordinal);

        public RuntimeUsageBuilder(string componentKey, VueComponentDescriptor descriptor)
        {
            ComponentKey = componentKey;
            Descriptor = descriptor;
        }

        public string ComponentKey { get; }

        public VueComponentDescriptor Descriptor { get; }

        public void MarkAllPropsAndEmits()
        {
            foreach (var prop in Descriptor.Props)
                MarkProp(prop);
            foreach (var emit in Descriptor.Emits)
                MarkEmit(emit);
        }

        public void MarkProp(VuePropDescriptor descriptor)
            => _props[descriptor.PublicName] = descriptor;

        public void MarkEmitByAlias(string razorAlias)
        {
            foreach (var emit in Descriptor.Emits)
            {
                if (string.Equals(emit.RazorAlias, razorAlias, StringComparison.Ordinal))
                    MarkEmit(emit);
            }
        }

        public void MarkEmit(VueEmitDescriptor descriptor)
            => _emits[descriptor.RazorAlias ?? descriptor.Name] = descriptor;

        public void MarkSlot(VueSlotDescriptor descriptor)
            => _slots[descriptor.PublicName] = descriptor;

        public ResolvedComponentRuntimeUsage Build()
            => new(
                ComponentKey,
                Descriptor,
                _props.Values.ToImmutableArray(),
                _emits.Values.ToImmutableArray(),
                _slots.Values.ToImmutableArray());
    }
}
