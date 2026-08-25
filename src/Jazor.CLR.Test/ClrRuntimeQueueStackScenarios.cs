namespace Jazor.CLR.Test;

internal static class ClrRuntimeQueueStackScenarios
{
    private const string QueueModulePath = "System/Collections/Generic/QueueT1Module.js";
    private const string StackModulePath = "System/Collections/Generic/StackT1Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("queue.constructor.empty", "System.Collections.Generic.Queue<T>.Queue()", QueueModulePath, [], Queue(Array())),
        Success("queue.constructor.capacity", "System.Collections.Generic.Queue<T>.Queue(int)", QueueModulePath, [Number(32)], Queue(Array())),
        Failure("queue.constructor.capacity-rejects-negative", "System.Collections.Generic.Queue<T>.Queue(int)", QueueModulePath, [Number(-1)], "ArgumentOutOfRangeException"),
        Success("queue.constructor.collection-preserves-order", "System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)", QueueModulePath, [Array(Text("release"), Text("owner"))], Queue(Array(Text("release"), Text("owner")))),
        Success("queue.count.empty", "System.Collections.Generic.Queue<T>.Count.get", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue()")], Number(0)),
        Success("queue.enqueue.preserves-fifo-order", "System.Collections.Generic.Queue<T>.ToArray()", QueueModulePath, [
            Sequence(
                Reference("queue-enqueue", Invoke("System.Collections.Generic.Queue<T>.Queue()")),
                Invoke("System.Collections.Generic.Queue<T>.Enqueue(T)", Reference("queue-enqueue", Queue(Array())), Text("first")),
                Invoke("System.Collections.Generic.Queue<T>.Enqueue(T)", Reference("queue-enqueue", Queue(Array())), Text("second")),
                Reference("queue-enqueue", Queue(Array())))],
            Array(Text("first"), Text("second"))),
        Success("queue.dequeue.advances-head", "System.Collections.Generic.Queue<T>.ToArray()", QueueModulePath, [
            Sequence(
                Reference("queue-dequeue", Invoke("System.Collections.Generic.Queue<T>.Queue()")),
                Invoke("System.Collections.Generic.Queue<T>.Enqueue(T)", Reference("queue-dequeue", Queue(Array())), Text("first")),
                Invoke("System.Collections.Generic.Queue<T>.Enqueue(T)", Reference("queue-dequeue", Queue(Array())), Text("second")),
                Invoke("System.Collections.Generic.Queue<T>.Dequeue()", Reference("queue-dequeue", Queue(Array()))),
                Reference("queue-dequeue", Queue(Array())))],
            Array(Text("second"))),
        Success("queue.try-dequeue.empty", "System.Collections.Generic.Queue<T>.TryDequeue(out T)", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue()")], Array(Bool(false), Null())),
        Success("queue.try-dequeue.value", "System.Collections.Generic.Queue<T>.TryDequeue(out T)", QueueModulePath, [
            Sequence(
                Reference("queue-try-dequeue", Invoke("System.Collections.Generic.Queue<T>.Queue()")),
                Invoke("System.Collections.Generic.Queue<T>.Enqueue(T)", Reference("queue-try-dequeue", Queue(Array())), Text("value")),
                Reference("queue-try-dequeue", Queue(Array())))],
            Array(Bool(true), Text("value"))),
        Success("queue.peek.returns-head", "System.Collections.Generic.Queue<T>.Peek()", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)", Array(Text("head"), Text("tail")))], Text("head")),
        Success("queue.try-peek.empty", "System.Collections.Generic.Queue<T>.TryPeek(out T)", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue()")], Array(Bool(false), Null())),
        Success("queue.try-peek.value", "System.Collections.Generic.Queue<T>.TryPeek(out T)", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)", Array(Text("head"), Text("tail")))], Array(Bool(true), Text("head"))),
        Success("queue.contains.uses-equality", "System.Collections.Generic.Queue<T>.Contains(T)", QueueModulePath, [Invoke("System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)", Array(Text("head"), Text("tail"))), Text("tail")], Bool(true)),
        Success("queue.clear.resets-count", "System.Collections.Generic.Queue<T>.Count.get", QueueModulePath, [
            Sequence(
                Reference("queue-clear", Invoke("System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)", Array(Text("head"), Text("tail")))),
                Invoke("System.Collections.Generic.Queue<T>.Clear()", Reference("queue-clear", Queue(Array()))),
                Reference("queue-clear", Queue(Array())))],
            Number(0)),

        Success("stack.constructor.empty", "System.Collections.Generic.Stack<T>.Stack()", StackModulePath, [], Stack(Array())),
        Success("stack.constructor.capacity", "System.Collections.Generic.Stack<T>.Stack(int)", StackModulePath, [Number(32)], Stack(Array())),
        Failure("stack.constructor.capacity-rejects-negative", "System.Collections.Generic.Stack<T>.Stack(int)", StackModulePath, [Number(-1)], "ArgumentOutOfRangeException"),
        Success("stack.constructor.collection-preserves-enumeration-order", "System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", StackModulePath, [Array(Text("release"), Text("owner"))], Stack(Array(Text("release"), Text("owner")))),
        Success("stack.count.empty", "System.Collections.Generic.Stack<T>.Count.get", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack()")], Number(0)),
        Success("stack.push.to-array-is-lifo", "System.Collections.Generic.Stack<T>.ToArray()", StackModulePath, [
            Sequence(
                Reference("stack-push", Invoke("System.Collections.Generic.Stack<T>.Stack()")),
                Invoke("System.Collections.Generic.Stack<T>.Push(T)", Reference("stack-push", Stack(Array())), Text("first")),
                Invoke("System.Collections.Generic.Stack<T>.Push(T)", Reference("stack-push", Stack(Array())), Text("second")),
                Reference("stack-push", Stack(Array())))],
            Array(Text("second"), Text("first"))),
        Success("stack.pop.returns-top", "System.Collections.Generic.Stack<T>.Pop()", StackModulePath, [
            Sequence(
                Reference("stack-pop", Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second")))),
                Reference("stack-pop", Stack(Array())))],
            Text("second")),
        Success("stack.try-pop.empty", "System.Collections.Generic.Stack<T>.TryPop(out T)", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack()")], Array(Bool(false), Null())),
        Success("stack.try-pop.value", "System.Collections.Generic.Stack<T>.TryPop(out T)", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second")))], Array(Bool(true), Text("second"))),
        Success("stack.peek.returns-top", "System.Collections.Generic.Stack<T>.Peek()", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second")))], Text("second")),
        Success("stack.try-peek.empty", "System.Collections.Generic.Stack<T>.TryPeek(out T)", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack()")], Array(Bool(false), Null())),
        Success("stack.try-peek.value", "System.Collections.Generic.Stack<T>.TryPeek(out T)", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second")))], Array(Bool(true), Text("second"))),
        Success("stack.contains.uses-equality", "System.Collections.Generic.Stack<T>.Contains(T)", StackModulePath, [Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second"))), Text("first")], Bool(true)),
        Success("stack.clear.resets-count", "System.Collections.Generic.Stack<T>.Count.get", StackModulePath, [
            Sequence(
                Reference("stack-clear", Invoke("System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", Array(Text("first"), Text("second")))),
                Invoke("System.Collections.Generic.Stack<T>.Clear()", Reference("stack-clear", Stack(Array()))),
                Reference("stack-clear", Stack(Array())))],
            Number(0))
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Queue(ClrRuntimeValue items) => ClrRuntimeValue.Record(("head", Number(0)), ("items", items));
    private static ClrRuntimeValue Stack(ClrRuntimeValue items) => ClrRuntimeValue.Record(("items", items));
    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);
    private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value)
        => ClrRuntimeValue.Reference(id, value);
    private static ClrRuntimeValue Sequence(params ClrRuntimeValue[] steps)
        => ClrRuntimeValue.Sequence(steps);
}
