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

        Success("stack.constructor.empty", "System.Collections.Generic.Stack<T>.Stack()", StackModulePath, [], Stack(Array())),
        Success("stack.constructor.capacity", "System.Collections.Generic.Stack<T>.Stack(int)", StackModulePath, [Number(32)], Stack(Array())),
        Failure("stack.constructor.capacity-rejects-negative", "System.Collections.Generic.Stack<T>.Stack(int)", StackModulePath, [Number(-1)], "ArgumentOutOfRangeException"),
        Success("stack.constructor.collection-preserves-enumeration-order", "System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)", StackModulePath, [Array(Text("release"), Text("owner"))], Stack(Array(Text("release"), Text("owner"))))
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Queue(ClrRuntimeValue items) => ClrRuntimeValue.Record(("head", Number(0)), ("items", items));
    private static ClrRuntimeValue Stack(ClrRuntimeValue items) => ClrRuntimeValue.Record(("items", items));
}
