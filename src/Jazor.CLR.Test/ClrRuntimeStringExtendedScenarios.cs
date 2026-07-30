namespace Jazor.CLR.Test;

internal static class ClrRuntimeStringExtendedScenarios
{
    private const string ModulePath = "System/StringModule.js";
    private const int Ordinal = 4;
    private const int OrdinalIgnoreCase = 5;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("string.compare.ordinal-ignore-case", "static string.Compare(string, string, System.StringComparison)", [Text("Alpha"), Text("alpha"), Number(OrdinalIgnoreCase)], Number(0)),
        Success("string.format.three-arguments", "static string.Format(string, object, object, object)", [Text("{0}/{1}/{2}"), Text("src"), Text("app"), Text("main")], Text("src/app/main")),
        Success("string.equals.instance-ordinal-ignore-case", "string.Equals(string, System.StringComparison)", [Text("Admin"), Text("admin"), Number(OrdinalIgnoreCase)], Bool(true)),

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
        Success("string.trim-end.character", "string.TrimEnd(char)", [Text("value.."), Text(".")], Text("value")),
        Success("string.trim-end.character-array", "string.TrimEnd(params char[])", [Text("value..--"), Array(Text("."), Text("-"))], Text("value")),
        Success("string.trim-start.character", "string.TrimStart(char)", [Text("..value"), Text(".")], Text("value")),
        Success("string.trim-start.character-array", "string.TrimStart(params char[])", [Text("--..value"), Array(Text("."), Text("-"))], Text("value"))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
