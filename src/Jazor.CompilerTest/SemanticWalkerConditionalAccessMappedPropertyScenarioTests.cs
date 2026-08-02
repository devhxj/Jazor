using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerConditionalAccessMappedPropertyScenarioTests
{
    [TestMethod]
    public void Visit_ConditionalAccess_DictionaryKeys_UsesNullishGuardForInlineGetter()
    {
        var block = CompileBlock("""
            using System.Collections.Generic;

            sealed class Scenario
            {
                void Run(Dictionary<string, int>? values)
                {
                    var keys = values?.Keys;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        const string expected = """
            {
              let v$0;
              let keys = (v$0 = values, v$0 == null ? undefined : Array.from(v$0.keys()));
            }
            """;
        Assert.AreEqual(expected, script?.ReplaceLineEndings("\n"));
        _ = new Parser().ParseScript(script!);
    }

    private static IBlockOperation CompileBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            "conditional-access-mapped-property.cs");
        var compilation = CSharpCompilation.Create(
            "ConditionalAccessMappedProperty",
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
