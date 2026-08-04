using Acornima;
using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterConstructorRefOutScenarioTests
{
    [TestMethod]
    public async Task ConvertModule_SingleConstructorRefAndOut_PreservesWriteBackOnEveryReturnPath()
    {
        const string scenarioId = "ast-converter.constructor-ref-out.runtime";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public static int Run(int start)
                {
                    var current = start;
                    var probe = new Probe(ref current, out var observed);
                    return probe.Marker * 100 + current * 10 + observed;
                }

                public static int RunNamed(int start)
                {
                    var current = start;
                    var probe = new Probe(observed: out var observed, value: ref current);
                    return probe.Marker * 100 + current * 10 + observed;
                }

                public sealed class Probe
                {
                    public int Marker;

                    public Probe(ref int value, out int observed)
                    {
                        if (value < 0)
                        {
                            Marker = 4;
                            observed = 7;
                            return;
                        }

                        value += 2;
                        Marker = 1;
                        observed = value;
                    }
                }
            }
            """,
            scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "constructor(value, observed, $jazorRefOut)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-constructor-ref-out-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "constructor-ref-out.mjs");
            var testPath = Path.Combine(root, "constructor-ref-out.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { run, runNamed } from "./constructor-ref-out.mjs";

                Deno.test("member constructors write ref and out values through the compiler sink", () => {
                  if (run(3) !== 155)
                    throw new Error(`positive path: ${run(3)}`);
                  if (run(-2) !== 387)
                    throw new Error(`early return path: ${run(-2)}`);
                  if (runNamed(3) !== 155)
                    throw new Error(`named positive path: ${runNamed(3)}`);
                  if (runNamed(-2) !== 387)
                    throw new Error(`named early return path: ${runNamed(-2)}`);
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

    private static ConstructorFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterConstructorRefOutScenario.cs");
        var compilation = CSharpCompilation.Create(
            "AstConverterConstructorRefOutScenarios_" + Guid.NewGuid().ToString("N"),
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

        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => compilation.GetSemanticModel(sourceTree).GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new ConstructorFixture(module, compilation.GetSemanticModel(sourceTree));
    }

    private sealed record ConstructorFixture(INamedTypeSymbol Module, SemanticModel SemanticModel);
}
