namespace Jazor.CLR.Test;

internal static class ClrRuntimeStringBuilderScenarios
{
    private const string ModulePath = "System/Text/StringBuilderModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "string-builder.capacity.default-constructor",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke("System.Text.StringBuilder.StringBuilder()")],
            Number(16)),
        Success(
            "string-builder.capacity.zero-request-uses-default",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke("System.Text.StringBuilder.StringBuilder(int)", Number(0))],
            Number(16)),
        Success(
            "string-builder.capacity.string-constructor-fits-content",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke("System.Text.StringBuilder.StringBuilder(string)", Text("01234567890123456789"))],
            Number(20)),
        Success(
            "string-builder.capacity.append-grows-by-doubling",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke(
                "System.Text.StringBuilder.Append(char, int)",
                Invoke("System.Text.StringBuilder.StringBuilder(int)", Number(3)),
                Text("x"),
                Number(4))],
            Number(6)),
        Success(
            "string-builder.capacity.small-append-may-grow-above-maximum",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke(
                "System.Text.StringBuilder.Append(char, int)",
                Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(4), Number(5)),
                Text("x"),
                Number(5))],
            Number(8)),
        Success(
            "string-builder.capacity.spare-room-above-maximum-remains-usable",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke(
                "System.Text.StringBuilder.Append(char, int)",
                Invoke(
                    "System.Text.StringBuilder.Append(char, int)",
                    Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(4), Number(5)),
                    Text("x"),
                    Number(5)),
                Text("x"),
                Number(3))],
            Number(8)),
        Failure(
            "string-builder.capacity.above-maximum-spare-room-cannot-grow-again",
            "System.Text.StringBuilder.Append(char, int)",
            [
                Invoke(
                    "System.Text.StringBuilder.Append(char, int)",
                    Invoke(
                        "System.Text.StringBuilder.Append(char, int)",
                        Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(4), Number(5)),
                        Text("x"),
                        Number(5)),
                    Text("x"),
                    Number(3)),
                Text("x"),
                Number(1)
            ],
            "ArgumentOutOfRangeException"),
        Success(
            "string-builder.max-capacity.bounded-constructor",
            "System.Text.StringBuilder.MaxCapacity.get",
            [Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(2), Number(5))],
            Number(5)),
        Success(
            "string-builder.capacity.bounded-zero-request-uses-maximum",
            "System.Text.StringBuilder.Capacity.get",
            [Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(0), Number(5))],
            Number(5)),
        Failure(
            "string-builder.constructor.rejects-capacity-above-maximum",
            "System.Text.StringBuilder.StringBuilder(int, int)",
            [Number(6), Number(5)],
            "ArgumentOutOfRangeException"),
        Success(
            "string-builder.ensure-capacity.expands-exactly",
            "System.Text.StringBuilder.EnsureCapacity(int)",
            [Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(2), Number(5)), Number(5)],
            Number(5)),
        Success(
            "string-builder.capacity.setter-persists",
            "System.Text.StringBuilder.Capacity.get",
            [
                Reference("capacity-builder", Invoke("System.Text.StringBuilder.StringBuilder(int)", Number(2))),
                Invoke(
                    "System.Text.StringBuilder.Capacity.set",
                    Reference("capacity-builder", Array()),
                    Number(7))
            ],
            Number(7)),
        Failure(
            "string-builder.capacity.setter-rejects-below-length",
            "System.Text.StringBuilder.Capacity.set",
            [Invoke("System.Text.StringBuilder.StringBuilder(string)", Text("abcd")), Number(3)],
            "ArgumentOutOfRangeException"),
        Failure(
            "string-builder.ensure-capacity.rejects-above-maximum",
            "System.Text.StringBuilder.EnsureCapacity(int)",
            [Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(2), Number(5)), Number(6)],
            "ArgumentOutOfRangeException"),
        Failure(
            "string-builder.ensure-capacity.null-receiver-precedes-argument-validation",
            "System.Text.StringBuilder.EnsureCapacity(int)",
            [Null(), Number(-1)],
            "NullReferenceException"),
        Failure(
            "string-builder.max-capacity.rejects-growth",
            "System.Text.StringBuilder.Append(char, int)",
            [Invoke("System.Text.StringBuilder.StringBuilder(int, int)", Number(2), Number(5)), Text("x"), Number(6)],
            "ArgumentOutOfRangeException"),
        Success("string-builder.ctor.capacity", "System.Text.StringBuilder.StringBuilder(int)", [Number(8)], Array()),
        Failure("string-builder.ctor.capacity.rejects-negative", "System.Text.StringBuilder.StringBuilder(int)", [Number(-1)], "ArgumentOutOfRangeException"),
        Success("string-builder.ctor.text-capacity", "System.Text.StringBuilder.StringBuilder(string, int)", [Text("Vue"), Number(1)], Characters("Vue")),
        Success("string-builder.ctor.range", "System.Text.StringBuilder.StringBuilder(string, int, int, int)", [Text("RazorVue"), Number(5), Number(3), Number(0)], Characters("Vue")),

        Success("string-builder.to-string.range", "System.Text.StringBuilder.ToString(int, int)", [Characters("RazorVue"), Number(5), Number(3)], Text("Vue")),
        Mutation(
            "string-builder.length.expand-with-nul",
            "System.Text.StringBuilder.Length.set",
            [Characters("ab"), Number(4)],
            [Array(Text("a"), Text("b"), Text("\0"), Text("\0")), Number(4)]),
        Failure("string-builder.length.rejects-negative", "System.Text.StringBuilder.Length.set", [Characters("ab"), Number(-1)], "ArgumentOutOfRangeException"),
        Success("string-builder.indexer.get", "System.Text.StringBuilder.this[int].get", [Characters("abc"), Number(1)], Text("b")),
        Mutation(
            "string-builder.indexer.set",
            "System.Text.StringBuilder.this[int].set",
            [Characters("abc"), Number(1), Text("X")],
            [Characters("aXc"), Number(1), Text("X")]),
        Failure("string-builder.indexer.rejects-end", "System.Text.StringBuilder.this[int].get", [Characters("abc"), Number(3)], "ArgumentOutOfRangeException"),

        Success("string-builder.append.character-repeat", "System.Text.StringBuilder.Append(char, int)", [Characters("a"), Text("b"), Number(3)], Characters("abbb")),
        Success("string-builder.append.character-range", "System.Text.StringBuilder.Append(char[], int, int)", [Characters("a"), Characters("WXYZ"), Number(1), Number(2)], Characters("aXY")),
        Success("string-builder.append.string", "System.Text.StringBuilder.Append(string)", [Characters("a"), Text("bc")], Characters("abc")),
        Success("string-builder.append.string-range", "System.Text.StringBuilder.Append(string, int, int)", [Characters("a"), Text("WXYZ"), Number(1), Number(2)], Characters("aXY")),
        Success("string-builder.append-line", "System.Text.StringBuilder.AppendLine()", [Characters("a")], Characters("a\n")),
        Success("string-builder.append-line.string", "System.Text.StringBuilder.AppendLine(string)", [Characters("a"), Text("bc")], Characters("abc\n")),
        Failure("string-builder.append.string-range.rejects-null", "System.Text.StringBuilder.Append(string, int, int)", [Characters("a"), Null(), Number(0), Number(1)], "ArgumentNullException"),
        SuccessMutation(
            "string-builder.append.builder-self-snapshots",
            "System.Text.StringBuilder.Append(System.Text.StringBuilder)",
            [Reference("self", Characters("ab")), Reference("self", Characters("ignored"))],
            Characters("abab"),
            [Characters("abab"), Characters("abab")]),
        Success("string-builder.append.builder-range", "System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)", [Characters("a"), Characters("WXYZ"), Number(1), Number(2)], Characters("aXY")),
        Mutation(
            "string-builder.copy-to.character-array",
            "System.Text.StringBuilder.CopyTo(int, char[], int, int)",
            [Characters("abcd"), Number(1), Characters("----"), Number(1), Number(2)],
            [Characters("abcd"), Number(1), Characters("-bc-"), Number(1), Number(2)]),
        Failure("string-builder.copy-to.rejects-small-destination", "System.Text.StringBuilder.CopyTo(int, char[], int, int)", [Characters("abcd"), Number(0), Characters("-"), Number(0), Number(2)], "ArgumentException"),
        Success("string-builder.insert.string-repeat", "System.Text.StringBuilder.Insert(int, string, int)", [Characters("ab"), Number(1), Text("xy"), Number(2)], Characters("axyxyb")),
        Success("string-builder.remove.range", "System.Text.StringBuilder.Remove(int, int)", [Characters("abcdef"), Number(2), Number(3)], Characters("abf")),

        Success("string-builder.append.boolean", "System.Text.StringBuilder.Append(bool)", [Array(), Bool(true)], Characters("True")),
        Success("string-builder.append.character", "System.Text.StringBuilder.Append(char)", [Characters("a"), Text("b")], Characters("ab")),
        Success("string-builder.append.sbyte", "System.Text.StringBuilder.Append(sbyte)", [Array(), Number(-8)], Characters("-8")),
        Success("string-builder.append.byte", "System.Text.StringBuilder.Append(byte)", [Array(), Number(255)], Characters("255")),
        Success("string-builder.append.int16", "System.Text.StringBuilder.Append(short)", [Array(), Number(-1234)], Characters("-1234")),
        Success("string-builder.append.int32", "System.Text.StringBuilder.Append(int)", [Array(), Number(-123456)], Characters("-123456")),
        Success("string-builder.append.int64", "System.Text.StringBuilder.Append(long)", [Array(), Big(-9223372036854775808)], Characters("-9223372036854775808")),
		Success("string-builder.append.single", "System.Text.StringBuilder.Append(float)", [Array(), Number(1.25)], Characters("1.25")),
		Success("string-builder.append.double", "System.Text.StringBuilder.Append(double)", [Array(), Number(-2.5)], Characters("-2.5")),
        Success("string-builder.append.decimal", "System.Text.StringBuilder.Append(decimal)", [Array(), Text("79228162514264337593543950335")], Characters("79228162514264337593543950335")),
        Success("string-builder.append.uint16", "System.Text.StringBuilder.Append(ushort)", [Array(), Number(65535)], Characters("65535")),
        Success("string-builder.append.uint32", "System.Text.StringBuilder.Append(uint)", [Array(), Number(4294967295)], Characters("4294967295")),
        Success("string-builder.append.uint64", "System.Text.StringBuilder.Append(ulong)", [Array(), UnsignedBig("18446744073709551615")], Characters("18446744073709551615")),
		Success("string-builder.append.object-boolean", "System.Text.StringBuilder.Append(object)", [Characters("a"), Bool(true)], Characters("aTrue")),
		Success("string-builder.append.object-null", "System.Text.StringBuilder.Append(object)", [Characters("a"), Null()], Characters("a")),
        Success("string-builder.append.character-array", "System.Text.StringBuilder.Append(char[])", [Characters("a"), Characters("bc")], Characters("abc")),
        Success("string-builder.append.character-span", "System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)", [Characters("a"), Text("bc")], Characters("abc")),
        Success("string-builder.append.array-backed-character-span", "System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)", [Characters("a"), Characters("bc")], Characters("abc")),
        Success("string-builder.append.default-character-span", "System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)", [Characters("ab"), Null()], Characters("ab")),
        Success("string-builder.append-join.string-array", "System.Text.StringBuilder.AppendJoin(string, params string[])", [Characters("a"), Text("|"), Array(Text("b"), Null(), Text("c"))], Characters("ab||c")),
		Success("string-builder.append-join.string-span", "System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)", [Characters("a"), Null(), Array(Text("b"), Text("c"))], Characters("abc")),
		Success("string-builder.append-join.object-array", "System.Text.StringBuilder.AppendJoin(string, params object[])", [Characters("a"), Text("|"), Array(Bool(true), Null(), Number(2))], Characters("aTrue||2")),
		Success("string-builder.append-join.object-span", "System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)", [Characters("a"), Text("|"), Array(Bool(true), Null(), Number(2))], Characters("aTrue||2")),
		Success("string-builder.append-join.generic-enumerable", "System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)", [Characters("a"), Text("-"), Array(Number(1), Number(2))], Characters("a1-2")),
		Failure("string-builder.append-join.object-array.rejects-null", "System.Text.StringBuilder.AppendJoin(string, params object[])", [Characters("a"), Text("|"), Null()], "ArgumentNullException"),
		Success("string-builder.append-join.character-object-array", "System.Text.StringBuilder.AppendJoin(char, params object[])", [Characters("a"), Text("|"), Array(Bool(true), Null(), Number(2))], Characters("aTrue||2")),
		Success("string-builder.append-join.character-object-span", "System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)", [Characters("a"), Text("|"), Array(Bool(true), Null(), Number(2))], Characters("aTrue||2")),
		Success("string-builder.append-join.character-generic-enumerable", "System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)", [Characters("a"), Text("-"), Array(Number(1), Number(2))], Characters("a1-2")),
        Success("string-builder.append-join.character-array", "System.Text.StringBuilder.AppendJoin(char, params string[])", [Characters("a"), Text("|"), Array(Text("b"), Null(), Text("c"))], Characters("ab||c")),
        Success("string-builder.append-join.character-span", "System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)", [Characters("a"), Text("/"), Array(Text("b"), Text("c"))], Characters("ab/c")),
        Failure("string-builder.append-join.rejects-null-values", "System.Text.StringBuilder.AppendJoin(string, params string[])", [Characters("a"), Text("|"), Null()], "ArgumentNullException"),

        Success("string-builder.insert.string", "System.Text.StringBuilder.Insert(int, string)", [Characters("ac"), Number(1), Text("b")], Characters("abc")),
        Success("string-builder.insert.boolean", "System.Text.StringBuilder.Insert(int, bool)", [Characters("ab"), Number(1), Bool(false)], Characters("aFalseb")),
        Success("string-builder.insert.sbyte", "System.Text.StringBuilder.Insert(int, sbyte)", [Characters("ab"), Number(1), Number(-8)], Characters("a-8b")),
        Success("string-builder.insert.byte", "System.Text.StringBuilder.Insert(int, byte)", [Characters("ab"), Number(1), Number(255)], Characters("a255b")),
        Success("string-builder.insert.int16", "System.Text.StringBuilder.Insert(int, short)", [Characters("ab"), Number(1), Number(-1234)], Characters("a-1234b")),
        Success("string-builder.insert.character", "System.Text.StringBuilder.Insert(int, char)", [Characters("ac"), Number(1), Text("b")], Characters("abc")),
        Success("string-builder.insert.character-array", "System.Text.StringBuilder.Insert(int, char[])", [Characters("ad"), Number(1), Characters("bc")], Characters("abcd")),
        Success("string-builder.insert.character-range", "System.Text.StringBuilder.Insert(int, char[], int, int)", [Characters("ad"), Number(1), Characters("XbcY"), Number(1), Number(2)], Characters("abcd")),
        Success("string-builder.insert.int32", "System.Text.StringBuilder.Insert(int, int)", [Characters("ab"), Number(1), Number(-123456)], Characters("a-123456b")),
        Success("string-builder.insert.int64", "System.Text.StringBuilder.Insert(int, long)", [Characters("ab"), Number(1), Big(-9223372036854775808)], Characters("a-9223372036854775808b")),
		Success("string-builder.insert.single", "System.Text.StringBuilder.Insert(int, float)", [Characters("ab"), Number(1), Number(1.25)], Characters("a1.25b")),
		Success("string-builder.insert.double", "System.Text.StringBuilder.Insert(int, double)", [Characters("ab"), Number(1), Number(-2.5)], Characters("a-2.5b")),
        Success("string-builder.insert.decimal", "System.Text.StringBuilder.Insert(int, decimal)", [Characters("ab"), Number(1), Text("123.4500")], Characters("a123.4500b")),
        Success("string-builder.insert.uint16", "System.Text.StringBuilder.Insert(int, ushort)", [Characters("ab"), Number(1), Number(65535)], Characters("a65535b")),
        Success("string-builder.insert.uint32", "System.Text.StringBuilder.Insert(int, uint)", [Characters("ab"), Number(1), Number(4294967295)], Characters("a4294967295b")),
        Success("string-builder.insert.uint64", "System.Text.StringBuilder.Insert(int, ulong)", [Characters("ab"), Number(1), UnsignedBig("18446744073709551615")], Characters("a18446744073709551615b")),
		Success("string-builder.insert.object-boolean", "System.Text.StringBuilder.Insert(int, object)", [Characters("ab"), Number(1), Bool(false)], Characters("aFalseb")),
		Success("string-builder.insert.object-null", "System.Text.StringBuilder.Insert(int, object)", [Characters("ab"), Number(1), Null()], Characters("ab")),
        Success("string-builder.insert.character-span", "System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)", [Characters("ad"), Number(1), Text("bc")], Characters("abcd")),
        Success("string-builder.insert.array-backed-character-span", "System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)", [Characters("ad"), Number(1), Characters("bc")], Characters("abcd")),
        Success("string-builder.insert.default-character-span", "System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)", [Characters("ab"), Number(1), Null()], Characters("ab")),
        Failure("string-builder.insert.rejects-index", "System.Text.StringBuilder.Insert(int, string)", [Characters("ab"), Number(3), Text("x")], "ArgumentOutOfRangeException"),

        Success("string-builder.replace.string", "System.Text.StringBuilder.Replace(string, string)", [Characters("ababa"), Text("aba"), Text("X")], Characters("Xba")),
        Success("string-builder.replace.string-range", "System.Text.StringBuilder.Replace(string, string, int, int)", [Characters("ababa"), Text("a"), Text("X"), Number(1), Number(3)], Characters("abXba")),
        Success("string-builder.replace.character-span", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Characters("ababa"), Text("aba"), Text("X")], Characters("Xba")),
        Success("string-builder.replace.array-backed-character-span", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Characters("ababa"), Characters("aba"), Characters("X")], Characters("Xba")),
        Success("string-builder.replace.character-span-with-default", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Characters("ababa"), Text("aba"), Null()], Characters("ba")),
        Failure("string-builder.replace.character-span.rejects-default-old-value", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Characters("abc"), Null(), Text("x")], "ArgumentException"),
        Success(
            "string-builder.equals.builder-compares-content-not-capacity",
            "System.Text.StringBuilder.Equals(System.Text.StringBuilder)",
            [
                Invoke("System.Text.StringBuilder.StringBuilder(string)", Text("Jazor")),
                Invoke(
                    "System.Text.StringBuilder.Append(string)",
                    Invoke("System.Text.StringBuilder.StringBuilder(int)", Number(32)),
                    Text("Jazor"))
            ],
            Bool(true)),
        Success(
            "string-builder.equals.builder-rejects-different-content",
            "System.Text.StringBuilder.Equals(System.Text.StringBuilder)",
            [Characters("Jazor"), Characters("Razor")],
            Bool(false)),
        Success(
            "string-builder.equals.builder-accepts-null",
            "System.Text.StringBuilder.Equals(System.Text.StringBuilder)",
            [Characters("Jazor"), Null()],
            Bool(false)),
        Failure(
            "string-builder.equals.builder-null-receiver",
            "System.Text.StringBuilder.Equals(System.Text.StringBuilder)",
            [Null(), Characters("Jazor")],
            "NullReferenceException"),
        Success("string-builder.equals.character-span", "System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)", [Characters("Jazor"), Text("Jazor")], Bool(true)),
        Success("string-builder.equals.array-backed-character-span", "System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)", [Characters("Jazor"), Characters("Jazor")], Bool(true)),
        Success("string-builder.equals.default-character-span", "System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)", [Array(), Null()], Bool(true)),
        Success("string-builder.replace.character-span-range", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)", [Characters("ababa"), Text("a"), Text("X"), Number(1), Number(3)], Characters("abXba")),
        Failure("string-builder.replace.character-span-range.rejects-range", "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)", [Characters("abc"), Text("a"), Text("x"), Number(2), Number(2)], "ArgumentOutOfRangeException"),
        Failure("string-builder.replace.rejects-empty-old-value", "System.Text.StringBuilder.Replace(string, string)", [Characters("abc"), Text(""), Text("x")], "ArgumentException"),
        Success("string-builder.replace.character", "System.Text.StringBuilder.Replace(char, char)", [Characters("abca"), Text("a"), Text("X")], Characters("XbcX")),
        Success("string-builder.replace.character-range", "System.Text.StringBuilder.Replace(char, char, int, int)", [Characters("abca"), Text("a"), Text("X"), Number(1), Number(3)], Characters("abcX"))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessMutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue UnsignedBig(string value)
        => ClrRuntimeValue.BigInt(System.Numerics.BigInteger.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Characters(string value)
        => ClrRuntimeValue.Array(value.Select(static character => Text(character.ToString())).ToArray());
    private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value) => ClrRuntimeValue.Reference(id, value);
    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);
}
