using System.Text;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorBootstrapPatchTests
{
    [TestMethod]
    public void DriverCompletionHook_BindsOfficialGeneratedCSharpWithoutRazorHostOutputs()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.DriverCompletion",
            [CSharpSyntaxTree.ParseText(
                """
                using ECMAScript;
                using static ECMAScript.Vue3;
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                }
                """,
                parseOptions,
                "Pages/Counter.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var options = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "11.0",
                ["build_property.RootNamespace"] = "Demo",
                ["build_property.MSBuildProjectDirectory"] = @"D:\repo\Demo"
            },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/Counter.razor"))
                }
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RazorVueGenerator().AsSourceGenerator(), new RazorSourceGenerator().AsSourceGenerator()],
            [new InMemoryAdditionalText(documentPath, "<button>Counter</button>")],
            parseOptions,
            options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics));
        Assert.IsTrue(outputCompilation.SyntaxTrees.Any(static tree => tree.FilePath.EndsWith("_razor.g.cs", StringComparison.Ordinal)));
        var catalog = outputCompilation.SyntaxTrees.SingleOrDefault(
            static tree => tree.FilePath == "obj/Jazor.RazorVue/Jazor.Generated.VueRenderCatalog.g.cs");
        Assert.IsNotNull(catalog);
        StringAssert.Contains(catalog!.GetText().ToString(), "components/counter.mjs");
        Assert.AreEqual(
            0,
            outputCompilation.GetDiagnostics().Count(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error),
            string.Join(Environment.NewLine, outputCompilation.GetDiagnostics()));
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private sealed class TestAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalOptions,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> additionalFileOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _additionalFileOptions = additionalFileOptions;
        private static readonly AnalyzerConfigOptions Empty = new TestAnalyzerConfigOptions(new Dictionary<string, string>());

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _additionalFileOptions.TryGetValue(textFile.Path, out var values)
                ? new TestAnalyzerConfigOptions(values)
                : Empty;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
            => values.TryGetValue(key, out value!);
    }
}
