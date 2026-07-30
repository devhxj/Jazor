namespace Jazor.CLR.Test;

internal static class ClrRuntimeCharScenarios
{
    private const string ModulePath = "System/CharModule.js";
    private const string GrinningFace = "\uD83D\uDE00";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("char.compare.less-than", "char.CompareTo(object)", [Text("A"), Text("B")], Number(-1)),
        Success("char.compare.equal", "char.CompareTo(object)", [Text("A"), Text("A")], Number(0)),
        Success("char.compare.null-is-before-value", "char.CompareTo(object)", [Text("A"), Null()], Number(1)),
        Failure("char.compare.wrong-type", "char.CompareTo(object)", [Text("A"), Number(65)], "ArgumentException"),
        Success("char.parse.single-code-unit", "static char.Parse(string)", [Text("Z")], Text("Z")),
        Failure("char.parse.null", "static char.Parse(string)", [Null()], "ArgumentNullException"),
        Failure("char.parse.empty", "static char.Parse(string)", [Text("")], "FormatException"),
        Failure("char.parse.surrogate-pair", "static char.Parse(string)", [Text(GrinningFace)], "FormatException"),
        Success("char.try-parse.single-code-unit", "static char.TryParse(string, out char)", [Text("Z"), Text("\0")], Array(Bool(true), Text("Z"))),
        Success("char.try-parse.null", "static char.TryParse(string, out char)", [Null(), Text("X")], Array(Bool(false), Text("\0"))),
        Success("char.try-parse.surrogate-pair", "static char.TryParse(string, out char)", [Text(GrinningFace), Text("X")], Array(Bool(false), Text("\0"))),
        Success("char.white-space.ascii-space", "static char.IsWhiteSpace(char)", [Text(" ")], Bool(true)),
        Success("char.white-space.no-break-space", "static char.IsWhiteSpace(char)", [Text("\u00A0")], Bool(true)),
        Success("char.white-space.letter", "static char.IsWhiteSpace(char)", [Text("A")], Bool(false)),
        Success("char.control.null-code-unit", "static char.IsControl(char)", [Text("\0")], Bool(true)),
        Success("char.control.unit-separator", "static char.IsControl(char)", [Text("\u001F")], Bool(true)),
        Success("char.control.printable-letter", "static char.IsControl(char)", [Text("A")], Bool(false)),
        Success("char.digit.string-index", "static char.IsDigit(string, int)", [Text("a7z"), Number(1)], Bool(true)),
        Success("char.digit.string-index-nondigit", "static char.IsDigit(string, int)", [Text("a7z"), Number(0)], Bool(false)),
        Failure("char.digit.string-index-out-of-range", "static char.IsDigit(string, int)", [Text("a7z"), Number(3)], "ArgumentOutOfRangeException"),
        Success("char.letter.string-index", "static char.IsLetter(string, int)", [Text("7Az"), Number(1)], Bool(true)),
        Success("char.letter-or-digit.string-index-digit", "static char.IsLetterOrDigit(string, int)", [Text("-9"), Number(1)], Bool(true)),
        Success("char.surrogate-pair.valid-pair", "static char.IsSurrogatePair(string, int)", [Text(GrinningFace), Number(0)], Bool(true)),
        Success("char.surrogate-pair.last-code-unit", "static char.IsSurrogatePair(string, int)", [Text(GrinningFace), Number(1)], Bool(false)),
        Success("char.convert-to-utf32.surrogate-pair", "static char.ConvertToUtf32(string, int)", [Text(GrinningFace), Number(0)], Number(128512)),
        Success("char.convert-to-utf32.bmp-code-unit", "static char.ConvertToUtf32(string, int)", [Text("A"), Number(0)], Number(65)),
        Success("char.numeric-value.ascii-digit", "static char.GetNumericValue(char)", [Text("7")], Number(7)),
        Success("char.numeric-value.nondigit", "static char.GetNumericValue(char)", [Text("A")], Number(-1)),
        Success("char.numeric-value.string-index", "static char.GetNumericValue(string, int)", [Text("a7"), Number(1)], Number(7)),
        Success("char.control.string-index", "static char.IsControl(string, int)", [Text("A\0"), Number(1)], Bool(true)),
        Success("char.high-surrogate.string-index", "static char.IsHighSurrogate(string, int)", [Text(GrinningFace), Number(0)], Bool(true)),
        Success("char.low-surrogate.string-index", "static char.IsLowSurrogate(string, int)", [Text(GrinningFace), Number(1)], Bool(true)),
        Success("char.lower.string-index", "static char.IsLower(string, int)", [Text("xY"), Number(0)], Bool(true)),
        Success("char.surrogate.string-index", "static char.IsSurrogate(string, int)", [Text(GrinningFace), Number(0)], Bool(true)),
        Success("char.upper.string-index", "static char.IsUpper(string, int)", [Text("xY"), Number(1)], Bool(true)),
        Success("char.white-space.string-index", "static char.IsWhiteSpace(string, int)", [Text("A\u00A0"), Number(1)], Bool(true))
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
