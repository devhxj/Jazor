using Acornima.Ast;
using Jazor.Compiler;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JavaScriptIdentifierScenarioTests
{
    public static IEnumerable<TestDataRow<JavaScriptIdentifierNameScenario>> IdentifierNameCases
        => JavaScriptIdentifierScenarioCatalog.IdentifierNames.Select(static scenario =>
            new TestDataRow<JavaScriptIdentifierNameScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<JavaScriptBindingIdentifierScenario>> BindingIdentifierCases
        => JavaScriptIdentifierScenarioCatalog.BindingIdentifiers.Select(static scenario =>
            new TestDataRow<JavaScriptBindingIdentifierScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<JavaScriptModuleExportNameScenario>> ModuleExportNameCases
        => JavaScriptIdentifierScenarioCatalog.ModuleExportNames.Select(static scenario =>
            new TestDataRow<JavaScriptModuleExportNameScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsAndInputs()
    {
        var allIds = JavaScriptIdentifierScenarioCatalog.IdentifierNames.Select(static scenario => scenario.Id)
            .Concat(JavaScriptIdentifierScenarioCatalog.BindingIdentifiers.Select(static scenario => scenario.Id))
            .Concat(JavaScriptIdentifierScenarioCatalog.ModuleExportNames.Select(static scenario => scenario.Id))
            .ToArray();
        var allInputs = JavaScriptIdentifierScenarioCatalog.IdentifierNames.Select(static scenario => scenario.InputIdentity)
            .Concat(JavaScriptIdentifierScenarioCatalog.BindingIdentifiers.Select(static scenario => scenario.InputIdentity))
            .Concat(JavaScriptIdentifierScenarioCatalog.ModuleExportNames.Select(static scenario => scenario.InputIdentity))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allInputs.Length, allInputs.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("javascript-identifier.", StringComparison.Ordinal)));
        Assert.IsTrue(JavaScriptIdentifierScenarioCatalog.IdentifierNames.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(JavaScriptIdentifierScenarioCatalog.BindingIdentifiers.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(JavaScriptIdentifierScenarioCatalog.ModuleExportNames.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.HasCount(
            Enum.GetValues<JavaScriptModuleExportNameNodeKind>().Length,
            JavaScriptIdentifierScenarioCatalog.ModuleExportNames
                .Select(static scenario => scenario.ExpectedNodeKind)
                .Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(IdentifierNameCases))]
    public void IsJavaScriptIdentifierName_ClassifiesUnicodeIdentifierGrammar(
        JavaScriptIdentifierNameScenario scenario)
    {
        var actual = JavaScriptAstFactory.IsJavaScriptIdentifierName(scenario.Value);

        Assert.AreEqual(scenario.Expected, actual, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(BindingIdentifierCases))]
    public void IsJavaScriptBindingIdentifier_RejectsModuleStrictModeReservedBindings(
        JavaScriptBindingIdentifierScenario scenario)
    {
        var actual = JavaScriptAstFactory.IsJavaScriptBindingIdentifier(scenario.Value);

        Assert.AreEqual(scenario.Expected, actual, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ModuleExportNameCases))]
    public void CreateModuleExportName_UsesIdentifierNameOrEscapedStringLiteral(
        JavaScriptModuleExportNameScenario scenario)
    {
        var actual = JavaScriptAstFactory.CreateModuleExportName(scenario.Value);

        switch (scenario.ExpectedNodeKind)
        {
            case JavaScriptModuleExportNameNodeKind.Identifier:
                Assert.IsInstanceOfType<Identifier>(actual, scenario.Id);
                Assert.AreEqual(scenario.Value, ((Identifier)actual).Name, scenario.Id);
                break;
            case JavaScriptModuleExportNameNodeKind.StringLiteral:
                Assert.IsInstanceOfType<StringLiteral>(actual, scenario.Id);
                var literal = (StringLiteral)actual;
                Assert.AreEqual(scenario.Value, literal.Value, scenario.Id);
                Assert.IsTrue(literal.Raw.StartsWith('"') && literal.Raw.EndsWith('"'), scenario.Id);
                break;
            default:
                throw new InvalidOperationException(
                    $"{scenario.Id}: unsupported module export node kind '{scenario.ExpectedNodeKind}'.");
        }
    }
}

public enum JavaScriptModuleExportNameNodeKind
{
    Identifier,
    StringLiteral
}

public sealed record JavaScriptIdentifierNameScenario(
    string Id,
    string Dimension,
    string Value,
    bool Expected)
{
    public string InputIdentity => $"identifier-name|{Value}|{Expected}";
}

public sealed record JavaScriptBindingIdentifierScenario(
    string Id,
    string Dimension,
    string Value,
    bool Expected)
{
    public string InputIdentity => $"binding-identifier|{Value}|{Expected}";
}

public sealed record JavaScriptModuleExportNameScenario(
    string Id,
    string Dimension,
    string Value,
    JavaScriptModuleExportNameNodeKind ExpectedNodeKind)
{
    public string InputIdentity => $"module-export-name|{Value}|{ExpectedNodeKind}";
}

internal static class JavaScriptIdentifierScenarioCatalog
{
    private static readonly string[] ReservedBindingNames =
    [
        "arguments",
        "await",
        "break",
        "case",
        "catch",
        "class",
        "const",
        "continue",
        "debugger",
        "default",
        "delete",
        "do",
        "else",
        "enum",
        "eval",
        "export",
        "extends",
        "false",
        "finally",
        "for",
        "function",
        "if",
        "implements",
        "import",
        "in",
        "instanceof",
        "interface",
        "let",
        "new",
        "null",
        "package",
        "private",
        "protected",
        "public",
        "return",
        "static",
        "super",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "var",
        "void",
        "while",
        "with",
        "yield"
    ];

    public static IReadOnlyList<JavaScriptIdentifierNameScenario> IdentifierNames { get; } =
    [
        IdentifierName("empty", "empty-name-is-not-identifier", string.Empty, false),
        IdentifierName("ascii", "ascii-letter-start", "value", true),
        IdentifierName("underscore", "underscore-start", "_value", true),
        IdentifierName("dollar", "dollar-start", "$value", true),
        IdentifierName("decimal-part", "decimal-digit-is-valid-part", "value1", true),
        IdentifierName("decimal-start", "decimal-digit-is-invalid-start", "1value", false),
        IdentifierName("hyphen", "hyphen-is-not-identifier-part", "value-name", false),
        IdentifierName("space", "space-is-not-identifier-part", "value name", false),
        IdentifierName("cjk", "unicode-other-letter-start", "发布", true),
        IdentifierName("combining-part", "unicode-combining-mark-is-valid-part", "a\u0301", true),
        IdentifierName("combining-start", "unicode-combining-mark-is-invalid-start", "\u0301a", false),
        IdentifierName("join-control", "zero-width-join-control-is-valid-part", "a\u200Cb", true),
        IdentifierName("other-id-start", "unicode-other-id-start-character", "\u2118value", true),
        IdentifierName("other-id-continue", "unicode-other-id-continue-character", "a\u00B7b", true),
        IdentifierName("astral-letter", "unicode-astral-letter-start", "\U00010400value", true),
        IdentifierName("emoji", "emoji-is-not-identifier-start", "\U0001F680", false),
        IdentifierName("lone-high-surrogate", "unpaired-high-surrogate-is-invalid", "\uD800", false),
        IdentifierName("lone-low-surrogate", "unpaired-low-surrogate-is-invalid", "\uDC00", false),
        IdentifierName("part-high-surrogate", "unpaired-high-surrogate-is-invalid-part", "a\uD800", false),
        IdentifierName("part-low-surrogate", "unpaired-low-surrogate-is-invalid-part", "a\uDC00", false)
    ];

    public static IReadOnlyList<JavaScriptBindingIdentifierScenario> BindingIdentifiers { get; } =
        ReservedBindingNames
            .Select(static name => BindingIdentifier(
                $"reserved-{name}",
                $"module-strict-mode-reserved-binding-{name}",
                name,
                false))
            .Concat([
                BindingIdentifier("contextual-async", "async-remains-contextual", "async", true),
                BindingIdentifier("contextual-of", "of-remains-contextual", "of", true),
                BindingIdentifier("contextual-from", "from-remains-contextual", "from", true),
                BindingIdentifier("contextual-get", "get-remains-contextual", "get", true),
                BindingIdentifier("contextual-set", "set-remains-contextual", "set", true),
                BindingIdentifier("constructor", "constructor-remains-valid-binding", "constructor", true),
                BindingIdentifier("undefined", "undefined-remains-shadowable-binding", "undefined", true),
                BindingIdentifier("dollar", "dollar-prefixed-binding", "$value", true),
                BindingIdentifier("cjk", "unicode-binding", "发布", true),
                BindingIdentifier("astral", "astral-unicode-binding", "\U00010400value", true),
                BindingIdentifier("other-id-start", "other-id-start-binding", "\u2118value", true),
                BindingIdentifier("hyphen", "non-identifier-cannot-bind", "value-name", false),
                BindingIdentifier("leading-digit", "leading-digit-cannot-bind", "1value", false),
                BindingIdentifier("empty", "empty-name-cannot-bind", string.Empty, false),
                BindingIdentifier("emoji", "emoji-cannot-bind", "\U0001F680", false)
            ])
            .ToArray();

    public static IReadOnlyList<JavaScriptModuleExportNameScenario> ModuleExportNames { get; } =
    [
        ModuleExportName("ascii", "ascii-export-uses-identifier", "value", JavaScriptModuleExportNameNodeKind.Identifier),
        ModuleExportName("reserved-class", "reserved-word-is-valid-identifier-name", "class", JavaScriptModuleExportNameNodeKind.Identifier),
        ModuleExportName("module-await", "await-is-valid-export-identifier-name", "await", JavaScriptModuleExportNameNodeKind.Identifier),
        ModuleExportName("unicode", "unicode-export-uses-identifier", "发布", JavaScriptModuleExportNameNodeKind.Identifier),
        ModuleExportName("hyphen", "hyphenated-export-uses-string", "value-name", JavaScriptModuleExportNameNodeKind.StringLiteral),
        ModuleExportName("space", "spaced-export-uses-string", "value name", JavaScriptModuleExportNameNodeKind.StringLiteral),
        ModuleExportName("quote", "quoted-export-uses-escaped-string", "value\"name", JavaScriptModuleExportNameNodeKind.StringLiteral),
        ModuleExportName("emoji", "emoji-export-uses-string", "\U0001F680", JavaScriptModuleExportNameNodeKind.StringLiteral),
        ModuleExportName("empty", "empty-export-name-uses-string", string.Empty, JavaScriptModuleExportNameNodeKind.StringLiteral)
    ];

    private static JavaScriptIdentifierNameScenario IdentifierName(
        string id,
        string dimension,
        string value,
        bool expected)
        => new($"javascript-identifier.identifier-name.{id}", dimension, value, expected);

    private static JavaScriptBindingIdentifierScenario BindingIdentifier(
        string id,
        string dimension,
        string value,
        bool expected)
        => new($"javascript-identifier.binding.{id}", dimension, value, expected);

    private static JavaScriptModuleExportNameScenario ModuleExportName(
        string id,
        string dimension,
        string value,
        JavaScriptModuleExportNameNodeKind expectedNodeKind)
        => new($"javascript-identifier.module-export.{id}", dimension, value, expectedNodeKind);
}
