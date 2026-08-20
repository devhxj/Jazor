// File: SemanticWalker.cs.Event.cs
// Purpose: Lowers supported event add/remove and invocation operations through EventLowering.
// 事件语义走明确 runtime seam；没有映射的 CLR event 场景必须保持不支持而非猜测 JavaScript 行为。
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// Lowers source-declared field-like events through the member-class multicast protocol.
/// </summary>
/// <remarks>
/// An event is never emitted as an ordinary writable JS property. The runtime class owns a private
/// invocation list and exposes generated add/remove/snapshot helpers. A snapshot is essential:
/// C# captures an invocation list before handler execution, so subscription changes made by one
/// handler cannot alter the remainder of that raise.
/// </remarks>
public sealed partial class SemanticWalker
{
    private readonly record struct EventHandlerBinding(
        Expression Callback,
        Expression Receiver,
        bool CallbackUsesReceiver);

    public override Node? VisitEventReference(IEventReferenceOperation operation, SenseArgument argument)
    {
        if (Host?.RewriteEventReference(operation, argument) is Expression hostExpression)
            return WithOriginIfMissing(hostExpression, operation);

        if (!TryValidateEventProtocol(operation, operation.Event, out var failure))
            return HandleTransformationFailure<Node>(operation, failure);

        var instance = Translate<Expression>(operation.Instance!, argument);

        return WithOriginIfMissing(
            CreateEventProtocolCall(instance, EventLowering.GetSnapshotMethodName(operation.Event)),
            operation);
    }

    public override Node? VisitEventAssignment(IEventAssignmentOperation operation, SenseArgument argument)
    {
        if (Host?.RewriteEventAssignment(operation, argument) is Expression hostExpression)
            return WithOriginIfMissing(hostExpression, operation);

        // Roslyn types EventReference as IOperation on the public interface, but valid event
        // assignments always bind that child as IEventReferenceOperation.
        var eventReference = (IEventReferenceOperation)operation.EventReference;

        if (!TryValidateEventProtocol(operation, eventReference.Event, out var failure))
            return HandleTransformationFailure<Node>(operation, failure);

        var eventReceiver = Translate<Expression>(eventReference.Instance!, argument);

        var handler = BuildEventHandlerBinding(operation.HandlerValue, argument);
        var helperName = operation.Adds
            ? EventLowering.GetAddMethodName(eventReference.Event)
            : EventLowering.GetRemoveMethodName(eventReference.Event);

        // C# evaluates the event receiver before the handler expression and only dispatches the
        // accessor after both succeed. Nested IIFEs preserve that order and avoid duplicate reads
        // of a side-effecting method-group receiver without constructing JavaScript source text.
        var lowered = JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
            [("$eventTarget", eventReceiver)],
            eventTargets => BuildEventAssignmentAfterTarget(eventTargets[0], handler, helperName));
        return WithOriginIfMissing(lowered, operation);
    }

    private Expression BuildEventAssignmentAfterTarget(
        Identifier eventTarget,
        EventHandlerBinding handler,
        string helperName)
    {
        if (handler.CallbackUsesReceiver)
        {
            return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
                [("$eventHandlerTarget", handler.Receiver)],
                handlerTargets => CreateEventProtocolCall(
                    eventTarget,
                    helperName,
                    new MemberExpression(
                        handlerTargets[0],
                        handler.Callback,
                        computed: false,
                        optional: false),
                    handlerTargets[0]));
        }

        return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
            [
                ("$eventHandler", handler.Callback),
                ("$eventHandlerTarget", handler.Receiver)
            ],
            handlerValues => CreateEventProtocolCall(
                eventTarget,
                helperName,
                handlerValues[0],
                handlerValues[1]));
    }

    private EventHandlerBinding BuildEventHandlerBinding(IOperation operation, SenseArgument argument)
    {
        var value = UnwrapEventHandlerValue(operation);
        if (value is IBinaryOperation { Type.TypeKind: TypeKind.Delegate })
        {
            return HandleTransformationFailure<EventHandlerBinding>(
                operation,
                "Delegate combination/removal expressions are not supported as event handlers. Subscribe or unsubscribe each delegate separately.");
        }

        if (value is not IMethodReferenceOperation methodReference)
        {
            return new EventHandlerBinding(
                Translate<Expression>(operation, argument),
                Null,
                CallbackUsesReceiver: false);
        }

        if (methodReference.Method.MethodKind == MethodKind.LocalFunction)
        {
            // Capturing local functions are lexical declarations, but Roslyn still provides their
            // containing instance as the delegate target. Keep it as the invocation receiver so
            // snapshot apply() preserves the original this and -= can compare the same pair.
            var localReceiver = methodReference.Method.IsStatic || methodReference.Instance is null
                ? Null
                : Translate<Expression>(methodReference.Instance, argument);

            return new EventHandlerBinding(
                new Identifier(GetJavaScriptBindingName(methodReference.Method)),
                localReceiver,
                CallbackUsesReceiver: false);
        }

        if (methodReference.Method.IsStatic)
        {
            if (!IsModuleDeclaredEventHandlerMethod(methodReference.Method))
            {
                return HandleTransformationFailure<EventHandlerBinding>(
                    operation,
                    $"Static method-group handler '{methodReference.Method.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is outside the compiler-owned event protocol. Assign it to a delegate local before subscribing, or use a source-declared module method.");
            }

            return new EventHandlerBinding(
                Translate<Expression>(methodReference, argument),
                Null,
                CallbackUsesReceiver: false);
        }

        if (!IsModuleDeclaredEventHandlerMethod(methodReference.Method))
        {
            return HandleTransformationFailure<EventHandlerBinding>(
                operation,
                $"Instance method-group handler '{methodReference.Method.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is outside the compiler-owned event protocol. Assign it to a stable delegate local before subscribing.");
        }

        // A non-static IMethodReferenceOperation always has a bound receiver.
        var instance = methodReference.Instance!;
        var methodName = GetCurrentModuleDeclaredOrConfigName(methodReference.Method);
        if (IsBaseInstanceReference(instance))
        {
            return new EventHandlerBinding(
                new MemberExpression(new Super(), new Identifier(methodName), computed: false, optional: false),
                new ThisExpression(),
                CallbackUsesReceiver: false);
        }

        var methodReceiver = Translate<Expression>(instance, argument);

        return new EventHandlerBinding(
            new Identifier(methodName),
            methodReceiver,
            CallbackUsesReceiver: true);
    }

    private static IOperation UnwrapEventHandlerValue(IOperation operation)
    {
        while (true)
        {
            switch (operation)
            {
                case IDelegateCreationOperation delegateCreation:
                    operation = delegateCreation.Target;
                    continue;
                default:
                    return operation;
            }
        }
    }

    private static string? GetUnsupportedEventBinaryOperationReason(IBinaryOperation operation)
    {
        var leftIsEvent = TryGetEventReference(operation.LeftOperand, out _);
        var rightIsEvent = TryGetEventReference(operation.RightOperand, out _);
        if (!leftIsEvent && !rightIsEvent)
            return null;

        if (operation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return "Event delegate equality and delegate combination are not supported by the multicast snapshot protocol. Only comparisons with null are supported for field-like events.";

        var oppositeOperand = leftIsEvent ? operation.RightOperand : operation.LeftOperand;
        if (IsNullEventComparisonOperand(oppositeOperand))
            return null;

        return "Event delegate equality and delegate combination are not supported by the multicast snapshot protocol. Only comparisons with null are supported for field-like events.";
    }

    private static bool TryGetEventReference(IOperation operation, out IEventReferenceOperation eventReference)
    {
        while (true)
        {
            switch (operation)
            {
                case IEventReferenceOperation direct:
                    eventReference = direct;
                    return true;
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    continue;
                default:
                    eventReference = null!;
                    return false;
            }
        }
    }

    private static bool IsNullEventComparisonOperand(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation is ILiteralOperation { ConstantValue.HasValue: true, ConstantValue.Value: null };
    }

    private bool TryValidateEventProtocol(IOperation operation, IEventSymbol eventSymbol, out string failure)
    {
        if (!EventLowering.IsSupportedFieldLikeInstanceEvent(eventSymbol, out var reason))
        {
            failure = $"Event '{eventSymbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' cannot lower: {reason}";
            return false;
        }

        if (_moduleDeclaredNames is null ||
            !_moduleDeclaredNames.ContainsKey(eventSymbol.ContainingType.OriginalDefinition))
        {
            failure = $"Event '{eventSymbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not declared by a runtime class emitted in the current module.";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    private bool IsModuleDeclaredEventHandlerMethod(IMethodSymbol method)
    {
        // Event validation has already established the current-module protocol before handlers
        // are classified, so this map is always available here.
        return _moduleDeclaredNames!.ContainsKey(method.OriginalDefinition) ||
            _moduleDeclaredNames.ContainsKey(method.ContainingType.OriginalDefinition);
    }

    private static CallExpression CreateEventProtocolCall(
        Expression receiver,
        string helperName,
        params Expression[] arguments)
        => new(
            new MemberExpression(receiver, new Identifier(helperName), computed: false, optional: false),
            NodeList.From(arguments),
            optional: false);
}
