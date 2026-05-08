using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorCarrierBridgeTests
{
    [TestMethod]
    public void RazorSourceGenerator_HostOutput_ExposesCodeDocumentForComponent()
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
            assemblyName: "RazorVue.RazorIr.SdkSourceGenerator.CarrierBridge",
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
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/Counter.razor"))
                }
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorSourceGenerator().AsSourceGenerator()],
            additionalTexts: [additionalText],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));

        var generatorResult = driver.GetRunResult().Results.Single();
        var razorGeneratorResult = GetHostOutputValue(generatorResult, "RazorGeneratorResult");
        Assert.IsNotNull(razorGeneratorResult, "The SDK Razor source generator did not publish RazorGeneratorResult.");

        var codeDocument = GetCodeDocument(razorGeneratorResult!, documentPath);
        Assert.IsNotNull(codeDocument, "RazorGeneratorResult.GetCodeDocument(...) returned null for the Razor component physical path.");

        var source = codeDocument!.GetType().GetProperty("Source")?.GetValue(codeDocument);
        var filePath = source?.GetType().GetProperty("FilePath")?.GetValue(source) as string;
        Assert.AreEqual(documentPath, filePath, "The bridged RazorCodeDocument source file path was not preserved.");
    }

    private static object? GetHostOutputValue(object generatorResult, string key)
    {
        var property = generatorResult.GetType().GetProperty("HostOutputs");
        Assert.IsNotNull(property, "GeneratorRunResult.HostOutputs was not available.");

        if (property.GetValue(generatorResult) is not IEnumerable entries)
        {
            Assert.Fail("GeneratorRunResult.HostOutputs did not implement IEnumerable.");
            return null;
        }

        foreach (var entry in entries)
        {
            Assert.IsNotNull(entry, "HostOutputs contained a null entry.");

            var entryType = entry!.GetType();
            var entryKey = entryType.GetProperty("Key")?.GetValue(entry) as string;
            if (!string.Equals(entryKey, key, StringComparison.Ordinal))
            {
                continue;
            }

            return entryType.GetProperty("Value")?.GetValue(entry);
        }

        return null;
    }

    private static object? GetCodeDocument(object razorGeneratorResult, string documentPath)
    {
        var method = razorGeneratorResult.GetType().GetMethod(
            "GetCodeDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(string)],
            modifiers: null);
        Assert.IsNotNull(method, "RazorGeneratorResult.GetCodeDocument(string) was not available.");
        return method!.Invoke(razorGeneratorResult, [documentPath]);
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
