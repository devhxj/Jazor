using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class EditorAnalysisTests
{
    [TestMethod]
    public void FocusedAuthorProjectionBindsInheritedSharedTypesAndDefersForOmittedExtensions()
    {
        var source = new JoltSource("Pages/Page.jazor", "@code { private Route route = new(); private string Status => route.Path; }");
        var imports = new JoltSource("_Imports.jazor", "@using Shared\n@using Npm.ElementPlus");
        var unrelated = new JoltSource("Other/Widget.jazor", "<div>ordinary markup</div>");
        var options = new JoltCompileOptions
        {
            CSharpSources = [new("Shared.cs", "namespace Shared; public class Route { public string Path => \"/\"; }")],
        };
        var focused = JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, unrelated], options, default);
        Assert.IsNotNull(focused);
        var full = JoltProjectCompiler.AnalyzeEditor([source, imports, unrelated], options,
            new HashSet<string> { source.Path }, null, default).Analysis.Documents.Single(item => item.Document.Path == source.Path);
        Assert.AreEqual(PathSymbol(full), PathSymbol(focused));
        Assert.AreSame(full.SemanticModel.Compilation.SyntaxTrees.Single(tree => tree.FilePath == "Shared.cs"),
            focused.SemanticModel.Compilation.SyntaxTrees.Single(tree => tree.FilePath == "Shared.cs"));

        var extension = new JoltSource("Pages/Extension.jazor", "@code { public static int Example(this Shared.Route route) => 1; }");
        Assert.IsNull(JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, extension], options, default));
        var extensionBlock = extension with { Content = "@code { extension(Shared.Route route) { public int Example() => 1; } }" };
        Assert.IsNull(JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, extensionBlock], options, default));
        var ordinaryMarkup = extension with { Content = "<p>this extension describes a button</p>" };
        Assert.IsNotNull(JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, ordinaryMarkup], options, default));
        var collision = source with { Content = source.Content.Replace("Route route", "__ModuleWidget route", StringComparison.Ordinal) };
        Assert.IsNull(JoltProjectCompiler.ProjectEditorDocument(collision, [collision, imports, unrelated], options, default));
        var conditional = options with
        {
            ProjectSources = [new("Host.cs", "#if HOST\nclass Host { App.Other.__ModuleWidget? widget; }\n#endif")],
        };
        Assert.IsNotNull(JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, unrelated], conditional, default));
        Assert.IsNull(JoltProjectCompiler.ProjectEditorDocument(source, [source, imports, unrelated],
            conditional with { ProjectDefines = ["HOST"] }, default));

        string PathSymbol(JoltProjectedDocument projection)
        {
            int offset = source.Content.IndexOf("route.Path", StringComparison.Ordinal) + "route.".Length;
            var span = projection.Spans.Single(span => span.Source.Contains(offset));
            var token = projection.SemanticModel.SyntaxTree.GetRoot().FindToken(span.Generated.Start + offset - span.Source.Start);
            return projection.SemanticModel.GetSymbolInfo(token.Parent!).Symbol!.ToDisplayString();
        }
    }

    [TestMethod]
    public void EarlyAuthorDeclarationsRetainSharedBindingsAndSourceSpans()
    {
        var source = new JoltSource("Page.jazor", """
            <p>@Status</p>
            @code { private string Status => Shared.Route.Path; }
            """);
        var options = new JoltCompileOptions
        {
            CSharpSources = [new("Shared.cs", "namespace Shared; public static class Route { public static string Path => \"/\"; }")],
        };
        JoltProjectedDocument? author = null;
        JoltProjectionSpan[]? originalSpans = null;
        var result = JoltProjectCompiler.AnalyzeEditor([source], options,
            new HashSet<string> { source.Path }, null, default, early =>
            {
                Assert.AreEqual(0, early.Diagnostics.Count);
                Assert.IsNull(author);
                author = early.Documents.Single();
                originalSpans = author.Spans.ToArray();
            });
        Assert.IsNotNull(author);
        CollectionAssert.AreEqual(originalSpans!, author.Spans.ToArray());
        Assert.IsFalse(author.SemanticModel.SyntaxTree.GetText().ToString().Contains("__JoltRender", StringComparison.Ordinal));
        var full = result.Analysis.Documents.Single();
        Assert.AreEqual(BoundPath(full), BoundPath(author));

        string BoundPath(JoltProjectedDocument document)
        {
            var offset = source.Content.IndexOf("Route.Path", StringComparison.Ordinal) + "Route.".Length;
            var map = document.Spans.Single(span => span.Source.Contains(offset));
            var token = document.SemanticModel.SyntaxTree.GetRoot().FindToken(map.Generated.Start + offset - map.Source.Start);
            Assert.AreEqual("Path", token.ValueText);
            var symbol = document.SemanticModel.GetSymbolInfo(token.Parent!).Symbol;
            Assert.IsNotNull(symbol);
            Assert.AreEqual("Shared.cs", symbol.Locations.Single().SourceTree!.FilePath);
            return symbol.ToDisplayString();
        }
    }

    [TestMethod]
    public void ChangingOpenDocumentsReusesGenericBindingAndMatchesFreshDiagnostics()
    {
        JoltSource[] sources =
        [
            new("_Imports.jazor", "@using MissingNamespace"),
            new("Generic.jazor", "@typeparam T\n@code { [Parameter] public T Value { get; set; } = default!; }"),
            new("First.jazor", "<Generic Value=\"@(42)\" />"),
            new("Second.jazor", "<input @ref=\"@target\" />\n<p>@Missing</p>\n@code { private ECMAScript.HTMLInputElement? target; [Setup] private void Start() { } }"),
        ];
        var options = new JoltCompileOptions();
        var initialPaths = new HashSet<string>(StringComparer.Ordinal) { "First.jazor" };
        var bothPaths = new HashSet<string>(initialPaths, StringComparer.Ordinal) { "Second.jazor" };
        var initial = JoltProjectCompiler.AnalyzeEditor(sources, options, initialPaths, null, default);
        var initialDiagnostics = Describe(initial.Analysis);
        var expanded = JoltProjectCompiler.AnalyzeEditor(sources, options, bothPaths, initial.State, default);
        var fresh = JoltProjectCompiler.AnalyzeEditor(sources, options, bothPaths, null, default);
        Assert.AreSame(initial.Analysis.Documents[0].SemanticModel.Compilation, expanded.Analysis.Documents[0].SemanticModel.Compilation);
        Assert.AreSame(initial.Analysis.Documents, expanded.Analysis.Documents);
        Assert.AreSame(initial.Analysis.NpmValueDefinitions, expanded.Analysis.NpmValueDefinitions);
        CollectionAssert.AreEqual(Describe(fresh.Analysis), Describe(expanded.Analysis));
        CollectionAssert.AreEqual(initialDiagnostics, Describe(initial.Analysis));
        Assert.AreEqual(1, expanded.Analysis.Diagnostics.Count(diagnostic => diagnostic.Id == "CS0246" &&
            diagnostic.Location.GetMappedLineSpan().Path == "_Imports.jazor"));
        Assert.IsTrue(expanded.Analysis.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0103"));

        var closed = JoltProjectCompiler.AnalyzeEditor(sources, options, initialPaths, expanded.State, default);
        CollectionAssert.AreEqual(initialDiagnostics, Describe(closed.Analysis));
        var reopened = JoltProjectCompiler.AnalyzeEditor(sources, options, bothPaths, closed.State, default);
        CollectionAssert.AreEqual(Describe(fresh.Analysis), Describe(reopened.Analysis));

        var changed = sources.ToArray();
        changed[^1] = changed[^1] with { Content = changed[^1].Content.Replace("@Missing", "@target", StringComparison.Ordinal) };
        var changedOverlay = JoltProjectCompiler.AnalyzeEditor(changed, options, bothPaths, reopened.State, default);
        Assert.AreNotSame(reopened.Analysis.Documents[0].SemanticModel.Compilation, changedOverlay.Analysis.Documents[0].SemanticModel.Compilation);
        Assert.IsFalse(changedOverlay.Analysis.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0103"));

        static string[] Describe(JoltAnalysis analysis) => analysis.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray();
    }

    [TestMethod]
    public void OpeningAnotherDocumentAfterABodyEditValidatesAgainstCurrentSymbols()
    {
        var page = new JoltSource("Page.jazor", "<p>@Count</p>\n@code { private int Count => 1; }");
        var sibling = new JoltSource("Sibling.jazor",
            "<input @ref=\"@target\" />\n@code { private ECMAScript.HTMLInputElement? target; [Setup] private void Start() { } }");
        var options = new JoltCompileOptions();
        var open = new HashSet<string>(StringComparer.Ordinal) { page.Path };
        var initial = JoltProjectCompiler.AnalyzeEditor([page, sibling], options, open, null, default);
        var sources = new[] { page with { Content = page.Content.Replace("=> 1", "=> 2", StringComparison.Ordinal) }, sibling };
        var edited = JoltProjectCompiler.AnalyzeEditor(sources, options, open, initial.State, default);
        var both = new HashSet<string>(open, StringComparer.Ordinal) { sibling.Path };
        var expanded = JoltProjectCompiler.AnalyzeEditor(sources, options, both, edited.State, default);
        var fresh = JoltProjectCompiler.AnalyzeEditor(sources, options, both, null, default);
        Assert.AreEqual(0, fresh.Analysis.Diagnostics.Count);
        Assert.AreEqual(0, expanded.Analysis.Diagnostics.Count, string.Join("\n", expanded.Analysis.Diagnostics));
        Assert.AreSame(edited.Analysis.Documents, expanded.Analysis.Documents);
    }

    [TestMethod]
    public void BodyAndMarkupEditsReuseOtherTreesAndRefreshCrossFileSymbols()
    {
        const string component = """
            <p>@Value</p>
            @code {
                [Parameter] public string Value { get; set; } = "";
                [Expose] public string Read() => Value;
            }
            """;
        var consumer = new JoltSource("Consumer.jazor", """
            <Example Value="text" @ref="@component" />
            @code {
                private ExampleRef? component;
                private string? Result => component?.Read();
            }
            """);
        var options = new JoltCompileOptions();
        var paths = new HashSet<string>(StringComparer.Ordinal) { "Example.jazor" };
        var first = JoltProjectCompiler.AnalyzeEditor([consumer, new("Example.jazor", component)], options, paths, null, default);
        var changedSource = new JoltSource("Example.jazor", component
            .Replace("<p>@Value</p>", "<p>@Missing</p>", StringComparison.Ordinal)
            .Replace("Read() => Value", "Read() => Value.Trim()", StringComparison.Ordinal));
        var updated = JoltProjectCompiler.AnalyzeEditor([consumer, changedSource], options, paths, first.State, default);
        var fresh = JoltProjectCompiler.AnalyzeEditor([consumer, changedSource], options, paths, null, default);
        Assert.AreEqual(0, first.Analysis.Diagnostics.Count);
        Assert.IsTrue(updated.Analysis.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0103"));
        CollectionAssert.AreEqual(fresh.Analysis.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray(),
            updated.Analysis.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray());

        var oldConsumer = first.Analysis.Documents.Single(document => document.Document.Path == consumer.Path);
        var newConsumer = updated.Analysis.Documents.Single(document => document.Document.Path == consumer.Path);
        var changedTree = updated.Analysis.Documents.Single(document => document.Document.Path == changedSource.Path).SemanticModel.SyntaxTree;
        Assert.AreSame(oldConsumer.SemanticModel.SyntaxTree, newConsumer.SemanticModel.SyntaxTree);
        Assert.AreSame(changedTree, newConsumer.SemanticModel.Compilation.GetTypeByMetadataName("App.ExampleRef")!.Locations.Single().SourceTree);

        var renamedParameter = changedSource with { Content = changedSource.Content.Replace("string Value {", "string Title {", StringComparison.Ordinal) };
        var contractEdit = JoltProjectCompiler.AnalyzeEditor([consumer, renamedParameter], options, paths, updated.State, default);
        Assert.AreNotSame(newConsumer.SemanticModel.SyntaxTree,
            contractEdit.Analysis.Documents.Single(document => document.Document.Path == consumer.Path).SemanticModel.SyntaxTree);
        Assert.IsTrue(contractEdit.Analysis.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT1004"));
    }
}
