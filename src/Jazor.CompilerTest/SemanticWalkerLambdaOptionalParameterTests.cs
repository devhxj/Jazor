using Acornima;
using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerLambdaOptionalParameterTests
{
    [TestMethod]
    public void Visit_AnonymousFunction_OptionalParameter_UsesRoslynBoundDefault()
    {
        const string scenarioId = "anonymous-function.optional-parameter.ast";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public static int Run()
                {
                    var add = (int value = 2) => value + 1;
                    return add() * 10 + add(4);
                }
            }
            """,
            scenarioId);
        var anonymousFunction = GetAnonymousFunctionOperation(fixture.SemanticModel, fixture.SourceTree);
        var parameter = anonymousFunction.Symbol.Parameters.Single();

        Assert.IsTrue(parameter.HasExplicitDefaultValue, scenarioId);
        Assert.AreEqual(2, parameter.ExplicitDefaultValue, scenarioId);

        var script = new SemanticWalker(true)
            .Visit(anonymousFunction, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.AreEqual(
            """
            (value = 2) => {
              return value + 1;
            }
            """.ReplaceLineEndings("\n"),
            script?.ReplaceLineEndings("\n"),
            scenarioId);
        _ = new Parser().ParseExpression(script!);
    }

    [TestMethod]
    public async Task ConvertModule_AnonymousFunction_OptionalParameter_PreservesOmittedAndExplicitCallsOnDenoHost()
    {
        const string scenarioId = "anonymous-function.optional-parameter.deno";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public static int Run()
                {
                    var add = (int value = 2) => value + 1;
                    return add() * 10 + add(4);
                }
            }
            """,
            scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "let add = (value = 2) =>", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-optional-lambda-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "optional-lambda.mjs");
            var testPath = Path.Combine(root, "optional-lambda.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { run } from "./optional-lambda.mjs";

                Deno.test("optional lambda parameters preserve omitted and explicit C# calls", () => {
                  const result = run();
                  if (result !== 35)
                    throw new Error(`expected 35, got ${result}`);
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

    [TestMethod]
    public void Visit_AnonymousFunction_ByReferenceReturn_ReportsUnsupportedBoundary()
    {
        const string scenarioId = "anonymous-function.by-reference-return.boundary";
        var fixture = CompileModule(
            """
            public delegate ref int RefSelector(ref int value);

            public static class TestModule
            {
                public static int Run(ref int value)
                {
                    RefSelector select = (ref int current) => ref current;
                    return select(ref value);
                }
            }
            """,
            scenarioId);
        var anonymousFunction = GetAnonymousFunctionOperation(fixture.SemanticModel, fixture.SourceTree);

        try
        {
            _ = new SemanticWalker(true).Visit(anonymousFunction, new SenseArgument());
            Assert.Fail($"{scenarioId}: expected by-reference return lowering to fail.");
        }
        catch (OperationTransformationException exception)
        {
            StringAssert.Contains(exception.Message, "by-reference returns", StringComparison.Ordinal);
        }
    }

    private static IAnonymousFunctionOperation GetAnonymousFunctionOperation(
        SemanticModel semanticModel,
        SyntaxTree sourceTree)
    {
        var lambda = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ParenthesizedLambdaExpressionSyntax>()
            .Single();
        return Assert.IsInstanceOfType<IAnonymousFunctionOperation>(semanticModel.GetOperation(lambda));
    }

    private static LambdaFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerLambdaOptionalParameterScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalkerLambdaOptionalParameterScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new LambdaFixture(module, semanticModel, sourceTree);
    }

    private sealed record LambdaFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel,
        SyntaxTree SourceTree);
}
