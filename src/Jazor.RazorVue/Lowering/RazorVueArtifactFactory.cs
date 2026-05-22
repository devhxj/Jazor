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

    internal static string CreateImperativeComponentMetadataAlias(string componentName)
        => "__jazorImperativeComponentMetadata_" + componentName;

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
        var descriptorShape = RazorVueDescriptorIdentityShapeBuilder.BuildForRenderTree(
            descriptor,
            snapshot.ComponentSymbol,
            snapshot.Compilation,
            renderTree,
            resolvedComponents);
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

        foreach (var property in snapshot.Logic.Properties
                     .OrderBy(static property => property.Name, StringComparer.Ordinal))
        {
            logicShape.AppendLine("property:" + property.Name + "|" + property.IsReadOnly + "|" + property.LoweringKind + "|" + DescribeSetupPropertyShape(property.PropertySymbol));
        }

        foreach (var method in snapshot.Logic.Methods
                     .OrderBy(static method => method.Name, StringComparer.Ordinal)
                     .ThenBy(static method => method.Arity))
        {
            logicShape.AppendLine("logic:" + method.Name + "|" + method.Arity + "|" + method.IsAsync + "|" + DescribeSetupMethodShape(method.MethodSymbol));
        }

        return logicShape.ToString();
    }

    internal static string BuildLogicShapeForSfc(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter)
        => BuildLogicShape(context: null, snapshot, renderTree, expressionEmitter);

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
        if (hasSupportedLifecycleLowering || snapshot.Logic.Properties.Length > 0 || snapshot.Logic.Fields.Length > 0 || snapshot.Logic.Methods.Length > 0)
            return HmrBoundaryKind.LogicSafe;

        if (HasTemplateShape(renderTree))
            return HmrBoundaryKind.TemplateOnly;

        return HmrBoundaryKind.Unknown;
    }

    internal static HmrBoundaryKind ClassifyHmrBoundaryForSfc(
        RazorVueRenderFragment renderTree,
        RazorVueSemanticSnapshot snapshot)
        => ClassifyHmrBoundary(renderTree, snapshot);

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
                case RazorVueTemplateScopeNode templateScope:
                    if (HasTemplateShape(templateScope.Children))
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
                case RazorVueImperativeBlockNode:
                    return true;
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
                case RazorVueTemplateScopeNode templateScope:
                    if (HasUnsupportedTemplateNode(templateScope.Children))
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
                case RazorVueImperativeBlockNode:
                    return true;
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
                case RazorVueTemplateScopeNode templateScope when ContainsComponentName(templateScope.Children, snapshot, componentName):
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
