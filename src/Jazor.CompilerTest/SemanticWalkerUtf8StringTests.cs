using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerUtf8StringTests
{
    public static IEnumerable<TestDataRow<Utf8LiteralScenario>> Cases
        => Utf8LiteralScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<Utf8LiteralScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_Utf8StringLiteral_EmitsExactByteArray(Utf8LiteralScenario scenario)
    {
        var block = GetBlockOperation($$"""
            class TestClass
            {
                void TestMethod()
                {
                    System.ReadOnlySpan<byte> value = {{scenario.Expression}};
                }
            }
            """);

        var utf8Operation = block.Descendants().OfType<IUtf8StringOperation>().Single();
        Assert.AreEqual(scenario.DecodedValue, utf8Operation.Value, scenario.Id);

        var node = new SemanticWalker(true).Visit(block, new SenseArgument());
        var script = node?.ToKnRECMAScript();

        Assert.IsInstanceOfType<NestedBlockStatement>(node, scenario.Id);
        var loweredBlock = (NestedBlockStatement)node!;
        var declaration = Assert.IsInstanceOfType<VariableDeclaration>(loweredBlock.Body.Single(), scenario.Id);
        var declarator = declaration.Declarations.Single();
        AssertUtf8ByteArray(declarator.Init, scenario.ExpectedBytes, scenario.Id);

        Assert.IsNotNull(script, scenario.Id);
        StringAssert.Contains(script, "let value = [", scenario.Id);
        Assert.IsFalse(script.Contains("TextEncoder", StringComparison.Ordinal), scenario.Id);
        _ = new Parser().ParseScript(script!);
    }

    [TestMethod]
    public async Task Convert_ModuleMethodReturningUtf8Literal_UsesReadOnlySpanArrayCarrier()
    {
        const string source = """
            public static class Utf8Module
            {
                public static System.ReadOnlySpan<byte> GetPayload()
                    => "A\u00A9"u8;
            }
            """;
        var (moduleSymbol, semanticModel) = CompileModule(source);

        var module = await new AstConverter(moduleSymbol, semanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module);
        var export = module.Body.OfType<ExportNamedDeclaration>().Single();
        var function = Assert.IsInstanceOfType<FunctionDeclaration>(export.Declaration);
        var returnStatement = function.Body.Body.OfType<ReturnStatement>().Single();
        AssertUtf8ByteArray(returnStatement.Argument, [65, 194, 169], "module-return");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export function GetPayload()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return [", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script!);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "Utf8StringOperation",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertNoErrors(compilation);

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static (INamedTypeSymbol Module, SemanticModel SemanticModel) CompileModule(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "Utf8StringModule",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertNoErrors(compilation);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var module = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        return (Assert.IsInstanceOfType<INamedTypeSymbol>(semanticModel.GetDeclaredSymbol(module)), semanticModel);
    }

    private static void AssertNoErrors(CSharpCompilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
    }

    private static void AssertUtf8ByteArray(Expression? expression, IReadOnlyList<byte> expectedBytes, string scenarioId)
    {
        Assert.IsInstanceOfType<ArrayExpression>(expression, scenarioId);
        var array = (ArrayExpression)expression!;
        Assert.HasCount(expectedBytes.Count, array.Elements, scenarioId);
        for (var index = 0; index < expectedBytes.Count; index++)
        {
            Assert.IsInstanceOfType<NumericLiteral>(array.Elements[index], scenarioId);
            var literal = (NumericLiteral)array.Elements[index]!;
            Assert.AreEqual((double)expectedBytes[index], literal.Value, scenarioId);
            Assert.AreEqual(
                expectedBytes[index].ToString(System.Globalization.CultureInfo.InvariantCulture),
                literal.Raw,
                scenarioId);
        }
    }
}

public sealed record Utf8LiteralScenario(
    string Id,
    string Expression,
    string DecodedValue,
    IReadOnlyList<byte> ExpectedBytes);

internal static class Utf8LiteralScenarioCatalog
{
    public static IReadOnlyList<Utf8LiteralScenario> All { get; } =
    [
        new("ascii", "\"Jazor\"u8", "Jazor", [74, 97, 122, 111, 114]),
        new("escaped-controls", "\"line\\n\\t\\\"\\\\\\0\"u8", "line\n\t\"\\\0", [108, 105, 110, 101, 10, 9, 34, 92, 0]),
        new("bmp-unicode", "\"A\\u00A9\\u4E2D\"u8", "A\u00A9\u4E2D", [65, 194, 169, 228, 184, 173]),
        new("supplementary-unicode", "\"\\U0001F642\"u8", "\U0001F642", [240, 159, 153, 130]),
        new("raw-literal", "\"\"\"raw \\u00A9\"\"\"u8", "raw \\u00A9", [114, 97, 119, 32, 92, 117, 48, 48, 65, 57])
    ];
}
