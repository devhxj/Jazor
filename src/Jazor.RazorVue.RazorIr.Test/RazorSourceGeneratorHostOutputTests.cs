using System.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;
using System.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorHostOutputTests
{
    [TestMethod]
    public void RazorSourceGenerator_SingleRun_ProducesGeneratedSourceAndHostOutput()
    {
        const string projectDirectory = @"D:\repo\Demo";
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.SdkSourceGenerator.HostOutput",
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
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var additionalText = new InMemoryAdditionalText(documentPath, documentText);
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "9.0",
                ["build_property.RootNamespace"] = "Demo",
                ["build_property.SupportLocalizedComponentNames"] = "true",
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.MSBuildProjectDirectory"] = projectDirectory,
                ["build_property.EnableRazorHostOutputs"] = "true"
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
        var hostOutputs = ReadHostOutputs(generatorResult);
        var compilationErrors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(0, compilationErrors.Length, string.Join(Environment.NewLine, compilationErrors.Select(static item => item.ToString())));
        Assert.IsTrue(generatedSources.Length > 0, "The SDK Razor source generator did not emit any generated source.");
        Assert.IsTrue(
            generatedSources.Any(static source => source.SourceText.ToString().Contains("BuildRenderTree", StringComparison.Ordinal)),
            "The SDK Razor source generator did not emit the expected component render method.");
        Assert.IsTrue(
            hostOutputs.Any(static entry => string.Equals(entry.ValueTypeName, "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorGeneratorResult", StringComparison.Ordinal)),
            "The SDK Razor source generator did not publish RazorGeneratorResult into HostOutputs.");
    }

    private static IReadOnlyList<HostOutputEntry> ReadHostOutputs(object generatorResult)
    {
        var property = generatorResult.GetType().GetProperty("HostOutputs");
        Assert.IsNotNull(property, "GeneratorRunResult.HostOutputs was not available.");

        var value = property.GetValue(generatorResult);
        Assert.IsNotNull(value, "GeneratorRunResult.HostOutputs returned null.");

        if (value is not IEnumerable entries)
        {
            Assert.Fail("GeneratorRunResult.HostOutputs did not implement IEnumerable.");
            return Array.Empty<HostOutputEntry>();
        }

        var results = new List<HostOutputEntry>();
        foreach (var entry in entries)
        {
            Assert.IsNotNull(entry, "HostOutputs contained a null entry.");

            var entryType = entry.GetType();
            var key = entryType.GetProperty("Key")?.GetValue(entry) as string;
            var outputValue = entryType.GetProperty("Value")?.GetValue(entry);
            results.Add(new HostOutputEntry(
                key ?? string.Empty,
                outputValue?.GetType().FullName ?? "<null>"));
        }

        return results;
    }

    private sealed record HostOutputEntry(string Key, string ValueTypeName);

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
