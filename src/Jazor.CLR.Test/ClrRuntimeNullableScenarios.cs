namespace Jazor.CLR.Test;

internal static class ClrRuntimeNullableScenarios
{
    private const string ModulePath = "System/NullableT1Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("nullable.compare.both-empty", "static System.Nullable.Compare<T>(T?, T?)", [Null(), Null()], Number(0)),
        Success("nullable.compare.empty-before-value", "static System.Nullable.Compare<T>(T?, T?)", [Null(), Number(1)], Number(-1)),
        Success("nullable.compare.value-after-empty", "static System.Nullable.Compare<T>(T?, T?)", [Number(1), Null()], Number(1)),
        Success("nullable.compare.orders-values", "static System.Nullable.Compare<T>(T?, T?)", [Number(3), Number(8)], Number(-1)),
        Success("nullable.equals.both-empty", "static System.Nullable.Equals<T>(T?, T?)", [Null(), Null()], Bool(true)),
        Success("nullable.equals.empty-and-value", "static System.Nullable.Equals<T>(T?, T?)", [Null(), Number(0)], Bool(false)),
        Success("nullable.equals.same-values", "static System.Nullable.Equals<T>(T?, T?)", [Number(7), Number(7)], Bool(true)),
        Success("nullable.equals.different-values", "static System.Nullable.Equals<T>(T?, T?)", [Number(7), Number(8)], Bool(false))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
}
