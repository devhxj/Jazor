using System;
using System.Collections.Generic;
using System.Linq;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Host seam for the first RazorVue current-component lowering slice.
/// It rewrites members declared on the current component into the explicit
/// component runtime surface used by render-function framing:
/// fields/non-parameter properties read from <c>state</c>, parameters read from
/// <c>props</c>, and current-component method references lower to stable module
/// function identifiers.
/// </summary>
public sealed class CurrentComponentSemanticWalkerHost : SemanticWalkerHost
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string EventCallbackFactoryMetadataName = "Microsoft.AspNetCore.Components.EventCallbackFactory";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private const string ComponentBaseMetadataName = "Microsoft.AspNetCore.Components.ComponentBase";
    private const string StateHasChangedRuntimeName = "stateHasChanged";
    private const string InvokeAsyncRuntimeName = "invokeAsync";
    private readonly INamedTypeSymbol _componentType;
    private readonly RenderTreeBuilderSemanticWalkerHost _renderTreeBuilderHost = new();
    private readonly string _stateIdentifier;
    private readonly string _propsIdentifier;
    private readonly IReadOnlyDictionary<string, string>? _parameterRuntimeNames;

    public CurrentComponentSemanticWalkerHost(
        INamedTypeSymbol componentType,
        string stateIdentifier = "state",
        string propsIdentifier = "props",
        IReadOnlyDictionary<string, string>? parameterRuntimeNames = null)
    {
        _componentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        if (string.IsNullOrWhiteSpace(stateIdentifier))
            throw new ArgumentException("State identifier cannot be empty.", nameof(stateIdentifier));
        if (string.IsNullOrWhiteSpace(propsIdentifier))
            throw new ArgumentException("Props identifier cannot be empty.", nameof(propsIdentifier));

        _stateIdentifier = stateIdentifier;
        _propsIdentifier = propsIdentifier;
        _parameterRuntimeNames = parameterRuntimeNames;
    }

    public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
    {
        if (IsEventCallbackFactoryCreateBinder(operation.TargetMethod))
            return RewriteEventCallbackFactoryCreateBinder(operation);

        if (IsEventCallbackFactoryCreate(operation.TargetMethod))
            return RewriteEventCallbackFactoryCreate(operation);

        if (IsStateHasChangedInvocation(operation.TargetMethod, operation.Instance))
            return RewriteStateHasChanged(operation);

        if (IsUnsupportedIndirectCurrentComponentDispatch(operation.TargetMethod, operation.Instance))
            throw CreateUnsupportedIndirectCurrentComponentDispatchException(operation, operation.TargetMethod, operation.Instance);

        return _renderTreeBuilderHost.RewriteInvocationPreorder(operation, argument);
    }

    public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        => _renderTreeBuilderHost.RewriteObjectCreationPreorder(operation, argument);

    public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        => _renderTreeBuilderHost.ShouldRewriteObjectCreation(operation);

    public override Expression? RewriteObjectCreation(
        IObjectCreationOperation operation,
        SenseArgument argument,
        IReadOnlyList<Expression> arguments)
        => _renderTreeBuilderHost.RewriteObjectCreation(operation, argument, arguments);

    public override bool ShouldSkipVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument)
        => _renderTreeBuilderHost.ShouldSkipVariableDeclarator(operation, argument);

    public override Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation,
        IArgumentOperation argumentOperation,
        int argumentIndex,
        SenseArgument argument)
        => _renderTreeBuilderHost.RewriteInvocationArgumentPreorder(operation, argumentOperation, argumentIndex, argument);

    public override Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
        if (_renderTreeBuilderHost.RewriteInvocation(operation, argument, instance, arguments) is Expression renderTreeBuilderExpression)
            return renderTreeBuilderExpression;

        if (IsStateHasChangedInvocation(operation.TargetMethod, operation.Instance))
            return RewriteStateHasChanged(operation);

        if (IsComponentBaseInvokeAsyncInvocation(operation.TargetMethod, operation.Instance))
            return RewriteInvokeAsync(operation, arguments);

        // VisitInvocation calls this before RejectUnsupportedRuntimeFallback.
        if (IsEventCallbackInvoke(operation.TargetMethod) &&
            TryGetCurrentComponentEventCallbackParameter(operation.Instance) is IPropertySymbol eventCallbackParameter)
        {
            return RewriteEventCallbackInvoke(eventCallbackParameter, arguments);
        }

        if (!IsCurrentComponentMethod(operation.TargetMethod, operation.Instance))
            return null;

        return new CallExpression(
            new Identifier(GetMemberName(operation.TargetMethod)),
            NodeList.From(arguments),
            optional: false);
    }

    public override Expression? RewriteMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => IsCurrentComponentMethod(operation.Method, operation.Instance)
            ? new Identifier(GetMemberName(operation.Method))
            : null;

    public override Expression? RewriteMethodReferencePreorder(IMethodReferenceOperation operation, SenseArgument argument)
    {
        if (IsUnsupportedIndirectCurrentComponentDispatch(operation.Method, operation.Instance))
            throw CreateUnsupportedIndirectCurrentComponentDispatchException(operation, operation.Method, operation.Instance);

        return null;
    }

    public override Expression? RewriteFieldReference(
        IFieldReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => IsCurrentComponentField(operation.Field, operation.Instance)
            ? BuildStateAccess(operation.Field)
            : null;

    public override Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
        if (!IsCurrentComponentProperty(operation.Property, operation.Instance))
            return null;

        if (operation.Property.IsIndexer || arguments.Count > 0)
        {
            throw new OperationTransformationException(
                operation,
                $"Current-component indexed property '{operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported by RazorVue current-component rewrite v1.");
        }

        return IsParameterProperty(operation.Property)
            ? BuildPropsAccess(operation.Property)
            : IsAutoProperty(operation.Property)
                ? BuildStateAccess(operation.Property)
                : throw new OperationTransformationException(
                    operation,
                    "Current-component property '" +
                    operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                    "' uses computed/accessor semantics that are not supported by RazorVue current-component rewrite v1. " +
                    "Use a field, an auto property, or an explicit method until computed property lowering is defined.");
    }

    public override Expression? RewriteSimpleAssignmentPreorder(ISimpleAssignmentOperation operation, SenseArgument argument)
    {
        if (operation.Target is IPropertyReferenceOperation propertyReference &&
            IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
            IsParameterProperty(propertyReference.Property))
        {
            throw new OperationTransformationException(
                operation,
                "Current-component parameter '" +
                propertyReference.Property.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                "' is decorated with Microsoft.AspNetCore.Components.ParameterAttribute and is lowered as read-only props in RazorVue current-component rewrite v1.");
        }

        return null;
    }

    private Expression RewriteEventCallbackFactoryCreate(IInvocationOperation operation)
    {
        if (operation.Arguments.Length < 2)
            throw CreateUnsupportedEventCallbackFactoryException(operation);

        if (!IsCurrentComponentEventCallbackReceiver(operation.Arguments[0].Value))
            throw CreateUnsupportedEventCallbackFactoryException(operation);

        return RewriteEventCallbackHandler(operation.Arguments[1].Value)
            ?? throw CreateUnsupportedEventCallbackFactoryException(operation);
    }

    private Expression RewriteEventCallbackFactoryCreateBinder(IInvocationOperation operation)
    {
        if (!TryGetCreateBinderReceiverAndHandler(operation, out var receiver, out var handler))
            throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);

        if (!IsCurrentComponentEventCallbackReceiver(receiver))
            throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);

        return RewriteBinderHandler(handler)
            ?? throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);
    }

    private static bool TryGetCreateBinderReceiverAndHandler(
        IInvocationOperation operation,
        out IOperation receiver,
        out IOperation handler)
    {
        receiver = operation;
        handler = operation;
        if (operation.Arguments.Length < 3)
            return false;

        // EventCallbackFactoryBinderExtensions.CreateBinder is commonly exposed
        // as an extension-method invocation with the factory field still present
        // as the first argument:
        //   [EventCallback.Factory, receiver, handler, currentValue, culture]
        var receiverIndex = operation.Arguments[0].Value is IFieldReferenceOperation ? 1 : 0;
        var handlerIndex = receiverIndex + 1;
        if (operation.Arguments.Length <= handlerIndex)
            return false;

        receiver = operation.Arguments[receiverIndex].Value;
        handler = operation.Arguments[handlerIndex].Value;
        return true;
    }

    private Expression? RewriteBinderHandler(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => RewriteBinderHandler(conversion.Operand),
            IDelegateCreationOperation delegateCreation => RewriteBinderHandler(delegateCreation.Target),
            IAnonymousFunctionOperation anonymousFunction => RewriteBinderHandler(anonymousFunction),
            _ => null
        };

    private Expression? RewriteBinderHandler(IAnonymousFunctionOperation anonymousFunction)
    {
        if (anonymousFunction.Symbol.Parameters.Length != 1 ||
            TryGetSingleBinderAssignment(anonymousFunction.Body) is not ISimpleAssignmentOperation assignment)
        {
            return null;
        }

        var parameter = anonymousFunction.Symbol.Parameters[0];
        if (!IsAssignmentFromParameter(assignment.Value, parameter))
            return null;

        var assignmentTarget = RewriteBinderAssignmentTarget(assignment.Target);
        if (assignmentTarget is null)
            return null;

        var parameterIdentifier = new Identifier(parameter.Name);
        return new ArrowFunctionExpression(
            NodeList.From<Node>(parameterIdentifier),
            new AssignmentExpression(
                Operator.Assignment,
                assignmentTarget,
                parameterIdentifier),
            expression: true,
            async: false);
    }

    private static ISimpleAssignmentOperation? TryGetSingleBinderAssignment(IBlockOperation body)
    {
        ISimpleAssignmentOperation? assignment = null;

        foreach (var operation in body.Operations)
        {
            if (TryGetBinderAssignment(operation) is ISimpleAssignmentOperation candidate)
            {
                if (assignment is not null)
                    return null;

                assignment = candidate;
                continue;
            }

            if (IsEmptyReturn(operation))
                continue;

            return null;
        }

        return assignment;
    }

    private static ISimpleAssignmentOperation? TryGetBinderAssignment(IOperation operation)
    {
        if (operation is IExpressionStatementOperation expressionStatement)
            operation = expressionStatement.Operation;

        if (operation is IReturnOperation { ReturnedValue: not null } returnOperation)
            operation = returnOperation.ReturnedValue;

        if (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation as ISimpleAssignmentOperation;
    }

    private static bool IsEmptyReturn(IOperation operation)
        => operation is IReturnOperation { ReturnedValue: null };

    private bool IsAssignmentFromParameter(IOperation operation, IParameterSymbol parameter)
        => operation switch
        {
            IConversionOperation conversion => IsAssignmentFromParameter(conversion.Operand, parameter),
            IParameterReferenceOperation parameterReference
                => SymbolEqualityComparer.Default.Equals(parameterReference.Parameter.OriginalDefinition, parameter.OriginalDefinition),
            _ => false
        };

    private Expression? RewriteBinderAssignmentTarget(IOperation target)
        => target switch
        {
            IFieldReferenceOperation fieldReference when IsCurrentComponentField(fieldReference.Field, fieldReference.Instance)
                => BuildStateAccess(fieldReference.Field),
            IPropertyReferenceOperation propertyReference when
                IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
                !IsParameterProperty(propertyReference.Property) &&
                !propertyReference.Property.IsIndexer &&
                IsAutoProperty(propertyReference.Property)
                    => BuildStateAccess(propertyReference.Property),
            IPropertyReferenceOperation propertyReference when
                IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
                IsParameterProperty(propertyReference.Property)
                    => throw new OperationTransformationException(
                        target,
                        "EventCallbackFactory.CreateBinder cannot bind to current-component parameter '" +
                        propertyReference.Property.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                        "' because RazorVue current-component parameters are lowered as read-only props."),
            _ => null
        };

    private Expression? RewriteEventCallbackHandler(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => RewriteEventCallbackHandler(conversion.Operand),
            IDelegateCreationOperation delegateCreation => RewriteEventCallbackHandler(delegateCreation.Target),
            IMethodReferenceOperation methodReference when IsCurrentComponentMethod(methodReference.Method, methodReference.Instance)
                => new Identifier(GetMemberName(methodReference.Method)),
            _ => null
        };

    private static bool IsEventCallbackFactoryCreate(IMethodSymbol method)
        => string.Equals(method.Name, "Create", StringComparison.Ordinal) &&
           string.Equals(
               method.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat),
               EventCallbackFactoryMetadataName,
               StringComparison.Ordinal);

    private static bool IsEventCallbackFactoryCreateBinder(IMethodSymbol method)
        => string.Equals(method.Name, "CreateBinder", StringComparison.Ordinal) &&
           string.Equals(
               method.ContainingNamespace?.ToDisplayString(),
               "Microsoft.AspNetCore.Components",
               StringComparison.Ordinal);

    private static bool IsCurrentComponentEventCallbackReceiver(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => IsCurrentComponentEventCallbackReceiver(conversion.Operand),
            IInstanceReferenceOperation
            {
                ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                    InstanceReferenceKind.ImplicitReceiver
            } => true,
            _ => false
        };

    private static OperationTransformationException CreateUnsupportedEventCallbackFactoryException(IInvocationOperation operation)
        => new(
            operation,
            "EventCallbackFactory.Create is supported by RazorVue current-component rewrite v1 only for current-component receivers and current-component method-group handlers.");

    private static OperationTransformationException CreateUnsupportedEventCallbackFactoryCreateBinderException(IInvocationOperation operation)
    {
        var receiverIndex = operation.Arguments.Length > 0 && operation.Arguments[0].Value is IFieldReferenceOperation ? 1 : 0;
        var handlerIndex = receiverIndex + 1;
        var handlerValue = operation.Arguments.Length > handlerIndex ? operation.Arguments[handlerIndex].Value : null;
        var handlerKind = handlerValue is not null
            ? handlerValue.Kind.ToString()
            : "<missing>";
        var handlerDetail = handlerValue is IConversionOperation conversion
            ? " Inner handler operation kind: " + conversion.Operand.Kind + "."
            : handlerValue is IDelegateCreationOperation delegateCreation
                ? " Delegate target operation kind: " +
                  delegateCreation.Target.Kind +
                  (delegateCreation.Target is IAnonymousFunctionOperation anonymousFunction
                      ? " Anonymous body operation kinds: [" +
                        string.Join(", ", anonymousFunction.Body.Operations.Select(static item => item.Kind.ToString())) +
                        "]."
                      : ".")
            : string.Empty;
        return new(
            operation,
            "EventCallbackFactory.CreateBinder is supported by RazorVue DOM @bind v1 only for current-component receivers and simple current-component state assignment lambdas, for example value => count = value. Handler operation kind: " +
            handlerKind +
            "." +
            handlerDetail);
    }

    private static bool IsEventCallbackInvoke(IMethodSymbol method)
    {
        if (method.Name is not ("Invoke" or "InvokeAsync") || method.IsStatic)
            return false;

        return IsEventCallbackType(method.ContainingType);
    }

    private IPropertySymbol? TryGetCurrentComponentEventCallbackParameter(IOperation? instance)
    {
        var property = UnwrapPropertyReference(instance);
        if (property is null ||
            !IsCurrentComponentProperty(property, GetPropertyInstance(instance)) ||
            !IsParameterProperty(property) ||
            !IsEventCallbackType(property.Type))
        {
            return null;
        }

        return property;
    }

    private static IPropertySymbol? UnwrapPropertyReference(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => UnwrapPropertyReference(conversion.Operand),
            IPropertyReferenceOperation propertyReference => propertyReference.Property,
            _ => null
        };

    private static IOperation? GetPropertyInstance(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => GetPropertyInstance(conversion.Operand),
            IPropertyReferenceOperation propertyReference => propertyReference.Instance,
            _ => null
        };

    private static bool IsEventCallbackType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var original = namedType.OriginalDefinition;
        var display = original.ToDisplayString(Format.NameFormat);
        // NameFormat expands open generics as EventCallback<TValue>, not EventCallback`1.
        return string.Equals(display, EventCallbackMetadataName, StringComparison.Ordinal) ||
               string.Equals(display, EventCallbackOfTMetadataName, StringComparison.Ordinal) ||
               string.Equals(display, "Microsoft.AspNetCore.Components.EventCallback<TValue>", StringComparison.Ordinal) ||
               (string.Equals(original.Name, "EventCallback", StringComparison.Ordinal) &&
                string.Equals(
                    original.ContainingNamespace?.ToDisplayString(),
                    "Microsoft.AspNetCore.Components",
                    StringComparison.Ordinal));
    }

    private Expression RewriteEventCallbackInvoke(IPropertySymbol parameter, IReadOnlyList<Expression> arguments)
    {
        // EventCallback parameters lower as optional Vue listener props: props.onX?.(args...)
        return new CallExpression(
            BuildPropsAccess(parameter),
            NodeList.From(arguments),
            optional: true);
    }

    private bool IsStateHasChangedInvocation(IMethodSymbol method, IOperation? instance)
    {
        if (!string.Equals(method.Name, "StateHasChanged", StringComparison.Ordinal) ||
            method.Parameters.Length != 0 ||
            method.IsStatic)
        {
            return false;
        }

        if (!IsCurrentComponentReceiver(instance) && instance is not null)
            return false;

        var containingType = method.ContainingType?.OriginalDefinition;
        if (containingType is null)
            return false;

        return SymbolEqualityComparer.Default.Equals(containingType, _componentType.OriginalDefinition) ||
               string.Equals(
                   containingType.ToDisplayString(Format.NameFormat),
                   ComponentBaseMetadataName,
                   StringComparison.Ordinal);
    }

    private static Expression RewriteStateHasChanged(IInvocationOperation operation)
    {
        if (operation.Arguments.Length != 0)
        {
            throw new OperationTransformationException(
                operation,
                "StateHasChanged is supported by RazorVue current-component rewrite v1 only as a parameterless call that maps to the setup-scoped stateHasChanged invalidator.");
        }

        return new CallExpression(
            new Identifier(StateHasChangedRuntimeName),
            NodeList.From<Expression>(),
            optional: false);
    }

    private bool IsComponentBaseInvokeAsyncInvocation(IMethodSymbol method, IOperation? instance)
    {
        if (!string.Equals(method.Name, "InvokeAsync", StringComparison.Ordinal) ||
            method.Parameters.Length != 1 ||
            method.IsStatic)
        {
            return false;
        }

        if (!IsCurrentComponentReceiver(instance) && instance is not null)
            return false;

        // Component-declared InvokeAsync overloads stay on the normal
        // current-component method path; only the ComponentBase dispatcher maps
        // to the setup-scoped invokeAsync helper.
        return string.Equals(
            method.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat),
            ComponentBaseMetadataName,
            StringComparison.Ordinal);
    }

    private static Expression RewriteInvokeAsync(IInvocationOperation operation, IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != 1)
        {
            throw new OperationTransformationException(
                operation,
                "ComponentBase.InvokeAsync is supported by RazorVue current-component rewrite v1 only as a single work-item call that maps to the setup-scoped invokeAsync dispatcher.");
        }

        return new CallExpression(
            new Identifier(InvokeAsyncRuntimeName),
            NodeList.From(arguments),
            optional: false);
    }

    private bool IsUnsupportedIndirectCurrentComponentDispatch(IMethodSymbol method, IOperation? instance)
        => !IsStateHasChangedInvocation(method, instance) &&
           !IsComponentBaseInvokeAsyncInvocation(method, instance) &&
           !IsCurrentComponentMethod(method, instance) &&
           IsCurrentComponentReceiver(instance);

    private static bool IsCurrentComponentReceiver(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => IsCurrentComponentReceiver(conversion.Operand),
            IInstanceReferenceOperation
            {
                ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                    InstanceReferenceKind.ImplicitReceiver
            } => true,
            _ => false
        };

    private static OperationTransformationException CreateUnsupportedIndirectCurrentComponentDispatchException(
        IOperation operation,
        IMethodSymbol method,
        IOperation? instance)
    {
        var receiverType = instance?.Type?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>";
        return new OperationTransformationException(
            operation,
            "Indirect current-component dispatch through static receiver type '" +
            receiverType +
            "' is not supported by RazorVue current-component rewrite v1 for member '" +
            method.OriginalDefinition.ToDisplayString(Format.NameFormat) +
            "'. Call the current-component member directly, or pass a direct current-component method group, so lowering can bind it to the render-function closure.");
    }

    private bool IsCurrentComponentMethod(IMethodSymbol method, IOperation? instance)
        => IsDeclaredOnCurrentComponent(method) &&
           method.MethodKind is MethodKind.Ordinary &&
           IsCurrentComponentInstance(method.IsStatic, instance);

    private bool IsCurrentComponentField(IFieldSymbol field, IOperation? instance)
        => IsDeclaredOnCurrentComponent(field) &&
           field.AssociatedSymbol is null &&
           !field.IsConst &&
           IsCurrentComponentInstance(field.IsStatic, instance);

    private bool IsCurrentComponentProperty(IPropertySymbol property, IOperation? instance)
        => IsDeclaredOnCurrentComponent(property) &&
           IsCurrentComponentInstance(property.IsStatic, instance);

    private bool IsDeclaredOnCurrentComponent(ISymbol symbol)
        => SymbolEqualityComparer.Default.Equals(
            symbol.ContainingType?.OriginalDefinition,
            _componentType.OriginalDefinition);

    private static bool IsCurrentComponentInstance(bool isStatic, IOperation? instance)
    {
        if (isStatic)
            return instance is null;

        return instance is null ||
               instance is IInstanceReferenceOperation
               {
                   ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                       InstanceReferenceKind.ImplicitReceiver
               };
    }

    private static bool IsParameterProperty(IPropertySymbol property)
        => property
            .GetAttributes()
            .Any(static attribute => string.Equals(
                attribute.AttributeClass?.OriginalDefinition.ToDisplayString(Format.NameFormat),
                ParameterAttributeMetadataName,
                StringComparison.Ordinal));

    private static bool IsAutoProperty(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.ExpressionBody is not null ||
                declaration.AccessorList is null)
            {
                return false;
            }

            foreach (var accessor in declaration.AccessorList.Accessors)
            {
                if (accessor.Body is not null || accessor.ExpressionBody is not null)
                    return false;
            }

            return true;
        }

        return false;
    }

    private Expression BuildStateAccess(ISymbol symbol)
        => BuildRuntimeAccess(_stateIdentifier, symbol);

    private Expression BuildPropsAccess(ISymbol symbol)
    {
        var runtimeName = GetParameterRuntimeName(symbol);
        return IsJavaScriptIdentifierName(runtimeName)
            ? new MemberExpression(
                new Identifier(_propsIdentifier),
                new Identifier(runtimeName),
                computed: false,
                optional: false)
            : new MemberExpression(
                new Identifier(_propsIdentifier),
                new StringLiteral(runtimeName, $"\"{EscapeJavaScriptString(runtimeName)}\""),
                computed: true,
                optional: false);
    }

    private static Expression BuildRuntimeAccess(string runtimeObjectName, ISymbol symbol)
        => new MemberExpression(
            new Identifier(runtimeObjectName),
            new Identifier(GetMemberName(symbol)),
            computed: false,
            optional: false);

    private static string GetMemberName(ISymbol symbol)
        => Util.GetConfigOrSymbolName(symbol);

    private string GetParameterRuntimeName(ISymbol symbol)
        => _parameterRuntimeNames is not null &&
           _parameterRuntimeNames.TryGetValue(symbol.Name, out var runtimeName) &&
           !string.IsNullOrWhiteSpace(runtimeName)
            ? runtimeName
            : GetMemberName(symbol);

    private static bool IsJavaScriptIdentifierName(string value)
    {
        if (string.IsNullOrEmpty(value) ||
            !(char.IsLetter(value[0]) || value[0] == '_' || value[0] == '$'))
        {
            return false;
        }

        for (var index = 1; index < value.Length; index++)
        {
            var ch = value[index];
            if (!(char.IsLetterOrDigit(ch) || ch == '_' || ch == '$'))
                return false;
        }

        return true;
    }

    private static string EscapeJavaScriptString(string value)
        => value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
}
