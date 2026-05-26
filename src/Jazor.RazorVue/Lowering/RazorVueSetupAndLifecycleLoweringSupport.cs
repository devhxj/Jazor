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
        if (property.LoweringKind == VueLogicPropertyLoweringKind.Unsupported)
            throw CreateUnsupportedSetupLoweringException(property.PropertySymbol);

        try
        {
            IOperation? operation;
            if (!TryGetPropertyExpressionOperation(snapshot, property.PropertySymbol, out operation))
            {
                if (RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedMutableSetupCarrierMemberReason(property.PropertySymbol, out var propertyReason))
                    throw CreateUnsupportedSetupLoweringException(property.PropertySymbol, propertyReason);

                _ = RazorVueCurrentComponentValueMemberHelper.TryGetMutableSetupCarrierInitializer(
                    snapshot.Compilation,
                    property.PropertySymbol,
                    out operation,
                    out var hasDeclarationInitializer);
                if (operation is null && hasDeclarationInitializer)
                    throw CreateUnsupportedSetupLoweringException(property.PropertySymbol);
            }

            var capture = operation is null
                ? new RazorVueExpressionEmitter.SetupDependencyCapture(
                    GetDefaultValueInitializerExpression(property.PropertySymbol.Type),
                    ImmutableArray<IPropertySymbol>.Empty,
                    ImmutableArray<IFieldSymbol>.Empty,
                    ImmutableArray<IMethodSymbol>.Empty)
                : expressionEmitter.CaptureSetupDependencies(() => expressionEmitter.EmitSetupExpression(operation));
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
        out IOperation? operation)
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

        operation = null;
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
        var onInitialized = TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedMethod, awaitResult: false, allowFirstRenderPayload: false);
        var onInitializedAsync = TryPlanLifecycleHook(snapshot, expressionEmitter, "onMounted", snapshot.OnInitializedAsyncMethod, awaitResult: true, allowFirstRenderPayload: false);
        var onParametersSet = TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetMethod, awaitResult: false);
        var onParametersSetAsync = TryPlanParametersSetHook(snapshot, expressionEmitter, snapshot.OnParametersSetAsyncMethod, awaitResult: true);
        var setParametersAsync = TryPlanSetParametersAsyncHook(snapshot, expressionEmitter);
        var dispose = TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeMethod, awaitResult: false, allowFirstRenderPayload: false);
        var disposeAsync = TryPlanLifecycleHook(snapshot, expressionEmitter, "onUnmounted", snapshot.DisposeAsyncMethod, awaitResult: true, allowFirstRenderPayload: false);
        var onAfterRender = TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderMethod, awaitResult: false);
        var onAfterRenderAsync = TryPlanAfterRenderHook(snapshot, expressionEmitter, snapshot.OnAfterRenderAsyncMethod, awaitResult: true);
        var shouldRenderGate = TryPlanShouldRenderGate(snapshot, expressionEmitter);

        var requiredProperties = ImmutableArray<VueLogicPropertyDescriptor>.Empty;
        var requiredFields = ImmutableArray<VueLogicFieldDescriptor>.Empty;
        var requiredMethods = ImmutableArray<VueLogicMethodDescriptor>.Empty;

        if (expressionEmitter is not null)
        {
            requiredProperties = expressionEmitter.GetRequiredSetupProperties();
            requiredFields = expressionEmitter.GetRequiredSetupFields();
            requiredMethods = expressionEmitter.GetRequiredSetupMethods();
        }

        return new LifecycleLoweringPlan(
            onInitialized,
            onInitializedAsync,
            onParametersSet,
            onParametersSetAsync,
            setParametersAsync,
            dispose,
            disposeAsync,
            onAfterRender,
            onAfterRenderAsync,
            shouldRenderGate,
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
        => !AnalyzeSetParametersAsync(snapshot, expressionEmitter: null, snapshot.SetParametersAsyncMethod).EmitCalls.IsDefaultOrEmpty;

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

        return analysis.EmitCalls.IsDefaultOrEmpty
            ? "none"
            : DescribeEmitCallSequenceShape(analysis.EmitCalls);
    }

    public static string DescribeShouldRenderShape(Compilation compilation, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        var analysis = AnalyzeShouldRender(compilation, method);
        return DescribeShouldRenderAnalysis(analysis);
    }

    public static string DescribeShouldRenderShape(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
    {
        _ = expressionEmitter;
        if (snapshot.ShouldRenderMethod is null)
            return "none";

        var analysis = AnalyzeShouldRender(
            snapshot.Compilation,
            snapshot.ShouldRenderMethod,
            new RazorVueExpressionEmitter(snapshot));
        return DescribeShouldRenderAnalysis(analysis);
    }

    public static string DescribeShouldRenderSupportShape(RazorVueSemanticSnapshot snapshot, IMethodSymbol? method)
    {
        if (method is null)
            return "none";

        var analysis = AnalyzeShouldRender(
            snapshot.Compilation,
            method,
            new RazorVueExpressionEmitter(snapshot));
        return DescribeShouldRenderAnalysis(analysis);
    }

    private static string DescribeShouldRenderAnalysis(ShouldRenderAnalysis analysis)
    {
        if (!analysis.IsSupported)
            return "unsupported";

        return analysis.RequiresRenderGate
            ? "condition:" + analysis.ExpressionText
            : "true";
    }

    private static SetupMemberLoweringResult BuildSetupFieldLowering(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueLogicFieldDescriptor field,
        string indent)
    {
        if (!RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(
                snapshot.Compilation,
                field.FieldSymbol,
                out var operation) ||
            operation is null)
        {
            if (RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedMutableSetupCarrierMemberReason(field.FieldSymbol, out var fieldReason))
                throw CreateUnsupportedSetupLoweringException(field.FieldSymbol, fieldReason);

            _ = RazorVueCurrentComponentValueMemberHelper.TryGetMutableSetupCarrierInitializer(
                snapshot.Compilation,
                field.FieldSymbol,
                out operation,
                out var hasDeclarationInitializer);
            if (operation is null && hasDeclarationInitializer)
                throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);
        }

        try
        {
            var capture = operation is null
                ? new RazorVueExpressionEmitter.SetupDependencyCapture(
                    GetDefaultFieldInitializerExpression(field.FieldSymbol),
                    ImmutableArray<IPropertySymbol>.Empty,
                    ImmutableArray<IFieldSymbol>.Empty,
                    ImmutableArray<IMethodSymbol>.Empty)
                : expressionEmitter.CaptureSetupDependencies(() => expressionEmitter.EmitSetupExpression(operation));
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

    private static string GetDefaultFieldInitializerExpression(IFieldSymbol field)
        => GetDefaultValueInitializerExpression(field.Type);

    private static string GetDefaultValueInitializerExpression(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Enum)
            return "0";

        return type.SpecialType switch
        {
            SpecialType.System_Boolean => "false",
            SpecialType.System_Char => "\"\\0\"",
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Decimal or
            SpecialType.System_Single or
            SpecialType.System_Double => "0",
            _ => "null"
        };
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

        var semanticModel = snapshot.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);

        try
        {
            var parameterAliases = method.MethodSymbol.Parameters
                .Select(static parameter => parameter.Name)
                .ToArray();
            var capture = TryGetSetupMethodReturnExpressionOperation(methodSyntax, semanticModel, out var expressionSyntax, out var operation)
                ? expressionEmitter.CaptureSetupDependenciesWithParameterAliases(
                    method.MethodSymbol.Parameters,
                    parameterAliases,
                    () =>
                    {
                        var expression = ContainsExplicitParentheses(expressionSyntax)
                            ? BuildSetupExpressionPreservingExplicitParentheses(expressionSyntax, semanticModel, expressionEmitter)
                            : expressionEmitter.EmitSetupExpression(operation);
                        if (RequiresWholeReturnParentheses(expressionSyntax) && !expression.StartsWith("(", StringComparison.Ordinal))
                            expression = "(" + expression + ")";

                        return "return " + NormalizeSetupMethodReturnExpression(expression) + ";";
                    })
                : TryGetSetupMethodBodyOperations(methodSyntax, semanticModel, out var operations)
                    ? expressionEmitter.CaptureSetupDependenciesWithParameterAliases(
                        method.MethodSymbol.Parameters,
                        parameterAliases,
                        () => expressionEmitter.EmitSetupStatementSequence(operations))
                    : throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);
            var methodBuilder = new StringBuilder();
            methodBuilder.Append(indent)
                .Append("function ")
                .Append(ToLowerCamelCase(method.Name))
                .Append('(')
                .Append(string.Join(", ", parameterAliases))
                .AppendLine(") {");
            AppendIndentedSetupMethodBody(methodBuilder, capture.Expression, indent);
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

    private static bool TryGetSetupMethodReturnExpressionOperation(
        MethodDeclarationSyntax methodSyntax,
        SemanticModel semanticModel,
        out ExpressionSyntax expressionSyntax,
        out IOperation operation)
    {
        expressionSyntax = default!;
        operation = default!;
        expressionSyntax = methodSyntax.ExpressionBody?.Expression
            ?? (methodSyntax.Body?.Statements.Count == 1 &&
                methodSyntax.Body.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement
                    ? returnStatement.Expression
                    : null!);
        if (expressionSyntax is null)
            return false;

        operation = semanticModel.GetOperation(expressionSyntax)!;
        return operation is not null;
    }

    private static bool TryGetSetupMethodBodyOperations(
        MethodDeclarationSyntax methodSyntax,
        SemanticModel semanticModel,
        out ImmutableArray<IOperation> operations)
    {
        operations = ImmutableArray<IOperation>.Empty;
        if (methodSyntax.Body is null ||
            semanticModel.GetOperation(methodSyntax.Body) is not IBlockOperation blockOperation)
        {
            return false;
        }

        operations = blockOperation.Operations;
        return !operations.IsDefault;
    }

    private static void AppendIndentedSetupMethodBody(
        StringBuilder builder,
        string body,
        string indent)
    {
        var normalized = Util.NormalizeLineEndingsToLf(body).Trim();
        if (normalized.Length == 0)
            return;

        foreach (var line in normalized.Split('\n'))
        {
            builder
                .Append(indent)
                .Append("  ")
                .AppendLine(line);
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

    private static void AppendEmitStatements(
        StringBuilder builder,
        ImmutableArray<SupportedEmitCall> emitCalls,
        bool awaitResult,
        string indent)
    {
        if (emitCalls.IsDefaultOrEmpty)
            return;

        foreach (var emitCall in emitCalls)
            AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null, indent);
    }

    private static void AppendEmitPrelude(
        StringBuilder builder,
        SupportedEmitCall emitCall,
        string indent)
    {
        if (emitCall.PreludeBindings.IsDefaultOrEmpty)
            return;

        foreach (var binding in emitCall.PreludeBindings)
            builder.Append(indent).AppendLine(binding.Code);
    }

    private static ImmutableArray<SupportedEmitCall> ConcatEmitCalls(
        ImmutableArray<SupportedEmitCall> first,
        ImmutableArray<SupportedEmitCall> second)
    {
        if (first.IsDefaultOrEmpty)
            return second.IsDefault ? ImmutableArray<SupportedEmitCall>.Empty : second;
        if (second.IsDefaultOrEmpty)
            return first;

        var builder = ImmutableArray.CreateBuilder<SupportedEmitCall>(first.Length + second.Length);
        builder.AddRange(first);
        builder.AddRange(second);
        return builder.ToImmutable();
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
            EmitCalls: [emitCall],
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
            EmitCalls: [emitCall],
            UsesImmediateWatch: true,
            WatchSource: BuildPropsWatchSource(snapshot.Descriptor));
    }

    private static LifecycleHookPlan? TryPlanSetParametersAsyncHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter)
    {
        var analysis = AnalyzeSetParametersAsync(snapshot, expressionEmitter, snapshot.SetParametersAsyncMethod);
        if (!analysis.IsSupported || analysis.EmitCalls.IsDefaultOrEmpty)
            return null;

        return new LifecycleHookPlan(
            HookKind: LifecycleHookKind.ImmediateWatch,
            HookName: "watch",
            AwaitResult: true,
            EmitCalls: analysis.EmitCalls,
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

    private static ShouldRenderGatePlan? TryPlanShouldRenderGate(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter)
    {
        var analysis = AnalyzeShouldRender(snapshot.Compilation, snapshot.ShouldRenderMethod, expressionEmitter);
        if (!analysis.IsSupported || !analysis.RequiresRenderGate)
            return null;

        return new ShouldRenderGatePlan(analysis.ExpressionText);
    }

    private static LifecycleHookPlan CreateAfterRenderHookPlan(
        SupportedEmitCall emitCall,
        bool awaitResult)
        => new(
            HookKind: LifecycleHookKind.AfterRender,
            HookName: "onAfterRender",
            AwaitResult: awaitResult,
            EmitCalls: [emitCall],
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
        if (plan is not { } hookPlan || hookPlan.EmitCalls.IsDefaultOrEmpty)
            return;

        var emitCalls = hookPlan.EmitCalls;

        switch (hookPlan.HookKind)
        {
            case LifecycleHookKind.Standard:
                builder.Append(indent).Append(hookPlan.HookName).Append("(");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendEmitStatements(builder, emitCalls, hookPlan.AwaitResult, indent + "  ");
                builder.Append(indent).AppendLine("});");
                return;

            case LifecycleHookKind.ImmediateWatch:
                builder.Append(indent).Append("watch(() => ").Append(hookPlan.WatchSource).Append(", ");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendEmitStatements(builder, emitCalls, hookPlan.AwaitResult, indent + "  ");
                builder.Append(indent).AppendLine("}, { immediate: true });");
                return;

            case LifecycleHookKind.AfterRender:
                var emitCall = emitCalls[0];
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
        out SupportedEmitCall? emitCall,
        int startIndex = 0,
        int? endExclusive = null)
    {
        emitCall = null;
        var exclusiveEnd = endExclusive ?? statements.Count;
        if (startIndex < 0 ||
            exclusiveEnd > statements.Count ||
            exclusiveEnd - startIndex < 2)
        {
            return false;
        }

        var lastStatement = statements[exclusiveEnd - 1];
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

        for (var index = startIndex; index < exclusiveEnd - 1; index++)
        {
            var statement = statements[index];
            if (!TryValidateLifecyclePrefixDeclarations(statement, semanticModel, localInitializers))
                return false;
        }

        emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, emitExpression, allowFirstRenderPayload);
        return emitCall is not null;
    }

    private static bool TryValidateLifecyclePrefixDeclarations(
        StatementSyntax statement,
        SemanticModel semanticModel,
        IReadOnlyDictionary<ILocalSymbol, IOperation> localInitializers)
    {
        switch (semanticModel.GetOperation(statement))
        {
            case IVariableDeclarationGroupOperation declarationGroup:
                var hasDeclarator = false;
                foreach (var declaration in declarationGroup.Declarations)
                {
                    foreach (var declarator in declaration.Declarators)
                    {
                        hasDeclarator = true;
                        if (!localInitializers.ContainsKey(declarator.Symbol))
                            return false;
                    }
                }

                return hasDeclarator;
            case ILocalFunctionOperation:
                return true;
            case IExpressionStatementOperation { Operation: IDeconstructionAssignmentOperation }:
                var declaredLocals = statement
                    .DescendantNodes()
                    .OfType<SingleVariableDesignationSyntax>()
                    .Select(designation => semanticModel.GetDeclaredSymbol(designation))
                    .OfType<ILocalSymbol>()
                    .ToImmutableArray();
                if (declaredLocals.IsDefaultOrEmpty)
                    return false;

                foreach (var local in declaredLocals)
                {
                    if (!localInitializers.ContainsKey(local))
                        return false;
                }

                return true;
            default:
                return false;
        }
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
            return new SupportedEmitCall(emitName, null, false, ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty);

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

            if (payload.Expression is null)
                throw CreateUnsupportedLifecycleLoweringException(method);

            return new SupportedEmitCall(emitName, payload.Expression, payload.UsesFirstRender, payload.PreludeBindings);
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
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
            return SetParametersAsyncAnalysis.Unsupported;

        if (method.DeclaringSyntaxReferences.Length == 0)
            return SetParametersAsyncAnalysis.Unsupported;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return SetParametersAsyncAnalysis.Unsupported;

        if (methodSyntax.ExpressionBody is not null)
        {
            if (IsNoOpLifecycleExpression(snapshot.Compilation, method, methodSyntax.ExpressionBody.Expression))
                return SetParametersAsyncAnalysis.NoOp;

            return IsBaseSetParametersAsyncCall(method, methodSyntax.ExpressionBody.Expression)
                ? AnalyzeBaseSetParametersAsync(snapshot, expressionEmitter, method, visitedMethods)
                : SetParametersAsyncAnalysis.Unsupported;
        }

        if (methodSyntax.Body is null)
            return SetParametersAsyncAnalysis.Unsupported;

        if (methodSyntax.Body.Statements.Count == 0)
            return SetParametersAsyncAnalysis.NoOp;

        var statements = methodSyntax.Body.Statements;
        var index = 0;
        var sawBaseCall = false;
        var emitCalls = ImmutableArray<SupportedEmitCall>.Empty;
        if (IsBaseSetParametersAsyncStatement(method, statements[0]))
        {
            sawBaseCall = true;
            var baseAnalysis = AnalyzeBaseSetParametersAsync(snapshot, expressionEmitter, method, visitedMethods);
            if (!baseAnalysis.IsSupported)
                return SetParametersAsyncAnalysis.Unsupported;

            emitCalls = baseAnalysis.EmitCalls;
            index++;
        }

        if (index >= statements.Count)
            return new SetParametersAsyncAnalysis(true, emitCalls);

        if (!sawBaseCall)
            return SetParametersAsyncAnalysis.Unsupported;

        return TryGetSetParametersAsyncEmitSequence(
                snapshot,
                expressionEmitter,
                method,
                statements,
                index,
                out var localEmitCalls)
            ? new SetParametersAsyncAnalysis(true, ConcatEmitCalls(emitCalls, localEmitCalls))
            : SetParametersAsyncAnalysis.Unsupported;
    }

    private static SetParametersAsyncAnalysis AnalyzeBaseSetParametersAsync(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        var baseMethod = FindBaseSetParametersAsyncMethod(method);
        if (baseMethod is null)
            return SetParametersAsyncAnalysis.NoOp;

        if (baseMethod.DeclaringSyntaxReferences.Length == 0)
        {
            return IsDefaultComponentBaseSetParametersAsyncMethod(snapshot.Compilation, baseMethod)
                ? SetParametersAsyncAnalysis.NoOp
                : SetParametersAsyncAnalysis.Unsupported;
        }

        return AnalyzeSetParametersAsync(snapshot, expressionEmitter, baseMethod, visitedMethods);
    }

    private static ShouldRenderAnalysis AnalyzeShouldRender(
        Compilation compilation,
        IMethodSymbol? method,
        RazorVueExpressionEmitter? expressionEmitter = null)
        => AnalyzeShouldRender(compilation, method, expressionEmitter, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static ShouldRenderAnalysis AnalyzeShouldRender(
        Compilation compilation,
        IMethodSymbol? method,
        RazorVueExpressionEmitter? expressionEmitter,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (method is null || !visitedMethods.Add(method) || method.DeclaringSyntaxReferences.Length == 0)
            return ShouldRenderAnalysis.Unsupported;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return ShouldRenderAnalysis.Unsupported;

        if (methodSyntax.ExpressionBody is not null)
            return AnalyzeShouldRenderExpression(
                compilation,
                method,
                expressionEmitter,
                methodSyntax.ExpressionBody.Expression,
                visitedMethods);

        if (methodSyntax.Body is null)
            return ShouldRenderAnalysis.Unsupported;

        if (methodSyntax.Body.Statements.Count == 1 &&
            methodSyntax.Body.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement)
        {
            return AnalyzeShouldRenderExpression(
                compilation,
                method,
                expressionEmitter,
                returnStatement.Expression,
                visitedMethods);
        }

        return AnalyzeShouldRenderStatementBody(
            compilation,
            method,
            expressionEmitter,
            methodSyntax.Body,
            visitedMethods);
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

    private static bool TryGetSetParametersAsyncEmitSequence(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SyntaxList<StatementSyntax> statements,
        int startIndex,
        out ImmutableArray<SupportedEmitCall> emitCalls)
    {
        emitCalls = ImmutableArray<SupportedEmitCall>.Empty;
        if (startIndex < 0 || startIndex > statements.Count)
            return false;

        var semanticModel = snapshot.Compilation.GetSemanticModel(statements[0].SyntaxTree);
        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax ||
            semanticModel.GetOperation(methodSyntax) is not IMethodBodyOperation methodBodyOperation)
        {
            return false;
        }

        var localInitializers = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
            snapshot.Compilation,
            methodBodyOperation.BlockBody?.Operations ?? ImmutableArray<IOperation>.Empty,
            RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);
        var emittedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var materializedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var localAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        var builder = ImmutableArray.CreateBuilder<SupportedEmitCall>();
        var sawEmit = false;

        for (var index = startIndex; index < statements.Count; index++)
        {
            var statement = statements[index];
            if (TryValidateLifecyclePrefixDeclarations(statement, semanticModel, localInitializers))
            {
                var prefixLocals = GetLifecyclePrefixDeclaredLocals(statement, semanticModel);
                foreach (var local in prefixLocals)
                {
                    if (!localInitializers.ContainsKey(local))
                        return false;

                    emittedLocals.Add(local);
                    localAliases[local] = "__jazorLifecycleLocal" + Jazor.Common.Format.HashName(local.ToDisplayString()).TrimStart('_');
                }

                continue;
            }

            if (!TryGetSetParametersAsyncNoOpOrEmit(
                    snapshot,
                    expressionEmitter,
                    method,
                    statement,
                    out var emitCall))
            {
                return false;
            }

            if (statement is ReturnStatementSyntax && index != statements.Count - 1)
                return false;

            if (emitCall is null)
                continue;

            sawEmit = true;
            if (emittedLocals.Count == 0)
            {
                builder.Add(emitCall);
                continue;
            }

            var preludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
            foreach (var binding in emitCall.PreludeBindings)
            {
                if (TryGetLifecycleLocalPreludeAlias(binding, out var alias) &&
                    TryFindLifecycleLocalByAlias(localAliases, alias, out var local))
                {
                    if (materializedLocals.Add(local))
                        preludeBuilder.Add(binding);

                    continue;
                }

                preludeBuilder.Add(binding);
            }

            builder.Add(emitCall with { PreludeBindings = preludeBuilder.ToImmutable() });
        }

        if (!sawEmit)
        {
            emitCalls = ImmutableArray<SupportedEmitCall>.Empty;
            return true;
        }

        emitCalls = builder.ToImmutable();
        return true;
    }

    private static ImmutableArray<ILocalSymbol> GetLifecyclePrefixDeclaredLocals(
        StatementSyntax statement,
        SemanticModel semanticModel)
    {
        switch (semanticModel.GetOperation(statement))
        {
            case IVariableDeclarationGroupOperation declarationGroup:
                return declarationGroup.Declarations
                    .SelectMany(static declaration => declaration.Declarators)
                    .Select(static declarator => declarator.Symbol)
                    .ToImmutableArray();

            case IExpressionStatementOperation { Operation: IDeconstructionAssignmentOperation }:
                return statement
                    .DescendantNodes()
                    .OfType<SingleVariableDesignationSyntax>()
                    .Select(designation => semanticModel.GetDeclaredSymbol(designation))
                    .OfType<ILocalSymbol>()
                    .ToImmutableArray();

            default:
                return ImmutableArray<ILocalSymbol>.Empty;
        }
    }

    private static bool TryGetLifecycleLocalPreludeAlias(
        RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding binding,
        out string alias)
    {
        const string constPrefix = "const __jazorLifecycleLocal";
        alias = string.Empty;
        if (!binding.Code.StartsWith(constPrefix, StringComparison.Ordinal))
            return false;

        var equalsIndex = binding.Code.IndexOf(" = ", StringComparison.Ordinal);
        if (equalsIndex <= "const ".Length)
            return false;

        alias = binding.Code.Substring("const ".Length, equalsIndex - "const ".Length);
        return alias.Length > 0;
    }

    private static bool TryFindLifecycleLocalByAlias(
        IReadOnlyDictionary<ILocalSymbol, string> aliases,
        string alias,
        out ILocalSymbol local)
    {
        foreach (var pair in aliases)
        {
            if (string.Equals(pair.Value, alias, StringComparison.Ordinal))
            {
                local = pair.Key;
                return true;
            }
        }

        local = default!;
        return false;
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

    private static ShouldRenderAnalysis AnalyzeShouldRenderStatementBody(
        Compilation compilation,
        IMethodSymbol method,
        RazorVueExpressionEmitter? expressionEmitter,
        BlockSyntax body,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (expressionEmitter is null)
            return ShouldRenderAnalysis.Unsupported;

        try
        {
            var semanticModel = compilation.GetSemanticModel(body.SyntaxTree);
            if (semanticModel.GetOperation(body) is not IBlockOperation blockOperation ||
                blockOperation.Operations.IsDefaultOrEmpty)
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            if (!TryValidateShouldRenderStatementSequence(
                    blockOperation.Operations,
                    ShouldRenderStatementScope.MethodBody,
                    out var bodyAlwaysReturns,
                    out var containsConditional) ||
                !bodyAlwaysReturns)
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            if (!containsConditional &&
                body.Statements[body.Statements.Count - 1] is ReturnStatementSyntax { Expression: not null } returnStatement &&
                (IsConstantTrueShouldRenderExpression(returnStatement.Expression) ||
                 TryAnalyzeBaseShouldRenderExpression(
                     compilation,
                     method,
                     expressionEmitter,
                     returnStatement.Expression,
                     visitedMethods,
                     out _)))
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            if (!TryCreateShouldRenderLocalAliases(
                    blockOperation.Operations,
                    out var localAliases))
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            var capture = expressionEmitter.CaptureSetupDependencies(
                () => expressionEmitter.WithScopedLocalAliases(
                    localAliases,
                    () => expressionEmitter.EmitSetupStatementSequence(blockOperation.Operations)));
            var bodyText = Util.NormalizeLineEndingsToLf(capture.Expression).Trim();
            if (bodyText.Length == 0)
                return ShouldRenderAnalysis.Unsupported;

            return new ShouldRenderAnalysis(
                IsSupported: true,
                RequiresRenderGate: true,
                ExpressionText: "(() => {\n" + bodyText + "\n})()");
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            return ShouldRenderAnalysis.Unsupported;
        }
    }

    private enum ShouldRenderStatementScope
    {
        MethodBody,
        BranchBody
    }

    private static bool TryValidateShouldRenderStatementSequence(
        ImmutableArray<IOperation> operations,
        ShouldRenderStatementScope scope,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = false;
        if (operations.IsDefaultOrEmpty)
            return true;

        for (var index = 0; index < operations.Length; index++)
        {
            if (alwaysReturns)
                return false;

            if (!TryValidateShouldRenderStatement(
                    operations[index],
                    scope,
                    out var statementAlwaysReturns,
                    out var statementContainsConditional))
            {
                return false;
            }

            containsConditional |= statementContainsConditional;
            if (statementAlwaysReturns)
                alwaysReturns = true;
        }

        return true;
    }

    private static bool TryValidateShouldRenderStatement(
        IOperation operation,
        ShouldRenderStatementScope scope,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IBlockOperation block:
                return TryValidateShouldRenderStatementSequence(
                    block.Operations,
                    scope,
                    out alwaysReturns,
                    out containsConditional);

            case IReturnOperation { ReturnedValue: not null } returnOperation:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(returnOperation.ReturnedValue))
                    return false;

                alwaysReturns = true;
                return true;

            case IVariableDeclarationGroupOperation declarationGroup:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(declarationGroup))
                    return false;

                return true;

            case IExpressionStatementOperation expressionStatement:
                return TryValidateShouldRenderLocalMutationExpressionStatement(expressionStatement);

            case ILocalFunctionOperation localFunction when scope == ShouldRenderStatementScope.MethodBody:
                return CanLowerShouldRenderLocalFunction(
                    localFunction,
                    CreateShouldRenderChildReservedLocalNames());

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(conditional.Condition))
                    return false;

                containsConditional = true;
                if (!TryValidateShouldRenderStatement(
                        conditional.WhenTrue,
                        ShouldRenderStatementScope.BranchBody,
                        out var trueAlwaysReturns,
                        out var trueContainsConditional))
                {
                    return false;
                }

                var falseAlwaysReturns = false;
                var falseContainsConditional = false;
                if (conditional.WhenFalse is not null &&
                    !TryValidateShouldRenderStatement(
                        conditional.WhenFalse,
                        ShouldRenderStatementScope.BranchBody,
                        out falseAlwaysReturns,
                        out falseContainsConditional))
                {
                    return false;
                }

                containsConditional |= trueContainsConditional || falseContainsConditional;
                alwaysReturns = trueAlwaysReturns && conditional.WhenFalse is not null && falseAlwaysReturns;
                return true;

            case ISwitchOperation switchOperation when switchOperation.Syntax is SwitchStatementSyntax:
                return TryValidateShouldRenderSwitchStatement(
                    switchOperation,
                    out alwaysReturns,
                    out containsConditional);

            default:
                return false;
        }
    }

    private static bool TryValidateShouldRenderSwitchStatement(
        ISwitchOperation switchOperation,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;
        if (ContainsShouldRenderUnsupportedExpressionConstruct(switchOperation.Value))
            return false;

        var hasDefaultCase = false;
        var allCaseBodiesAlwaysReturn = true;
        foreach (var switchCase in switchOperation.Cases)
        {
            if (switchCase.Clauses.IsDefaultOrEmpty)
                return false;

            foreach (var clause in switchCase.Clauses)
            {
                switch (clause)
                {
                    case IDefaultCaseClauseOperation:
                        hasDefaultCase = true;
                        break;

                    case ISingleValueCaseClauseOperation singleValueClause:
                        if (ContainsShouldRenderUnsupportedExpressionConstruct(singleValueClause.Value))
                            return false;

                        break;

                    default:
                        return false;
                }
            }

            if (!TryValidateShouldRenderStatementSequence(
                    switchCase.Body,
                    ShouldRenderStatementScope.BranchBody,
                    out var caseAlwaysReturns,
                    out var caseContainsConditional))
            {
                return false;
            }

            containsConditional |= caseContainsConditional;
            allCaseBodiesAlwaysReturn &= caseAlwaysReturns;
        }

        alwaysReturns = hasDefaultCase && allCaseBodiesAlwaysReturn;
        return true;
    }

    private static bool TryValidateShouldRenderLocalMutationExpressionStatement(
        IExpressionStatementOperation expressionStatement)
    {
        var expression = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
        return expression is not null &&
            TryValidateShouldRenderLocalMutationExpression(expression);
    }

    private static bool TryValidateShouldRenderLocalMutationExpression(IOperation operation)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case ISimpleAssignmentOperation assignment:
                return IsShouldRenderLocalMutationTarget(assignment.Target) &&
                    !ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value);

            case ICompoundAssignmentOperation assignment:
                return IsShouldRenderLocalMutationTarget(assignment.Target) &&
                    !ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value);

            case IIncrementOrDecrementOperation incrementOrDecrement:
                return IsShouldRenderLocalMutationTarget(incrementOrDecrement.Target);

            default:
                return false;
        }
    }

    private static bool IsShouldRenderLocalMutationTarget(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation) is ILocalReferenceOperation;

    private static bool TryCreateShouldRenderLocalAliases(
        ImmutableArray<IOperation> operations,
        out IReadOnlyDictionary<ILocalSymbol, string> localAliases)
    {
        localAliases = ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default);
        if (operations.IsDefaultOrEmpty ||
            ContainsShouldRenderAnonymousFunction(operations))
        {
            return false;
        }

        if (!TryValidateShouldRenderStatementSequence(
                operations,
                ShouldRenderStatementScope.MethodBody,
                out var alwaysReturns,
                out _) ||
            !alwaysReturns)
        {
            return false;
        }

        if (!TryCreateShouldRenderReservedLocalNames(operations, out var reservedNames))
            return false;

        var aliases = ImmutableDictionary.CreateBuilder<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var operation in EnumerateShouldRenderMethodScopedOperations(operations))
        {
            switch (RazorVueOperationNormalizer.Unwrap(operation))
            {
                case IVariableDeclarationGroupOperation declarationGroup:
                    foreach (var declaration in declarationGroup.Declarations)
                    {
                        foreach (var declarator in declaration.Declarators)
                        {
                            if (!TryCreateShouldRenderLocalAlias(declarator.Symbol, reservedNames, out var alias))
                                return false;

                            aliases[declarator.Symbol] = alias;
                        }
                    }

                    break;

                case IDeclarationExpressionOperation declarationExpression:
                    foreach (var local in CollectShouldRenderDeclaredLocals(declarationExpression.Expression))
                    {
                        if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var alias))
                            return false;

                        aliases[local] = alias;
                    }

                    break;

                case IDeclarationPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var patternAlias))
                        return false;

                    aliases[local] = patternAlias;
                    break;

                case IRecursivePatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var recursivePatternAlias))
                        return false;

                    aliases[local] = recursivePatternAlias;
                    break;

                case IListPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var listPatternAlias))
                        return false;

                    aliases[local] = listPatternAlias;
                    break;

                case ILocalFunctionOperation localFunction:
                    if (!CanLowerShouldRenderLocalFunction(localFunction, reservedNames))
                        return false;

                    break;
            }
        }

        localAliases = aliases.ToImmutable();
        return true;
    }

    private static bool TryCreateShouldRenderExpressionLocalAliases(
        IOperation operation,
        out IReadOnlyDictionary<ILocalSymbol, string> localAliases)
    {
        localAliases = ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default);
        if (ContainsShouldRenderAnonymousFunction(operation))
            return false;

        if (!TryCreateShouldRenderExpressionReservedLocalNames(operation, out var reservedNames))
            return false;

        var aliases = ImmutableDictionary.CreateBuilder<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var current in EnumerateShouldRenderExpressionScopedOperations(operation))
        {
            switch (RazorVueOperationNormalizer.Unwrap(current))
            {
                case IDeclarationExpressionOperation declarationExpression:
                    foreach (var local in CollectShouldRenderDeclaredLocals(declarationExpression.Expression))
                    {
                        if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var alias))
                            return false;

                        aliases[local] = alias;
                    }

                    break;

                case IDeclarationPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var patternAlias))
                        return false;

                    aliases[local] = patternAlias;
                    break;

                case IRecursivePatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var recursivePatternAlias))
                        return false;

                    aliases[local] = recursivePatternAlias;
                    break;

                case IListPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var listPatternAlias))
                        return false;

                    aliases[local] = listPatternAlias;
                    break;
            }
        }

        localAliases = aliases.ToImmutable();
        return true;
    }

    private static IEnumerable<IOperation> EnumerateShouldRenderMethodScopedOperations(
        ImmutableArray<IOperation> operations)
    {
        foreach (var operation in operations)
        {
            foreach (var current in EnumerateShouldRenderMethodScopedOperations(operation))
                yield return current;
        }
    }

    private static IEnumerable<IOperation> EnumerateShouldRenderMethodScopedOperations(IOperation operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            yield break;

        yield return current;

        switch (current)
        {
            case ILocalFunctionOperation:
                yield break;

            case IBlockOperation block:
                foreach (var child in EnumerateShouldRenderMethodScopedOperations(block.Operations))
                    yield return child;
                yield break;

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                if (conditional.Condition is not null)
                {
                    foreach (var child in EnumerateShouldRenderExpressionScopedOperations(conditional.Condition))
                        yield return child;
                }

                foreach (var child in EnumerateShouldRenderMethodScopedOperations(conditional.WhenTrue))
                    yield return child;

                if (conditional.WhenFalse is not null)
                {
                    foreach (var child in EnumerateShouldRenderMethodScopedOperations(conditional.WhenFalse))
                        yield return child;
                }

                yield break;

            default:
                foreach (var child in current.ChildOperations)
                {
                    if (child is null)
                        continue;

                    foreach (var nested in EnumerateShouldRenderMethodScopedOperations(child))
                        yield return nested;
                }

                yield break;
        }
    }

    private static IEnumerable<IOperation> EnumerateShouldRenderExpressionScopedOperations(IOperation operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            yield break;

        yield return current;

        if (current is IAnonymousFunctionOperation or ILocalFunctionOperation)
            yield break;

        foreach (var child in current.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateShouldRenderExpressionScopedOperations(child))
                yield return nested;
        }
    }

    private static bool ContainsShouldRenderUnsupportedExpressionConstruct(IOperation? operation)
    {
        if (operation is null)
            return false;

        foreach (var current in EnumerateShouldRenderExpressionScopedOperations(operation))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is
                IAnonymousFunctionOperation or
                ILocalFunctionOperation or
                ISimpleAssignmentOperation or
                ICompoundAssignmentOperation or
                IIncrementOrDecrementOperation)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryCreateShouldRenderLocalAlias(
        ILocalSymbol local,
        HashSet<string> reservedNames,
        out string alias)
    {
        alias = "__jazorShouldRenderLocal" + Jazor.Common.Format.HashName(local.ToDisplayString()).TrimStart('_');
        if (!reservedNames.Contains(alias) && reservedNames.Add(alias))
            return true;

        var suffix = 0;
        while (suffix < 1024)
        {
            suffix++;
            var candidate = alias + "$" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!reservedNames.Contains(candidate) && reservedNames.Add(candidate))
            {
                alias = candidate;
                return true;
            }
        }

        alias = string.Empty;
        return false;
    }

    private static bool CanLowerShouldRenderLocalFunction(
        ILocalFunctionOperation operation,
        HashSet<string> reservedNames)
    {
        var method = operation.Symbol;
        if (method.IsAsync ||
            method.IsGenericMethod ||
            method.RefKind != RefKind.None ||
            !IsJavaScriptBindingIdentifier(method.Name) ||
            IsShouldRenderSetupReservedName(method.Name))
        {
            return false;
        }

        var functionScopeNames = CreateShouldRenderChildReservedLocalNames();
        foreach (var parameter in method.Parameters)
        {
            if (parameter.RefKind is not (RefKind.None or RefKind.In) ||
                !TryReserveShouldRenderBindingIdentifier(parameter.Name, functionScopeNames))
            {
                return false;
            }
        }

        return TryReserveShouldRenderLocalFunctionBodyBindings(operation, functionScopeNames);
    }

    private static bool TryReserveShouldRenderLocalFunctionBodyBindings(
        ILocalFunctionOperation operation,
        HashSet<string> reservedNames)
    {
        foreach (var current in operation.DescendantsAndSelf())
        {
            switch (RazorVueOperationNormalizer.Unwrap(current))
            {
                case IAnonymousFunctionOperation:
                    return false;

                case ILocalFunctionOperation localFunction when !ReferenceEquals(localFunction, operation):
                    return false;

                case ISimpleAssignmentOperation assignment when !IsShouldRenderLocalMutationTarget(assignment.Target):
                    return false;

                case ISimpleAssignmentOperation assignment:
                    if (ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value))
                        return false;

                    break;

                case ICompoundAssignmentOperation assignment when !IsShouldRenderLocalMutationTarget(assignment.Target):
                    return false;

                case ICompoundAssignmentOperation assignment:
                    if (ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value))
                        return false;

                    break;

                case IIncrementOrDecrementOperation incrementOrDecrement when !IsShouldRenderLocalMutationTarget(incrementOrDecrement.Target):
                    return false;

                case IVariableDeclaratorOperation declarator:
                    if (!TryReserveShouldRenderBindingIdentifier(declarator.Symbol.Name, reservedNames))
                        return false;

                    break;

                case IForLoopOperation forLoop:
                    foreach (var local in forLoop.Locals)
                    {
                        if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                            return false;
                    }

                    break;

                case IForEachLoopOperation forEachLoop:
                    foreach (var local in forEachLoop.Locals)
                    {
                        if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                            return false;
                    }

                    break;

                case ICatchClauseOperation catchClause:
                    foreach (var local in CollectShouldRenderDeclaredLocals(catchClause.ExceptionDeclarationOrExpression))
                    {
                        if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                            return false;
                    }

                    break;

                case IDeclarationExpressionOperation declarationExpression:
                    foreach (var local in CollectShouldRenderDeclaredLocals(declarationExpression.Expression))
                    {
                        if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                            return false;
                    }

                    break;

                case IDeclarationPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                        return false;

                    break;

                case IRecursivePatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                        return false;

                    break;

                case IListPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryReserveShouldRenderBindingIdentifier(local.Name, reservedNames))
                        return false;

                    break;
            }
        }

        return true;
    }

    private static bool ContainsShouldRenderAnonymousFunction(ImmutableArray<IOperation> operations)
    {
        foreach (var operation in operations)
        {
            if (ContainsShouldRenderAnonymousFunction(operation))
                return true;
        }

        return false;
    }

    private static bool ContainsShouldRenderAnonymousFunction(IOperation operation)
    {
        foreach (var current in operation.DescendantsAndSelf())
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is IAnonymousFunctionOperation)
                return true;
        }

        return false;
    }

    private static IEnumerable<ILocalSymbol> CollectShouldRenderDeclaredLocals(IOperation? operation)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case ILocalReferenceOperation localReference:
                yield return localReference.Local;
                break;

            case IVariableDeclaratorOperation declarator:
                yield return declarator.Symbol;
                break;

            case IDeclarationExpressionOperation declarationExpression:
                foreach (var local in CollectShouldRenderDeclaredLocals(declarationExpression.Expression))
                    yield return local;

                break;

            case ITupleOperation tuple:
                foreach (var element in tuple.Elements)
                {
                    foreach (var local in CollectShouldRenderDeclaredLocals(element))
                        yield return local;
                }

                break;
        }
    }

    private static bool TryCreateShouldRenderReservedLocalNames(
        ImmutableArray<IOperation> operations,
        out HashSet<string> reservedNames)
    {
        reservedNames = CreateShouldRenderChildReservedLocalNames();
        foreach (var operation in EnumerateShouldRenderMethodScopedOperations(operations))
        {
            if (RazorVueOperationNormalizer.Unwrap(operation) is ILocalFunctionOperation localFunction &&
                !TryReserveShouldRenderBindingIdentifier(localFunction.Symbol.Name, reservedNames))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryCreateShouldRenderExpressionReservedLocalNames(
        IOperation operation,
        out HashSet<string> reservedNames)
    {
        reservedNames = CreateShouldRenderChildReservedLocalNames();
        foreach (var current in EnumerateShouldRenderExpressionScopedOperations(operation))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is ILocalFunctionOperation localFunction &&
                !TryReserveShouldRenderBindingIdentifier(localFunction.Symbol.Name, reservedNames))
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<string> CreateShouldRenderChildReservedLocalNames()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "props",
            "emit",
            "slots",
            "expose",
            "attrs",
            "__jazorRawProps",
            "__jazorShouldRenderHasRendered",
            "__jazorShouldRenderCachedVNode",
            "__jazorNextVNode",
            RazorVueExpressionEmitter.ImperativeRenderContextAlias
        };
    }

    private static bool TryReserveShouldRenderBindingIdentifier(string name, HashSet<string> reservedNames)
        => IsJavaScriptBindingIdentifier(name) &&
           !IsShouldRenderSetupReservedName(name) &&
           reservedNames.Add(name);

    private static bool IsShouldRenderSetupReservedName(string name)
        => name is "props"
            or "emit"
            or "slots"
            or "expose"
            or "attrs"
            or "__jazorRawProps"
            or "__jazorShouldRenderHasRendered"
            or "__jazorShouldRenderCachedVNode"
            or "__jazorNextVNode"
            or RazorVueExpressionEmitter.ImperativeRenderContextAlias;

    private static bool IsJavaScriptBindingIdentifier(string? name)
    {
        if (name is not { Length: > 0 } ||
            IsJavaScriptReservedBindingIdentifier(name) ||
            !IsJavaScriptIdentifierStart(name[0]))
        {
            return false;
        }

        for (var index = 1; index < name.Length; index++)
        {
            if (!IsJavaScriptIdentifierPart(name[index]))
                return false;
        }

        return true;
    }

    private static bool IsJavaScriptReservedBindingIdentifier(string name)
        => name is "arguments"
            or "await"
            or "break"
            or "case"
            or "catch"
            or "class"
            or "const"
            or "continue"
            or "debugger"
            or "default"
            or "delete"
            or "do"
            or "else"
            or "enum"
            or "eval"
            or "export"
            or "extends"
            or "false"
            or "finally"
            or "for"
            or "function"
            or "if"
            or "implements"
            or "import"
            or "in"
            or "instanceof"
            or "interface"
            or "let"
            or "new"
            or "null"
            or "package"
            or "private"
            or "protected"
            or "public"
            or "return"
            or "static"
            or "super"
            or "switch"
            or "this"
            or "throw"
            or "true"
            or "try"
            or "typeof"
            or "var"
            or "void"
            or "while"
            or "with"
            or "yield";

    private static bool IsJavaScriptIdentifierStart(char value)
        => value is '$' or '_' ||
           char.GetUnicodeCategory(value) is
               System.Globalization.UnicodeCategory.UppercaseLetter or
               System.Globalization.UnicodeCategory.LowercaseLetter or
               System.Globalization.UnicodeCategory.TitlecaseLetter or
               System.Globalization.UnicodeCategory.ModifierLetter or
               System.Globalization.UnicodeCategory.OtherLetter or
               System.Globalization.UnicodeCategory.LetterNumber;

    private static bool IsJavaScriptIdentifierPart(char value)
        => IsJavaScriptIdentifierStart(value) ||
           value is '\u200C' or '\u200D' ||
           char.GetUnicodeCategory(value) is
               System.Globalization.UnicodeCategory.NonSpacingMark or
               System.Globalization.UnicodeCategory.SpacingCombiningMark or
               System.Globalization.UnicodeCategory.DecimalDigitNumber or
               System.Globalization.UnicodeCategory.ConnectorPunctuation;

    private static ShouldRenderAnalysis AnalyzeShouldRenderExpression(
        Compilation compilation,
        IMethodSymbol method,
        RazorVueExpressionEmitter? expressionEmitter,
        ExpressionSyntax expression,
        HashSet<IMethodSymbol> visitedMethods)
    {
        expression = UnwrapLifecycleExpression(expression);
        if (IsConstantTrueShouldRenderExpression(expression))
            return ShouldRenderAnalysis.NoGate;

        if (TryAnalyzeBaseShouldRenderExpression(compilation, method, expressionEmitter, expression, visitedMethods, out var baseAnalysis))
            return baseAnalysis;

        if (expressionEmitter is null)
            return ShouldRenderAnalysis.Unsupported;

        try
        {
            if (!TryGetShouldRenderExpressionOperation(compilation, expression, out var operation))
                return ShouldRenderAnalysis.Unsupported;

            if (ContainsShouldRenderUnsupportedExpressionConstruct(operation))
                return ShouldRenderAnalysis.Unsupported;

            if (!TryCreateShouldRenderExpressionLocalAliases(operation, out var localAliases))
                return ShouldRenderAnalysis.Unsupported;

            var condition = expressionEmitter.CaptureSetupDependencies(
                () => expressionEmitter.WithScopedLocalAliases(
                    localAliases,
                    () => expressionEmitter.EmitSetupExpression(operation))).Expression;
            return new ShouldRenderAnalysis(
                true,
                RequiresRenderGate: true,
                condition);
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            return ShouldRenderAnalysis.Unsupported;
        }
    }

    private static bool TryAnalyzeBaseShouldRenderExpression(
        Compilation compilation,
        IMethodSymbol method,
        RazorVueExpressionEmitter? expressionEmitter,
        ExpressionSyntax expression,
        HashSet<IMethodSymbol> visitedMethods,
        out ShouldRenderAnalysis analysis)
    {
        analysis = ShouldRenderAnalysis.Unsupported;
        expression = UnwrapLifecycleExpression(expression);
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
            return true;

        var semanticModel = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
        if (semanticModel.GetOperation(invocationExpression) is not IInvocationOperation invocation)
            return true;

        if (SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType.OriginalDefinition, componentBase))
        {
            analysis = ShouldRenderAnalysis.NoGate;
            return true;
        }

        var baseMethod = FindBaseLifecycleMethod(method);
        if (baseMethod is null ||
            !SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.OriginalDefinition, baseMethod.OriginalDefinition))
        {
            return true;
        }

        analysis = AnalyzeShouldRender(compilation, baseMethod, expressionEmitter, visitedMethods);
        return true;
    }

    private static bool TryGetShouldRenderExpressionOperation(
        Compilation compilation,
        ExpressionSyntax expression,
        out IOperation operation)
    {
        var semanticModel = compilation.GetSemanticModel(expression.SyntaxTree);
        operation = semanticModel.GetOperation(expression)!;
        return operation is not null;
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
        if (emitCall.PreludeBindings.IsDefaultOrEmpty)
            return emitCall.EmitName + "|" + payload;

        var prelude = string.Join(
            ";",
            emitCall.PreludeBindings.Select(static binding => binding.Code));
        return emitCall.EmitName + "|" + payload + "|locals:" + prelude;
    }

    private static string DescribeEmitCallSequenceShape(ImmutableArray<SupportedEmitCall> emitCalls)
        => string.Join(">", emitCalls.Select(DescribeEmitCallShape));

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
        ShouldRenderGatePlan? ShouldRenderGate,
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
        ImmutableArray<SupportedEmitCall> EmitCalls,
        bool UsesImmediateWatch,
        string WatchSource);

    internal readonly record struct ShouldRenderGatePlan(string ConditionExpression);

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
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> PreludeBindings);
    private sealed record SetParametersAsyncAnalysis(
        bool IsSupported,
        ImmutableArray<SupportedEmitCall> EmitCalls)
    {
        public static SetParametersAsyncAnalysis Unsupported { get; } = new(false, ImmutableArray<SupportedEmitCall>.Empty);
        public static SetParametersAsyncAnalysis NoOp { get; } = new(true, ImmutableArray<SupportedEmitCall>.Empty);
    }
    private sealed record ShouldRenderAnalysis(
        bool IsSupported,
        bool RequiresRenderGate,
        string ExpressionText)
    {
        public static ShouldRenderAnalysis Unsupported { get; } = new(false, false, string.Empty);
        public static ShouldRenderAnalysis NoGate { get; } = new(true, false, string.Empty);
    }
}
