using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class EditorAnalysisTests
{
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
