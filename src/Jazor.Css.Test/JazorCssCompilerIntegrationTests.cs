using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.Css.Tests;

[TestClass]
public sealed class JazorCssCompilerIntegrationTests
{
    [TestMethod]
    public async Task Convert_CSharpStyleModule_ImportsJazorCssRuntimeWithoutCompilerSpecialCase()
    {
        const string source = """
            using ECMAScript;
            using Jazor.Css;

            namespace Demo;

            [ECMAScriptModule("styles/button.mjs")]
            public static class ButtonStyles
            {
                public static readonly string Button = Css.Class(new CssRule
                {
                    Display = "inline-flex",
                    Color = "red",
                    BackgroundColor = "#1769aa",
                    ["--button-gap"] = "0.5rem",
                    Children =
                    [
                        new(CssChildKind.Selector, "&:hover", new CssRule
                        {
                            Color = "blue"
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
            "JazorCssConsumer",
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Css).Assembly.Location)
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

        StringAssert.Contains(script, "from \"Jazor.Css/runtime.mjs\"");
        StringAssert.Contains(script, "css(");
        StringAssert.Contains(script, "display: \"inline-flex\"");
        StringAssert.Contains(script, "\"background-color\": \"#1769aa\"");
        StringAssert.Contains(script, "\"--button-gap\"");
        StringAssert.Contains(script, "$children");
    }
}
