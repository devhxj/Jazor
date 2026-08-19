using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueLegacyOutputRetirementTests
{
    [TestMethod]
    public void Generator_RegistersRazorAdditionalTextAsFinalCompilationCarrier()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorSourceCarrier.Tests",
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorVueGenerator().AsSourceGenerator()],
            additionalTexts:
            [
                new InMemoryAdditionalText("Pages/Counter.razor", "<button>Counter</button>"),
                new InMemoryAdditionalText("Pages/Ignore.cs", "internal sealed class Ignore; ")
            ],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics));
        var source = driver.GetRunResult()
            .Results
            .Single()
            .GeneratedSources
            .Single();
        Assert.AreEqual(RazorSourceTextRegistry.CarrierHintName, source.HintName);
        StringAssert.Contains(source.SourceText.ToString(), "RazorSourceTextCatalog", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Generator_DoesNotEmitRetiredCatalog_ForHandwrittenBuildRenderTree()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.LegacyOutputRetirement.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    using ECMAScript;
                    using static ECMAScript.Vue;
                    using Microsoft.AspNetCore.Components;
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo;

                    [ECMAScriptModule("./components/counter")]
                    public sealed class Counter : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.AddContent(0, "counter");
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "Counter.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorVueGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var generatedSources = driver.GetRunResult()
            .Results
            .Single()
            .GeneratedSources;

        Assert.IsFalse(
            generatedSources.Any(static source => source.HintName == "Jazor.Generated.RazorVueCatalog.g.cs"),
            "The retired Razor-to-SFC catalog must not be emitted.");
        Assert.IsFalse(
            generatedSources.Any(static source => source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal)),
            "The retired Razor-to-SFC artifact source must not be emitted.");
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }
}
