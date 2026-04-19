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
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                       HasSupportedSetParametersAsyncLowering(snapshot);
        if (hasParametersSetLowering)
            vueImports.Add("watch");

        var hasAfterRenderLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                     HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        if (hasAfterRenderLowering)
        {
            vueImports.Add("onMounted");
            vueImports.Add("onUpdated");
        }

        var hasDisposeLowering = HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                                 HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);
        if (hasDisposeLowering)
            vueImports.Add("onUnmounted");

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
        AppendSetParametersAsyncHook(builder, snapshot);
        // Dispose bridges to Vue teardown so safe callback-shaped cleanup can
        // participate in the same lifecycle lowering path as other hooks.
        AppendLifecycleHook(builder, snapshot, "onUnmounted", snapshot.DisposeMethod, awaitResult: false);
        AppendLifecycleHook(builder, snapshot, "onUnmounted", snapshot.DisposeAsyncMethod, awaitResult: true);

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

    private static bool HasSupportedSetParametersAsyncLowering(RazorVueSemanticSnapshot snapshot)
        => AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod).EmitCall is not null;

    private static string DescribeLifecycleLoweringShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return "none";

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload);
        if (emitCall is null)
            return "none";

        return emitCall.EmitName + "|" + (emitCall.PayloadExpression ?? string.Empty);
    }

    private static string DescribeSetParametersAsyncShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        var analysis = AnalyzeSetParametersAsync(snapshot, method);
        if (!analysis.IsSupported)
            return "unsupported";

        return analysis.EmitCall is null
            ? "none"
            : analysis.EmitCall.EmitName + "|" + (analysis.EmitCall.PayloadExpression ?? string.Empty);
    }

    private static string DescribeShouldRenderShape(Compilation compilation, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        return AnalyzeShouldRender(compilation, method).IsSupported
            ? "true"
            : "unsupported";
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

    private static void AppendSetParametersAsyncHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
    {
        var analysis = AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod);
        if (!analysis.IsSupported || analysis.EmitCall is null)
            return;

        builder.Append("    watch(() => ").Append(BuildPropsWatchSource(snapshot.Descriptor)).Append(", async () => {").AppendLine();
        AppendEmitStatement(builder, analysis.EmitCall, awaitResult: true, payloadOverride: null);
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
        => ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SupportedEmitCall? ExtractSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        bool allowFirstRenderPayload,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (!visitedMethods.Add(method))
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (method.DeclaringSyntaxReferences.Length == 0)
        {
            // ComponentBase default lifecycle implementations are no-op compatibility
            // shims, so a pure `return base.*(...);` override should not force a reload.
            if (IsDefaultComponentBaseLifecycleMethod(snapshot.Compilation, method))
                return null;

            throw CreateUnsupportedLifecycleLoweringException(method);
        }

        var reference = method.DeclaringSyntaxReferences[0];
        if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (methodSyntax.ExpressionBody is not null)
        {
            if (TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload, visitedMethods, out var baseEmitCall))
                return baseEmitCall;

            return ExtractSupportedEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload);
        }

        if (methodSyntax.Body is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (methodSyntax.Body.Statements.Count == 0)
            return null;

        if (methodSyntax.Body.Statements.Count == 1 &&
            TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.Body.Statements[0], allowFirstRenderPayload, visitedMethods, out var passThroughEmitCall))
        {
            return passThroughEmitCall;
        }

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
        if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
            expression = wrappedExpression;

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

    private static bool TryExtractBaseLifecycleEmitCall(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        StatementSyntax statement,
        bool allowFirstRenderPayload,
        HashSet<IMethodSymbol> visitedMethods,
        out SupportedEmitCall? emitCall)
        => statement switch
        {
            ExpressionStatementSyntax expressionStatement =>
                TryExtractBaseLifecycleEmitCall(snapshot, method, expressionStatement.Expression, allowFirstRenderPayload, visitedMethods, out emitCall),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                TryExtractBaseLifecycleEmitCall(snapshot, method, returnStatement.Expression, allowFirstRenderPayload, visitedMethods, out emitCall),
            _ => ReturnNoBaseLifecycleEmitCall(out emitCall)
        };

    private static bool TryExtractBaseLifecycleEmitCall(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        ExpressionSyntax expression,
        bool allowFirstRenderPayload,
        HashSet<IMethodSymbol> visitedMethods,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        if (!IsBaseLifecyclePassThroughCall(method, expression))
            return false;

        var baseMethod = FindBaseLifecycleMethod(method);
        if (baseMethod is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (baseMethod.DeclaringSyntaxReferences.Length == 0)
        {
            if (IsComponentBaseNoOpLifecycle(snapshot.Compilation, baseMethod))
            {
                emitCall = null;
                return true;
            }

            throw CreateUnsupportedLifecycleLoweringException(method);
        }

        // A pure base pass-through should keep the base lifecycle lowering shape
        // instead of forcing derived components back to full-reload semantics.
        emitCall = ExtractSupportedEmitCall(snapshot, baseMethod, allowFirstRenderPayload, visitedMethods);
        return true;
    }

    private static bool IsBaseLifecyclePassThroughCall(IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
            expression = wrappedExpression;

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression is not BaseExpressionSyntax ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, method.Name, StringComparison.Ordinal) ||
            invocation.ArgumentList.Arguments.Count != method.Parameters.Length)
        {
            return false;
        }

        for (var index = 0; index < method.Parameters.Length; index++)
        {
            var argument = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[index].Expression);
            if (argument is not IdentifierNameSyntax identifier ||
                !string.Equals(identifier.Identifier.ValueText, method.Parameters[index].Name, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method)
        => AnalyzeSetParametersAsync(snapshot, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (method is null || !visitedMethods.Add(method))
            return new SetParametersAsyncAnalysis(false, null);

        if (method.DeclaringSyntaxReferences.Length == 0)
            return new SetParametersAsyncAnalysis(false, null);

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return new SetParametersAsyncAnalysis(false, null);

        if (methodSyntax.ExpressionBody is not null)
        {
            return IsBaseSetParametersAsyncCall(method, methodSyntax.ExpressionBody.Expression)
                ? AnalyzeBaseSetParametersAsync(snapshot, method, visitedMethods)
                : new SetParametersAsyncAnalysis(false, null);
        }

        if (methodSyntax.Body is null)
            return new SetParametersAsyncAnalysis(false, null);

        if (methodSyntax.Body.Statements.Count == 0)
            return new SetParametersAsyncAnalysis(true, null);

        var statements = methodSyntax.Body.Statements;
        var index = 0;
        var sawBaseCall = false;
        SetParametersAsyncAnalysis? baseAnalysis = null;
        if (IsBaseSetParametersAsyncStatement(method, statements[0]))
        {
            sawBaseCall = true;
            baseAnalysis = AnalyzeBaseSetParametersAsync(snapshot, method, visitedMethods);
            if (!baseAnalysis.IsSupported)
                return new SetParametersAsyncAnalysis(false, null);

            index++;
        }

        if (index >= statements.Count)
            return sawBaseCall
                ? baseAnalysis!
                : new SetParametersAsyncAnalysis(true, null);

        if (TryGetSetParametersAsyncNoOpOrEmit(snapshot, method, statements[index], out var emitCall))
        {
            index++;
            if (index == statements.Count)
            {
                if (emitCall is null)
                {
                    return sawBaseCall
                        ? baseAnalysis!
                        : new SetParametersAsyncAnalysis(true, null);
                }

                // RazorVue only has one watch-to-emit lowering shape, so a derived
                // override cannot stack a second emit on top of a base emit contract.
                return sawBaseCall
                    ? baseAnalysis!.EmitCall is null
                        ? new SetParametersAsyncAnalysis(true, emitCall)
                        : new SetParametersAsyncAnalysis(false, null)
                    : new SetParametersAsyncAnalysis(false, null);
            }

            if (index == statements.Count - 1 &&
                IsNoOpSetParametersAsyncStatement(statements[index]))
            {
                if (emitCall is null)
                {
                    return sawBaseCall
                        ? baseAnalysis!
                        : new SetParametersAsyncAnalysis(true, null);
                }

                // RazorVue only has one watch-to-emit lowering shape, so a derived
                // override cannot stack a second emit on top of a base emit contract.
                return sawBaseCall
                    ? baseAnalysis!.EmitCall is null
                        ? new SetParametersAsyncAnalysis(true, emitCall)
                        : new SetParametersAsyncAnalysis(false, null)
                    : new SetParametersAsyncAnalysis(false, null);
            }
        }

        return new SetParametersAsyncAnalysis(false, null);
    }

    private static SetParametersAsyncAnalysis AnalyzeBaseSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        var baseMethod = FindBaseSetParametersAsyncMethod(method);
        if (baseMethod is null || baseMethod.DeclaringSyntaxReferences.Length == 0)
            return new SetParametersAsyncAnalysis(true, null);

        return AnalyzeSetParametersAsync(snapshot, baseMethod, visitedMethods);
    }

    private static ShouldRenderAnalysis AnalyzeShouldRender(Compilation compilation, IMethodSymbol? method)
    {
        if (method is null || method.DeclaringSyntaxReferences.Length == 0)
            return new ShouldRenderAnalysis(false);

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return new ShouldRenderAnalysis(false);

        if (methodSyntax.ExpressionBody is not null)
            return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, methodSyntax.ExpressionBody.Expression));

        if (methodSyntax.Body?.Statements.Count != 1 ||
            methodSyntax.Body.Statements[0] is not ReturnStatementSyntax { Expression: not null } returnStatement)
        {
            return new ShouldRenderAnalysis(false);
        }

        // Constant `true` and direct ComponentBase `base.ShouldRender()` are both
        // explicit spellings of the default reactive render path, so RazorVue can
        // safely treat them as no-op compatibility shims.
        return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, returnStatement.Expression));
    }

    private static ExpressionSyntax UnwrapLifecycleExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }

    private static bool IsBaseSetParametersAsyncCall(IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression is not BaseExpressionSyntax ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, "SetParametersAsync", StringComparison.Ordinal) ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var argument = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[0].Expression);
        return argument is IdentifierNameSyntax identifier &&
               string.Equals(identifier.Identifier.ValueText, method.Parameters[0].Name, StringComparison.Ordinal);
    }

    private static bool IsBaseSetParametersAsyncStatement(IMethodSymbol method, StatementSyntax statement)
        => statement switch
        {
            ExpressionStatementSyntax expressionStatement => IsBaseSetParametersAsyncCall(method, expressionStatement.Expression),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                IsBaseSetParametersAsyncCall(method, returnStatement.Expression),
            _ => false
        };

    private static IMethodSymbol? FindBaseLifecycleMethod(IMethodSymbol method)
    {
        for (var current = method.ContainingType.BaseType; current is not null; current = current.BaseType)
        {
            var candidate = current.GetMembers(method.Name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(member =>
                    !member.IsStatic &&
                    member.Parameters.Length == method.Parameters.Length &&
                    ParametersMatch(member, method));
            if (candidate is not null)
                return candidate;
        }

        return null;
    }

    private static bool IsDefaultComponentBaseLifecycleMethod(Compilation compilation, IMethodSymbol method)
    {
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        return componentBase is not null &&
               SafeLifecycleMethods.Contains(method.Name) &&
               SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, componentBase);
    }

    private static IMethodSymbol? FindBaseSetParametersAsyncMethod(IMethodSymbol method)
    {
        for (var current = method.ContainingType.BaseType; current is not null; current = current.BaseType)
        {
            var candidate = current.GetMembers("SetParametersAsync")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(member =>
                    !member.IsStatic &&
                    member.Parameters.Length == 1 &&
                    SymbolEqualityComparer.Default.Equals(
                        member.Parameters[0].Type.OriginalDefinition,
                        method.Parameters[0].Type.OriginalDefinition));
            if (candidate is not null)
                return candidate;
        }

        return null;
    }

    private static bool ParametersMatch(IMethodSymbol candidate, IMethodSymbol method)
    {
        for (var index = 0; index < method.Parameters.Length; index++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    candidate.Parameters[index].Type.OriginalDefinition,
                    method.Parameters[index].Type.OriginalDefinition))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNoOpSetParametersAsyncStatement(StatementSyntax statement)
        => statement switch
        {
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null => true,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                IsNoOpLifecycleExpression(returnStatement.Expression),
            ExpressionStatementSyntax expressionStatement => IsNoOpLifecycleExpression(expressionStatement.Expression),
            _ => false
        };

    private static bool TryGetSetParametersAsyncNoOpOrEmit(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        StatementSyntax statement,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        switch (statement)
        {
            case ReturnStatementSyntax returnStatement when returnStatement.Expression is null:
                return true;
            case ReturnStatementSyntax returnStatement when returnStatement.Expression is not null:
                if (IsNoOpLifecycleExpression(returnStatement.Expression))
                    return true;

                try
                {
                    emitCall = ExtractSupportedEmitCall(snapshot, method, returnStatement.Expression, allowFirstRenderPayload: false);
                    return emitCall is not null;
                }
                catch (RazorVueCompilationIssueException)
                {
                    return false;
                }
            case ExpressionStatementSyntax expressionStatement:
                if (IsNoOpLifecycleExpression(expressionStatement.Expression))
                    return true;

                try
                {
                    emitCall = ExtractSupportedEmitCall(snapshot, method, expressionStatement.Expression, allowFirstRenderPayload: false);
                    return emitCall is not null;
                }
                catch (RazorVueCompilationIssueException)
                {
                    return false;
                }
            default:
                return false;
        }
    }

    private static bool TryUnwrapValueTaskCreation(ExpressionSyntax expression, out ExpressionSyntax innerExpression)
    {
        innerExpression = expression;
        if (expression is not ObjectCreationExpressionSyntax creation ||
            creation.ArgumentList?.Arguments.Count != 1)
        {
            return false;
        }

        var typeName = creation.Type.ToString();
        if (!string.Equals(typeName, "ValueTask", StringComparison.Ordinal) &&
            !string.Equals(typeName, "System.Threading.Tasks.ValueTask", StringComparison.Ordinal))
        {
            return false;
        }

        innerExpression = UnwrapLifecycleExpression(creation.ArgumentList.Arguments[0].Expression);
        return true;
    }

    private static bool ReturnNoBaseLifecycleEmitCall(out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        return false;
    }

    private static bool IsConstantTrueShouldRenderExpression(ExpressionSyntax expression)
    {
        expression = UnwrapLifecycleExpression(expression);
        return expression.IsKind(SyntaxKind.TrueLiteralExpression);
    }

    private static bool IsSupportedShouldRenderExpression(Compilation compilation, ExpressionSyntax expression)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (IsConstantTrueShouldRenderExpression(expression))
            return true;

        if (expression is not InvocationExpressionSyntax invocationExpression ||
            invocationExpression.Expression is not MemberAccessExpressionSyntax
            {
                Expression: BaseExpressionSyntax,
                Name.Identifier.ValueText: "ShouldRender"
            } ||
            invocationExpression.ArgumentList.Arguments.Count != 0)
        {
            return false;
        }

        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        if (componentBase is null)
            return false;

        var semanticModel = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
        return semanticModel.GetOperation(invocationExpression) is IInvocationOperation invocation &&
               SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType.OriginalDefinition, componentBase);
    }

    private static bool IsComponentBaseNoOpLifecycle(Compilation compilation, IMethodSymbol method)
    {
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        return componentBase is not null &&
               SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, componentBase) &&
               method.Name is "OnInitialized" or "OnInitializedAsync" or "OnParametersSet" or "OnParametersSetAsync" or "OnAfterRender" or "OnAfterRenderAsync";
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
    private sealed record SetParametersAsyncAnalysis(bool IsSupported, SupportedEmitCall? EmitCall);
    private sealed record ShouldRenderAnalysis(bool IsSupported);

    private static string FormatStringArray(IEnumerable<string> values)
        => "[" + string.Join(", ", values.Select(ToJavaScriptString)) + "]";
}
