using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerImplicitIndexerStableReceiverScenarioTests
{
    [TestMethod]
    public void Visit_SimpleAssignment_ListImplicitIndexerWithStableReceiver_UsesSetterWithoutCache()
    {
        var block = CompileBlock("""
            using System.Collections.Generic;

            sealed class Scenario
            {
                void Run()
                {
                    var values = new List<int> { 1, 2, 3 };
                    values[0] = 9;
                    values[^1] = 4;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        const string expected = """
            {
              let values = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              _c16a7960302ea054(values, 0, 9);
              _c16a7960302ea054(values, values.length - 1, 4);
            }
            """;
        Assert.AreEqual(expected, script?.ReplaceLineEndings("\n"));
        Assert.AreEqual(1, CountOccurrences(script!, "let v$"));
        _ = new Parser().ParseScript(script!);
    }

    private static int CountOccurrences(string value, string fragment)
        => (value.Length - value.Replace(fragment, string.Empty, StringComparison.Ordinal).Length) / fragment.Length;

    private static IBlockOperation CompileBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            "stable-implicit-indexer.cs");
        var compilation = CSharpCompilation.Create(
            "StableImplicitIndexer",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Run");
        var operation = model.GetOperation(method.Body!);
        Assert.IsInstanceOfType<IBlockOperation>(operation);
        return (IBlockOperation)operation;
    }
}
