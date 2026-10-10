using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorCompilationTimingTests
{
    [TestMethod]
    public void FinalCompilationTiming_WritesConfiguredSidecarWithoutChangingCatalogOrDiagnostics()
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("compilation-timing");
        try
        {
            var compilation = CreateCompilation();
            var logPath = Path.Combine(root, "timing.log");
            var timed = RunGenerator(compilation, logPath);
            var untimed = RunGenerator(compilation, null);

            Assert.IsTrue(File.Exists(logPath));
            var line = File.ReadAllText(logPath);
            StringAssert.Contains(line, "components=1; status=completed; total=", StringComparison.Ordinal);
            StringAssert.Contains(line, "generators=", StringComparison.Ordinal);
            StringAssert.Contains(line, "final-validation=", StringComparison.Ordinal);
            StringAssert.Contains(line, "artifact-emission=", StringComparison.Ordinal);
            StringAssert.Contains(line, "catalog-attach=", StringComparison.Ordinal);
            Assert.IsFalse(timed.SyntaxTrees.Any(RazorSourceTextRegistry.IsCarrierTree));
            Assert.AreEqual(CatalogSource(untimed), CatalogSource(timed));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void FinalCompilationTiming_UnwritableSidecarPreservesSuccessfulCompilation()
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("compilation-timing-unwritable");
        try
        {
            // A directory is deliberately an invalid file sink. Only optional timing I/O may
            // fail; the render catalog and the original compilation diagnostics remain valid.
            var output = RunGenerator(CreateCompilation(), root);
            Assert.IsNotNull(CatalogSource(output));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static Compilation RunGenerator(CSharpCompilation compilation, string? timingPath)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RazorVueGenerator().AsSourceGenerator()],
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
            optionsProvider: new TimingOptionsProvider(timingPath));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
        Assert.IsEmpty(RazorSgTestHost.GetCompilationErrors(output));
        return output;
    }

    private static string CatalogSource(Compilation compilation)
        => compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("Jazor.Generated.ModuleCatalog.g.cs", StringComparison.Ordinal)).GetText().ToString();

    private static CSharpCompilation CreateCompilation()
        => CSharpCompilation.Create("RazorVue.CompilationTiming." + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Rendering;
                using static ECMAScript.Vue;
                namespace Demo;
                [ECMAScriptModule("./components/timed")]
                public sealed class TimedComponent : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "timed");
                    }
                }
                """, new CSharpParseOptions(LanguageVersion.Preview), "TimedComponent.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private sealed class TimingOptionsProvider(string? timingPath) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _options = new TimingOptions(timingPath);
        public override AnalyzerConfigOptions GlobalOptions => _options;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _options;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _options;
    }

    private sealed class TimingOptions(string? timingPath) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            value = timingPath ?? string.Empty;
            return key == "build_property.JazorCompilationTimingPath" && timingPath is not null;
        }
    }
}
