using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Analysis.Artifacts;
using Jazor.RazorVue.Analysis.Descriptor;
using Jazor.RazorVue.Analysis.Extensibility;
using Jazor.RazorVue.Analysis.RenderTree;

namespace Jazor.RazorVue.Analysis.Lowering;

internal sealed class RazorVueArtifactFactory : IRazorVueArtifactLowerer
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
            Imports: BuildImports(componentReferences),
            Styles: descriptor.StyleDependencies,
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
        descriptorShape.AppendLine(descriptor.ImportSpecifier);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            descriptorShape.AppendLine(prop.PublicName + "|" + prop.Name + "|" + prop.TypeName + "|" + prop.Kind);
        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            descriptorShape.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);
        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
            descriptorShape.AppendLine(slot.Name + "|" + slot.IsDefault + "|" + slot.Required);

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
        logicShape.AppendLine("component:" + descriptor.FullName);
        logicShape.AppendLine("module:" + descriptor.ImportSpecifier);
        logicShape.AppendLine("lifecycle:onInitialized=" + snapshot.Lifecycle.HasOnInitialized);
        logicShape.AppendLine("lifecycle:onInitializedAsync=" + snapshot.Lifecycle.HasOnInitializedAsync);
        logicShape.AppendLine("lifecycle:onParametersSet=" + snapshot.Lifecycle.HasOnParametersSet);
        logicShape.AppendLine("lifecycle:onParametersSetAsync=" + snapshot.Lifecycle.HasOnParametersSetAsync);
        logicShape.AppendLine("lifecycle:onAfterRender=" + snapshot.Lifecycle.HasOnAfterRender);
        logicShape.AppendLine("lifecycle:onAfterRenderAsync=" + snapshot.Lifecycle.HasOnAfterRenderAsync);
        logicShape.AppendLine("lifecycle:shouldRender=" + snapshot.Lifecycle.HasShouldRender);
        logicShape.AppendLine("lifecycle:setParametersAsync=" + snapshot.Lifecycle.HasSetParametersAsync);
        logicShape.AppendLine("lifecycle:dispose=" + snapshot.Lifecycle.HasDispose);
        logicShape.AppendLine("lifecycle:disposeAsync=" + snapshot.Lifecycle.HasDisposeAsync);

        foreach (var method in snapshot.Logic.Methods
                     .OrderBy(static method => method.Name, StringComparer.Ordinal)
                     .ThenBy(static method => method.Arity))
        {
            logicShape.AppendLine("logic:" + method.Name + "|" + method.Arity + "|" + method.IsAsync);
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

        if (snapshot.Lifecycle.HasDispose || snapshot.Lifecycle.HasDisposeAsync ||
            snapshot.Lifecycle.HasShouldRender || snapshot.Lifecycle.HasSetParametersAsync)
            return HmrBoundaryKind.FullReloadRequired;

        if (snapshot.Lifecycle.HasAnyHook || snapshot.Logic.Methods.Length > 0)
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

    private static string BuildModuleCode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptor = snapshot.Descriptor;
        var builder = new StringBuilder();
        builder.AppendLine("import { defineComponent, h } from \"vue\";");
        AppendComponentImports(builder, resolvedComponents);
        builder.AppendLine();
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.AppendLine("  setup(props, { emit, slots, expose, attrs }) {");
        builder.Append("    return () => ").Append(expressionEmitter.EmitFragment(renderTree)).AppendLine(";");
        builder.AppendLine("  }");
        builder.AppendLine("});");
        return builder.ToString();
    }

    private static string FormatStringArray(IEnumerable<string> values)
        => "[" + string.Join(", ", values.Select(ToJavaScriptString)) + "]";

    private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
    {
        var components = CollectComponents(renderTree);
        if (components.Count == 0)
            return ImmutableDictionary<string, VueComponentDescriptor>.Empty;

        var registry = context.CreateComponentRegistry();
        var builder = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);

        foreach (var component in components)
        {
            if (!registry.ComponentsByFullName.TryGetValue(component.ComponentFullName, out var descriptor))
                throw new NotSupportedException(
                    $"RazorVue render could not resolve component node '{component.ComponentFullName}' in component '{snapshot.Descriptor.FullName}'.");

            builder[component.ComponentName] = descriptor;
        }

        return builder.ToImmutable();
    }

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

        return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }

    private static ImmutableArray<string> BuildImports(ImmutableDictionary<string, string> componentReferences)
    {
        if (componentReferences.IsEmpty)
            return ImmutableArray.Create("vue");

        return ImmutableArray.Create("vue").AddRange(componentReferences.Values.Where(static value => value.EndsWith("Component", StringComparison.Ordinal)));
    }

    private static void AppendComponentImports(StringBuilder builder, ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        foreach (var item in resolvedComponents.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (string.Equals(item.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
                continue;

            builder.Append("import ");
            builder.Append(CreateComponentAlias(item.Key));
            builder.Append(" from ");
            builder.Append(ToJavaScriptString(item.Value.ImportSpecifier));
            builder.AppendLine(";");
        }
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

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        return normalized;
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

