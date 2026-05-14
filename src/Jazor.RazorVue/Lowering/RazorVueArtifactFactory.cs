using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory : IRazorVueArtifactLowerer
{
    private static readonly ImmutableHashSet<string> SafeLifecycleMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "OnInitialized",
        "OnInitializedAsync",
        "OnParametersSet",
        "OnParametersSetAsync",
        "OnAfterRender",
        "OnAfterRenderAsync");

    private static readonly ImmutableHashSet<string> RiskyLifecycleMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "Dispose",
        "DisposeAsync",
        "SetParametersAsync",
        "ShouldRender");

    private readonly IRazorVueTemplateFrontend _templateFrontend;

    public RazorVueArtifactFactory(IRazorVueTemplateFrontend templateFrontend)
    {
        _templateFrontend = templateFrontend ?? throw new ArgumentNullException(nameof(templateFrontend));
    }

    internal static RazorVueExpressionEmitter CreateExpressionEmitterForCanonicalization(
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var componentReferences = BuildComponentReferences(resolvedComponents);
        var componentEmitsByRazorAlias = BuildComponentEmitsByRazorAlias(resolvedComponents);
        return new RazorVueExpressionEmitter(
            snapshot,
            componentReferences,
            resolvedComponents,
            componentEmitsByRazorAlias);
    }

    public VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var renderTree = _templateFrontend.CreateRenderTree(context, snapshot);
        return CreateCore(context, snapshot, renderTree);
    }

    public VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        return CreateCore(context: null, snapshot, RazorVueRenderFragment.Empty);
    }

    private static VueCompiledArtifact CreateCore(
        RazorVueCompilationContext? context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
    {
        var descriptor = snapshot.Descriptor;
        var relativeModulePath = NormalizeRelativePath(descriptor.ImportSpecifier);
        var resolvedComponents = context is null
            ? ImmutableDictionary<string, VueComponentDescriptor>.Empty
            : ResolveComponents(context, snapshot, renderTree);
        var componentReferences = BuildComponentReferences(resolvedComponents);
        var componentEmitsByRazorAlias = BuildComponentEmitsByRazorAlias(resolvedComponents);
        var expressionEmitter = new RazorVueExpressionEmitter(
            snapshot,
            componentReferences,
            resolvedComponents,
            componentEmitsByRazorAlias);
        var moduleCode = BuildModuleCode(snapshot, renderTree, expressionEmitter, resolvedComponents, out var compilerImports);
        var sourceOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree));

        return new VueCompiledArtifact(
            ComponentName: descriptor.Name,
            RelativeModulePath: relativeModulePath,
            ModuleCode: moduleCode,
            RouteTemplates: descriptor.RouteTemplates,
            Imports: BuildImports(resolvedComponents, compilerImports),
            Styles: BuildStyles(descriptor, resolvedComponents),
            PluginRequirements: BuildPluginRequirements(descriptor, resolvedComponents),
            Identity: BuildIdentity(context, snapshot, renderTree, expressionEmitter, relativeModulePath, resolvedComponents),
            Hints: BuildHints(moduleCode),
            SourceOrigins: sourceOrigins);
    }

    private static VueArtifactIdentity BuildIdentity(
        RazorVueCompilationContext? context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        string relativeModulePath,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptor = snapshot.Descriptor;
        var descriptorShape = BuildDescriptorShape(descriptor, renderTree, resolvedComponents);
        var templateShape = expressionEmitter.DescribeFragment(renderTree);
        var logicShape = BuildLogicShape(context, snapshot, renderTree, expressionEmitter);
        var boundaryKind = ClassifyHmrBoundary(renderTree, snapshot);

        return new VueArtifactIdentity(
            ComponentId: descriptor.FullName,
            ModuleId: relativeModulePath,
            DescriptorHash: ComputeSha256Hex(descriptorShape),
            TemplateHash: ComputeSha256Hex(templateShape),
            LogicHash: ComputeSha256Hex(logicShape),
            HmrBoundaryKind: boundaryKind);
    }

    private static string BuildDescriptorShape(
        VueComponentDescriptor descriptor,
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptorShape = new StringBuilder();
        descriptorShape.AppendLine(descriptor.FullName);
        descriptorShape.AppendLine(descriptor.SourceKind.ToString());
        descriptorShape.AppendLine(descriptor.ImportSpecifier);
        descriptorShape.AppendLine(descriptor.ExportName);
        foreach (var routeTemplate in descriptor.RouteTemplates)
            descriptorShape.AppendLine("route:" + routeTemplate);
        // Keep authoring-contract hash inputs aligned with emitted library metadata so
        // override-only descriptor changes still trigger deterministic update planning.
        descriptorShape.AppendLine("flags:" + descriptor.Flags);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            descriptorShape.AppendLine(
                prop.PublicName + "|" +
                prop.Name + "|" +
                prop.TypeName + "|" +
                prop.Required + "|" +
                prop.AcceptsBinding + "|" +
                (prop.DefaultExpression ?? string.Empty) + "|" +
                prop.Kind + "|" +
                prop.CaptureUnmatchedValues);
        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            descriptorShape.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);
        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
            descriptorShape.AppendLine(
                slot.PublicName + "|" +
                slot.Name + "|" +
                (slot.NamePattern ?? string.Empty) + "|" +
                slot.PatternOnly + "|" +
                slot.IsDefault + "|" +
                slot.Required + "|" +
                string.Join(",", slot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
        foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
            descriptorShape.AppendLine("plugin:" + pluginRequirement);
        AppendResolvedComponentRuntimeShape(descriptorShape, renderTree, resolvedComponents);

        return descriptorShape.ToString();
    }

    private static void AppendResolvedComponentRuntimeShape(
        StringBuilder builder,
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return;

        var referencedComponents = CollectReferencedComponents(renderTree, resolvedComponents);
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

    private static ImmutableArray<ResolvedComponentRuntimeUsage> CollectReferencedComponents(
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableArray<ResolvedComponentRuntimeUsage>.Empty;

        var usageByComponent = new Dictionary<string, ReferencedRuntimeSurfaceBuilder>(StringComparer.Ordinal);
        CollectReferencedComponents(renderTree, resolvedComponents, usageByComponent);

        return usageByComponent
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => pair.Value.Build())
            .ToImmutableArray();
    }

    private static void CollectReferencedComponents(
        RazorVueRenderFragment fragment,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, ReferencedRuntimeSurfaceBuilder> usageByComponent)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            CollectReferencedComponents(child, resolvedComponents, usageByComponent);
    }

    private static void CollectReferencedComponents(
        RazorVueRenderNode node,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, ReferencedRuntimeSurfaceBuilder> usageByComponent)
    {
        switch (node)
        {
            case RazorVueElementNode element:
                CollectReferencedComponents(element.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueComponentNode component:
                CollectReferencedComponent(component, resolvedComponents, usageByComponent);
                CollectReferencedComponents(component.Children, resolvedComponents, usageByComponent);
                foreach (var slotTemplate in component.SlotTemplates)
                    CollectReferencedComponents(slotTemplate.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueConditionalNode conditional:
                CollectReferencedComponents(conditional.WhenTrue, resolvedComponents, usageByComponent);
                CollectReferencedComponents(conditional.WhenFalse, resolvedComponents, usageByComponent);
                break;
            case RazorVueForEachNode loop:
                CollectReferencedComponents(loop.Body, resolvedComponents, usageByComponent);
                break;
            case RazorVueForNode loop:
                CollectReferencedComponents(loop.Body, resolvedComponents, usageByComponent);
                break;
        }
    }

    private static void CollectReferencedComponent(
        RazorVueComponentNode component,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, ReferencedRuntimeSurfaceBuilder> usageByComponent)
    {
        if (!resolvedComponents.TryGetValue(component.ComponentName, out var descriptor))
            return;

        if (!usageByComponent.TryGetValue(component.ComponentName, out var usage))
        {
            usage = new ReferencedRuntimeSurfaceBuilder(component.ComponentName, descriptor);
            usageByComponent.Add(component.ComponentName, usage);
        }

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

    private sealed record ResolvedComponentRuntimeUsage(
        string ComponentKey,
        VueComponentDescriptor Descriptor,
        ImmutableArray<VuePropDescriptor> UsedProps,
        ImmutableArray<VueEmitDescriptor> UsedEmits,
        ImmutableArray<VueSlotDescriptor> UsedSlots);

    private sealed class ReferencedRuntimeSurfaceBuilder
    {
        private readonly Dictionary<string, VuePropDescriptor> _props = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueEmitDescriptor> _emits = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueSlotDescriptor> _slots = new(StringComparer.Ordinal);

        public ReferencedRuntimeSurfaceBuilder(string componentKey, VueComponentDescriptor descriptor)
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

        public void MarkSlot(VueSlotDescriptor descriptor)
            => _slots[descriptor.PublicName] = descriptor;

        public ResolvedComponentRuntimeUsage Build()
            => new(
                ComponentKey,
                Descriptor,
                _props.Values.ToImmutableArray(),
                _emits.Values.ToImmutableArray(),
                _slots.Values.ToImmutableArray());

        private void MarkEmit(VueEmitDescriptor descriptor)
            => _emits[descriptor.RazorAlias ?? descriptor.Name] = descriptor;
    }

    private static string BuildLogicShape(
        RazorVueCompilationContext? context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter)
    {
        _ = context;
        _ = renderTree;
        _ = expressionEmitter;

        var descriptor = snapshot.Descriptor;
        var logicShape = new StringBuilder();
        var onInitializedShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedMethod, false);
        var onInitializedAsyncShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedAsyncMethod, false);
        var onParametersSetShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetMethod, false);
        var onParametersSetAsyncShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetAsyncMethod, false);
        var setParametersAsyncShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeSetParametersAsyncShape(snapshot, snapshot.SetParametersAsyncMethod);
        var onAfterRenderShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderMethod, true);
        var onAfterRenderAsyncShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        var disposeShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeMethod, false);
        var disposeAsyncShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeAsyncMethod, false);
        logicShape.AppendLine("component:" + descriptor.FullName);
        logicShape.AppendLine("module:" + descriptor.ImportSpecifier);
        // LogicHash should reflect emitted runtime behavior. No-op lifecycle methods
        // must not perturb the hash when they do not lower into Vue hooks.
        logicShape.AppendLine("lifecycle:onInitialized=" + onInitializedShape);
        logicShape.AppendLine("lifecycle:onInitializedAsync=" + onInitializedAsyncShape);
        logicShape.AppendLine("lifecycle:onParametersSet=" + onParametersSetShape);
        logicShape.AppendLine("lifecycle:onParametersSetAsync=" + onParametersSetAsyncShape);
        logicShape.AppendLine("lifecycle:setParametersAsync=" + setParametersAsyncShape);
        logicShape.AppendLine("lifecycle:onAfterRender=" + onAfterRenderShape);
        logicShape.AppendLine("lifecycle:onAfterRenderAsync=" + onAfterRenderAsyncShape);
        logicShape.AppendLine("lifecycle:shouldRender=" + RazorVueSetupAndLifecycleLoweringSupport.DescribeShouldRenderShape(snapshot.Compilation, snapshot.ShouldRenderMethod));
        logicShape.AppendLine("lifecycle:dispose=" + disposeShape);
        logicShape.AppendLine("lifecycle:disposeAsync=" + disposeAsyncShape);

        foreach (var field in snapshot.Logic.Fields
                     .OrderBy(static field => field.Name, StringComparer.Ordinal))
        {
            logicShape.AppendLine("field:" + field.Name + "|" + field.IsReadOnly + "|" + DescribeSetupFieldShape(field.FieldSymbol));
        }

        foreach (var method in snapshot.Logic.Methods
                     .OrderBy(static method => method.Name, StringComparer.Ordinal)
                     .ThenBy(static method => method.Arity))
        {
            logicShape.AppendLine("logic:" + method.Name + "|" + method.Arity + "|" + method.IsAsync + "|" + DescribeSetupMethodShape(method.MethodSymbol));
        }

        return logicShape.ToString();
    }

    private static HmrBoundaryKind ClassifyHmrBoundary(
        RazorVueRenderFragment renderTree,
        RazorVueSemanticSnapshot snapshot)
    {
        var descriptor = snapshot.Descriptor;
        if (descriptor.Props.Length == 0 && descriptor.Emits.Length == 0 && descriptor.Slots.Length == 0)
            return HmrBoundaryKind.FullReloadRequired;

        if (HasUnsupportedTemplateNode(renderTree))
            return HmrBoundaryKind.FullReloadRequired;

        if (snapshot.Lifecycle.HasShouldRender &&
            RazorVueSetupAndLifecycleLoweringSupport.DescribeShouldRenderShape(snapshot.Compilation, snapshot.ShouldRenderMethod) == "unsupported")
        {
            return HmrBoundaryKind.FullReloadRequired;
        }

        if (snapshot.Lifecycle.HasSetParametersAsync &&
            RazorVueSetupAndLifecycleLoweringSupport.DescribeSetParametersAsyncShape(snapshot, snapshot.SetParametersAsyncMethod) == "unsupported")
        {
            return HmrBoundaryKind.FullReloadRequired;
        }

        var hasSupportedLifecycleLowering = RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedSetParametersAsyncLowering(snapshot) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                                           RazorVueSetupAndLifecycleLoweringSupport.HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);
        // HMR should only escalate to LogicSafe when lifecycle methods actually lower
        // into runtime hooks; no-op methods should behave like pure template changes.
        if (hasSupportedLifecycleLowering || snapshot.Logic.Fields.Length > 0 || snapshot.Logic.Methods.Length > 0)
            return HmrBoundaryKind.LogicSafe;

        if (HasTemplateShape(renderTree))
            return HmrBoundaryKind.TemplateOnly;

        return HmrBoundaryKind.Unknown;
    }

    private static bool HasTemplateShape(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueElementNode:
                case RazorVueComponentNode:
                case RazorVueTextNode:
                case RazorVueExpressionNode:
                case RazorVueSlotOutletNode:
                    return true;
                case RazorVueUnsupportedTemplateNode:
                    return true;
                case RazorVueConditionalNode conditional:
                    if (HasTemplateShape(conditional.WhenTrue) || HasTemplateShape(conditional.WhenFalse))
                        return true;
                    break;
                case RazorVueForEachNode loop:
                    if (HasTemplateShape(loop.Body))
                        return true;
                    break;
                case RazorVueForNode loop:
                    if (HasTemplateShape(loop.Body))
                        return true;
                    break;
            }
        }

        return false;
    }

    private static bool HasUnsupportedTemplateNode(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueConditionalNode conditional:
                    if (HasUnsupportedTemplateNode(conditional.WhenTrue) || HasUnsupportedTemplateNode(conditional.WhenFalse))
                        return true;
                    break;
                case RazorVueForEachNode loop:
                    if (HasUnsupportedTemplateNode(loop.Body))
                        return true;
                    break;
                case RazorVueForNode loop:
                    if (HasUnsupportedTemplateNode(loop.Body))
                        return true;
                    break;
                case RazorVueUnsupportedTemplateNode:
                    return true;
                case RazorVueRenderNode:
                    break;
                default:
                    return true;
            }
        }

        return false;
    }

    private static VueRuntimeHints BuildHints(string moduleCode)
        => new(
            RequiresVueRuntime: true,
            RequiresHydration: false,
            SupportsSsr: true,
            UsesTeleport: moduleCode.Contains("Teleport", StringComparison.Ordinal),
            UsesSuspense: moduleCode.Contains("Suspense", StringComparison.Ordinal),
            UsesKeepAlive: moduleCode.Contains("KeepAlive", StringComparison.Ordinal));

    internal static VueRuntimeHints BuildHintsForCanonicalization(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
        => new(
            RequiresVueRuntime: true,
            RequiresHydration: false,
            SupportsSsr: true,
            UsesTeleport: ContainsComponentName(renderTree, snapshot, "Teleport"),
            UsesSuspense: ContainsComponentName(renderTree, snapshot, "Suspense"),
            UsesKeepAlive: ContainsComponentName(renderTree, snapshot, "KeepAlive"));

    private static bool ContainsComponentName(
        RazorVueRenderFragment fragment,
        RazorVueSemanticSnapshot snapshot,
        string componentName)
    {
        _ = snapshot;

        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueComponentNode component when string.Equals(component.ComponentName, componentName, StringComparison.Ordinal):
                    return true;
                case RazorVueElementNode element when ContainsComponentName(element.Children, snapshot, componentName):
                    return true;
                case RazorVueComponentNode component:
                    if (ContainsComponentName(component.Children, snapshot, componentName))
                        return true;
                    foreach (var slotTemplate in component.SlotTemplates)
                    {
                        if (ContainsComponentName(slotTemplate.Children, snapshot, componentName))
                            return true;
                    }
                    break;
                case RazorVueConditionalNode conditional when ContainsComponentName(conditional.WhenTrue, snapshot, componentName) ||
                                                             ContainsComponentName(conditional.WhenFalse, snapshot, componentName):
                    return true;
                case RazorVueForEachNode loop when ContainsComponentName(loop.Body, snapshot, componentName):
                    return true;
                case RazorVueForNode loop when ContainsComponentName(loop.Body, snapshot, componentName):
                    return true;
            }
        }

        return false;
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("X2"));
        return builder.ToString();
    }

}
