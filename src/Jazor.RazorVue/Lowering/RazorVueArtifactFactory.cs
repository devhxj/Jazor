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

    private readonly RazorVueRenderTreeExtractor _renderTreeExtractor = new();

    public VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var renderTree = _renderTreeExtractor.Extract(context, snapshot);
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
        var moduleCode = BuildModuleCode(snapshot, renderTree, expressionEmitter, resolvedComponents);
        var sourceOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree));

        return new VueCompiledArtifact(
            ComponentName: descriptor.Name,
            RelativeModulePath: relativeModulePath,
            ModuleCode: moduleCode,
            Imports: BuildImports(resolvedComponents),
            Styles: BuildStyles(descriptor, resolvedComponents),
            PluginRequirements: BuildPluginRequirements(descriptor, resolvedComponents),
            Identity: BuildIdentity(context, snapshot, renderTree, expressionEmitter, relativeModulePath),
            Hints: BuildHints(moduleCode),
            SourceOrigins: sourceOrigins);
    }

    private static VueArtifactIdentity BuildIdentity(
        RazorVueCompilationContext? context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        string relativeModulePath)
    {
        var descriptor = snapshot.Descriptor;
        var descriptorShape = BuildDescriptorShape(descriptor);
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

    private static string BuildDescriptorShape(VueComponentDescriptor descriptor)
    {
        var descriptorShape = new StringBuilder();
        descriptorShape.AppendLine(descriptor.FullName);
        descriptorShape.AppendLine(descriptor.SourceKind.ToString());
        descriptorShape.AppendLine(descriptor.ImportSpecifier);
        descriptorShape.AppendLine(descriptor.ExportName);
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
                prop.Kind);
        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            descriptorShape.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);
        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
            descriptorShape.AppendLine(
                slot.PublicName + "|" +
                slot.Name + "|" +
                slot.IsDefault + "|" +
                slot.Required + "|" +
                string.Join(",", slot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
        foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
            descriptorShape.AppendLine("plugin:" + pluginRequirement);

        return descriptorShape.ToString();
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
        var onInitializedShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedMethod, false);
        var onInitializedAsyncShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedAsyncMethod, false);
        var onParametersSetShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetMethod, false);
        var onParametersSetAsyncShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetAsyncMethod, false);
        var setParametersAsyncShape = DescribeSetParametersAsyncShape(snapshot, snapshot.SetParametersAsyncMethod);
        var onAfterRenderShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderMethod, true);
        var onAfterRenderAsyncShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        var disposeShape = DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeMethod, false);
        var disposeAsyncShape = DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeAsyncMethod, false);
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
        logicShape.AppendLine("lifecycle:shouldRender=" + DescribeShouldRenderShape(snapshot.Compilation, snapshot.ShouldRenderMethod));
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
            !AnalyzeShouldRender(snapshot.Compilation, snapshot.ShouldRenderMethod).IsSupported)
        {
            return HmrBoundaryKind.FullReloadRequired;
        }

        if (snapshot.Lifecycle.HasSetParametersAsync &&
            !AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod).IsSupported)
        {
            return HmrBoundaryKind.FullReloadRequired;
        }

        var hasSupportedLifecycleLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                           HasSupportedSetParametersAsyncLowering(snapshot) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);
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
                case RazorVueConditionalNode conditional:
                    if (HasTemplateShape(conditional.WhenTrue) || HasTemplateShape(conditional.WhenFalse))
                        return true;
                    break;
                case RazorVueForEachNode loop:
                    if (HasTemplateShape(loop.Body))
                        return true;
                    break;
            }
        }

        return false;
    }

    private static bool HasUnsupportedTemplateNode(RazorVueRenderFragment fragment)
    {
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
