using Acornima;
using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterBoundArgumentScenarioTests
{
    [TestMethod]
    public async Task ConvertModule_NamedArguments_PreserveSourceEvaluationAndBoundParameterOrder()
    {
        const string scenarioId = "ast-converter.bound-arguments.named-order";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public static int Run()
                {
                    var trace = 0;
                    int Mark(int value)
                    {
                        trace = trace * 10 + value;
                        return value;
                    }

                    var reordered = Combine(last: Mark(2), first: Mark(1), middle: Mark(3));
                    var optional = WithDefaults(third: Mark(5), first: Mark(4));
                    var current = 3;
                    var updated = Update(observed: out var observed, value: ref current);
                    return trace * 1000000 + reordered * 10000 + optional * 100 + updated * 10 + current + observed;
                }

                public static int RunReceiverOrder()
                {
                    var trace = 0;
                    int Mark(int value)
                    {
                        trace = trace * 10 + value;
                        return value;
                    }

                    Receiver GetReceiver()
                    {
                        trace = trace * 10 + 9;
                        return new Receiver();
                    }

                    var result = GetReceiver().Combine(last: Mark(2), first: Mark(1), middle: Mark(3));
                    return trace * 1000 + result;
                }

                private static int Combine(int first, int middle, int last)
                    => first * 100 + middle * 10 + last;

                private static int WithDefaults(int first = 1, int second = 2, int third = 3)
                    => first * 100 + second * 10 + third;

                private static int Update(ref int value, out int observed)
                {
                    value += 2;
                    observed = value + 1;
                    return value * 10;
                }

                public sealed class Receiver
                {
                    public int Combine(int first, int middle, int last)
                        => first * 100 + middle * 10 + last;
                }
            }
            """,
            scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "__arg$", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-bound-arguments-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "bound-arguments.mjs");
            var testPath = Path.Combine(root, "bound-arguments.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Run, RunReceiverOrder } from "./bound-arguments.mjs";

                Deno.test("named arguments preserve source evaluation, parameter binding, defaults, and ref/out write-back", () => {
                  const result = Run();
                  if (result !== 21355363011)
                    throw new Error(`expected 21355363011, got ${result}`);
                  const receiverResult = RunReceiverOrder();
                  if (receiverResult !== 9213132)
                    throw new Error(`expected receiver-first evaluation result 9213132, got ${receiverResult}`);
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

    private static BoundArgumentFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterBoundArgumentScenario.cs");
        var compilation = CSharpCompilation.Create(
            "AstConverterBoundArgumentScenarios_" + Guid.NewGuid().ToString("N"),
            [sourceTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            scenarioId + ":" + Environment.NewLine +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new BoundArgumentFixture(module, semanticModel);
    }

    private sealed record BoundArgumentFixture(INamedTypeSymbol Module, SemanticModel SemanticModel);
}
