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
        => AppendLifecyclePlan(builder, CreateLifecyclePlan(snapshot, expressionEmitter: null), indent);

    public static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableArray<VueLogicPropertyDescriptor> initialRequiredProperties,
        ImmutableArray<VueLogicFieldDescriptor> initialRequiredFields,
        ImmutableArray<VueLogicMethodDescriptor> initialRequiredMethods,
        string indent)
    {
        var propertyMap = new Dictionary<IPropertySymbol, VueLogicPropertyDescriptor>(SymbolEqualityComparer.Default);
        foreach (var property in snapshot.Logic.Properties)
            propertyMap[property.PropertySymbol] = property;

        var fieldMap = new Dictionary<IFieldSymbol, VueLogicFieldDescriptor>(SymbolEqualityComparer.Default);
        foreach (var field in snapshot.Logic.Fields)
            fieldMap[field.FieldSymbol] = field;

        var methodMap = new Dictionary<IMethodSymbol, VueLogicMethodDescriptor>(SymbolEqualityComparer.Default);
        foreach (var method in snapshot.Logic.Methods)
            methodMap[method.MethodSymbol] = method;

        var propertyCache = new Dictionary<IPropertySymbol, SetupMemberLoweringResult>(SymbolEqualityComparer.Default);
        var fieldCache = new Dictionary<IFieldSymbol, SetupMemberLoweringResult>(SymbolEqualityComparer.Default);
        var methodCache = new Dictionary<IMethodSymbol, SetupMemberLoweringResult>(SymbolEqualityComparer.Default);
        var emittedProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        var emittedFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var emittedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var activeProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        var activeFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var activeMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var propertyBlocks = new List<string>();
        var fieldBlocks = new List<string>();
        var methodBlocks = new List<string>();

        foreach (var property in initialRequiredProperties
                     .Concat(expressionEmitter.GetRequiredSetupProperties())
                     .GroupBy(static item => item.PropertySymbol, SymbolEqualityComparer.Default)
                     .Select(static group => group.First())
                     .OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            EmitProperty(property);
        }

        foreach (var field in initialRequiredFields
                     .Concat(expressionEmitter.GetRequiredSetupFields())
                     .GroupBy(static item => item.FieldSymbol, SymbolEqualityComparer.Default)
                     .Select(static group => group.First())
                     .OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            EmitField(field);
        }

        foreach (var method in initialRequiredMethods
                     .Concat(expressionEmitter.GetRequiredSetupMethods())
                     .GroupBy(static item => item.MethodSymbol, SymbolEqualityComparer.Default)
                     .Select(static group => group.First())
                     .OrderBy(static item => item.Name, StringComparer.Ordinal)
                     .ThenBy(static item => item.Arity))
        {
            EmitMethod(method);
        }

        foreach (var propertyBlock in propertyBlocks)
            builder.Append(propertyBlock);

        foreach (var fieldBlock in fieldBlocks)
            builder.Append(fieldBlock);

        foreach (var methodBlock in methodBlocks)
            builder.Append(methodBlock);

        void EmitProperty(VueLogicPropertyDescriptor property)
        {
            if (!activeProperties.Add(property.PropertySymbol))
                throw CreateSetupCycleException(property.PropertySymbol);

            try
            {
                var result = GetPropertyLowering(property);
                foreach (var dependency in result.PropertyDependencies)
                {
                    if (propertyMap.TryGetValue(dependency, out var dependencyProperty))
                        EmitProperty(dependencyProperty);
                }

                foreach (var dependency in result.FieldDependencies)
                {
                    if (fieldMap.TryGetValue(dependency, out var dependencyField))
                        EmitField(dependencyField);
                }

                foreach (var dependency in result.MethodDependencies)
                {
                    if (methodMap.TryGetValue(dependency, out var dependencyMethod))
                        EmitMethod(dependencyMethod);
                }

                if (emittedProperties.Add(property.PropertySymbol))
                    propertyBlocks.Add(result.Block);
            }
            finally
            {
                activeProperties.Remove(property.PropertySymbol);
            }
        }

        void EmitField(VueLogicFieldDescriptor field)
        {
            if (!activeFields.Add(field.FieldSymbol))
                throw CreateSetupCycleException(field.FieldSymbol);

            try
            {
                var result = GetFieldLowering(field);
                foreach (var dependency in result.PropertyDependencies)
                {
                    if (propertyMap.TryGetValue(dependency, out var dependencyProperty))
                        EmitProperty(dependencyProperty);
                }

                foreach (var dependency in result.FieldDependencies)
                {
                    if (fieldMap.TryGetValue(dependency, out var dependencyField))
                        EmitField(dependencyField);
                }

                foreach (var dependency in result.MethodDependencies)
                {
                    if (methodMap.TryGetValue(dependency, out var dependencyMethod))
                        EmitMethod(dependencyMethod);
                }

                if (emittedFields.Add(field.FieldSymbol))
                    fieldBlocks.Add(result.Block);
            }
            finally
            {
                activeFields.Remove(field.FieldSymbol);
            }
        }

        void EmitMethod(VueLogicMethodDescriptor method)
        {
            if (!activeMethods.Add(method.MethodSymbol))
                throw CreateSetupCycleException(method.MethodSymbol);

            try
            {
                var result = GetMethodLowering(method);
                foreach (var dependency in result.PropertyDependencies)
                {
                    if (propertyMap.TryGetValue(dependency, out var dependencyProperty))
                        EmitProperty(dependencyProperty);
                }

                foreach (var dependency in result.FieldDependencies)
                {
                    if (fieldMap.TryGetValue(dependency, out var dependencyField))
                        EmitField(dependencyField);
                }

                foreach (var dependency in result.MethodDependencies)
                {
                    if (methodMap.TryGetValue(dependency, out var dependencyMethod))
                        EmitMethod(dependencyMethod);
                }

                if (emittedMethods.Add(method.MethodSymbol))
                    methodBlocks.Add(result.Block);
            }
            finally
            {
                activeMethods.Remove(method.MethodSymbol);
            }
        }

        SetupMemberLoweringResult GetPropertyLowering(VueLogicPropertyDescriptor property)
        {
            if (propertyCache.TryGetValue(property.PropertySymbol, out var cached))
                return cached;

            var created = BuildSetupPropertyLowering(snapshot, expressionEmitter, property, indent);
            propertyCache[property.PropertySymbol] = created;
            return created;
        }

        SetupMemberLoweringResult GetFieldLowering(VueLogicFieldDescriptor field)
        {
            if (fieldCache.TryGetValue(field.FieldSymbol, out var cached))
                return cached;

            var created = BuildSetupFieldLowering(snapshot, expressionEmitter, field, indent);
            fieldCache[field.FieldSymbol] = created;
            return created;
        }

        SetupMemberLoweringResult GetMethodLowering(VueLogicMethodDescriptor method)
        {
            if (methodCache.TryGetValue(method.MethodSymbol, out var cached))
                return cached;

            var created = BuildSetupMethodLowering(snapshot, expressionEmitter, method, indent);
            methodCache[method.MethodSymbol] = created;
            return created;
        }
    }

    private static SetupMemberLoweringResult BuildSetupPropertyLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicPropertyDescriptor property,
        string indent)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedValueMemberReason(snapshot.Compilation, property.PropertySymbol, out var propertyReason))
        {
            throw CreateUnsupportedSetupLoweringException(property.PropertySymbol, propertyReason);
        }

        if (property.LoweringKind == VueLogicPropertyLoweringKind.Unsupported)
            throw CreateUnsupportedSetupLoweringException(property.PropertySymbol);

        try
        {
            if (!TryGetPropertyExpressionOperation(snapshot, property.PropertySymbol, out var operation))
                throw CreateUnsupportedSetupLoweringException(property.PropertySymbol);

            var capture = expressionEmitter.CaptureSetupDependencies(() => expressionEmitter.EmitSetupExpression(operation));
            var propertyBuilder = new StringBuilder();
            if (property.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding)
            {
                propertyBuilder.Append(indent)
                    .Append(property.PropertySymbol.IsStatic || property.IsReadOnly ? "const " : "let ")
                    .Append(ToLowerCamelCase(property.Name))
                    .Append(" = ")
                    .Append(capture.Expression)
                    .AppendLine(";");
            }
            else
            {
                propertyBuilder.Append(indent)
                    .Append("function ")
                    .Append(ToLowerCamelCase(property.Name))
                    .AppendLine("() {");
                propertyBuilder.Append(indent)
                    .Append("  return ")
                    .Append(NormalizeSetupMethodReturnExpression(capture.Expression))
                    .AppendLine(";");
                propertyBuilder.Append(indent).AppendLine("}");
            }

            return new SetupMemberLoweringResult(
                propertyBuilder.ToString(),
                capture.PropertyDependencies,
                capture.FieldDependencies,
                capture.MethodDependencies);
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            throw CreateUnsupportedSetupLoweringException(property.PropertySymbol);
        }
    }

    private static bool TryGetPropertyExpressionOperation(
        RazorVueSemanticSnapshot snapshot,
        IPropertySymbol property,
        out IOperation operation)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(
                snapshot.Compilation,
                property,
                out var valueInitializer) &&
            valueInitializer is not null)
        {
            operation = valueInitializer;
            return true;
        }

        operation = default!;
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.Initializer?.Value is not null)
                continue;

            var semanticModel = snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation) &&
                RazorVueOperationNormalizer.Unwrap(propertyOperation) is { } initializer)
            {
                operation = initializer;
                return true;
            }
        }

        return false;
    }

    public static LifecycleLoweringPlan CreateLifecyclePlan(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter)
    {
        var requiredProperties = ImmutableArray<VueLogicPropertyDescriptor>.Empty;
        var requiredFields = ImmutableArray<VueLogicFieldDescriptor>.Empty;
        var requiredMethods = ImmutableArray<VueLogicMethodDescriptor>.Empty;

        if (expressionEmitter is not null)
        {
            _ = TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedMethod, awaitResult: false, allowFirstRenderPayload: false);
            _ = TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedAsyncMethod, awaitResult: true, allowFirstRenderPayload: false);
            _ = TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetMethod, awaitResult: false);
            _ = TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetAsyncMethod, awaitResult: true);
            _ = TryPlanSetParametersAsyncHook(snapshot, expressionEmitter);
            _ = TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeMethod, awaitResult: false, allowFirstRenderPayload: false);
            _ = TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeAsyncMethod, awaitResult: true, allowFirstRenderPayload: false);
            _ = TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderMethod, awaitResult: false);
            _ = TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderAsyncMethod, awaitResult: true);

            requiredProperties = expressionEmitter.GetRequiredSetupProperties();
            requiredFields = expressionEmitter.GetRequiredSetupFields();
            requiredMethods = expressionEmitter.GetRequiredSetupMethods();
        }

        return new LifecycleLoweringPlan(
            TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedMethod, awaitResult: false, allowFirstRenderPayload: false),
            TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedAsyncMethod, awaitResult: true, allowFirstRenderPayload: false),
            TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetMethod, awaitResult: false),
            TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetAsyncMethod, awaitResult: true),
            TryPlanSetParametersAsyncHook(snapshot, expressionEmitter),
            TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeMethod, awaitResult: false, allowFirstRenderPayload: false),
            TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeAsyncMethod, awaitResult: true, allowFirstRenderPayload: false),
            TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderMethod, awaitResult: false),
            TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderAsyncMethod, awaitResult: true),
            requiredProperties,
            requiredFields,
            requiredMethods);
    }

    public static bool HasSupportedLifecycleLowering(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return false;

        return ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload) is not null;
    }

    public static bool HasSupportedSetParametersAsyncLowering(RazorVueSemanticSnapshot snapshot)
        => AnalyzeSetParametersAsync(snapshot, expressionEmitter: null, snapshot.SetParametersAsyncMethod).EmitCall is not null;

    public static string DescribeLifecycleSupportShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return "none";

        try
        {
            var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload);
            return emitCall is null
                ? "none"
                : DescribeEmitCallShape(emitCall);
        }
        catch (RazorVueCompilationIssueException issue) when (issue.Issue.Code == RazorVueIssueCode.UnsupportedLifecycleLowering)
        {
            return "unsupported";
        }
    }

    public static string DescribeLifecycleLoweringShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method, bool allowFirstRenderPayload)
    {
        if (method is null)
            return "none";

        var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload);
        if (emitCall is null)
            return "none";

        return DescribeEmitCallShape(emitCall);
    }

    public static string DescribeSetParametersAsyncShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        var analysis = AnalyzeSetParametersAsync(snapshot, expressionEmitter: null, method);
        if (!analysis.IsSupported)
            return "unsupported";

        return analysis.EmitCall is null
            ? "none"
            : DescribeEmitCallShape(analysis.EmitCall);
    }

    public static string DescribeShouldRenderShape(Compilation compilation, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        return AnalyzeShouldRender(compilation, method).IsSupported
            ? "true"
            : "unsupported";
    }

    private static SetupMemberLoweringResult BuildSetupFieldLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicFieldDescriptor field,
        string indent)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedValueMemberReason(snapshot.Compilation, field.FieldSymbol, out var fieldReason))
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol, fieldReason);

        if (!RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(
                snapshot.Compilation,
                field.FieldSymbol,
                out var operation) ||
            operation is null)
        {
            throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);
        }

        try
        {
            var capture = expressionEmitter.CaptureSetupDependencies(() => expressionEmitter.EmitSetupExpression(operation));
            var fieldBuilder = new StringBuilder();
            fieldBuilder.Append(indent)
                .Append(field.FieldSymbol.IsStatic || field.IsReadOnly ? "const " : "let ")
                .Append(ToLowerCamelCase(field.Name))
                .Append(" = ")
                .Append(capture.Expression)
                .AppendLine(";");
            return new SetupMemberLoweringResult(
                fieldBuilder.ToString(),
                capture.PropertyDependencies,
                capture.FieldDependencies,
                capture.MethodDependencies);
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

    private static SetupMemberLoweringResult BuildSetupMethodLowering(
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
            var parameterAliases = method.MethodSymbol.Parameters
                .Select(static parameter => parameter.Name)
                .ToArray();
            var capture = expressionEmitter.CaptureSetupDependenciesWithParameterAliases(
                method.MethodSymbol.Parameters,
                parameterAliases,
                () => ContainsExplicitParentheses(expressionSyntax)
                    ? BuildSetupExpressionPreservingExplicitParentheses(expressionSyntax, semanticModel, expressionEmitter)
                    : expressionEmitter.EmitSetupExpression(operation));
            var expression = capture.Expression;
            if (RequiresWholeReturnParentheses(expressionSyntax) && !expression.StartsWith("(", StringComparison.Ordinal))
                expression = "(" + expression + ")";
            var normalizedReturnExpression = NormalizeSetupMethodReturnExpression(expression);
            var methodBuilder = new StringBuilder();
            methodBuilder.Append(indent)
                .Append("function ")
                .Append(ToLowerCamelCase(method.Name))
                .Append('(')
                .Append(string.Join(", ", parameterAliases))
                .AppendLine(") {");
            methodBuilder.Append(indent)
                .Append("  return ")
                .Append(normalizedReturnExpression)
                .AppendLine(";");
            methodBuilder.Append(indent).AppendLine("}");
            return new SetupMemberLoweringResult(
                methodBuilder.ToString(),
                capture.PropertyDependencies,
                capture.FieldDependencies,
                capture.MethodDependencies);
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
        AppendLifecycleHook(builder, TryPlanLifecycleHook(snapshot, expressionEmitter: null, hookName, method, awaitResult, allowFirstRenderPayload: false), indent);
    }

    private static void AppendParametersSetHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method,
        bool awaitResult,
        string indent)
    {
        AppendLifecycleHook(builder, TryPlanParametersSetHook(snapshot, expressionEmitter: null, method, awaitResult), indent);
    }

    private static void AppendSetParametersAsyncHook(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        string indent)
    {
        AppendLifecycleHook(builder, TryPlanSetParametersAsyncHook(snapshot, expressionEmitter: null), indent);
    }

    private static void AppendAfterRenderHook(
        StringBuilder builder,
        SupportedEmitCall? emitCall,
        bool awaitResult,
        string indent)
    {
        AppendLifecycleHook(builder, emitCall is null ? null : CreateAfterRenderHookPlan(emitCall, awaitResult), indent);
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

        AppendEmitPrelude(builder, emitCall, indent);
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

    private static void AppendEmitPrelude(
        StringBuilder builder,
        SupportedEmitCall emitCall,
        string indent)
    {
        if (emitCall.PreludeLocals.IsDefaultOrEmpty)
            return;

        foreach (var local in emitCall.PreludeLocals)
            builder.Append(indent).Append("const ").Append(local.Alias).Append(" = ").Append(local.Expression).AppendLine(";");
    }

    private static LifecycleHookPlan? TryPlanLifecycleHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        string hookName,
        IMethodSymbol? method,
        bool awaitResult,
        bool allowFirstRenderPayload)
    {
        if (method is null)
            return null;

        var emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, allowFirstRenderPayload);
        if (emitCall is null)
            return null;

        return new LifecycleHookPlan(
            HookKind: LifecycleHookKind.Standard,
            HookName: hookName,
            AwaitResult: awaitResult,
            EmitCall: emitCall,
            UsesImmediateWatch: false,
            WatchSource: string.Empty);
    }

    private static LifecycleHookPlan? TryPlanParametersSetHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol? method,
        bool awaitResult)
    {
        if (method is null)
            return null;

        var emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, allowFirstRenderPayload: false);
        if (emitCall is null)
            return null;

        return new LifecycleHookPlan(
            HookKind: LifecycleHookKind.ImmediateWatch,
            HookName: "watch",
            AwaitResult: awaitResult,
            EmitCall: emitCall,
            UsesImmediateWatch: true,
            WatchSource: BuildPropsWatchSource(snapshot.Descriptor));
    }

    private static LifecycleHookPlan? TryPlanSetParametersAsyncHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter)
    {
        var analysis = AnalyzeSetParametersAsync(snapshot, expressionEmitter, snapshot.SetParametersAsyncMethod);
        if (!analysis.IsSupported || analysis.EmitCall is null)
            return null;

        return new LifecycleHookPlan(
            HookKind: LifecycleHookKind.ImmediateWatch,
            HookName: "watch",
            AwaitResult: true,
            EmitCall: analysis.EmitCall,
            UsesImmediateWatch: true,
            WatchSource: BuildPropsWatchSource(snapshot.Descriptor));
    }

    private static LifecycleHookPlan? TryPlanAfterRenderHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol? method,
        bool awaitResult)
    {
        if (method is null)
            return null;

        var emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, allowFirstRenderPayload: true);
        return emitCall is null ? null : CreateAfterRenderHookPlan(emitCall, awaitResult);
    }

    private static LifecycleHookPlan CreateAfterRenderHookPlan(
        SupportedEmitCall emitCall,
        bool awaitResult)
        => new(
            HookKind: LifecycleHookKind.AfterRender,
            HookName: "onAfterRender",
            AwaitResult: awaitResult,
            EmitCall: emitCall,
            UsesImmediateWatch: false,
            WatchSource: string.Empty);

    internal static void AppendLifecyclePlan(
        StringBuilder builder,
        LifecycleLoweringPlan plan,
        string indent)
    {
        AppendLifecycleHook(builder, plan.OnInitialized, indent);
        AppendLifecycleHook(builder, plan.OnInitializedAsync, indent);
        AppendLifecycleHook(builder, plan.OnParametersSet, indent);
        AppendLifecycleHook(builder, plan.OnParametersSetAsync, indent);
        AppendLifecycleHook(builder, plan.SetParametersAsync, indent);
        AppendLifecycleHook(builder, plan.Dispose, indent);
        AppendLifecycleHook(builder, plan.DisposeAsync, indent);
        AppendLifecycleHook(builder, plan.OnAfterRender, indent);
        AppendLifecycleHook(builder, plan.OnAfterRenderAsync, indent);
    }

    private static void AppendLifecycleHook(
        StringBuilder builder,
        LifecycleHookPlan? plan,
        string indent)
    {
        if (plan is not { } hookPlan || hookPlan.EmitCall is null)
            return;

        var emitCall = hookPlan.EmitCall;

        switch (hookPlan.HookKind)
        {
            case LifecycleHookKind.Standard:
                builder.Append(indent).Append(hookPlan.HookName).Append("(");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendEmitStatement(builder, emitCall, hookPlan.AwaitResult, payloadOverride: null, indent + "  ");
                builder.Append(indent).AppendLine("});");
                return;

            case LifecycleHookKind.ImmediateWatch:
                builder.Append(indent).Append("watch(() => ").Append(hookPlan.WatchSource).Append(", ");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendEmitStatement(builder, emitCall, hookPlan.AwaitResult, payloadOverride: null, indent + "  ");
                builder.Append(indent).AppendLine("}, { immediate: true });");
                return;

            case LifecycleHookKind.AfterRender:
                var snapshotsFirstRender = emitCall.UsesFirstRender;
                var payloadOverride = snapshotsFirstRender
                    ? emitCall.PayloadExpression?.Replace(RazorVueExpressionEmitter.LifecycleFirstRenderPlaceholder, "currentFirstRender")
                    : null;

                if (snapshotsFirstRender)
                    builder.Append(indent).AppendLine("{");
                if (snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  let firstRender = true;");

                builder.Append(indent).Append("onMounted(");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                if (snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  const currentFirstRender = firstRender;");
                if (hookPlan.AwaitResult && snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  firstRender = false;");
                AppendEmitStatement(builder, emitCall, hookPlan.AwaitResult, payloadOverride, indent + "  ");
                if (!hookPlan.AwaitResult && snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  firstRender = false;");
                builder.Append(indent).AppendLine("});");

                builder.Append(indent).Append("onUpdated(");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                if (snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  const currentFirstRender = firstRender;");
                if (hookPlan.AwaitResult && snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  firstRender = false;");
                AppendEmitStatement(builder, emitCall, hookPlan.AwaitResult, payloadOverride, indent + "  ");
                if (!hookPlan.AwaitResult && snapshotsFirstRender)
                    builder.Append(indent).AppendLine("  firstRender = false;");
                builder.Append(indent).AppendLine("});");

                if (snapshotsFirstRender)
                    builder.Append(indent).AppendLine("}");
                return;
        }
    }

    private static SupportedEmitCall? ExtractSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        bool allowFirstRenderPayload)
        => ExtractSupportedEmitCall(snapshot, expressionEmitter, method, allowFirstRenderPayload, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SupportedEmitCall? ExtractSupportedEmitCall(RazorVueSemanticSnapshot snapshot, IMethodSymbol method, bool allowFirstRenderPayload)
        => ExtractSupportedEmitCall(snapshot, expressionEmitter: null, method, allowFirstRenderPayload, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SupportedEmitCall? ExtractSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
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
            if (TryExtractBaseLifecycleEmitCall(snapshot, expressionEmitter, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload, visitedMethods, out var baseEmitCall))
                return baseEmitCall;

            return ExtractSupportedEmitCall(snapshot, expressionEmitter, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload);
        }

        if (methodSyntax.Body is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        if (methodSyntax.Body.Statements.Count == 0)
            return null;

        if (methodSyntax.Body.Statements.Count == 1 &&
            TryExtractBaseLifecycleEmitCall(snapshot, expressionEmitter, method, methodSyntax.Body.Statements[0], allowFirstRenderPayload, visitedMethods, out var passThroughEmitCall))
        {
            return passThroughEmitCall;
        }

        if (methodSyntax.Body.Statements.Count == 2 &&
            TryExtractBaseLifecycleEmitCall(snapshot, expressionEmitter, method, methodSyntax.Body.Statements[0], allowFirstRenderPayload, visitedMethods, out var passThroughWithTrailingNoOpEmitCall) &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingNoOpReturn &&
            (trailingNoOpReturn.Expression is null || IsNoOpLifecycleExpression(snapshot.Compilation, method, trailingNoOpReturn.Expression)))
        {
            return passThroughWithTrailingNoOpEmitCall;
        }

        if (methodSyntax.Body.Statements.Count == 2 &&
            methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn &&
            (trailingReturn.Expression is null || IsNoOpLifecycleExpression(snapshot.Compilation, method, trailingReturn.Expression)))
        {
            return ExtractSupportedEmitCall(snapshot, expressionEmitter, method, leadingExpression.Expression, allowFirstRenderPayload);
        }

        if (TryExtractSupportedLocalPrefixedLifecycleEmitCall(
                snapshot,
                expressionEmitter,
                method,
                methodSyntax.Body.Statements,
                allowFirstRenderPayload,
                out var localPrefixedEmitCall))
        {
            return localPrefixedEmitCall;
        }

        if (methodSyntax.Body.Statements.Count != 1)
            throw CreateUnsupportedLifecycleLoweringException(method);

        return methodSyntax.Body.Statements[0] switch
        {
            ExpressionStatementSyntax expressionStatement => ExtractSupportedEmitCall(snapshot, expressionEmitter, method, expressionStatement.Expression, allowFirstRenderPayload),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null || IsNoOpLifecycleExpression(snapshot.Compilation, method, returnStatement.Expression) => null,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null => ExtractSupportedEmitCall(snapshot, expressionEmitter, method, returnStatement.Expression, allowFirstRenderPayload),
            _ => throw CreateUnsupportedLifecycleLoweringException(method)
        };
    }

    private static bool TryExtractSupportedLocalPrefixedLifecycleEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SyntaxList<StatementSyntax> statements,
        bool allowFirstRenderPayload,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        if (statements.Count < 2)
            return false;

        for (var index = 0; index < statements.Count - 1; index++)
        {
            if (statements[index] is not LocalDeclarationStatementSyntax)
                return false;
        }

        var lastStatement = statements[statements.Count - 1];
        ExpressionSyntax? emitExpression = lastStatement switch
        {
            ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null => returnStatement.Expression,
            _ => null
        };
        if (emitExpression is null)
            return false;

        var semanticModel = snapshot.Compilation.GetSemanticModel(lastStatement.SyntaxTree);
        if (semanticModel.GetOperation(method.DeclaringSyntaxReferences[0].GetSyntax()) is not IMethodBodyOperation methodBodyOperation)
            return false;

        var localInitializers = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
            snapshot.Compilation,
            methodBodyOperation.BlockBody?.Operations ?? ImmutableArray<IOperation>.Empty,
            RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);

        foreach (var declaration in statements.Take(statements.Count - 1).OfType<LocalDeclarationStatementSyntax>())
        {
            foreach (var variable in declaration.Declaration.Variables)
            {
                if (semanticModel.GetDeclaredSymbol(variable) is not ILocalSymbol local ||
                    !localInitializers.ContainsKey(local))
                {
                    return false;
                }
            }
        }

        emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, emitExpression, allowFirstRenderPayload);
        return true;
    }

    private static SupportedEmitCall? ExtractSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        ExpressionSyntax expression,
        bool allowFirstRenderPayload)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (IsNoOpLifecycleExpression(snapshot.Compilation, method, expression))
            return null;

        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(snapshot.Compilation, expression, out var wrappedExpression))
            expression = wrappedExpression;

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, "InvokeAsync", StringComparison.Ordinal) ||
            TryGetLifecycleCallbackName(memberAccess.Expression) is not string callbackName)
        {
            throw CreateUnsupportedLifecycleLoweringException(method);
        }

        var emitName = ToLifecycleEmitName(method, callbackName);
        if (invocation.ArgumentList.Arguments.Count == 0)
            return new SupportedEmitCall(emitName, null, false, ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadLocalBinding>.Empty);

        if (invocation.ArgumentList.Arguments.Count != 1)
            throw CreateUnsupportedLifecycleLoweringException(method);

        var payloadSyntax = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[0].Expression);
        var semanticModel = snapshot.Compilation.GetSemanticModel(payloadSyntax.SyntaxTree);
        var payloadOperation = semanticModel.GetOperation(payloadSyntax);
        if (payloadOperation is null)
            throw CreateUnsupportedLifecycleLoweringException(method);

        try
        {
            var payload = expressionEmitter is null
                ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, payloadOperation, allowFirstRenderPayload)
                : expressionEmitter.EmitLifecyclePayload(method, payloadOperation, allowFirstRenderPayload);
            return new SupportedEmitCall(emitName, payload.Expression, payload.UsesFirstRender, payload.PreludeLocals);
        }
        catch (NotSupportedException)
        {
            throw CreateUnsupportedLifecycleLoweringException(method);
        }
    }

    private static bool IsNoOpLifecycleExpression(Compilation compilation, IMethodSymbol method, ExpressionSyntax syntax)
    {
        return IsNoOpAwaitableExpression(
            compilation,
            syntax,
            allowBareDefaultLiteral: IsNonGenericValueTaskType(compilation, method.ReturnType));
    }

    private static bool IsNoOpAwaitableExpression(
        Compilation compilation,
        ExpressionSyntax syntax,
        bool allowBareDefaultLiteral)
    {
        syntax = UnwrapLifecycleExpression(syntax);
        if (syntax is AwaitExpressionSyntax awaitExpression)
            return IsNoOpAwaitableExpression(compilation, awaitExpression.Expression, allowBareDefaultLiteral);

        if (TryUnwrapValueTaskCreation(compilation, syntax, out var wrappedExpression))
            return IsNoOpAwaitableExpression(compilation, wrappedExpression, allowBareDefaultLiteral: true);

        if (syntax.IsKind(SyntaxKind.DefaultLiteralExpression))
            return allowBareDefaultLiteral;

        if (syntax is DefaultExpressionSyntax defaultExpression)
        {
            var semanticModel = compilation.GetSemanticModel(defaultExpression.SyntaxTree);
            var defaultType = semanticModel.GetTypeInfo(defaultExpression).Type;
            return IsNonGenericValueTaskType(compilation, defaultType);
        }

        return IsKnownCompletedTaskExpression(compilation, syntax);
    }

    private static bool TryExtractBaseLifecycleEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        StatementSyntax statement,
        bool allowFirstRenderPayload,
        HashSet<IMethodSymbol> visitedMethods,
        out SupportedEmitCall? emitCall)
        => statement switch
        {
            ExpressionStatementSyntax expressionStatement =>
                TryExtractBaseLifecycleEmitCall(snapshot, expressionEmitter, method, expressionStatement.Expression, allowFirstRenderPayload, visitedMethods, out emitCall),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                TryExtractBaseLifecycleEmitCall(snapshot, expressionEmitter, method, returnStatement.Expression, allowFirstRenderPayload, visitedMethods, out emitCall),
            _ => ReturnNoBaseLifecycleEmitCall(out emitCall)
        };

    private static bool TryExtractBaseLifecycleEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        ExpressionSyntax expression,
        bool allowFirstRenderPayload,
        HashSet<IMethodSymbol> visitedMethods,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        if (!IsBaseLifecyclePassThroughCall(snapshot.Compilation, method, expression))
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

        emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, baseMethod, allowFirstRenderPayload, visitedMethods);
        return true;
    }

    private static bool IsBaseLifecyclePassThroughCall(Compilation compilation, IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(compilation, expression, out var wrappedExpression))
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
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol? method)
        => AnalyzeSetParametersAsync(snapshot, expressionEmitter, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol? method)
        => AnalyzeSetParametersAsync(snapshot, expressionEmitter: null, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
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
            if (IsNoOpLifecycleExpression(snapshot.Compilation, method, methodSyntax.ExpressionBody.Expression))
                return new SetParametersAsyncAnalysis(true, null);

            return IsBaseSetParametersAsyncCall(method, methodSyntax.ExpressionBody.Expression)
                ? AnalyzeBaseSetParametersAsync(snapshot, expressionEmitter, method, visitedMethods)
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
            baseAnalysis = AnalyzeBaseSetParametersAsync(snapshot, expressionEmitter, method, visitedMethods);
            if (!baseAnalysis.IsSupported)
                return new SetParametersAsyncAnalysis(false, null);

            index++;
        }

        if (index >= statements.Count)
            return sawBaseCall
                ? baseAnalysis!
                : new SetParametersAsyncAnalysis(true, null);

        if (TryGetSetParametersAsyncNoOpOrEmit(snapshot, expressionEmitter, method, statements[index], out var emitCall))
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
                IsNoOpSetParametersAsyncStatement(snapshot.Compilation, method, statements[index]))
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
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        var baseMethod = FindBaseSetParametersAsyncMethod(method);
        if (baseMethod is null)
            return new SetParametersAsyncAnalysis(true, null);

        if (baseMethod.DeclaringSyntaxReferences.Length == 0)
        {
            return IsDefaultComponentBaseSetParametersAsyncMethod(snapshot.Compilation, baseMethod)
                ? new SetParametersAsyncAnalysis(true, null)
                : new SetParametersAsyncAnalysis(false, null);
        }

        return AnalyzeSetParametersAsync(snapshot, expressionEmitter, baseMethod, visitedMethods);
    }

    private static ShouldRenderAnalysis AnalyzeShouldRender(Compilation compilation, IMethodSymbol? method)
        => AnalyzeShouldRender(compilation, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static ShouldRenderAnalysis AnalyzeShouldRender(
        Compilation compilation,
        IMethodSymbol? method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (method is null || !visitedMethods.Add(method) || method.DeclaringSyntaxReferences.Length == 0)
            return new ShouldRenderAnalysis(false);

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return new ShouldRenderAnalysis(false);

        if (methodSyntax.ExpressionBody is not null)
            return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, method, methodSyntax.ExpressionBody.Expression, visitedMethods));

        if (methodSyntax.Body?.Statements.Count != 1 ||
            methodSyntax.Body.Statements[0] is not ReturnStatementSyntax { Expression: not null } returnStatement)
        {
            return new ShouldRenderAnalysis(false);
        }

        return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, method, returnStatement.Expression, visitedMethods));
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

    private static bool IsDefaultComponentBaseSetParametersAsyncMethod(Compilation compilation, IMethodSymbol method)
    {
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        return componentBase is not null &&
               string.Equals(method.Name, "SetParametersAsync", StringComparison.Ordinal) &&
               method.Parameters.Length == 1 &&
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

    private static bool IsNoOpSetParametersAsyncStatement(Compilation compilation, IMethodSymbol method, StatementSyntax statement)
        => statement switch
        {
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null => true,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                IsNoOpLifecycleExpression(compilation, method, returnStatement.Expression),
            ExpressionStatementSyntax expressionStatement => IsNoOpLifecycleExpression(compilation, method, expressionStatement.Expression),
            _ => false
        };

    private static bool TryGetSetParametersAsyncNoOpOrEmit(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
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
                if (IsNoOpLifecycleExpression(snapshot.Compilation, method, returnStatement.Expression))
                    return true;

                try
                {
                    emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, returnStatement.Expression, allowFirstRenderPayload: false);
                    return emitCall is not null;
                }
                catch (RazorVueCompilationIssueException)
                {
                    return false;
                }
            case ExpressionStatementSyntax expressionStatement:
                if (IsNoOpLifecycleExpression(snapshot.Compilation, method, expressionStatement.Expression))
                    return true;

                try
                {
                    emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, expressionStatement.Expression, allowFirstRenderPayload: false);
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

    private static bool TryUnwrapValueTaskCreation(Compilation compilation, ExpressionSyntax expression, out ExpressionSyntax innerExpression)
    {
        innerExpression = expression;
        expression = UnwrapLifecycleExpression(expression);
        if (expression is not ObjectCreationExpressionSyntax creation ||
            creation.ArgumentList?.Arguments.Count != 1)
        {
            return false;
        }

        var semanticModel = compilation.GetSemanticModel(creation.SyntaxTree);
        var createdType = semanticModel.GetTypeInfo(creation).Type;
        if (!IsNonGenericValueTaskType(compilation, createdType))
        {
            return false;
        }

        innerExpression = UnwrapLifecycleExpression(creation.ArgumentList.Arguments[0].Expression);
        return true;
    }

    private static bool IsKnownCompletedTaskExpression(Compilation compilation, ExpressionSyntax syntax)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
        if (semanticModel.GetSymbolInfo(syntax).Symbol is not IPropertySymbol
            {
                IsStatic: true,
                Name: "CompletedTask",
                ContainingType: { } containingType
            })
        {
            return false;
        }

        return IsNonGenericTaskType(compilation, containingType) ||
               IsNonGenericValueTaskType(compilation, containingType);
    }

    private static bool IsNonGenericTaskType(Compilation compilation, ITypeSymbol? type)
    {
        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        return taskType is not null &&
               type is not null &&
               SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, taskType);
    }

    private static bool IsNonGenericValueTaskType(Compilation compilation, ITypeSymbol? type)
    {
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        return valueTaskType is not null &&
               type is not null &&
               SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, valueTaskType);
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

    private static bool IsSupportedShouldRenderExpression(
        Compilation compilation,
        IMethodSymbol method,
        ExpressionSyntax expression,
        HashSet<IMethodSymbol> visitedMethods)
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
        if (semanticModel.GetOperation(invocationExpression) is not IInvocationOperation invocation)
            return false;

        if (SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType.OriginalDefinition, componentBase))
            return true;

        var baseMethod = FindBaseLifecycleMethod(method);
        if (baseMethod is null || !SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.OriginalDefinition, baseMethod.OriginalDefinition))
            return false;

        return AnalyzeShouldRender(compilation, baseMethod, visitedMethods).IsSupported;
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

    private static string DescribeEmitCallShape(SupportedEmitCall emitCall)
    {
        var payload = emitCall.PayloadExpression ?? string.Empty;
        if (emitCall.PreludeLocals.IsDefaultOrEmpty)
            return emitCall.EmitName + "|" + payload;

        var prelude = string.Join(
            ";",
            emitCall.PreludeLocals.Select(static local => local.Alias + "=" + local.Expression));
        return emitCall.EmitName + "|" + payload + "|locals:" + prelude;
    }

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
        => CreateUnsupportedSetupLoweringException(symbol, null);

    private static RazorVueCompilationIssueException CreateUnsupportedSetupLoweringException(ISymbol symbol, string? reason)
    {
        var originLocation = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            reason is null
                ? $"RazorVue setup lowering does not support member '{symbol.Name}' in component '{symbol.ContainingType?.ToDisplayString() ?? string.Empty}'."
                : $"RazorVue setup lowering does not support member '{symbol.Name}' in component '{symbol.ContainingType?.ToDisplayString() ?? string.Empty}': {reason}",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, symbol.ContainingType?.ToDisplayString() ?? string.Empty, origin);
    }

    private static RazorVueCompilationIssueException CreateSetupCycleException(ISymbol symbol)
    {
        var originLocation = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue setup lowering does not support recursively dependent member '{symbol.Name}' in component '{symbol.ContainingType?.ToDisplayString() ?? string.Empty}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, symbol.ContainingType?.ToDisplayString() ?? string.Empty, origin);
    }

    internal readonly record struct LifecycleLoweringPlan(
        LifecycleHookPlan? OnInitialized,
        LifecycleHookPlan? OnInitializedAsync,
        LifecycleHookPlan? OnParametersSet,
        LifecycleHookPlan? OnParametersSetAsync,
        LifecycleHookPlan? SetParametersAsync,
        LifecycleHookPlan? Dispose,
        LifecycleHookPlan? DisposeAsync,
        LifecycleHookPlan? OnAfterRender,
        LifecycleHookPlan? OnAfterRenderAsync,
        ImmutableArray<VueLogicPropertyDescriptor> RequiredProperties,
        ImmutableArray<VueLogicFieldDescriptor> RequiredFields,
        ImmutableArray<VueLogicMethodDescriptor> RequiredMethods);

    internal enum LifecycleHookKind
    {
        Standard,
        ImmediateWatch,
        AfterRender
    }

    internal readonly record struct LifecycleHookPlan(
        LifecycleHookKind HookKind,
        string HookName,
        bool AwaitResult,
        SupportedEmitCall? EmitCall,
        bool UsesImmediateWatch,
        string WatchSource);

    private readonly record struct SetupMemberLoweringResult(
        string Block,
        ImmutableArray<IPropertySymbol> PropertyDependencies,
        ImmutableArray<IFieldSymbol> FieldDependencies,
        ImmutableArray<IMethodSymbol> MethodDependencies);

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

    internal sealed record SupportedEmitCall(
        string EmitName,
        string? PayloadExpression,
        bool UsesFirstRender,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadLocalBinding> PreludeLocals);
    private sealed record SetParametersAsyncAnalysis(bool IsSupported, SupportedEmitCall? EmitCall);
    private sealed record ShouldRenderAnalysis(bool IsSupported);
}
