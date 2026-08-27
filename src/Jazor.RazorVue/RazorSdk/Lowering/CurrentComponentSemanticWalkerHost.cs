using System;
using System.Collections.Generic;
using System.Linq;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Host seam for the first RazorVue current-component lowering slice.
/// It rewrites members declared on the current component into the explicit
/// component runtime surface used by render-function framing:
/// fields/non-parameter properties read from <c>state</c>, parameters normally read from
/// <c>props</c> (or state for a ParameterView adapter), and current-component method references lower to stable module
/// function identifiers.
/// </summary>
/// <remarks>
/// 当前组件不是普通 JavaScript class 实例：状态、参数和方法分别属于不同的运行时载体。
/// 这里集中处理这种投影，避免在各个普通成员 visitor 中散落 RazorVue 特殊判断。
/// </remarks>
internal sealed class CurrentComponentSemanticWalkerHost : SemanticWalkerHost
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string EventCallbackFactoryMetadataName = "Microsoft.AspNetCore.Components.EventCallbackFactory";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private const string ComponentBaseMetadataName = "Microsoft.AspNetCore.Components.ComponentBase";
    private const string ParameterViewMetadataName = "Microsoft.AspNetCore.Components.ParameterView";
    private const string NavigationManagerMetadataName = "Microsoft.AspNetCore.Components.NavigationManager";
    private const string StateHasChangedRuntimeName = "stateHasChanged";
    private const string InvokeAsyncRuntimeName = "invokeAsync";
    private const string ParameterAdapterRuntimeName = "parameterAdapter";
    private readonly INamedTypeSymbol _componentType;
    private readonly string _stateIdentifier;
    private readonly string _propsIdentifier;
    private readonly IReadOnlyDictionary<string, string>? _parameterRuntimeNames;
    private readonly IReadOnlyDictionary<ISymbol, string>? _memberRuntimeNames;
    private readonly bool _parameterPropertiesUseState;
    private readonly Func<IParameterReferenceOperation, SenseArgument, Expression?>? _parameterReferenceRewriter;
    private readonly Func<ILocalReferenceOperation, SenseArgument, Expression?>? _localReferenceRewriter;
    private readonly Func<IPropertyReferenceOperation, SenseArgument, Expression?>? _propertyReferenceRewriter;
    private readonly Action<Expression, DirectBinderValueKind>? _directBinderHandlerObserver;

    public CurrentComponentSemanticWalkerHost(
        INamedTypeSymbol componentType,
        string stateIdentifier = "state",
        string propsIdentifier = "props",
        IReadOnlyDictionary<string, string>? parameterRuntimeNames = null,
        IReadOnlyDictionary<ISymbol, string>? memberRuntimeNames = null,
        bool parameterPropertiesUseState = false,
        Func<IParameterReferenceOperation, SenseArgument, Expression?>? parameterReferenceRewriter = null,
        Func<ILocalReferenceOperation, SenseArgument, Expression?>? localReferenceRewriter = null,
        Func<IPropertyReferenceOperation, SenseArgument, Expression?>? propertyReferenceRewriter = null,
        Action<Expression, DirectBinderValueKind>? directBinderHandlerObserver = null)
    {
        _componentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        if (string.IsNullOrWhiteSpace(stateIdentifier))
            throw new ArgumentException("State identifier cannot be empty.", nameof(stateIdentifier));
        if (string.IsNullOrWhiteSpace(propsIdentifier))
            throw new ArgumentException("Props identifier cannot be empty.", nameof(propsIdentifier));

        _stateIdentifier = stateIdentifier;
        _propsIdentifier = propsIdentifier;
        _parameterRuntimeNames = parameterRuntimeNames;
        _memberRuntimeNames = memberRuntimeNames;
        _parameterPropertiesUseState = parameterPropertiesUseState;
        _parameterReferenceRewriter = parameterReferenceRewriter;
        _localReferenceRewriter = localReferenceRewriter;
        _propertyReferenceRewriter = propertyReferenceRewriter;
        _directBinderHandlerObserver = directBinderHandlerObserver;
    }

    public override Expression? RewriteParameterReference(
        IParameterReferenceOperation operation,
        SenseArgument argument)
        => _parameterReferenceRewriter?.Invoke(operation, argument);

    public override Expression? RewriteLocalReference(
        ILocalReferenceOperation operation,
        SenseArgument argument)
        => _localReferenceRewriter?.Invoke(operation, argument);

    public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
    {
        // Claim protocol helpers before generic member lowering. A claimed unsupported dispatch
        // must fail here rather than fall through as a normal JavaScript member invocation.
        // 先处理组件协议调用，防止不支持的间接 dispatch 被静默降级成普通 JS 调用。
        if (IsEventCallbackFactoryCreateBinder(operation.TargetMethod))
            return RewriteEventCallbackFactoryCreateBinder(operation, argument);

        if (IsEventCallbackFactoryCreate(operation.TargetMethod))
            return RewriteEventCallbackFactoryCreate(operation, argument);

        if (IsStateHasChangedInvocation(operation.TargetMethod, operation.Instance))
            return RewriteStateHasChanged(operation);

        // The normal compiler cannot materialize ComponentBase or ParameterView. In adapter
        // mode these two calls are explicit runtime protocol seams, so claim them before the
        // generic current-component dispatch guard can report a misleading indirect-call error.
        // ParameterView 只在 adapter 定义的两个入口穿过 compiler，其他 API 不得静默降级。
        if (_parameterPropertiesUseState &&
            (IsComponentBaseSetParametersAsyncInvocation(operation.TargetMethod) ||
             IsParameterViewSetParameterPropertiesInvocation(operation)))
        {
            return null;
        }

        if (IsUnsupportedIndirectCurrentComponentDispatch(operation.TargetMethod, operation.Instance))
            throw CreateUnsupportedIndirectCurrentComponentDispatchException(operation, operation.TargetMethod, operation.Instance);

        return null;
    }

    public override Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
        // SemanticWalker already lowered receiver/arguments once. These rewrites only choose
        // the Vue runtime carrier, preserving the expression evaluation performed by core lowering.
        // 此 hook 仅选择 state/props/setup 载体，不重复计算已由 compiler 降低的参数。
        if (IsStateHasChangedInvocation(operation.TargetMethod, operation.Instance))
            return RewriteStateHasChanged(operation);

        if (IsComponentBaseInvokeAsyncInvocation(operation.TargetMethod, operation.Instance))
            return RewriteInvokeAsync(operation, arguments);

        if (_parameterPropertiesUseState && IsComponentBaseSetParametersAsyncInvocation(operation.TargetMethod))
            return RewriteComponentBaseSetParametersAsync(operation, arguments);

        if (_parameterPropertiesUseState && IsParameterViewSetParameterPropertiesInvocation(operation))
            return RewriteParameterViewSetParameterProperties(operation, instance);

        if (IsRazorRuntimeHelpersTypeCheck(operation.TargetMethod))
            return RewriteRazorRuntimeHelpersTypeCheck(operation, arguments);

        if (IsRazorRuntimeHelpersInvokeAsynchronousDelegate(operation.TargetMethod))
            return RewriteRazorRuntimeHelpersInvokeAsynchronousDelegate(operation, arguments);

        if (IsSingleValueBindConverterFormatValue(operation) && arguments.Count > 0)
            return arguments[0];

        // VisitInvocation calls this before RejectUnsupportedRuntimeFallback.
        if (IsEventCallbackInvoke(operation.TargetMethod))
        {
            if (TryGetCurrentComponentEventCallbackParameter(operation.Instance) is IPropertySymbol eventCallbackParameter)
                return RewriteEventCallbackInvoke(eventCallbackParameter, arguments);

            if (instance is not null)
                return RewriteEventCallbackInvoke(instance, arguments);
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

    public override Expression? RewriteEventAssignment(
        IEventAssignmentOperation operation,
        SenseArgument argument)
    {
        if (operation.EventReference is not IEventReferenceOperation eventReference ||
            !IsNavigationManagerEvent(eventReference.Event, eventReference.Instance))
        {
            return null;
        }

        // NavigationManager.LocationChanged and NavigationManager.OnNotFound use custom CLR
        // accessors, so neither can enter the field-like event protocol. Project only these
        // host-owned events to the browser service; all other external events keep the normal
        // explicit failure.
        // 它们是 host-owned custom event，不能伪装成 CLR field-like event。
        var eventName = eventReference.Event.Name;
        var walker = new SemanticWalker(true)
        {
            Host = this
        };
        var receiver = walker.Visit(eventReference.Instance!, argument) as Expression
            ?? throw new OperationTransformationException(
                operation,
                $"NavigationManager.{eventName} receiver could not be lowered to the browser service.");
        var handler = RewriteEventCallbackHandler(operation.HandlerValue, argument)
            ?? throw new OperationTransformationException(
                operation,
                $"NavigationManager.{eventName} requires a component method or lambda handler that RazorVue can lower.");
        var accessor = (operation.Adds ? "add" : "remove") + eventName;

        return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
            [
                ("$navigation", receiver),
                ("$handler", handler)
            ],
            values => new CallExpression(
                new MemberExpression(
                    values[0],
                    new Identifier(accessor),
                    computed: false,
                    optional: false),
                NodeList.From<Expression>([values[1]]),
                optional: false));
    }

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
            ? operation.Field.IsStatic
                ? new Identifier(GetMemberName(operation.Field))
                : BuildStateAccess(operation.Field)
            : null;

    public override Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
        if (!IsCurrentComponentProperty(operation.Property, operation.Instance))
            return null;

        if (_propertyReferenceRewriter?.Invoke(operation, argument) is { } projected)
            return projected;

        if (operation.Property.IsIndexer || arguments.Count > 0)
        {
            throw new OperationTransformationException(
                operation,
                $"Current-component indexed property '{operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported by RazorVue current-component rewrite v1.");
        }

        // Parameters live on props; instance auto-properties live in reactive state. Static
        // properties have module lifetime and must stay lexical, even when the setup factory is
        // invoked for more than one component instance.
        // [Parameter]、实例 auto-property 与 static property 的生命周期载体必须分开。
        if (IsParameterProperty(operation.Property))
            return BuildParameterAccess(operation.Property);

        if (operation.Property.IsStatic)
        {
            if (IsAutoProperty(operation.Property) &&
                GetBackingField(operation.Property) is { } backingField)
            {
                return new Identifier(GetMemberName(backingField));
            }

            ISymbol accessor = operation.Property.GetMethod is { } getter
                ? getter
                : operation.Property;
            return new CallExpression(
                new Identifier(GetMemberName(accessor)),
                NodeList.Empty<Expression>(),
                optional: false);
        }

        return IsAutoProperty(operation.Property)
            ? BuildStateAccess(operation.Property)
            : new CallExpression(
                new Identifier(GetMemberName(operation.Property)),
                NodeList.Empty<Expression>(),
                optional: false);
    }

    public override Expression? RewriteSimpleAssignmentPreorder(ISimpleAssignmentOperation operation, SenseArgument argument)
    {
        if (operation.Target is IPropertyReferenceOperation propertyReference &&
            IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
            IsParameterProperty(propertyReference.Property) &&
            !_parameterPropertiesUseState)
        {
            throw new OperationTransformationException(
                operation,
                "Current-component parameter '" +
                propertyReference.Property.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                "' is decorated with Microsoft.AspNetCore.Components.ParameterAttribute and is lowered as read-only props in RazorVue current-component rewrite v1.");
        }

        return null;
    }

    public override Expression? RewriteSimpleAssignmentPostorder(
        ISimpleAssignmentOperation operation,
        SenseArgument argument,
        Expression value)
    {
        if (operation.Target is not IPropertyReferenceOperation propertyReference ||
            propertyReference.Property.SetMethod is null ||
            propertyReference.Property.IsIndexer ||
            propertyReference.Arguments.Length != 0 ||
            !IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) ||
            (!_parameterPropertiesUseState && IsParameterProperty(propertyReference.Property)) ||
            (!propertyReference.Property.IsStatic && !IsAutoProperty(propertyReference.Property)))
        {
            return null;
        }

        // The RHS was lowered by SemanticWalker before this hook, so the projection preserves
        // its original evaluation semantics. Static auto-properties assign their module lexical
        // backing binding; computed static properties call their lowered setter.
        if (propertyReference.Property.IsStatic)
        {
            if (IsAutoProperty(propertyReference.Property) &&
                GetBackingField(propertyReference.Property) is { } backingField)
            {
                return new AssignmentExpression(
                    Operator.Assignment,
                    new Identifier(GetMemberName(backingField)),
                    value);
            }

            return new CallExpression(
                new Identifier(GetMemberName(propertyReference.Property.SetMethod)),
                NodeList.From<Expression>(value),
                optional: false);
        }

        return new AssignmentExpression(Operator.Assignment, BuildStateAccess(propertyReference.Property), value);
    }

    private Expression RewriteEventCallbackFactoryCreate(IInvocationOperation operation, SenseArgument argument)
    {
        if (operation.Arguments.Length < 2)
            throw CreateUnsupportedEventCallbackFactoryException(operation);

        if (!IsCurrentComponentEventCallbackReceiver(operation.Arguments[0].Value))
            throw CreateUnsupportedEventCallbackFactoryException(operation);

        return RewriteEventCallbackHandler(operation.Arguments[1].Value, argument)
            ?? throw CreateUnsupportedEventCallbackFactoryException(operation);
    }

    private Expression RewriteEventCallbackFactoryCreateBinder(
        IInvocationOperation operation,
        SenseArgument argument)
    {
        if (!TryGetCreateBinderReceiverAndHandler(operation, out var receiver, out var handler))
            throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);

        if (!IsCurrentComponentEventCallbackReceiver(receiver))
            throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);

        var rewritten = RewriteBinderHandler(handler, argument)
            ?? throw CreateUnsupportedEventCallbackFactoryCreateBinderException(operation);
        if (HasOnlyDefaultBinderOptions(operation) &&
            TryGetDirectBinderValueKind(handler, out var valueKind))
            _directBinderHandlerObserver?.Invoke(rewritten, valueKind);

        return rewritten;
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
        // The minimum arity check above covers both layouts: handlerIndex is always 1 or 2.
        // 不要在这里再保留等价长度判断，否则会掩盖 Razor SG binder protocol 的真实变化。

        receiver = operation.Arguments[receiverIndex].Value;
        handler = operation.Arguments[handlerIndex].Value;
        return true;
    }

    private Expression? RewriteBinderHandler(IOperation operation, SenseArgument argument)
        => operation switch
        {
            IConversionOperation conversion => RewriteBinderHandler(conversion.Operand, argument),
            IDelegateCreationOperation delegateCreation => RewriteBinderHandler(delegateCreation.Target, argument),
            // Razor SDK uses this generic-inference wrapper for @bind:after and
            // @bind:set delegates. The outer helper has no runtime role in the
            // Vue event-handler protocol.
            IInvocationOperation invocation when TryGetInferredBindSetterHandler(invocation, out var inferredHandler)
                => RewriteInferredBindSetterHandler(inferredHandler, argument),
            IAnonymousFunctionOperation anonymousFunction => RewriteBinderHandler(anonymousFunction, argument),
            _ => null
        };

    private static bool HasOnlyDefaultBinderOptions(IInvocationOperation operation)
    {
        var receiverIndex = operation.Arguments[0].Value is IFieldReferenceOperation ? 1 : 0;
        var handlerIndex = receiverIndex + 1;
        var currentValueIndex = handlerIndex + 1;
        return operation.Arguments.Length > currentValueIndex &&
               operation.Arguments
                   .Skip(currentValueIndex + 1)
                   .All(static option => option.ArgumentKind == ArgumentKind.DefaultValue);
    }

    private static bool TryGetDirectBinderValueKind(
        IOperation operation,
        out DirectBinderValueKind valueKind)
    {
        switch (operation)
        {
            case IConversionOperation conversion:
                return TryGetDirectBinderValueKind(conversion.Operand, out valueKind);
            case IDelegateCreationOperation delegateCreation:
                return TryGetDirectBinderValueKind(delegateCreation.Target, out valueKind);
            case IAnonymousFunctionOperation anonymousFunction:
                return TryGetDirectBinderValueKind(anonymousFunction, out valueKind);
            default:
                valueKind = DirectBinderValueKind.None;
                return false;
        }
    }

    private static bool TryGetDirectBinderValueKind(
        IAnonymousFunctionOperation anonymousFunction,
        out DirectBinderValueKind valueKind)
    {
        if (anonymousFunction.Symbol.Parameters.Length != 1 ||
            TryGetSingleBinderAssignment(anonymousFunction.Body) is not ISimpleAssignmentOperation assignment)
        {
            valueKind = DirectBinderValueKind.None;
            return false;
        }

        var parameter = anonymousFunction.Symbol.Parameters[0];
        if (!IsAssignmentFromParameter(assignment.Value, parameter))
        {
            valueKind = DirectBinderValueKind.None;
            return false;
        }

        // This fact is intentionally narrower than general CreateBinder support. Only the
        // exact single assignment recognized by Roslyn may be fused with target.value/checked;
        // inferred setters, conversions and callbacks retain the generic adapter.
        // 该标记只证明“参数直接写入目标”，不能从最终 JS lambda 形状反推复杂 bind 安全性。
        valueKind = parameter.Type.SpecialType switch
        {
            SpecialType.System_String => DirectBinderValueKind.String,
            SpecialType.System_Boolean => DirectBinderValueKind.Boolean,
            _ => DirectBinderValueKind.None
        };
        return valueKind != DirectBinderValueKind.None;
    }

    private Expression? RewriteInferredBindSetterHandler(IOperation operation, SenseArgument argument)
        => operation switch
        {
            IConversionOperation conversion => RewriteInferredBindSetterHandler(conversion.Operand, argument),
            IDelegateCreationOperation delegateCreation => RewriteInferredBindSetterHandler(delegateCreation.Target, argument),
            IAnonymousFunctionOperation anonymousFunction => RewriteEventCallbackLambdaHandler(anonymousFunction, argument),
            IMethodReferenceOperation methodReference => RewriteInferredBindSetterMethodReference(methodReference, argument),
            _ => null
        };

    private Expression? RewriteInferredBindSetterMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument)
    {
        if (!IsCurrentComponentMethod(operation.Method, operation.Instance) ||
            operation.Method.Parameters.Length != 1 ||
            operation.Method.Parameters[0].RefKind != RefKind.None)
        {
            return null;
        }

        var walker = new SemanticWalker(true)
        {
            Host = this
        };
        var callback = walker.Visit(operation, argument) as Expression;
        if (callback is null)
            return null;

        // Razor SDK CreateInferredBindSetter accepts Action<T> and Func<T, Task>.
        // The wrapper is protocol framing only; SemanticWalker still lowers the
        // method group so its ordinary C# call semantics are retained.
        var value = new Identifier("__value");
        return new ArrowFunctionExpression(
            NodeList.From<Node>(value),
            new CallExpression(
                callback,
                NodeList.From<Expression>(value),
                optional: false),
            expression: true,
            async: false);
    }

    private static bool TryGetInferredBindSetterHandler(
        IInvocationOperation operation,
        out IOperation handler)
    {
        handler = operation;
        var method = operation.TargetMethod.OriginalDefinition;
        if (!IsRazorRuntimeHelpersMethod(method, "CreateInferredBindSetter") ||
            method.Parameters.Length != 2 ||
            operation.Arguments.Length != 2)
        {
            return false;
        }

        handler = operation.Arguments[0].Value;
        return true;
    }

    private Expression? RewriteBinderHandler(
        IAnonymousFunctionOperation anonymousFunction,
        SenseArgument argument)
    {
        if (anonymousFunction.Symbol.Parameters.Length != 1 ||
            TryGetSingleBinderAssignment(anonymousFunction.Body) is not ISimpleAssignmentOperation assignment)
        {
            return null;
        }

        var parameter = anonymousFunction.Symbol.Parameters[0];
        if (!IsAssignmentFromParameter(assignment.Value, parameter))
            return null;

        var assignmentTarget = RewriteBinderAssignmentTarget(assignment.Target, argument);
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

    private static bool IsAssignmentFromParameter(IOperation operation, IParameterSymbol parameter)
        => operation switch
        {
            IConversionOperation conversion => IsAssignmentFromParameter(conversion.Operand, parameter),
            IParameterReferenceOperation parameterReference
                => SymbolEqualityComparer.Default.Equals(parameterReference.Parameter.OriginalDefinition, parameter.OriginalDefinition),
            _ => false
        };

    private Expression? RewriteBinderAssignmentTarget(
        IOperation target,
        SenseArgument argument)
        => target switch
        {
            IFieldReferenceOperation fieldReference when IsCurrentComponentField(fieldReference.Field, fieldReference.Instance)
                => BuildStateAccess(fieldReference.Field),
            IPropertyReferenceOperation propertyReference when
                IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
                (!IsParameterProperty(propertyReference.Property) || _parameterPropertiesUseState) &&
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
            IPropertyReferenceOperation propertyReference when
                propertyReference.Property.SetMethod is not null &&
                !propertyReference.Property.IsStatic &&
                !propertyReference.Property.IsIndexer &&
                propertyReference.Arguments.Length == 0
                    => RewriteWritableCapturedPropertyTarget(propertyReference, argument),
            _ => null
        };

    private Expression? RewriteWritableCapturedPropertyTarget(
        IPropertyReferenceOperation propertyReference,
        SenseArgument argument)
    {
        // Razor emits CreateBinder(value => captured.Property = value, currentValue) for a
        // foreach item. Let the compiler lower that member reference so the active direct-render
        // lexical aliases still resolve to the mapper parameter rather than a C# local name.
        var walker = new SemanticWalker(true)
        {
            Host = this
        };
        return walker.Visit(propertyReference, argument) as Expression;
    }

    private Expression? RewriteEventCallbackHandler(IOperation operation, SenseArgument argument)
        => operation switch
        {
            IConversionOperation conversion => RewriteEventCallbackHandler(conversion.Operand, argument),
            IDelegateCreationOperation delegateCreation => RewriteEventCallbackHandler(delegateCreation.Target, argument),
            // Official Razor SG wraps component-bind callbacks in RuntimeHelpers.TypeCheck<T>.
            // Handler classification must erase that compile-time wrapper before inspecting the lambda.
            IInvocationOperation invocation when
                IsRazorRuntimeHelpersTypeCheck(invocation.TargetMethod) &&
                invocation.Arguments.Length == 1
                    => RewriteEventCallbackHandler(invocation.Arguments[0].Value, argument),
            // Component @bind:after can wrap the callback with
            // CreateInferredBindSetter before the EventCallback inference wrapper.
            // Its lambda owns the required assignment-then-after-callback ordering.
            IInvocationOperation invocation when
                TryGetInferredBindSetterHandler(invocation, out var inferredBindSetterHandler)
                    => RewriteInferredBindSetterHandler(inferredBindSetterHandler, argument),
            // Component @bind uses CreateInferredEventCallback(receiver, callback, value)
            // solely to carry generic inference through generated C#.
            IInvocationOperation invocation when
                TryGetInferredEventCallbackHandler(invocation, out var inferredHandler)
                    => RewriteEventCallbackHandler(inferredHandler, argument),
            IConditionalOperation conditional => RewriteConditionalEventCallbackHandler(conditional, argument),
            IAnonymousFunctionOperation anonymousFunction
                => RewriteBinderHandler(anonymousFunction, argument) ??
                   RewriteEventCallbackLambdaHandler(anonymousFunction, argument),
            IMethodReferenceOperation methodReference when IsCurrentComponentMethod(methodReference.Method, methodReference.Instance)
                => new Identifier(GetMemberName(methodReference.Method)),
            IPropertyReferenceOperation propertyReference when
                IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
                IsParameterProperty(propertyReference.Property) &&
                IsEventCallbackType(propertyReference.Property.Type)
                    => BuildParameterAccess(propertyReference.Property),
            _ => null
        };

    private bool TryGetInferredEventCallbackHandler(
        IInvocationOperation operation,
        out IOperation handler)
    {
        handler = operation;
        var method = operation.TargetMethod.OriginalDefinition;
        if (!IsRazorRuntimeHelpersMethod(method, "CreateInferredEventCallback") ||
            method.Parameters.Length != 3 ||
            operation.Arguments.Length != 3 ||
            !IsCurrentComponentEventCallbackReceiver(operation.Arguments[0].Value))
        {
            return false;
        }

        handler = operation.Arguments[1].Value;
        return true;
    }

    private Expression? RewriteConditionalEventCallbackHandler(
        IConditionalOperation operation,
        SenseArgument argument)
    {
        if (operation.WhenFalse is null)
            return null;

        var walker = new SemanticWalker(true)
        {
            Host = this
        };
        var test = walker.Visit(operation.Condition, argument) as Expression;
        var consequent = RewriteEventCallbackHandler(operation.WhenTrue, argument);
        var alternate = RewriteEventCallbackHandler(operation.WhenFalse, argument);
        return test is not null && consequent is not null && alternate is not null
            ? new ConditionalExpression(test, consequent, alternate)
            : null;
    }

    private Expression? RewriteEventCallbackLambdaHandler(
        IAnonymousFunctionOperation anonymousFunction,
        SenseArgument argument)
    {
        var walker = new SemanticWalker(true)
        {
            Host = this
        };

        return walker.Visit(anonymousFunction, argument) as Expression;
    }

    private static bool IsEventCallbackFactoryCreate(IMethodSymbol method)
        => string.Equals(method.Name, "Create", StringComparison.Ordinal) &&
           string.Equals(
                method.ContainingType!.OriginalDefinition.ToDisplayString(Format.NameFormat),
               EventCallbackFactoryMetadataName,
               StringComparison.Ordinal);

    private static bool IsBindConverterFormatValue(IMethodSymbol method)
        => method.IsStatic &&
           string.Equals(method.Name, "FormatValue", StringComparison.Ordinal) &&
           method.ContainingType is { Name: "BindConverter" } containingType &&
           string.Equals(
                containingType.ContainingNamespace!.ToDisplayString(),
               "Microsoft.AspNetCore.Components",
               StringComparison.Ordinal);

    private static bool IsSingleValueBindConverterFormatValue(IInvocationOperation operation)
        => IsBindConverterFormatValue(operation.TargetMethod) &&
           operation.Arguments.Length > 0 &&
           operation.Arguments[0].ArgumentKind != ArgumentKind.DefaultValue &&
           operation.Arguments.Skip(1).All(static argument => argument.ArgumentKind == ArgumentKind.DefaultValue);

    private static bool IsEventCallbackFactoryCreateBinder(IMethodSymbol method)
        => string.Equals(method.Name, "CreateBinder", StringComparison.Ordinal) &&
           string.Equals(
                method.ContainingNamespace!.ToDisplayString(),
               "Microsoft.AspNetCore.Components",
               StringComparison.Ordinal);

    private bool IsCurrentComponentEventCallbackReceiver(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => IsCurrentComponentEventCallbackReceiver(conversion.Operand),
            IInstanceReferenceOperation
            {
                ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                    InstanceReferenceKind.ImplicitReceiver
            } instanceReference => IsComponentHierarchyType(instanceReference.Type),
            _ => false
        };

    private static OperationTransformationException CreateUnsupportedEventCallbackFactoryException(IInvocationOperation operation)
        => new(
            operation,
            "EventCallbackFactory.Create is supported by RazorVue current-component rewrite v1 only for current-component receivers and current-component method-group or simple state-assignment lambda handlers.");

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
            : handlerValue is IInvocationOperation invocation
                ? " Invocation target: " +
                  invocation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                  "."
            : string.Empty;
        return new(
            operation,
            "EventCallbackFactory.CreateBinder is supported by RazorVue DOM @bind v1 only for current-component receivers and either simple current-component state assignment lambdas, for example value => count = value, or the official Razor SDK CreateInferredBindSetter<T>(Func<T, Task>, T) protocol used by explicit binding features. Handler operation kind: " +
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
                    original.ContainingNamespace!.ToDisplayString(),
                    "Microsoft.AspNetCore.Components",
                    StringComparison.Ordinal));
    }

    private Expression RewriteEventCallbackInvoke(IPropertySymbol parameter, IReadOnlyList<Expression> arguments)
    {
        // EventCallback parameters lower as optional Vue listener props: props.onX?.(args...)
        return RewriteEventCallbackInvoke(BuildParameterAccess(parameter), arguments);
    }

    private static Expression RewriteEventCallbackInvoke(
        Expression callback,
        IReadOnlyList<Expression> arguments)
        => new CallExpression(
            callback,
            NodeList.From(arguments),
            optional: true);

    private bool IsStateHasChangedInvocation(IMethodSymbol method, IOperation? instance)
    {
        if (!string.Equals(method.Name, "StateHasChanged", StringComparison.Ordinal) ||
            method.Parameters.Length != 0 ||
            method.IsStatic)
        {
            return false;
        }

        // ComponentBase is intentionally outside the source-member projection, but its
        // lifecycle protocol still executes against this component's base receiver.
        if (!IsCurrentComponentReceiver(instance) &&
            !IsCurrentComponentBaseReceiver(instance) &&
            instance is not null)
            return false;

        // Invocation targets in a bound C# operation are always owned by a named type.
        // Roslyn 已完成绑定；这里不应把不可能的 symbol ownership 缺失当作普通 false。
        var containingType = method.ContainingType!.OriginalDefinition;

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

        if (!IsCurrentComponentReceiver(instance) &&
            !IsCurrentComponentBaseReceiver(instance) &&
            instance is not null)
            return false;

        // Component-declared InvokeAsync overloads stay on the normal
        // current-component method path; only the ComponentBase dispatcher maps
        // to the setup-scoped invokeAsync helper.
        return string.Equals(
            method.ContainingType!.OriginalDefinition.ToDisplayString(Format.NameFormat),
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

    private static bool IsComponentBaseSetParametersAsyncInvocation(IMethodSymbol method)
        => !method.IsStatic &&
           string.Equals(method.Name, "SetParametersAsync", StringComparison.Ordinal) &&
           method.Parameters.Length == 1 &&
           string.Equals(
               method.ContainingType!.OriginalDefinition.ToDisplayString(Format.NameFormat),
               ComponentBaseMetadataName,
               StringComparison.Ordinal);

    private static Expression RewriteComponentBaseSetParametersAsync(
        IInvocationOperation operation,
        IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != 1)
        {
            throw new OperationTransformationException(
                operation,
                "ComponentBase.SetParametersAsync is supported by RazorVue only for the standard single ParameterView argument shape.");
        }

        return new CallExpression(
            new MemberExpression(
                new Identifier(ParameterAdapterRuntimeName),
                new Identifier("applyComponentBaseParameters"),
                computed: false,
                optional: false),
            NodeList.From(arguments),
            optional: false);
    }

    private bool IsParameterViewSetParameterPropertiesInvocation(IInvocationOperation operation)
    {
        var method = operation.TargetMethod;
        if (method.IsStatic ||
            !string.Equals(method.Name, "SetParameterProperties", StringComparison.Ordinal) ||
            method.Parameters.Length != 1 ||
            !string.Equals(
                method.ContainingType!.OriginalDefinition.ToDisplayString(Format.NameFormat),
                ParameterViewMetadataName,
                StringComparison.Ordinal))
        {
            return false;
        }

        return operation.Arguments.Length == 1 &&
               IsCurrentComponentReceiver(operation.Arguments[0].Value);
    }

    private static Expression RewriteParameterViewSetParameterProperties(
        IInvocationOperation operation,
        Expression? instance)
    {
        if (instance is null)
        {
            throw new OperationTransformationException(
                operation,
                "ParameterView.SetParameterProperties requires the current ParameterView instance in RazorVue's component adapter.");
        }

        return new CallExpression(
            new MemberExpression(
                new Identifier(ParameterAdapterRuntimeName),
                new Identifier("applyParameterProperties"),
                computed: false,
                optional: false),
            NodeList.From<Expression>(instance),
            optional: false);
    }

    // The adapter names its accessors after the CLR event, so "add"/"remove" + event name matches
    // the browser service surface exactly for every event listed here.
    private bool IsNavigationManagerEvent(IEventSymbol eventSymbol, IOperation? instance)
        => eventSymbol.Name is "LocationChanged" or "OnNotFound" &&
           IsNavigationManagerType(eventSymbol.ContainingType) &&
           IsNavigationManagerReceiver(instance);

    private bool IsNavigationManagerReceiver(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation is IPropertyReferenceOperation propertyReference &&
               IsCurrentComponentProperty(propertyReference.Property, propertyReference.Instance) &&
               IsNavigationManagerType(propertyReference.Property.Type);
    }

    private static bool IsNavigationManagerType(ITypeSymbol? type)
        => type is INamedTypeSymbol namedType &&
           string.Equals(
               namedType.OriginalDefinition.ToDisplayString(Format.NameFormat),
               NavigationManagerMetadataName,
               StringComparison.Ordinal);

    private static Expression RewriteRazorRuntimeHelpersTypeCheck(
        IInvocationOperation operation,
        IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != 1)
        {
            throw new OperationTransformationException(
                operation,
                "RuntimeHelpers.TypeCheck is supported by RazorVue current-component rewrite only for the single-value Razor SG helper shape.");
        }

        return arguments[0];
    }

    private static bool IsRazorRuntimeHelpersInvokeAsynchronousDelegate(IMethodSymbol method)
        => IsRazorRuntimeHelpersMethod(method, "InvokeAsynchronousDelegate");

    private static Expression RewriteRazorRuntimeHelpersInvokeAsynchronousDelegate(
        IInvocationOperation operation,
        IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != 1)
        {
            throw new OperationTransformationException(
                operation,
                "RuntimeHelpers.InvokeAsynchronousDelegate is supported by RazorVue only for the official single-callback @bind:after helper shape.");
        }

        // The SDK helper invokes the supplied Action/Func<Task> exactly once.
        // Its argument has already passed through SemanticWalker, preserving the
        // source lambda's assignment-before-callback ordering.
        return new CallExpression(
            arguments[0],
            NodeList.From<Expression>(),
            optional: false);
    }

    private bool IsUnsupportedIndirectCurrentComponentDispatch(IMethodSymbol method, IOperation? instance)
        => !IsStateHasChangedInvocation(method, instance) &&
           !IsComponentBaseInvokeAsyncInvocation(method, instance) &&
           !(_parameterPropertiesUseState && IsComponentBaseSetParametersAsyncInvocation(method)) &&
           !IsRazorRuntimeHelpersTypeCheck(method) &&
           !IsCurrentComponentMethod(method, instance) &&
           IsCurrentComponentReceiver(instance);

    private static bool IsRazorRuntimeHelpersTypeCheck(IMethodSymbol method)
        => IsRazorRuntimeHelpersMethod(method, "TypeCheck");

    private static bool IsRazorRuntimeHelpersMethod(IMethodSymbol method, string methodName)
        => method.IsStatic &&
           string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
           method.ContainingType is { Name: "RuntimeHelpers" } containingType &&
           string.Equals(
                containingType.ContainingNamespace!.ToDisplayString(),
               "Microsoft.AspNetCore.Components.CompilerServices",
               StringComparison.Ordinal);

    private bool IsCurrentComponentReceiver(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => IsCurrentComponentReceiver(conversion.Operand),
            // An implicit receiver also occurs inside nested runtime classes. Only a receiver
            // whose static type belongs to the component hierarchy may use the state/props
            // projection; other classes must retain normal compiler member dispatch.
            IInstanceReferenceOperation
            {
                ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                    InstanceReferenceKind.ImplicitReceiver
            } instanceReference => IsComponentHierarchyType(instanceReference.Type),
            _ => false
        };

    private bool IsCurrentComponentBaseReceiver(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => IsCurrentComponentBaseReceiver(conversion.Operand),
            IInstanceReferenceOperation
            {
                ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                    InstanceReferenceKind.ImplicitReceiver
            } instanceReference => IsTypeInCurrentComponentBaseChain(instanceReference.Type),
            _ => false
        };

    private bool IsTypeInCurrentComponentBaseChain(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var originalType = namedType.OriginalDefinition;
        for (var current = _componentType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, originalType))
                return true;
        }

        return false;
    }

    private bool IsComponentHierarchyType(ITypeSymbol? type)
        => ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(
            _componentType,
            type as INamedTypeSymbol);

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
        => ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentType, symbol.ContainingType);

    private bool IsCurrentComponentInstance(bool isStatic, IOperation? instance)
    {
        if (isStatic)
            return instance is null;

        return instance is null || IsCurrentComponentReceiver(instance);
    }

    private static bool IsParameterProperty(IPropertySymbol property)
        => property
            .GetAttributes()
            .Any(static attribute => string.Equals(
                attribute.AttributeClass!.OriginalDefinition.ToDisplayString(Format.NameFormat),
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

    private static IFieldSymbol? GetBackingField(IPropertySymbol property)
        => property.ContainingType
            .GetMembers($"<{property.Name}>k__BackingField")
            .OfType<IFieldSymbol>()
            .FirstOrDefault();

    private Expression BuildStateAccess(ISymbol symbol)
        => BuildRuntimeAccess(_stateIdentifier, symbol);

    private Expression BuildPropsAccess(ISymbol symbol)
    {
        var runtimeName = GetParameterRuntimeName(symbol);
        return JavaScriptAstFactory.IsJavaScriptIdentifierName(runtimeName)
            ? new MemberExpression(
                new Identifier(_propsIdentifier),
                new Identifier(runtimeName),
                computed: false,
                optional: false)
            : new MemberExpression(
                new Identifier(_propsIdentifier),
                JavaScriptAstFactory.CreateStringLiteral(runtimeName),
                computed: true,
                optional: false);
    }

    private Expression BuildParameterAccess(ISymbol symbol)
        => _parameterPropertiesUseState
            ? BuildStateAccess(symbol)
            : BuildPropsAccess(symbol);

    private Expression BuildRuntimeAccess(string runtimeObjectName, ISymbol symbol)
    {
        var memberName = GetMemberName(symbol);
        return JavaScriptAstFactory.IsJavaScriptIdentifierName(memberName)
            ? new MemberExpression(
                new Identifier(runtimeObjectName),
                new Identifier(memberName),
                computed: false,
                optional: false)
            : new MemberExpression(
                new Identifier(runtimeObjectName),
                JavaScriptAstFactory.CreateStringLiteral(memberName),
                computed: true,
                optional: false);
    }

    private string GetMemberName(ISymbol symbol)
        => _memberRuntimeNames is not null &&
           _memberRuntimeNames.TryGetValue(symbol.OriginalDefinition, out var runtimeName) &&
           !string.IsNullOrWhiteSpace(runtimeName)
            ? runtimeName
            : Util.GetConfigOrSymbolName(symbol);

    private string GetParameterRuntimeName(ISymbol symbol)
        => _parameterRuntimeNames is not null &&
           _parameterRuntimeNames.TryGetValue(symbol.Name, out var runtimeName) &&
           !string.IsNullOrWhiteSpace(runtimeName)
            ? runtimeName
            : GetMemberName(symbol);

}

/// <summary>DOM carrier kind proven by Roslyn for a single-assignment CreateBinder lambda.</summary>
internal enum DirectBinderValueKind
{
    None,
    String,
    Boolean
}
