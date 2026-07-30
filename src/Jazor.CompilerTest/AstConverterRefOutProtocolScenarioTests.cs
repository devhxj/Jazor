using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterRefOutProtocolScenarioTests
{
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
}
