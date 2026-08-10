namespace Jazor.CLR.Test;

internal static class ClrRuntimeIndexRangeScenarios
{
    private const string IndexModulePath = "System/IndexModule.js";
    private const string RangeModulePath = "System/RangeModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("index.ctor.default", "System.Index.Index()", IndexModulePath, [], Index(0, false)),
        Success("index.ctor.from-end", "System.Index.Index(int, bool)", IndexModulePath, [Number(2), Bool(true)], Index(2, true)),
        Failure("index.ctor.rejects-negative-value", "System.Index.Index(int, bool)", IndexModulePath, [Number(-1), Bool(false)], "ArgumentOutOfRangeException"),
        Success("index.start", "static System.Index.Start.get", IndexModulePath, [], Index(0, false)),
        Success("index.end", "static System.Index.End.get", IndexModulePath, [], Index(0, true)),
        Success("index.from-start", "static System.Index.FromStart(int)", IndexModulePath, [Number(3)], Index(3, false)),
        Success("index.from-end", "static System.Index.FromEnd(int)", IndexModulePath, [Number(3)], Index(3, true)),
        Success("index.value", "System.Index.Value.get", IndexModulePath, [Invoke("static System.Index.FromEnd(int)", Number(3))], Number(3)),
        Success("index.is-from-end", "System.Index.IsFromEnd.get", IndexModulePath, [Invoke("static System.Index.FromEnd(int)", Number(3))], Bool(true)),
        Success("index.get-offset", "System.Index.GetOffset(int)", IndexModulePath, [Invoke("static System.Index.FromEnd(int)", Number(3)), Number(10)], Number(7)),
        Success("index.implicit-from-int", "static System.Index.implicit operator System.Index(int)", IndexModulePath, [Number(4)], Index(4, false)),
        Success("index.equals.uses-value-and-origin", "System.Index.Equals(System.Index)", IndexModulePath,
            [Invoke("static System.Index.FromEnd(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(2))], Bool(true)),
        Success("index.equals.distinguishes-origin", "System.Index.Equals(System.Index)", IndexModulePath,
            [Invoke("static System.Index.FromStart(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(2))], Bool(false)),
		Success("index.equals-object.uses-carrier-value", "override System.Index.Equals(object)", IndexModulePath,
			[Invoke("static System.Index.FromEnd(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(2))], Bool(true)),
		Success("index.equals-object.rejects-unrelated-value", "override System.Index.Equals(object)", IndexModulePath,
			[Invoke("static System.Index.FromEnd(int)", Number(2)), Number(2)], Bool(false)),
        Success("index.hash-code.includes-origin", "override System.Index.GetHashCode()", IndexModulePath,
            [Invoke("static System.Index.FromEnd(int)", Number(2))], Number(5)),
        Success("index.to-string.prefixes-from-end", "override System.Index.ToString()", IndexModulePath,
            [Invoke("static System.Index.FromEnd(int)", Number(2))], Text("^2")),

        Success("range.ctor.default", "System.Range.Range()", RangeModulePath, [], Range(Index(0, false), Index(0, false))),
        Success("range.ctor.bounded", "System.Range.Range(System.Index, System.Index)", RangeModulePath,
            [Invoke("static System.Index.FromStart(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(1))],
            Range(Index(2, false), Index(1, true))),
        Success("range.start", "System.Range.Start.get", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(1)))],
            Index(2, false)),
        Success("range.end", "System.Range.End.get", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(1)))],
            Index(1, true)),
        Success("range.start-at", "static System.Range.StartAt(System.Index)", RangeModulePath,
            [Invoke("static System.Index.FromStart(int)", Number(2))],
            Range(Index(2, false), Index(0, true))),
        Success("range.end-at", "static System.Range.EndAt(System.Index)", RangeModulePath,
            [Invoke("static System.Index.FromEnd(int)", Number(2))],
            Range(Index(0, false), Index(2, true))),
        Success("range.all", "static System.Range.All.get", RangeModulePath, [], Range(Index(0, false), Index(0, true))),
        Success("range.get-offset-and-length", "System.Range.GetOffsetAndLength(int)", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(2)), Invoke("static System.Index.FromEnd(int)", Number(1))), Number(8)],
            Record(("Offset", Number(2)), ("Length", Number(5)))),
        Failure("range.get-offset-and-length.rejects-inverted-range", "System.Range.GetOffsetAndLength(int)", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromEnd(int)", Number(2)), Invoke("static System.Index.FromStart(int)", Number(1))), Number(4)],
            "ArgumentOutOfRangeException"),
        Success("range.equals.compares-both-boundaries", "System.Range.Equals(System.Range)", RangeModulePath,
            [
                Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2))),
                Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2)))
            ], Bool(true)),
		Success("range.equals-object.compares-carrier-boundaries", "override System.Range.Equals(object)", RangeModulePath,
			[
				Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2))),
				Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2)))
			], Bool(true)),
		Success("range.equals-object.rejects-unrelated-value", "override System.Range.Equals(object)", RangeModulePath,
			[Invoke("static System.Range.All.get"), Number(0)], Bool(false)),
        Success("range.hash-code-combines-boundaries", "override System.Range.GetHashCode()", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2)))], Number(799)),
        Success("range.to-string-uses-boundary-syntax", "override System.Range.ToString()", RangeModulePath,
            [Invoke("System.Range.Range(System.Index, System.Index)", Invoke("static System.Index.FromStart(int)", Number(1)), Invoke("static System.Index.FromEnd(int)", Number(2)))], Text("1..^2"))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Index(double value, bool fromEnd)
        => Record(("value", Number(value)), ("fromEnd", Bool(fromEnd)));

    private static ClrRuntimeValue Range(ClrRuntimeValue start, ClrRuntimeValue end)
        => Record(("start", start), ("end", end));

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties)
        => ClrRuntimeValue.Record(properties);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
}
