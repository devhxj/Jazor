namespace Jazor.CLR.Test;

internal static class ClrRuntimeStringScenarios
{
    private const string ModulePath = "System/StringModule.js";
    private const int Ordinal = 4;
    private const int OrdinalIgnoreCase = 5;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("string.compare.both-null", "static string.Compare(string, string)", [Null(), Null()], Number(0)),
        Success("string.compare.null-before-value", "static string.Compare(string, string)", [Null(), Text("a")], Number(-1)),
        Success("string.compare.value-after-null", "static string.Compare(string, string)", [Text("a"), Null()], Number(1)),
        Success("string.compare.ordinal-less-than", "static string.Compare(string, string)", [Text("alpha"), Text("beta")], Number(-1)),
        Success("string.compare.ignore-case-equal", "static string.Compare(string, string, bool)", [Text("Alpha"), Text("alpha"), Bool(true)], Number(0)),
        Success("string.compare.case-sensitive-order", "static string.Compare(string, string, bool)", [Text("Alpha"), Text("alpha"), Bool(false)], Number(-1)),
        Success(
            "string.compare.substring-ignore-case",
            "static string.Compare(string, int, string, int, int, System.StringComparison)",
            [Text("xxALPHAyy"), Number(2), Text("--alpha++"), Number(2), Number(5), Number(OrdinalIgnoreCase)],
            Number(0)),
        Success("string.compare-ordinal.equal", "static string.CompareOrdinal(string, string)", [Text("same"), Text("same")], Number(0)),
        Success("string.compare-to.null", "string.CompareTo(string)", [Text("value"), Null()], Number(1)),
        Failure("string.compare-to.wrong-object-type", "string.CompareTo(object)", [Text("value"), Number(3)], "ArgumentException"),
        Success("string.ends-with.ignore-case", "string.EndsWith(string, System.StringComparison)", [Text("Report.JSON"), Text(".json"), Number(OrdinalIgnoreCase)], Bool(true)),
        Success("string.equals.ignore-case", "static string.Equals(string, string, System.StringComparison)", [Text("Admin"), Text("admin"), Number(OrdinalIgnoreCase)], Bool(true)),
        Success("string.equals.ordinal-different-case", "static string.Equals(string, string, System.StringComparison)", [Text("Admin"), Text("admin"), Number(Ordinal)], Bool(false)),
        Success("string.starts-with.ignore-case", "string.StartsWith(string, System.StringComparison)", [Text("Jazor.Compiler"), Text("jazor"), Number(OrdinalIgnoreCase)], Bool(true)),
        Success("string.indexer.valid-code-unit", "string.this[int].get", [Text("abc"), Number(1)], Text("b")),
        Failure("string.indexer.negative-index", "string.this[int].get", [Text("abc"), Number(-1)], "IndexOutOfRangeException"),
        Failure("string.indexer.index-equals-length", "string.this[int].get", [Text("abc"), Number(3)], "IndexOutOfRangeException"),
        Success("string.format.single-string-argument", "static string.Format(string, object)", [Text("Hello, {0}!"), Text("Jazor")], Text("Hello, Jazor!")),
        Success("string.format.two-string-arguments", "static string.Format(string, object, object)", [Text("{0}/{1}"), Text("src"), Text("app")], Text("src/app")),
        Success("string.format.parameter-array", "static string.Format(string, params object[])", [Text("{0}-{1}-{2}"), Array(Text("a"), Text("b"), Text("c"))], Text("a-b-c")),
        Success("string.replace.ignore-case-all-occurrences", "string.Replace(string, string, System.StringComparison)", [Text("one ONE One"), Text("one"), Text("1"), Number(OrdinalIgnoreCase)], Text("1 1 1")),
        Success("string.replace.character-all-occurrences", "string.Replace(char, char)", [Text("banana"), Text("a"), Text("o")], Text("bonono")),
        Success("string.split.character-keep-empty", "string.Split(char, System.StringSplitOptions)", [Text("a,,b"), Text(","), Number(0)], Array(Text("a"), Text(""), Text("b"))),
        Success("string.split.character-trim-remove-empty", "string.Split(char, System.StringSplitOptions)", [Text(" a, ,b "), Text(","), Number(3)], Array(Text("a"), Text("b"))),
        Success("string.trim.character-set", "string.Trim(params char[])", [Text("__value--"), Array(Text("_"), Text("-"))], Text("value")),
        Success("string.contains.ignore-case", "string.Contains(string, System.StringComparison)", [Text("ReleaseQueue"), Text("queue"), Number(OrdinalIgnoreCase)], Bool(true)),
        Success("string.contains.character-ignore-case", "string.Contains(char, System.StringComparison)", [Text("Admin"), Text("a"), Number(OrdinalIgnoreCase)], Bool(true)),
        Success("string.index-of.character-bounded-range", "string.IndexOf(char, int, int)", [Text("abcabc"), Text("a"), Number(1), Number(3)], Number(3)),
        Success("string.index-of-any.character-array", "string.IndexOfAny(char[])", [Text("compiler"), Array(Text("x"), Text("p"))], Number(3)),
        Success("string.index-of.substring-bounded-range", "string.IndexOf(string, int, int)", [Text("abc--abc"), Text("abc"), Number(1), Number(5)], Number(-1)),
        Success("string.index-of.substring-ignore-case", "string.IndexOf(string, System.StringComparison)", [Text("RazorVue"), Text("vue"), Number(OrdinalIgnoreCase)], Number(5)),
        Success("string.last-index-of-any.character-array", "string.LastIndexOfAny(char[])", [Text("compiler"), Array(Text("c"), Text("e"))], Number(6)),
        Success("string.last-index-of.substring-ignore-case", "string.LastIndexOf(string, System.StringComparison)", [Text("one TWO two"), Text("TWO"), Number(OrdinalIgnoreCase)], Number(8))
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

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
