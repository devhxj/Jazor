namespace Jazor.CLR.Test;

internal static class ClrRuntimeStringExtendedScenarios
{
    private const string ModulePath = "System/StringModule.js";
    private const int Ordinal = 4;
    private const int OrdinalIgnoreCase = 5;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("string.intern.returns-canonical-carrier", "static string.Intern(string)", [Text("Jazor")], Text("Jazor")),
        Failure("string.intern.rejects-null", "static string.Intern(string)", [Null()], "ArgumentNullException"),
        Success("string.compare.ordinal-ignore-case", "static string.Compare(string, string, System.StringComparison)", [Text("Alpha"), Text("alpha"), Number(OrdinalIgnoreCase)], Number(0)),
        Success("string.compare-ordinal.range-code-unit-difference", "static string.CompareOrdinal(string, int, string, int, int)", [Text("az"), Number(1), Text("aa"), Number(1), Number(1)], Number(25)),
        Success("string.compare-ordinal.range-null-precedes-validation", "static string.CompareOrdinal(string, int, string, int, int)", [Null(), Number(99), Text("a"), Number(0), Number(1)], Number(-1)),
        Success("string.compare-ordinal.range-clamps-length", "static string.CompareOrdinal(string, int, string, int, int)", [Text("a"), Number(0), Text("ab"), Number(0), Number(10)], Number(-1)),
        Failure("string.compare-ordinal.range-rejects-index", "static string.CompareOrdinal(string, int, string, int, int)", [Text("a"), Number(2), Text("a"), Number(0), Number(0)], "ArgumentOutOfRangeException"),
        Success("string.format.three-arguments", "static string.Format(string, object, object, object)", [Text("{0}/{1}/{2}"), Text("src"), Text("app"), Text("main")], Text("src/app/main")),
        Success("string.equals.instance-ordinal-ignore-case", "string.Equals(string, System.StringComparison)", [Text("Admin"), Text("admin"), Number(OrdinalIgnoreCase)], Bool(true)),

        Success("string.constructor.characters", "string.String(char[])", [Array(Text("J"), Text("S"))], Text("JS")),
        Failure("string.constructor.characters.rejects-null", "string.String(char[])", [Null()], "ArgumentNullException"),
        Success("string.constructor.character-range", "string.String(char[], int, int)", [Array(Text("a"), Text("b"), Text("c"), Text("d")), Number(1), Number(2)], Text("bc")),
        Failure("string.constructor.character-range.rejects-invalid-range", "string.String(char[], int, int)", [Array(Text("a")), Number(1), Number(1)], "ArgumentOutOfRangeException"),
        Success("string.constructor.repeated-character", "string.String(char, int)", [Text("x"), Number(4)], Text("xxxx")),
        Failure("string.constructor.repeated-character.rejects-negative-count", "string.String(char, int)", [Text("x"), Number(-1)], "ArgumentOutOfRangeException"),
        Success("string.constructor.read-only-character-span", "string.String(System.ReadOnlySpan<char>)", [Text("RazorVue")], Text("RazorVue")),
        Success("string.constructor.array-backed-read-only-character-span", "string.String(System.ReadOnlySpan<char>)", [Array(Text("J"), Text("S"))], Text("JS")),
        Success("string.constructor.default-read-only-character-span", "string.String(System.ReadOnlySpan<char>)", [Null()], Text("")),
        Success("string.implicit-read-only-character-span", "static string.implicit operator System.ReadOnlySpan<char>(string)", [Text("Jazor")], Text("Jazor")),
        Success("string.implicit-read-only-character-span.normalizes-null", "static string.implicit operator System.ReadOnlySpan<char>(string)", [Null()], Text("")),
        Success("string.hash-code.read-only-character-span", "static string.GetHashCode(System.ReadOnlySpan<char>)", [Text("Jazor")], Number(558046645)),
        Success("string.hash-code.array-backed-read-only-character-span", "static string.GetHashCode(System.ReadOnlySpan<char>)", [Array(Text("J"), Text("a"), Text("z"), Text("o"), Text("r"))], Number(558046645)),
        Success("string.hash-code.default-read-only-character-span", "static string.GetHashCode(System.ReadOnlySpan<char>)", [Null()], Number(17)),
        Success("string.hash-code.ordinal-comparison", "string.GetHashCode(System.StringComparison)", [Text("Jazor"), Number(Ordinal)], Number(558046645)),
        Success("string.hash-code.read-only-character-span.ordinal-comparison", "static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)", [Array(Text("J"), Text("a"), Text("z"), Text("o"), Text("r")), Number(Ordinal)], Number(558046645)),
        Failure("string.hash-code.comparison.rejects-culture-mode", "string.GetHashCode(System.StringComparison)", [Text("Jazor"), Number(0)], "NotSupportedException"),
        Failure("string.hash-code.read-only-character-span.comparison.rejects-ordinal-ignore-case", "static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)", [Text("Jazor"), Number(OrdinalIgnoreCase)], "NotSupportedException"),
        Failure("string.hash-code.comparison.rejects-invalid-mode", "string.GetHashCode(System.StringComparison)", [Text("Jazor"), Number(99)], "ArgumentException"),
        Mutation(
            "string.copy-to.character-array",
            "string.CopyTo(int, char[], int, int)",
            [Text("abcd"), Number(1), Array(Text("-"), Text("-"), Text("-"), Text("-")), Number(1), Number(2)],
            [Text("abcd"), Number(1), Array(Text("-"), Text("b"), Text("c"), Text("-")), Number(1), Number(2)]),
        Failure("string.copy-to.rejects-small-destination", "string.CopyTo(int, char[], int, int)", [Text("abcd"), Number(0), Array(Text("-")), Number(0), Number(2)], "ArgumentException"),
		Success("string.copy.preserves-immutable-value", "static string.Copy(string)", [Text("Jazor")], Text("Jazor")),
		Failure("string.copy.rejects-null", "static string.Copy(string)", [Null()], "ArgumentNullException"),

        Success("string.normalize.default-composes", "string.Normalize()", [Text("e\u0301")], Text("\u00E9")),
        Success("string.normalize.form-d-decomposes", "string.Normalize(System.Text.NormalizationForm)", [Text("\u00E9"), Number(2)], Text("e\u0301")),
        Success("string.is-normalized.default-false-for-decomposed", "string.IsNormalized()", [Text("e\u0301")], Bool(false)),
        Success("string.is-normalized.form-d", "string.IsNormalized(System.Text.NormalizationForm)", [Text("e\u0301"), Number(2)], Bool(true)),
        Failure("string.normalize.rejects-unknown-form", "string.Normalize(System.Text.NormalizationForm)", [Text("value"), Number(3)], "ArgumentException"),

        Success("string.concat.string-array-normalizes-null", "static string.Concat(params string[])", [Array(Text("a"), Null(), Text("b"))], Text("ab")),
        Success("string.concat.string-enumerable", "static string.Concat(System.Collections.Generic.IEnumerable<string>)", [Array(Text("a"), Text("b"), Text("c"))], Text("abc")),
        Success("string.concat.string-span", "static string.Concat(params System.ReadOnlySpan<string>)", [Array(Text("x"), Text("y"))], Text("xy")),
        Success("string.concat.two-character-spans", "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Text("Razor"), Text("Vue")], Text("RazorVue")),
        Success("string.concat.two-mixed-backed-character-spans", "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Array(Text("R"), Text("V")), Text("JS")], Text("RVJS")),
        Success("string.concat.three-character-spans-with-default", "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Text("Jazor"), Null(), Text(".CLR")], Text("Jazor.CLR")),
        Success("string.concat.four-character-spans", "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", [Text("A"), Text("B"), Text("C"), Text("D")], Text("ABCD")),
        Failure("string.concat.string-array.rejects-null", "static string.Concat(params string[])", [Null()], "ArgumentNullException"),
		Success("string.concat.object.boolean", "static string.Concat(object)", [Bool(true)], Text("True")),
		Success("string.concat.two-objects", "static string.Concat(object, object)", [Bool(true), Number(2)], Text("True2")),
		Success("string.concat.three-objects.normalizes-null", "static string.Concat(object, object, object)", [Text("A"), Null(), Number(2)], Text("A2")),
		Success("string.concat.object-array", "static string.Concat(params object[])", [Array(Text("A"), Bool(false), Null(), Number(2))], Text("AFalse2")),
		Success("string.concat.object-span", "static string.Concat(params System.ReadOnlySpan<object>)", [Array(Text("A"), Bool(false), Null(), Number(2))], Text("AFalse2")),
		Failure("string.concat.object-array.rejects-null", "static string.Concat(params object[])", [Null()], "ArgumentNullException"),
		Success("string.concat.generic-enumerable", "static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)", [Array(Number(1), Number(2), Number(3))], Text("123")),

        Success("string.join.character-array", "static string.Join(char, params string[])", [Text("|"), Array(Text("a"), Null(), Text("b"))], Text("a||b")),
        Success("string.join.string-array-null-separator", "static string.Join(string, params string[])", [Null(), Array(Text("a"), Text("b"))], Text("ab")),
        Success("string.join.character-span", "static string.Join(char, params System.ReadOnlySpan<string>)", [Text("/"), Array(Text("a"), Text("b"))], Text("a/b")),
        Success("string.join.string-span", "static string.Join(string, params System.ReadOnlySpan<string>)", [Text("::"), Array(Text("a"), Text("b"))], Text("a::b")),
        Success("string.join.character-range", "static string.Join(char, string[], int, int)", [Text("-"), Array(Text("a"), Text("b"), Text("c")), Number(1), Number(2)], Text("b-c")),
        Success("string.join.string-range", "static string.Join(string, string[], int, int)", [Text("--"), Array(Text("a"), Text("b"), Text("c")), Number(0), Number(2)], Text("a--b")),
        Failure("string.join.range.rejects-overflow", "static string.Join(string, string[], int, int)", [Text("-"), Array(Text("a")), Number(1), Number(1)], "ArgumentOutOfRangeException"),
        Success("string.join.string-enumerable", "static string.Join(string, System.Collections.Generic.IEnumerable<string>)", [Text(","), Array(Text("a"), Text("b"))], Text("a,b")),
		Success("string.join.object-array", "static string.Join(string, params object[])", [Text("|"), Array(Text("A"), Bool(true), Null(), Number(2))], Text("A|True||2")),
		Failure("string.join.object-array.rejects-null", "static string.Join(string, params object[])", [Text("|"), Null()], "ArgumentNullException"),
		Success("string.join.generic-enumerable", "static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)", [Text("-"), Array(Number(1), Number(2), Number(3))], Text("1-2-3")),
		Success("string.join.character-object-array", "static string.Join(char, params object[])", [Text("/"), Array(Text("A"), Bool(true), Null(), Number(2))], Text("A/True//2")),
		Success("string.join.character-object-span", "static string.Join(char, params System.ReadOnlySpan<object>)", [Text("/"), Array(Text("A"), Bool(true), Null(), Number(2))], Text("A/True//2")),
		Success("string.join.object-span", "static string.Join(string, params System.ReadOnlySpan<object>)", [Text("|"), Array(Text("A"), Bool(true), Null(), Number(2))], Text("A|True||2")),
		Success("string.join.character-generic-enumerable", "static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)", [Text("/"), Array(Number(1), Number(2), Number(3))], Text("1/2/3")),

        Success("string.pad-left.spaces", "string.PadLeft(int)", [Text("x"), Number(3)], Text("  x")),
        Success("string.pad-left.character", "string.PadLeft(int, char)", [Text("x"), Number(3), Text("0")], Text("00x")),
        Success("string.pad-right.spaces", "string.PadRight(int)", [Text("x"), Number(3)], Text("x  ")),
        Success("string.pad-right.character", "string.PadRight(int, char)", [Text("x"), Number(3), Text("0")], Text("x00")),
        Failure("string.pad-left.rejects-negative-width", "string.PadLeft(int)", [Text("x"), Number(-1)], "ArgumentOutOfRangeException"),
        Success("string.replace-line-endings.all-sequences", "string.ReplaceLineEndings(string)", [Text("a\r\nb\rc\nd\fe\u0085f\u2028g\u2029h"), Text("|")], Text("a|b|c|d|e|f|g|h")),
        Failure("string.replace-line-endings.rejects-null-replacement", "string.ReplaceLineEndings(string)", [Text("a\n"), Null()], "ArgumentNullException"),
        Success("string.replace-line-endings.default-uses-deno-newline", "string.ReplaceLineEndings()", [Text("a\r\nb\rc\nd")], Text("a\nb\nc\nd")),

        Success("string.index-of.character-ordinal-ignore-case", "string.IndexOf(char, System.StringComparison)", [Text("Alpha"), Text("a"), Number(OrdinalIgnoreCase)], Number(0)),
        Success("string.index-of.string-start-ordinal-ignore-case", "string.IndexOf(string, int, System.StringComparison)", [Text("RazorVue"), Text("vue"), Number(3), Number(OrdinalIgnoreCase)], Number(5)),
        Success("string.index-of.string-range-ordinal-ignore-case", "string.IndexOf(string, int, int, System.StringComparison)", [Text("aaBBcc"), Text("bb"), Number(1), Number(3), Number(OrdinalIgnoreCase)], Number(2)),
        Success("string.index-of-any.from-start", "string.IndexOfAny(char[], int)", [Text("abcabc"), Array(Text("a")), Number(1)], Number(3)),
        Success("string.index-of-any.bounded", "string.IndexOfAny(char[], int, int)", [Text("abcabc"), Array(Text("c")), Number(1), Number(2)], Number(2)),
        Success("string.last-index-of.character-range", "string.LastIndexOf(char, int, int)", [Text("abca"), Text("a"), Number(2), Number(3)], Number(0)),
        Success("string.last-index-of.string-start-ordinal", "string.LastIndexOf(string, int, System.StringComparison)", [Text("abcabc"), Text("abc"), Number(5), Number(Ordinal)], Number(3)),
        Success("string.last-index-of.string-range", "string.LastIndexOf(string, int, int)", [Text("abc--abc"), Text("abc"), Number(7), Number(5)], Number(5)),
        Success("string.last-index-of.string-range-ordinal-ignore-case", "string.LastIndexOf(string, int, int, System.StringComparison)", [Text("abC--ABC"), Text("abc"), Number(7), Number(8), Number(OrdinalIgnoreCase)], Number(5)),
        Success("string.last-index-of-any.from-start", "string.LastIndexOfAny(char[], int)", [Text("abcabc"), Array(Text("a")), Number(4)], Number(3)),
        Success("string.last-index-of-any.bounded", "string.LastIndexOfAny(char[], int, int)", [Text("abcabc"), Array(Text("a")), Number(4), Number(2)], Number(3)),

        Success("string.split.character-count", "string.Split(char, int, System.StringSplitOptions)", [Text("a,b,c"), Text(","), Number(2), Number(0)], Array(Text("a"), Text("b,c"))),
        Success("string.split.character-array-options", "string.Split(char[], System.StringSplitOptions)", [Text(" a, ,b "), Array(Text(",")), Number(3)], Array(Text("a"), Text("b"))),
        Success("string.split.character-array-count", "string.Split(char[], int)", [Text("a,b,c"), Array(Text(",")), Number(2)], Array(Text("a"), Text("b,c"))),
        Success("string.split.character-array-count-options", "string.Split(char[], int, System.StringSplitOptions)", [Text(" a, ,b "), Array(Text(",")), Number(2), Number(3)], Array(Text("a"), Text(",b"))),
        Success("string.split.read-only-span", "string.Split(params System.ReadOnlySpan<char>)", [Text("a,b"), Array(Text(","))], Array(Text("a"), Text("b"))),
        Success("string.split.params-character-array", "string.Split(params char[])", [Text("a,b;c"), Array(Text(","), Text(";"))], Array(Text("a"), Text("b"), Text("c"))),
        Success("string.split.string-options", "string.Split(string, System.StringSplitOptions)", [Text("a--b--"), Text("--"), Number(0)], Array(Text("a"), Text("b"), Text(""))),
        Success("string.split.string-count-options", "string.Split(string, int, System.StringSplitOptions)", [Text("a--b--"), Text("--"), Number(2), Number(0)], Array(Text("a"), Text("b--"))),
        Success("string.split.string-array-options", "string.Split(string[], System.StringSplitOptions)", [Text("a|b,c"), Array(Text("|"), Text(",")), Number(0)], Array(Text("a"), Text("b"), Text("c"))),
        Success("string.split.string-array-count-options", "string.Split(string[], int, System.StringSplitOptions)", [Text("a|b,c"), Array(Text("|"), Text(",")), Number(2), Number(0)], Array(Text("a"), Text("b,c"))),

        Success("string.trim.character", "string.Trim(char)", [Text("..value.."), Text(".")], Text("value")),
        Success("string.trim.empty-character-array-uses-whitespace", "string.Trim(params char[])", [Text("  value  "), Array()], Text("value")),
        Success("string.trim-end.character", "string.TrimEnd(char)", [Text("value.."), Text(".")], Text("value")),
        Success("string.trim-end.character-array", "string.TrimEnd(params char[])", [Text("value..--"), Array(Text("."), Text("-"))], Text("value")),
        Success("string.trim-start.character", "string.TrimStart(char)", [Text("..value"), Text(".")], Text("value")),
        Success("string.trim-start.character-array", "string.TrimStart(params char[])", [Text("--..value"), Array(Text("."), Text("-"))], Text("value")),
        Success("string.trim.read-only-character-span", "string.Trim(params System.ReadOnlySpan<char>)", [Text("--value.."), Array(Text("."), Text("-"))], Text("value")),
        Success("string.trim.read-only-character-span-empty-preserves-input", "string.Trim(params System.ReadOnlySpan<char>)", [Text("  value  "), Array()], Text("  value  ")),
        Success("string.trim-start.read-only-character-span", "string.TrimStart(params System.ReadOnlySpan<char>)", [Text("--..value"), Text(".-")], Text("value")),
        Success("string.trim-end.read-only-character-span", "string.TrimEnd(params System.ReadOnlySpan<char>)", [Text("value..--"), Array(Text("."), Text("-"))], Text("value")),
        Success("string.trim-end.read-only-character-span-empty-preserves-input", "string.TrimEnd(params System.ReadOnlySpan<char>)", [Text("value  "), Null()], Text("value  "))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
