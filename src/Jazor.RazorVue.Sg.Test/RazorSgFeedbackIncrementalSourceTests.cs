using System.Text;
using System.Text.RegularExpressions;
using Jazor.Common.SourceMaps;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgFeedbackIncrementalSourceTests
{
    [TestMethod]
    public void SameDriver_EditedAdditionalText_RefreshesInputChecksumGeneratedChecksumAndDiagnosticPosition()
    {
        var path = RazorSgTestHost.GetTestDocumentPath("Pages/IncrementalChecksumFeedback.razor");
        var first = new Document(path, "<div>@MissingFirst</div>");
        var second = new Document(path, "<header>edited</header>\n\n<section>prefix @MissingSecond</section>");
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create("Feedback.SourceChecksum",
            [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using static ECMAScript.Vue;
                namespace Demo.Pages;
                [ECMAScriptModule("./components/incremental-checksum-feedback")]
                public partial class IncrementalChecksumFeedback : ComponentBase, IVueComponent { }
                """, parseOptions, path + ".cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RazorSourceGenerator().AsSourceGenerator()],
            [first], parseOptions, new Options(path));

        var firstResult = Run(ref driver, compilation, first, "MissingFirst");
        driver = driver.ReplaceAdditionalText(first, second);
        var secondResult = Run(ref driver, compilation, second, "MissingSecond");
        Assert.AreNotEqual(firstResult.SourceChecksum, secondResult.SourceChecksum);
        Assert.AreNotEqual(firstResult.GeneratedChecksum, secondResult.GeneratedChecksum);
        Assert.AreNotEqual(firstResult.Position, secondResult.Position);
        Assert.AreEqual(new LinePosition(0, 6), firstResult.Position);
        Assert.AreEqual(new LinePosition(2, 17), secondResult.Position);
        Assert.IsFalse(secondResult.Generated.Contains("MissingFirst", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task SamePath_EditedValidSource_RefreshesVueArtifactAndSourceMapContent()
    {
        var path = RazorSgTestHost.GetTestDocumentPath("Pages/EditedArtifactFeedback.razor");
        const string codeBehind = """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/edited-artifact-feedback")]
            public partial class EditedArtifactFeedback : ComponentBase, IVueComponent { }
            """;
        const string first = "<div>first revision</div>";
        const string second = "<header>new revision</header>\n<section>edited source</section>";
        var oldArtifact = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            path, first, codeBehind, "Demo.Pages", "Demo.Pages.EditedArtifactFeedback");
        var newArtifact = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            path, second, codeBehind, "Demo.Pages", "Demo.Pages.EditedArtifactFeedback");
        Assert.AreNotEqual(oldArtifact.ModuleText, newArtifact.ModuleText);
        Assert.IsFalse(newArtifact.ModuleText.Contains("first revision", StringComparison.Ordinal));
        var map = new SourceMapReader().Read(newArtifact.SourceMapContent);
        Assert.IsTrue(map.Sources.Any(source => source.Content == second));
        Assert.IsFalse(map.Sources.Any(source => source.Content == first));
    }

    private static Observation Run(ref GeneratorDriver driver, Compilation compilation, Document document, string missing)
    {
        // The same driver sees changed AdditionalText, exactly as an incremental Razor SG build does.
        using var scope = RazorSourceTextRegistry.Push(document.Path, document.GetText().ToString());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        Assert.IsFalse(diagnostics.Any(item => item.Id.StartsWith("RZ", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, diagnostics));
        var generated = driver.GetRunResult().Results.Single().GeneratedSources
            .Single(source => source.HintName.EndsWith("_razor.g.cs", StringComparison.Ordinal)).SourceText;
        var sourceChecksum = Convert.ToHexString(document.GetText().GetChecksum().AsSpan());
        var generatedText = generated.ToString();
        var checksum = Regex.Match(generatedText, "#pragma checksum \"[^\"]+\" \"[^\"]+\" \"(?<checksum>[0-9a-fA-F]+)\"");
        Assert.IsTrue(checksum.Success, generatedText);
        Assert.AreEqual(sourceChecksum, checksum.Groups["checksum"].Value.ToUpperInvariant());
        var error = RazorSgTestHost.GetCompilationErrorDiagnostics(output)
            .Single(item => item.Id == "CS0103" && item.GetMessage().Contains(missing, StringComparison.Ordinal));
        var mapped = error.Location.GetMappedLineSpan();
        Assert.AreEqual(document.Path, mapped.Path);
        return new Observation(sourceChecksum,
            Convert.ToHexString(generated.GetChecksum().AsSpan()), mapped.StartLinePosition, generatedText);
    }

    private sealed record Observation(string SourceChecksum, string GeneratedChecksum, LinePosition Position, string Generated);

    private sealed class Document(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text, Encoding.UTF8, SourceHashAlgorithm.Sha256);
        public override string Path { get; } = path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }

    private sealed class Options(string path) : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty = new Values(new Dictionary<string, string>());
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Values(new Dictionary<string, string>
        {
            ["build_property.RazorLangVersion"] = "11.0",
            ["build_property.RootNamespace"] = "Demo.Pages",
            ["build_property.SupportLocalizedComponentNames"] = "true",
            ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
            ["build_property.MSBuildProjectDirectory"] = Path.GetDirectoryName(path)!
        });
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new Values(new Dictionary<string, string>
        {
            ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(Path.GetFileName(textFile.Path)))
        });
    }

    private sealed class Values(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }
}
