using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory
{
    private static string BuildModuleCode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        out ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var descriptor = snapshot.Descriptor;
        var usesImperativeRenderBody = expressionEmitter.ContainsImperativeRenderBody(renderTree);
        var renderExpression = usesImperativeRenderBody ? null : expressionEmitter.EmitFragment(renderTree);
        var requiresAttributeMergeHelper = renderExpression is not null &&
                                           RazorVueAttributeMergeHelper.ContainsInvocation(renderExpression);
        var propDefaultBindings = CollectPropDefaultBindings(snapshot, descriptor, expressionEmitter);
        var setupBodyBuilder = new StringBuilder();
        setupBodyBuilder.AppendLine("  setup(__jazorRawProps, { emit, slots, expose, attrs }) {");
        AppendPropsBinding(setupBodyBuilder, propDefaultBindings);
        if (RazorVueForLoopLoweringSupport.ContainsForLoop(renderTree))
            RazorVueForLoopLoweringSupport.AppendForRangeHelper(setupBodyBuilder, "    ");
        if (requiresAttributeMergeHelper)
            RazorVueAttributeMergeHelper.AppendHelper(setupBodyBuilder, "    ");
        if (usesImperativeRenderBody)
            AppendImperativeComponentMetadataBindings(setupBodyBuilder, resolvedComponents, "    ");
        if (usesImperativeRenderBody)
            AppendImperativeRenderBridgeHelper(setupBodyBuilder, "    ");
        AppendLifecycleLowering(setupBodyBuilder, snapshot);
        AppendSetupLogicLowering(setupBodyBuilder, snapshot, expressionEmitter);
        if (usesImperativeRenderBody)
        {
            setupBodyBuilder.AppendLine("    const __jazorComponent = { props, emit, slots, expose, attrs };");
            setupBodyBuilder.AppendLine("    return () => {");
            setupBodyBuilder.Append("      const ").Append(RazorVueExpressionEmitter.ImperativeRenderContextAlias).AppendLine(" = __jazorCreateRenderContext(h);");
            setupBodyBuilder.AppendLine("      __jazorComponent.props = props;");
            setupBodyBuilder.AppendLine("      __jazorComponent.emit = emit;");
            setupBodyBuilder.AppendLine("      __jazorComponent.slots = slots;");
            setupBodyBuilder.AppendLine("      __jazorComponent.expose = expose;");
            setupBodyBuilder.AppendLine("      __jazorComponent.attrs = attrs;");
            setupBodyBuilder.Append("      ").Append(expressionEmitter.EmitImperativeRenderBody(renderTree).Replace("\n", "\n      ")).AppendLine();
            setupBodyBuilder.AppendLine("    };");
        }
        else
        {
            setupBodyBuilder.Append("    return () => ").Append(renderExpression).AppendLine(";");
        }
        setupBodyBuilder.AppendLine("  }");

        compilerImports = expressionEmitter.FlushCompilerImports();

        var builder = new StringBuilder();
        AppendVueImports(builder, snapshot, resolvedComponents, compilerImports);
        builder.AppendLine();
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.Append(setupBodyBuilder);
        builder.AppendLine("});");
        return builder.ToString();
    }

    internal static void AppendImperativeRenderBridgeHelperForSfc(StringBuilder builder, string indent)
        => AppendImperativeRenderBridgeHelper(builder, indent);

    private static void AppendImperativeRenderBridgeHelper(StringBuilder builder, string indent)
    {
        builder.Append(indent).AppendLine("function __jazorCreateRenderContext(h) {");
        builder.Append(indent).AppendLine("  const __rootNodes = [];");
        builder.Append(indent).AppendLine("  const __stack = [];");
        builder.Append(indent).AppendLine("  const __regions = [];");
        builder.Append(indent).AppendLine("  function __jazorCreateSlotReference(slot, acceptsContext) {");
        builder.Append(indent).AppendLine("    return { __jazorSlotReference: true, slot, acceptsContext: !!acceptsContext };");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __jazorCreateRenderSlot(fragment) { return { __jazorRenderSlot: true, fragment }; }");
        builder.Append(indent).AppendLine("  function __jazorCreateContextualRenderSlot(fragment) { return { __jazorContextualRenderSlot: true, fragment }; }");
        builder.Append(indent).AppendLine("  function __jazorCreateSlotMap() { return Object.create(null); }");
        builder.Append(indent).AppendLine("  function __jazorInvokeRenderFragment(fragment) {");
        builder.Append(indent).AppendLine("    const fragmentContext = __jazorCreateRenderContext(h);");
        builder.Append(indent).AppendLine("    fragment(fragmentContext);");
        builder.Append(indent).AppendLine("    return fragmentContext.finish();");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __jazorIsSlotReference(value) { return !!(value && value.__jazorSlotReference === true); }");
        builder.Append(indent).AppendLine("  function __jazorIsRenderSlot(value) { return !!(value && value.__jazorRenderSlot === true); }");
        builder.Append(indent).AppendLine("  function __jazorIsContextualRenderSlot(value) { return !!(value && value.__jazorContextualRenderSlot === true); }");
        builder.Append(indent).AppendLine("  function __jazorResolveSlotValue(value, context) {");
        builder.Append(indent).AppendLine("    if (__jazorIsSlotReference(value)) {");
        builder.Append(indent).AppendLine("      const slot = value.slot;");
        builder.Append(indent).AppendLine("      if (typeof slot !== \"function\") return null;");
        builder.Append(indent).AppendLine("      return value.acceptsContext ? slot(context) : slot();");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (__jazorIsRenderSlot(value)) {");
        builder.Append(indent).AppendLine("      return () => __jazorInvokeRenderFragment(value.fragment);");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (__jazorIsContextualRenderSlot(value)) {");
        builder.Append(indent).AppendLine("      return (slotContext) => {");
        builder.Append(indent).AppendLine("        const templateResult = value.fragment(slotContext);");
        builder.Append(indent).AppendLine("        return typeof templateResult === \"function\"");
        builder.Append(indent).AppendLine("          ? __jazorInvokeRenderFragment(templateResult)");
        builder.Append(indent).AppendLine("          : templateResult;");
        builder.Append(indent).AppendLine("      };");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (typeof value !== \"function\") return value;");
        builder.Append(indent).AppendLine("    return value;");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __applyProp(frame, key, value) {");
        builder.Append(indent).AppendLine("    if (!key || value === null || value === undefined || value === false) return;");
        builder.Append(indent).AppendLine("    frame.props[key] = value === true ? true : value;");
        builder.Append(indent).AppendLine("    frame.hasProps = true;");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __applyComponentParameter(frame, key, value) {");
        builder.Append(indent).AppendLine("    const metadata = frame.metadata;");
        builder.Append(indent).AppendLine("    if (metadata && metadata.slots) {");
        builder.Append(indent).AppendLine("      const slot = metadata.slots[key];");
        builder.Append(indent).AppendLine("      if (slot) {");
        builder.Append(indent).AppendLine("        frame.slots[slot.runtimeName] = __jazorResolveSlotValue(value);");
        builder.Append(indent).AppendLine("        frame.hasSlots = true;");
        builder.Append(indent).AppendLine("        return;");
        builder.Append(indent).AppendLine("      }");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (metadata && metadata.emits && Object.prototype.hasOwnProperty.call(metadata.emits, key)) {");
        builder.Append(indent).AppendLine("      __applyProp(frame, metadata.emits[key], value);");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (metadata && metadata.props && Object.prototype.hasOwnProperty.call(metadata.props, key)) {");
        builder.Append(indent).AppendLine("      __applyProp(frame, metadata.props[key], value);");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    __applyProp(frame, key, value);");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __appendNode(node) {");
        builder.Append(indent).AppendLine("    if (node === null || node === undefined || node === false) return;");
        builder.Append(indent).AppendLine("    if (Array.isArray(node)) {");
        builder.Append(indent).AppendLine("      for (const child of node) __appendNode(child);");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    const current = __stack.length === 0 ? null : __stack[__stack.length - 1];");
        builder.Append(indent).AppendLine("    if (current) current.children.push(node); else __rootNodes.push(node);");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __invokeFragment(fragment, value, hasValueArgument) {");
        builder.Append(indent).AppendLine("    if (typeof fragment !== \"function\") return fragment;");
        builder.Append(indent).AppendLine("    return hasValueArgument ? fragment(value) : fragment();");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __materializeChildren(children) {");
        builder.Append(indent).AppendLine("    if (!children || children.length === 0) return null;");
        builder.Append(indent).AppendLine("    return children.length === 1 ? children[0] : children;");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __closeFrame(expectedKind) {");
        builder.Append(indent).AppendLine("    const frame = __stack.pop();");
        builder.Append(indent).AppendLine("    if (!frame || frame.kind !== expectedKind) throw new Error(`RazorVue imperative render bridge encountered mismatched ${expectedKind} closure.`);");
        builder.Append(indent).AppendLine("    const props = frame.hasProps ? frame.props : null;");
        builder.Append(indent).AppendLine("    const slotChildren = frame.hasSlots ? frame.slots : null;");
        builder.Append(indent).AppendLine("    const children = frame.hasSlots ? slotChildren : __materializeChildren(frame.children);");
        builder.Append(indent).AppendLine("    __appendNode(h(frame.target, props, children));");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  function __applyMultipleAttributes(frame, values) {");
        builder.Append(indent).AppendLine("    if (!values) return;");
        builder.Append(indent).AppendLine("    if (typeof values[Symbol.iterator] === \"function\") {");
        builder.Append(indent).AppendLine("      for (const entry of values) {");
        builder.Append(indent).AppendLine("        if (!entry) continue;");
        builder.Append(indent).AppendLine("        const key = Array.isArray(entry) ? entry[0] : entry.key;");
        builder.Append(indent).AppendLine("        const value = Array.isArray(entry) ? entry[1] : entry.value;");
        builder.Append(indent).AppendLine("        __applyProp(frame, key, value);");
        builder.Append(indent).AppendLine("      }");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    for (const [key, value] of Object.entries(values)) __applyProp(frame, key, value);");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  return {");
        builder.Append(indent).AppendLine("    enterElement(name) { __stack.push({ kind: \"element\", target: name, props: Object.create(null), hasProps: false, children: [] }); },");
        builder.Append(indent).AppendLine("    leaveElement() { __closeFrame(\"element\"); },");
        builder.Append(indent).AppendLine("    enterComponent(component, metadata) { __stack.push({ kind: \"component\", target: component, metadata: metadata ?? null, props: Object.create(null), hasProps: false, slots: __jazorCreateSlotMap(), hasSlots: false, children: [] }); },");
        builder.Append(indent).AppendLine("    leaveComponent() { __closeFrame(\"component\"); },");
        builder.Append(indent).AppendLine("    append(fragmentOrValue, value) { __appendNode(__invokeFragment(fragmentOrValue, value, arguments.length >= 2)); },");
        builder.Append(indent).AppendLine("    setAttribute(name, value) { const frame = __stack[__stack.length - 1]; if (!frame) throw new Error(\"RazorVue imperative render context setAttribute requires an open frame.\"); if (frame.kind === \"component\") { __applyComponentParameter(frame, name, value); return; } __applyProp(frame, name, value); },");
        builder.Append(indent).AppendLine("    setComponentParameter(name, value) { const frame = __stack[__stack.length - 1]; if (!frame || frame.kind !== \"component\") throw new Error(\"RazorVue imperative render context setComponentParameter requires an open component frame.\"); __applyComponentParameter(frame, name, value); },");
        builder.Append(indent).AppendLine("    mergeAttributes(values) { const frame = __stack[__stack.length - 1]; if (!frame) throw new Error(\"RazorVue imperative render context mergeAttributes requires an open frame.\"); __applyMultipleAttributes(frame, values); },");
        builder.Append(indent).AppendLine("    setKey(value) { const frame = __stack[__stack.length - 1]; if (!frame) throw new Error(\"RazorVue imperative render context setKey requires an open frame.\"); __applyProp(frame, \"key\", value); },");
        builder.Append(indent).AppendLine("    openRegion() { __regions.push(__rootNodes.length); },");
        builder.Append(indent).AppendLine("    closeRegion() { if (__regions.length === 0) throw new Error(\"RazorVue imperative render context closeRegion requires a matching openRegion.\"); __regions.pop(); },");
        builder.Append(indent).AppendLine("    finish() {");
        builder.Append(indent).AppendLine("      if (__stack.length !== 0) throw new Error(\"RazorVue imperative render bridge completed with unclosed frames.\");");
        builder.Append(indent).AppendLine("      if (__rootNodes.length === 0) return null;");
        builder.Append(indent).AppendLine("      return __rootNodes.length === 1 ? __rootNodes[0] : __rootNodes;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("  };");
        builder.Append(indent).AppendLine("}");
    }

    internal static void AppendImperativeComponentMetadataBindingsForSfc(
        StringBuilder builder,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        string indent)
        => AppendImperativeComponentMetadataBindings(builder, resolvedComponents, indent);

    private static void AppendImperativeComponentMetadataBindings(
        StringBuilder builder,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        string indent)
    {
        foreach (var item in resolvedComponents.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append(indent)
                .Append("const ")
                .Append(CreateImperativeComponentMetadataAlias(item.Key))
                .Append(" = ")
                .Append(BuildImperativeComponentMetadata(item.Value))
                .AppendLine(";");
        }
    }

    private static string BuildImperativeComponentMetadata(VueComponentDescriptor descriptor)
    {
        var propEntries = descriptor.Props
            .OrderBy(static prop => prop.PublicName, StringComparer.Ordinal)
            .Select(static prop => ToJavaScriptString(prop.PublicName) + ": " + ToJavaScriptString(prop.Name));
        var emitEntries = descriptor.Emits
            .Where(static emit => !string.IsNullOrWhiteSpace(emit.RazorAlias))
            .OrderBy(static emit => emit.RazorAlias, StringComparer.Ordinal)
            .Select(static emit => ToJavaScriptString(emit.RazorAlias!) + ": " + ToJavaScriptString(ToVueEventHandlerName(emit.Name)));
        var slotEntries = descriptor.Slots
            .OrderBy(static slot => slot.PublicName, StringComparer.Ordinal)
            .Select(static slot => ToJavaScriptString(slot.PublicName) + ": { runtimeName: " + ToJavaScriptString(slot.Name) + " }");

        return "{ props: { " + string.Join(", ", propEntries) + " }, emits: { " + string.Join(", ", emitEntries) + " }, slots: { " + string.Join(", ", slotEntries) + " } }";
    }

    internal static void AppendVueImportsForSfc(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
        => AppendVueImports(builder, snapshot, resolvedComponents, compilerImports);

    private static void AppendVueImports(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var vueImports = new List<string> { "defineComponent", "h" };
        vueImports.AddRange(RazorVueSetupAndLifecycleLoweringSupport.CollectVueRuntimeImports(snapshot));

        builder.Append("import { ")
            .Append(string.Join(", ", vueImports.Distinct(StringComparer.Ordinal)))
            .AppendLine(" } from \"vue\";");
        RazorVueCompilerImportFormatter.AppendImportStatements(builder, compilerImports);
        AppendComponentImports(builder, resolvedComponents);
    }

    internal static void AppendLifecycleLoweringForSfc(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
        => AppendLifecycleLowering(builder, snapshot);

    private static void AppendLifecycleLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
        => RazorVueSetupAndLifecycleLoweringSupport.AppendLifecycleLowering(builder, snapshot, "    ");

    internal static void AppendSetupLogicLoweringForSfc(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => AppendSetupLogicLowering(builder, snapshot, expressionEmitter);

    private static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueSetupAndLifecycleLoweringSupport.AppendSetupLogicLowering(
            builder,
            snapshot,
            expressionEmitter,
            ImmutableArray<VueLogicFieldDescriptor>.Empty,
            ImmutableArray<VueLogicMethodDescriptor>.Empty,
            "    ");

    internal static ImmutableArray<PropDefaultBinding> CollectPropDefaultBindingsForSfc(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueExpressionEmitter expressionEmitter)
        => CollectPropDefaultBindings(snapshot, descriptor, expressionEmitter);

    private static ImmutableArray<PropDefaultBinding> CollectPropDefaultBindings(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueExpressionEmitter expressionEmitter)
    {
        if (descriptor.Props.IsDefaultOrEmpty)
            return ImmutableArray<PropDefaultBinding>.Empty;

        var builder = ImmutableArray.CreateBuilder<PropDefaultBinding>();
        foreach (var prop in descriptor.Props)
        {
            if (prop.DefaultSource != VuePropDefaultSource.PropertyInitializer ||
                string.IsNullOrWhiteSpace(prop.DefaultExpression))
                continue;

            var expression = LowerDefaultExpression(snapshot, expressionEmitter, prop);
            builder.Add(new PropDefaultBinding(prop.Name, expression));
        }

        return builder.ToImmutable();
    }

    internal static void AppendPropsBindingForSfc(
        StringBuilder builder,
        ImmutableArray<PropDefaultBinding> propDefaultBindings)
        => AppendPropsBinding(builder, propDefaultBindings);

    private static void AppendPropsBinding(
        StringBuilder builder,
        ImmutableArray<PropDefaultBinding> propDefaultBindings)
    {
        if (propDefaultBindings.IsDefaultOrEmpty)
        {
            builder.AppendLine("    const props = __jazorRawProps;");
            return;
        }

        AppendPropDefaultProxy(builder, propDefaultBindings);
    }

    private static void AppendPropDefaultProxy(
        StringBuilder builder,
        ImmutableArray<PropDefaultBinding> propDefaultBindings)
    {
        builder.AppendLine("    const __jazorPropDefaultCache = Object.create(null);");
        builder.AppendLine("    const props = new Proxy(__jazorRawProps, {");
        builder.AppendLine("      get(target, key, receiver) {");
        builder.AppendLine("        if (typeof key === \"string\") {");
        foreach (var binding in propDefaultBindings)
        {
            builder.Append("          if (key === ")
                .Append(ToJavaScriptString(binding.PropName))
                .AppendLine(") {");
            builder.AppendLine("            const value = Reflect.get(target, key, receiver);");
            builder.AppendLine("            if (value !== undefined) return value;");
            builder.AppendLine("            if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];");
            builder.Append("            const defaultValue = ")
                .Append(binding.ExpressionText)
                .AppendLine(";");
            builder.AppendLine("            __jazorPropDefaultCache[key] = defaultValue;");
            builder.AppendLine("            return defaultValue;");
            builder.AppendLine("          }");
        }

        builder.AppendLine("        }");
        builder.AppendLine("        return Reflect.get(target, key, receiver);");
        builder.AppendLine("      }");
        builder.AppendLine("    });");
    }

    private static string LowerDefaultExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VuePropDescriptor prop)
    {
        if (prop.DefaultSource != VuePropDefaultSource.PropertyInitializer ||
            string.IsNullOrWhiteSpace(prop.DefaultExpression))
            throw new InvalidOperationException($"Prop '{prop.PublicName}' does not declare a default expression.");

        var propertySymbol = snapshot.ComponentSymbol
            .GetMembers(prop.PublicName)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(static candidate => !candidate.IsStatic);
        if (propertySymbol is null ||
            propertySymbol.DeclaringSyntaxReferences.Length == 0)
        {
            return prop.DefaultExpression!;
        }

        foreach (var reference in propertySymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (!RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declaration.Initializer.Value,
                    out var operation))
            {
                continue;
            }

            return expressionEmitter.EmitSetupExpression(operation!);
        }

        return prop.DefaultExpression!;
    }

    internal sealed record PropDefaultBinding(string PropName, string ExpressionText);

    private static string DescribeSetupFieldShape(IFieldSymbol field)
    {
        if (field.DeclaringSyntaxReferences.Length == 0)
            return "unsupported";

        var syntax = field.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not VariableDeclaratorSyntax declarator || declarator.Initializer is null)
            return "unsupported";

        return declarator.Initializer.Value.ToString();
    }

    private static string DescribeSetupMethodShape(IMethodSymbol method)
    {
        if (method.DeclaringSyntaxReferences.Length == 0)
            return "unsupported";

        var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not MethodDeclarationSyntax methodSyntax)
            return "unsupported";

        if (methodSyntax.ExpressionBody is not null)
            return methodSyntax.ExpressionBody.Expression.ToString();

        if (methodSyntax.Body?.Statements.Count == 1 &&
            methodSyntax.Body.Statements[0] is ReturnStatementSyntax returnStatement &&
            returnStatement.Expression is not null)
        {
            return returnStatement.Expression.ToString();
        }

        return "unsupported";
    }

    internal static string FormatStringArrayForSfc(IEnumerable<string> values)
        => FormatStringArray(values);

    private static string FormatStringArray(IEnumerable<string> values)
        => "[" + string.Join(", ", values.Select(ToJavaScriptString)) + "]";
}
