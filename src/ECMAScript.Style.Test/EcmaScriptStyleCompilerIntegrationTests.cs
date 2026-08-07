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
                    new("from", new CssDeclarations { Opacity = 0 }),
                    new("to", new CssDeclarations { Opacity = 1 })
                ]);

                public static readonly string Button = style(new CssRule
                {
                    Display = inlineFlex,
                    Width = percent(100) - rem(2),
                    Gap = rem(0.5),
                    Color = varOr("--button-color", color("red")),
                    BackgroundColor = hex("1769aa"),
                    Border = px(1) | solid | hex("d7ebe4"),
                    BackdropFilter = filters(blur(px(12)), saturate(1.15)),
                    TransitionDuration = ms(180),
                    Opacity = 0.9,
                    GridColumn = 2,
                    BoxShadow = shadows(
                        new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05)),
                        new CssShadow(px(0), px(1), Color: currentColor)),
                    ["--button-gap"] = rem(0.5),
                    Children =
                    [
                        new(CssChildKind.Selector, "&:hover", new CssRule
                        {
                            Color = color("blue")
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
        StringAssert.Contains(script, "border: px(1) + \" \" + solid + \" \" + hex(\"d7ebe4\")");
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
                    ContainerType = keyword("inline-size"),
                    Children =
                    [
                        new(CssChildKind.Container, "card (width > 30rem)", new CssRule
                        {
                            Display = grid
                        })
                    ]
                });

                public static CssSnapshot BuildSnapshot()
                {
                    atRule(Context, new CssAtRule(
                        "font-face",
                        new CssDeclarations
                        {
                            FontFamily = raw("Example Sans"),
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
                    Width = deg(10),
                    Color = rem(1),
                    Height = "10px",
                    ColumnWidth = percent(100) - rem(2)
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
    public void Compile_RawEscapeHatch_AllowsFutureSyntaxExplicitly()
    {
        const string source = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class FutureStyles
            {
                public static readonly CssRule Rule = new()
                {
                    Width = raw("anchor-size(--card inline, 20rem)"),
                    Color = raw("oklch(from var(--brand) l c h)")
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
                    BoxShadow = shadows(new CssShadow(px(0), px(4), Blur: px(12), Color: var("--shadow-color"))),
                    WebkitBoxShadow = var("--shadow"),
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
                public static readonly CssRule Rule = new() { BoxShadow = px(4) };
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
    public void Compile_BorderPipe_ProducesOnlyTheBorderDomain()
    {
        const string validSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace Demo;

            public static class ValidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    Border = px(1) | solid | var("--border-color"),
                    BorderTop = thin | dashed | currentColor,
                    Additional =
                    [
                        important("display", flex)
                    ]
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
                    Width = px(1) | solid
                };
            }
            """;
        var invalidErrors = CreateCompilation(invalidSource, "InvalidBorderPipeConsumer")
            .GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(1, invalidErrors, string.Join(Environment.NewLine, invalidErrors.Select(static error => error.ToString())));
        StringAssert.Contains(invalidErrors[0].GetMessage(), nameof(CssBorder));
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
