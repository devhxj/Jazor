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
		Success("char.digit.unicode-decimal", "static char.IsDigit(char)", [Text("\u0661")], Bool(true)),
		Success("char.letter.cjk", "static char.IsLetter(char)", [Text("\u6C49")], Bool(true)),
		Success("char.upper.greek", "static char.IsUpper(char)", [Text("\u03A9")], Bool(true)),
		Success("char.lower.greek", "static char.IsLower(char)", [Text("\u03C9")], Bool(true)),
		Success("char.punctuation.em-dash", "static char.IsPunctuation(char)", [Text("\u2014")], Bool(true)),
		Success("char.letter-or-digit.cjk", "static char.IsLetterOrDigit(char)", [Text("\u6C49")], Bool(true)),
        Success("char.control.null-code-unit", "static char.IsControl(char)", [Text("\0")], Bool(true)),
        Success("char.control.unit-separator", "static char.IsControl(char)", [Text("\u001F")], Bool(true)),
        Success("char.control.printable-letter", "static char.IsControl(char)", [Text("A")], Bool(false)),
        Success("char.digit.string-index", "static char.IsDigit(string, int)", [Text("a7z"), Number(1)], Bool(true)),
		Success("char.digit.string-index-unicode-decimal", "static char.IsDigit(string, int)", [Text("a\u0661z"), Number(1)], Bool(true)),
        Success("char.digit.string-index-nondigit", "static char.IsDigit(string, int)", [Text("a7z"), Number(0)], Bool(false)),
        Failure("char.digit.string-index-out-of-range", "static char.IsDigit(string, int)", [Text("a7z"), Number(3)], "ArgumentOutOfRangeException"),
		Failure("char.digit.string-index-fractional", "static char.IsDigit(string, int)", [Text("a7z"), Number(1.5)], "ArgumentOutOfRangeException"),
        Success("char.letter.string-index", "static char.IsLetter(string, int)", [Text("7Az"), Number(1)], Bool(true)),
		Success("char.letter.string-index-cjk", "static char.IsLetter(string, int)", [Text("7\u6C49z"), Number(1)], Bool(true)),
        Success("char.letter-or-digit.string-index-digit", "static char.IsLetterOrDigit(string, int)", [Text("-9"), Number(1)], Bool(true)),
		Success("char.letter-or-digit.string-index-unicode-decimal", "static char.IsLetterOrDigit(string, int)", [Text("-\u0661"), Number(1)], Bool(true)),
		Success("char.number.letter-number", "static char.IsNumber(char)", [Text("\u2167")], Bool(true)),
		Success("char.number.string-index-other-number", "static char.IsNumber(string, int)", [Text("x\u00B2"), Number(1)], Bool(true)),
		Success("char.punctuation.string-index-em-dash", "static char.IsPunctuation(string, int)", [Text("x\u2014"), Number(1)], Bool(true)),
		Success("char.separator.line", "static char.IsSeparator(char)", [Text("\u2028")], Bool(true)),
		Success("char.separator.string-index-paragraph", "static char.IsSeparator(string, int)", [Text("x\u2029"), Number(1)], Bool(true)),
		Success("char.symbol.currency", "static char.IsSymbol(char)", [Text("\u20AC")], Bool(true)),
		Success("char.symbol.string-index-math", "static char.IsSymbol(string, int)", [Text("x+"), Number(1)], Bool(true)),
		Success("char.unicode-category.uppercase-letter", "static char.GetUnicodeCategory(char)", [Text("A")], Number(0)),
		Success("char.unicode-category.lowercase-letter", "static char.GetUnicodeCategory(char)", [Text("a")], Number(1)),
		Success("char.unicode-category.titlecase-letter", "static char.GetUnicodeCategory(char)", [Text("\u01C5")], Number(2)),
		Success("char.unicode-category.modifier-letter", "static char.GetUnicodeCategory(char)", [Text("\u02B0")], Number(3)),
		Success("char.unicode-category.other-letter", "static char.GetUnicodeCategory(char)", [Text("\u6C49")], Number(4)),
		Success("char.unicode-category.non-spacing-mark", "static char.GetUnicodeCategory(char)", [Text("\u0301")], Number(5)),
		Success("char.unicode-category.spacing-combining-mark", "static char.GetUnicodeCategory(char)", [Text("\u0903")], Number(6)),
		Success("char.unicode-category.enclosing-mark", "static char.GetUnicodeCategory(char)", [Text("\u20DD")], Number(7)),
		Success("char.unicode-category.decimal-digit-number", "static char.GetUnicodeCategory(char)", [Text("\u0661")], Number(8)),
		Success("char.unicode-category.letter-number", "static char.GetUnicodeCategory(char)", [Text("\u2167")], Number(9)),
		Success("char.unicode-category.other-number", "static char.GetUnicodeCategory(char)", [Text("\u00B2")], Number(10)),
		Success("char.unicode-category.space-separator", "static char.GetUnicodeCategory(char)", [Text("\u00A0")], Number(11)),
		Success("char.unicode-category.line-separator", "static char.GetUnicodeCategory(char)", [Text("\u2028")], Number(12)),
		Success("char.unicode-category.paragraph-separator", "static char.GetUnicodeCategory(char)", [Text("\u2029")], Number(13)),
		Success("char.unicode-category.control", "static char.GetUnicodeCategory(char)", [Text("\0")], Number(14)),
		Success("char.unicode-category.format", "static char.GetUnicodeCategory(char)", [Text("\u200D")], Number(15)),
		Success("char.unicode-category.surrogate", "static char.GetUnicodeCategory(string, int)", [Text(GrinningFace), Number(0)], Number(16)),
		Success("char.unicode-category.private-use", "static char.GetUnicodeCategory(char)", [Text("\uE000")], Number(17)),
		Success("char.unicode-category.connector-punctuation", "static char.GetUnicodeCategory(char)", [Text("_")], Number(18)),
		Success("char.unicode-category.dash-punctuation", "static char.GetUnicodeCategory(char)", [Text("\u2014")], Number(19)),
		Success("char.unicode-category.open-punctuation", "static char.GetUnicodeCategory(char)", [Text("(")], Number(20)),
		Success("char.unicode-category.close-punctuation", "static char.GetUnicodeCategory(char)", [Text(")")], Number(21)),
		Success("char.unicode-category.initial-quote-punctuation", "static char.GetUnicodeCategory(char)", [Text("\u2018")], Number(22)),
		Success("char.unicode-category.final-quote-punctuation", "static char.GetUnicodeCategory(char)", [Text("\u2019")], Number(23)),
		Success("char.unicode-category.other-punctuation", "static char.GetUnicodeCategory(char)", [Text("!")], Number(24)),
		Success("char.unicode-category.math-symbol", "static char.GetUnicodeCategory(char)", [Text("+")], Number(25)),
		Success("char.unicode-category.currency-symbol", "static char.GetUnicodeCategory(char)", [Text("$")], Number(26)),
		Success("char.unicode-category.modifier-symbol", "static char.GetUnicodeCategory(char)", [Text("^")], Number(27)),
		Success("char.unicode-category.other-symbol", "static char.GetUnicodeCategory(char)", [Text("\u00A9")], Number(28)),
		Success("char.unicode-category.other-not-assigned", "static char.GetUnicodeCategory(char)", [Text("\u0378")], Number(29)),
		Success("char.unicode-category.string-index-currency", "static char.GetUnicodeCategory(string, int)", [Text("x\u20AC"), Number(1)], Number(26)),
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
		Success("char.lower.string-index-greek", "static char.IsLower(string, int)", [Text("x\u03C9"), Number(1)], Bool(true)),
        Success("char.surrogate.string-index", "static char.IsSurrogate(string, int)", [Text(GrinningFace), Number(0)], Bool(true)),
        Success("char.upper.string-index", "static char.IsUpper(string, int)", [Text("xY"), Number(1)], Bool(true)),
		Success("char.upper.string-index-greek", "static char.IsUpper(string, int)", [Text("x\u03A9"), Number(1)], Bool(true)),
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
