using System.Collections.Immutable;
using Jazor.Analyzer.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgFinalDocumentBindingTests
{
    [TestMethod]
    public void Bind_ReusesHookCompilation_WhenCurrentGeneratedTreeAlreadyExists()
    {
        var fixture = CreateFixture();

        var adapted = RazorSgFinalDocumentAdapter.TryCreateBatch(
            fixture.Compilation,
            ImmutableArray.Create(fixture.Input),
            out var batch,
            out var adaptationFailure);

        Assert.IsTrue(adapted, adaptationFailure);

        var bound = RazorSgGeneratedCSharpBinder.TryBind(
            batch!,
            out var result,
            out var bindingFailure);

        Assert.IsTrue(bound, bindingFailure);
        Assert.AreEqual(RazorSgCompilationBindingMode.ReusedHookCompilation, result!.BindingMode);
        Assert.AreSame(fixture.Compilation, result.Compilation);

        var component = result.Components.Single();
        Assert.AreEqual("Demo.Pages.Counter", component.ComponentSymbol.ToDisplayString());
        Assert.IsNotNull(component.BuildRenderTreeBody);
        Assert.IsInstanceOfType<IBlockOperation>(component.BuildRenderTreeBody);
    }

    [TestMethod]
    public void Bind_DerivesHookCompilationOnce_WhenCurrentGeneratedTreeIsMissing()
    {
        var fixture = CreateFixture();
        var hookCompilation = fixture.Compilation.RemoveSyntaxTrees(fixture.GeneratedTree);

        var adapted = RazorSgFinalDocumentAdapter.TryCreateBatch(
            hookCompilation,
            ImmutableArray.Create(fixture.Input),
            out var batch,
            out var adaptationFailure);

        Assert.IsTrue(adapted, adaptationFailure);

        var bound = RazorSgGeneratedCSharpBinder.TryBind(
            batch!,
            out var result,
            out var bindingFailure);

        Assert.IsTrue(bound, bindingFailure);
        Assert.AreEqual(RazorSgCompilationBindingMode.DerivedHookCompilation, result!.BindingMode);
        Assert.AreNotSame(hookCompilation, result.Compilation);
        Assert.AreEqual(hookCompilation.Assembly.Identity, result.Compilation.Assembly.Identity);
        Assert.AreEqual(hookCompilation.References.Count(), result.Compilation.References.Count());
        Assert.AreEqual(hookCompilation.Options, result.Compilation.Options);
        Assert.AreEqual(1, result.Components.Length);
        Assert.IsNotNull(result.Components[0].BuildRenderTreeBody);
    }

    [TestMethod]
    public void TailOutput_EmitsVueRenderCatalogAndFinalDocumentEvidenceWithoutLegacyCatalog()
    {
        var fixture = CreateFixture();
        var documents = ImmutableArray.Create(
            new RazorSourceGeneratorDocumentOutput(
                fixture.Input.HintName,
                fixture.Input.CodeDocument,
                fixture.Input.CSharpDocument));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new FinalDocumentTailOutputGenerator(documents).AsSourceGenerator()
            ],
            parseOptions: (CSharpParseOptions?)fixture.Compilation.SyntaxTrees.First().Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            fixture.Compilation,
            out _,
            out var diagnostics);

        Assert.AreEqual(
            0,
            diagnostics.Length,
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
        var generatedSources = driver.GetRunResult()
            .Results
            .Single()
            .GeneratedSources;
        var evidence = generatedSources.Single(
            static source => source.HintName == "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs");
        var catalog = generatedSources.Single(
            static source => source.HintName == "Jazor.Generated.VueRenderCatalog.g.cs");
        var catalogSource = catalog.SourceText.ToString();

        StringAssert.Contains(evidence.SourceText.ToString(), "BindingMode = \"ReusedHookCompilation\"");
        StringAssert.Contains(evidence.SourceText.ToString(), "ComponentCount = 1");
        StringAssert.Contains(catalogSource, "internal static partial class VueRenderCatalog");
        StringAssert.Contains(catalogSource, "SchemaVersion = 1");
        StringAssert.Contains(catalogSource, "RuntimeProtocolVersion = 1");
        StringAssert.Contains(catalogSource, "GetModules()");
        StringAssert.Contains(catalogSource, "components/counter.mjs");
        StringAssert.Contains(catalogSource, "sha256:");
        StringAssert.Contains(catalogSource, "SourceMapRelativePath");
        StringAssert.Contains(catalogSource, "SourceMapContent");
        StringAssert.Contains(catalogSource, "MapHash");
        StringAssert.Contains(catalogSource, "components/counter.mjs.map");
        StringAssert.Contains(catalogSource, "Pages/Counter.razor");
        StringAssert.Contains(catalogSource, "defineComponent");
        StringAssert.Contains(catalogSource, "createRenderContext");
        Assert.IsFalse(
            catalogSource.Contains(@"D:\repo", StringComparison.OrdinalIgnoreCase),
            "The VueRenderCatalog carrier must not persist machine-absolute source paths.");
        Assert.IsFalse(
            generatedSources.Any(static source => source.HintName == "Jazor.Generated.RazorVueCatalog.g.cs"),
            "The final-document G0 tail must not emit the legacy SFC catalog.");
    }

    private static RazorSgFixture CreateFixture()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string hintName = "Counter.razor.g.cs";
        const string documentText = """
            <button @onclick="Increment">@count</button>
            """;
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.FinalDocument.Binding.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using Microsoft.AspNetCore.Components;
                    global using Microsoft.AspNetCore.Components.Rendering;
                    global using ECMAScript;
                    global using static ECMAScript.Vue3;
                    """,
                    options: parseOptions,
                    path: "GlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Demo.Pages
                    {
                        [ECMAScriptModule("./components/counter")]
                        public partial class Counter : ComponentBase, IVueComponent
                        {
                            private int count;

                            private void Increment()
                            {
                                count++;
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "Counter.razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorSgTestDocumentFactory.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorSgTestDocumentFactory.DiscoverTagHelpers(projectEngine, baseCompilation);
        var codeDocument = projectEngine.Process(
            RazorSgTestDocumentFactory.CreateSourceDocument(documentPath, SourceText.From(documentText)),
            RazorFileKind.Component,
            ImmutableArray<RazorSourceDocument>.Empty,
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorSgTestDocumentFactory.GetRequiredCSharpDocument(codeDocument);
        var generatedTree = CSharpSyntaxTree.ParseText(
            csharpDocument.Text,
            options: parseOptions,
            path: hintName);
        var compilation = baseCompilation.AddSyntaxTrees(generatedTree);
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
            0,
            errors.Length,
            string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        return new RazorSgFixture(
            compilation,
            generatedTree,
            new RazorSgTailDocumentInput(hintName, codeDocument, csharpDocument));
    }

    private sealed record RazorSgFixture(
        Compilation Compilation,
        SyntaxTree GeneratedTree,
        RazorSgTailDocumentInput Input);

    [Generator]
    private sealed class FinalDocumentTailOutputGenerator(
        ImmutableArray<RazorSourceGeneratorDocumentOutput> documents) : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(
                context.CompilationProvider,
                (outputContext, compilation) => RazorSourceGeneratorTailOutput.EmitDocuments(
                    outputContext,
                    compilation,
                    documents,
                    new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: true)));
        }
    }
}
