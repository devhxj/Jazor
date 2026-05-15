using System.Collections.Immutable;
using System.Text;
using Jazor.Compiler;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueSetupAndLifecycleLoweringSupport
{
    private static readonly ImmutableHashSet<string> SafeLifecycleMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "OnInitialized",
        "OnInitializedAsync",
        "OnParametersSet",
        "OnParametersSetAsync",
        "OnAfterRender",
        "OnAfterRenderAsync");

    public static ImmutableArray<string> CollectVueRuntimeImports(RazorVueSemanticSnapshot snapshot)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var hasInitializedLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                     HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false);
        if (hasInitializedLowering)
            builder.Add("onMounted");

        var hasParametersSetLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                       HasSupportedSetParametersAsyncLowering(snapshot);
        if (hasParametersSetLowering)
            builder.Add("watch");

        var hasAfterRenderLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                     HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
        if (hasAfterRenderLowering)
        {
            builder.Add("onMounted");
            builder.Add("onUpdated");
        }

        var hasDisposeLowering = HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                                 HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);
        if (hasDisposeLowering)
            builder.Add("onUnmounted");

        return builder.Distinct(StringComparer.Ordinal).ToImmutableArray();
    }

    public static void AppendLifecycleLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        string indent)
    {
        AppendLifecycleHook(builder, snapshot, "onMounted", snapshot.OnInitializedMethod, awaitResult: false, indent);
        AppendLifecycleHook(builder, snapshot, "onMounted", snapshot.OnInitializedAsyncMethod, awaitResult: true, indent);
        AppendParametersSetHook(builder, snapshot, snapshot.OnParametersSetMethod, awaitResult: false, indent);
        AppendParametersSetHook(builder, snapshot, snapshot.OnParametersSetAsyncMethod, awaitResult: true, indent);
        AppendSetParametersAsyncHook(builder, snapshot, indent);
        AppendLifecycleHook(builder, snapshot, "onUnmounted", snapshot.DisposeMethod, awaitResult: false, indent);
        AppendLifecycleHook(builder, snapshot, "onUnmounted", snapshot.DisposeAsyncMethod, awaitResult: true, indent);

        var onAfterRenderEmitCall = snapshot.OnAfterRenderMethod is null
            ? null
            : ExtractSupportedEmitCall(snapshot, snapshot.OnAfterRenderMethod, allowFirstRenderPayload: true);
        var onAfterRenderAsyncEmitCall = snapshot.OnAfterRenderAsyncMethod is null
            ? null
            : ExtractSupportedEmitCall(snapshot, snapshot.OnAfterRenderAsyncMethod, allowFirstRenderPayload: true);

        if (onAfterRenderEmitCall is not null)
        {
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("{");
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("  let firstRender = true;");
            AppendAfterRenderHook(builder, onAfterRenderEmitCall, awaitResult: false, indent);
            if (onAfterRenderEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("}");
        }

        if (onAfterRenderAsyncEmitCall is not null)
        {
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("{");
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("  let firstRender = true;");
            AppendAfterRenderHook(builder, onAfterRenderAsyncEmitCall, awaitResult: true, indent);
            if (onAfterRenderAsyncEmitCall.UsesFirstRender)
                builder.Append(indent).AppendLine("}");
        }
    }

    public static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableArray<VueLogicFieldDescriptor> initialRequiredFields,
        ImmutableArray<VueLogicMethodDescriptor> initialRequiredMethods,
        string indent)
    {
        var emittedFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var emittedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var fieldBlocks = new List<string>();
        var methodBlocks = new List<string>();
        var helperDepth = 1;

        while (true)
        {
            var nextFields = initialRequiredFields
                .Concat(expressionEmitter.GetRequiredSetupFields())
                .Where(field => !emittedFields.Contains(field.FieldSymbol))
                .GroupBy(static field => field.FieldSymbol, SymbolEqualityComparer.Default)
                .Select(static group => group.First())
                .OrderBy(static field => field.Name, StringComparer.Ordinal)
                .ToArray();
            var nextMethods = initialRequiredMethods
                .Concat(expressionEmitter.GetRequiredSetupMethods())
                .Where(method => !emittedMethods.Contains(method.MethodSymbol))
                .GroupBy(static method => method.MethodSymbol, SymbolEqualityComparer.Default)
                .Select(static group => group.First())
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
                fieldBlocks.Add(BuildSetupFieldLowering(snapshot, expressionEmitter, field, indent));
            }

            foreach (var method in nextMethods)
            {
                emittedMethods.Add(method.MethodSymbol);
                methodBlocks.Add(BuildSetupMethodLowering(snapshot, expressionEmitter, method, indent));
            }

            helperDepth++;
        }

        foreach (var fieldBlock in fieldBlocks)
            builder.Append(fieldBlock);

        foreach (var methodBlock in methodBlocks)
            builder.Append(methodBlock);
    }

    public static bool HasSupportedLifecycleLowering(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return false;

        return ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload) is not null;
    }

    public static bool HasSupportedSetParametersAsyncLowering(RazorVueSemanticSnapshot snapshot)
        => AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod).EmitCall is not null;

    public static string DescribeLifecycleLoweringShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return "none";

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload);
        if (emitCall is null)
            return "none";

        return emitCall.EmitName + "|" + (emitCall.PayloadExpression ?? string.Empty);
    }

    public static string DescribeSetParametersAsyncShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method)
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

    public static string DescribeShouldRenderShape(Compilation compilation, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        return AnalyzeShouldRender(compilation, method).IsSupported
            ? "true"
            : "unsupported";
    }

    private static string BuildSetupFieldLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicFieldDescriptor field,
        string indent)
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
            fieldBuilder.Append(indent)
                .Append(field.FieldSymbol.IsStatic || field.IsReadOnly ? "const " : "let ")
                .Append(ToLowerCamelCase(field.Name))
                .Append(" = ")
                .Append(expression)
                .AppendLine(";");
            return fieldBuilder.ToString();
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);
        }
    }

    private static string BuildSetupMethodLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicMethodDescriptor method,
        string indent)
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
            var expression = ContainsExplicitParentheses(expressionSyntax)
                ? BuildSetupExpressionPreservingExplicitParentheses(expressionSyntax, semanticModel, expressionEmitter)
                : expressionEmitter.EmitSetupExpression(operation);
            if (RequiresWholeReturnParentheses(expressionSyntax) && !expression.StartsWith("(", StringComparison.Ordinal))
                expression = "(" + expression + ")";
            var normalizedReturnExpression = NormalizeSetupMethodReturnExpression(expression);
            var methodBuilder = new StringBuilder();
            methodBuilder.Append(indent)
                .Append("function ")
                .Append(ToLowerCamelCase(method.Name))
                .Append('(')
                .Append(string.Join(", ", method.MethodSymbol.Parameters.Select(static parameter => parameter.Name)))
                .AppendLine(") {");
            methodBuilder.Append(indent)
                .Append("  return ")
                .Append(normalizedReturnExpression)
                .AppendLine(";");
            methodBuilder.Append(indent).AppendLine("}");
            return methodBuilder.ToString();
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);
        }
    }

    private static void AppendLifecycleHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        string hookName,
        IMethodSymbol? method,
        bool awaitResult,
        string indent)
    {
        if (method is null)
            return;

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
        if (emitCall is null)
            return;

        builder.Append(indent).Append(hookName).Append("(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null, indent + "  ");
        builder.Append(indent).AppendLine("});");
    }

    private static void AppendParametersSetHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method,
        bool awaitResult,
        string indent)
    {
        if (method is null)
            return;

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
        if (emitCall is null)
            return;

        builder.Append(indent).Append("watch(() => ").Append(BuildPropsWatchSource(snapshot.Descriptor)).Append(", ");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null, indent + "  ");
        builder.Append(indent).AppendLine("}, { immediate: true });");
    }

    private static void AppendSetParametersAsyncHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        string indent)
    {
        var analysis = AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod);
        if (!analysis.IsSupported || analysis.EmitCall is null)
            return;

        builder.Append(indent).Append("watch(() => ").Append(BuildPropsWatchSource(snapshot.Descriptor)).Append(", async () => {").AppendLine();
        AppendEmitStatement(builder, analysis.EmitCall, awaitResult: true, payloadOverride: null, indent + "  ");
        builder.Append(indent).AppendLine("}, { immediate: true });");
    }

    private static void AppendAfterRenderHook(
        StringBuilder builder,
        SupportedEmitCall? emitCall,
        bool awaitResult,
        string indent)
    {
        if (emitCall is null)
            return;

        var snapshotsFirstRender = emitCall.UsesFirstRender;
        var payloadOverride = snapshotsFirstRender
            ? emitCall.PayloadExpression?.Replace(RazorVueExpressionEmitter.LifecycleFirstRenderPlaceholder, "currentFirstRender")
            : null;

        builder.Append(indent).Append("onMounted(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        if (snapshotsFirstRender)
            builder.Append(indent).AppendLine("  const currentFirstRender = firstRender;");
        if (awaitResult && snapshotsFirstRender)
            builder.Append(indent).AppendLine("  firstRender = false;");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride, indent + "  ");
        if (!awaitResult && snapshotsFirstRender)
            builder.Append(indent).AppendLine("  firstRender = false;");
        builder.Append(indent).AppendLine("});");

        builder.Append(indent).Append("onUpdated(");
        if (awaitResult)
            builder.Append("async ");
        builder.AppendLine("() => {");
        if (snapshotsFirstRender)
            builder.Append(indent).AppendLine("  const currentFirstRender = firstRender;");
        if (awaitResult && snapshotsFirstRender)
            builder.Append(indent).AppendLine("  firstRender = false;");
        AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride, indent + "  ");
        if (!awaitResult && snapshotsFirstRender)
            builder.Append(indent).AppendLine("  firstRender = false;");
        builder.Append(indent).AppendLine("});");
    }

    private static void AppendEmitStatement(
        StringBuilder builder,
        SupportedEmitCall? emitCall,
        bool awaitResult,
        string? payloadOverride,
        string indent)
    {
        if (emitCall is null)
            return;

        var payloadExpression = payloadOverride ?? emitCall.PayloadExpression;
        builder.Append(indent);
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

    private static string NormalizeSetupMethodReturnExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        if (expression[0] == '(')
            return expression;

        if (char.IsLetter(expression[0]) || expression[0] == '_' || expression[0] == '"' || expression[0] == '\'' || char.IsDigit(expression[0]))
            return expression;

        return "(" + expression + ")";
    }

    private static bool ContainsExplicitParentheses(ExpressionSyntax syntax)
        => syntax.DescendantNodesAndSelf().Any(static node => node is ParenthesizedExpressionSyntax);

    private static string BuildSetupExpressionPreservingExplicitParentheses(
        ExpressionSyntax syntax,
        SemanticModel semanticModel,
        RazorVueExpressionEmitter expressionEmitter)
    {
        switch (syntax)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return "(" + BuildSetupExpressionPreservingExplicitParentheses(parenthesized.Expression, semanticModel, expressionEmitter) + ")";

            case BinaryExpressionSyntax binary:
                return BuildSetupExpressionPreservingExplicitParentheses(binary.Left, semanticModel, expressionEmitter) +
                       " " + MapBinaryOperator(binary.OperatorToken.Kind()) + " " +
                       BuildSetupExpressionPreservingExplicitParentheses(binary.Right, semanticModel, expressionEmitter);

            case InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess:
                return BuildSetupMemberAccessTarget(memberAccess, semanticModel, expressionEmitter) +
                       "(" + string.Join(", ", invocation.ArgumentList.Arguments.Select(argument =>
                           BuildSetupExpressionPreservingExplicitParentheses(argument.Expression, semanticModel, expressionEmitter))) + ")";

            case MemberAccessExpressionSyntax memberAccess:
                return BuildSetupMemberAccessTarget(memberAccess, semanticModel, expressionEmitter);

            default:
                var operation = semanticModel.GetOperation(syntax);
                if (operation is null)
                    throw new NotSupportedException("RazorVue setup expression syntax could not be resolved for parentheses-preserving lowering.");

                return expressionEmitter.EmitSetupExpression(operation);
        }
    }

    private static string BuildSetupMemberAccessTarget(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        RazorVueExpressionEmitter expressionEmitter)
    {
        var receiver = BuildSetupExpressionPreservingExplicitParentheses(memberAccess.Expression, semanticModel, expressionEmitter);
        var memberName = string.Equals(memberAccess.Name.Identifier.ValueText, "ToString", StringComparison.Ordinal)
            ? "toString"
            : memberAccess.Name.Identifier.ValueText;

        if (string.Equals(memberName, "toString", StringComparison.Ordinal) &&
            RequiresAdditionalJsMemberTargetParentheses(memberAccess.Expression))
        {
            receiver = "(" + receiver + ")";
        }

        return receiver + "." + memberName;
    }

    private static bool RequiresAdditionalJsMemberTargetParentheses(ExpressionSyntax syntax)
    {
        syntax = UnwrapParenthesizedSyntax(syntax);
        return syntax is BinaryExpressionSyntax or ConditionalExpressionSyntax;
    }

    private static bool RequiresWholeReturnParentheses(ExpressionSyntax syntax)
    {
        syntax = UnwrapParenthesizedSyntax(syntax);
        return syntax is BinaryExpressionSyntax or ConditionalExpressionSyntax;
    }

    private static ExpressionSyntax UnwrapParenthesizedSyntax(ExpressionSyntax syntax)
    {
        while (syntax is ParenthesizedExpressionSyntax parenthesized)
            syntax = parenthesized.Expression;

        return syntax;
    }

    private static string MapBinaryOperator(SyntaxKind kind)
        => kind switch
        {
            SyntaxKind.PlusToken => "+",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.AsteriskToken => "*",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.PercentToken => "%",
            SyntaxKind.EqualsEqualsToken => "===",
            SyntaxKind.ExclamationEqualsToken => "!==",
            SyntaxKind.LessThanToken => "<",
            SyntaxKind.LessThanEqualsToken => "<=",
            SyntaxKind.GreaterThanToken => ">",
            SyntaxKind.GreaterThanEqualsToken => ">=",
            SyntaxKind.AmpersandAmpersandToken => "&&",
            SyntaxKind.BarBarToken => "||",
            SyntaxKind.AmpersandToken => "&",
            SyntaxKind.BarToken => "|",
            SyntaxKind.CaretToken => "^",
            SyntaxKind.LessThanLessThanToken => "<<",
            SyntaxKind.GreaterThanGreaterThanToken => ">>",
            SyntaxKind.GreaterThanGreaterThanGreaterThanToken => ">>>",
            _ => throw new NotSupportedException($"RazorVue setup lowering does not support explicit-parentheses binary operator '{kind}'.")
        };

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";

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

    private sealed record SupportedEmitCall(string EmitName, string? PayloadExpression, bool UsesFirstRender);
    private sealed record SetParametersAsyncAnalysis(bool IsSupported, SupportedEmitCall? EmitCall);
    private sealed record ShouldRenderAnalysis(bool IsSupported);
}
