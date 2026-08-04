using Acornima;
using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerStringForEachTests
{
    [TestMethod]
    public async Task VisitForEachLoop_String_UsesUtf16CharArrayContractOnDenoHost()
    {
        var block = GetBlockOperation("""
            class TestModule
            {
                public static int CountUnits()
                {
                    var evaluations = 0;
                    string GetText()
                    {
                        evaluations++;
                        return "A\U0001F600";
                    }

                    var count = 0;
                    foreach (char unit in GetText())
                        count++;
                    return evaluations * 10 + count;
                }
            }
            """);

        var body = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(body);
        StringAssert.Contains(body, "for (let unit of GetText().split(\"\"))", StringComparison.Ordinal);

        var module = "export function countUnits() " + body;
        _ = new Parser().ParseModule(module);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-string-foreach-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "string-foreach.mjs");
            var testPath = Path.Combine(root, "string-foreach.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                module,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { countUnits } from "./string-foreach.mjs";

                Deno.test("foreach string preserves C# UTF-16 char iteration", () => {
                  if (countUnits() !== 13)
                    throw new Error(`expected one evaluation and 3 UTF-16 units, got ${countUnits()}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalker.StringForEach.Tests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "CountUnits");
        var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!);
        Assert.IsInstanceOfType<IBlockOperation>(operation);
        return (IBlockOperation)operation!;
    }
}
