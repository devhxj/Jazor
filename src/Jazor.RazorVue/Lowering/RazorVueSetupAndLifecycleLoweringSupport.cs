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
        var setupBlocks = new List<string>();

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

        foreach (var setupBlock in setupBlocks)
            builder.Append(setupBlock);

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
                    setupBlocks.Add(result.Block);
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
                    setupBlocks.Add(result.Block);
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
                    setupBlocks.Add(result.Block);
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
                if (RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedSetupPropertyReason(
                        snapshot.Compilation,
                        property.PropertySymbol,
                        out var propertyReason))
                {
                    throw CreateUnsupportedSetupLoweringException(property.PropertySymbol, propertyReason);
                }

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
        => !AnalyzeSetParametersAsync(snapshot, expressionEmitter: null, snapshot.SetParametersAsyncMethod).Statements.IsDefaultOrEmpty;

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

        return analysis.Statements.IsDefaultOrEmpty
            ? "none"
            : DescribeLifecycleStatementSequenceShape(analysis.Statements);
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

    private static void AppendLifecycleStatements(
        StringBuilder builder,
        ImmutableArray<SupportedLifecycleStatement> statements,
        bool awaitResult,
        string indent)
    {
        if (statements.IsDefaultOrEmpty)
            return;

        foreach (var statement in statements)
            AppendLifecycleStatement(builder, statement, awaitResult, indent);
    }

    private static void AppendLifecycleStatement(
        StringBuilder builder,
        SupportedLifecycleStatement statement,
        bool awaitResult,
        string indent)
    {
        switch (statement)
        {
            case SupportedLifecycleEmitStatement emit:
                AppendEmitStatement(builder, emit.EmitCall, awaitResult, payloadOverride: null, indent);
                return;

            case SupportedLifecycleIfStatement conditional:
                AppendLifecyclePreludeBindings(builder, conditional.ConditionPreludeBindings, indent);
                builder.Append(indent).Append("if (").Append(conditional.ConditionExpression).AppendLine(") {");
                AppendLifecycleStatements(builder, conditional.WhenTrue, awaitResult, indent + "  ");
                if (!conditional.WhenFalse.IsDefaultOrEmpty)
                {
                    builder.Append(indent).AppendLine("} else {");
                    AppendLifecycleStatements(builder, conditional.WhenFalse, awaitResult, indent + "  ");
                }

                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleGuardReturnStatement guardReturn:
                AppendLifecyclePreludeBindings(builder, guardReturn.ConditionPreludeBindings, indent);
                builder.Append(indent).Append("if (").Append(guardReturn.ConditionExpression).AppendLine(") {");
                builder.Append(indent).AppendLine("  return;");
                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleReturnStatement returnStatement:
                AppendLifecyclePreludeBindings(builder, returnStatement.PreludeBindings, indent);
                builder.Append(indent).AppendLine("return;");
                return;

            case SupportedLifecycleIfReturnStatement ifReturn:
                AppendLifecyclePreludeBindings(builder, ifReturn.ConditionPreludeBindings, indent);
                builder.Append(indent).Append("if (").Append(ifReturn.ConditionExpression).AppendLine(") {");
                if (ifReturn.ReturnsWhenTrue)
                {
                    builder.Append(indent).AppendLine("  return;");
                }
                else
                {
                    AppendLifecycleStatements(builder, ifReturn.WhenTrue, awaitResult, indent + "  ");
                }

                if (!ifReturn.WhenFalse.IsDefaultOrEmpty || !ifReturn.ReturnsWhenTrue)
                {
                    builder.Append(indent).AppendLine("} else {");
                    if (ifReturn.ReturnsWhenTrue)
                    {
                        AppendLifecycleStatements(builder, ifReturn.WhenFalse, awaitResult, indent + "  ");
                    }
                    else
                    {
                        builder.Append(indent).AppendLine("  return;");
                    }
                }

                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleTerminalIfReturnStatement terminalIfReturn:
                AppendLifecyclePreludeBindings(builder, terminalIfReturn.ConditionPreludeBindings, indent);
                builder.Append(indent).Append("if (").Append(terminalIfReturn.ConditionExpression).AppendLine(") {");
                builder.Append(indent).AppendLine("  return;");
                builder.Append(indent).AppendLine("} else {");
                builder.Append(indent).AppendLine("  return;");
                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleSwitchStatement switchStatement:
                AppendLifecyclePreludeBindings(builder, switchStatement.ValuePreludeBindings, indent);
                builder.Append(indent).Append("switch (").Append(switchStatement.ValueExpression).AppendLine(") {");
                foreach (var section in switchStatement.Sections)
                {
                    foreach (var label in section.Labels)
                    {
                        builder.Append(indent).Append("  ");
                        if (label.IsDefault)
                            builder.AppendLine("default:");
                        else
                            builder.Append("case ").Append(label.Expression).AppendLine(":");
                    }

                    AppendLifecycleStatements(builder, section.Statements, awaitResult, indent + "    ");
                    builder.Append(indent).AppendLine("    break;");
                }

                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecyclePatternSwitchStatement patternSwitchStatement:
                AppendLifecyclePreludeBindings(builder, patternSwitchStatement.ValuePreludeBindings, indent);
                for (var index = 0; index < patternSwitchStatement.Sections.Length; index++)
                {
                    var section = patternSwitchStatement.Sections[index];
                    if (index == 0)
                    {
                        if (section.IsDefault)
                            builder.Append(indent).AppendLine("if (true) {");
                        else
                            builder.Append(indent).Append("if (").Append(section.ConditionExpression).AppendLine(") {");
                    }
                    else if (section.IsDefault)
                    {
                        builder.Append(indent).AppendLine("} else {");
                    }
                    else
                    {
                        builder.Append(indent).Append("} else if (").Append(section.ConditionExpression).AppendLine(") {");
                    }

                    AppendLifecycleStatements(builder, section.Statements, awaitResult, indent + "  ");
                }

                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleCompilerStatement compilerStatement:
                AppendLifecyclePreludeBindings(builder, compilerStatement.PreludeBindings, indent);
                AppendLifecycleCompilerStatement(builder, compilerStatement.StatementText, indent);
                return;

            case SupportedLifecycleTryFinallyStatement tryFinally:
                AppendLifecyclePreludeBindings(builder, tryFinally.TryPreludeBindings, indent);
                builder.Append(indent).AppendLine("try {");
                AppendLifecycleStatements(builder, tryFinally.TryStatements, awaitResult, indent + "  ");
                builder.Append(indent).AppendLine("} finally {");
                AppendLifecycleStatements(builder, tryFinally.FinallyStatements, awaitResult, indent + "  ");
                builder.Append(indent).AppendLine("}");
                return;

            case SupportedLifecycleTryCatchStatement tryCatch:
                AppendLifecyclePreludeBindings(builder, tryCatch.TryPreludeBindings, indent);
                builder.Append(indent).AppendLine("try {");
                AppendLifecycleStatements(builder, tryCatch.TryStatements, awaitResult, indent + "  ");
                if (tryCatch.CatchFilterExpression is null)
                {
                    builder.Append(indent).AppendLine("} catch {");
                    AppendLifecycleStatements(builder, tryCatch.CatchStatements, awaitResult, indent + "  ");
                }
                else
                {
                    builder.Append(indent).AppendLine("} catch (__jazorLifecycleCatch) {");
                    builder.Append(indent).AppendLine("  let __jazorLifecycleCatchHandled = false;");
                    builder.Append(indent).AppendLine("  try {");
                    AppendLifecyclePreludeBindings(builder, tryCatch.CatchFilterPreludeBindings, indent + "    ");
                    builder.Append(indent).Append("    __jazorLifecycleCatchHandled = ").Append(tryCatch.CatchFilterExpression).AppendLine(";");
                    builder.Append(indent).AppendLine("  } catch {");
                    builder.Append(indent).AppendLine("    __jazorLifecycleCatchHandled = false;");
                    builder.Append(indent).AppendLine("  }");
                    builder.Append(indent).AppendLine("  if (__jazorLifecycleCatchHandled) {");
                    AppendLifecycleStatements(builder, tryCatch.CatchStatements, awaitResult, indent + "    ");
                    builder.Append(indent).AppendLine("  } else {");
                    builder.Append(indent).AppendLine("    throw __jazorLifecycleCatch;");
                    builder.Append(indent).AppendLine("  }");
                }

                if (tryCatch.HasFinally)
                {
                    builder.Append(indent).AppendLine("} finally {");
                    AppendLifecycleStatements(builder, tryCatch.FinallyStatements, awaitResult, indent + "  ");
                }

                builder.Append(indent).AppendLine("}");
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(statement), statement.GetType().FullName, "Unsupported lifecycle statement.");
        }
    }

    private static void AppendLifecycleCompilerStatement(StringBuilder builder, string statementText, string indent)
    {
        var normalized = Util.NormalizeLineEndingsToLf(statementText).Trim();
        if (string.IsNullOrEmpty(normalized))
            return;

        foreach (var line in normalized.Split('\n'))
            builder.Append(indent).AppendLine(line);
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

    private static void AppendLifecyclePreludeBindings(
        StringBuilder builder,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> bindings,
        string indent)
    {
        if (bindings.IsDefaultOrEmpty)
            return;

        foreach (var binding in bindings)
            builder.Append(indent).AppendLine(binding.Code);
    }

    private static ImmutableArray<SupportedLifecycleStatement> ConcatLifecycleStatements(
        ImmutableArray<SupportedLifecycleStatement> first,
        ImmutableArray<SupportedLifecycleStatement> second)
    {
        if (first.IsDefaultOrEmpty)
            return second.IsDefault ? ImmutableArray<SupportedLifecycleStatement>.Empty : second;
        if (second.IsDefaultOrEmpty)
            return first;

        var builder = ImmutableArray.CreateBuilder<SupportedLifecycleStatement>(first.Length + second.Length);
        builder.AddRange(first);
        builder.AddRange(second);
        return builder.ToImmutable();
    }

    private static ImmutableArray<SupportedLifecycleStatement> CreateLifecycleEmitStatementSequence(SupportedEmitCall emitCall)
        => ImmutableArray.Create<SupportedLifecycleStatement>(new SupportedLifecycleEmitStatement(emitCall));

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
            Statements: CreateLifecycleEmitStatementSequence(emitCall),
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
            Statements: CreateLifecycleEmitStatementSequence(emitCall),
            UsesImmediateWatch: true,
            WatchSource: BuildPropsWatchSource(snapshot.Descriptor));
    }

    private static LifecycleHookPlan? TryPlanSetParametersAsyncHook(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter)
    {
        var analysis = AnalyzeSetParametersAsync(snapshot, expressionEmitter, snapshot.SetParametersAsyncMethod);
        if (!analysis.IsSupported || analysis.Statements.IsDefaultOrEmpty)
            return null;

        return new LifecycleHookPlan(
            HookKind: LifecycleHookKind.ImmediateWatch,
            HookName: "watch",
            AwaitResult: true,
            Statements: analysis.Statements,
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
            Statements: CreateLifecycleEmitStatementSequence(emitCall),
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
        if (plan is not { } hookPlan || hookPlan.Statements.IsDefaultOrEmpty)
            return;

        var statements = hookPlan.Statements;

        switch (hookPlan.HookKind)
        {
            case LifecycleHookKind.Standard:
                builder.Append(indent).Append(hookPlan.HookName).Append("(");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendLifecycleStatements(builder, statements, hookPlan.AwaitResult, indent + "  ");
                builder.Append(indent).AppendLine("});");
                return;

            case LifecycleHookKind.ImmediateWatch:
                builder.Append(indent).Append("watch(() => ").Append(hookPlan.WatchSource).Append(", ");
                if (hookPlan.AwaitResult)
                    builder.Append("async ");
                builder.AppendLine("() => {");
                AppendLifecycleStatements(builder, statements, hookPlan.AwaitResult, indent + "  ");
                builder.Append(indent).AppendLine("}, { immediate: true });");
                return;

            case LifecycleHookKind.AfterRender:
                if (statements[0] is not SupportedLifecycleEmitStatement emitStatement)
                    throw new InvalidOperationException("AfterRender lifecycle hook requires a single emit statement.");

                var emitCall = emitStatement.EmitCall;
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

        if (IsNoOpLocalOnlyLifecycleBody(snapshot.Compilation, method, methodSyntax.Body))
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

    private static bool IsNoOpLocalOnlyLifecycleBody(
        Compilation compilation,
        IMethodSymbol method,
        BlockSyntax body)
    {
        if (body.Statements.Count == 0)
            return true;

        var semanticModel = compilation.GetSemanticModel(body.SyntaxTree);
        IReadOnlyDictionary<ILocalSymbol, IOperation>? localInitializers = null;
        if (semanticModel.GetOperation(body) is IBlockOperation blockOperation)
        {
            localInitializers = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                compilation,
                blockOperation.Operations,
                RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);
        }

        for (var index = 0; index < body.Statements.Count; index++)
        {
            var statement = body.Statements[index];
            switch (statement)
            {
                case EmptyStatementSyntax:
                    continue;

                case LocalDeclarationStatementSyntax localDeclaration
                    when IsNoOpLifecycleLocalDeclaration(
                        compilation,
                        method,
                        localDeclaration,
                        semanticModel,
                        body,
                        allowProjectedStructuredLocalInitializer: true,
                        sourceStableLocalInitializers: localInitializers):
                    continue;

                case ExpressionStatementSyntax expressionStatement
                    when IsNoOpLifecycleExpression(compilation, method, expressionStatement.Expression):
                    continue;

                case ExpressionStatementSyntax expressionStatement
                    when IsIgnorableNoOpLifecycleDiscardedValueHelperInvocation(
                        compilation,
                        method,
                        expressionStatement,
                        semanticModel,
                        paramsArrayLengthParameters: null,
                        sourceStableLocalInitializers: null,
                        new HashSet<ISymbol>(SymbolEqualityComparer.Default)):
                    continue;

                case ExpressionStatementSyntax expressionStatement
                    when IsIgnorableCurrentComponentNoOpLifecycleVoidHelperInvocation(
                        compilation,
                        method,
                        expressionStatement,
                        semanticModel):
                    continue;

                case IfStatementSyntax ifStatement
                    when index == body.Statements.Count - 1 &&
                         IsNoOpLifecycleTerminalIfReturnStatement(compilation, method, ifStatement, semanticModel, localInitializers):
                    return true;

                case ReturnStatementSyntax returnStatement
                    when index == body.Statements.Count - 1 &&
                         (returnStatement.Expression is null ||
                          IsNoOpLifecycleExpression(compilation, method, returnStatement.Expression)):
                    return true;

                default:
                    return false;
            }
        }

        return true;
    }

    private static bool IsNoOpLifecycleLocalDeclaration(
        Compilation compilation,
        IMethodSymbol method,
        LocalDeclarationStatementSyntax statement,
        SemanticModel semanticModel,
        BlockSyntax? containingBody = null,
        bool allowProjectedStructuredLocalInitializer = false,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null)
    {
        if (statement.UsingKeyword != default ||
            statement.AwaitKeyword != default ||
            statement.Declaration.Type is RefTypeSyntax)
        {
            return false;
        }

        foreach (var variable in statement.Declaration.Variables)
        {
            if (variable.Initializer?.Value is null)
                continue;

            if (variable.Initializer.Value is RefExpressionSyntax)
                return false;

            var initializer = semanticModel.GetOperation(variable.Initializer.Value);
            if (!IsIgnorableNoOpLifecycleValueExpression(
                    compilation,
                    method,
                    initializer,
                    new HashSet<ISymbol>(SymbolEqualityComparer.Default),
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers) &&
                !IsIgnorableNoOpLifecycleUnusedArrayLocalInitializer(
                    compilation,
                    method,
                    statement,
                    variable,
                    initializer,
                    semanticModel,
                    containingBody,
                    paramsArrayLengthParameters) &&
                !IsIgnorableNoOpLifecycleUnusedAnonymousObjectLocalInitializer(
                    compilation,
                    method,
                    statement,
                    variable,
                    initializer,
                    semanticModel,
                    containingBody,
                    paramsArrayLengthParameters) &&
                !(allowProjectedStructuredLocalInitializer &&
                  IsIgnorableNoOpLifecycleProjectedStructuredLocalInitializer(
                    compilation,
                    method,
                    statement,
                    variable,
                    initializer,
                    semanticModel,
                    containingBody,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsIgnorableNoOpLifecycleUnusedArrayLocalInitializer(
        Compilation compilation,
        IMethodSymbol method,
        LocalDeclarationStatementSyntax declaration,
        VariableDeclaratorSyntax variable,
        IOperation? initializer,
        SemanticModel semanticModel,
        BlockSyntax? containingBody,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (containingBody is null ||
            declaration.Declaration.Variables.Count != 1 ||
            semanticModel.GetDeclaredSymbol(variable) is not ILocalSymbol local ||
            local.Type is not IArrayTypeSymbol ||
            semanticModel.GetOperation(containingBody) is not IBlockOperation blockOperation ||
            IsNoOpLifecycleLocalReferencedOutsideInitializer(blockOperation, local, initializer))
        {
            return false;
        }

        return IsIgnorableNoOpLifecycleArrayCarrier(
            compilation,
            method,
            initializer,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers: null,
            new HashSet<ISymbol>(SymbolEqualityComparer.Default));
    }

    private static bool IsIgnorableNoOpLifecycleUnusedAnonymousObjectLocalInitializer(
        Compilation compilation,
        IMethodSymbol method,
        LocalDeclarationStatementSyntax declaration,
        VariableDeclaratorSyntax variable,
        IOperation? initializer,
        SemanticModel semanticModel,
        BlockSyntax? containingBody,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (containingBody is null ||
            declaration.Declaration.Variables.Count != 1 ||
            semanticModel.GetDeclaredSymbol(variable) is not ILocalSymbol local ||
            local.Type is not INamedTypeSymbol { IsAnonymousType: true } ||
            semanticModel.GetOperation(containingBody) is not IBlockOperation blockOperation ||
            IsNoOpLifecycleLocalReferencedOutsideInitializer(blockOperation, local, initializer))
        {
            return false;
        }

        return IsIgnorableNoOpLifecycleAnonymousObjectCarrier(
            compilation,
            method,
            initializer,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers: null,
            new HashSet<ISymbol>(SymbolEqualityComparer.Default));
    }

    private static bool IsIgnorableNoOpLifecycleProjectedStructuredLocalInitializer(
        Compilation compilation,
        IMethodSymbol method,
        LocalDeclarationStatementSyntax declaration,
        VariableDeclaratorSyntax variable,
        IOperation? initializer,
        SemanticModel semanticModel,
        BlockSyntax? containingBody,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (containingBody is null ||
            declaration.Declaration.Variables.Count != 1 ||
            semanticModel.GetDeclaredSymbol(variable) is not ILocalSymbol local ||
            semanticModel.GetOperation(containingBody) is not IBlockOperation blockOperation)
        {
            return false;
        }

        return local.Type switch
        {
            IArrayTypeSymbol => AreNoOpLifecycleLocalReferencesOnlySafeProjections(blockOperation, local, initializer) &&
                IsIgnorableNoOpLifecycleArrayCarrier(
                    compilation,
                    method,
                    initializer,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    new HashSet<ISymbol>(SymbolEqualityComparer.Default)),
            INamedTypeSymbol { IsAnonymousType: true } => AreNoOpLifecycleLocalReferencesOnlySafeProjections(blockOperation, local, initializer) &&
                IsIgnorableNoOpLifecycleAnonymousObjectCarrier(
                    compilation,
                    method,
                    initializer,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    new HashSet<ISymbol>(SymbolEqualityComparer.Default)),
            _ => false
        };
    }

    private static bool AreNoOpLifecycleLocalReferencesOnlySafeProjections(
        IOperation root,
        ILocalSymbol local,
        IOperation? initializer)
    {
        var sawReference = false;
        foreach (var current in EnumerateNoOpLifecycleOperations(root))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is not ILocalReferenceOperation localReference ||
                !SymbolEqualityComparer.Default.Equals(localReference.Local, local))
            {
                continue;
            }

            if (initializer is not null && IsOperationWithin(current, initializer))
                continue;

            sawReference = true;
            if (!IsNoOpLifecycleSafeStructuredLocalProjection(localReference))
                return false;
        }

        return sawReference;
    }

    private static bool IsNoOpLifecycleSafeStructuredLocalProjection(ILocalReferenceOperation localReference)
    {
        var parent = RazorVueOperationNormalizer.Unwrap(localReference.Parent);
        return parent switch
        {
            IPropertyReferenceOperation property
                when IsNoOpLifecycleArrayLengthProjection(localReference, property) => true,
            IPropertyReferenceOperation property
                when IsNoOpLifecycleAnonymousObjectProjection(localReference, property) => true,
            _ => false
        };
    }

    private static bool IsNoOpLifecycleArrayLengthProjection(
        ILocalReferenceOperation localReference,
        IPropertyReferenceOperation propertyReference)
        => localReference.Local.Type is IArrayTypeSymbol &&
           string.Equals(propertyReference.Property.Name, "Length", StringComparison.Ordinal) &&
           propertyReference.Arguments.Length == 0 &&
           ReferenceEquals(RazorVueOperationNormalizer.Unwrap(propertyReference.Instance), localReference);

    private static bool IsNoOpLifecycleAnonymousObjectProjection(
        ILocalReferenceOperation localReference,
        IPropertyReferenceOperation propertyReference)
        => localReference.Local.Type is INamedTypeSymbol { IsAnonymousType: true } localType &&
           propertyReference.Arguments.Length == 0 &&
           propertyReference.Property.ContainingType is not null &&
           SymbolEqualityComparer.Default.Equals(propertyReference.Property.ContainingType, localType) &&
           ReferenceEquals(RazorVueOperationNormalizer.Unwrap(propertyReference.Instance), localReference);

    private static bool IsNoOpLifecycleLocalReferencedOutsideInitializer(
        IOperation root,
        ILocalSymbol local,
        IOperation? initializer)
    {
        foreach (var current in EnumerateNoOpLifecycleOperations(root))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is not ILocalReferenceOperation localReference ||
                !SymbolEqualityComparer.Default.Equals(localReference.Local, local))
            {
                continue;
            }

            if (initializer is not null && IsOperationWithin(current, initializer))
                continue;

            return true;
        }

        return false;
    }

    private static bool IsOperationWithin(IOperation operation, IOperation root)
    {
        for (var current = operation; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, root))
                return true;
        }

        return false;
    }

    private static IEnumerable<IOperation> EnumerateNoOpLifecycleOperations(IOperation operation)
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

            foreach (var nested in EnumerateNoOpLifecycleOperations(child))
                yield return nested;
        }
    }

    private static bool IsNoOpLifecycleTerminalIfReturnStatement(
        Compilation compilation,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SemanticModel semanticModel,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null)
    {
        if (!TerminatesWithNoOpReturn(ifStatement, allowImplicitContinue: true))
            return false;

        var condition = semanticModel.GetOperation(ifStatement.Condition);
        return IsIgnorableNoOpLifecycleValueExpression(
            compilation,
            method,
            condition,
            new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            paramsArrayLengthParameters,
            sourceStableLocalInitializers);
    }

    private static bool IsIgnorableNoOpLifecycleValueExpression(
        Compilation compilation,
        IMethodSymbol method,
        IOperation? operation)
        => IsIgnorableNoOpLifecycleValueExpression(
            compilation,
            method,
            operation,
            new HashSet<ISymbol>(SymbolEqualityComparer.Default));

    private static bool IsIgnorableNoOpLifecycleValueExpression(
        Compilation compilation,
        IMethodSymbol method,
        IOperation? operation,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return false;

        if (current.ConstantValue.HasValue)
            return true;

        return current switch
        {
            IDefaultValueOperation => true,
            ITypeOfOperation => true,
            ILocalReferenceOperation localReference
                when IsIgnorableNoOpLifecycleSourceStableLocalReference(
                    compilation,
                    method,
                    localReference,
                    visitedSymbols,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers) => true,
            ILocalReferenceOperation => true,
            IParameterReferenceOperation => true,
            IPropertyReferenceOperation property
                when IsIgnorableNoOpLifecycleParamsArrayLength(property, paramsArrayLengthParameters) => true,
            IPropertyReferenceOperation property
                when IsIgnorableNoOpLifecycleSourceStableArrayLengthProjection(compilation, method, property, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IPropertyReferenceOperation property
                when IsIgnorableNoOpLifecycleSourceStableMemberArrayLengthProjection(compilation, method, property, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IPropertyReferenceOperation property
                when IsIgnorableNoOpLifecycleSourceStableAnonymousObjectProjection(compilation, method, property, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IPropertyReferenceOperation property
                when IsCurrentComponentParameterProperty(method, property) => true,
            IPropertyReferenceOperation property
                when IsCurrentComponentSourceStableValueMember(compilation, method, property, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IFieldReferenceOperation field
                when IsIgnorableNoOpLifecycleSourceStableTupleProjection(compilation, method, field, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IFieldReferenceOperation field
                when IsIgnorableNoOpLifecycleSourceStableMemberTupleProjection(compilation, method, field, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IFieldReferenceOperation field
                when IsCurrentComponentSourceStableValueMember(compilation, method, field, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IInvocationOperation invocation
                when IsIgnorableNoOpLifecycleParamsArrayGetLength(invocation, paramsArrayLengthParameters) => true,
            IInvocationOperation invocation
                when IsIgnorableCurrentComponentNoOpLifecycleHelperInvocation(compilation, method, invocation, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) => true,
            IConversionOperation conversion
                when conversion.OperatorMethod is null =>
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, conversion.Operand, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers),
            IUnaryOperation unary
                when unary.OperatorMethod is null =>
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, unary.Operand, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers),
            IBinaryOperation binary
                when binary.OperatorMethod is null =>
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, binary.LeftOperand, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) &&
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, binary.RightOperand, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers),
            IConditionalOperation conditional
                when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, conditional.Condition, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) &&
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, conditional.WhenTrue, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) &&
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, conditional.WhenFalse, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers),
            ICoalesceOperation coalesce =>
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, coalesce.Value, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers) &&
                IsIgnorableNoOpLifecycleValueExpression(compilation, method, coalesce.WhenNull, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers),
            ITupleOperation tuple =>
                tuple.Elements.All(element => IsIgnorableNoOpLifecycleValueExpression(compilation, method, element, visitedSymbols, paramsArrayLengthParameters, sourceStableLocalInitializers)),
            _ => false
        };
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableArrayLengthProjection(
        Compilation compilation,
        IMethodSymbol method,
        IPropertyReferenceOperation propertyReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (sourceStableLocalInitializers is null ||
            RazorVueOperationNormalizer.Unwrap(propertyReference.Instance) is not ILocalReferenceOperation localReference ||
            !IsNoOpLifecycleArrayLengthProjection(localReference, propertyReference) ||
            !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var initializer) ||
            !visitedSymbols.Add(localReference.Local))
        {
            return false;
        }

        try
        {
            return IsIgnorableNoOpLifecycleArrayCarrier(
                compilation,
                method,
                initializer,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                visitedSymbols);
        }
        finally
        {
            visitedSymbols.Remove(localReference.Local);
        }
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableMemberArrayLengthProjection(
        Compilation compilation,
        IMethodSymbol method,
        IPropertyReferenceOperation propertyReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (!string.Equals(propertyReference.Property.Name, "Length", StringComparison.Ordinal) ||
            propertyReference.Arguments.Length != 0 ||
            RazorVueOperationNormalizer.Unwrap(propertyReference.Instance) is not { } instance)
        {
            return false;
        }

        if (!TryGetCurrentComponentSourceStableValueMember(method, instance, out var sourceMember) ||
            !TryGetNoOpLifecycleArrayProjectionSourceInitializer(compilation, sourceMember, out var initializer) ||
            initializer is null ||
            !visitedSymbols.Add(sourceMember))
        {
            return false;
        }

        try
        {
            return IsIgnorableNoOpLifecycleArrayCarrier(
                compilation,
                method,
                initializer,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                visitedSymbols);
        }
        finally
        {
            visitedSymbols.Remove(sourceMember);
        }
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableAnonymousObjectProjection(
        Compilation compilation,
        IMethodSymbol method,
        IPropertyReferenceOperation propertyReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (sourceStableLocalInitializers is null ||
            RazorVueOperationNormalizer.Unwrap(propertyReference.Instance) is not ILocalReferenceOperation localReference ||
            !IsNoOpLifecycleAnonymousObjectProjection(localReference, propertyReference) ||
            !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var initializer) ||
            !visitedSymbols.Add(localReference.Local))
        {
            return false;
        }

        try
        {
            return IsIgnorableNoOpLifecycleAnonymousObjectCarrier(
                compilation,
                method,
                initializer,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                visitedSymbols);
        }
        finally
        {
            visitedSymbols.Remove(localReference.Local);
        }
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableLocalReference(
        Compilation compilation,
        IMethodSymbol method,
        ILocalReferenceOperation localReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (sourceStableLocalInitializers is null ||
            !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var initializer) ||
            !visitedSymbols.Add(localReference.Local))
        {
            return false;
        }

        try
        {
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                method,
                initializer,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers);
        }
        finally
        {
            visitedSymbols.Remove(localReference.Local);
        }
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableTupleProjection(
        Compilation compilation,
        IMethodSymbol method,
        IFieldReferenceOperation fieldReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (sourceStableLocalInitializers is null ||
            !TryGetNoOpLifecycleTupleFieldIndex(fieldReference.Field, out var fieldIndex) ||
            RazorVueOperationNormalizer.Unwrap(fieldReference.Instance) is not ILocalReferenceOperation localReference ||
            !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var initializer) ||
            !visitedSymbols.Add(localReference.Local))
        {
            return false;
        }

        var resolvedAliasLocals = new List<ISymbol>();
        try
        {
            if (!TryGetNoOpLifecycleTupleElement(
                    initializer,
                    fieldIndex,
                    sourceStableLocalInitializers,
                    visitedSymbols,
                    resolvedAliasLocals,
                    out var element))
            {
                return false;
            }

            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                method,
                element,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers);
        }
        finally
        {
            foreach (var aliasLocal in resolvedAliasLocals)
                visitedSymbols.Remove(aliasLocal);

            visitedSymbols.Remove(localReference.Local);
        }
    }

    private static bool IsIgnorableNoOpLifecycleSourceStableMemberTupleProjection(
        Compilation compilation,
        IMethodSymbol method,
        IFieldReferenceOperation fieldReference,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (!TryGetNoOpLifecycleTupleFieldIndex(fieldReference.Field, out var fieldIndex))
            return false;

        var instance = RazorVueOperationNormalizer.Unwrap(fieldReference.Instance);
        if (instance is null)
            return false;

        if (!TryGetCurrentComponentSourceStableValueMember(
                method,
                instance,
                out var sourceMember))
        {
            return false;
        }

        if (!TryGetNoOpLifecycleTupleProjectionSourceInitializer(compilation, sourceMember, out var initializer) ||
            initializer is null ||
            !visitedSymbols.Add(sourceMember))
        {
            return false;
        }

        try
        {
            var normalized = RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer;
            if (normalized is not ITupleOperation tuple ||
                fieldIndex < 0 ||
                fieldIndex >= tuple.Elements.Length ||
                !tuple.Elements.All(element =>
                    IsIgnorableNoOpLifecycleValueExpression(
                        compilation,
                        method,
                        element,
                        visitedSymbols,
                        paramsArrayLengthParameters,
                        sourceStableLocalInitializers)))
            {
                return false;
            }

            var element = RazorVueOperationNormalizer.Unwrap(tuple.Elements[fieldIndex]) ?? tuple.Elements[fieldIndex];
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                method,
                element,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers);
        }
        finally
        {
            visitedSymbols.Remove(sourceMember);
        }
    }

    private static bool TryGetNoOpLifecycleTupleProjectionSourceInitializer(
        Compilation compilation,
        ISymbol sourceMember,
        out IOperation? initializer)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(compilation, sourceMember, out initializer))
            return initializer is not null;

        if (sourceMember is IPropertySymbol property &&
            RazorVueCurrentComponentValueMemberHelper.TryGetSupportedPropertyLoweringKind(compilation, property, out var loweringKind) &&
            loweringKind == VueLogicPropertyLoweringKind.GetterFunction &&
            TryGetNoOpLifecycleGetterValueOperation(compilation, property, out initializer))
        {
            return initializer is not null;
        }

        initializer = null;
        return false;
    }

    private static bool TryGetNoOpLifecycleArrayProjectionSourceInitializer(
        Compilation compilation,
        ISymbol sourceMember,
        out IOperation? initializer)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(compilation, sourceMember, out initializer) &&
            sourceMember switch
            {
                IPropertySymbol property => property.Type is IArrayTypeSymbol,
                IFieldSymbol field => field.Type is IArrayTypeSymbol,
                _ => false
            })
        {
            return initializer is not null;
        }

        initializer = null;
        return false;
    }

    private static bool TryGetNoOpLifecycleGetterValueOperation(
        Compilation compilation,
        IPropertySymbol property,
        out IOperation? operation)
    {
        operation = null;
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.Initializer?.Value is not null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation) &&
                RazorVueOperationNormalizer.Unwrap(propertyOperation) is { } valueOperation)
            {
                operation = valueOperation;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetCurrentComponentSourceStableValueMember(
        IMethodSymbol method,
        IOperation instance,
        out ISymbol member)
    {
        member = default!;
        switch (instance)
        {
            case IPropertyReferenceOperation propertyReference:
                var property = propertyReference.Property;
                if (property.IsStatic ||
                    property.IsIndexer ||
                    propertyReference.Arguments.Length != 0 ||
                    property.ContainingType is null ||
                    !ContainsTypeOrBase(method.ContainingType, property.ContainingType))
                {
                    return false;
                }

                var propertyInstance = RazorVueOperationNormalizer.Unwrap(propertyReference.Instance);
                if (propertyInstance is not null and not IInstanceReferenceOperation)
                    return false;

                member = property;
                return true;

            case IFieldReferenceOperation fieldReference:
                var field = fieldReference.Field;
                if (field.IsStatic ||
                    field.ContainingType is null ||
                    !ContainsTypeOrBase(method.ContainingType, field.ContainingType))
                {
                    return false;
                }

                var fieldInstance = RazorVueOperationNormalizer.Unwrap(fieldReference.Instance);
                if (fieldInstance is not null and not IInstanceReferenceOperation)
                    return false;

                member = field;
                return true;

            default:
                return false;
        }
    }

    private static bool TryGetNoOpLifecycleTupleFieldIndex(IFieldSymbol field, out int fieldIndex)
    {
        fieldIndex = -1;
        var tupleField = field.CorrespondingTupleField ?? field;
        if (field.CorrespondingTupleField is null &&
            field.ContainingType is not { IsTupleType: true })
        {
            return false;
        }

        var name = tupleField.Name;
        if (!name.StartsWith("Item", StringComparison.Ordinal) ||
            name.Length <= 4 ||
            !int.TryParse(name.Substring(4), out var oneBasedIndex) ||
            oneBasedIndex <= 0)
        {
            return false;
        }

        fieldIndex = oneBasedIndex - 1;
        return true;
    }

    private static bool TryGetNoOpLifecycleTupleElement(
        IOperation initializer,
        int fieldIndex,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols,
        List<ISymbol> resolvedAliasLocals,
        out IOperation element)
    {
        element = default!;
        var current = RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer;

        while (current is ILocalReferenceOperation localReference)
        {
            if (sourceStableLocalInitializers is null ||
                !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var aliasInitializer) ||
                !visitedSymbols.Add(localReference.Local))
            {
                return false;
            }

            resolvedAliasLocals.Add(localReference.Local);
            current = RazorVueOperationNormalizer.Unwrap(aliasInitializer) ?? aliasInitializer;
        }

        if (current is not ITupleOperation tuple ||
            fieldIndex < 0 ||
            fieldIndex >= tuple.Elements.Length)
        {
            return false;
        }

        element = RazorVueOperationNormalizer.Unwrap(tuple.Elements[fieldIndex]) ?? tuple.Elements[fieldIndex];
        return true;
    }

    private static bool IsIgnorableNoOpLifecycleParamsArrayLength(
        IPropertyReferenceOperation propertyReference,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (paramsArrayLengthParameters is null ||
            paramsArrayLengthParameters.Count == 0 ||
            !string.Equals(propertyReference.Property.Name, "Length", StringComparison.Ordinal) ||
            propertyReference.Arguments.Length != 0)
        {
            return false;
        }

        return RazorVueOperationNormalizer.Unwrap(propertyReference.Instance) is IParameterReferenceOperation parameterReference &&
               parameterReference.Parameter.Type is IArrayTypeSymbol &&
               paramsArrayLengthParameters.Contains(parameterReference.Parameter);
    }

    private static bool IsIgnorableNoOpLifecycleParamsArrayGetLength(
        IInvocationOperation invocation,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (paramsArrayLengthParameters is null ||
            paramsArrayLengthParameters.Count == 0 ||
            !string.Equals(invocation.TargetMethod.Name, "GetLength", StringComparison.Ordinal) ||
            invocation.TargetMethod.MethodKind != MethodKind.Ordinary ||
            invocation.TargetMethod.Parameters.Length != 1 ||
            invocation.TargetMethod.Parameters[0].Type.SpecialType != SpecialType.System_Int32 ||
            invocation.TargetMethod.ReturnType.SpecialType != SpecialType.System_Int32 ||
            invocation.Arguments.Length != 1)
        {
            return false;
        }

        if (RazorVueOperationNormalizer.Unwrap(invocation.Instance) is not IParameterReferenceOperation parameterReference ||
            parameterReference.Parameter.Type is not IArrayTypeSymbol { Rank: 1 } ||
            !paramsArrayLengthParameters.Contains(parameterReference.Parameter))
        {
            return false;
        }

        var dimensionArgument = invocation.Arguments[0];
        if (dimensionArgument.Parameter is null ||
            dimensionArgument.Parameter.RefKind != RefKind.None ||
            dimensionArgument.ArgumentKind != ArgumentKind.Explicit)
        {
            return false;
        }

        var dimensionValue = RazorVueOperationNormalizer.Unwrap(dimensionArgument.Value);
        return dimensionValue?.ConstantValue is { HasValue: true, Value: int dimension } &&
               dimension == 0;
    }

    private static bool IsIgnorableCurrentComponentNoOpLifecycleHelperInvocation(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IInvocationOperation invocation,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        var helperMethod = invocation.TargetMethod;
        if (helperMethod.MethodKind != MethodKind.Ordinary ||
            helperMethod.DeclaredAccessibility != Accessibility.Private ||
            helperMethod.IsStatic ||
            helperMethod.IsAsync ||
            helperMethod.ReturnsVoid ||
            helperMethod.ReturnsByRef ||
            helperMethod.ReturnsByRefReadonly ||
            IsTaskLikeType(compilation, helperMethod.ReturnType) ||
            helperMethod.TypeParameters.Length != 0 ||
            !AreIgnorableNoOpLifecycleHelperArguments(
                compilation,
                lifecycleMethod,
                helperMethod,
                invocation,
                allowReadOnlyByRefArguments: false,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                out var helperParamsArrayLengthParameters) ||
            helperMethod.ContainingType is null ||
            !SymbolEqualityComparer.Default.Equals(helperMethod.ContainingType, lifecycleMethod.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(invocation.Instance);
        if (instance is not null and not IInstanceReferenceOperation)
            return false;

        return IsIgnorableNoOpLifecycleHelperMethod(
            compilation,
            lifecycleMethod,
            helperMethod,
            visitedSymbols,
            helperParamsArrayLengthParameters);
    }

    private static bool IsIgnorableCurrentComponentNoOpLifecycleVoidHelperInvocation(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        ExpressionStatementSyntax expressionStatement,
        SemanticModel semanticModel)
    {
        if (semanticModel.GetOperation(expressionStatement.Expression) is not IInvocationOperation invocation)
            return false;

        var helperMethod = invocation.TargetMethod;
        if (helperMethod.MethodKind != MethodKind.Ordinary ||
            helperMethod.DeclaredAccessibility != Accessibility.Private ||
            helperMethod.IsStatic ||
            helperMethod.IsAsync ||
            !helperMethod.ReturnsVoid ||
            helperMethod.ReturnsByRef ||
            helperMethod.ReturnsByRefReadonly ||
            helperMethod.TypeParameters.Length != 0 ||
            helperMethod.ContainingType is null ||
            !SymbolEqualityComparer.Default.Equals(helperMethod.ContainingType, lifecycleMethod.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(invocation.Instance);
        if (instance is not null and not IInstanceReferenceOperation)
            return false;

        var visitedSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        return IsIgnorableCurrentComponentNoOpLifecycleVoidHelperInvocation(
            compilation,
            lifecycleMethod,
            invocation,
            visitedSymbols,
            paramsArrayLengthParameters: null,
            sourceStableLocalInitializers: null);
    }

    private static bool IsIgnorableCurrentComponentNoOpLifecycleVoidHelperInvocation(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IInvocationOperation invocation,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        var helperMethod = invocation.TargetMethod;
        if (helperMethod.MethodKind != MethodKind.Ordinary ||
            helperMethod.DeclaredAccessibility != Accessibility.Private ||
            helperMethod.IsStatic ||
            helperMethod.IsAsync ||
            !helperMethod.ReturnsVoid ||
            helperMethod.ReturnsByRef ||
            helperMethod.ReturnsByRefReadonly ||
            helperMethod.TypeParameters.Length != 0 ||
            helperMethod.ContainingType is null ||
            !SymbolEqualityComparer.Default.Equals(helperMethod.ContainingType, lifecycleMethod.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(invocation.Instance);
        if (instance is not null and not IInstanceReferenceOperation)
            return false;

        return AreIgnorableNoOpLifecycleHelperArguments(
                compilation,
                lifecycleMethod,
                helperMethod,
                invocation,
                allowReadOnlyByRefArguments: true,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                out var helperParamsArrayLengthParameters) &&
            IsIgnorableNoOpLifecycleVoidHelperMethod(
                compilation,
                lifecycleMethod,
                helperMethod,
                visitedSymbols,
                helperParamsArrayLengthParameters);
    }

    private static bool AreIgnorableNoOpLifecycleHelperArguments(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IMethodSymbol helperMethod,
        IInvocationOperation invocation,
        bool allowReadOnlyByRefArguments,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        out HashSet<IParameterSymbol>? helperParamsArrayLengthParameters)
    {
        helperParamsArrayLengthParameters = null;
        foreach (var parameter in helperMethod.Parameters)
        {
            if (!CanBindNoOpLifecycleHelperArgument(parameter, allowReadOnlyByRefArguments))
            {
                return false;
            }
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is null ||
                !CanBindNoOpLifecycleHelperArgument(argument.Parameter, allowReadOnlyByRefArguments) ||
                IsForwardedReadOnlyByRefNoOpLifecycleArgument(argument))
            {
                return false;
            }

            if (argument.Parameter.IsParams)
            {
                if (!IsIgnorableNoOpLifecycleParamsArgument(
                        compilation,
                        lifecycleMethod,
                        argument,
                        paramsArrayLengthParameters,
                        sourceStableLocalInitializers,
                        visitedSymbols))
                {
                    return false;
                }

                helperParamsArrayLengthParameters ??= new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
                helperParamsArrayLengthParameters.Add(argument.Parameter);
                continue;
            }

            if (argument.ArgumentKind == ArgumentKind.ParamArray)
                return false;

            if (argument.Parameter.Type is IArrayTypeSymbol &&
                IsIgnorableNoOpLifecycleExplicitArrayArgument(
                    compilation,
                    lifecycleMethod,
                    argument,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols))
            {
                continue;
            }

            if (!IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                lifecycleMethod,
                argument.Value,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CanBindNoOpLifecycleHelperArgument(
        IParameterSymbol parameter,
        bool allowReadOnlyByRefArguments)
        => parameter.RefKind == RefKind.None ||
           (allowReadOnlyByRefArguments && parameter.RefKind == RefKind.In);

    private static bool IsForwardedReadOnlyByRefNoOpLifecycleArgument(IArgumentOperation argument)
        => argument.Parameter?.RefKind == RefKind.In &&
           ContainsReadOnlyByRefParameterReference(argument.Value);

    private static bool ContainsReadOnlyByRefParameterReference(IOperation? operation)
    {
        if (operation is null)
            return false;

        foreach (var current in operation.DescendantsAndSelf())
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is IParameterReferenceOperation parameterReference &&
                parameterReference.Parameter.RefKind == RefKind.In)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsIgnorableNoOpLifecycleExplicitArrayArgument(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IArgumentOperation argument,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
    {
        if (argument.ArgumentKind != ArgumentKind.Explicit)
            return false;

        return IsIgnorableNoOpLifecycleArrayCarrier(
            compilation,
            lifecycleMethod,
            argument.Value,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers,
            visitedSymbols);
    }

    private static bool IsIgnorableNoOpLifecycleParamsArgument(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IArgumentOperation argument,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
    {
        if (argument.Parameter?.Type is not IArrayTypeSymbol)
            return false;

        var value = RazorVueOperationNormalizer.Unwrap(argument.Value);
        return argument.ArgumentKind switch
        {
            ArgumentKind.ParamArray => IsIgnorableNoOpLifecycleParamsArrayCarrier(
                compilation,
                lifecycleMethod,
                value,
                requireImplicitArrayCarrier: true,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                visitedSymbols),
            ArgumentKind.Explicit => IsIgnorableNoOpLifecycleParamsArrayCarrier(
                compilation,
                lifecycleMethod,
                value,
                requireImplicitArrayCarrier: false,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers,
                visitedSymbols),
            _ => false
        };
    }

    private static bool IsIgnorableNoOpLifecycleParamsArrayCarrier(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IOperation? value,
        bool requireImplicitArrayCarrier,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
        => value switch
        {
            IArrayCreationOperation { Initializer: not null } arrayCreation
                when !requireImplicitArrayCarrier || arrayCreation.IsImplicit =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    arrayCreation.Initializer.ElementValues,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            IArrayInitializerOperation arrayInitializer
                when !requireImplicitArrayCarrier || arrayInitializer.IsImplicit =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    arrayInitializer.ElementValues,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            ICollectionExpressionOperation collectionExpression
                when !requireImplicitArrayCarrier =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    collectionExpression.Elements,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            _ => false
        };

    private static bool IsIgnorableNoOpLifecycleArrayCarrier(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IOperation? value,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
        => RazorVueOperationNormalizer.Unwrap(value) switch
        {
            IArrayCreationOperation { Initializer: not null } arrayCreation
                when IsArrayCreationNoOpLifecycleElementOnly(arrayCreation) =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    arrayCreation.Initializer.ElementValues,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            IArrayInitializerOperation arrayInitializer =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    arrayInitializer.ElementValues,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            ICollectionExpressionOperation collectionExpression =>
                IsIgnorableNoOpLifecycleParamsElementList(
                    compilation,
                    lifecycleMethod,
                    collectionExpression.Elements,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers,
                    visitedSymbols),
            _ => false
        };

    private static bool IsIgnorableNoOpLifecycleAnonymousObjectCarrier(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IOperation? value,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
        => RazorVueOperationNormalizer.Unwrap(value) is IAnonymousObjectCreationOperation anonymousObject &&
           anonymousObject.Initializers.All(initializer =>
               IsIgnorableNoOpLifecycleAnonymousObjectInitializer(
                   compilation,
                   lifecycleMethod,
                   initializer,
                   paramsArrayLengthParameters,
                   sourceStableLocalInitializers,
                   visitedSymbols));

    private static bool IsIgnorableNoOpLifecycleAnonymousObjectInitializer(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IOperation initializer,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
    {
        var current = RazorVueOperationNormalizer.Unwrap(initializer);
        if (current is ISimpleAssignmentOperation assignment)
        {
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                lifecycleMethod,
                assignment.Value,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers);
        }

        return IsIgnorableNoOpLifecycleValueExpression(
            compilation,
            lifecycleMethod,
            current,
            visitedSymbols,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers);
    }

    private static bool IsArrayCreationNoOpLifecycleElementOnly(IArrayCreationOperation arrayCreation)
    {
        if (arrayCreation.DimensionSizes.Length != 1)
            return false;

        var dimensionSize = RazorVueOperationNormalizer.Unwrap(arrayCreation.DimensionSizes[0]);
        return dimensionSize is null ||
               dimensionSize.ConstantValue.HasValue ||
               dimensionSize is IInvalidOperation;
    }

    private static bool IsIgnorableNoOpLifecycleParamsElementList(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        ImmutableArray<IOperation> elements,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
        => elements.All(element =>
            IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                lifecycleMethod,
                element,
                visitedSymbols,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers));

    private static bool IsIgnorableNoOpLifecycleHelperMethod(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IMethodSymbol helperMethod,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (helperMethod.DeclaringSyntaxReferences.Length == 0 ||
            !visitedSymbols.Add(helperMethod))
        {
            return false;
        }

        try
        {
            if (!TryGetMethodDeclarationWithBody(helperMethod, out var methodSyntax))
                return false;

            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            if (methodSyntax.ExpressionBody is not null)
            {
                var expressionOperation = semanticModel.GetOperation(methodSyntax.ExpressionBody.Expression);
                return IsIgnorableNoOpLifecycleValueExpression(
                    compilation,
                    lifecycleMethod,
                    expressionOperation,
                    visitedSymbols,
                    paramsArrayLengthParameters);
            }

            if (methodSyntax.Body is null ||
                methodSyntax.Body.Statements.Count == 0)
            {
                return false;
            }

            IReadOnlyDictionary<ILocalSymbol, IOperation>? helperLocalInitializers = null;
            if (semanticModel.GetOperation(methodSyntax.Body) is IBlockOperation blockOperation)
            {
                helperLocalInitializers = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                    compilation,
                    blockOperation.Operations,
                    RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);
            }

            for (var index = 0; index < methodSyntax.Body.Statements.Count - 1; index++)
            {
                if (!IsIgnorableNoOpLifecycleHelperPrefixStatement(
                        compilation,
                        lifecycleMethod,
                        methodSyntax.Body.Statements[index],
                        semanticModel,
                        methodSyntax.Body,
                        paramsArrayLengthParameters,
                        helperLocalInitializers))
                {
                    return false;
                }
            }

            if (methodSyntax.Body.Statements[methodSyntax.Body.Statements.Count - 1] is not ReturnStatementSyntax { Expression: not null } returnStatement)
                return false;

            var returnOperation = semanticModel.GetOperation(returnStatement.Expression);
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                lifecycleMethod,
                returnOperation,
                visitedSymbols,
                paramsArrayLengthParameters,
                helperLocalInitializers);
        }
        finally
        {
            visitedSymbols.Remove(helperMethod);
        }
    }

    private static bool IsIgnorableNoOpLifecycleHelperPrefixStatement(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        StatementSyntax statement,
        SemanticModel semanticModel,
        BlockSyntax containingBody,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax localDeclaration:
                return IsNoOpLifecycleLocalDeclaration(
                    compilation,
                    lifecycleMethod,
                    localDeclaration,
                    semanticModel,
                    containingBody,
                    allowProjectedStructuredLocalInitializer: true,
                    paramsArrayLengthParameters: paramsArrayLengthParameters,
                    sourceStableLocalInitializers: sourceStableLocalInitializers);
            case ExpressionStatementSyntax expressionStatement:
                return IsIgnorableNoOpLifecycleHelperPrefixAssignment(
                    compilation,
                    lifecycleMethod,
                    semanticModel,
                    expressionStatement,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers) ||
                    IsIgnorableNoOpLifecycleHelperPrefixVoidInvocation(
                        compilation,
                        lifecycleMethod,
                        semanticModel,
                        expressionStatement,
                        paramsArrayLengthParameters,
                        sourceStableLocalInitializers,
                        visitedSymbols: null);
            default:
                return false;
        }
    }

    private static bool IsIgnorableNoOpLifecycleHelperPrefixStatement(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        StatementSyntax statement,
        SemanticModel semanticModel,
        BlockSyntax containingBody,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax localDeclaration:
                return IsNoOpLifecycleLocalDeclaration(
                    compilation,
                    lifecycleMethod,
                    localDeclaration,
                    semanticModel,
                    containingBody,
                    allowProjectedStructuredLocalInitializer: true,
                    paramsArrayLengthParameters: paramsArrayLengthParameters,
                    sourceStableLocalInitializers: sourceStableLocalInitializers);
            case ExpressionStatementSyntax expressionStatement:
                return IsIgnorableNoOpLifecycleHelperPrefixAssignment(
                    compilation,
                    lifecycleMethod,
                    semanticModel,
                    expressionStatement,
                    paramsArrayLengthParameters,
                    sourceStableLocalInitializers) ||
                    IsIgnorableNoOpLifecycleDiscardedValueHelperInvocation(
                        compilation,
                        lifecycleMethod,
                        expressionStatement,
                        semanticModel,
                        paramsArrayLengthParameters,
                        sourceStableLocalInitializers,
                        visitedSymbols) ||
                    IsIgnorableNoOpLifecycleHelperPrefixVoidInvocation(
                        compilation,
                        lifecycleMethod,
                        semanticModel,
                        expressionStatement,
                        paramsArrayLengthParameters,
                        sourceStableLocalInitializers,
                        visitedSymbols);
            default:
                return false;
        }
    }

    private static bool IsIgnorableNoOpLifecycleHelperPrefixVoidInvocation(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        SemanticModel semanticModel,
        ExpressionStatementSyntax expressionStatement,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol>? visitedSymbols)
        => visitedSymbols is not null &&
           semanticModel.GetOperation(expressionStatement.Expression) is IInvocationOperation invocation &&
           IsIgnorableCurrentComponentNoOpLifecycleVoidHelperInvocation(
               compilation,
               lifecycleMethod,
               invocation,
               visitedSymbols,
               paramsArrayLengthParameters,
               sourceStableLocalInitializers);

    private static bool IsIgnorableNoOpLifecycleDiscardedValueHelperInvocation(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        ExpressionStatementSyntax expressionStatement,
        SemanticModel semanticModel,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers,
        HashSet<ISymbol> visitedSymbols)
    {
        var operation = semanticModel.GetOperation(expressionStatement.Expression);
        if (RazorVueOperationNormalizer.Unwrap(operation) is not IInvocationOperation invocation ||
            invocation.TargetMethod.ReturnsVoid)
        {
            return false;
        }

        return IsIgnorableNoOpLifecycleValueExpression(
            compilation,
            lifecycleMethod,
            operation,
            visitedSymbols,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers);
    }

    private static bool IsIgnorableNoOpLifecycleHelperPrefixAssignment(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        SemanticModel semanticModel,
        ExpressionStatementSyntax expressionStatement,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers)
    {
        if (sourceStableLocalInitializers is null ||
            sourceStableLocalInitializers.Count == 0 ||
            semanticModel.GetOperation(expressionStatement.Expression) is not ISimpleAssignmentOperation assignment ||
            assignment.Target is not ILocalReferenceOperation localReference ||
            !sourceStableLocalInitializers.TryGetValue(localReference.Local, out var initializer))
        {
            return false;
        }

        var assignedValue = RazorVueOperationNormalizer.Unwrap(assignment.Value) ?? assignment.Value;
        return OperationSyntaxEquals(initializer, assignedValue) &&
               IsIgnorableNoOpLifecycleValueExpression(
                   compilation,
                   lifecycleMethod,
                   assignedValue,
                   new HashSet<ISymbol>(SymbolEqualityComparer.Default),
                   paramsArrayLengthParameters,
                   sourceStableLocalInitializers);
    }

    private static bool OperationSyntaxEquals(IOperation left, IOperation right)
        => left.Syntax.SyntaxTree == right.Syntax.SyntaxTree &&
           left.Syntax.Span.Equals(right.Syntax.Span);

    private static bool IsIgnorableNoOpLifecycleVoidHelperMethod(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        IMethodSymbol helperMethod,
        HashSet<ISymbol> visitedSymbols,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters)
    {
        if (helperMethod.DeclaringSyntaxReferences.Length == 0 ||
            !visitedSymbols.Add(helperMethod))
        {
            return false;
        }

        try
        {
            if (!TryGetMethodDeclarationWithBody(helperMethod, out var methodSyntax))
            {
                return false;
            }

            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            if (methodSyntax.ExpressionBody is not null)
            {
                return IsIgnorableNoOpLifecycleExpressionBodiedVoidHelperMethod(
                    compilation,
                    lifecycleMethod,
                    methodSyntax.ExpressionBody.Expression,
                    semanticModel,
                    paramsArrayLengthParameters,
                    visitedSymbols);
            }

            if (methodSyntax.Body is null)
            {
                return false;
            }

            IReadOnlyDictionary<ILocalSymbol, IOperation>? helperLocalInitializers = null;
            if (semanticModel.GetOperation(methodSyntax.Body) is IBlockOperation blockOperation)
            {
                helperLocalInitializers = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                    compilation,
                    blockOperation.Operations,
                    RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);
            }

            for (var index = 0; index < methodSyntax.Body.Statements.Count; index++)
            {
                var statement = methodSyntax.Body.Statements[index];
                if (statement is ReturnStatementSyntax { Expression: null })
                    continue;

                if (statement is IfStatementSyntax ifStatement &&
                    index == methodSyntax.Body.Statements.Count - 1 &&
                    IsNoOpLifecycleTerminalIfReturnStatement(
                        compilation,
                        lifecycleMethod,
                        ifStatement,
                        semanticModel,
                        helperLocalInitializers,
                        paramsArrayLengthParameters))
                {
                    return true;
                }

                if (!IsIgnorableNoOpLifecycleHelperPrefixStatement(
                        compilation,
                        lifecycleMethod,
                        statement,
                        semanticModel,
                        methodSyntax.Body,
                        paramsArrayLengthParameters,
                        helperLocalInitializers,
                        visitedSymbols))
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            visitedSymbols.Remove(helperMethod);
        }
    }

    private static bool IsIgnorableNoOpLifecycleExpressionBodiedVoidHelperMethod(
        Compilation compilation,
        IMethodSymbol lifecycleMethod,
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters,
        HashSet<ISymbol> visitedSymbols)
    {
        var operation = semanticModel.GetOperation(expression);
        var normalized = RazorVueOperationNormalizer.Unwrap(operation);
        if (normalized is ISimpleAssignmentOperation { Target: IDiscardOperation } assignment)
        {
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                lifecycleMethod,
                assignment.Value,
                visitedSymbols,
                paramsArrayLengthParameters);
        }

        if (normalized is not IInvocationOperation invocation ||
            invocation.TargetMethod.ReturnsVoid)
        {
            return false;
        }

        return IsIgnorableNoOpLifecycleValueExpression(
            compilation,
            lifecycleMethod,
            normalized,
            visitedSymbols,
            paramsArrayLengthParameters);
    }

    private static bool TryGetMethodDeclarationWithBody(
        IMethodSymbol method,
        out MethodDeclarationSyntax methodSyntax)
    {
        if (TryGetMethodDeclarationWithBody(method.DeclaringSyntaxReferences, out methodSyntax))
            return true;

        if (method.PartialImplementationPart is not null &&
            TryGetMethodDeclarationWithBody(method.PartialImplementationPart.DeclaringSyntaxReferences, out methodSyntax))
        {
            return true;
        }

        if (method.PartialDefinitionPart is not null &&
            TryGetMethodDeclarationWithBody(method.PartialDefinitionPart.DeclaringSyntaxReferences, out methodSyntax))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetMethodDeclarationWithBody(
        ImmutableArray<SyntaxReference> syntaxReferences,
        out MethodDeclarationSyntax methodSyntax)
    {
        foreach (var syntaxReference in syntaxReferences)
        {
            if (syntaxReference.GetSyntax() is MethodDeclarationSyntax candidate &&
                (candidate.ExpressionBody is not null || candidate.Body is not null))
            {
                methodSyntax = candidate;
                return true;
            }
        }

        methodSyntax = null!;
        return false;
    }

    private static bool IsCurrentComponentSourceStableValueMember(
        Compilation compilation,
        IMethodSymbol method,
        IPropertyReferenceOperation propertyReference,
        HashSet<ISymbol> visitedValueMembers,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null)
    {
        var property = propertyReference.Property;
        if (property.IsStatic ||
            property.IsIndexer ||
            propertyReference.Arguments.Length != 0 ||
            property.ContainingType is null ||
            !ContainsTypeOrBase(method.ContainingType, property.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(propertyReference.Instance);
        if (instance is not null and not IInstanceReferenceOperation)
            return false;

        return IsIgnorableSourceStableValueMemberInitializer(
            compilation,
            method,
            property,
            visitedValueMembers,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers);
    }

    private static bool IsCurrentComponentSourceStableValueMember(
        Compilation compilation,
        IMethodSymbol method,
        IFieldReferenceOperation fieldReference,
        HashSet<ISymbol> visitedValueMembers,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null)
    {
        var field = fieldReference.Field;
        if (field.IsStatic ||
            field.ContainingType is null ||
            !ContainsTypeOrBase(method.ContainingType, field.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(fieldReference.Instance);
        if (instance is not null and not IInstanceReferenceOperation)
            return false;

        return IsIgnorableSourceStableValueMemberInitializer(
            compilation,
            method,
            field,
            visitedValueMembers,
            paramsArrayLengthParameters,
            sourceStableLocalInitializers);
    }

    private static bool IsIgnorableSourceStableValueMemberInitializer(
        Compilation compilation,
        IMethodSymbol method,
        ISymbol member,
        HashSet<ISymbol> visitedValueMembers,
        HashSet<IParameterSymbol>? paramsArrayLengthParameters = null,
        IReadOnlyDictionary<ILocalSymbol, IOperation>? sourceStableLocalInitializers = null)
    {
        if (!RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(compilation, member, out var initializer))
            return false;

        if (initializer is null || !visitedValueMembers.Add(member))
            return false;

        try
        {
            return IsIgnorableNoOpLifecycleValueExpression(
                compilation,
                method,
                initializer,
                visitedValueMembers,
                paramsArrayLengthParameters,
                sourceStableLocalInitializers);
        }
        finally
        {
            visitedValueMembers.Remove(member);
        }
    }

    private static bool IsCurrentComponentParameterProperty(
        IMethodSymbol method,
        IPropertyReferenceOperation propertyReference)
    {
        var property = propertyReference.Property;
        if (property.IsStatic ||
            property.IsIndexer ||
            propertyReference.Arguments.Length != 0 ||
            !IsParameterProperty(property) ||
            property.ContainingType is null ||
            !ContainsTypeOrBase(method.ContainingType, property.ContainingType))
        {
            return false;
        }

        var instance = RazorVueOperationNormalizer.Unwrap(propertyReference.Instance);
        return instance is null or IInstanceReferenceOperation;
    }

    private static bool ContainsTypeOrBase(INamedTypeSymbol type, INamedTypeSymbol candidate)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, candidate.OriginalDefinition))
                return true;
        }

        return false;
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
        var lifecycleStatements = ImmutableArray<SupportedLifecycleStatement>.Empty;
        if (IsBaseSetParametersAsyncStatement(method, statements[0]))
        {
            sawBaseCall = true;
            var baseAnalysis = AnalyzeBaseSetParametersAsync(snapshot, expressionEmitter, method, visitedMethods);
            if (!baseAnalysis.IsSupported)
                return SetParametersAsyncAnalysis.Unsupported;

            lifecycleStatements = baseAnalysis.Statements;
            index++;
        }

        if (index >= statements.Count)
            return new SetParametersAsyncAnalysis(true, lifecycleStatements);

        if (!sawBaseCall)
            return SetParametersAsyncAnalysis.Unsupported;

        return TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                statements,
                index,
                endExclusive: null,
                state: null,
                out var localStatements)
            ? new SetParametersAsyncAnalysis(true, ConcatLifecycleStatements(lifecycleStatements, localStatements))
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

    private static bool TryGetSetParametersAsyncNoOpOrEmit(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        StatementSyntax statement,
        SetParametersAsyncStatementSequenceState state,
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
                    return TryExtractSetParametersAsyncSupportedEmitCall(
                        snapshot,
                        expressionEmitter,
                        method,
                        returnStatement.Expression,
                        state,
                        out emitCall);
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
                    return TryExtractSetParametersAsyncSupportedEmitCall(
                        snapshot,
                        expressionEmitter,
                        method,
                        expressionStatement.Expression,
                        state,
                        out emitCall);
                }
                catch (RazorVueCompilationIssueException)
                {
                    return false;
                }
            default:
                return false;
        }
    }

    private static bool TryExtractSetParametersAsyncSupportedEmitCall(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        ExpressionSyntax expression,
        SetParametersAsyncStatementSequenceState state,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        try
        {
            emitCall = ExtractSupportedEmitCall(snapshot, expressionEmitter, method, expression, allowFirstRenderPayload: false);
            return emitCall is not null;
        }
        catch (RazorVueCompilationIssueException)
        {
            return TryExtractSetParametersAsyncCatchLocalPayloadEmitCall(
                snapshot,
                method,
                expression,
                state,
                out emitCall);
        }
    }

    private static bool TryExtractSetParametersAsyncCatchLocalPayloadEmitCall(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        ExpressionSyntax expression,
        SetParametersAsyncStatementSequenceState state,
        out SupportedEmitCall? emitCall)
    {
        emitCall = null;
        if (state.FoldableCatchLocals.Count == 0)
            return false;

        expression = UnwrapLifecycleExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapLifecycleExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(snapshot.Compilation, expression, out var wrappedExpression))
            expression = wrappedExpression;

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, "InvokeAsync", StringComparison.Ordinal) ||
            TryGetLifecycleCallbackName(memberAccess.Expression) is not string callbackName ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var payloadSyntax = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[0].Expression);
        var semanticModel = snapshot.Compilation.GetSemanticModel(payloadSyntax.SyntaxTree);
        var payloadOperation = semanticModel.GetOperation(payloadSyntax);
        if (payloadOperation is null ||
            !TryFoldSetParametersAsyncCatchBodyNullCondition(
                payloadOperation,
                state,
                out var payloadExpression))
        {
            return false;
        }

        emitCall = new SupportedEmitCall(
            ToLifecycleEmitName(method, callbackName),
            payloadExpression,
            false,
            ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty);
        return true;
    }

    private static bool TryGetSetParametersAsyncStatementSequence(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IReadOnlyList<StatementSyntax> statements,
        int startIndex,
        int? endExclusive,
        SetParametersAsyncStatementSequenceState? state,
        out ImmutableArray<SupportedLifecycleStatement> lifecycleStatements)
    {
        lifecycleStatements = ImmutableArray<SupportedLifecycleStatement>.Empty;
        var exclusiveEnd = endExclusive ?? statements.Count;
        if (startIndex < 0 || startIndex > statements.Count || exclusiveEnd < startIndex || exclusiveEnd > statements.Count)
            return false;
        if (statements.Count == 0)
            return true;

        if (state is null)
        {
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
            state = new SetParametersAsyncStatementSequenceState(semanticModel, localInitializers);
        }

        var builder = ImmutableArray.CreateBuilder<SupportedLifecycleStatement>();
        var sawEmit = false;
        var canIgnoreControlOnlyStatements = true;

        for (var index = startIndex; index < exclusiveEnd; index++)
        {
            var statement = statements[index];
            if (TryValidateLifecyclePrefixDeclarations(statement, state.SemanticModel, state.LocalInitializers))
            {
                var prefixLocals = GetLifecyclePrefixDeclaredLocals(statement, state.SemanticModel);
                foreach (var local in prefixLocals)
                {
                    if (!state.LocalInitializers.ContainsKey(local))
                        return false;

                    state.EmittedLocals.Add(local);
                    state.LocalAliases[local] = "__jazorLifecycleLocal" + Jazor.Common.Format.HashName(local.ToDisplayString()).TrimStart('_');
                }

                continue;
            }

            if (statement is IfStatementSyntax ifStatement)
            {
                if (TryGetTerminalSetParametersAsyncNoOpReturnStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        ifStatement,
                        state,
                        index,
                        exclusiveEnd,
                        sawEmit))
                {
                    break;
                }

                if (TryGetSetParametersAsyncTerminalIfReturnStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        ifStatement,
                        state,
                        index,
                        exclusiveEnd,
                        sawEmit,
                        out var terminalIfReturnStatement))
                {
                    builder.Add(terminalIfReturnStatement);
                    break;
                }

                if (TryGetSetParametersAsyncIfReturnStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        ifStatement,
                        state,
                        out var ifReturnStatement))
                {
                    sawEmit = true;
                    builder.Add(ifReturnStatement);
                    continue;
                }

                if (TryGetSetParametersAsyncGuardReturnStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        ifStatement,
                        state,
                        out var guardReturnStatement))
                {
                    if (index >= exclusiveEnd - 1)
                        return false;

                    canIgnoreControlOnlyStatements &=
                        CanIgnoreSetParametersAsyncNoOpCondition(
                            snapshot,
                            expressionEmitter,
                            method,
                            ifStatement.Condition,
                            state);
                    builder.Add(guardReturnStatement);
                    continue;
                }

                if (!TryGetSetParametersAsyncIfStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        ifStatement,
                        state,
                        out var lifecycleIfStatement))
                {
                    return false;
                }

                if (lifecycleIfStatement is not null)
                {
                    sawEmit = true;
                    builder.Add(lifecycleIfStatement);
                }

                continue;
            }

            if (statement is SwitchStatementSyntax switchStatement)
            {
                if (!TryGetSetParametersAsyncSwitchStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        switchStatement,
                        state,
                        out var lifecycleSwitchStatement))
                {
                    return false;
                }

                if (lifecycleSwitchStatement is not null)
                {
                    sawEmit = true;
                    builder.Add(lifecycleSwitchStatement);
                }

                continue;
            }

            if (IsSetParametersAsyncLoopStatement(statement))
            {
                if (!TryGetSetParametersAsyncLoopStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        statement,
                        state,
                        out var lifecycleLoopStatement))
                {
                    return false;
                }

                if (lifecycleLoopStatement is not null)
                {
                    sawEmit = true;
                    builder.Add(lifecycleLoopStatement);
                }

                continue;
            }

            if (statement is TryStatementSyntax tryStatement)
            {
                if (TryGetTerminalSetParametersAsyncNoOpTryCatchStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        tryStatement,
                        state,
                        sawEmit))
                {
                    break;
                }

                if (TryGetSetParametersAsyncTryCatchStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        tryStatement,
                        state,
                        out var lifecycleTryCatchStatement))
                {
                    sawEmit = true;
                    builder.Add(lifecycleTryCatchStatement);
                    continue;
                }

                if (!TryGetSetParametersAsyncTryFinallyStatement(
                        snapshot,
                        expressionEmitter,
                        method,
                        tryStatement,
                        state,
                        out var lifecycleTryFinallyStatement))
                {
                    return false;
                }

                if (lifecycleTryFinallyStatement is not null)
                {
                    sawEmit = true;
                    builder.Add(lifecycleTryFinallyStatement);
                }

                continue;
            }

            if (statement is ReturnStatementSyntax { Expression: null } && state.AllowDirectNoOpReturnStatement)
            {
                var returnPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
                if (!TryAppendPendingLifecycleLocalPreludes(
                        snapshot,
                        expressionEmitter,
                        method,
                        state,
                        returnPreludeBuilder))
                {
                    return false;
                }

                builder.Add(new SupportedLifecycleReturnStatement(
                    returnPreludeBuilder.Count == 0
                        ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
                        : returnPreludeBuilder.ToImmutable()));
                sawEmit = true;
                break;
            }

            if (!TryGetSetParametersAsyncNoOpOrEmit(
                snapshot,
                expressionEmitter,
                method,
                statement,
                state,
                out var emitCall))
            {
                return false;
            }

            if (emitCall is null)
            {
                if (statement is ReturnStatementSyntax)
                    break;

                continue;
            }

            if (statement is ReturnStatementSyntax && index != exclusiveEnd - 1)
                return false;

            sawEmit = true;
            var emitPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
            if (!TryAppendPendingLifecycleLocalPreludes(
                    snapshot,
                    expressionEmitter,
                    method,
                    state,
                    emitPreludeBuilder))
            {
                return false;
            }

            emitPreludeBuilder.AddRange(FilterLifecyclePreludeBindings(emitCall.PreludeBindings, state));
            builder.Add(new SupportedLifecycleEmitStatement(
                emitPreludeBuilder.Count == 0
                    ? emitCall with { PreludeBindings = ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty }
                    : emitCall with { PreludeBindings = emitPreludeBuilder.ToImmutable() }));
        }

        if (!sawEmit)
        {
            if (builder.Count != 0 && !state.AllowTerminalNoOpControlFlow)
                return false;
            if (builder.Count != 0 && !canIgnoreControlOnlyStatements)
                return false;

            lifecycleStatements = ImmutableArray<SupportedLifecycleStatement>.Empty;
            return true;
        }

        lifecycleStatements = builder.ToImmutable();
        return true;
    }

    private static bool TryGetTerminalSetParametersAsyncNoOpTryCatchStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        TryStatementSyntax tryStatement,
        SetParametersAsyncStatementSequenceState state,
        bool sawEmit)
    {
        if (!state.AllowTerminalNoOpControlFlow ||
            sawEmit ||
            tryStatement.Catches.Count != 1 ||
            tryStatement.Finally is not null ||
            ContainsThrowStatement(tryStatement) ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Block, allowDirectNoOpReturnStatement: true) ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Catches[0].Block, allowDirectNoOpReturnStatement: false))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncCatchClause(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Catches[0],
                state.CloneForBranch(),
                allowCatchBodyNullConditionFolding: false,
                out _,
                out _,
                out _))
        {
            return false;
        }

        var noOpState = state.CloneForDirectNoOpReturnBody();
        return TryGetSetParametersAsyncNoOpReturningStatementSequence(
                   snapshot,
                   method,
                   tryStatement.Block.Statements,
                   noOpState) &&
               CanIgnoreSetParametersAsyncPendingLocals(snapshot, method, noOpState);
    }

    private static bool TryGetSetParametersAsyncNoOpReturningStatementSequence(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        IReadOnlyList<StatementSyntax> statements,
        SetParametersAsyncStatementSequenceState state)
    {
        foreach (var statement in statements)
        {
            if (TryValidateLifecyclePrefixDeclarations(statement, state.SemanticModel, state.LocalInitializers))
            {
                var prefixLocals = GetLifecyclePrefixDeclaredLocals(statement, state.SemanticModel);
                foreach (var local in prefixLocals)
                {
                    if (!state.LocalInitializers.ContainsKey(local))
                        return false;

                    state.EmittedLocals.Add(local);
                    state.LocalAliases[local] = "__jazorLifecycleLocal" + Jazor.Common.Format.HashName(local.ToDisplayString()).TrimStart('_');
                }

                continue;
            }

            switch (statement)
            {
                case EmptyStatementSyntax:
                    continue;

                case ExpressionStatementSyntax expressionStatement
                    when IsNoOpLifecycleExpression(snapshot.Compilation, method, expressionStatement.Expression):
                    continue;

                case ReturnStatementSyntax { Expression: null }:
                    return true;

                default:
                    return false;
            }
        }

        return false;
    }

    private static bool TryGetTerminalSetParametersAsyncNoOpReturnStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SetParametersAsyncStatementSequenceState state,
        int index,
        int exclusiveEnd,
        bool sawEmit)
    {
        if (!state.AllowTerminalNoOpControlFlow ||
            sawEmit ||
            !TerminatesWithNoOpReturn(ifStatement, allowImplicitContinue: index >= exclusiveEnd - 1))
        {
            return false;
        }

        return CanIgnoreSetParametersAsyncNoOpCondition(
            snapshot,
            expressionEmitter,
            method,
            ifStatement.Condition,
            state);
    }

    private static bool TryGetSetParametersAsyncTerminalIfReturnStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SetParametersAsyncStatementSequenceState state,
        int index,
        int exclusiveEnd,
        bool sawEmit,
        out SupportedLifecycleTerminalIfReturnStatement lifecycleStatement)
    {
        lifecycleStatement = default!;
        if (!sawEmit ||
            index != exclusiveEnd - 1 ||
            ifStatement.Else is null ||
            !IsGuardReturnStatement(ifStatement.Statement) ||
            !IsGuardReturnStatement(ifStatement.Else.Statement))
        {
            return false;
        }

        if (!TryGetLifecycleConditionEmission(
                snapshot,
                expressionEmitter,
                method,
                ifStatement.Condition,
                state,
                out var conditionExpression,
                out var conditionPreludeBindings))
        {
            return false;
        }

        lifecycleStatement = new SupportedLifecycleTerminalIfReturnStatement(
            conditionExpression,
            conditionPreludeBindings);
        return true;
    }

    private static bool TryGetLifecycleConditionEmission(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        ExpressionSyntax conditionSyntax,
        SetParametersAsyncStatementSequenceState state,
        out string conditionExpression,
        out ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> conditionPreludeBindings)
    {
        conditionExpression = string.Empty;
        conditionPreludeBindings = ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty;

        var semanticModel = snapshot.Compilation.GetSemanticModel(conditionSyntax.SyntaxTree);
        var conditionOperation = semanticModel.GetOperation(conditionSyntax);
        if (conditionOperation is null)
            return false;

        if (TryFoldSetParametersAsyncCatchBodyNullCondition(
                conditionOperation,
                state,
                out conditionExpression))
        {
            var foldedPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
            if (!TryAppendPendingLifecycleLocalPreludes(
                    snapshot,
                    expressionEmitter,
                    method,
                    state,
                    foldedPreludeBuilder))
            {
                return false;
            }

            conditionPreludeBindings = foldedPreludeBuilder.Count == 0
                ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
                : foldedPreludeBuilder.ToImmutable();
            return true;
        }

        if (ContainsShouldRenderUnsupportedExpressionConstruct(conditionOperation))
        {
            return false;
        }

        RazorVueExpressionEmitter.LifecyclePayloadEmission condition;
        try
        {
            condition = expressionEmitter is null
                ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, conditionOperation, allowFirstRenderPayload: false)
                : expressionEmitter.EmitLifecyclePayload(method, conditionOperation, allowFirstRenderPayload: false);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        if (condition.UsesFirstRender ||
            string.IsNullOrWhiteSpace(condition.Expression))
        {
            return false;
        }

        var conditionPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        if (!TryAppendPendingLifecycleLocalPreludes(
                snapshot,
                expressionEmitter,
                method,
                state,
                conditionPreludeBuilder))
        {
            return false;
        }

        conditionPreludeBuilder.AddRange(FilterLifecyclePreludeBindings(condition.PreludeBindings, state));
        conditionExpression = condition.Expression;
        conditionPreludeBindings = conditionPreludeBuilder.Count == 0
            ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
            : conditionPreludeBuilder.ToImmutable();
        return true;
    }

    private static bool TryGetSetParametersAsyncIfReturnStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleIfReturnStatement lifecycleStatement)
    {
        lifecycleStatement = default!;
        if (ifStatement.Else?.Statement is not { } falseStatement)
            return false;

        var trueStatement = ifStatement.Statement;
        var trueReturns = IsGuardReturnStatement(trueStatement);
        var falseReturns = IsGuardReturnStatement(falseStatement);
        if (trueReturns == falseReturns)
            return false;

        var continueStatement = trueReturns ? falseStatement : trueStatement;
        if (ContainsReturnStatement(continueStatement))
            return false;

        if (!TryGetLifecycleConditionEmission(
                snapshot,
                expressionEmitter,
                method,
                ifStatement.Condition,
                state,
                out var conditionExpression,
                out var conditionPreludeBindings))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                ToLifecycleBranchStatements(continueStatement),
                0,
                endExclusive: null,
                state.CloneForBranch(),
                out var continueStatements) ||
            continueStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        lifecycleStatement = new SupportedLifecycleIfReturnStatement(
            conditionExpression,
            conditionPreludeBindings,
            ReturnsWhenTrue: trueReturns,
            WhenTrue: trueReturns ? ImmutableArray<SupportedLifecycleStatement>.Empty : continueStatements,
            WhenFalse: trueReturns ? continueStatements : ImmutableArray<SupportedLifecycleStatement>.Empty);
        return true;
    }

    private static bool TryGetSetParametersAsyncGuardReturnStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleGuardReturnStatement lifecycleStatement)
    {
        lifecycleStatement = default!;
        if (ifStatement.Else is not null ||
            !IsGuardReturnStatement(ifStatement.Statement))
        {
            return false;
        }

        if (!TryGetLifecycleConditionEmission(
                snapshot,
                expressionEmitter,
                method,
                ifStatement.Condition,
                state,
                out var conditionExpression,
                out var conditionPreludeBindings))
        {
            return false;
        }

        lifecycleStatement = new SupportedLifecycleGuardReturnStatement(
            conditionExpression,
            conditionPreludeBindings);
        return true;
    }

    private static bool TryGetSetParametersAsyncIfStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        IfStatementSyntax ifStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleIfStatement? lifecycleStatement)
    {
        lifecycleStatement = null;
        if (ContainsReturnStatement(ifStatement.Statement) ||
            (ifStatement.Else?.Statement is { } elseStatement && ContainsReturnStatement(elseStatement)))
        {
            return false;
        }

        if (!TryGetLifecycleConditionEmission(
                snapshot,
                expressionEmitter,
                method,
                ifStatement.Condition,
                state,
                out var conditionExpression,
                out var conditionPreludeBindings))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                ToLifecycleBranchStatements(ifStatement.Statement),
                0,
                endExclusive: null,
                state.CloneForBranch(),
                out var whenTrue))
        {
            return false;
        }

        var whenFalse = ImmutableArray<SupportedLifecycleStatement>.Empty;
        if (ifStatement.Else?.Statement is { } falseStatement &&
            !TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                ToLifecycleBranchStatements(falseStatement),
                0,
                endExclusive: null,
                state.CloneForBranch(),
                out whenFalse))
        {
            return false;
        }

        if (whenTrue.IsDefaultOrEmpty && whenFalse.IsDefaultOrEmpty)
            return false;

        lifecycleStatement = new SupportedLifecycleIfStatement(
            conditionExpression,
            conditionPreludeBindings,
            whenTrue,
            whenFalse);
        return true;
    }

    private static bool TryGetSetParametersAsyncSwitchStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SwitchStatementSyntax switchStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleStatement? lifecycleStatement)
    {
        lifecycleStatement = null;
        if (switchStatement.Sections.Any(static section => section.Labels.Any(static label => label is CasePatternSwitchLabelSyntax)))
        {
            return TryGetSetParametersAsyncPatternSwitchStatement(
                snapshot,
                expressionEmitter,
                method,
                switchStatement,
                state,
                out lifecycleStatement);
        }

        if (switchStatement.Sections.Count == 0 ||
            ContainsReturnStatement(switchStatement))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncSwitchValueEmission(
                snapshot,
                expressionEmitter,
                method,
                switchStatement,
                state,
                out var value,
                out var valuePreludeBindings))
        {
            return false;
        }

        var sectionBuilder = ImmutableArray.CreateBuilder<SupportedLifecycleSwitchSection>();
        foreach (var section in switchStatement.Sections)
        {
            if (!TryGetSetParametersAsyncSwitchSection(
                    snapshot,
                    expressionEmitter,
                    method,
                    section,
                    state,
                    out var lifecycleSection))
            {
                return false;
            }

            if (lifecycleSection is not null)
                sectionBuilder.Add(lifecycleSection);
        }

        if (sectionBuilder.Count == 0)
            return false;

        lifecycleStatement = new SupportedLifecycleSwitchStatement(
            value.Expression,
            valuePreludeBindings,
            sectionBuilder.ToImmutable());
        return true;
    }

    private static bool TryGetSetParametersAsyncSwitchSection(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SwitchSectionSyntax section,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleSwitchSection? lifecycleSection)
    {
        lifecycleSection = null;
        if (section.Labels.Count == 0 ||
            section.Statements.Count == 0)
        {
            return false;
        }

        var labelsBuilder = ImmutableArray.CreateBuilder<SupportedLifecycleSwitchLabel>();
        foreach (var label in section.Labels)
        {
            switch (label)
            {
                case DefaultSwitchLabelSyntax:
                    labelsBuilder.Add(new SupportedLifecycleSwitchLabel(IsDefault: true, Expression: string.Empty));
                    break;

                case CaseSwitchLabelSyntax caseLabel:
                    if (!TryGetLifecycleSwitchLabelExpression(
                            snapshot,
                            expressionEmitter,
                            method,
                            state,
                            caseLabel.Value,
                            out var labelExpression))
                    {
                        return false;
                    }

                    labelsBuilder.Add(new SupportedLifecycleSwitchLabel(IsDefault: false, labelExpression));
                    break;

                default:
                    return false;
            }
        }

        var statements = section.Statements;
        if (statements[statements.Count - 1] is not BreakStatementSyntax)
            return false;

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                statements,
                0,
                statements.Count - 1,
                state.CloneForBranch(),
                out var bodyStatements) ||
            bodyStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        lifecycleSection = new SupportedLifecycleSwitchSection(labelsBuilder.ToImmutable(), bodyStatements);
        return true;
    }

    private static bool TryGetSetParametersAsyncPatternSwitchStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SwitchStatementSyntax switchStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleStatement? lifecycleStatement)
    {
        lifecycleStatement = null;
        if (switchStatement.Sections.Count == 0 ||
            ContainsReturnStatement(switchStatement))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncSwitchValueEmission(
                snapshot,
                expressionEmitter,
                method,
                switchStatement,
                state,
                out var value,
                out var valuePreludeBindings))
        {
            return false;
        }

        var valueAlias = CreateLifecycleSwitchValueAlias(switchStatement);
        var valuePreludeBuilder = valuePreludeBindings.ToBuilder();
        valuePreludeBuilder.Add(RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding.Const(valueAlias, value.Expression));

        var sectionBuilder = ImmutableArray.CreateBuilder<SupportedLifecyclePatternSwitchSection>();
        var sawDefault = false;
        for (var index = 0; index < switchStatement.Sections.Count; index++)
        {
            if (!TryGetSetParametersAsyncPatternSwitchSection(
                    snapshot,
                    expressionEmitter,
                    method,
                    switchStatement.Sections[index],
                    state,
                    valueAlias,
                    isLastSection: index == switchStatement.Sections.Count - 1,
                    out var lifecycleSection))
            {
                return false;
            }

            if (lifecycleSection is null)
                continue;

            if (sawDefault)
                return false;
            if (lifecycleSection.IsDefault)
                sawDefault = true;

            sectionBuilder.Add(lifecycleSection);
        }

        if (sectionBuilder.Count == 0)
            return false;

        lifecycleStatement = new SupportedLifecyclePatternSwitchStatement(
            valuePreludeBuilder.ToImmutable(),
            sectionBuilder.ToImmutable());
        return true;
    }

    private static bool TryGetSetParametersAsyncSwitchValueEmission(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SwitchStatementSyntax switchStatement,
        SetParametersAsyncStatementSequenceState state,
        out RazorVueExpressionEmitter.LifecyclePayloadEmission value,
        out ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> valuePreludeBindings)
    {
        value = default;
        valuePreludeBindings = ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty;
        var semanticModel = snapshot.Compilation.GetSemanticModel(switchStatement.Expression.SyntaxTree);
        var valueOperation = semanticModel.GetOperation(switchStatement.Expression);
        if (valueOperation is null ||
            ContainsShouldRenderUnsupportedExpressionConstruct(valueOperation))
        {
            return false;
        }

        try
        {
            value = expressionEmitter is null
                ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, valueOperation, allowFirstRenderPayload: false)
                : expressionEmitter.EmitLifecyclePayload(method, valueOperation, allowFirstRenderPayload: false);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        if (value.UsesFirstRender ||
            string.IsNullOrWhiteSpace(value.Expression))
        {
            return false;
        }

        var valuePreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        if (!TryAppendPendingLifecycleLocalPreludes(
                snapshot,
                expressionEmitter,
                method,
                state,
                valuePreludeBuilder))
        {
            return false;
        }

        valuePreludeBuilder.AddRange(FilterLifecyclePreludeBindings(value.PreludeBindings, state));
        valuePreludeBindings = valuePreludeBuilder.Count == 0
            ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
            : valuePreludeBuilder.ToImmutable();
        return true;
    }

    private static bool TryGetSetParametersAsyncPatternSwitchSection(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SwitchSectionSyntax section,
        SetParametersAsyncStatementSequenceState state,
        string valueAlias,
        bool isLastSection,
        out SupportedLifecyclePatternSwitchSection? lifecycleSection)
    {
        lifecycleSection = null;
        if (section.Labels.Count == 0 ||
            section.Statements.Count == 0)
        {
            return false;
        }

        var isDefault = false;
        var conditionsBuilder = ImmutableArray.CreateBuilder<string>();
        foreach (var label in section.Labels)
        {
            switch (label)
            {
                case DefaultSwitchLabelSyntax:
                    isDefault = true;
                    break;

                case CaseSwitchLabelSyntax caseLabel:
                    if (!TryGetLifecycleSwitchLabelExpression(
                            snapshot,
                            expressionEmitter,
                            method,
                            state,
                            caseLabel.Value,
                            out var labelExpression))
                    {
                        return false;
                    }

                    conditionsBuilder.Add(valueAlias + " === " + labelExpression);
                    break;

                case CasePatternSwitchLabelSyntax patternLabel:
                    if (!TryGetLifecyclePatternSwitchLabelCondition(
                            snapshot,
                            expressionEmitter,
                            patternLabel,
                            valueAlias,
                            out var conditionExpression))
                    {
                        return false;
                    }

                    conditionsBuilder.Add(conditionExpression);
                    break;

                default:
                    return false;
            }
        }

        if (isDefault && conditionsBuilder.Count != 0)
            return false;
        if (isDefault && !isLastSection)
            return false;
        if (!isDefault && conditionsBuilder.Count == 0)
            return false;

        var statements = section.Statements;
        if (statements[statements.Count - 1] is not BreakStatementSyntax)
            return false;

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                statements,
                0,
                statements.Count - 1,
                state.CloneForBranch(),
                out var bodyStatements) ||
            bodyStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        lifecycleSection = new SupportedLifecyclePatternSwitchSection(
            isDefault
                ? string.Empty
                : string.Join(" || ", conditionsBuilder.Select(static condition => "(" + condition + ")")),
            isDefault,
            bodyStatements);
        return true;
    }

    private static bool TryGetLifecyclePatternSwitchLabelCondition(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        CasePatternSwitchLabelSyntax label,
        string valueAlias,
        out string conditionExpression)
    {
        conditionExpression = string.Empty;
        var semanticModel = snapshot.Compilation.GetSemanticModel(label.SyntaxTree);
        if (semanticModel.GetOperation(label) is not IPatternCaseClauseOperation patternClause)
            return false;
        if (!CanLowerSetParametersAsyncPatternSwitchCase(patternClause))
            return false;

        try
        {
            conditionExpression = (expressionEmitter ?? new RazorVueExpressionEmitter(snapshot))
                .EmitSwitchPatternCaseCondition(patternClause, valueAlias);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(conditionExpression);
    }

    private static bool CanLowerSetParametersAsyncPatternSwitchCase(IPatternCaseClauseOperation patternClause)
    {
        if (ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Pattern) ||
            ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Guard))
        {
            return false;
        }

        foreach (var descendant in EnumerateShouldRenderExpressionScopedOperations(patternClause.Pattern))
        {
            switch (RazorVueOperationNormalizer.Unwrap(descendant))
            {
                case IDeclarationPatternOperation { DeclaredSymbol: not null }:
                case IRecursivePatternOperation { DeclaredSymbol: not null }:
                case IListPatternOperation { DeclaredSymbol: not null }:
                    return false;
            }
        }

        return true;
    }

    private static string CreateLifecycleSwitchValueAlias(SwitchStatementSyntax switchStatement)
        => "__jazorLifecycleSwitchValue" +
           Jazor.Common.Format.HashName(
               (switchStatement.SyntaxTree.FilePath ?? string.Empty) +
               ":" +
               switchStatement.SpanStart.ToString(System.Globalization.CultureInfo.InvariantCulture))
               .TrimStart('_');

    private static bool TryGetSetParametersAsyncTryCatchStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        TryStatementSyntax tryStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleTryCatchStatement lifecycleStatement)
    {
        lifecycleStatement = default!;
        if (tryStatement.Catches.Count != 1 ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Block, allowDirectNoOpReturnStatement: true) ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Catches[0].Block, allowDirectNoOpReturnStatement: false) ||
            (tryStatement.Finally is not null && ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Finally.Block, allowDirectNoOpReturnStatement: false)) ||
            ContainsThrowStatement(tryStatement))
        {
            return false;
        }

        var tryPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        if (!TryAppendPendingLifecycleLocalPreludes(
                snapshot,
                expressionEmitter,
                method,
                state,
                tryPreludeBuilder))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncCatchClause(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Catches[0],
                state,
                allowCatchBodyNullConditionFolding: true,
                out var catchFilterExpression,
                out var catchFilterPreludeBindings,
                out var catchLocal))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Block.Statements,
                0,
                endExclusive: null,
                state.CloneForDirectNoOpReturnBody(),
                out var tryStatements))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Catches[0].Block.Statements,
                0,
                endExclusive: null,
                state.CloneForCatchBody(catchLocal),
                out var catchStatements))
        {
            return false;
        }

        var finallyStatements = ImmutableArray<SupportedLifecycleStatement>.Empty;
        var hasFinally = false;
        if (tryStatement.Finally is { } finallyClause)
        {
            hasFinally = true;
            if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                finallyClause.Block.Statements,
                0,
                endExclusive: null,
                state.CloneForBranch(),
                out finallyStatements))
            {
                return false;
            }
        }

        if (tryStatements.IsDefaultOrEmpty &&
            catchStatements.IsDefaultOrEmpty &&
            finallyStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        if (ContainsLifecycleDirectReturnStatement(tryStatements) &&
            catchStatements.IsDefaultOrEmpty &&
            finallyStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        if (ContainsLifecycleDirectReturnStatement(tryStatements) &&
            !catchStatements.IsDefaultOrEmpty &&
            finallyStatements.IsDefaultOrEmpty &&
            !HasLifecycleRuntimeBeforeDirectReturn(tryStatements))
        {
            return false;
        }

        lifecycleStatement = new SupportedLifecycleTryCatchStatement(
            tryPreludeBuilder.Count == 0
                ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
                : tryPreludeBuilder.ToImmutable(),
            tryStatements,
            catchStatements,
            catchFilterExpression,
            catchFilterPreludeBindings,
            hasFinally,
            finallyStatements);
        return true;
    }

    private static bool TryGetSetParametersAsyncTryFinallyStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        TryStatementSyntax tryStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleTryFinallyStatement? lifecycleStatement)
    {
        lifecycleStatement = null;
        if (tryStatement.Catches.Count != 0 ||
            tryStatement.Finally is null ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Block, allowDirectNoOpReturnStatement: true) ||
            ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Finally.Block, allowDirectNoOpReturnStatement: false) ||
            ContainsThrowStatement(tryStatement))
        {
            return false;
        }

        var tryPreludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        if (!TryAppendPendingLifecycleLocalPreludes(
                snapshot,
                expressionEmitter,
                method,
                state,
                tryPreludeBuilder))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Block.Statements,
                0,
                endExclusive: null,
                state.CloneForDirectNoOpReturnBody(),
                out var tryStatements))
        {
            return false;
        }

        if (!TryGetSetParametersAsyncStatementSequence(
                snapshot,
                expressionEmitter,
                method,
                tryStatement.Finally.Block.Statements,
                0,
                endExclusive: null,
                state.CloneForBranch(),
                out var finallyStatements))
        {
            return false;
        }

        if (tryStatements.IsDefaultOrEmpty && finallyStatements.IsDefaultOrEmpty)
            return false;

        if (ContainsLifecycleDirectReturnStatement(tryStatements) &&
            finallyStatements.IsDefaultOrEmpty)
        {
            return false;
        }

        lifecycleStatement = new SupportedLifecycleTryFinallyStatement(
            tryPreludeBuilder.Count == 0
                ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
                : tryPreludeBuilder.ToImmutable(),
            tryStatements,
            finallyStatements);
        return true;
    }

    private static bool IsSetParametersAsyncLoopStatement(StatementSyntax statement)
        => statement is ForEachStatementSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax;

    private static bool TryGetSetParametersAsyncLoopStatement(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        StatementSyntax loopStatement,
        SetParametersAsyncStatementSequenceState state,
        out SupportedLifecycleStatement? lifecycleStatement)
    {
        lifecycleStatement = null;
        if (ContainsReturnStatement(loopStatement))
            return false;

        var semanticModel = snapshot.Compilation.GetSemanticModel(loopStatement.SyntaxTree);
        var operation = semanticModel.GetOperation(loopStatement);
        if (operation is null ||
            !CanLowerSetParametersAsyncLoopOperation(snapshot, operation, out var containsEmit) ||
            !containsEmit)
        {
            return false;
        }

        var preludeBuilder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        if (!TryAppendPendingLifecycleLocalPreludes(
                snapshot,
                expressionEmitter,
                method,
                state,
                preludeBuilder))
        {
            return false;
        }

        try
        {
            var statementText = (expressionEmitter ?? new RazorVueExpressionEmitter(snapshot))
                .EmitSetupLifecycleStatement(operation);
            if (string.IsNullOrWhiteSpace(statementText))
                return false;

            lifecycleStatement = new SupportedLifecycleCompilerStatement(
                statementText,
                preludeBuilder.Count == 0
                    ? ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty
                    : preludeBuilder.ToImmutable());
            return true;
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            return false;
        }
    }

    private static bool CanLowerSetParametersAsyncLoopOperation(
        RazorVueSemanticSnapshot snapshot,
        IOperation operation,
        out bool containsEmit)
    {
        containsEmit = false;
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IForEachLoopOperation forEachLoop:
                return !ContainsShouldRenderUnsupportedExpressionConstruct(forEachLoop.Collection) &&
                       CanLowerSetParametersAsyncLoopBody(snapshot, forEachLoop.Body, out containsEmit);

            case IForLoopOperation forLoop:
                return CanLowerSetParametersAsyncForLoopHeader(forLoop) &&
                       CanLowerSetParametersAsyncLoopBody(snapshot, forLoop.Body, out containsEmit);

            case IWhileLoopOperation whileLoop:
                return whileLoop.Condition is not null &&
                       !ContainsShouldRenderUnsupportedExpressionConstruct(whileLoop.Condition) &&
                       CanLowerSetParametersAsyncLoopBody(snapshot, whileLoop.Body, out containsEmit);

            default:
                return false;
        }
    }

    private static bool CanLowerSetParametersAsyncForLoopHeader(IForLoopOperation forLoop)
    {
        if (ContainsShouldRenderUnsupportedExpressionConstruct(forLoop.Condition))
            return false;

        foreach (var before in forLoop.Before)
        {
            if (!CanLowerSetParametersAsyncLoopLocalMutation(before) &&
                !CanLowerSetParametersAsyncLoopVariableDeclaration(before))
            {
                return false;
            }
        }

        foreach (var atLoopBottom in forLoop.AtLoopBottom)
        {
            if (!CanLowerSetParametersAsyncLoopLocalMutation(atLoopBottom))
                return false;
        }

        return true;
    }

    private static bool CanLowerSetParametersAsyncLoopBody(
        RazorVueSemanticSnapshot snapshot,
        IOperation operation,
        out bool containsEmit)
    {
        containsEmit = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IBlockOperation block:
                if (block.Operations.IsDefaultOrEmpty)
                    return false;

                foreach (var child in block.Operations)
                {
                    if (!CanLowerSetParametersAsyncLoopBodyStatement(snapshot, child, out var childContainsEmit))
                        return false;

                    containsEmit |= childContainsEmit;
                }

                return true;

            default:
                return CanLowerSetParametersAsyncLoopBodyStatement(snapshot, current, out containsEmit);
        }
    }

    private static bool CanLowerSetParametersAsyncLoopBodyStatement(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation,
        out bool containsEmit)
    {
        containsEmit = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case null:
                return false;

            case IBlockOperation block:
                if (block.Operations.IsDefaultOrEmpty)
                    return false;

                foreach (var child in block.Operations)
                {
                    if (!CanLowerSetParametersAsyncLoopBodyStatement(snapshot, child, out var childContainsEmit))
                        return false;

                    containsEmit |= childContainsEmit;
                }

                return true;

            case IEmptyOperation:
                return true;

            case IExpressionStatementOperation expressionStatement:
                return CanLowerSetParametersAsyncLoopExpressionStatement(
                    snapshot,
                    expressionStatement.Operation,
                    out containsEmit);

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(conditional.Condition) ||
                    !CanLowerSetParametersAsyncLoopBodyStatement(snapshot, conditional.WhenTrue, out var trueContainsEmit))
                {
                    return false;
                }

                containsEmit = trueContainsEmit;
                if (conditional.WhenFalse is null)
                    return true;

                if (!CanLowerSetParametersAsyncLoopBodyStatement(snapshot, conditional.WhenFalse, out var falseContainsEmit))
                    return false;

                containsEmit |= falseContainsEmit;
                return true;

            case IBranchOperation { BranchKind: BranchKind.Break or BranchKind.Continue }:
                return true;

            case IForEachLoopOperation or IForLoopOperation or IWhileLoopOperation:
                return CanLowerSetParametersAsyncLoopOperation(snapshot, current, out containsEmit);

            default:
                return false;
        }
    }

    private static bool CanLowerSetParametersAsyncLoopExpressionStatement(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation,
        out bool containsEmit)
    {
        containsEmit = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is IAwaitOperation awaitOperation)
            current = RazorVueOperationNormalizer.Unwrap(awaitOperation.Operation);

        switch (current)
        {
            case IInvocationOperation invocation:
                containsEmit = true;
                return IsSetParametersAsyncEmitInvocation(snapshot, invocation);

            case IIncrementOrDecrementOperation incrementOrDecrement:
                return CanLowerSetParametersAsyncLoopLocalMutation(incrementOrDecrement);

            case ISimpleAssignmentOperation assignment:
                return CanLowerSetParametersAsyncLoopLocalMutation(assignment);

            case ICompoundAssignmentOperation assignment:
                return CanLowerSetParametersAsyncLoopLocalMutation(assignment);

            default:
                return false;
        }
    }

    private static bool IsSetParametersAsyncEmitInvocation(
        RazorVueSemanticSnapshot snapshot,
        IInvocationOperation invocation)
    {
        if (!string.Equals(invocation.TargetMethod.Name, "InvokeAsync", StringComparison.Ordinal))
            return false;

        var callbackInstance = RazorVueOperationNormalizer.Unwrap(invocation.Instance);
        if (callbackInstance is not IPropertyReferenceOperation property ||
            !RazorVueSymbolIdentity.IsCurrentComponentMember(
                snapshot.ComponentSymbol,
                property.Property,
                property.Instance,
                RazorVueOperationNormalizer.Unwrap) ||
            !IsComponentParameterProperty(property.Property) ||
            !IsEventCallbackLike(property.Property.Type))
        {
            return false;
        }

        if (invocation.Arguments.Length > 1)
            return false;

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { RefKind: RefKind.None } ||
                argument.ArgumentKind != ArgumentKind.Explicit ||
                ContainsShouldRenderUnsupportedExpressionConstruct(argument.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CanLowerSetParametersAsyncLoopVariableDeclaration(IOperation? operation)
    {
        if (RazorVueOperationNormalizer.Unwrap(operation) is not IVariableDeclarationGroupOperation declarationGroup)
            return false;

        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Initializer?.Value is { } initializer &&
                    ContainsShouldRenderUnsupportedExpressionConstruct(initializer))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool CanLowerSetParametersAsyncLoopLocalMutation(IOperation? operation)
    {
        return RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IIncrementOrDecrementOperation incrementOrDecrement =>
                IsSetParametersAsyncLoopLocalMutationTarget(incrementOrDecrement.Target),
            ISimpleAssignmentOperation assignment =>
                IsSetParametersAsyncLoopLocalMutationTarget(assignment.Target) &&
                !ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value),
            ICompoundAssignmentOperation assignment =>
                IsSetParametersAsyncLoopLocalMutationTarget(assignment.Target) &&
                !ContainsShouldRenderUnsupportedExpressionConstruct(assignment.Value),
            _ => false
        };
    }

    private static bool IsSetParametersAsyncLoopLocalMutationTarget(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation) is ILocalReferenceOperation;

    private static bool IsComponentParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.ParameterAttribute",
                StringComparison.Ordinal));

    private static bool IsEventCallbackLike(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            var originalDefinition = namedType.OriginalDefinition.ToDisplayString();
            return string.Equals(
                       originalDefinition,
                       "Microsoft.AspNetCore.Components.EventCallback",
                       StringComparison.Ordinal) ||
                   string.Equals(
                       originalDefinition,
                       "Microsoft.AspNetCore.Components.EventCallback<TValue>",
                       StringComparison.Ordinal);
        }

        return false;
    }

    private static bool TryGetLifecycleSwitchLabelExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SetParametersAsyncStatementSequenceState state,
        ExpressionSyntax expressionSyntax,
        out string expression)
    {
        expression = string.Empty;
        var semanticModel = snapshot.Compilation.GetSemanticModel(expressionSyntax.SyntaxTree);
        var operation = semanticModel.GetOperation(expressionSyntax);
        if (operation is null ||
            ContainsShouldRenderUnsupportedExpressionConstruct(operation))
        {
            return false;
        }

        RazorVueExpressionEmitter.LifecyclePayloadEmission emission;
        try
        {
            emission = expressionEmitter is null
                ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, operation, allowFirstRenderPayload: false)
                : expressionEmitter.EmitLifecyclePayload(method, operation, allowFirstRenderPayload: false);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        if (emission.UsesFirstRender ||
            !emission.PreludeBindings.IsDefaultOrEmpty ||
            string.IsNullOrWhiteSpace(emission.Expression) ||
            ReferencesLifecycleMaterializedLocal(emission.Expression, state))
        {
            return false;
        }

        expression = emission.Expression;
        return true;
    }

    private static bool ReferencesLifecycleMaterializedLocal(
        string expression,
        SetParametersAsyncStatementSequenceState state)
    {
        foreach (var alias in state.LocalAliases.Values)
        {
            if (expression.Contains(alias, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static bool CanIgnoreSetParametersAsyncNoOpCondition(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        ExpressionSyntax conditionSyntax,
        SetParametersAsyncStatementSequenceState state)
    {
        _ = expressionEmitter;
        if (!CanIgnoreSetParametersAsyncPendingLocals(
                snapshot,
                method,
                state))
        {
            return false;
        }

        var semanticModel = snapshot.Compilation.GetSemanticModel(conditionSyntax.SyntaxTree);
        var operation = semanticModel.GetOperation(conditionSyntax);
        if (operation is null ||
            ContainsSetParametersAsyncNoOpIgnoredSideEffect(operation))
        {
            return false;
        }

        try
        {
            var condition = RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, operation, allowFirstRenderPayload: false);

            return !condition.UsesFirstRender &&
                   !string.IsNullOrWhiteSpace(condition.Expression);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static bool CanIgnoreSetParametersAsyncPendingLocals(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        SetParametersAsyncStatementSequenceState state)
    {
        foreach (var local in OrderLifecycleLocalsBySource(state.EmittedLocals))
        {
            if (state.MaterializedLocals.Contains(local))
                continue;
            if (!state.LocalInitializers.TryGetValue(local, out var initializer))
                return false;
            if (ContainsSetParametersAsyncNoOpIgnoredSideEffect(initializer))
                return false;

            try
            {
                _ = RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, initializer, allowFirstRenderPayload: false);
            }
            catch (RazorVueCompilationIssueException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsSetParametersAsyncNoOpIgnoredSideEffect(IOperation operation)
    {
        foreach (var current in EnumerateShouldRenderExpressionScopedOperations(operation))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is
                IAnonymousFunctionOperation or
                ILocalFunctionOperation or
                IInvocationOperation or
                IMethodReferenceOperation or
                IObjectCreationOperation or
                IAnonymousObjectCreationOperation or
                IArrayCreationOperation or
                ICollectionExpressionOperation or
                ISimpleAssignmentOperation or
                ICompoundAssignmentOperation or
                IIncrementOrDecrementOperation)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsLifecycleDirectReturnStatement(ImmutableArray<SupportedLifecycleStatement> statements)
        => !statements.IsDefaultOrEmpty &&
           statements.Any(static statement => statement is SupportedLifecycleReturnStatement);

    private static bool HasLifecycleRuntimeBeforeDirectReturn(ImmutableArray<SupportedLifecycleStatement> statements)
    {
        if (statements.IsDefaultOrEmpty)
            return false;

        for (var index = 0; index < statements.Length; index++)
        {
            if (statements[index] is SupportedLifecycleReturnStatement returnStatement)
                return index > 0 || !returnStatement.PreludeBindings.IsDefaultOrEmpty;
        }

        return false;
    }

    private static ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> FilterLifecyclePreludeBindings(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> bindings,
        SetParametersAsyncStatementSequenceState state)
    {
        if (bindings.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty;

        var builder = ImmutableArray.CreateBuilder<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>();
        foreach (var binding in bindings)
        {
            if (TryGetLifecycleConstPreludeAlias(binding, out var alias))
            {
                if (TryFindLifecycleLocalByAlias(state.LocalAliases, alias, out var local))
                {
                    if (state.MaterializedLocals.Add(local))
                    {
                        state.MaterializedPreludeAliases.Add(alias);
                        builder.Add(binding);
                    }

                    continue;
                }

                if (state.MaterializedPreludeAliases.Add(alias))
                {
                    builder.Add(binding);
                }

                continue;
            }

            builder.Add(binding);
        }

        return builder.ToImmutable();
    }

    private static bool TryAppendPendingLifecycleLocalPreludes(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        SetParametersAsyncStatementSequenceState state,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Builder builder)
    {
        foreach (var local in OrderLifecycleLocalsBySource(state.EmittedLocals))
        {
            if (local.Type.TypeKind == TypeKind.Delegate)
                continue;
            if (state.MaterializedLocals.Contains(local))
                continue;
            if (!state.LocalInitializers.TryGetValue(local, out var initializer))
                return false;
            if (!state.LocalAliases.TryGetValue(local, out var alias))
                return false;

            RazorVueExpressionEmitter.LifecyclePayloadEmission emission;
            try
            {
                emission = expressionEmitter is null
                    ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, initializer, allowFirstRenderPayload: false)
                    : expressionEmitter.EmitLifecyclePayload(method, initializer, allowFirstRenderPayload: false);
            }
            catch (RazorVueCompilationIssueException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }

            if (emission.UsesFirstRender ||
                string.IsNullOrWhiteSpace(emission.Expression))
            {
                return false;
            }

            builder.AddRange(FilterLifecyclePreludeBindings(emission.PreludeBindings, state));
            if (state.MaterializedLocals.Add(local))
                builder.Add(RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding.Const(alias, emission.Expression));
        }

        return true;
    }

    private static IEnumerable<ILocalSymbol> OrderLifecycleLocalsBySource(IEnumerable<ILocalSymbol> locals)
        => locals
            .OrderBy(static local => local.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceTree?.FilePath ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(static local => local.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
            .ThenBy(static local => local.Name, StringComparer.Ordinal);

    private static IReadOnlyList<StatementSyntax> ToLifecycleBranchStatements(StatementSyntax statement)
        => statement is BlockSyntax block
            ? block.Statements.ToArray()
            : new[] { statement };

    private static bool TerminatesWithNoOpReturn(IfStatementSyntax ifStatement, bool allowImplicitContinue)
    {
        if (ifStatement.Else is null)
            return allowImplicitContinue && IsGuardReturnStatement(ifStatement.Statement);

        return IsGuardReturnStatement(ifStatement.Statement) &&
               IsGuardReturnStatement(ifStatement.Else.Statement);
    }

    private static bool ContainsReturnStatement(StatementSyntax statement)
        => statement.DescendantNodesAndSelf().OfType<ReturnStatementSyntax>().Any();

    private static bool ContainsDirectUnsupportedLifecycleReturnStatement(
        BlockSyntax block,
        bool allowDirectNoOpReturnStatement)
    {
        foreach (var statement in block.Statements)
        {
            if (statement is ReturnStatementSyntax)
                return !allowDirectNoOpReturnStatement || statement is not ReturnStatementSyntax { Expression: null };

            switch (statement)
            {
                case IfStatementSyntax ifStatement:
                    if (IsGuardReturnStatement(ifStatement.Statement) &&
                        ifStatement.Else is null)
                    {
                        continue;
                    }

                    if (ifStatement.Else?.Statement is { } elseStatement &&
                        IsGuardReturnStatement(ifStatement.Statement) != IsGuardReturnStatement(elseStatement))
                    {
                        continue;
                    }

                    if (ContainsReturnStatement(ifStatement))
                        return true;

                    break;

                case SwitchStatementSyntax switchStatement when ContainsReturnStatement(switchStatement):
                    return true;

                case TryStatementSyntax tryStatement:
                    if (ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Block, allowDirectNoOpReturnStatement: false))
                        return true;

                    foreach (var catchClause in tryStatement.Catches)
                    {
                        if (ContainsDirectUnsupportedLifecycleReturnStatement(catchClause.Block, allowDirectNoOpReturnStatement: false))
                            return true;
                    }

                    if (tryStatement.Finally is not null &&
                        ContainsDirectUnsupportedLifecycleReturnStatement(tryStatement.Finally.Block, allowDirectNoOpReturnStatement: false))
                    {
                        return true;
                    }

                    break;

                default:
                    if (statement.DescendantNodes().OfType<ReturnStatementSyntax>().Any())
                        return true;

                    break;
            }
        }

        return false;
    }

    private static bool ContainsThrowStatement(StatementSyntax statement)
        => statement.DescendantNodesAndSelf().OfType<ThrowStatementSyntax>().Any();

    private static bool TryGetSetParametersAsyncCatchClause(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter? expressionEmitter,
        IMethodSymbol method,
        CatchClauseSyntax catchClause,
        SetParametersAsyncStatementSequenceState state,
        bool allowCatchBodyNullConditionFolding,
        out string? filterExpression,
        out ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> filterPreludeBindings,
        out ILocalSymbol? catchLocal)
    {
        filterExpression = null;
        filterPreludeBindings = ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty;
        catchLocal = null;

        var semanticModel = snapshot.Compilation.GetSemanticModel(catchClause.SyntaxTree);
        if (catchClause.Declaration is null)
        {
            // catch-all
        }
        else
        {
            var catchType = semanticModel.GetTypeInfo(catchClause.Declaration.Type).Type;
            var systemException = snapshot.Compilation.GetTypeByMetadataName("System.Exception");
            if (systemException is null ||
                catchType is null ||
                !SymbolEqualityComparer.Default.Equals(catchType.OriginalDefinition, systemException))
            {
                return false;
            }

            if (catchClause.Declaration.Identifier.ValueText.Length > 0)
            {
                if (semanticModel.GetDeclaredSymbol(catchClause.Declaration) is not ILocalSymbol declaredCatchLocal)
                    return false;

                catchLocal = declaredCatchLocal;
                if (!allowCatchBodyNullConditionFolding &&
                    ReferencesLocal(catchClause.Block, semanticModel, catchLocal))
                {
                    return false;
                }
            }
        }

        if (catchClause.Filter is null)
            return true;

        var filterSyntax = catchClause.Filter.FilterExpression;
        if (catchLocal is not null &&
            ReferencesLocal(filterSyntax, semanticModel, catchLocal))
        {
            if (!TryFoldSetParametersAsyncCatchLocalNullFilter(
                    filterSyntax,
                    semanticModel,
                    catchLocal,
                    out filterExpression))
            {
                return false;
            }

            return true;
        }

        var filterOperation = semanticModel.GetOperation(filterSyntax);
        if (filterOperation is null ||
            ContainsShouldRenderUnsupportedExpressionConstruct(filterOperation))
        {
            return false;
        }

        RazorVueExpressionEmitter.LifecyclePayloadEmission emission;
        try
        {
            emission = expressionEmitter is null
                ? RazorVueExpressionEmitter.EmitLifecyclePayload(snapshot, method, filterOperation, allowFirstRenderPayload: false)
                : expressionEmitter.EmitLifecyclePayload(method, filterOperation, allowFirstRenderPayload: false);
        }
        catch (RazorVueCompilationIssueException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        if (emission.UsesFirstRender ||
            string.IsNullOrWhiteSpace(emission.Expression))
        {
            return false;
        }

        filterExpression = emission.Expression;
        filterPreludeBindings = FilterLifecyclePreludeBindings(emission.PreludeBindings, state.CloneForBranch());
        return true;
    }

    private static bool TryFoldSetParametersAsyncCatchBodyNullCondition(
        IOperation conditionOperation,
        SetParametersAsyncStatementSequenceState state,
        out string conditionExpression)
    {
        conditionExpression = string.Empty;
        if (state.FoldableCatchLocals.Count == 0)
            return false;

        ILocalSymbol? referencedCatchLocal = null;
        foreach (var current in conditionOperation.DescendantsAndSelf())
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is not ILocalReferenceOperation localReference ||
                !state.FoldableCatchLocals.Contains(localReference.Local))
            {
                continue;
            }

            if (referencedCatchLocal is null)
            {
                referencedCatchLocal = localReference.Local;
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(referencedCatchLocal, localReference.Local))
                return false;
        }

        if (referencedCatchLocal is null ||
            !TryEvaluateSetParametersAsyncCatchLocalNullCheck(
                conditionOperation,
                referencedCatchLocal,
                out var result))
        {
            return false;
        }

        conditionExpression = result ? "true" : "false";
        return true;
    }

    private static bool TryFoldSetParametersAsyncCatchLocalNullFilter(
        ExpressionSyntax filterSyntax,
        SemanticModel semanticModel,
        ILocalSymbol catchLocal,
        out string filterExpression)
    {
        filterExpression = string.Empty;
        if (!TryEvaluateSetParametersAsyncCatchLocalNullCheck(
                semanticModel.GetOperation(filterSyntax),
                catchLocal,
                out var result))
        {
            return false;
        }

        filterExpression = result ? "true" : "false";
        return true;
    }

    private static bool TryEvaluateSetParametersAsyncCatchLocalNullCheck(
        IOperation? operation,
        ILocalSymbol catchLocal,
        out bool result)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IIsPatternOperation isPattern
                when IsSetParametersAsyncCatchLocalReference(isPattern.Value, catchLocal) &&
                     TryEvaluateSetParametersAsyncCatchLocalNullPattern(isPattern.Pattern, out result):
                return true;

            case IBinaryOperation binaryOperation
                when binaryOperation.OperatorKind is BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals &&
                     IsSetParametersAsyncCatchLocalNullComparison(binaryOperation, catchLocal):
                result = binaryOperation.OperatorKind == BinaryOperatorKind.NotEquals;
                return true;

            case IUnaryOperation { OperatorKind: UnaryOperatorKind.Not } unaryOperation
                when TryEvaluateSetParametersAsyncCatchLocalNullCheck(unaryOperation.Operand, catchLocal, out var nestedResult):
                result = !nestedResult;
                return true;

            default:
                result = false;
                return false;
        }
    }

    private static bool IsSetParametersAsyncCatchLocalNullComparison(
        IBinaryOperation operation,
        ILocalSymbol catchLocal)
        =>
            (IsSetParametersAsyncCatchLocalReference(operation.LeftOperand, catchLocal) &&
             IsNullConstantOperation(operation.RightOperand)) ||
            (IsSetParametersAsyncCatchLocalReference(operation.RightOperand, catchLocal) &&
             IsNullConstantOperation(operation.LeftOperand));

    private static bool TryEvaluateSetParametersAsyncCatchLocalNullPattern(
        IPatternOperation pattern,
        out bool result)
    {
        switch (RazorVueOperationNormalizer.Unwrap(pattern))
        {
            case IConstantPatternOperation constantPattern
                when IsNullConstantOperation(constantPattern.Value):
                result = false;
                return true;

            case INegatedPatternOperation negatedPattern
                when TryEvaluateSetParametersAsyncCatchLocalNullPattern(negatedPattern.Pattern, out var nestedResult):
                result = !nestedResult;
                return true;

            default:
                result = false;
                return false;
        }
    }

    private static bool IsSetParametersAsyncCatchLocalReference(
        IOperation? operation,
        ILocalSymbol catchLocal)
        => RazorVueOperationNormalizer.Unwrap(operation) is ILocalReferenceOperation localReference &&
           SymbolEqualityComparer.Default.Equals(localReference.Local, catchLocal);

    private static bool ReferencesLocal(
        SyntaxNode syntax,
        SemanticModel semanticModel,
        ILocalSymbol local)
    {
        if (semanticModel.GetOperation(syntax) is not { } operation)
            return false;

        foreach (var current in operation.DescendantsAndSelf())
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is ILocalReferenceOperation localReference &&
                SymbolEqualityComparer.Default.Equals(localReference.Local, local))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsGuardReturnStatement(StatementSyntax statement)
        => statement switch
        {
            ReturnStatementSyntax { Expression: null } => true,
            BlockSyntax { Statements.Count: 1 } block => block.Statements[0] is ReturnStatementSyntax { Expression: null },
            _ => false
        };

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

    private static bool TryGetLifecycleConstPreludeAlias(
        RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding binding,
        out string alias)
    {
        const string constPrefix = "const __jazorLifecycle";
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

    private static bool IsTaskLikeType(Compilation compilation, ITypeSymbol? type)
    {
        if (type is null)
            return false;

        return IsSameOriginalDefinition(type, compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")) ||
               IsSameOriginalDefinition(type, compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")) ||
               IsSameOriginalDefinition(type, compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask")) ||
               IsSameOriginalDefinition(type, compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1"));
    }

    private static bool IsSameOriginalDefinition(ITypeSymbol type, INamedTypeSymbol? candidate)
        => candidate is not null &&
           SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, candidate);

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

            if (!TryCollectShouldRenderAssignedDelegateLocalInitializers(
                    blockOperation.Operations,
                    out var assignedDelegateInitializers))
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            if (!TryValidateShouldRenderStatementSequence(
                    blockOperation.Operations,
                    ShouldRenderStatementScope.MethodBody,
                    assignedDelegateInitializers,
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
                    assignedDelegateInitializers,
                    out var localAliases))
            {
                return ShouldRenderAnalysis.Unsupported;
            }

            if (!ValidateShouldRenderDelegateLocalUsage(blockOperation.Operations, assignedDelegateInitializers))
                return ShouldRenderAnalysis.Unsupported;

            var capture = expressionEmitter.CaptureSetupDependencies(
                () => expressionEmitter.WithScopedLocalAliases(
                    localAliases,
                    () => expressionEmitter.EmitSetupShouldRenderStatementSequence(blockOperation.Operations)));
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
        BranchBody,
        SwitchCaseBody,
        LoopBody,
        LoopSwitchCaseBody
    }

    private static ShouldRenderStatementScope CreateShouldRenderNestedStatementScope(ShouldRenderStatementScope scope)
        => scope == ShouldRenderStatementScope.MethodBody
            ? ShouldRenderStatementScope.BranchBody
            : scope;

    private static ShouldRenderStatementScope CreateShouldRenderSwitchCaseScope(ShouldRenderStatementScope scope)
        => IsShouldRenderLoopScope(scope)
            ? ShouldRenderStatementScope.LoopSwitchCaseBody
            : ShouldRenderStatementScope.SwitchCaseBody;

    private static bool IsShouldRenderLoopScope(ShouldRenderStatementScope scope)
        => scope is ShouldRenderStatementScope.LoopBody or ShouldRenderStatementScope.LoopSwitchCaseBody;

    private static bool IsShouldRenderSwitchCaseScope(ShouldRenderStatementScope scope)
        => scope is ShouldRenderStatementScope.SwitchCaseBody or ShouldRenderStatementScope.LoopSwitchCaseBody;

    private static bool TryValidateShouldRenderStatementSequence(
        ImmutableArray<IOperation> operations,
        ShouldRenderStatementScope scope,
        out bool alwaysReturns,
        out bool containsConditional)
        => TryValidateShouldRenderStatementSequence(
            operations,
            scope,
            EmptyShouldRenderDelegateLocalUsages,
            out alwaysReturns,
            out containsConditional);

    private static bool TryValidateShouldRenderStatementSequence(
        ImmutableArray<IOperation> operations,
        ShouldRenderStatementScope scope,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers,
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
                    assignedDelegateInitializers,
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
        => TryValidateShouldRenderStatement(
            operation,
            scope,
            EmptyShouldRenderDelegateLocalUsages,
            out alwaysReturns,
            out containsConditional);

    private static bool TryValidateShouldRenderStatement(
        IOperation operation,
        ShouldRenderStatementScope scope,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers,
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
                    CreateShouldRenderNestedStatementScope(scope),
                    assignedDelegateInitializers,
                    out alwaysReturns,
                    out containsConditional);

            case IReturnOperation { ReturnedValue: not null } returnOperation:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(returnOperation.ReturnedValue))
                    return false;

                alwaysReturns = true;
                return true;

            case IThrowOperation throwOperation:
                if (throwOperation.Exception is null &&
                    throwOperation.Syntax.FirstAncestorOrSelf<CatchClauseSyntax>() is null)
                {
                    return false;
                }

                if (ContainsShouldRenderUnsupportedExpressionConstruct(throwOperation.Exception))
                    return false;

                alwaysReturns = true;
                return true;

            case IVariableDeclarationGroupOperation declarationGroup:
                if (!TryValidateShouldRenderVariableDeclarationGroup(declarationGroup, assignedDelegateInitializers))
                    return false;

                return true;

            case IExpressionStatementOperation expressionStatement:
                return TryValidateShouldRenderLocalMutationExpressionStatement(expressionStatement) ||
                    (scope == ShouldRenderStatementScope.MethodBody &&
                     TryValidateShouldRenderAssignedDelegateInitializerExpressionStatement(
                         expressionStatement,
                         assignedDelegateInitializers));

            case IBranchOperation { BranchKind: BranchKind.Break } when IsShouldRenderSwitchCaseScope(scope) || IsShouldRenderLoopScope(scope):
                return true;

            case IBranchOperation { BranchKind: BranchKind.Continue } when IsShouldRenderLoopScope(scope):
                return true;

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
                        CreateShouldRenderNestedStatementScope(scope),
                        assignedDelegateInitializers,
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
                        CreateShouldRenderNestedStatementScope(scope),
                        assignedDelegateInitializers,
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
                    scope,
                    out alwaysReturns,
                    out containsConditional);

            case IWhileLoopOperation whileLoop:
                return TryValidateShouldRenderWhileLoop(
                    whileLoop,
                    out alwaysReturns,
                    out containsConditional);

            case IForLoopOperation forLoop:
                return TryValidateShouldRenderForLoop(
                    forLoop,
                    out alwaysReturns,
                    out containsConditional);

            case IForEachLoopOperation forEachLoop:
                return TryValidateShouldRenderForEachLoop(
                    forEachLoop,
                    out alwaysReturns,
                    out containsConditional);

            case ITryOperation tryOperation:
                return TryValidateShouldRenderTryStatement(
                    tryOperation,
                    scope,
                    out alwaysReturns,
                    out containsConditional);

            default:
                return false;
        }
    }

    private static bool TryValidateShouldRenderSwitchStatement(
        ISwitchOperation switchOperation,
        ShouldRenderStatementScope scope,
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

                    case IPatternCaseClauseOperation patternClause:
                        if (ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Pattern) ||
                            ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Guard))
                        {
                            return false;
                        }

                        break;

                    default:
                        return false;
                }
            }

            if (!TryValidateShouldRenderStatementSequence(
                    switchCase.Body,
                    CreateShouldRenderSwitchCaseScope(scope),
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

    private static bool TryValidateShouldRenderWhileLoop(
        IWhileLoopOperation whileLoop,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;

        if (ContainsShouldRenderUnsupportedExpressionConstruct(whileLoop.Condition))
            return false;

        if (!TryValidateShouldRenderStatement(
                whileLoop.Body,
                ShouldRenderStatementScope.LoopBody,
                out _,
                out var bodyContainsConditional))
        {
            return false;
        }

        containsConditional |= bodyContainsConditional;
        return true;
    }

    private static bool TryValidateShouldRenderForLoop(
        IForLoopOperation forLoop,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;

        foreach (var before in forLoop.Before)
        {
            if (!TryValidateShouldRenderLoopHeaderOperation(before))
                return false;
        }

        if (ContainsShouldRenderUnsupportedExpressionConstruct(forLoop.Condition))
            return false;

        foreach (var atLoopBottom in forLoop.AtLoopBottom)
        {
            if (!TryValidateShouldRenderLoopHeaderOperation(atLoopBottom))
                return false;
        }

        if (!TryValidateShouldRenderStatement(
                forLoop.Body,
                ShouldRenderStatementScope.LoopBody,
                out _,
                out var bodyContainsConditional))
        {
            return false;
        }

        containsConditional |= bodyContainsConditional;
        return true;
    }

    private static bool TryValidateShouldRenderForEachLoop(
        IForEachLoopOperation forEachLoop,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;

        if (forEachLoop.IsAsynchronous ||
            ContainsShouldRenderUnsupportedExpressionConstruct(forEachLoop.Collection))
        {
            return false;
        }

        if (!TryValidateShouldRenderStatement(
                forEachLoop.Body,
                ShouldRenderStatementScope.LoopBody,
                out _,
                out var bodyContainsConditional))
        {
            return false;
        }

        containsConditional |= bodyContainsConditional;
        return true;
    }

    private static bool TryValidateShouldRenderTryStatement(
        ITryOperation tryOperation,
        ShouldRenderStatementScope scope,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;

        if (!TryValidateShouldRenderStatement(
                tryOperation.Body,
                CreateShouldRenderNestedStatementScope(scope),
                out var bodyAlwaysReturns,
                out var bodyContainsConditional))
        {
            return false;
        }

        containsConditional |= bodyContainsConditional;

        var allCatchHandlersAlwaysReturn = true;
        foreach (var catchClause in tryOperation.Catches)
        {
            if (!TryValidateShouldRenderCatchClause(
                    catchClause,
                    scope,
                    out var catchAlwaysReturns,
                    out var catchContainsConditional))
            {
                return false;
            }

            containsConditional |= catchContainsConditional;
            allCatchHandlersAlwaysReturn &= catchAlwaysReturns;
        }

        if (tryOperation.Finally is not null)
        {
            if (!TryValidateShouldRenderStatement(
                    tryOperation.Finally,
                    CreateShouldRenderNestedStatementScope(scope),
                    out var finallyAlwaysReturns,
                    out var finallyContainsConditional))
            {
                return false;
            }

            if (finallyAlwaysReturns)
                return false;

            containsConditional |= finallyContainsConditional;
        }

        alwaysReturns = bodyAlwaysReturns &&
            (tryOperation.Catches.IsDefaultOrEmpty || allCatchHandlersAlwaysReturn);
        return true;
    }

    private static bool TryValidateShouldRenderCatchClause(
        ICatchClauseOperation catchClause,
        ShouldRenderStatementScope scope,
        out bool alwaysReturns,
        out bool containsConditional)
    {
        alwaysReturns = false;
        containsConditional = true;

        if (!TryValidateShouldRenderCatchBinding(catchClause) ||
            ContainsShouldRenderUnsupportedExpressionConstruct(catchClause.Filter))
        {
            return false;
        }

        if (!TryValidateShouldRenderStatement(
                catchClause.Handler,
                CreateShouldRenderNestedStatementScope(scope),
                out alwaysReturns,
                out var handlerContainsConditional))
        {
            return false;
        }

        containsConditional |= handlerContainsConditional;
        return true;
    }

    private static bool TryValidateShouldRenderLoopHeaderOperation(IOperation operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IVariableDeclarationGroupOperation declarationGroup:
                return !ContainsShouldRenderUnsupportedExpressionConstruct(declarationGroup);

            case IVariableDeclarationOperation declaration:
                return !ContainsShouldRenderUnsupportedExpressionConstruct(declaration);

            case IVariableDeclaratorOperation declarator:
                return !ContainsShouldRenderUnsupportedExpressionConstruct(declarator);

            case IExpressionStatementOperation expressionStatement:
                return TryValidateShouldRenderLocalMutationExpressionStatement(expressionStatement);

            case ISimpleAssignmentOperation or ICompoundAssignmentOperation or IIncrementOrDecrementOperation:
                return TryValidateShouldRenderLocalMutationExpression(current);

            default:
                return false;
        }
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
        => RazorVueOperationNormalizer.Unwrap(operation) is ILocalReferenceOperation localReference &&
           localReference.Local.Type.TypeKind != TypeKind.Delegate;

    private static bool TryCollectShouldRenderAssignedDelegateLocalInitializers(
        ImmutableArray<IOperation> operations,
        out ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        assignedDelegateInitializers = EmptyShouldRenderDelegateLocalUsages;
        if (operations.IsDefaultOrEmpty)
            return true;

        var uninitializedDelegateLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var assignedDelegates = ImmutableDictionary.CreateBuilder<ILocalSymbol, ShouldRenderDelegateLocalUsageKind>(SymbolEqualityComparer.Default);
        var executablePrefixClosed = false;

        foreach (var operation in operations)
        {
            switch (RazorVueOperationNormalizer.Unwrap(operation))
            {
                case ILocalFunctionOperation:
                    continue;

                case IVariableDeclarationGroupOperation declarationGroup:
                    if (executablePrefixClosed &&
                        ContainsUninitializedShouldRenderDelegateLocalDeclaration(declarationGroup))
                    {
                        return false;
                    }

                    if (!executablePrefixClosed &&
                        !TryCollectUninitializedShouldRenderDelegateLocalDeclarations(
                            declarationGroup,
                            uninitializedDelegateLocals,
                            assignedDelegates))
                    {
                        return false;
                    }

                    continue;

                case IExpressionStatementOperation expressionStatement
                    when TryGetShouldRenderDelegateInitializerAssignment(
                        expressionStatement,
                        out var local,
                        out var usageKind):
                    if (executablePrefixClosed ||
                        !uninitializedDelegateLocals.Contains(local) ||
                        assignedDelegates.ContainsKey(local))
                    {
                        return false;
                    }

                    assignedDelegates[local] = usageKind;
                    continue;

                default:
                    executablePrefixClosed = true;
                    continue;
            }
        }

        foreach (var local in uninitializedDelegateLocals)
        {
            if (!assignedDelegates.ContainsKey(local))
                return false;
        }

        assignedDelegateInitializers = assignedDelegates.ToImmutable();
        return true;
    }

    private static bool TryCollectUninitializedShouldRenderDelegateLocalDeclarations(
        IVariableDeclarationGroupOperation declarationGroup,
        HashSet<ILocalSymbol> uninitializedDelegateLocals,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind>.Builder assignedDelegates)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate ||
                    declarator.Initializer is not null)
                {
                    continue;
                }

                if (assignedDelegates.ContainsKey(declarator.Symbol) ||
                    !uninitializedDelegateLocals.Add(declarator.Symbol))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ContainsUninitializedShouldRenderDelegateLocalDeclaration(IVariableDeclarationGroupOperation declarationGroup)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Symbol.Type.TypeKind == TypeKind.Delegate &&
                    declarator.Initializer is null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryValidateShouldRenderAssignedDelegateInitializerExpressionStatement(
        IExpressionStatementOperation expressionStatement,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
        => TryGetShouldRenderDelegateInitializerAssignment(expressionStatement, out var local, out var usageKind) &&
           assignedDelegateInitializers.TryGetValue(local, out var expectedUsageKind) &&
           usageKind == expectedUsageKind;

    private static bool TryGetShouldRenderDelegateInitializerAssignment(
        IExpressionStatementOperation expressionStatement,
        out ILocalSymbol local,
        out ShouldRenderDelegateLocalUsageKind usageKind)
    {
        local = default!;
        usageKind = default;

        return RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation) is ISimpleAssignmentOperation assignment &&
               TryGetShouldRenderDelegateInitializerAssignment(assignment, out local, out usageKind);
    }

    private static bool TryGetShouldRenderDelegateInitializerAssignment(
        ISimpleAssignmentOperation assignment,
        out ILocalSymbol local,
        out ShouldRenderDelegateLocalUsageKind usageKind)
    {
        local = default!;
        usageKind = default;

        if (RazorVueOperationNormalizer.Unwrap(assignment.Target) is not ILocalReferenceOperation localReference ||
            localReference.Local.Type.TypeKind != TypeKind.Delegate)
        {
            return false;
        }

        if (TryGetShouldRenderAnonymousFunction(assignment.Value, out var anonymousFunction))
        {
            if (!CanLowerShouldRenderLocalAnonymousFunction(anonymousFunction))
                return false;

            local = localReference.Local;
            usageKind = ShouldRenderDelegateLocalUsageKind.Callable;
            return true;
        }

        if (TryGetShouldRenderMethodGroup(assignment.Value, out var methodReference) &&
            CanLowerShouldRenderMethodGroupDelegateInitializer(
                methodReference,
                assignment.SemanticModel ?? methodReference.SemanticModel))
        {
            local = localReference.Local;
            usageKind = ShouldRenderDelegateLocalUsageKind.Callable;
            return true;
        }

        return false;
    }

    private static bool TryValidateShouldRenderVariableDeclarationGroup(IVariableDeclarationGroupOperation declarationGroup)
        => TryValidateShouldRenderVariableDeclarationGroup(declarationGroup, EmptyShouldRenderDelegateLocalUsages);

    private static bool TryValidateShouldRenderVariableDeclarationGroup(
        IVariableDeclarationGroupOperation declarationGroup,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Symbol.Type.TypeKind == TypeKind.Delegate)
                {
                    if (assignedDelegateInitializers.ContainsKey(declarator.Symbol) &&
                        declarator.Initializer is null)
                    {
                        continue;
                    }

                    if (TryGetShouldRenderLocalAnonymousFunctionInitializer(declarator, out var anonymousFunction))
                    {
                        if (!CanLowerShouldRenderLocalAnonymousFunction(
                                anonymousFunction))
                        {
                            return false;
                        }

                        continue;
                    }

                    if (IsNullConstantOperation(declarator.Initializer?.Value))
                        continue;

                    if (TryGetShouldRenderLocalFunctionDelegateIdentityInitializer(declarator, out var identityInvocation) &&
                        CanLowerShouldRenderLocalFunctionDelegateIdentityInitializer(identityInvocation))
                    {
                        continue;
                    }

                    if (TryGetShouldRenderLocalFunctionDelegateFactoryInitializer(declarator, out var factoryInvocation) &&
                        CanLowerShouldRenderLocalFunctionDelegateFactoryInitializer(factoryInvocation))
                    {
                        continue;
                    }

                    if (!TryGetShouldRenderLocalMethodGroupInitializer(declarator, out var methodReference, out var semanticModel) ||
                        !CanLowerShouldRenderMethodGroupDelegateInitializer(methodReference, semanticModel))
                    {
                        return false;
                    }

                    continue;
                }

                if (ContainsShouldRenderUnsupportedExpressionConstruct(declarator))
                    return false;
            }
        }

        return true;
    }

    private static bool TryCreateShouldRenderLocalAliases(
        ImmutableArray<IOperation> operations,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers,
        out IReadOnlyDictionary<ILocalSymbol, string> localAliases)
    {
        localAliases = ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default);
        if (operations.IsDefaultOrEmpty)
        {
            return false;
        }

        if (!TryValidateShouldRenderStatementSequence(
                operations,
                ShouldRenderStatementScope.MethodBody,
                assignedDelegateInitializers,
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
                            if (!TryAddShouldRenderLocalAlias(declarator.Symbol, reservedNames, aliases))
                                return false;
                        }
                    }

                    break;

                case IDeclarationExpressionOperation declarationExpression:
                    foreach (var local in CollectShouldRenderDeclaredLocals(declarationExpression.Expression))
                    {
                        if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                            return false;
                    }

                    break;

                case IDeclarationPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                        return false;
                    break;

                case IRecursivePatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                        return false;
                    break;

                case IListPatternOperation { DeclaredSymbol: ILocalSymbol local }:
                    if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                        return false;
                    break;

                case IForEachLoopOperation forEachLoop:
                    foreach (var local in forEachLoop.Locals)
                    {
                        if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                            return false;
                    }

                    break;

                case ICatchClauseOperation catchClause:
                    if (!TryAddShouldRenderCatchBindingAliases(catchClause, reservedNames, aliases))
                        return false;

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

    private static bool ValidateShouldRenderDelegateLocalUsage(
        ImmutableArray<IOperation> operations,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        var delegateLocals = ImmutableDictionary.CreateBuilder<ILocalSymbol, ShouldRenderDelegateLocalUsageKind>(SymbolEqualityComparer.Default);
        foreach (var operation in EnumerateShouldRenderMethodScopedOperations(operations))
        {
            if (RazorVueOperationNormalizer.Unwrap(operation) is IVariableDeclaratorOperation declarator &&
                declarator.Symbol.Type.TypeKind == TypeKind.Delegate)
            {
                if (assignedDelegateInitializers.TryGetValue(declarator.Symbol, out var assignedUsageKind) &&
                    declarator.Initializer is null)
                {
                    delegateLocals[declarator.Symbol] = assignedUsageKind;
                    continue;
                }

                if (TryGetShouldRenderLocalAnonymousFunctionInitializer(declarator, out _) ||
                    TryGetShouldRenderLocalMethodGroupInitializer(declarator, out _, out _))
                {
                    delegateLocals[declarator.Symbol] = ShouldRenderDelegateLocalUsageKind.Callable;
                    continue;
                }

                if (TryGetShouldRenderLocalFunctionDelegateIdentityInitializer(declarator, out var identityInvocation) &&
                    TryGetShouldRenderLocalFunctionDelegateIdentityInitializerUsageKind(
                        identityInvocation,
                        delegateLocals.ToImmutable(),
                        EmptyShouldRenderDelegateParameterUsages,
                        out var identityUsageKind))
                {
                    delegateLocals[declarator.Symbol] = identityUsageKind;
                    continue;
                }

                if (TryGetShouldRenderLocalFunctionDelegateFactoryInitializer(declarator, out var factoryInvocation) &&
                    CanLowerShouldRenderLocalFunctionDelegateFactoryInitializer(factoryInvocation))
                {
                    delegateLocals[declarator.Symbol] = ShouldRenderDelegateLocalUsageKind.Callable;
                    continue;
                }

                if (IsNullConstantOperation(declarator.Initializer?.Value))
                {
                    delegateLocals[declarator.Symbol] = ShouldRenderDelegateLocalUsageKind.NullOnly;
                    continue;
                }

                return false;
            }
        }

        return delegateLocals.Count == 0 ||
            ValidateShouldRenderDelegateUsage(
                operations,
                delegateLocals.ToImmutable(),
                EmptyShouldRenderDelegateParameterUsages,
                assignedDelegateInitializers);
    }

    private static bool ValidateShouldRenderDelegateUsage(
        ImmutableArray<IOperation> operations,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        foreach (var operation in operations)
        {
            if (!ValidateShouldRenderDelegateUsage(operation, delegateLocals, delegateParameters, assignedDelegateInitializers))
                return false;
        }

        return true;
    }

    private static bool ValidateShouldRenderDelegateUsage(
        IOperation operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return true;

        switch (current)
        {
            case IBinaryOperation binaryOperation
                when IsShouldRenderTrackedDelegateNullComparison(binaryOperation, delegateLocals, delegateParameters):
                return true;

            case IBinaryOperation binaryOperation
                when IsShouldRenderTrackedDelegateComparison(binaryOperation, delegateLocals, delegateParameters):
                return true;

            case IIsPatternOperation isPatternOperation
                when IsShouldRenderTrackedDelegateNullPattern(isPatternOperation, delegateLocals, delegateParameters):
                return true;

            case ISimpleAssignmentOperation assignment
                when TryValidateShouldRenderTrackedDelegateAssignment(assignment, assignedDelegateInitializers):
                return true;

            case IInvocationOperation invocation
                when TryValidateShouldRenderTrackedDelegateLocalFunctionInvocation(
                    invocation,
                    delegateLocals,
                    delegateParameters,
                    assignedDelegateInitializers):
                return true;

            case IInvocationOperation invocation
                when invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke &&
                     TryValidateShouldRenderTrackedDelegateInvocationInstance(
                         invocation.Instance,
                         delegateLocals,
                         delegateParameters,
                         assignedDelegateInitializers):
                foreach (var argument in invocation.Arguments)
                {
                    if (!ValidateShouldRenderDelegateUsage(argument, delegateLocals, delegateParameters, assignedDelegateInitializers))
                        return false;
                }

                return true;

            case ILocalReferenceOperation localReference
                when delegateLocals.ContainsKey(localReference.Local):
                return false;

            case IParameterReferenceOperation parameterReference
                when delegateParameters.ContainsKey(parameterReference.Parameter):
                return false;

            default:
                foreach (var child in current.ChildOperations)
                {
                    if (child is null)
                        continue;

                    if (!ValidateShouldRenderDelegateUsage(child, delegateLocals, delegateParameters, assignedDelegateInitializers))
                        return false;
                }

                return true;
        }
    }

    private static bool TryValidateShouldRenderTrackedDelegateAssignment(
        ISimpleAssignmentOperation assignment,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
        => TryGetShouldRenderDelegateInitializerAssignment(assignment, out var local, out var usageKind) &&
           assignedDelegateInitializers.TryGetValue(local, out var expectedUsageKind) &&
           usageKind == expectedUsageKind;

    private static bool TryValidateShouldRenderTrackedDelegateLocalFunctionInvocation(
        IInvocationOperation invocation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        if (invocation.Instance is not null ||
            invocation.TargetMethod.MethodKind != MethodKind.LocalFunction)
        {
            return false;
        }

        if (!TryGetShouldRenderLocalFunctionOperation(
                invocation.TargetMethod,
                invocation.SemanticModel,
                out var localFunctionOperation) ||
            localFunctionOperation.Body is not { } localFunctionBody ||
            !CanLowerShouldRenderLocalFunction(
                localFunctionOperation,
                CreateShouldRenderChildReservedLocalNames()))
        {
            return false;
        }

        var hasTrackedDelegateArgument = false;
        var delegateParameterUsages = ImmutableDictionary.CreateBuilder<IParameterSymbol, ShouldRenderDelegateLocalUsageKind>(SymbolEqualityComparer.Default);
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { } parameter ||
                argument.ArgumentKind != ArgumentKind.Explicit ||
                parameter.RefKind != RefKind.None)
            {
                return false;
            }

            if (parameter.Type.TypeKind == TypeKind.Delegate)
            {
                if (!TryGetShouldRenderTrackedDelegateValueUsageKind(
                        argument.Value,
                        delegateLocals,
                        delegateParameters,
                        out var usageKind))
                {
                    return false;
                }

                delegateParameterUsages[parameter] = usageKind;
                hasTrackedDelegateArgument = true;
                continue;
            }

            if (!ValidateShouldRenderDelegateUsage(argument, delegateLocals, delegateParameters, assignedDelegateInitializers))
                return false;
        }

        if (!hasTrackedDelegateArgument)
            return false;

        if (TryGetShouldRenderDelegateParameterIdentityReturnParameter(
                localFunctionOperation,
                out var identityParameter))
        {
            return delegateParameterUsages.ContainsKey(identityParameter);
        }

        return ValidateShouldRenderDelegateUsage(
                localFunctionBody.Operations,
                EmptyShouldRenderDelegateLocalUsages,
                delegateParameterUsages.ToImmutable(),
                EmptyShouldRenderDelegateLocalUsages);
    }

    private static bool TryValidateShouldRenderTrackedDelegateInvocationInstance(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> assignedDelegateInitializers)
    {
        if (TryGetShouldRenderTrackedDelegateReference(operation, delegateLocals, delegateParameters, out var usageKind))
            return usageKind == ShouldRenderDelegateLocalUsageKind.Callable;

        if (!TryGetShouldRenderLocalFunctionDelegateIdentityInitializerUsageKind(
                operation,
                delegateLocals,
                delegateParameters,
                out usageKind) ||
            usageKind != ShouldRenderDelegateLocalUsageKind.Callable)
        {
            return false;
        }

        return ValidateShouldRenderDelegateUsage(
            operation!,
            delegateLocals,
            delegateParameters,
            assignedDelegateInitializers);
    }

    private static bool TryGetShouldRenderTrackedDelegateValueUsageKind(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        out ShouldRenderDelegateLocalUsageKind usageKind)
        => TryGetShouldRenderTrackedDelegateReference(operation, delegateLocals, delegateParameters, out usageKind) ||
           TryGetShouldRenderLocalFunctionDelegateIdentityInitializerUsageKind(
               operation,
               delegateLocals,
               delegateParameters,
               out usageKind);

    private static bool IsShouldRenderTrackedDelegateNullComparison(
        IBinaryOperation operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters)
    {
        if (operation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return false;

        return
            (IsShouldRenderTrackedDelegateReference(operation.LeftOperand, delegateLocals, delegateParameters) &&
             IsNullConstantOperation(operation.RightOperand)) ||
            (IsShouldRenderTrackedDelegateReference(operation.RightOperand, delegateLocals, delegateParameters) &&
             IsNullConstantOperation(operation.LeftOperand));
    }

    private static bool IsShouldRenderTrackedDelegateComparison(
        IBinaryOperation operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters)
    {
        if (operation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return false;

        return IsShouldRenderTrackedDelegateReference(operation.LeftOperand, delegateLocals, delegateParameters) &&
               IsShouldRenderTrackedDelegateReference(operation.RightOperand, delegateLocals, delegateParameters);
    }

    private static bool IsShouldRenderTrackedDelegateNullPattern(
        IIsPatternOperation operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters)
        => IsShouldRenderTrackedDelegateReference(operation.Value, delegateLocals, delegateParameters) &&
           IsNullConstantPattern(operation.Pattern);

    private static bool IsNullConstantPattern(IPatternOperation pattern)
    {
        switch (RazorVueOperationNormalizer.Unwrap(pattern))
        {
            case IConstantPatternOperation constantPattern:
                return IsNullConstantOperation(constantPattern.Value);
            case INegatedPatternOperation negatedPattern:
                return IsNullConstantPattern(negatedPattern.Pattern);
            default:
                return false;
        }
    }

    private static bool IsNullConstantOperation(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        return current?.ConstantValue.HasValue == true &&
               current.ConstantValue.Value is null;
    }

    private static bool IsShouldRenderTrackedDelegateReference(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters)
        => TryGetShouldRenderTrackedDelegateReference(operation, delegateLocals, delegateParameters, out _);

    private static bool TryGetShouldRenderTrackedDelegateReference(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        out ShouldRenderDelegateLocalUsageKind usageKind)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case ILocalReferenceOperation localReference
                when delegateLocals.TryGetValue(localReference.Local, out usageKind):
                return true;

            case IParameterReferenceOperation parameterReference
                when delegateParameters.TryGetValue(parameterReference.Parameter, out usageKind):
                return true;
        }

        usageKind = default;
        return false;
    }

    private enum ShouldRenderDelegateLocalUsageKind
    {
        Callable,
        NullOnly
    }

    private static readonly ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> EmptyShouldRenderDelegateLocalUsages =
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind>.Empty.WithComparers(SymbolEqualityComparer.Default);

    private static readonly ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> EmptyShouldRenderDelegateParameterUsages =
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind>.Empty.WithComparers(SymbolEqualityComparer.Default);

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
            case IAnonymousFunctionOperation:
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

            case ITryOperation tryOperation:
                foreach (var child in EnumerateShouldRenderMethodScopedOperations(tryOperation.Body))
                    yield return child;

                foreach (var catchClause in tryOperation.Catches)
                {
                    foreach (var child in EnumerateShouldRenderMethodScopedOperations(catchClause))
                        yield return child;
                }

                if (tryOperation.Finally is not null)
                {
                    foreach (var child in EnumerateShouldRenderMethodScopedOperations(tryOperation.Finally))
                        yield return child;
                }

                yield break;

            case ICatchClauseOperation catchClause:
                if (catchClause.ExceptionDeclarationOrExpression is not null)
                {
                    foreach (var child in EnumerateShouldRenderExpressionScopedOperations(catchClause.ExceptionDeclarationOrExpression))
                        yield return child;
                }

                if (catchClause.Filter is not null)
                {
                    foreach (var child in EnumerateShouldRenderExpressionScopedOperations(catchClause.Filter))
                        yield return child;
                }

                foreach (var child in EnumerateShouldRenderMethodScopedOperations(catchClause.Handler))
                    yield return child;

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

    private static bool ContainsShouldRenderNormalReturn(ImmutableArray<IOperation> operations)
        => operations.Any(ContainsShouldRenderNormalReturn);

    private static bool ContainsShouldRenderNormalReturn(IOperation operation)
    {
        foreach (var current in EnumerateShouldRenderMethodScopedOperations(operation))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is IReturnOperation { ReturnedValue: not null })
                return true;
        }

        return false;
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

    private static bool TryGetShouldRenderLocalAnonymousFunctionInitializer(
        IVariableDeclaratorOperation declarator,
        out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate)
            return false;

        return TryGetShouldRenderAnonymousFunction(declarator.Initializer?.Value, out anonymousFunction);
    }

    private static bool TryGetShouldRenderLocalMethodGroupInitializer(
        IVariableDeclaratorOperation declarator,
        out IMethodReferenceOperation methodReference,
        out SemanticModel? semanticModel)
    {
        methodReference = default!;
        semanticModel = null;
        if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate)
            return false;

        if (!TryGetShouldRenderMethodGroup(declarator.Initializer?.Value, out methodReference))
            return false;

        semanticModel = declarator.SemanticModel ?? methodReference.SemanticModel;
        return true;
    }

    private static bool TryGetShouldRenderLocalFunctionDelegateFactoryInitializer(
        IVariableDeclaratorOperation declarator,
        out IInvocationOperation invocation)
    {
        invocation = default!;
        if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate)
            return false;

        return TryGetShouldRenderLocalFunctionInvocation(declarator.Initializer?.Value, out invocation) &&
               invocation.TargetMethod.ReturnType.TypeKind == TypeKind.Delegate;
    }

    private static bool TryGetShouldRenderLocalFunctionDelegateIdentityInitializer(
        IVariableDeclaratorOperation declarator,
        out IInvocationOperation invocation)
    {
        invocation = default!;
        if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate)
            return false;

        return TryGetShouldRenderLocalFunctionInvocation(declarator.Initializer?.Value, out invocation) &&
               invocation.TargetMethod.ReturnType.TypeKind == TypeKind.Delegate &&
               TryGetShouldRenderLocalFunctionOperation(
                   invocation.TargetMethod,
                   invocation.SemanticModel,
                   out var localFunctionOperation) &&
               TryGetShouldRenderDelegateParameterIdentityReturnParameter(localFunctionOperation, out _);
    }

    private static bool TryGetShouldRenderLocalFunctionInvocation(
        IOperation? operation,
        out IInvocationOperation invocation)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IInvocationOperation directInvocation
                when directInvocation.TargetMethod.MethodKind == MethodKind.LocalFunction:
                invocation = directInvocation;
                return true;

            case IConversionOperation conversion:
                return TryGetShouldRenderLocalFunctionInvocation(conversion.Operand, out invocation);

            default:
                invocation = default!;
                return false;
        }
    }

    private static bool CanLowerShouldRenderLocalFunctionDelegateFactoryInitializer(
        IInvocationOperation invocation)
    {
        if (invocation.Instance is not null ||
            invocation.TargetMethod.MethodKind != MethodKind.LocalFunction ||
            invocation.TargetMethod.ReturnType.TypeKind != TypeKind.Delegate)
        {
            return false;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { RefKind: RefKind.None } ||
                argument.ArgumentKind != ArgumentKind.Explicit ||
                ContainsShouldRenderUnsupportedExpressionConstruct(argument.Value))
            {
                return false;
            }
        }

        return TryGetShouldRenderLocalFunctionOperation(
                invocation.TargetMethod,
                invocation.SemanticModel,
                out var localFunctionOperation) &&
            CanLowerShouldRenderLocalFunction(
                localFunctionOperation,
                CreateShouldRenderChildReservedLocalNames());
    }

    private static bool CanLowerShouldRenderLocalFunctionDelegateIdentityInitializer(
        IInvocationOperation invocation)
    {
        if (invocation.Instance is not null ||
            invocation.TargetMethod.MethodKind != MethodKind.LocalFunction ||
            invocation.TargetMethod.ReturnType.TypeKind != TypeKind.Delegate)
        {
            return false;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { RefKind: RefKind.None } ||
                argument.ArgumentKind != ArgumentKind.Explicit ||
                ContainsShouldRenderUnsupportedExpressionConstruct(argument.Value))
            {
                return false;
            }
        }

        return TryGetShouldRenderLocalFunctionOperation(
                invocation.TargetMethod,
                invocation.SemanticModel,
                out var localFunctionOperation) &&
            TryGetShouldRenderDelegateParameterIdentityReturnParameter(localFunctionOperation, out _) &&
            CanLowerShouldRenderLocalFunction(
                localFunctionOperation,
                CreateShouldRenderChildReservedLocalNames());
    }

    private static bool TryGetShouldRenderLocalFunctionDelegateIdentityInitializerUsageKind(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, ShouldRenderDelegateLocalUsageKind> delegateLocals,
        ImmutableDictionary<IParameterSymbol, ShouldRenderDelegateLocalUsageKind> delegateParameters,
        out ShouldRenderDelegateLocalUsageKind usageKind)
    {
        usageKind = default;
        if (!TryGetShouldRenderLocalFunctionInvocation(operation, out var invocation) ||
            invocation.TargetMethod.ReturnType.TypeKind != TypeKind.Delegate ||
            !TryGetShouldRenderLocalFunctionOperation(
                invocation.TargetMethod,
                invocation.SemanticModel,
                out var localFunctionOperation) ||
            !TryGetShouldRenderDelegateParameterIdentityReturnParameter(localFunctionOperation, out var identityParameter))
        {
            return false;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { } parameter ||
                !SymbolEqualityComparer.Default.Equals(parameter, identityParameter))
            {
                continue;
            }

            return TryGetShouldRenderTrackedDelegateValueUsageKind(
                argument.Value,
                delegateLocals,
                delegateParameters,
                out usageKind);
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

    private static bool TryAddShouldRenderLocalAlias(
        ILocalSymbol local,
        HashSet<string> reservedNames,
        ImmutableDictionary<ILocalSymbol, string>.Builder aliases)
    {
        if (aliases.ContainsKey(local))
            return true;

        if (!TryCreateShouldRenderLocalAlias(local, reservedNames, out var alias))
            return false;

        aliases[local] = alias;
        return true;
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

        if (method.ReturnType.TypeKind == TypeKind.Delegate &&
            !CanLowerShouldRenderDelegateReturningLocalFunction(operation))
        {
            return false;
        }

        return TryReserveShouldRenderLocalFunctionBodyBindings(operation, functionScopeNames) &&
            ValidateShouldRenderDelegateParameterUsage(operation);
    }

    private static bool CanLowerShouldRenderDelegateReturningLocalFunction(
        ILocalFunctionOperation operation)
    {
        if (operation.Symbol.ReturnType.TypeKind != TypeKind.Delegate ||
            operation.Body is null ||
            operation.Body.Operations.IsDefaultOrEmpty)
        {
            return false;
        }

        if (TryGetShouldRenderDelegateParameterIdentityReturnParameter(operation, out _))
            return true;

        return TryValidateShouldRenderDelegateFactoryStatementSequence(
                operation.Body.Operations,
                out var alwaysReturns,
                out var hasDelegateReturn) &&
            alwaysReturns &&
            hasDelegateReturn;
    }

    private static bool TryGetShouldRenderDelegateParameterIdentityReturnParameter(
        ILocalFunctionOperation operation,
        out IParameterSymbol parameter)
    {
        parameter = default!;
        if (operation.Symbol.ReturnType.TypeKind != TypeKind.Delegate ||
            operation.Body is null ||
            operation.Body.Operations.IsDefaultOrEmpty)
        {
            return false;
        }

        var aliasSources = ImmutableDictionary.CreateBuilder<ILocalSymbol, IParameterSymbol>(SymbolEqualityComparer.Default);
        var pendingDelegateLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        IParameterSymbol? identityParameter = null;
        var foundReturn = false;

        foreach (var statement in operation.Body.Operations)
        {
            if (foundReturn)
                return false;

            if (!TryProcessShouldRenderDelegateIdentityReturnStatement(
                    statement,
                    aliasSources,
                    pendingDelegateLocals,
                    ref identityParameter,
                    out var statementReturns))
            {
                return false;
            }

            if (statementReturns)
                foundReturn = true;
        }

        if (!foundReturn ||
            identityParameter is null ||
            pendingDelegateLocals.Count != 0)
        {
            return false;
        }

        parameter = identityParameter;
        return true;
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnStatement(
        IOperation operation,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        ref IParameterSymbol? identityParameter,
        out bool statementReturns)
    {
        statementReturns = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IBlockOperation block:
                return TryProcessShouldRenderDelegateIdentityReturnBlock(
                    block,
                    aliasSources,
                    pendingDelegateLocals,
                    ref identityParameter,
                    out statementReturns);

            case IVariableDeclarationGroupOperation declarationGroup:
                return TryProcessShouldRenderDelegateIdentityReturnDeclarations(
                    declarationGroup,
                    aliasSources,
                    pendingDelegateLocals,
                    ref identityParameter);

            case IExpressionStatementOperation expressionStatement:
                if (TryGetShouldRenderDelegateIdentityAssignment(
                        expressionStatement,
                        aliasSources,
                        out var targetLocal,
                        out var sourceParameter))
                {
                    if (!pendingDelegateLocals.Contains(targetLocal) &&
                        !aliasSources.ContainsKey(targetLocal))
                    {
                        return false;
                    }

                    if (identityParameter is null)
                    {
                        identityParameter = sourceParameter;
                    }
                    else if (!SymbolEqualityComparer.Default.Equals(identityParameter, sourceParameter))
                    {
                        return false;
                    }

                    aliasSources[targetLocal] = sourceParameter;
                    pendingDelegateLocals.Remove(targetLocal);
                    return true;
                }

                return !ContainsShouldRenderDelegateIdentityEscapeReference(current);

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                if (ContainsShouldRenderDelegateIdentityEscapeReference(conditional.Condition))
                    return false;

                return TryProcessShouldRenderDelegateIdentityReturnBranch(
                    conditional.WhenTrue,
                    aliasSources,
                    pendingDelegateLocals,
                    identityParameter,
                    out var trueIdentityParameter,
                    out var trueReturns) &&
                    TryProcessShouldRenderDelegateIdentityReturnBranch(
                        conditional.WhenFalse,
                        aliasSources,
                        pendingDelegateLocals,
                        identityParameter,
                        out var falseIdentityParameter,
                        out var falseReturns) &&
                    TryMergeShouldRenderDelegateIdentityReturnBranches(
                        trueIdentityParameter,
                        trueReturns,
                        falseIdentityParameter,
                        falseReturns,
                        ref identityParameter,
                        out statementReturns);

            case ISwitchOperation switchOperation when switchOperation.Syntax is SwitchStatementSyntax:
                return TryProcessShouldRenderDelegateIdentityReturnSwitchStatement(
                    switchOperation,
                    aliasSources,
                    pendingDelegateLocals,
                    ref identityParameter,
                    out statementReturns);

            case ITryOperation tryOperation:
                return TryProcessShouldRenderDelegateIdentityReturnTryStatement(
                    tryOperation,
                    aliasSources,
                    pendingDelegateLocals,
                    ref identityParameter,
                    out statementReturns);

            case IReturnOperation { ReturnedValue: not null } returnOperation:
                if (!TryGetShouldRenderDelegateIdentitySource(
                        returnOperation.ReturnedValue,
                        aliasSources,
                        identityParameter,
                        out var returnSourceParameter))
                {
                    return false;
                }

                if (identityParameter is null)
                {
                    identityParameter = returnSourceParameter;
                }
                else if (!SymbolEqualityComparer.Default.Equals(identityParameter, returnSourceParameter))
                {
                    return false;
                }

                statementReturns = true;
                return true;

            default:
                return !ContainsShouldRenderDelegateIdentityEscapeReference(current);
        }
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnSwitchStatement(
        ISwitchOperation switchOperation,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        ref IParameterSymbol? identityParameter,
        out bool statementReturns)
    {
        statementReturns = false;
        if (ContainsShouldRenderUnsupportedExpressionConstruct(switchOperation.Value) ||
            ContainsShouldRenderDelegateIdentityEscapeReference(switchOperation.Value))
        {
            return false;
        }

        var hasDefaultCase = false;
        var allCasesReturn = true;
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
                        if (ContainsShouldRenderUnsupportedExpressionConstruct(singleValueClause.Value) ||
                            ContainsShouldRenderDelegateIdentityEscapeReference(singleValueClause.Value))
                        {
                            return false;
                        }

                        break;

                    case IPatternCaseClauseOperation patternClause:
                        if (ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Pattern) ||
                            ContainsShouldRenderUnsupportedExpressionConstruct(patternClause.Guard) ||
                            ContainsShouldRenderDelegateIdentityEscapeReference(patternClause.Pattern) ||
                            ContainsShouldRenderDelegateIdentityEscapeReference(patternClause.Guard))
                        {
                            return false;
                        }

                        break;

                    default:
                        return false;
                }
            }

            if (!TryProcessShouldRenderDelegateIdentityReturnStatementSequence(
                    switchCase.Body,
                    aliasSources,
                    pendingDelegateLocals,
                    identityParameter,
                    out var caseIdentityParameter,
                    out var caseReturns))
            {
                return false;
            }

            if (caseReturns &&
                !TryMergeShouldRenderDelegateIdentityReturnBranchSource(
                    caseIdentityParameter,
                    ref identityParameter))
            {
                return false;
            }

            allCasesReturn &= caseReturns;
        }

        statementReturns = hasDefaultCase && allCasesReturn;
        return true;
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnTryStatement(
        ITryOperation tryOperation,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        ref IParameterSymbol? identityParameter,
        out bool statementReturns)
    {
        statementReturns = false;
        if (!TryProcessShouldRenderDelegateIdentityReturnBranch(
                tryOperation.Body,
                aliasSources,
                pendingDelegateLocals,
                identityParameter,
                out var tryIdentityParameter,
                out var tryReturns) ||
            !tryReturns ||
            !TryMergeShouldRenderDelegateIdentityReturnBranchSource(
                tryIdentityParameter,
                ref identityParameter))
        {
            return false;
        }

        foreach (var catchClause in tryOperation.Catches)
        {
            if (!TryValidateShouldRenderCatchBinding(catchClause) ||
                ContainsShouldRenderUnsupportedExpressionConstruct(catchClause.Filter) ||
                ContainsShouldRenderDelegateIdentityEscapeReference(catchClause.ExceptionDeclarationOrExpression) ||
                ContainsShouldRenderDelegateIdentityEscapeReference(catchClause.Filter))
            {
                return false;
            }

            if (!TryProcessShouldRenderDelegateIdentityReturnBranch(
                    catchClause.Handler,
                    aliasSources,
                    pendingDelegateLocals,
                    identityParameter,
                    out var catchIdentityParameter,
                    out var catchReturns) ||
                !catchReturns ||
                !TryMergeShouldRenderDelegateIdentityReturnBranchSource(
                    catchIdentityParameter,
                    ref identityParameter))
            {
                return false;
            }
        }

        if (tryOperation.Finally is not null)
        {
            var finallyIdentityParameter = identityParameter;
            if (!TryProcessShouldRenderDelegateIdentityReturnBranch(
                    tryOperation.Finally,
                    aliasSources,
                    pendingDelegateLocals,
                    identityParameter,
                    out _,
                    out var finallyReturns) ||
                finallyReturns ||
                ContainsShouldRenderDelegateIdentityEscapeReference(tryOperation.Finally))
            {
                return false;
            }

            identityParameter = finallyIdentityParameter;
        }

        statementReturns = true;
        return true;
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnBlock(
        IBlockOperation block,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        ref IParameterSymbol? identityParameter,
        out bool blockReturns)
    {
        blockReturns = false;
        if (!TryProcessShouldRenderDelegateIdentityReturnStatementSequence(
                block.Operations,
                aliasSources,
                pendingDelegateLocals,
                identityParameter,
                out var blockIdentityParameter,
                out blockReturns))
        {
            return false;
        }

        if (!blockReturns)
            return !ContainsShouldRenderDelegateIdentityEscapeReference(block);

        return TryMergeShouldRenderDelegateIdentityReturnBranchSource(
            blockIdentityParameter,
            ref identityParameter);
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnBranch(
        IOperation? operation,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        IParameterSymbol? identityParameter,
        out IParameterSymbol? branchIdentityParameter,
        out bool branchReturns)
    {
        branchIdentityParameter = identityParameter;
        branchReturns = false;
        if (operation is null)
            return true;

        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IBlockOperation block:
                return TryProcessShouldRenderDelegateIdentityReturnStatementSequence(
                    block.Operations,
                    aliasSources,
                    pendingDelegateLocals,
                    identityParameter,
                    out branchIdentityParameter,
                    out branchReturns);

            default:
                var branchAliasSources = aliasSources.ToImmutable().ToBuilder();
                var branchPendingDelegateLocals = new HashSet<ILocalSymbol>(pendingDelegateLocals, SymbolEqualityComparer.Default);
                return TryProcessShouldRenderDelegateIdentityReturnStatement(
                    operation,
                    branchAliasSources,
                    branchPendingDelegateLocals,
                    ref branchIdentityParameter,
                    out branchReturns) &&
                    (branchReturns || branchPendingDelegateLocals.SetEquals(pendingDelegateLocals));
        }
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnStatementSequence(
        ImmutableArray<IOperation> operations,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        IParameterSymbol? identityParameter,
        out IParameterSymbol? sequenceIdentityParameter,
        out bool sequenceReturns)
    {
        sequenceIdentityParameter = identityParameter;
        sequenceReturns = false;
        var sequenceAliasSources = aliasSources.ToImmutable().ToBuilder();
        var sequencePendingDelegateLocals = new HashSet<ILocalSymbol>(pendingDelegateLocals, SymbolEqualityComparer.Default);

        foreach (var statement in operations)
        {
            if (sequenceReturns)
                return false;

            if (!TryProcessShouldRenderDelegateIdentityReturnStatement(
                    statement,
                    sequenceAliasSources,
                    sequencePendingDelegateLocals,
                    ref sequenceIdentityParameter,
                    out var statementReturns))
            {
                return false;
            }

            if (statementReturns)
                sequenceReturns = true;
        }

        return sequenceReturns || sequencePendingDelegateLocals.SetEquals(pendingDelegateLocals);
    }

    private static bool TryMergeShouldRenderDelegateIdentityReturnBranches(
        IParameterSymbol? trueIdentityParameter,
        bool trueReturns,
        IParameterSymbol? falseIdentityParameter,
        bool falseReturns,
        ref IParameterSymbol? identityParameter,
        out bool statementReturns)
    {
        statementReturns = trueReturns && falseReturns;

        if (trueReturns &&
            !TryMergeShouldRenderDelegateIdentityReturnBranchSource(trueIdentityParameter, ref identityParameter))
        {
            return false;
        }

        if (falseReturns &&
            !TryMergeShouldRenderDelegateIdentityReturnBranchSource(falseIdentityParameter, ref identityParameter))
        {
            return false;
        }

        return true;
    }

    private static bool TryMergeShouldRenderDelegateIdentityReturnBranchSource(
        IParameterSymbol? branchIdentityParameter,
        ref IParameterSymbol? identityParameter)
    {
        if (branchIdentityParameter is null)
            return false;

        if (identityParameter is null)
        {
            identityParameter = branchIdentityParameter;
            return true;
        }

        return SymbolEqualityComparer.Default.Equals(identityParameter, branchIdentityParameter);
    }

    private static bool TryProcessShouldRenderDelegateIdentityReturnDeclarations(
        IVariableDeclarationGroupOperation declarationGroup,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        HashSet<ILocalSymbol> pendingDelegateLocals,
        ref IParameterSymbol? identityParameter)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Symbol.Type.TypeKind != TypeKind.Delegate)
                {
                    if (ContainsShouldRenderDelegateIdentityEscapeReference(declarator.Initializer?.Value))
                        return false;

                    continue;
                }

                if (declarator.Initializer is null)
                {
                    pendingDelegateLocals.Add(declarator.Symbol);
                    continue;
                }

                if (!TryGetShouldRenderDelegateIdentitySource(
                        declarator.Initializer.Value,
                        aliasSources,
                        identityParameter,
                        out var sourceParameter))
                {
                    return false;
                }

                if (identityParameter is null)
                {
                    identityParameter = sourceParameter;
                }
                else if (!SymbolEqualityComparer.Default.Equals(identityParameter, sourceParameter))
                {
                    return false;
                }

                aliasSources[declarator.Symbol] = sourceParameter;
                pendingDelegateLocals.Remove(declarator.Symbol);
            }
        }

        return true;
    }

    private static bool TryGetShouldRenderDelegateIdentityAssignment(
        IExpressionStatementOperation expressionStatement,
        ImmutableDictionary<ILocalSymbol, IParameterSymbol>.Builder aliasSources,
        out ILocalSymbol targetLocal,
        out IParameterSymbol sourceParameter)
    {
        targetLocal = default!;
        sourceParameter = default!;

        if (RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation) is not ISimpleAssignmentOperation assignment ||
            RazorVueOperationNormalizer.Unwrap(assignment.Target) is not ILocalReferenceOperation targetReference ||
            targetReference.Local.Type.TypeKind != TypeKind.Delegate ||
            !TryGetShouldRenderDelegateIdentitySource(
                assignment.Value,
                aliasSources,
                null,
                out sourceParameter))
        {
            return false;
        }

        targetLocal = targetReference.Local;
        return true;
    }

    private static bool TryGetShouldRenderDelegateIdentitySource(
        IOperation? operation,
        IReadOnlyDictionary<ILocalSymbol, IParameterSymbol> aliasSources,
        IParameterSymbol? identityParameter,
        out IParameterSymbol sourceParameter)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IParameterReferenceOperation parameterReference
                when parameterReference.Parameter.Type.TypeKind == TypeKind.Delegate:
                if (identityParameter is not null &&
                    !SymbolEqualityComparer.Default.Equals(identityParameter, parameterReference.Parameter))
                {
                    break;
                }

                sourceParameter = parameterReference.Parameter;
                return true;

            case ILocalReferenceOperation localReference
                when localReference.Local.Type.TypeKind == TypeKind.Delegate &&
                     aliasSources.TryGetValue(localReference.Local, out sourceParameter):
                if (identityParameter is not null &&
                    !SymbolEqualityComparer.Default.Equals(identityParameter, sourceParameter))
                {
                    break;
                }

                return true;

            case IConditionalOperation conditional
                when conditional.WhenTrue is not null &&
                     conditional.WhenFalse is not null &&
                     !ContainsShouldRenderDelegateIdentityEscapeReference(conditional.Condition) &&
                     TryGetShouldRenderDelegateIdentitySource(
                         conditional.WhenTrue,
                         aliasSources,
                         identityParameter,
                         out var trueSourceParameter) &&
                     TryGetShouldRenderDelegateIdentitySource(
                         conditional.WhenFalse,
                         aliasSources,
                         identityParameter ?? trueSourceParameter,
                         out var falseSourceParameter) &&
                     SymbolEqualityComparer.Default.Equals(trueSourceParameter, falseSourceParameter):
                sourceParameter = trueSourceParameter;
                return true;
        }

        sourceParameter = default!;
        return false;
    }

    private static bool ContainsShouldRenderDelegateIdentityEscapeReference(IOperation? operation)
    {
        if (operation is null)
            return false;

        foreach (var current in EnumerateShouldRenderExpressionScopedOperations(operation))
        {
            switch (RazorVueOperationNormalizer.Unwrap(current))
            {
                case IParameterReferenceOperation parameterReference
                    when parameterReference.Parameter.Type.TypeKind == TypeKind.Delegate:
                case ILocalReferenceOperation localReference
                    when localReference.Local.Type.TypeKind == TypeKind.Delegate:
                    return true;
            }
        }

        return false;
    }

    private static bool TryValidateShouldRenderDelegateFactoryStatementSequence(
        ImmutableArray<IOperation> operations,
        out bool alwaysReturns,
        out bool hasDelegateReturn)
    {
        alwaysReturns = false;
        hasDelegateReturn = false;
        if (operations.IsDefaultOrEmpty)
            return true;

        for (var index = 0; index < operations.Length; index++)
        {
            if (alwaysReturns)
                return false;

            if (!TryValidateShouldRenderDelegateFactoryStatement(
                    operations[index],
                    out var statementAlwaysReturns,
                    out var statementHasDelegateReturn))
            {
                return false;
            }

            hasDelegateReturn |= statementHasDelegateReturn;
            if (statementAlwaysReturns)
                alwaysReturns = true;
        }

        return true;
    }

    private static bool TryValidateShouldRenderDelegateFactoryStatement(
        IOperation operation,
        out bool alwaysReturns,
        out bool hasDelegateReturn)
    {
        alwaysReturns = false;
        hasDelegateReturn = false;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        switch (current)
        {
            case IBlockOperation block:
                return TryValidateShouldRenderDelegateFactoryStatementSequence(
                    block.Operations,
                    out alwaysReturns,
                    out hasDelegateReturn);

            case IReturnOperation { ReturnedValue: not null } returnOperation:
                if (!TryGetShouldRenderAnonymousFunction(returnOperation.ReturnedValue, out var anonymousFunction) ||
                    !CanLowerShouldRenderLocalAnonymousFunction(anonymousFunction))
                {
                    return false;
                }

                alwaysReturns = true;
                hasDelegateReturn = true;
                return true;

            case IThrowOperation throwOperation:
                if (throwOperation.Exception is null &&
                    throwOperation.Syntax.FirstAncestorOrSelf<CatchClauseSyntax>() is null)
                {
                    return false;
                }

                if (ContainsShouldRenderUnsupportedExpressionConstruct(throwOperation.Exception))
                    return false;

                alwaysReturns = true;
                return true;

            case IVariableDeclarationGroupOperation declarationGroup:
                return TryValidateShouldRenderDelegateFactoryVariableDeclarationGroup(declarationGroup);

            case IExpressionStatementOperation expressionStatement:
                return TryValidateShouldRenderLocalMutationExpressionStatement(expressionStatement);

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                if (ContainsShouldRenderUnsupportedExpressionConstruct(conditional.Condition))
                    return false;

                if (!TryValidateShouldRenderDelegateFactoryStatement(
                        conditional.WhenTrue,
                        out var trueAlwaysReturns,
                        out var trueHasDelegateReturn))
                {
                    return false;
                }

                var falseAlwaysReturns = false;
                var falseHasDelegateReturn = false;
                if (conditional.WhenFalse is not null &&
                    !TryValidateShouldRenderDelegateFactoryStatement(
                        conditional.WhenFalse,
                        out falseAlwaysReturns,
                        out falseHasDelegateReturn))
                {
                    return false;
                }

                alwaysReturns = trueAlwaysReturns && falseAlwaysReturns;
                hasDelegateReturn = trueHasDelegateReturn || falseHasDelegateReturn;
                return true;

            default:
                return false;
        }
    }

    private static bool TryValidateShouldRenderDelegateFactoryVariableDeclarationGroup(
        IVariableDeclarationGroupOperation declarationGroup)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (declarator.Symbol.Type.TypeKind == TypeKind.Delegate ||
                    ContainsShouldRenderUnsupportedExpressionConstruct(declarator))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ValidateShouldRenderDelegateParameterUsage(
        ILocalFunctionOperation operation)
    {
        if (operation.Body is null)
            return false;

        if (TryGetShouldRenderDelegateParameterIdentityReturnParameter(operation, out _))
            return true;

        var delegateParameters = ImmutableDictionary.CreateBuilder<IParameterSymbol, ShouldRenderDelegateLocalUsageKind>(SymbolEqualityComparer.Default);
        foreach (var parameter in operation.Symbol.Parameters)
        {
            if (parameter.Type.TypeKind == TypeKind.Delegate)
                delegateParameters[parameter] = ShouldRenderDelegateLocalUsageKind.Callable;
        }

        return delegateParameters.Count == 0 ||
            ValidateShouldRenderDelegateUsage(
                operation.Body.Operations,
                EmptyShouldRenderDelegateLocalUsages,
                delegateParameters.ToImmutable(),
                EmptyShouldRenderDelegateLocalUsages);
    }

    private static bool CanLowerShouldRenderLocalAnonymousFunction(
        IAnonymousFunctionOperation operation)
    {
        var method = operation.Symbol;
        if (method.IsAsync ||
            method.IsGenericMethod ||
            method.RefKind != RefKind.None ||
            method.ReturnType.SpecialType != SpecialType.System_Boolean)
        {
            return false;
        }

        var functionScopeNames = CreateShouldRenderChildReservedLocalNames();
        foreach (var parameter in method.Parameters)
        {
            if (parameter.RefKind != RefKind.None ||
                !TryReserveShouldRenderBindingIdentifier(parameter.Name, functionScopeNames))
            {
                return false;
            }
        }

        if (operation.Body is null ||
            !TryValidateShouldRenderStatementSequence(
                operation.Body.Operations,
                ShouldRenderStatementScope.BranchBody,
                out var bodyAlwaysReturns,
                out _) ||
            !bodyAlwaysReturns ||
            !ContainsShouldRenderNormalReturn(operation.Body.Operations))
        {
            return false;
        }

        return TryReserveShouldRenderAnonymousFunctionBodyBindings(operation, functionScopeNames);
    }

    private static bool CanLowerShouldRenderMethodGroupDelegateInitializer(
        IMethodReferenceOperation operation,
        SemanticModel? semanticModel)
    {
        var method = operation.Method;
        if (method.MethodKind == MethodKind.LocalFunction)
        {
            if (RazorVueOperationNormalizer.Unwrap(operation.Instance) is not (null or IInstanceReferenceOperation) ||
                method.ReturnType.SpecialType != SpecialType.System_Boolean ||
                method.Parameters.Any(static parameter => parameter.RefKind != RefKind.None))
            {
                return false;
            }

            return
                TryGetShouldRenderLocalFunctionOperation(
                    method,
                    semanticModel,
                    out var localFunctionOperation) &&
                CanLowerShouldRenderLocalFunction(
                    localFunctionOperation,
                    CreateShouldRenderChildReservedLocalNames());
        }

        if (RazorVueOperationNormalizer.Unwrap(operation.Instance) is not IInstanceReferenceOperation ||
            method.MethodKind != MethodKind.Ordinary ||
            method.IsStatic ||
            method.IsAsync ||
            method.IsGenericMethod ||
            method.RefKind != RefKind.None ||
            method.ReturnType.SpecialType != SpecialType.System_Boolean)
        {
            return false;
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.RefKind != RefKind.None)
                return false;
        }

        return TryValidateShouldRenderMethodGroupMethod(method, semanticModel);
    }

    private static bool TryReserveShouldRenderLocalFunctionBodyBindings(
        ILocalFunctionOperation operation,
        HashSet<string> reservedNames)
    {
        foreach (var current in operation.DescendantsAndSelf())
        {
            if (IsInsideShouldRenderDelegateFactoryReturnedAnonymousFunction(current, operation))
                continue;

            switch (RazorVueOperationNormalizer.Unwrap(current))
            {
                case IAnonymousFunctionOperation anonymousFunction:
                    if (!IsShouldRenderDelegateFactoryReturnedAnonymousFunction(operation, anonymousFunction))
                        return false;

                    break;

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
                    foreach (var local in CollectShouldRenderCatchBindingLocals(catchClause))
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

    private static bool IsInsideShouldRenderDelegateFactoryReturnedAnonymousFunction(
        IOperation operation,
        ILocalFunctionOperation localFunction)
    {
        var parent = operation.Parent;
        while (parent is not null)
        {
            if (RazorVueOperationNormalizer.Unwrap(parent) is IAnonymousFunctionOperation anonymousFunction &&
                IsShouldRenderDelegateFactoryReturnedAnonymousFunction(localFunction, anonymousFunction))
            {
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static bool IsShouldRenderDelegateFactoryReturnedAnonymousFunction(
        ILocalFunctionOperation localFunction,
        IAnonymousFunctionOperation anonymousFunction)
    {
        if (localFunction.Symbol.ReturnType.TypeKind != TypeKind.Delegate ||
            localFunction.Body is null)
        {
            return false;
        }

        foreach (var current in EnumerateShouldRenderMethodScopedOperations(localFunction.Body.Operations))
        {
            if (RazorVueOperationNormalizer.Unwrap(current) is IReturnOperation { ReturnedValue: not null } returnOperation &&
                TryGetShouldRenderAnonymousFunction(returnOperation.ReturnedValue, out var returnedAnonymousFunction) &&
                ReferenceEquals(returnedAnonymousFunction, anonymousFunction))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryReserveShouldRenderAnonymousFunctionBodyBindings(
        IAnonymousFunctionOperation operation,
        HashSet<string> reservedNames)
    {
        foreach (var current in operation.Body.DescendantsAndSelf())
        {
            switch (RazorVueOperationNormalizer.Unwrap(current))
            {
                case IAnonymousFunctionOperation anonymousFunction when !ReferenceEquals(anonymousFunction, operation):
                    return false;

                case ILocalFunctionOperation:
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
                    foreach (var local in CollectShouldRenderCatchBindingLocals(catchClause))
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

    private static bool TryGetShouldRenderAnonymousFunction(
        IOperation? operation,
        out IAnonymousFunctionOperation anonymousFunction)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IAnonymousFunctionOperation directAnonymousFunction:
                anonymousFunction = directAnonymousFunction;
                return true;
            case IDelegateCreationOperation delegateCreation:
                return TryGetShouldRenderAnonymousFunction(delegateCreation.Target, out anonymousFunction);
            default:
                anonymousFunction = default!;
                return false;
        }
    }

    private static bool TryGetShouldRenderMethodGroup(
        IOperation? operation,
        out IMethodReferenceOperation methodReference)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IMethodReferenceOperation directMethodReference:
                methodReference = directMethodReference;
                return true;
            case IDelegateCreationOperation delegateCreation:
                return TryGetShouldRenderMethodGroup(delegateCreation.Target, out methodReference);
            case IConversionOperation conversion:
                return TryGetShouldRenderMethodGroup(conversion.Operand, out methodReference);
            default:
                methodReference = default!;
                return false;
        }
    }

    private static bool TryGetShouldRenderLocalFunctionOperation(
        IMethodSymbol localFunction,
        SemanticModel? fallbackModel,
        out ILocalFunctionOperation localFunctionOperation)
    {
        localFunctionOperation = default!;
        var syntaxReference = localFunction.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference?.GetSyntax() is not LocalFunctionStatementSyntax localFunctionSyntax)
            return false;

        var semanticModel = fallbackModel?.Compilation.GetSemanticModel(localFunctionSyntax.SyntaxTree);
        if (semanticModel is null)
            return false;

        if (semanticModel.GetOperation(localFunctionSyntax) is not ILocalFunctionOperation operation)
            return false;

        localFunctionOperation = operation;
        return true;
    }

    private static bool TryValidateShouldRenderMethodGroupMethod(
        IMethodSymbol method,
        SemanticModel? fallbackModel)
    {
        var syntaxReference = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference?.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        var semanticModel = fallbackModel?.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
        if (semanticModel is null)
            return false;

        if (methodSyntax.Body is { } body)
        {
            if (semanticModel.GetOperation(body) is not IBlockOperation blockOperation)
                return false;

            return !blockOperation.Operations.IsDefaultOrEmpty &&
                !ContainsShouldRenderAnonymousFunction(blockOperation.Operations) &&
                TryValidateShouldRenderStatementSequence(
                    blockOperation.Operations,
                    ShouldRenderStatementScope.BranchBody,
                    out var alwaysReturns,
                    out _) &&
                alwaysReturns &&
                ContainsShouldRenderNormalReturn(blockOperation.Operations);
        }

        if (methodSyntax.ExpressionBody?.Expression is { } expressionSyntax &&
            semanticModel.GetOperation(expressionSyntax) is { } expressionOperation)
        {
            return !ContainsShouldRenderUnsupportedExpressionConstruct(expressionOperation);
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

    private static bool TryAddShouldRenderCatchBindingAliases(
        ICatchClauseOperation catchClause,
        HashSet<string> reservedNames,
        ImmutableDictionary<ILocalSymbol, string>.Builder aliases)
    {
        var added = false;
        foreach (var local in CollectShouldRenderCatchBindingLocals(catchClause))
        {
            if (!TryAddShouldRenderLocalAlias(local, reservedNames, aliases))
                return false;

            added = true;
        }

        return added || catchClause.Syntax is not CatchClauseSyntax { Declaration.Identifier.ValueText.Length: > 0 };
    }

    private static IEnumerable<ILocalSymbol> CollectShouldRenderCatchBindingLocals(ICatchClauseOperation catchClause)
    {
        foreach (var local in CollectShouldRenderDeclaredLocals(catchClause.ExceptionDeclarationOrExpression))
            yield return local;

        if (catchClause.Syntax is CatchClauseSyntax { Declaration: not null } catchSyntax &&
            catchClause.SemanticModel?.GetDeclaredSymbol(catchSyntax.Declaration) is ILocalSymbol syntaxLocal)
        {
            yield return syntaxLocal;
        }
    }

    private static bool TryValidateShouldRenderCatchBinding(ICatchClauseOperation catchClause)
    {
        return catchClause.Syntax is not CatchClauseSyntax { Declaration.Identifier.ValueText.Length: > 0 } ||
            CollectShouldRenderCatchBindingLocals(catchClause).Any();
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
            RazorVueExpressionEmitter.ShouldRenderSwitchFallthroughAlias,
            RazorVueExpressionEmitter.ShouldRenderSwitchResultAlias,
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
            or RazorVueExpressionEmitter.ShouldRenderSwitchFallthroughAlias
            or RazorVueExpressionEmitter.ShouldRenderSwitchResultAlias
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
                .Any(IsParameterProperty))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.ParameterAttribute",
                StringComparison.Ordinal));

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

    private static string DescribeLifecycleStatementShape(SupportedLifecycleStatement statement)
        => statement switch
        {
            SupportedLifecycleEmitStatement emit => DescribeEmitCallShape(emit.EmitCall),
            SupportedLifecycleIfStatement conditional =>
                "if|" + conditional.ConditionExpression + "|then:" +
                DescribeLifecycleStatementSequenceShape(conditional.WhenTrue) +
                "|else:" +
                DescribeLifecycleStatementSequenceShape(conditional.WhenFalse),
            SupportedLifecycleGuardReturnStatement guardReturn =>
                "guard-return|" +
                DescribeLifecyclePreludeBindingsShape(guardReturn.ConditionPreludeBindings) +
                "|" +
                guardReturn.ConditionExpression,
            SupportedLifecycleReturnStatement returnStatement =>
                "return|prelude:" +
                DescribeLifecyclePreludeBindingsShape(returnStatement.PreludeBindings),
            SupportedLifecycleIfReturnStatement ifReturn =>
                "if-return|" +
                DescribeLifecyclePreludeBindingsShape(ifReturn.ConditionPreludeBindings) +
                "|" +
                ifReturn.ConditionExpression +
                "|returns-true:" +
                ifReturn.ReturnsWhenTrue +
                "|then:" +
                DescribeLifecycleStatementSequenceShape(ifReturn.WhenTrue) +
                "|else:" +
                DescribeLifecycleStatementSequenceShape(ifReturn.WhenFalse),
            SupportedLifecycleTerminalIfReturnStatement terminalIfReturn =>
                "terminal-if-return|" +
                DescribeLifecyclePreludeBindingsShape(terminalIfReturn.ConditionPreludeBindings) +
                "|" +
                terminalIfReturn.ConditionExpression,
            SupportedLifecycleSwitchStatement switchStatement =>
                "switch|" + switchStatement.ValueExpression + "|" +
                string.Join(
                    ">",
                    switchStatement.Sections.Select(static section =>
                        "case:" +
                        string.Join(
                            ",",
                            section.Labels.Select(static label => label.IsDefault ? "default" : label.Expression)) +
                        "|body:" +
                        DescribeLifecycleStatementSequenceShape(section.Statements))),
            SupportedLifecyclePatternSwitchStatement patternSwitchStatement =>
                "pattern-switch|prelude:" +
                DescribeLifecyclePreludeBindingsShape(patternSwitchStatement.ValuePreludeBindings) +
                "|" +
                string.Join(
                    ">",
                    patternSwitchStatement.Sections.Select(static section =>
                        "case:" +
                        (section.IsDefault ? "default" : section.ConditionExpression) +
                        "|body:" +
                        DescribeLifecycleStatementSequenceShape(section.Statements))),
            SupportedLifecycleCompilerStatement compilerStatement =>
                "compiler|prelude:" +
                DescribeLifecyclePreludeBindingsShape(compilerStatement.PreludeBindings) +
                "|" +
                compilerStatement.StatementText,
            SupportedLifecycleTryFinallyStatement tryFinally =>
                "try|prelude:" +
                DescribeLifecyclePreludeBindingsShape(tryFinally.TryPreludeBindings) +
                "|body:" +
                DescribeLifecycleStatementSequenceShape(tryFinally.TryStatements) +
                "|finally:" +
                DescribeLifecycleStatementSequenceShape(tryFinally.FinallyStatements),
            SupportedLifecycleTryCatchStatement tryCatch =>
                "try-catch|prelude:" +
                DescribeLifecyclePreludeBindingsShape(tryCatch.TryPreludeBindings) +
                "|body:" +
                DescribeLifecycleStatementSequenceShape(tryCatch.TryStatements) +
                "|catch:" +
                DescribeLifecycleStatementSequenceShape(tryCatch.CatchStatements) +
                "|filter:" +
                (tryCatch.CatchFilterExpression ?? string.Empty) +
                "|filter-prelude:" +
                DescribeLifecyclePreludeBindingsShape(tryCatch.CatchFilterPreludeBindings) +
                "|has-finally:" +
                tryCatch.HasFinally +
                "|finally:" +
                DescribeLifecycleStatementSequenceShape(tryCatch.FinallyStatements),
            _ => "unsupported"
        };

    private static string DescribeLifecycleStatementSequenceShape(ImmutableArray<SupportedLifecycleStatement> statements)
        => string.Join(">", statements.Select(DescribeLifecycleStatementShape));

    private static string DescribeLifecyclePreludeBindingsShape(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> bindings)
        => bindings.IsDefaultOrEmpty
            ? string.Empty
            : string.Join(";", bindings.Select(static binding => binding.Code));

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
        ImmutableArray<SupportedLifecycleStatement> Statements,
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

    internal abstract record SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleEmitStatement(SupportedEmitCall EmitCall) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleIfStatement(
        string ConditionExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ConditionPreludeBindings,
        ImmutableArray<SupportedLifecycleStatement> WhenTrue,
        ImmutableArray<SupportedLifecycleStatement> WhenFalse) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleGuardReturnStatement(
        string ConditionExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ConditionPreludeBindings) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleReturnStatement(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> PreludeBindings) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleIfReturnStatement(
        string ConditionExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ConditionPreludeBindings,
        bool ReturnsWhenTrue,
        ImmutableArray<SupportedLifecycleStatement> WhenTrue,
        ImmutableArray<SupportedLifecycleStatement> WhenFalse) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleTerminalIfReturnStatement(
        string ConditionExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ConditionPreludeBindings) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleSwitchStatement(
        string ValueExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ValuePreludeBindings,
        ImmutableArray<SupportedLifecycleSwitchSection> Sections) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleSwitchSection(
        ImmutableArray<SupportedLifecycleSwitchLabel> Labels,
        ImmutableArray<SupportedLifecycleStatement> Statements);

    internal readonly record struct SupportedLifecycleSwitchLabel(bool IsDefault, string Expression);

    internal sealed record SupportedLifecyclePatternSwitchStatement(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> ValuePreludeBindings,
        ImmutableArray<SupportedLifecyclePatternSwitchSection> Sections) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecyclePatternSwitchSection(
        string ConditionExpression,
        bool IsDefault,
        ImmutableArray<SupportedLifecycleStatement> Statements);

    internal sealed record SupportedLifecycleCompilerStatement(
        string StatementText,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> PreludeBindings) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleTryFinallyStatement(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> TryPreludeBindings,
        ImmutableArray<SupportedLifecycleStatement> TryStatements,
        ImmutableArray<SupportedLifecycleStatement> FinallyStatements) : SupportedLifecycleStatement;

    internal sealed record SupportedLifecycleTryCatchStatement(
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> TryPreludeBindings,
        ImmutableArray<SupportedLifecycleStatement> TryStatements,
        ImmutableArray<SupportedLifecycleStatement> CatchStatements,
        string? CatchFilterExpression,
        ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> CatchFilterPreludeBindings,
        bool HasFinally,
        ImmutableArray<SupportedLifecycleStatement> FinallyStatements) : SupportedLifecycleStatement;

    private sealed class SetParametersAsyncStatementSequenceState
    {
        public SetParametersAsyncStatementSequenceState(
            SemanticModel semanticModel,
            IReadOnlyDictionary<ILocalSymbol, IOperation> localInitializers,
            bool allowTerminalNoOpControlFlow = true,
            bool allowDirectNoOpReturnStatement = false)
        {
            SemanticModel = semanticModel;
            LocalInitializers = localInitializers;
            AllowTerminalNoOpControlFlow = allowTerminalNoOpControlFlow;
            AllowDirectNoOpReturnStatement = allowDirectNoOpReturnStatement;
            EmittedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            MaterializedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            MaterializedPreludeAliases = new HashSet<string>(StringComparer.Ordinal);
            LocalAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
            FoldableCatchLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        }

        private SetParametersAsyncStatementSequenceState(
            SemanticModel semanticModel,
            IReadOnlyDictionary<ILocalSymbol, IOperation> localInitializers,
            bool allowTerminalNoOpControlFlow,
            bool allowDirectNoOpReturnStatement,
            HashSet<ILocalSymbol> emittedLocals,
            HashSet<ILocalSymbol> materializedLocals,
            HashSet<string> materializedPreludeAliases,
            Dictionary<ILocalSymbol, string> localAliases,
            HashSet<ILocalSymbol> foldableCatchLocals)
        {
            SemanticModel = semanticModel;
            LocalInitializers = localInitializers;
            AllowTerminalNoOpControlFlow = allowTerminalNoOpControlFlow;
            AllowDirectNoOpReturnStatement = allowDirectNoOpReturnStatement;
            EmittedLocals = emittedLocals;
            MaterializedLocals = materializedLocals;
            MaterializedPreludeAliases = materializedPreludeAliases;
            LocalAliases = localAliases;
            FoldableCatchLocals = foldableCatchLocals;
        }

        public SemanticModel SemanticModel { get; }
        public IReadOnlyDictionary<ILocalSymbol, IOperation> LocalInitializers { get; }
        public bool AllowTerminalNoOpControlFlow { get; }
        public bool AllowDirectNoOpReturnStatement { get; }
        public HashSet<ILocalSymbol> EmittedLocals { get; }
        public HashSet<ILocalSymbol> MaterializedLocals { get; }
        public HashSet<string> MaterializedPreludeAliases { get; }
        public Dictionary<ILocalSymbol, string> LocalAliases { get; }
        public HashSet<ILocalSymbol> FoldableCatchLocals { get; }

        public SetParametersAsyncStatementSequenceState CloneForBranch()
            => new(
                SemanticModel,
                LocalInitializers,
                allowTerminalNoOpControlFlow: false,
                allowDirectNoOpReturnStatement: false,
                new HashSet<ILocalSymbol>(EmittedLocals, SymbolEqualityComparer.Default),
                new HashSet<ILocalSymbol>(MaterializedLocals, SymbolEqualityComparer.Default),
                new HashSet<string>(MaterializedPreludeAliases, StringComparer.Ordinal),
                new Dictionary<ILocalSymbol, string>(LocalAliases, SymbolEqualityComparer.Default),
                new HashSet<ILocalSymbol>(FoldableCatchLocals, SymbolEqualityComparer.Default));

        public SetParametersAsyncStatementSequenceState CloneForDirectNoOpReturnBody()
            => new(
                SemanticModel,
                LocalInitializers,
                allowTerminalNoOpControlFlow: false,
                allowDirectNoOpReturnStatement: true,
                new HashSet<ILocalSymbol>(EmittedLocals, SymbolEqualityComparer.Default),
                new HashSet<ILocalSymbol>(MaterializedLocals, SymbolEqualityComparer.Default),
                new HashSet<string>(MaterializedPreludeAliases, StringComparer.Ordinal),
                new Dictionary<ILocalSymbol, string>(LocalAliases, SymbolEqualityComparer.Default),
                new HashSet<ILocalSymbol>(FoldableCatchLocals, SymbolEqualityComparer.Default));

        public SetParametersAsyncStatementSequenceState CloneForCatchBody(ILocalSymbol? catchLocal)
        {
            var foldableCatchLocals = new HashSet<ILocalSymbol>(FoldableCatchLocals, SymbolEqualityComparer.Default);
            if (catchLocal is not null)
                foldableCatchLocals.Add(catchLocal);

            return new(
                SemanticModel,
                LocalInitializers,
                allowTerminalNoOpControlFlow: false,
                allowDirectNoOpReturnStatement: false,
                new HashSet<ILocalSymbol>(EmittedLocals, SymbolEqualityComparer.Default),
                new HashSet<ILocalSymbol>(MaterializedLocals, SymbolEqualityComparer.Default),
                new HashSet<string>(MaterializedPreludeAliases, StringComparer.Ordinal),
                new Dictionary<ILocalSymbol, string>(LocalAliases, SymbolEqualityComparer.Default),
                foldableCatchLocals);
        }
    }

    private sealed record SetParametersAsyncAnalysis(
        bool IsSupported,
        ImmutableArray<SupportedLifecycleStatement> Statements)
    {
        public static SetParametersAsyncAnalysis Unsupported { get; } = new(false, ImmutableArray<SupportedLifecycleStatement>.Empty);
        public static SetParametersAsyncAnalysis NoOp { get; } = new(true, ImmutableArray<SupportedLifecycleStatement>.Empty);
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
