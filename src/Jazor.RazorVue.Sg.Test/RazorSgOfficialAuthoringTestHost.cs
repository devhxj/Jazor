using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.RazorVue.Sg.Test;

internal static class RazorSgOfficialAuthoringTestHost
{
    private const string GlobalUsingsSource = """
        global using Microsoft.AspNetCore.Components;
        global using Microsoft.AspNetCore.Components.Web;
        global using Microsoft.AspNetCore.Components.Rendering;
        global using ECMAScript;
        global using static ECMAScript.Vue;
        """;

    public static async Task<RazorSgOfficialAuthoringObservation> BuildComponentAsync(
        string documentPath,
        string documentText,
        string codeBehindSource,
        string rootNamespace,
        string componentMetadataName,
        IReadOnlyDictionary<string, string>? supportingSources = null)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTrees = ImmutableArray.CreateBuilder<SyntaxTree>();
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(
            GlobalUsingsSource,
            options: parseOptions,
            path: "GlobalUsings.g.cs"));
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(
            codeBehindSource,
            options: parseOptions,
            path: documentPath + ".cs"));
        if (supportingSources is not null)
        {
            foreach (var source in supportingSources.OrderBy(static item => item.Key, StringComparer.Ordinal))
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                    source.Value,
                    options: parseOptions,
                    path: source.Key));
            }
        }

        var baseCompilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.OfficialAuthoring.Tests",
            syntaxTrees: syntaxTrees,
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectDirectory = Path.GetDirectoryName(documentPath);
        Assert.IsFalse(string.IsNullOrWhiteSpace(projectDirectory), "Official Razor document path must have a parent directory.");
        var additionalText = new InMemoryAdditionalText(documentPath, documentText);
        using var sourceTextScope = RazorSourceTextRegistry.Push(documentPath, documentText);
        var optionsProvider = new OfficialAuthoringAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "11.0",
                ["build_property.RootNamespace"] = rootNamespace,
                ["build_property.SupportLocalizedComponentNames"] = "true",
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.MSBuildProjectDirectory"] = projectDirectory!
            },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(Path.GetFileName(documentPath)))
                }
            });
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorSourceGenerator().AsSourceGenerator()],
            additionalTexts: [additionalText],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider);
        driver = driver.RunGeneratorsAndUpdateCompilation(baseCompilation, out var compilation, out var diagnostics);
        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics));
        var generatedSource = driver.GetRunResult().Results
            .Single()
            .GeneratedSources
            .Single(source => source.HintName.EndsWith("_razor.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName(componentMetadataName);
        Assert.IsNotNull(componentSymbol, "Official Razor SG did not produce the requested component symbol.");
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var component = binding!.Components.Single();
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(
                binding,
                component,
                out var closure,
                out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        return new RazorSgOfficialAuthoringObservation(
            generatedSource.ReplaceLineEndings("\n"),
            artifact.ModuleText.ReplaceLineEndings("\n"),
            artifact.SourceMapContent.ReplaceLineEndings("\n"));
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private sealed class OfficialAuthoringAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalOptions,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> additionalFileOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new OfficialAuthoringAnalyzerConfigOptions(globalOptions);
        private static readonly AnalyzerConfigOptions EmptyOptions = new OfficialAuthoringAnalyzerConfigOptions(
            new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => additionalFileOptions.TryGetValue(textFile.Path, out var values)
                ? new OfficialAuthoringAnalyzerConfigOptions(values)
                : EmptyOptions;
    }

    private sealed class OfficialAuthoringAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
            => values.TryGetValue(key, out value!);
    }

    public static void AssertDirectRenderModule(string moduleText)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(moduleText), "Official Razor authoring produced a blank Vue module.");
        Assert.IsFalse(moduleText.Contains(".vue", StringComparison.Ordinal), moduleText);
        Assert.IsFalse(moduleText.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), moduleText);
        Assert.IsFalse(moduleText.Contains("builder.finish()", StringComparison.Ordinal), moduleText);
    }
}

internal sealed record RazorSgOfficialAuthoringObservation(
    string GeneratedCSharp,
    string ModuleText,
    string SourceMapContent);
