using System.Collections.Immutable;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class OrdinaryLibraryAnalyzerTests
{
    [TestMethod]
    public void FinalGeneratorHook_ExistingSourceModuleWithoutImportManifest_PreservesCatalog()
    {
        Assert.IsTrue(Jazor.RazorVue.Generation.InitializeHookInstaller.TryInstall());
        var compilation = CSharpCompilation.Create("SourceOnly", [CSharpSyntaxTree.ParseText("""
            using ECMAScript;
            [ECMAScriptModule("source.mjs")]
            public static class SourceOnly { public static int Read() => 7; }
            """, new CSharpParseOptions(LanguageVersion.Preview))], RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new ESGenerator().AsSourceGenerator()],
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        Assert.IsEmpty(diagnostics);
        Assert.IsNotNull(output.Assembly.GetTypeByMetadataName("Jazor.Generated.ModuleCatalog"));
        Assert.IsNull(LibraryImportContext.ForCompilation(output));
        Assert.IsEmpty(RazorSgTestHost.GetCompilationErrors(output));
    }

    [TestMethod]
    [DataRow("namespace Backend; public class Service { public int Value; }")]
    [DataRow("namespace Backend; public class Container { public class Child { public int Value; } }")]
    [DataRow("namespace Jazor.Generated { internal class ModuleCatalog {} }")]
    public void FinalCompilationWithoutFrontendRoots_ProducesNoCatalog(string source)
    {
        var compilation = CSharpCompilation.Create("NoFrontend", [CSharpSyntaxTree.ParseText(source)],
            RazorSgTestHost.CreateMetadataReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsTrue(Jazor.RazorVue.Generation.RazorTailOutput.TryBuildFinalCompilationCatalog(compilation,
            CancellationToken.None, out var catalog, out var diagnostics));
        Assert.IsNull(catalog);
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public void FinalCompilationWithoutComponents_EmitsSelectedOrdinaryModuleAndReportsLoweringFailure()
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("ordinary-tail");
        try
        {
            var references = RazorSgTestHost.CreateMetadataReferences();
            var library = CSharpCompilation.Create("PlainTail", [CSharpSyntaxTree.ParseText("""
                namespace PlainTail;
                public static class Helpers
                {
                    public static int Read() => 3;
                    public static bool Backend() => System.IO.File.Exists("server");
                }
                """, new CSharpParseOptions(LanguageVersion.Preview))], references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var path = Path.Combine(root, "PlainTail.dll");
            Assert.IsTrue(library.Emit(path).Success);
            foreach (var useBackend in new[] { false, true })
            {
                var source = "using ECMAScript; [ECMAScriptModule(\"app.mjs\")] public static class App { public static " +
                    (useBackend ? "bool Run() => PlainTail.Helpers.Backend();" : "int Run() => PlainTail.Helpers.Read();") + " }";
                var compilation = CSharpCompilation.Create("Consumer", [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
                    references.Append(MetadataReference.CreateFromFile(path)), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                LibraryImportContext.Configure(compilation, [new("PlainTail", path)]);
                var success = Jazor.RazorVue.Generation.RazorTailOutput.TryBuildFinalCompilationCatalog(compilation,
                    CancellationToken.None, out var catalog, out var diagnostics);
                Assert.AreEqual(!useBackend, success);
                if (useBackend)
                    Assert.IsFalse(diagnostics.IsEmpty);
                else
                    StringAssert.Contains(catalog!, "_jazor/libraries/PlainTail-");
            }
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [TestMethod]
    public async Task OfficialRazor_ImportsOrdinaryInstanceMethodsWithRealVueProxyStorage()
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("ordinary-razor");
        try
        {
            var compilation = CSharpCompilation.Create("PlainRazor", [CSharpSyntaxTree.ParseText("""
                namespace PlainRazor;
                public sealed class Counter
                {
                    private int value;
                    public Counter(int initial) { value = initial; }
                    public int Read() => value;
                }
                """, new CSharpParseOptions(LanguageVersion.Preview))], RazorSgTestHost.CreateMetadataReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var path = Path.Combine(root, "PlainRazor.dll");
            var emit = compilation.Emit(path);
            Assert.IsTrue(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
            var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
                RazorSgTestHost.GetTestDocumentPath("Components/OrdinaryCounter.razor"),
                "<p>@counter.Read()</p>",
                """
                namespace Demo.Components;
                [ECMAScriptModule("./components/ordinary-counter")]
                public partial class OrdinaryCounter : ComponentBase, IVueComponent
                {
                    private readonly PlainRazor.Counter counter = new(7);
                }
                """, "Demo.Components", "Demo.Components.OrdinaryCounter",
                additionalReferences: [MetadataReference.CreateFromFile(path)],
                libraryImports: [new("PlainRazor", path)]);
            Assert.HasCount(1, observation.ImportedModules);
            await RazorSgOfficialNodeRuntimeTestHost.RunModuleTestAsync(
                "components/ordinary-counter.js", observation.ModuleText, "ordinary-razor.test.mjs", """
                import assert from 'node:assert/strict';
                import component from './components/ordinary-counter.js';
                function text(node) {
                    if (Array.isArray(node)) return node.map(text).join('');
                    if (node && typeof node === 'object') return text(node.children);
                    return node == null ? '' : String(node);
                }
                globalThis.__jazorTest('ordinary DLL object works through real Vue reactive state', () => {
                    const render = component.setup({}, { slots: {}, attrs: {} });
                    assert.equal(text(render()), '7');
                });
                """, supportingModules: observation.ImportedModules.ToDictionary(static module => module.RelativePath, static module => module.Artifact.Content),
                vueModuleSpecifier: "vue", packageDependencies: new Dictionary<string, string> { ["vue"] = "3.5.42" });
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [TestMethod]
    public async Task SelectedImplementation_AnalyzerAcceptsOriginalDllCallsAndInstanceSignatures()
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("ordinary-analyzer");
        try
        {
            var references = RazorSgTestHost.CreateMetadataReferences();
            var library = CSharpCompilation.Create("PlainAnalyzer", [CSharpSyntaxTree.ParseText("""
                namespace PlainAnalyzer;
                public sealed class Model { public Model(int value) { Value = value; } public int Value { get; } }
                public static class Factory { public static Model Create(int value) => new Model(value); }
                """, new CSharpParseOptions(LanguageVersion.Preview))], references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var path = Path.Combine(root, "PlainAnalyzer.dll");
            var emit = library.Emit(path);
            Assert.IsTrue(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
            var compilation = CSharpCompilation.Create("Consumer", [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                [ECMAScriptModule("app.mjs")]
                public static class App
                {
                    public static PlainAnalyzer.Model Create() => PlainAnalyzer.Factory.Create(7);
                    public static int Run() => Create().Value;
                }
                """, new CSharpParseOptions(LanguageVersion.Preview))],
                references.Append(MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var diagnostics = await compilation.WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new Jazor.Analyzer.Analyzer()),
                new AnalyzerOptions([new Manifest("asset|PlainAnalyzer|" + path)]))
                .GetAnalyzerDiagnosticsAsync();
            Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [TestMethod]
    public async Task MissingImplementation_ReportsImportDiagnosticWithoutAnalyzerCrash()
    {
        var compilation = CSharpCompilation.Create("Consumer", [CSharpSyntaxTree.ParseText("""
            using ECMAScript;
            [ECMAScriptModule("app.mjs")]
            public static class App { public static int Run() => 7; }
            """, new CSharpParseOptions(LanguageVersion.Preview))], RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = await compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new Jazor.Analyzer.Analyzer()),
            new AnalyzerOptions([new Manifest("asset|MissingImplementation")])).GetAnalyzerDiagnosticsAsync();
        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "AD0001"), string.Join(Environment.NewLine, diagnostics));
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR007" && diagnostic.GetMessage().Contains("MissingImplementation", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, diagnostics));
    }

    private sealed class Manifest(string content) : AdditionalText
    {
        public override string Path => "consumer.jazorimports";
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
    }
}
