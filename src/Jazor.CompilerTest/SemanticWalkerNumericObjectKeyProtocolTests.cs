using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNumericObjectKeyProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IObjectCreationOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<NumericObjectKeyProtocolCase>> Cases
        => NumericObjectKeyProtocolCatalog.Cases.Select(static testCase =>
            new TestDataRow<NumericObjectKeyProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndExpressions()
    {
        var cases = NumericObjectKeyProtocolCatalog.Cases;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.KeyExpression).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("numeric-object-key.", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_NumericObjectKeyProtocol_PreservesClrWidthAndLiteralText(NumericObjectKeyProtocolCase testCase)
    {
        var operation = Operations.Value[testCase.Id];
        Assert.IsNotNull(operation.Initializer, testCase.Id);
        var initializer = operation.Initializer;
        Assert.HasCount(1, initializer.Initializers, testCase.Id);
        var add = Assert.IsInstanceOfType<IInvocationOperation>(initializer.Initializers.Single());
        Assert.AreEqual("Add", add.TargetMethod.Name, testCase.Id);
        Assert.AreEqual("ECMAScript.Number", add.TargetMethod.Parameters[0].Type.ToDisplayString(), testCase.Id);

        var keyArgument = add.Arguments.Single(static argument => argument.Parameter?.Ordinal == 0).Value;
        var numberConversion = Assert.IsInstanceOfType<IConversionOperation>(keyArgument);
        Assert.AreEqual("ECMAScript.Number", numberConversion.Type?.ToDisplayString(), testCase.Id);
        Assert.IsNotNull(numberConversion.OperatorMethod, testCase.Id);
        Assert.IsTrue(numberConversion.Operand.ConstantValue.HasValue, testCase.Id);
        Assert.IsInstanceOfType(numberConversion.Operand.ConstantValue.Value, testCase.ConstantType, testCase.Id);

        var first = new SemanticWalker(true).VisitObjectCreation(operation, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).VisitObjectCreation(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual($"{{ {testCase.ExpectedKey}: 9 }}", first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        _ = new Parser().ParseExpression($"({first})");
    }

    private static IReadOnlyDictionary<string, IObjectCreationOperation> CreateOperations()
    {
        var methods = string.Join(
            Environment.NewLine,
            NumericObjectKeyProtocolCatalog.Cases.Select(static (testCase, index) => $$"""
                    public void Scenario{{index:D2}}()
                    {
                        var state = new HistoryState
                        {
                            { {{testCase.KeyExpression}}, (Number)9 }
                        };
                    }
                """));
        var source = $$"""
            using ECMAScript;

            public sealed class NumericObjectKeyProtocolScenarios
            {
            {{methods}}
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "NumericObjectKeyProtocolScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(HistoryState).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var creations = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText.StartsWith("Scenario", StringComparison.Ordinal))
            .OrderBy(static method => method.Identifier.ValueText, StringComparer.Ordinal)
            .Select(method => Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)))
            .Select(static block => block.DescendantsAndSelf().OfType<IObjectCreationOperation>().Single())
            .ToArray();

        return NumericObjectKeyProtocolCatalog.Cases
            .Select(static testCase => testCase.Id)
            .Zip(creations, static (id, creation) => (id, creation))
            .ToDictionary(static item => item.id, static item => item.creation, StringComparer.Ordinal);
    }
}

public sealed record NumericObjectKeyProtocolCase(
    string Id,
    string Dimension,
    string KeyExpression,
    Type ConstantType,
    string ExpectedKey);

internal static class NumericObjectKeyProtocolCatalog
{
    public static IReadOnlyList<NumericObjectKeyProtocolCase> Cases { get; } =
    [
        Case("byte", "source=byte;format=integer;range=unsigned-8", "(Number)(byte)251", typeof(byte), "251"),
        Case("sbyte", "source=sbyte;format=integer;range=signed-8", "(Number)(sbyte)101", typeof(sbyte), "101"),
        Case("short", "source=short;format=integer;range=signed-16", "(Number)(short)1234", typeof(short), "1234"),
        Case("ushort", "source=ushort;format=integer;range=unsigned-16", "(Number)(ushort)60000", typeof(ushort), "60000"),
        Case("int", "source=int;format=integer;range=signed-32;property-key=computed-negative", "(Number)(-123456789)", typeof(int), "[-123456789]"),
        Case("uint", "source=uint;format=integer;range=unsigned-32", "(Number)4000000000u", typeof(uint), "4000000000"),
        Case("float", "source=float;format=round-trip;fraction=quarter", "(Number)1.25f", typeof(float), "1.25"),
        Case("double", "source=double;format=round-trip;fraction=half", "(Number)2.5d", typeof(double), "2.5"),
        Case("decimal", "source=decimal;format=invariant;fraction=three-quarters", "(Number)3.75m", typeof(decimal), "3.75")
    ];

    private static NumericObjectKeyProtocolCase Case(
        string id,
        string dimension,
        string keyExpression,
        Type constantType,
        string expectedKey)
        => new($"numeric-object-key.{id}", dimension, keyExpression, constantType, expectedKey);
}
