namespace Jazor.CLR.Test;

internal static class ClrRuntimeSetScenarios
{
    public static IReadOnlyList<ClrRuntimeScenario> HashSet { get; } = Create(
        "hash-set",
        "HashSet",
        "System/Collections/Generic/HashSetT1Module.js");

    public static IReadOnlyList<ClrRuntimeScenario> InterfaceSet { get; } = Create(
        "iset",
        "ISet",
        "System/Collections/Generic/ISetT1Module.js");

    private static IReadOnlyList<ClrRuntimeScenario> Create(string idPrefix, string typeName, string modulePath)
    {
        var type = $"System.Collections.Generic.{typeName}<T>";
        return
        [
            SuccessMutation($"{idPrefix}.add.new-value", $"{type}.Add(T)", modulePath, [Set(Text("a"), Text("b")), Text("c")], Bool(true), [Set(Text("a"), Text("b"), Text("c")), Text("c")]),
            SuccessMutation($"{idPrefix}.add.existing-value", $"{type}.Add(T)", modulePath, [Set(Text("a")), Text("a")], Bool(false), [Set(Text("a")), Text("a")]),
            Mutation($"{idPrefix}.union.with-overlap", $"{type}.UnionWith(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("b"), Text("c"))], [Set(Text("a"), Text("b"), Text("c")), Array(Text("b"), Text("c"))]),
            Mutation($"{idPrefix}.intersect.retains-common-values", $"{type}.IntersectWith(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b"), Text("c")), Array(Text("b"), Text("d"))], [Set(Text("b")), Array(Text("b"), Text("d"))]),
            Mutation($"{idPrefix}.except.removes-other-values", $"{type}.ExceptWith(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b"), Text("c")), Array(Text("b"))], [Set(Text("a"), Text("c")), Array(Text("b"))]),
            Mutation($"{idPrefix}.symmetric-except.toggles-membership", $"{type}.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("b"), Text("c"))], [Set(Text("a"), Text("c")), Array(Text("b"), Text("c"))]),

            Success($"{idPrefix}.subset.ignores-duplicate-other-values", $"{type}.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("a"), Text("b"), Text("b"), Text("c"))], Bool(true)),
            Success($"{idPrefix}.proper-subset", $"{type}.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("a"), Text("b"), Text("c"))], Bool(true)),
            Success($"{idPrefix}.superset", $"{type}.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b"), Text("c")), Array(Text("a"), Text("b"))], Bool(true)),
            Success($"{idPrefix}.proper-superset", $"{type}.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b"), Text("c")), Array(Text("a"), Text("b"))], Bool(true)),
            Success($"{idPrefix}.overlaps", $"{type}.Overlaps(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("b"), Text("c"))], Bool(true)),
            Success($"{idPrefix}.set-equals.ignores-order-and-duplicates", $"{type}.SetEquals(System.Collections.Generic.IEnumerable<T>)", modulePath, [Set(Text("a"), Text("b")), Array(Text("b"), Text("a"), Text("a"))], Bool(true))
        ];
    }

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessMutation(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Set(params ClrRuntimeValue[] values) => ClrRuntimeValue.Set(values);
}
