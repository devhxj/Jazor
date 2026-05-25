using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueImperativeSfcModuleBuilder
{
    public static string BuildModuleCode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        out ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var descriptor = snapshot.Descriptor;
        var propDefaultBindings = CollectPropDefaultBindings(snapshot, descriptor, expressionEmitter);
        var renderBody = expressionEmitter.EmitImperativeRenderBody(renderTree);
        var requiresAttributeMergeHelper = RazorVueAttributeMergeHelper.ContainsInvocation(renderBody);
        var setupBodyBuilder = new StringBuilder();
        setupBodyBuilder.AppendLine("  setup(__jazorRawProps, { emit, slots, expose, attrs }) {");
        AppendPropsBinding(setupBodyBuilder, propDefaultBindings);
        if (RazorVueForLoopLoweringSupport.ContainsForLoop(renderTree))
            RazorVueForLoopLoweringSupport.AppendForRangeHelper(setupBodyBuilder, "    ");
        if (requiresAttributeMergeHelper)
            RazorVueAttributeMergeHelper.AppendHelper(setupBodyBuilder, "    ");
        AppendImperativeComponentMetadataBindings(setupBodyBuilder, resolvedComponents, "    ");
        AppendImperativeRenderBridgeHelper(setupBodyBuilder, "    ");
        var lifecyclePlan = RazorVueSetupAndLifecycleLoweringSupport.CreateLifecyclePlan(snapshot, expressionEmitter);
        RazorVueSetupAndLifecycleLoweringSupport.AppendSetupLogicLowering(
            setupBodyBuilder,
            snapshot,
            expressionEmitter,
            lifecyclePlan.RequiredProperties,
            lifecyclePlan.RequiredFields,
            lifecyclePlan.RequiredMethods,
            "    ");
        RazorVueSetupAndLifecycleLoweringSupport.AppendLifecyclePlan(setupBodyBuilder, lifecyclePlan, "    ");
        RazorVueArtifactFactory.AppendShouldRenderGateStateForSfc(setupBodyBuilder, lifecyclePlan.ShouldRenderGate, "    ");
        setupBodyBuilder.AppendLine("    const __jazorComponent = { props, emit, slots, expose, attrs };");
        setupBodyBuilder.AppendLine("    return () => {");
        RazorVueArtifactFactory.AppendShouldRenderGateEarlyReturnForSfc(setupBodyBuilder, lifecyclePlan.ShouldRenderGate, "      ");
        if (lifecyclePlan.ShouldRenderGate is null)
        {
            setupBodyBuilder.Append("      const ").Append(RazorVueExpressionEmitter.ImperativeRenderContextAlias).AppendLine(" = __jazorCreateRenderContext(h);");
            setupBodyBuilder.AppendLine("      __jazorComponent.props = props;");
            setupBodyBuilder.AppendLine("      __jazorComponent.emit = emit;");
            setupBodyBuilder.AppendLine("      __jazorComponent.slots = slots;");
            setupBodyBuilder.AppendLine("      __jazorComponent.expose = expose;");
            setupBodyBuilder.AppendLine("      __jazorComponent.attrs = attrs;");
            setupBodyBuilder.Append("      ").Append(renderBody.Replace("\n", "\n      ")).AppendLine();
        }
        else
        {
            setupBodyBuilder.AppendLine("      const __jazorNextVNode = (() => {");
            setupBodyBuilder.Append("        const ").Append(RazorVueExpressionEmitter.ImperativeRenderContextAlias).AppendLine(" = __jazorCreateRenderContext(h);");
            setupBodyBuilder.AppendLine("        __jazorComponent.props = props;");
            setupBodyBuilder.AppendLine("        __jazorComponent.emit = emit;");
            setupBodyBuilder.AppendLine("        __jazorComponent.slots = slots;");
            setupBodyBuilder.AppendLine("        __jazorComponent.expose = expose;");
            setupBodyBuilder.AppendLine("        __jazorComponent.attrs = attrs;");
            setupBodyBuilder.Append("        ").Append(renderBody.Replace("\n", "\n        ")).AppendLine();
            setupBodyBuilder.AppendLine("      })();");
            RazorVueArtifactFactory.AppendShouldRenderGateCacheNextVNodeForSfc(setupBodyBuilder, "      ");
        }
        setupBodyBuilder.AppendLine("    };");
        setupBodyBuilder.AppendLine("  }");

        var helperDeclarationsBuilder = new StringBuilder();
        expressionEmitter.AppendRequiredHelperTypeDeclarations(helperDeclarationsBuilder, string.Empty);
        var helperDeclarations = helperDeclarationsBuilder.ToString();
        compilerImports = expressionEmitter.FlushCompilerImports();

        var builder = new StringBuilder();
        AppendVueImports(builder, snapshot, resolvedComponents, compilerImports);
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(helperDeclarations))
        {
            builder.AppendLine(helperDeclarations.TrimEnd());
            builder.AppendLine();
        }
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.Append(setupBodyBuilder);
        builder.AppendLine("});");
        return builder.ToString();
    }

    public static string BuildLogicShape(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueArtifactFactory.BuildLogicShapeForSfc(snapshot, renderTree, expressionEmitter);

    public static HmrBoundaryKind ClassifyHmrBoundary(
        RazorVueRenderFragment renderTree,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueArtifactFactory.ClassifyHmrBoundaryForSfc(renderTree, snapshot, expressionEmitter);

    private static ImmutableArray<RazorVueArtifactFactory.PropDefaultBinding> CollectPropDefaultBindings(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueArtifactFactory.CollectPropDefaultBindingsForSfc(snapshot, descriptor, expressionEmitter);

    private static void AppendPropsBinding(
        StringBuilder builder,
        ImmutableArray<RazorVueArtifactFactory.PropDefaultBinding> propDefaultBindings)
        => RazorVueArtifactFactory.AppendPropsBindingForSfc(builder, propDefaultBindings);

    private static void AppendImperativeRenderBridgeHelper(StringBuilder builder, string indent)
        => RazorVueArtifactFactory.AppendImperativeRenderBridgeHelperForSfc(builder, indent);

    private static void AppendImperativeComponentMetadataBindings(
        StringBuilder builder,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        string indent)
        => RazorVueArtifactFactory.AppendImperativeComponentMetadataBindingsForSfc(builder, resolvedComponents, indent);

    private static void AppendVueImports(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
        => RazorVueArtifactFactory.AppendVueImportsForSfc(builder, snapshot, resolvedComponents, compilerImports);

    private static void AppendLifecycleLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
        => RazorVueArtifactFactory.AppendLifecycleLoweringForSfc(builder, snapshot);

    private static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueArtifactFactory.AppendSetupLogicLoweringForSfc(builder, snapshot, expressionEmitter);

    private static string FormatStringArray(IEnumerable<string> values)
        => RazorVueArtifactFactory.FormatStringArrayForSfc(values);
}
