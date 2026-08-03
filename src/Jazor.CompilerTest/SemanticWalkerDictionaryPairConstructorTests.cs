using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerDictionaryPairConstructorTests
{
    [TestMethod]
    public void Visit_DictionaryPairConstructors_UseBoundImportsAndPreserveEntrySequence()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;

            public static class DictionaryPairConstructorScenarios
            {
                public static Dictionary<string, int> Create(
                    IEnumerable<KeyValuePair<string, int>> entries,
                    IEqualityComparer<string> comparer)
                {
                    var defaultMap = new Dictionary<string, int>(entries);
                    return new Dictionary<string, int>(entries, comparer);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Collections/Generic/DictionaryT2Module.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEquivalent(new[] { "createFromPairs", "createFromPairsWithComparer" }, importNames);
        StringAssert.Contains(body, "createFromPairs(entries)", StringComparison.Ordinal);
        StringAssert.Contains(body, "createFromPairsWithComparer(entries, comparer)", StringComparison.Ordinal);

        var moduleImport = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImport + "\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "DictionaryPairConstructorScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "Create");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
