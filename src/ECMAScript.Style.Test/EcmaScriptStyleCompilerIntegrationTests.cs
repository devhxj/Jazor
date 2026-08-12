using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleCompilerIntegrationTests
{
    [TestMethod]
    public async Task Convert_CSharpStyleModule_ImportsEcmaScriptStyleRuntimeWithoutCompilerSpecialCase()
    {
        const string source = """
            using ECMAScript;
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            [ECMAScriptModule("styles/button.mjs")]
            public static class ButtonStyles
            {
                public static readonly string Enter = keyframes(
                [
                    new("from", new CssDeclarations { opacity = 0 }),
                    new("to", new CssDeclarations { opacity = 1 })
                ]);

                public static readonly string Button = style(new CssRule
                {
                    display = inline_flex,
                    width = percent(100) - rem(2),
                    gap = rem(0.5),
                    color = var_or("--button-color", color("red")),
                    background_color = hex("1769aa"),
                    border = important(px(1) | solid | hex("d7ebe4")),
                    padding = important(px(8) | px(12)),
                    backdrop_filter = filters(blur(px(12)), saturate(1.15)),
                    transition_duration = ms(180),
                    opacity = 0.9,
                    grid_column = 2,
                    box_shadow = shadows(
                        new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05)),
                        new CssShadow(px(0), px(1), Color: current_color)),
                    ["--button-gap"] = rem(0.5),
                    children =
                    [
                        new(ChildKind.Selector, "&:hover", new CssRule
                        {
                            color = color("blue")
                        })
                    ]
                });
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
            path: "/src/ButtonStyles.cs");
        var compilation = CSharpCompilation.Create(
            "EcmaScriptStyleConsumer",
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(css).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var symbol = semanticModel.GetDeclaredSymbol(declaration);
        Assert.IsNotNull(symbol);

        var module = await new AstConverter(symbol, semanticModel).Convert();
        var script = module?.ToKnRECMAScript() ?? string.Empty;

        StringAssert.Contains(script, "from \"style.mjs\"");
        StringAssert.Contains(script, "keyframes([{ selector: \"from\", declarations: { opacity: 0 } }, { selector: \"to\", declarations: { opacity: 1 } }])");
        StringAssert.Contains(script, "style(");
        StringAssert.Contains(script, "display: inlineFlex");
        StringAssert.Contains(script, "calc(${percent(100)} - ${rem(2)})");
        StringAssert.Contains(script, "gap: rem(0.5)");
        StringAssert.Contains(script, "color: varOr(\"--button-color\", color(\"red\"))");
        StringAssert.Contains(script, "\"background-color\": hex(\"1769aa\")");
        StringAssert.Contains(script, "border: importantValue(px(1) + \" \" + solid + \" \" + hex(\"d7ebe4\"))");
        StringAssert.Contains(script, "padding: importantValue(px(8) + \" \" + px(12))");
        StringAssert.Contains(script, "\"backdrop-filter\": filters([blur(px(12)), saturate(1.15)])");
        StringAssert.Contains(script, "\"transition-duration\": ms(180)");
        StringAssert.Contains(script, "opacity: 0.9");
        StringAssert.Contains(script, "\"grid-column\": 2");
        StringAssert.Contains(script, "\"box-shadow\": shadows([");
        Assert.DoesNotContain("class CssShadow", script);
        StringAssert.Contains(script, "\"--button-gap\"");
        StringAssert.Contains(script, "$children");
    }

    [TestMethod]
    public async Task Convert_ContextAtRulesAndSnapshots_UseOrdinaryModuleImportsAndStructuralRecords()
    {
        const string source = """
            using ECMAScript;
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            [ECMAScriptModule("styles/server.mjs")]
            public static class ServerStyles
            {
                private static readonly CssContext Context = context(new CssOptions
                {
                    Detached = true,
                    StyleId = "server-css"
                });

                public static readonly string Card = style(Context, new CssRule
                {
                    container_type = keyword("inline-size"),
                    children =
                    [
                        new(ChildKind.Container, "card (width > 30rem)", new CssRule
                        {
                            display = grid
                        })
                    ]
                });

                public static CssSnapshot BuildSnapshot()
                {
                    at_rule(Context, new CssAtRule(
                        "font-face",
                        new CssDeclarations
                        {
                            font_family = raw("Example Sans"),
                            ["src"] = raw("url(example.woff2)")
                        }));
                    return snapshot(Context);
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
            path: "/src/ServerStyles.cs");
        var compilation = CSharpCompilation.Create(
            "EcmaScriptStyleContextConsumer",
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(css).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var symbol = semanticModel.GetDeclaredSymbol(declaration);
        Assert.IsNotNull(symbol);

        var module = await new AstConverter(symbol, semanticModel).Convert();
        var script = module?.ToKnRECMAScript() ?? string.Empty;

        StringAssert.Contains(script, "context");
        StringAssert.Contains(script, "styleIn");
        StringAssert.Contains(script, "atRuleIn");
        StringAssert.Contains(script, "snapshotFrom");
        StringAssert.Contains(script, "detached: true");
        StringAssert.Contains(script, "kind: \"container\"");
        StringAssert.Contains(script, "name: \"font-face\"");
        StringAssert.Contains(script, "\"font-family\": raw(\"Example Sans\")");
    }

    [TestMethod]
    public async Task Convert_AnchorAndSizingValues_UseBoundOverloadExportsAndNarrowDomains()
    {
        const string source = """
            using ECMAScript;
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            [ECMAScriptModule("styles/anchor.mjs")]
            public static class AnchorStyles
            {
                private static readonly CssAnchorName Card = anchor_name("--card");

                public static readonly string Popover = style(new CssRule
                {
                    anchor_name = Card,
                    position_anchor = Card,
                    width = calc_size(min_content, size + rem(1)),
                    height = anchor_size(Card, anchor_inline, rem(20)),
                    top = anchor(Card, anchor_bottom, rem(0.5)),
                    inset = inset_sides(anchor(anchor_top), anchor_size(Card, anchor_block)),
                    margin_top = anchor_size(Card, anchor_inline),
                    column_width = fit_content(percent(50))
                });
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
            path: "/src/AnchorStyles.cs");
        var compilation = CSharpCompilation.Create(
            "EcmaScriptStyleAnchorConsumer",
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(css).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var symbol = semanticModel.GetDeclaredSymbol(declaration);
        Assert.IsNotNull(symbol);

        var module = await new AstConverter(symbol, semanticModel).Convert();
        var script = module?.ToKnRECMAScript() ?? string.Empty;

        StringAssert.Contains(script, "anchorName(\"--card\")");
        StringAssert.Contains(script, "calcSize(minContent, size + \" + \" + rem(1))");
        StringAssert.Contains(script, "anchorSizeNamedAxis(Card, anchorInline)");
        StringAssert.Contains(script, "anchorNamedFallback(Card, anchorBottom, rem(0.5))");
        StringAssert.Contains(script, "insetSides2(anchor(anchorTop), anchorSizeNamedAxis(Card, anchorBlock))");
        StringAssert.Contains(script, "\"anchor-name\": Card");
        StringAssert.Contains(script, "\"position-anchor\": Card");
        StringAssert.Contains(script, "\"column-width\": fitContent(percent(50))");
    }

    [TestMethod]
    public void Compile_ValueDomains_RejectCrossDomainAndImplicitStringAssignments()
    {
        const string source = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class InvalidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    width = deg(10),
                    color = rem(1),
                    height = "10px",
                    column_width = percent(100) - rem(2)
                };
            }
            """;
        var compilation = CreateCompilation(source, "InvalidStyleConsumer");
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.HasCount(4, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        StringAssert.Contains(errors[0].GetMessage(), nameof(CssAngle));
        StringAssert.Contains(errors[1].GetMessage(), nameof(CssLength));
        StringAssert.Contains(errors[2].GetMessage(), "string");
        StringAssert.Contains(errors[3].GetMessage(), nameof(CssLengthPercentage));
    }

    [TestMethod]
    public void Compile_TypedAnchorAndSizingSyntax_CompilesWithoutRawWidthFallback()
    {
        const string source = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class FutureStyles
            {
                public static readonly CssRule Rule = new()
                {
                    width = calc_size(min_content, size + rem(2)),
                    height = anchor_size(anchor_name("--card"), anchor_inline, rem(20)),
                    top = anchor(anchor_name("--card"), anchor_bottom, rem(1)),
                    inset = inset_sides(anchor(anchor_top), anchor_size(anchor_name("--card"), anchor_block)),
                    color = raw("oklch(from var(--brand) l c h)")
                };
            }
            """;
        var errors = CreateCompilation(source, "FutureStyleConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
    }

    [TestMethod]
    public void Compile_BoxShadow_RequiresItsNarrowDomain()
    {
        const string validSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class ValidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    box_shadow = shadows(new CssShadow(px(0), px(4), Blur: px(12), Color: var("--shadow-color"))),
                    webkit_box_shadow = var("--shadow"),
                    ["--shadow"] = shadows(new CssShadow(px(0), px(2), Blur: px(8), Color: rgba(0, 0, 0, 0.2)))
                };
            }
            """;
        var validErrors = CreateCompilation(validSource, "ValidBoxShadowConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(validErrors, string.Join(Environment.NewLine, validErrors.Select(static error => error.ToString())));

        const string invalidSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class InvalidStyles
            {
                public static readonly CssRule Rule = new() { box_shadow = px(4) };
            }
            """;
        var invalidErrors = CreateCompilation(invalidSource, "InvalidBoxShadowConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(1, invalidErrors, string.Join(Environment.NewLine, invalidErrors.Select(static error => error.ToString())));
        StringAssert.Contains(invalidErrors[0].GetMessage(), nameof(CssLength));
    }

    [TestMethod]
    public void Compile_BorderPipeAndImportant_StayWithinTheirPropertyDomains()
    {
        const string validSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class ValidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    border = px(1) | solid | var("--border-color"),
                    border_top = thin | dashed | current_color,
                    display = important(flex),
                    padding = important(px(8) | px(12))
                };
            }
            """;
        var validErrors = CreateCompilation(validSource, "ValidBorderPipeConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(validErrors, string.Join(Environment.NewLine, validErrors.Select(static error => error.ToString())));

        const string invalidSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class InvalidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    width = px(1) | solid,
                    height = important(hex("fff")),
                    box_shadow = shadows(new CssShadow(px(0), px(2), Blur: important(px(8))))
                };
            }
            """;
        var invalidErrors = CreateCompilation(invalidSource, "InvalidBorderPipeConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(3, invalidErrors, string.Join(Environment.NewLine, invalidErrors.Select(static error => error.ToString())));
        StringAssert.Contains(invalidErrors[0].GetMessage(), nameof(CssBorder));
        StringAssert.Contains(invalidErrors[1].GetMessage(), nameof(CssImportant<CssColor>));
        StringAssert.Contains(invalidErrors[2].GetMessage(), nameof(CssImportant<CssLength>));
    }

    private static CSharpCompilation CreateCompilation(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(css).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
