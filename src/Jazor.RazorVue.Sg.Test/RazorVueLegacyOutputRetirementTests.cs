using Jazor.RazorVue.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueLegacyOutputRetirementTests
{
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
                    using static ECMAScript.Vue3;
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
}
