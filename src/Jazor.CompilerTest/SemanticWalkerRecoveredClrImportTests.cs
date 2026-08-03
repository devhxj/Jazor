using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRecoveredClrImportTests
{
    [TestMethod]
    public void Visit_StringAndNullableMembers_CollectRuntimeImportsAndEmitCalls()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Text;

            public static class RecoveredClrImportScenarios
            {
                public static void Evaluate(char[] characters, string?[] values, int? left, int? right)
                {
                    var constructed = new string(characters);
                    var range = new string(characters, 1, 2);
                    var repeated = new string('x', 3);
                    var normalized = constructed.Normalize();
                    var decomposed = normalized.Normalize(NormalizationForm.FormD);
                    var isNormalized = decomposed.IsNormalized(NormalizationForm.FormD);
                    var concatenated = string.Concat(values);
                    var joined = string.Join("/", values);
                    var paddedLeft = constructed.PadLeft(8, '0');
                    var paddedRight = constructed.PadRight(8);
                    var lines = constructed.ReplaceLineEndings("\n");
                    var compared = Nullable.Compare(left, right);
                    var equal = Nullable.Equals(left, right);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports);
        Assert.IsTrue(imports.TryGetValue("System/StringModule.js", out var stringImports));
        Assert.IsTrue(imports.TryGetValue("System/NullableT1Module.js", out var nullableImports));
        Assert.HasCount(
            11,
            stringImports,
            body + Environment.NewLine + string.Join(Environment.NewLine, stringImports.Select(static item => item.ToECMAScript())));
        Assert.HasCount(2, nullableImports);
        StringAssert.Contains(body, "let constructed =", StringComparison.Ordinal);
        StringAssert.Contains(body, "let normalized =", StringComparison.Ordinal);
        StringAssert.Contains(body, "let joined =", StringComparison.Ordinal);
        StringAssert.Contains(body, "let lines =", StringComparison.Ordinal);
        StringAssert.Contains(body, "let compared =", StringComparison.Ordinal);
        StringAssert.Contains(body, "let equal =", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains(".Normalize", StringComparison.Ordinal), body);
        Assert.IsFalse(body.Contains("Nullable.Compare", StringComparison.Ordinal), body);

        _ = new Parser().ParseScript("function verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "RecoveredClrImportScenarios",
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
