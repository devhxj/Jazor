using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Jazor.Compiler;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    private string EmitExpression(IOperation operation, SenseArgument? compilerArgument = null)
    {
        var current = Unwrap(operation);
        if (current is null)
            return "undefined";

        return EmitCompilerLoweredExpression(current, compilerArgument);
    }

    internal string EmitSetupExpression(IOperation operation, SenseArgument? compilerArgument = null)
    {
        var current = Unwrap(operation);
        if (current is null)
            return "undefined";

        return EmitCompilerLoweredExpression(current, compilerArgument);
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadCore(
        IMethodSymbol method,
        IOperation? operation,
        bool allowFirstRenderPayload)
    {
        var current = Unwrap(operation);
        if (current is null)
            throw new NotSupportedException($"RazorVue lifecycle payload is missing an operation in component '{method.ContainingType.ToDisplayString()}'.");

        return current switch
        {
            ILiteralOperation literal => new LifecyclePayloadEmission(EmitLiteral(literal), false),
            IDefaultValueOperation defaultValue when IsNullDefaultValue(defaultValue) => new LifecyclePayloadEmission("null", false),
            IParameterReferenceOperation parameter when IsFirstRenderPayloadParameter(method, parameter, allowFirstRenderPayload) =>
                new LifecyclePayloadEmission(LifecycleFirstRenderPlaceholder, true),
            IPropertyReferenceOperation property => EmitLifecyclePayloadPropertyReference(method, property),
            IUnaryOperation unary => EmitLifecyclePayloadUnary(method, unary, allowFirstRenderPayload),
            IBinaryOperation binary => EmitLifecyclePayloadBinary(method, binary, allowFirstRenderPayload),
            IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
                EmitLifecyclePayloadConditional(method, conditional, allowFirstRenderPayload),
            IInterpolatedStringOperation interpolated => EmitLifecyclePayloadInterpolatedString(method, interpolated, allowFirstRenderPayload),
            _ => throw new NotSupportedException(
                $"RazorVue lifecycle payload does not support expression '{current.Kind}' in component '{method.ContainingType.ToDisplayString()}'.")
        };
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadPropertyReference(
        IMethodSymbol method,
        IPropertyReferenceOperation property)
    {
        if (IsCurrentComponentMember(method.ContainingType, property.Property, property.Instance) &&
            IsComponentParameterProperty(property.Property))
        {
            return new LifecyclePayloadEmission("props." + ToLifecyclePropName(property.Property.Name), false);
        }

        throw new NotSupportedException(
            $"RazorVue lifecycle payload only supports component [Parameter] properties. Unsupported member: '{property.Property.Name}'.");
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadUnary(
        IMethodSymbol method,
        IUnaryOperation unary,
        bool allowFirstRenderPayload)
    {
        var operand = EmitLifecyclePayloadCore(method, unary.Operand, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(GetUnaryOperator(unary.OperatorKind) + operand.Expression, operand.UsesFirstRender);
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadBinary(
        IMethodSymbol method,
        IBinaryOperation binary,
        bool allowFirstRenderPayload)
    {
        var left = EmitLifecyclePayloadCore(method, binary.LeftOperand, allowFirstRenderPayload);
        var right = EmitLifecyclePayloadCore(method, binary.RightOperand, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(
            "(" + left.Expression + " " + GetBinaryOperator(binary.OperatorKind) + " " + right.Expression + ")",
            left.UsesFirstRender || right.UsesFirstRender);
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadConditional(
        IMethodSymbol method,
        IConditionalOperation conditional,
        bool allowFirstRenderPayload)
    {
        var condition = EmitLifecyclePayloadCore(method, conditional.Condition, allowFirstRenderPayload);
        var whenTrue = EmitLifecyclePayloadCore(method, conditional.WhenTrue, allowFirstRenderPayload);
        var whenFalse = EmitLifecyclePayloadCore(method, conditional.WhenFalse, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(
            "(" + condition.Expression + " ? " + whenTrue.Expression + " : " + whenFalse.Expression + ")",
            condition.UsesFirstRender || whenTrue.UsesFirstRender || whenFalse.UsesFirstRender);
    }

    private static LifecyclePayloadEmission EmitLifecyclePayloadInterpolatedString(
        IMethodSymbol method,
        IInterpolatedStringOperation interpolated,
        bool allowFirstRenderPayload)
    {
        var builder = new StringBuilder();
        var usesFirstRender = false;
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    var expression = EmitLifecyclePayloadCore(method, interpolation.Expression, allowFirstRenderPayload);
                    builder.Append("${").Append(expression.Expression).Append('}');
                    usesFirstRender |= expression.UsesFirstRender;
                    break;
            }
        }

        builder.Append('`');
        return new LifecyclePayloadEmission(builder.ToString(), usesFirstRender);
    }

    private string EmitPropertyReference(IPropertyReferenceOperation property)
    {
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: false, compilerArgument: null, out var alias))
            return alias;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
                return "props." + prop.Name;

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
            {
                if (slot.IsDefault)
                    return "slots.default ? slots.default() : null";

                return "props." + ToLowerCamelCase(property.Property.Name);
            }

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
                return EmitCurrentComponentCallbackReference(property.Property);

            throw new NotSupportedException(
                $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
        }

        return EmitMemberTarget(property.Instance) + "." + property.Property.Name;
    }

    internal bool TryRewritePropertyReference(IPropertyReferenceOperation property, SenseArgument argument, out string expression)
    {
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: false, argument, out expression))
            return true;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
            {
                expression = "props." + prop.Name;
                return true;
            }

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
            {
                expression = slot.IsDefault
                    ? "slots.default ? slots.default() : null"
                    : "props." + ToLowerCamelCase(property.Property.Name);
                return true;
            }

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
            {
                expression = EmitCurrentComponentCallbackReference(property.Property);
                return true;
            }

            throw new NotSupportedException(
                $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
        }

        expression = string.Empty;
        return false;
    }

    private string EmitSetupPropertyReference(IPropertyReferenceOperation property)
    {
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: true, compilerArgument: null, out var alias))
            return alias;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
                return "props." + prop.Name;

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
                return EmitCurrentComponentCallbackReference(property.Property);

            throw CreateUnsupportedSetupLogicException(
                property.Property,
                $"RazorVue setup-side logic only supports component [Parameter] properties. Unsupported member: '{property.Property.Name}'.");
        }

        return EmitMemberTarget(property.Instance) + "." + property.Property.Name;
    }

    private bool TryEmitKnownAliasedProperty(
        IPropertyReferenceOperation property,
        bool useSetupEmitter,
        SenseArgument? compilerArgument,
        out string expression)
    {
        expression = string.Empty;
        if (property.Instance is null)
            return false;

        if (!string.Equals(property.Property.Name, "Count", StringComparison.Ordinal))
            return false;

        if (!IsArrayLikeCountCarrier(property.Property.ContainingType))
            return false;

        expression = (useSetupEmitter
            ? EmitSetupExpression(property.Instance, compilerArgument)
            : EmitExpression(property.Instance, compilerArgument)) + ".length";
        return true;
    }

    private static bool IsArrayLikeCountCarrier(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        var displayName = type.OriginalDefinition.ToDisplayString();
        return string.Equals(displayName, "System.Collections.Generic.List<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.Generic.ICollection<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.ICollection", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.ObjectModel.ReadOnlyCollection<T>", StringComparison.Ordinal);
    }

    private string EmitFieldReference(IFieldReferenceOperation field)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                _requiredSetupFields.Add(field.Field);
                return ToLowerCamelCase(field.Field.Name);
            }

            throw new NotSupportedException(
                $"RazorVue render currently does not support component field '{field.Field.Name}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        return EmitMemberTarget(field.Instance) + "." + field.Field.Name;
    }

    internal bool TryRewriteFieldReference(IFieldReferenceOperation field, SenseArgument argument, out string expression)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                _requiredSetupFields.Add(field.Field);
                expression = ToLowerCamelCase(field.Field.Name);
                return true;
            }

            throw new NotSupportedException(
                $"RazorVue render currently does not support component field '{field.Field.Name}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        expression = string.Empty;
        return false;
    }

    private string EmitSetupFieldReference(IFieldReferenceOperation field)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                _requiredSetupFields.Add(field.Field);
                return ToLowerCamelCase(field.Field.Name);
            }

            throw CreateUnsupportedSetupLogicException(
                field.Field,
                $"RazorVue setup-side logic does not support component field '{field.Field.Name}'.");
        }

        return EmitMemberTarget(field.Instance) + "." + field.Field.Name;
    }

    private static bool IsNullDefaultValue(IDefaultValueOperation defaultValue)
    {
        var type = defaultValue.Type;
        if (type is null)
            return false;

        if (type.IsReferenceType)
            return true;

        return type is INamedTypeSymbol namedType &&
               namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private static bool IsFirstRenderPayloadParameter(
        IMethodSymbol method,
        IParameterReferenceOperation parameter,
        bool allowFirstRenderPayload)
    {
        if (!allowFirstRenderPayload)
            return false;

        return method.Parameters.Any(candidate =>
            candidate.Name == "firstRender" &&
            SymbolEqualityComparer.Default.Equals(candidate, parameter.Parameter));
    }

    private static bool IsCurrentComponentMember(
        INamedTypeSymbol componentSymbol,
        ISymbol symbol,
        IOperation? instance)
    {
        for (var current = componentSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
                return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
        }

        return false;
    }

    private static bool IsComponentParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.ParameterAttribute",
                StringComparison.Ordinal));

    private static string ToLifecyclePropName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private string EmitInvocation(IInvocationOperation invocation)
    {
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
            return normalizedCallbackFactory;

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
            return normalizedInferredCallback;

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            return EmitExpression(invocation.Instance) + "(" +
                   string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            // Keep render-side helper lowering conservative by requiring the call-site
            // arity to match the helper signature exactly; unsupported method shapes still
            // fail later in setup lowering.
            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            _requiredSetupMethods.Add(invocation.TargetMethod);
            return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
                   string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
        }

        var targetMethodName = GetEmittedMethodName(invocation.TargetMethod);
        var target = invocation.Instance is not null
            ? EmitMemberInvocationTarget(invocation.Instance, targetMethodName, useSetupEmitter: false)
            : targetMethodName;

        return target + "(" + string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
    }

    internal bool TryRewriteInvocation(IInvocationOperation invocation, SenseArgument argument, out string expression)
    {
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
        {
            expression = normalizedCallbackFactory;
            return true;
        }

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
        {
            expression = normalizedInferredCallback;
            return true;
        }

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            expression = EmitExpression(invocation.Instance, argument) + "(" +
                         string.Join(", ", invocation.Arguments.Select(item => EmitExpression(item.Value, argument))) + ")";
            return true;
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            _requiredSetupMethods.Add(invocation.TargetMethod);
            expression = ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
                         string.Join(", ", invocation.Arguments.Select(item => EmitExpression(item.Value, argument))) + ")";
            return true;
        }

        expression = string.Empty;
        return false;
    }

    private string EmitSetupInvocation(IInvocationOperation invocation)
    {
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
            return normalizedCallbackFactory;

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
            return normalizedInferredCallback;

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            return EmitSetupExpression(invocation.Instance) + "(" +
                   string.Join(", ", invocation.Arguments.Select(argument => EmitSetupExpression(argument.Value))) + ")";
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            _requiredSetupMethods.Add(invocation.TargetMethod);
            return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
                   string.Join(", ", invocation.Arguments.Select(argument => EmitSetupExpression(argument.Value))) + ")";
        }

        var targetMethodName = GetEmittedMethodName(invocation.TargetMethod);
        var target = invocation.Instance is not null
            ? EmitMemberInvocationTarget(invocation.Instance, targetMethodName, useSetupEmitter: true)
            : targetMethodName;

        return target + "(" + string.Join(", ", invocation.Arguments.Select(argument => EmitSetupExpression(argument.Value))) + ")";
    }

    internal bool TryRewriteMethodReference(IMethodReferenceOperation operation, SenseArgument argument, out string expression)
    {
        if (IsCurrentComponentMember(operation.Method, operation.Instance))
        {
            _requiredSetupMethods.Add(operation.Method);
            expression = ToLowerCamelCase(operation.Method.Name);
            return true;
        }

        expression = string.Empty;
        return false;
    }

    private string EmitMemberInvocationTarget(IOperation instance, string targetMethodName, bool useSetupEmitter)
    {
        var target = useSetupEmitter
            ? EmitSetupExpression(instance)
            : EmitExpression(instance);

        if (targetMethodName == "toString" && RequiresParenthesizedMemberTarget(instance))
            target = "(" + target + ")";

        return target + "." + targetMethodName;
    }

    private static bool RequiresParenthesizedMemberTarget(IOperation instance)
        => Unwrap(instance) is IBinaryOperation or IConditionalOperation;

    private static string GetEmittedMethodName(IMethodSymbol method)
        => method.Name == "ToString" && method.Parameters.Length == 0 && method.MethodKind == MethodKind.Ordinary
            ? "toString"
            : method.Name;

    private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(IMethodSymbol method)
        => CreateUnsupportedSetupLogicException(
            method,
            $"RazorVue setup lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.");

    private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(ISymbol symbol, string message)
    {
        var originLocation = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var ownerComponent = symbol.ContainingType?.ToDisplayString() ?? _snapshot.Descriptor.FullName;
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, ownerComponent, origin);
    }

    private string EmitInterpolatedString(IInterpolatedStringOperation interpolated)
    {
        var builder = new StringBuilder();
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    builder.Append("${").Append(EmitExpression(interpolation.Expression)).Append('}');
                    break;
            }
        }

        builder.Append('`');
        return builder.ToString();
    }

    private string EmitSetupInterpolatedString(IInterpolatedStringOperation interpolated)
    {
        var builder = new StringBuilder();
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    builder.Append("${").Append(EmitSetupExpression(interpolation.Expression)).Append('}');
                    break;
            }
        }

        builder.Append('`');
        return builder.ToString();
    }

    private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
    {
        for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
                return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
        }

        return false;
    }

    private string EmitMemberTarget(IOperation? instance)
    {
        var current = Unwrap(instance);
        if (current is null)
            throw new NotSupportedException("RazorVue render member access is missing an instance target.");

        return EmitExpression(current);
    }

    private static IOperation? Unwrap(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation);

    private string TryNormalizeRazorGeneratedCallbackFactory(IInvocationOperation invocation)
    {
        if (!IsEventCallbackFactoryCreate(invocation) || invocation.Arguments.Length < 2)
            return string.Empty;

        var receiver = Unwrap(invocation.Arguments[0].Value);
        if (receiver is not IInstanceReferenceOperation)
            return string.Empty;

        var callbackTarget = Unwrap(invocation.Arguments[1].Value);
        if (callbackTarget is null)
            return string.Empty;

        if (TryNormalizeRazorInferredEventCallback(callbackTarget, out var inferredCallbackFactory))
            return inferredCallbackFactory;

        return callbackTarget switch
        {
            IPropertyReferenceOperation property when IsCurrentComponentMember(property.Property, property.Instance) =>
                EmitCurrentComponentCallbackReference(property.Property),
            IFieldReferenceOperation field when IsCurrentComponentMember(field.Field, field.Instance) =>
                EmitCurrentComponentCallbackReference(field.Field),
            _ => string.Empty
        };
    }

    private bool TryNormalizeRazorInferredEventCallback(IOperation callbackTarget, out string expression)
    {
        expression = string.Empty;
        if (callbackTarget is not IInvocationOperation invocation ||
            !IsInferredEventCallback(invocation) ||
            invocation.Arguments.Length < 2)
        {
            return false;
        }

        if (!TryGetAssignedLambdaTarget(invocation.Arguments[1].Value, out var assignedTarget))
            return false;

        switch (assignedTarget)
        {
            case IPropertyReferenceOperation property when IsCurrentComponentMember(property.Property, property.Instance):
                var changedAlias = GetBindChangedSymbol(property.Property);
                if (!_emitsByRazorAlias.ContainsKey(changedAlias))
                    throw CreateInvalidBindTargetException(property.Property);

                expression = EmitCurrentComponentCallbackReference(changedAlias);
                return true;
            case IFieldReferenceOperation field when IsCurrentComponentMember(field.Field, field.Instance):
                _requiredSetupFields.Add(field.Field);
                expression = "(__value) => (" + ToLowerCamelCase(field.Field.Name) + " = __value)";
                return true;
            case ILocalReferenceOperation local:
                expression = "(__value) => (" + local.Local.Name + " = __value)";
                return true;
            case IParameterReferenceOperation:
                throw CreateUnsupportedSetupLogicException(
                    _snapshot.ComponentSymbol,
                    $"RazorVue setup-side logic does not support assigning to method parameters from generated two-way binding in component '{_snapshot.Descriptor.FullName}'.");
            default:
                return false;
        }
    }

    private bool TryGetAssignedLambdaTarget(IOperation operation, out IOperation target)
    {
        target = default!;
        if (!TryGetAnonymousFunction(operation, out var anonymousFunction))
            return false;

        var body = UnwrapLambdaBody(anonymousFunction.Body);

        if (body is not ISimpleAssignmentOperation assignment)
            return false;

        target = Unwrap(assignment.Target)!;
        return target is not null;
    }

    private static IOperation? UnwrapLambdaBody(IOperation? operation)
    {
        var current = Unwrap(operation);
        while (true)
        {
            switch (current)
            {
                case IBlockOperation block:
                    if (TryGetSingleEffectiveLambdaOperation(block, out var effectiveOperation))
                    {
                        current = Unwrap(effectiveOperation);
                        continue;
                    }

                    return current;
                case IExpressionStatementOperation statement:
                    current = Unwrap(statement.Operation);
                    continue;
                case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                    current = Unwrap(returnOperation.ReturnedValue);
                    continue;
                default:
                    return current;
            }
        }
    }

    private static bool TryGetSingleEffectiveLambdaOperation(
        IBlockOperation block,
        out IOperation effectiveOperation)
    {
        effectiveOperation = default!;
        if (block.Operations.Length == 0)
            return false;

        var effectiveOperations = block.Operations
            .Where(static operation => operation is not IReturnOperation { ReturnedValue: null })
            .ToArray();
        if (effectiveOperations.Length != 1)
            return false;

        effectiveOperation = effectiveOperations[0];
        return true;
    }

    private static bool TryGetAnonymousFunction(IOperation? operation, out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        var current = UnwrapDelegateCarrier(operation);
        switch (current)
        {
            case IAnonymousFunctionOperation directAnonymousFunction:
                anonymousFunction = directAnonymousFunction;
                return true;
            case IDelegateCreationOperation delegateCreation when UnwrapDelegateCarrier(delegateCreation.Target) is IAnonymousFunctionOperation targetAnonymousFunction:
                anonymousFunction = targetAnonymousFunction;
                return true;
            default:
                return false;
        }
    }

    private static IOperation? UnwrapDelegateCarrier(IOperation? operation)
    {
        var current = Unwrap(operation);
        while (current is IConversionOperation conversion)
            current = Unwrap(conversion.Operand);
        return current;
    }

    private static bool IsInferredEventCallback(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "CreateInferredEventCallback" &&
           string.Equals(
               invocation.TargetMethod.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers",
               StringComparison.Ordinal);

    private static string GetBindChangedSymbol(IPropertySymbol property)
        => property.Name + "Changed";

    private string EmitCurrentComponentCallbackReference(ISymbol symbol)
    {
        return EmitCurrentComponentCallbackReference(symbol.Name);
    }

    private string EmitCurrentComponentCallbackReference(string razorAlias)
    {
        if (_emitsByRazorAlias.TryGetValue(razorAlias, out var emitDescriptor))
        {
            var payloadParameterName = GetVueEmitPayloadParameterName(emitDescriptor);
            return payloadParameterName.Length == 0
                ? "() => emit(" + ToJavaScriptString(emitDescriptor.Name) + ")"
                : "(" + payloadParameterName + ") => emit(" + ToJavaScriptString(emitDescriptor.Name) + ", " + payloadParameterName + ")";
        }

        if (_propsByPublicName.TryGetValue(razorAlias, out var propDescriptor))
            return "props." + propDescriptor.Name;

        throw new NotSupportedException(
            $"RazorVue render currently does not support callback member '{razorAlias}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    private static string GetVueEmitPayloadParameterName(VueEmitDescriptor emitDescriptor)
        => string.Equals(emitDescriptor.PayloadTypeName, "void", StringComparison.Ordinal)
            ? string.Empty
            : "__value";

    private RazorVueCompilationIssueException CreateInvalidBindTargetException(IPropertySymbol property)
    {
        var originLocation = property.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidBindTarget,
            RazorVueIssueSeverity.Error,
            $"Component '{_snapshot.Descriptor.Name}' does not support two-way binding for parameter '{property.Name}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private static bool IsEventCallbackFactoryCreate(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "Create" &&
           string.Equals(
               invocation.TargetMethod.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.EventCallbackFactory",
               StringComparison.Ordinal);

    private static bool IsCallableSlotExpression(IOperation operation)
        => Unwrap(operation)?.Type?.TypeKind == TypeKind.Delegate;

    private static string EmitLiteral(ILiteralOperation literal)
    {
        if (!literal.ConstantValue.HasValue || literal.ConstantValue.Value is null)
            return "null";

        return literal.ConstantValue.Value switch
        {
            string text => ToJavaScriptString(text),
            char ch => ToJavaScriptString(ch.ToString()),
            bool value => value ? "true" : "false",
            float value => value.ToString("R", CultureInfo.InvariantCulture),
            double value => value.ToString("R", CultureInfo.InvariantCulture),
            decimal value => value.ToString(CultureInfo.InvariantCulture),
            sbyte value => value.ToString(CultureInfo.InvariantCulture),
            byte value => value.ToString(CultureInfo.InvariantCulture),
            short value => value.ToString(CultureInfo.InvariantCulture),
            ushort value => value.ToString(CultureInfo.InvariantCulture),
            int value => value.ToString(CultureInfo.InvariantCulture),
            uint value => value.ToString(CultureInfo.InvariantCulture),
            long value => value.ToString(CultureInfo.InvariantCulture),
            ulong value => value.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(literal.ConstantValue.Value, CultureInfo.InvariantCulture) ?? "null"
        };
    }

    private static string GetBinaryOperator(BinaryOperatorKind kind)
        => kind switch
        {
            BinaryOperatorKind.Add => "+",
            BinaryOperatorKind.Subtract => "-",
            BinaryOperatorKind.Multiply => "*",
            BinaryOperatorKind.Divide => "/",
            BinaryOperatorKind.Remainder => "%",
            BinaryOperatorKind.Equals => "===",
            BinaryOperatorKind.NotEquals => "!==",
            BinaryOperatorKind.LessThan => "<",
            BinaryOperatorKind.LessThanOrEqual => "<=",
            BinaryOperatorKind.GreaterThan => ">",
            BinaryOperatorKind.GreaterThanOrEqual => ">=",
            BinaryOperatorKind.ConditionalAnd => "&&",
            BinaryOperatorKind.ConditionalOr => "||",
            BinaryOperatorKind.And => "&",
            BinaryOperatorKind.Or => "|",
            BinaryOperatorKind.ExclusiveOr => "^",
            _ => throw new NotSupportedException($"Unsupported RazorVue binary operator: {kind}.")
        };

    private static string GetUnaryOperator(UnaryOperatorKind kind)
        => kind switch
        {
            UnaryOperatorKind.Not => "!",
            UnaryOperatorKind.Minus => "-",
            UnaryOperatorKind.Plus => "+",
            _ => throw new NotSupportedException($"Unsupported RazorVue unary operator: {kind}.")
        };

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";

    private static string EscapeTemplateText(string value)
        => (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("`", "\\`")
            .Replace("${", "\\${");
}
