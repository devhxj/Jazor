using Acornima;
using DenoHost.Core;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerBindingIdentifierTests
{
    [TestMethod]
    public async Task Visit_EscapedKeywordLocalAndLambdaParameter_UseStableLegalBindingsOnDenoHost()
    {
        const string source = """
public static class BindingScenarios
{
    public static int Transform(int input)
    {
        System.Func<int, int> @class = (@await) => @await + input;
        return @class(3);
    }
}
""";
        var firstBlock = GetBlockOperation(source, "Bindings/first.cs");
        var secondBlock = GetBlockOperation(source, "Bindings/second.cs");

        var first = new SemanticWalker(true).Visit(firstBlock, new SenseArgument())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");
        var second = new SemanticWalker(true).Visit(secondBlock, new SenseArgument())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second, "Binding names must remain stable across equivalent lowering passes.");
        Assert.IsFalse(first.Contains("let class", StringComparison.Ordinal), first);
        Assert.IsFalse(first.Contains("await =>", StringComparison.Ordinal), first);

        var module = "export function transform(input) " + first;
        _ = new Parser().ParseModule(module);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-binding-identifiers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "bindings.mjs");
            var testPath = Path.Combine(root, "bindings.test.mjs");
            await System.IO.File.WriteAllTextAsync(
                modulePath,
                module,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await System.IO.File.WriteAllTextAsync(
                testPath,
                """
                import { transform } from "./bindings.mjs";

                Deno.test("escaped C# keyword bindings stay lexical in JavaScript", () => {
                  const result = transform(4);
                  if (result !== 7)
                    throw new Error(`expected 7, got ${result}`);
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

    private static IBlockOperation GetBlockOperation(string source, string path)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, path: path);
        var compilation = CSharpCompilation.Create(
            "BindingIdentifierScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "Transform");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
