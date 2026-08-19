using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;
using System.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorFinalCompilationTests
{
    [TestMethod]
    public void RazorSourceGenerator_SingleRun_ProducesGeneratedSourceWithoutHostOutputs()
    {
        var documentPath = RazorSgTestHost.GetTestDocumentPath("Pages/Counter.razor");
        var projectDirectory = Path.GetDirectoryName(documentPath)!;
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Sg.SdkSourceGenerator.FinalCompilation",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    internal static class EntryPoint
                    {
                    }
                    """,
                    options: parseOptions,
                    path: "EntryPoint.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var additionalText = new InMemoryAdditionalText(documentPath, documentText);
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "9.0",
                ["build_property.RootNamespace"] = "Demo",
                ["build_property.SupportLocalizedComponentNames"] = "true",
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.MSBuildProjectDirectory"] = projectDirectory
            },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    // The SDK Razor source generator expects TargetPath metadata to be UTF-8 base64 encoded.
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/Counter.razor"))
                }
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorSourceGenerator().AsSourceGenerator()],
            additionalTexts: [additionalText],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var runResult = driver.GetRunResult();
        var generatorResult = runResult.Results.Single();
        var generatedSources = generatorResult.GeneratedSources;
        var compilationErrors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(0, compilationErrors.Length, string.Join(Environment.NewLine, compilationErrors.Select(static item => item.ToString())));
        Assert.IsTrue(generatedSources.Length > 0, "The SDK Razor source generator did not emit any generated source.");
        Assert.IsTrue(
            generatedSources.Any(static source => source.SourceText.ToString().Contains("BuildRenderTree", StringComparison.Ordinal)),
            "The SDK Razor source generator did not emit the expected component render method.");
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
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _additionalFileOptions.TryGetValue(textFile.Path, out var values)
                ? new TestAnalyzerConfigOptions(values)
                : EmptyOptions;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values = values;

        public override bool TryGetValue(string key, out string value)
            => _values.TryGetValue(key, out value!);
    }
}
