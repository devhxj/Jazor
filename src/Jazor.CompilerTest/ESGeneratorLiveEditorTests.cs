using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ESGeneratorLiveEditorTests
{
    [TestMethod]
    [DataRow(null, 2)]
    [DataRow("", 2)]
    [DataRow("editor-tool", 1)]
    public void RunGenerator_DefersUnboundNpmCallsOnlyInLiveEditor(string? editorTool, int diagnosticCount)
    {
        const string source = """
            using ECMAScript;

            [ECMAScriptModule("ready.mjs")]
            public static class Ready
            {
                public static int Read() => 42;
            }

            [ECMAScriptModule("pending.mjs")]
            public static class Pending
            {
                public static int Read() => Npm.Package.Exports.Read();
            }

            [ECMAScriptModule("unsupported.mjs")]
            public static class Unsupported
            {
                public static event System.Action? Changed;
            }
            """;
        var syntax = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, "App.cs");
        var compilation = CSharpCompilation.Create("EditorModules", [syntax], TestMetadataReferences.Net11.Add(
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create([new ESGenerator().AsSourceGenerator()],
            parseOptions: TestMetadataReferences.PreviewParseOptions,
            optionsProvider: new EditorOptionsProvider(editorTool)).RunGenerators(compilation);
        var result = driver.GetRunResult();
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Id == "JAZORG001").ToArray();

        Assert.HasCount(diagnosticCount, diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.GetMessage().Contains("Unsupported", StringComparison.Ordinal)));
        Assert.AreEqual(diagnosticCount == 2,
            diagnostics.Any(diagnostic => diagnostic.GetMessage().Contains("Pending", StringComparison.Ordinal)));
        var catalog = result.GeneratedTrees.Single().GetText().ToString();
        StringAssert.Contains(catalog, "ready.mjs");
        StringAssert.Contains(catalog, "return 42");
    }

    private sealed class EditorOptionsProvider(string? editorTool) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new EditorOptions(editorTool);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
    }

    private sealed class EditorOptions(string? editorTool) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            value = editorTool ?? string.Empty;
            return editorTool is not null && key == "build_property._JazorNpmEditorTool";
        }
    }
}
