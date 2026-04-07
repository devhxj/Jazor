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
        descriptorShape.AppendLine(descriptor.ImportSpecifier);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            descriptorShape.AppendLine(prop.PublicName + "|" + prop.Name + "|" + prop.TypeName + "|" + prop.Kind);
        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            descriptorShape.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);
        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
            descriptorShape.AppendLine(slot.Name + "|" + slot.IsDefault + "|" + slot.Required);
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
        var onAfterRenderShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderMethod, true);
        var onAfterRenderAsyncShape = DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        logicShape.AppendLine("component:" + descriptor.FullName);
        logicShape.AppendLine("module:" + descriptor.ImportSpecifier);
        // LogicHash should reflect emitted runtime behavior. No-op lifecycle methods
        // must not perturb the hash when they do not lower into Vue hooks.
        logicShape.AppendLine("lifecycle:onInitialized=" + onInitializedShape);
        logicShape.AppendLine("lifecycle:onInitializedAsync=" + onInitializedAsyncShape);
        logicShape.AppendLine("lifecycle:onParametersSet=" + onParametersSetShape);
        logicShape.AppendLine("lifecycle:onParametersSetAsync=" + onParametersSetAsyncShape);
        logicShape.AppendLine("lifecycle:onAfterRender=" + onAfterRenderShape);
        logicShape.AppendLine("lifecycle:onAfterRenderAsync=" + onAfterRenderAsyncShape);
        logicShape.AppendLine("lifecycle:shouldRender=" + snapshot.Lifecycle.HasShouldRender);
        logicShape.AppendLine("lifecycle:setParametersAsync=" + snapshot.Lifecycle.HasSetParametersAsync);
        logicShape.AppendLine("lifecycle:dispose=" + snapshot.Lifecycle.HasDispose);
        logicShape.AppendLine("lifecycle:disposeAsync=" + snapshot.Lifecycle.HasDisposeAsync);

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

        if (snapshot.Lifecycle.HasDispose || snapshot.Lifecycle.HasDisposeAsync ||
            snapshot.Lifecycle.HasShouldRender || snapshot.Lifecycle.HasSetParametersAsync)
            return HmrBoundaryKind.FullReloadRequired;

        var hasSupportedLifecycleLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                           HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
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

    private static string BuildModuleCode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptor = snapshot.Descriptor;
        var builder = new StringBuilder();
        AppendVueImports(builder, snapshot, resolvedComponents);
        var renderExpression = expressionEmitter.EmitFragment(renderTree);
        builder.AppendLine();
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.AppendLine("  setup(props, { emit, slots, expose, attrs }) {");
        AppendLifecycleLowering(builder, snapshot);
        AppendSetupLogicLowering(builder, snapshot, expressionEmitter);
        builder.Append("    return () => ").Append(renderExpression).AppendLine(";");
        builder.AppendLine("  }");
        builder.AppendLine("});");
        return builder.ToString();
    }

    private static void AppendVueImports(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var vueImports = new List<string> { "defineComponent", "h" };
        // Vue imports must track actual lowering, otherwise no-op lifecycle methods
        // would leave behind imports for hooks that never materialize in setup().
        var hasInitializedLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                     HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false);
        if (hasInitializedLowering)
            vueImports.Add("onMounted");

        var hasParametersSetLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false);
        if (hasParametersSetLowering)
            vueImports.Add("watch");

        var hasAfterRenderLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                     HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        if (hasAfterRenderLowering)
        {
            vueImports.Add("onMounted");
            vueImports.Add("onUpdated");
        }

        builder.Append("import { ")
            .Append(string.Join(", ", vueImports.Distinct(StringComparer.Ordinal)))
            .AppendLine(" } from \"vue\";");
        AppendComponentImports(builder, resolvedComponents);
    }

    private static void AppendLifecycleLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
    {
        AppendLifecycleHook(builder, snapshot, "onMounted", snapshot.OnInitializedMethod, awaitResult: false);
        AppendLifecycleHook(builder, snapshot, "onMounted", snapshot.OnInitializedAsyncMethod, awaitResult: true);
        AppendParametersSetHook(builder, snapshot, snapshot.OnParametersSetMethod, awaitResult: false);
        AppendParametersSetHook(builder, snapshot, snapshot.OnParametersSetAsyncMethod, awaitResult: true);

        var onAfterRenderEmitCall = snapshot.OnAfterRenderMethod is null
            ? null
            : ExtractSupportedEmitCall(snapshot, snapshot.OnAfterRenderMethod, allowFirstRenderPayload: true);
        var onAfterRenderAsyncEmitCall = snapshot.OnAfterRenderAsyncMethod is null
            ? null
            : ExtractSupportedEmitCall(snapshot, snapshot.OnAfterRenderAsyncMethod, allowFirstRenderPayload: true);

        if (onAfterRenderEmitCall is not null)
        {
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.AppendLine("    {");
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.AppendLine("      let firstRender = true;");
            AppendAfterRenderHook(builder, onAfterRenderEmitCall, awaitResult: false);
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.AppendLine("    }");
        }

        if (onAfterRenderAsyncEmitCall is not null)
        {
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.AppendLine("    {");
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.AppendLine("      let firstRender = true;");
            AppendAfterRenderHook(builder, onAfterRenderAsyncEmitCall, awaitResult: true);
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.AppendLine("    }");
        }
    }

    private static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
    {
        var emittedFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var emittedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var fieldBlocks = new List<string>();
        var methodBlocks = new List<string>();
        var helperDepth = 1;

        while (true)
        {
            var nextFields = expressionEmitter.GetRequiredSetupFields()
                .Where(field => !emittedFields.Contains(field.FieldSymbol))
                .OrderBy(static field => field.Name, StringComparer.Ordinal)
                .ToArray();
            var nextMethods = expressionEmitter.GetRequiredSetupMethods()
                .Where(method => !emittedMethods.Contains(method.MethodSymbol))
                .OrderBy(static method => method.Name, StringComparer.Ordinal)
                .ThenBy(static method => method.Arity)
                .ToArray();

            if (nextFields.Length == 0 && nextMethods.Length == 0)
                break;

            if (helperDepth > 2 && nextMethods.Length > 0)
                throw CreateUnsupportedSetupLoweringException(nextMethods[0].MethodSymbol);

            foreach (var field in nextFields)
            {
                emittedFields.Add(field.FieldSymbol);
                fieldBlocks.Add(BuildSetupFieldLowering(snapshot, expressionEmitter, field));
            }

            foreach (var method in nextMethods)
            {
                emittedMethods.Add(method.MethodSymbol);
                methodBlocks.Add(BuildSetupMethodLowering(snapshot, expressionEmitter, method));
            }

            helperDepth++;
        }

        foreach (var fieldBlock in fieldBlocks)
            builder.Append(fieldBlock);

        foreach (var methodBlock in methodBlocks)
            builder.Append(methodBlock);
    }

    private static string BuildSetupFieldLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicFieldDescriptor field)
    {
        if (field.FieldSymbol.DeclaringSyntaxReferences.Length == 0)
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

        var syntax = field.FieldSymbol.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not VariableDeclaratorSyntax declarator || declarator.Initializer is null)
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

        var semanticModel = snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
        var operation = semanticModel.GetOperation(declarator.Initializer.Value);
        if (operation is null)
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

        try
        {
            var expression = expressionEmitter.EmitSetupExpression(operation);
            var fieldBuilder = new StringBuilder();
            fieldBuilder.Append("    ")
                .Append(field.IsReadOnly ? "const " : "let ")
                .Append(ToLowerCamelCase(field.Name))
                .Append(" = ")
                .Append(expression)
                .AppendLine(";");
            return fieldBuilder.ToString();
        }
        catch (NotSupportedException)
        {
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);
        }
    }

    private static string BuildSetupMethodLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicMethodDescriptor method)
    {
        if (method.IsAsync || method.MethodSymbol.DeclaringSyntaxReferences.Length == 0)
            throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

        var syntax = method.MethodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not MethodDeclarationSyntax methodSyntax)
            throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

        ExpressionSyntax expressionSyntax = methodSyntax.ExpressionBody?.Expression
            ?? (methodSyntax.Body?.Statements.Count == 1 && methodSyntax.Body.Statements[0] is ReturnStatementSyntax returnStatement && returnStatement.Expression is not null
                ? returnStatement.Expression
                : throw CreateUnsupportedSetupLoweringException(method.MethodSymbol));

        var semanticModel = snapshot.Compilation.GetSemanticModel(expressionSyntax.SyntaxTree);
        var operation = semanticModel.GetOperation(expressionSyntax);
        if (operation is null)
            throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

        try
        {
            var expression = expressionEmitter.EmitSetupExpression(operation);
            var methodBuilder = new StringBuilder();
            methodBuilder.Append("    function ")
                .Append(ToLowerCamelCase(method.Name))
                .Append('(')
                .Append(string.Join(", ", method.MethodSymbol.Parameters.Select(static parameter => parameter.Name)))
                .AppendLine(") {");
            methodBuilder.Append("      return ")
                .Append(expression)
                .AppendLine(";");
            methodBuilder.AppendLine("    }");
            return methodBuilder.ToString();
        }
        catch (NotSupportedException)
        {
            throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);
        }
    }

    private static bool HasSupportedLifecycleLowering(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return false;

        return ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload) is not null;
    }

    private static string DescribeLifecycleLoweringShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return "none";

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload);
        if (emitCall is null)
            return "none";

        return emitCall.EmitName + "|" + (emitCall.PayloadExpression ?? string.Empty);
    }

    private static void AppendLifecycleHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        string hookName,
        IMethodSymbol? method,
        bool awaitResult)
    {
        if (method is null)
            return;

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
        // No-op lifecycle methods should not materialize empty Vue hooks.
        if (emitCall is null)
            return;

        builder.Append("    ").Append(hookName).Append("(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null);
        builder.AppendLine("    });");
    }

    private static void AppendParametersSetHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method,
        bool awaitResult)
    {
        if (method is null)
            return;

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
        // No-op lifecycle methods should not materialize empty Vue hooks.
        if (emitCall is null)
            return;

        builder.Append("    watch(() => ").Append(BuildPropsWatchSource(snapshot.Descriptor)).Append(", ");
        // Async lifecycle lowering must keep the watch callback async, otherwise
        // generated JavaScript would place await inside a non-async function.
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null);
        builder.AppendLine("    }, { immediate: true });");
    }

    private static void AppendAfterRenderHook(
        StringBuilder builder,
        SupportedEmitCall? emitCall,
        bool awaitResult)
    {
        if (emitCall is null)
            return;

        var snapshotsFirstRender = emitCall.UsesFirstRender;
        var payloadOverride = snapshotsFirstRender
            ? emitCall.PayloadExpression?.Replace(RazorVueExpressionEmitter.LifecycleFirstRenderPlaceholder, "currentFirstRender")
            : null;
        builder.Append("    onMounted(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        if (snapshotsFirstRender)
            builder.AppendLine("      const currentFirstRender = firstRender;");
        if (awaitResult && snapshotsFirstRender)
            builder.AppendLine("      firstRender = false;");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride);
        if (!awaitResult && snapshotsFirstRender)
            builder.AppendLine("      firstRender = false;");
        builder.AppendLine("    });");
        builder.Append("    onUpdated(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        if (snapshotsFirstRender)
            builder.AppendLine("      const currentFirstRender = firstRender;");
        if (awaitResult && snapshotsFirstRender)
            builder.AppendLine("      firstRender = false;");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride);
        if (!awaitResult && snapshotsFirstRender)
            builder.AppendLine("      firstRender = false;");
        builder.AppendLine("    });");
    }

    private static void AppendEmitStatement(
        StringBuilder builder,
        SupportedEmitCall? emitCall,
        bool awaitResult,
        string? payloadOverride)
    {
        if (emitCall is null)
            return;

        var payloadExpression = payloadOverride ?? emitCall.PayloadExpression;
        builder.Append("      ");
        if (awaitResult)
            builder.Append("await ");
        builder.Append("emit(")
            .Append(ToJavaScriptString(emitCall.EmitName));

        if (!string.IsNullOrWhiteSpace(payloadExpression))
            builder.Append(", ").Append(payloadExpression);

        builder.AppendLine(");");
    }

    private static SupportedEmitCall? ExtractSupportedEmitCall(RazorVueSemanticSnapshot snapshot, IMethodSymbol method, bool allowFirstRenderPayload)
    {
        if (method.DeclaringSyntaxReferences.Length == 0)
            throw CreateUnsupportedLifecycleLoweringException(method);

        var reference = method.DeclaringSyntaxReferences[0];
        if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (methodSyntax.ExpressionBody is not null)
            return ExtractSupportedEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload);

        if (methodSyntax.Body is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (methodSyntax.Body.Statements.Count == 0)
            return null;

        if (methodSyntax.Body.Statements.Count == 2 &&
            methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn &&
            (trailingReturn.Expression is null || IsNoOpLifecycleExpression(trailingReturn.Expression)))
        {
            return ExtractSupportedEmitCall(snapshot, method, leadingExpression.Expression, allowFirstRenderPayload);
        }

        if (methodSyntax.Body.Statements.Count != 1)
            throw CreateUnsupportedLifecycleLoweringException(method);

        return methodSyntax.Body.Statements[0] switch
        {
            ExpressionStatementSyntax expressionStatement => ExtractSupportedEmitCall(snapshot, method, expressionStatement.Expression, allowFirstRenderPayload),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null || IsNoOpLifecycleExpression(returnStatement.Expression) => null,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null => ExtractSupportedEmitCall(snapshot, method, returnStatement.Expression, allowFirstRenderPayload),
            _ => throw CreateUnsupportedLifecycleLoweringException(method)
        };
    }

    private static SupportedEmitCall? ExtractSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        ExpressionSyntax expression,
        bool allowFirstRenderPayload)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);

        if (IsNoOpLifecycleExpression(expression))
            return null;

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, "InvokeAsync", StringComparison.Ordinal) ||
            TryGetLifecycleCallbackName(memberAccess.Expression) is not string callbackName)
        {
            throw CreateUnsupportedLifecycleLoweringException(method);
        }

        var emitName = ToLifecycleEmitName(method, callbackName);
        if (invocation.ArgumentList.Arguments.Count == 0)
            return new SupportedEmitCall(emitName, null, false);

        if (invocation.ArgumentList.Arguments.Count != 1)
            throw CreateUnsupportedLifecycleLoweringException(method);

        var payloadSyntax = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[0].Expression);
        var semanticModel = snapshot.Compilation.GetSemanticModel(payloadSyntax.SyntaxTree);
        var payloadOperation = semanticModel.GetOperation(payloadSyntax);
        if (payloadOperation is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        try
        {
            var payload = RazorVueExpressionEmitter.EmitLifecyclePayload(method, payloadOperation, allowFirstRenderPayload);
            return new SupportedEmitCall(emitName, payload.Expression, payload.UsesFirstRender);
        }
        catch (NotSupportedException)
        {
            throw CreateUnsupportedLifecycleLoweringException(method);
        }
    }

    private static bool IsNoOpLifecycleExpression(ExpressionSyntax syntax)
    {
        syntax = UnwrapLifecycleExpression(syntax);
        if (syntax is AwaitExpressionSyntax awaitExpression)
            syntax = UnwrapLifecycleExpression(awaitExpression.Expression);

        var expressionText = syntax.ToString().Trim();
        return string.Equals(expressionText, "Task.CompletedTask", StringComparison.Ordinal) ||
               string.Equals(expressionText, "ValueTask.CompletedTask", StringComparison.Ordinal) ||
               string.Equals(expressionText, "default", StringComparison.Ordinal) ||
               string.Equals(expressionText, "default(ValueTask)", StringComparison.Ordinal) ||
               string.Equals(expressionText, "default(System.Threading.Tasks.ValueTask)", StringComparison.Ordinal);
    }

    private static ExpressionSyntax UnwrapLifecycleExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }

    private static string TranslateLifecyclePayload(
        IMethodSymbol method,
        ExpressionSyntax payloadExpression,
        bool allowFirstRenderPayload)
    {
        switch (payloadExpression)
        {
            case IdentifierNameSyntax identifier:
                if (allowFirstRenderPayload && string.Equals(identifier.Identifier.ValueText, "firstRender", StringComparison.Ordinal))
                    return "firstRender";
                if (HasComponentProperty(method, identifier.Identifier.ValueText))
                    return "props." + ToLowerCamelCase(identifier.Identifier.ValueText);
                break;
            case MemberAccessExpressionSyntax memberAccess when memberAccess.Expression is ThisExpressionSyntax:
                if (HasComponentProperty(method, memberAccess.Name.Identifier.ValueText))
                    return "props." + ToLowerCamelCase(memberAccess.Name.Identifier.ValueText);
                break;
            case LiteralExpressionSyntax:
                return payloadExpression.ToString();
        }

        throw CreateUnsupportedLifecycleLoweringException(method);
    }

    private static bool HasComponentProperty(IMethodSymbol method, string propertyName)
    {
        for (var current = method.ContainingType; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(propertyName)
                .OfType<IPropertySymbol>()
                .Any(static property =>
                    property.GetAttributes().Any(static attribute =>
                        string.Equals(
                            attribute.AttributeClass?.ToDisplayString(),
                            "Microsoft.AspNetCore.Components.ParameterAttribute",
                            StringComparison.Ordinal))))
            {
                return true;
            }
        }

        return false;
    }

    private static string ToLifecycleEmitName(IMethodSymbol method, string callbackName)
    {
        if (callbackName.EndsWith("Changed", StringComparison.Ordinal) && callbackName.Length > "Changed".Length)
        {
            var parameterName = callbackName.Substring(0, callbackName.Length - "Changed".Length);
            if (HasComponentProperty(method, parameterName))
                return "update:" + ToLowerCamelCase(parameterName);
        }

        if (callbackName.StartsWith("On", StringComparison.Ordinal) && callbackName.Length > 2 && char.IsUpper(callbackName[2]))
            return ToLowerCamelCase(callbackName.Substring(2));

        return ToLowerCamelCase(callbackName);
    }

    private static string? TryGetLifecycleCallbackName(ExpressionSyntax expression)
        => expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax identifier } => identifier.Identifier.ValueText,
            _ => null
        };

    private static string BuildPropsWatchSource(VueComponentDescriptor descriptor)
    {
        if (descriptor.Props.IsDefaultOrEmpty)
            return "[]";

        return "[" + string.Join(", ", descriptor.Props.Select(static prop => "props." + prop.Name)) + "]";
    }

    private static RazorVueCompilationIssueException CreateUnsupportedLifecycleLoweringException(IMethodSymbol method)
    {
        var originLocation = method.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedLifecycleLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue lifecycle lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, method.ContainingType.ToDisplayString(), origin);
    }

    private static RazorVueCompilationIssueException CreateUnsupportedSetupLoweringException(ISymbol symbol)
    {
        var originLocation = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue setup lowering does not support member '{symbol.Name}' in component '{symbol.ContainingType?.ToDisplayString() ?? string.Empty}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, symbol.ContainingType?.ToDisplayString() ?? string.Empty, origin);
    }

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

    private sealed record SupportedEmitCall(string EmitName, string? PayloadExpression, bool UsesFirstRender);

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

        return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }

    private static ImmutableArray<string> BuildImports(ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableArray.Create("vue");

        // Host-facing artifacts should carry declared dependency specifiers rather
        // than local alias names generated during lowering.
        return ImmutableArray.Create("vue").AddRange(
            resolvedComponents.Values
                .Select(static descriptor => descriptor.ImportSpecifier)
                .Where(static importSpecifier => !string.Equals(importSpecifier, "vue", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal));
    }

    private static ImmutableArray<string> BuildStyles(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var style in descriptor.StyleDependencies)
        {
            if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
                builder.Add(style);
        }

        foreach (var component in resolvedComponents.Values)
        {
            foreach (var style in component.StyleDependencies)
            {
                if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
                    builder.Add(style);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> BuildPluginRequirements(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var requirement in descriptor.PluginRequirements)
        {
            if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
                builder.Add(requirement);
        }

        foreach (var component in resolvedComponents.Values)
        {
            foreach (var requirement in component.PluginRequirements)
            {
                if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
                    builder.Add(requirement);
            }
        }

        return builder.ToImmutable();
    }

    private static void AppendComponentImports(StringBuilder builder, ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var groups = resolvedComponents
            .Where(static pair => !string.Equals(pair.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .GroupBy(static pair => pair.Value.ImportSpecifier, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            AppendGroupedComponentImports(builder, group.Key, group.ToImmutableArray());
        }
    }

    private static void AppendGroupedComponentImports(
        StringBuilder builder,
        string importSpecifier,
        ImmutableArray<KeyValuePair<string, VueComponentDescriptor>> components)
    {
        var namedImports = components
            .Where(static item => item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
                                  !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
            .Select(static item => item.Value.ExportName + " as " + CreateComponentAlias(item.Key))
            .ToImmutableArray();

        foreach (var item in components)
        {
            if (item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
                !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
            {
                continue;
            }

            AppendDefaultComponentImport(builder, item.Key, importSpecifier);
        }

        if (!namedImports.IsDefaultOrEmpty)
        {
            // Aggregate named library exports from the same package into one import
            // so generated modules stay compact while preserving local aliases.
            builder.Append("import { ");
            builder.Append(string.Join(", ", namedImports));
            builder.Append(" } from ");
            builder.Append(ToJavaScriptString(importSpecifier));
            builder.AppendLine(";");
        }
    }

    private static void AppendDefaultComponentImport(
        StringBuilder builder,
        string componentName,
        string importSpecifier)
    {
        var alias = CreateComponentAlias(componentName);

        builder.Append("import ");
        builder.Append(alias);
        builder.Append(" from ");
        builder.Append(ToJavaScriptString(importSpecifier));
        builder.AppendLine(";");
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

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
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

