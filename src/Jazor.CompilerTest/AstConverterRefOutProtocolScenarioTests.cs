using Acornima;
using Acornima.Ast;
using DenoHost.Core;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterRefOutProtocolScenarioTests
{

	[TestMethod]
	public async Task Convert_AnonymousFunctionsWithRefAndOut_UseSharedCalleeAndCallerProtocol()
	{
		const string scenarioId = "ast-converter-ref-out.anonymous-functions-round-trip";
		var fixture = CompileModule(
			"""
			public delegate int RefTransformer(ref int value);
			public delegate void OutProjector(int source, out int value);

			public static class TestModule
			{
				public static int Run(int initial)
				{
					RefTransformer transform = delegate(ref int current)
					{
						current += 2;
						return current * 10;
					};
					OutProjector project = (int source, out int current) => current = source + 1;

					var transformed = transform(ref initial);
					project(transformed, out var projected);
					return projected + initial;
				}
			}
			""",
			scenarioId);
		var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

		Assert.IsNotNull(module, scenarioId);
		var run = GetExportedFunction(module, scenarioId);
		var functionInitializers = run.Body.Body
			.OfType<VariableDeclaration>()
			.SelectMany(static declaration => declaration.Declarations)
			.Where(static declarator => declarator.Init is ArrowFunctionExpression)
			.Select(static declarator => (ArrowFunctionExpression)declarator.Init!)
			.ToArray();
		Assert.HasCount(2, functionInitializers, scenarioId);

		var transformReturn = ((FunctionBody)functionInitializers[0].Body).Body
			.OfType<ReturnStatement>()
			.Single();
		AssertProtocolArray(transformReturn.Argument, ["current * 10", "current"], scenarioId);

		var projectReturns = ((FunctionBody)functionInitializers[1].Body).Body
			.OfType<ReturnStatement>()
			.ToArray();
		Assert.IsNotEmpty(projectReturns, scenarioId);
		foreach (var projectReturn in projectReturns)
			AssertProtocolArray(projectReturn.Argument, ["current"], scenarioId);

		var transformed = run.Body.Body
			.OfType<VariableDeclaration>()
			.SelectMany(static declaration => declaration.Declarations)
			.Single(static declarator => declarator.Id is Identifier { Name: "transformed" });
		Assert.IsInstanceOfType<SequenceExpression>(transformed.Init, scenarioId);
		var transformCall = (SequenceExpression)transformed.Init!;
		AssertWriteBack(transformCall, "initial", 1, scenarioId);
		AssertSequenceResult(transformCall, 0, scenarioId);

		var projectCall = run.Body.Body
			.OfType<NonSpecialExpressionStatement>()
			.Select(static statement => statement.Expression)
			.OfType<SequenceExpression>()
			.Single();
		AssertWriteBack(projectCall, "projected", 0, scenarioId);

		_ = new Parser().ParseModule(module.ToKnRECMAScript());
	}

	[TestMethod]
	public async Task ConvertModule_AnonymousFunctionsWithRefAndOut_PreserveRoundTripOnDenoHost()
	{
		const string scenarioId = "ast-converter-ref-out.anonymous-functions-deno-round-trip";
		var fixture = CompileModule(
			"""
			public delegate int RefTransformer(ref int value);
			public delegate void OutProjector(int source, out int value);

			public static class TestModule
			{
				public static int Run(int initial)
				{
					RefTransformer transform = delegate(ref int current)
					{
						current += 2;
						return current * 10;
					};
					OutProjector project = (int source, out int current) => current = source + 1;

					var transformed = transform(ref initial);
					project(transformed, out var projected);
					return projected + initial;
				}
			}
			""",
			scenarioId);
		var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
		var script = module?.ToKnRECMAScript();

		Assert.IsNotNull(script, scenarioId);
		_ = new Parser().ParseModule(script);

		var root = Path.Combine(
			Path.GetTempPath(),
			"jazor-anonymous-ref-out-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(root);

		try
		{
			var modulePath = Path.Combine(root, "anonymous-ref-out.mjs");
			var testPath = Path.Combine(root, "anonymous-ref-out.test.mjs");
			await File.WriteAllTextAsync(
				modulePath,
				script,
				new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			await File.WriteAllTextAsync(
				testPath,
				"""
				import { run } from "./anonymous-ref-out.mjs";

				Deno.test("anonymous ref and out delegates write values back through the shared protocol", () => {
				  const result = run(3);
				  if (result !== 56)
				    throw new Error(`expected 56 after ref and out write-back, got ${result}`);
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
	public async Task Convert_LocalFunctionWithRefParameter_UsesSharedCalleeAndCallerProtocol()
    {
        const string scenarioId = "ast-converter-ref-out.local-function-ref-round-trip";
        const string source = """
            public static class TestModule
            {
                public static int Run(int initial)
                {
                    var value = initial;

                    int Increment(ref int current)
                    {
                        current++;
                        return current;
                    }

                    var result = Increment(ref value);
                    return result + value;
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var outerFunction = GetExportedFunction(module, scenarioId);
        var localFunction = outerFunction.Body.Body.OfType<FunctionDeclaration>().Single();
        var localReturn = localFunction.Body.Body.OfType<ReturnStatement>().Single();

        AssertProtocolArray(localReturn.Argument, ["current", "current"], scenarioId);

        var resultDeclarator = outerFunction.Body.Body
            .OfType<VariableDeclaration>()
            .SelectMany(static declaration => declaration.Declarations)
            .Single(declarator => declarator.Id is Identifier { Name: "result" });
        Assert.IsInstanceOfType<SequenceExpression>(resultDeclarator.Init, scenarioId);
        var callerProtocol = (SequenceExpression)resultDeclarator.Init!;
        AssertWriteBack(callerProtocol, "value", 1, scenarioId);
        AssertSequenceResult(callerProtocol, 0, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_LocalVoidFunctionWithOutParameter_ReturnsAndWritesBackOutValue()
    {
        const string scenarioId = "ast-converter-ref-out.local-function-void-out";
        const string source = """
            public static class TestModule
            {
                public static int Run(int input)
                {
                    void Normalize(int source, out int current)
                    {
                        current = source > 0 ? source : 0;
                    }

                    Normalize(input, out var result);
                    return result;
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var outerFunction = GetExportedFunction(module, scenarioId);
        var localFunction = outerFunction.Body.Body.OfType<FunctionDeclaration>().Single();
        var localReturns = localFunction.Body.Body.OfType<ReturnStatement>().ToArray();

        Assert.IsNotEmpty(localReturns, scenarioId);
        foreach (var localReturn in localReturns)
            AssertProtocolArray(localReturn.Argument, ["current"], scenarioId);

        var callerProtocol = outerFunction.Body.Body
            .OfType<NonSpecialExpressionStatement>()
            .Select(static statement => statement.Expression)
            .OfType<SequenceExpression>()
            .Single();
        AssertWriteBack(callerProtocol, "result", 0, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_LocalFunctionRefProtocol_DoesNotRewriteItsNestedLocalFunctionReturn()
    {
        const string scenarioId = "ast-converter-ref-out.local-function-nested-boundary";
        const string source = """
            public static class TestModule
            {
                public static int Run(int initial)
                {
                    var value = initial;

                    int Increment(ref int current)
                    {
                        int Step() => 1;
                        current += Step();
                        return current;
                    }

                    return Increment(ref value);
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var outerFunction = GetExportedFunction(module, scenarioId);
        var incrementFunction = outerFunction.Body.Body.OfType<FunctionDeclaration>().Single();
        var stepFunction = incrementFunction.Body.Body.OfType<FunctionDeclaration>().Single();
        var stepReturn = stepFunction.Body.Body.OfType<ReturnStatement>().Single();
        var incrementReturn = incrementFunction.Body.Body.OfType<ReturnStatement>().Single();

        Assert.IsInstanceOfType<NumericLiteral>(stepReturn.Argument, scenarioId);
        AssertProtocolArray(incrementReturn.Argument, ["current", "current"], scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_RefOutProtocol_DoesNotRewriteNestedLocalFunctionReturn()
    {
        const string scenarioId = "ast-converter-ref-out.nested-local-function-return";
        const string source = """
            public static class TestModule
            {
                public static int IncrementAndRead(ref int value)
                {
                    int Seed()
                    {
                        return 1;
                    }

                    value++;
                    return value + Seed();
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var exportedMethod = module!.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<FunctionDeclaration>(exportedMethod.Declaration, scenarioId);
        var outerFunction = (FunctionDeclaration)exportedMethod.Declaration;
        var localFunction = outerFunction.Body.Body.OfType<FunctionDeclaration>().Single();
        var localReturn = localFunction.Body.Body.OfType<ReturnStatement>().Single();

        Assert.IsInstanceOfType<NumericLiteral>(localReturn.Argument, scenarioId);

        var outerReturn = outerFunction.Body.Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<ArrayExpression>(outerReturn.Argument, scenarioId);
        var protocolResult = (ArrayExpression)outerReturn.Argument;
        Assert.HasCount(2, protocolResult.Elements, scenarioId);
        Assert.IsInstanceOfType<BinaryExpression>(protocolResult.Elements[0], scenarioId);
        Assert.IsInstanceOfType<Identifier>(protocolResult.Elements[1], scenarioId);
        Assert.AreEqual("value", ((Identifier)protocolResult.Elements[1]!).Name, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_RefOutProtocol_DoesNotRewriteNestedLambdaReturn()
    {
        const string scenarioId = "ast-converter-ref-out.nested-lambda-return";
        const string source = """
            public static class TestModule
            {
                public static int IncrementAndRead(ref int value)
                {
                    System.Func<int> seed = () => 1;
                    value++;
                    return value + seed();
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var exportedMethod = module!.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<FunctionDeclaration>(exportedMethod.Declaration, scenarioId);
        var outerFunction = (FunctionDeclaration)exportedMethod.Declaration;
        var seedDeclaration = outerFunction.Body.Body.OfType<VariableDeclaration>().Single();
        var seedInitializer = seedDeclaration.Declarations.Single().Init;

        Assert.IsInstanceOfType<ArrowFunctionExpression>(seedInitializer, scenarioId);
        var lambda = (ArrowFunctionExpression)seedInitializer;
        Assert.IsInstanceOfType<FunctionBody>(lambda.Body, scenarioId);
        var lambdaReturn = ((FunctionBody)lambda.Body).Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<NumericLiteral>(lambdaReturn.Argument, scenarioId);

        var outerReturn = outerFunction.Body.Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<ArrayExpression>(outerReturn.Argument, scenarioId);
        var protocolResult = (ArrayExpression)outerReturn.Argument;
        Assert.HasCount(2, protocolResult.Elements, scenarioId);
        Assert.IsInstanceOfType<BinaryExpression>(protocolResult.Elements[0], scenarioId);
        Assert.IsInstanceOfType<Identifier>(protocolResult.Elements[1], scenarioId);
        Assert.AreEqual("value", ((Identifier)protocolResult.Elements[1]!).Name, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_RefOutProtocol_DoesNotRewriteNestedAnonymousDelegateReturn()
    {
        const string scenarioId = "ast-converter-ref-out.nested-anonymous-delegate-return";
        const string source = """
            public static class TestModule
            {
                public static int IncrementAndRead(ref int value)
                {
                    System.Func<int> seed = delegate
                    {
                        return 1;
                    };
                    value++;
                    return value + seed();
                }
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var outerFunction = GetExportedFunction(module, scenarioId);
        var seedDeclaration = outerFunction.Body.Body.OfType<VariableDeclaration>().Single();
        var seedInitializer = seedDeclaration.Declarations.Single().Init;

        Assert.IsInstanceOfType<ArrowFunctionExpression>(seedInitializer, scenarioId);
        var anonymousDelegate = (ArrowFunctionExpression)seedInitializer;
        Assert.IsInstanceOfType<FunctionBody>(anonymousDelegate.Body, scenarioId);
        var delegateReturn = ((FunctionBody)anonymousDelegate.Body).Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<NumericLiteral>(delegateReturn.Argument, scenarioId);

        var outerReturn = outerFunction.Body.Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<ArrayExpression>(outerReturn.Argument, scenarioId);
        var protocolResult = (ArrayExpression)outerReturn.Argument;
        Assert.HasCount(2, protocolResult.Elements, scenarioId);
        Assert.IsInstanceOfType<BinaryExpression>(protocolResult.Elements[0], scenarioId);
        Assert.IsInstanceOfType<Identifier>(protocolResult.Elements[1], scenarioId);
        Assert.AreEqual("value", ((Identifier)protocolResult.Elements[1]!).Name, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_RefOutProtocol_DoesNotRewriteHostFunctionExpressionReturn()
    {
        const string scenarioId = "ast-converter-ref-out.host-function-expression-return";
        const string source = """
            public static class TestModule
            {
                public static int Read(ref int value)
                {
                    System.Func<int> seed = FunctionProbe.Create();
                    value++;
                    return seed();
                }
            }

            public static class FunctionProbe
            {
                public static System.Func<int> Create() => null!;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                Host: new FunctionExpressionProbeHost()));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenarioId);
        var outerFunction = GetExportedFunction(module, scenarioId);
        var seedDeclaration = outerFunction.Body.Body.OfType<VariableDeclaration>().Single();
        var seedInitializer = seedDeclaration.Declarations.Single().Init;

        Assert.IsInstanceOfType<FunctionExpression>(seedInitializer, scenarioId);
        var hostFunction = (FunctionExpression)seedInitializer;
        var hostReturn = hostFunction.Body.Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<NumericLiteral>(hostReturn.Argument, scenarioId);

        var outerReturn = outerFunction.Body.Body.OfType<ReturnStatement>().Single();
        Assert.IsInstanceOfType<ArrayExpression>(outerReturn.Argument, scenarioId);
        var protocolResult = (ArrayExpression)outerReturn.Argument;
        Assert.HasCount(2, protocolResult.Elements, scenarioId);
        Assert.IsInstanceOfType<CallExpression>(protocolResult.Elements[0], scenarioId);
        Assert.IsInstanceOfType<Identifier>(protocolResult.Elements[1], scenarioId);
        Assert.AreEqual("value", ((Identifier)protocolResult.Elements[1]!).Name, scenarioId);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    private static FunctionDeclaration GetExportedFunction(Module module, string scenarioId)
    {
        var exportedMethod = module.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<FunctionDeclaration>(exportedMethod.Declaration, scenarioId);
        return (FunctionDeclaration)exportedMethod.Declaration!;
    }

	private static void AssertProtocolArray(
		Expression? expression,
		IReadOnlyList<string> expectedExpressions,
		string scenarioId)
	{
		Assert.IsInstanceOfType<ArrayExpression>(expression, scenarioId);
		var array = (ArrayExpression)expression!;
		Assert.HasCount(expectedExpressions.Count, array.Elements, scenarioId);
		for (var index = 0; index < expectedExpressions.Count; index++)
		{
			Assert.AreEqual(expectedExpressions[index], array.Elements[index]!.ToKnRECMAScript(), scenarioId);
		}
	}

    private static void AssertWriteBack(
        SequenceExpression sequence,
        string targetName,
        double expectedProtocolIndex,
        string scenarioId)
    {
        var writeBack = sequence.Expressions
            .OfType<AssignmentExpression>()
            .Single(assignment => assignment.Left is Identifier { Name: var name } && name == targetName);
        Assert.IsInstanceOfType<MemberExpression>(writeBack.Right, scenarioId);
        var protocolElement = (MemberExpression)writeBack.Right;
        Assert.IsTrue(protocolElement.Computed, scenarioId);
        Assert.IsInstanceOfType<NumericLiteral>(protocolElement.Property, scenarioId);
        Assert.AreEqual(expectedProtocolIndex, ((NumericLiteral)protocolElement.Property).Value, scenarioId);
    }

    private static void AssertSequenceResult(
        SequenceExpression sequence,
        double expectedProtocolIndex,
        string scenarioId)
    {
        Assert.IsInstanceOfType<MemberExpression>(sequence.Expressions[^1], scenarioId);
        var result = (MemberExpression)sequence.Expressions[^1];
        Assert.IsTrue(result.Computed, scenarioId);
        Assert.IsInstanceOfType<NumericLiteral>(result.Property, scenarioId);
        Assert.AreEqual(expectedProtocolIndex, ((NumericLiteral)result.Property).Value, scenarioId);
    }

    private static RefOutFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterRefOutProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterRefOutProtocolScenarios_" + Guid.NewGuid().ToString("N"),
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
        return new RefOutFixture(module, semanticModel);
    }

    private sealed record RefOutFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel);

    private sealed class FunctionExpressionProbeHost : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationPreorder(
            IInvocationOperation operation,
            SenseArgument argument)
        {
            if (operation.TargetMethod.ContainingType.Name != "FunctionProbe" ||
                operation.TargetMethod.Name != "Create")
            {
                return null;
            }

            return new FunctionExpression(
                id: null,
                parameters: NodeList.Empty<Node>(),
                body: new FunctionBody(
                    NodeList.From<Statement>(new ReturnStatement(new NumericLiteral(1, "1"))),
                    strict: true),
                generator: false,
                async: false);
        }
    }
}
