using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerStringRecoveredDiscardTests
{
    [TestMethod]
    public void Visit_OrdinalRangeAndCharacterCopy_UseStringRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            public static class StringScenarios
            {
                public static int Evaluate(string left, string right, char[] destination)
                {
                    var comparison = string.CompareOrdinal(left, 1, right, 2, 3);
                    left.CopyTo(0, destination, 1, 2);
                    return comparison;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/StringModule.js"], body);
        StringAssert.Contains(body, "_dc789454b6ef6bcb(left, 1, right, 2, 3)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_45bb6097c28a2f1e(left, 0, destination, 1, 2)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(left, right, destination) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "StringScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
