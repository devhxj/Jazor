using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class TransformationExceptionScenarioTests
{
    public static IEnumerable<TestDataRow<SymbolTransformationExceptionScenario>> SymbolCases
        => TransformationExceptionScenarioCatalog.Symbols.Select(static testCase =>
            new TestDataRow<SymbolTransformationExceptionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<OperationTransformationExceptionScenario>> OperationCases
        => TransformationExceptionScenarioCatalog.Operations.Select(static testCase =>
            new TestDataRow<OperationTransformationExceptionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SyntaxTransformationExceptionScenario>> SyntaxCases
        => TransformationExceptionScenarioCatalog.SyntaxNodes.Select(static testCase =>
            new TestDataRow<SyntaxTransformationExceptionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<OperationLocationExceptionScenario>> LocationCases
        => TransformationExceptionScenarioCatalog.Locations.Select(static testCase =>
            new TestDataRow<OperationLocationExceptionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsAndKinds()
    {
        var allIds = TransformationExceptionScenarioCatalog.Symbols.Select(static testCase => testCase.Id)
            .Concat(TransformationExceptionScenarioCatalog.Operations.Select(static testCase => testCase.Id))
            .Concat(TransformationExceptionScenarioCatalog.SyntaxNodes.Select(static testCase => testCase.Id))
            .Concat(TransformationExceptionScenarioCatalog.Locations.Select(static testCase => testCase.Id))
            .ToArray();
        var allDimensions = TransformationExceptionScenarioCatalog.Symbols.Select(static testCase => testCase.Dimension)
            .Concat(TransformationExceptionScenarioCatalog.Operations.Select(static testCase => testCase.Dimension))
            .Concat(TransformationExceptionScenarioCatalog.SyntaxNodes.Select(static testCase => testCase.Dimension))
            .Concat(TransformationExceptionScenarioCatalog.Locations.Select(static testCase => testCase.Dimension))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("transformation-exception.", StringComparison.Ordinal)));
        Assert.IsTrue(allDimensions.All(static dimension => !string.IsNullOrWhiteSpace(dimension)));
        Assert.HasCount(
            Enum.GetValues<OperationLocationExceptionScenarioKind>().Length,
            TransformationExceptionScenarioCatalog.Locations.Select(static testCase => testCase.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(SymbolCases))]
    public void SymbolException_PreservesKindMessageAndInnerException(
        SymbolTransformationExceptionScenario testCase)
    {
        var inner = testCase.HasInnerException
            ? new InvalidOperationException(testCase.Id + ".inner")
            : null;

        var exception = inner is null
            ? new SymbolTransformationException(testCase.Kind, testCase.Message)
            : new SymbolTransformationException(testCase.Kind, testCase.Message, inner);

        Assert.AreEqual(testCase.Kind, exception.Kind, testCase.Id);
        Assert.AreEqual(testCase.Message, exception.Message, testCase.Id);
        Assert.AreSame(inner, exception.InnerException, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(OperationCases))]
    public void OperationException_PreservesKindMessageAndInnerException(
        OperationTransformationExceptionScenario testCase)
    {
        var inner = testCase.HasInnerException
            ? new InvalidOperationException(testCase.Id + ".inner")
            : null;

        var exception = inner is null
            ? new OperationTransformationException(testCase.Kind, testCase.Message)
            : new OperationTransformationException(testCase.Kind, testCase.Message, inner);

        Assert.AreEqual(testCase.Kind, exception.Kind, testCase.Id);
        Assert.AreEqual(testCase.Message, exception.Message, testCase.Id);
        Assert.AreSame(inner, exception.InnerException, testCase.Id);
        Assert.HasCount(0, exception.Data, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(SyntaxCases))]
    public void SyntaxException_PreservesKindMessageAndInnerException(
        SyntaxTransformationExceptionScenario testCase)
    {
        var inner = testCase.HasInnerException
            ? new InvalidOperationException(testCase.Id + ".inner")
            : null;

        var exception = inner is null
            ? new SyntaxNodeTransformationException(testCase.Kind, testCase.Message)
            : new SyntaxNodeTransformationException(testCase.Kind, testCase.Message, inner);

        Assert.AreEqual(testCase.Kind, exception.Kind, testCase.Id);
        Assert.AreEqual(testCase.Message, exception.Message, testCase.Id);
        Assert.AreSame(inner, exception.InnerException, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(LocationCases))]
    public void OperationException_MatchesSourceLocationContract(OperationLocationExceptionScenario testCase)
    {
        if (testCase.Kind == OperationLocationExceptionScenarioKind.NullOperation)
        {
            var argumentException = Assert.ThrowsExactly<ArgumentNullException>(() =>
                new OperationTransformationException(null!, testCase.Message));

            Assert.AreEqual("operation", argumentException.ParamName, testCase.Id);
            return;
        }

        var operation = GetBinaryOperation(testCase.SourcePath);
        var exception = new OperationTransformationException(operation, testCase.Message);

        Assert.AreEqual(OperationKind.Binary, exception.Kind, testCase.Id);
        Assert.AreEqual(testCase.Message, exception.Message, testCase.Id);
        Assert.AreEqual(testCase.ExpectedLocationPath, ReadData<string>(exception, "location.path", testCase.Id));
        Assert.AreEqual(5, ReadData<int>(exception, "location.startLine", testCase.Id));
        Assert.AreEqual(16, ReadData<int>(exception, "location.startColumn", testCase.Id));
        Assert.AreEqual(5, ReadData<int>(exception, "location.endLine", testCase.Id));
        Assert.AreEqual(25, ReadData<int>(exception, "location.endColumn", testCase.Id));
    }

    [TestMethod]
    public void ExplicitSourceLocationConstructors_PreserveMetadataAndNormalizeMissingLocations()
    {
        var fileTree = CSharpSyntaxTree.ParseText(
            "class Sample { int Value; }",
            TestMetadataReferences.PreviewParseOptions,
            path: "src/ExceptionContracts.cs");
        var memoryTree = CSharpSyntaxTree.ParseText(
            "class Sample { int Value; }",
            TestMetadataReferences.PreviewParseOptions,
            path: string.Empty);
        var fileLocation = fileTree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().Single().GetLocation();
        var memoryLocation = memoryTree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().Single().GetLocation();
        var inner = new InvalidOperationException("inner");

        var operation = new OperationTransformationException(
            OperationKind.FieldReference,
            "operation location",
            fileLocation,
            inner);
        var syntax = new SyntaxNodeTransformationException(
            SyntaxKind.FieldDeclaration,
            "syntax location",
            fileLocation);
        var inMemorySyntax = new SyntaxNodeTransformationException(
            SyntaxKind.FieldDeclaration,
            "memory syntax location",
            memoryLocation,
            inner);
        var missingSymbol = new SymbolTransformationException(SymbolKind.Field, "missing symbol", (Location)null!);
        var missingOperation = new OperationTransformationException(OperationKind.FieldReference, "missing operation", (Location)null!);
        var missingSyntax = new SyntaxNodeTransformationException(SyntaxKind.FieldDeclaration, "missing syntax", (Location)null!);

        Assert.AreSame(inner, operation.InnerException);
        Assert.AreEqual("src/ExceptionContracts.cs", ReadData<string>(operation, "location.path", "operation"));
        Assert.AreEqual("src/ExceptionContracts.cs", ReadData<string>(syntax, "location.path", "syntax"));
        Assert.AreEqual("<unknown>", ReadData<string>(inMemorySyntax, "location.path", "memory syntax"));
        Assert.AreSame(inner, inMemorySyntax.InnerException);
        Assert.AreEqual(Location.None, missingSymbol.SourceLocation);
        Assert.AreEqual(Location.None, missingOperation.SourceLocation);
        Assert.AreEqual(Location.None, missingSyntax.SourceLocation);
        Assert.HasCount(0, missingOperation.Data);
        Assert.HasCount(0, missingSyntax.Data);
    }

    [TestMethod]
    public void InnerExceptionLocationConstructors_NormalizeNullLocationsWithoutMetadata()
    {
        var inner = new InvalidOperationException("inner");
        var symbol = new SymbolTransformationException(
            SymbolKind.Field,
            "symbol",
            (Location)null!,
            inner);
        var operation = new OperationTransformationException(
            OperationKind.FieldReference,
            "operation",
            (Location)null!,
            inner);
        var syntax = new SyntaxNodeTransformationException(
            SyntaxKind.FieldDeclaration,
            "syntax",
            (Location)null!,
            inner);

        Assert.AreSame(inner, symbol.InnerException);
        Assert.AreSame(inner, operation.InnerException);
        Assert.AreSame(inner, syntax.InnerException);
        Assert.AreEqual(Location.None, symbol.SourceLocation);
        Assert.AreEqual(Location.None, operation.SourceLocation);
        Assert.AreEqual(Location.None, syntax.SourceLocation);
        Assert.HasCount(0, operation.Data);
        Assert.HasCount(0, syntax.Data);
    }

    private static T ReadData<T>(Exception exception, string key, string scenarioId)
    {
        var value = exception.Data[key];
        Assert.IsInstanceOfType<T>(value, $"{scenarioId}: metadata '{key}'.");
        return (T)value;
    }

    private static IBinaryOperation GetBinaryOperation(string sourcePath)
    {
        const string source = """
            class Sample
            {
                int Method(int value)
                {
                    return value + 1;
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: sourcePath);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TransformationExceptionScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var binary = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Single();
        return (IBinaryOperation?)compilation.GetSemanticModel(syntaxTree).GetOperation(binary)
            ?? throw new InvalidOperationException("Expected a binary operation for the return expression.");
    }
}

public sealed record SymbolTransformationExceptionScenario(
    string Id,
    string Dimension,
    SymbolKind Kind,
    string Message,
    bool HasInnerException);

public sealed record OperationTransformationExceptionScenario(
    string Id,
    string Dimension,
    OperationKind Kind,
    string Message,
    bool HasInnerException);

public sealed record SyntaxTransformationExceptionScenario(
    string Id,
    string Dimension,
    SyntaxKind Kind,
    string Message,
    bool HasInnerException);

public enum OperationLocationExceptionScenarioKind
{
    FileBackedSource,
    InMemorySource,
    NullOperation
}

public sealed record OperationLocationExceptionScenario(
    string Id,
    string Dimension,
    OperationLocationExceptionScenarioKind Kind,
    string SourcePath,
    string ExpectedLocationPath,
    string Message);

internal static class TransformationExceptionScenarioCatalog
{
    public static IReadOnlyList<SymbolTransformationExceptionScenario> Symbols { get; } =
    [
        new(
            "transformation-exception.symbol.message",
            "symbol-kind-and-message",
            SymbolKind.Field,
            "Field lowering is not supported.",
            HasInnerException: false),
        new(
            "transformation-exception.symbol.inner",
            "symbol-inner-exception-propagation",
            SymbolKind.Method,
            "Method lowering failed.",
            HasInnerException: true)
    ];

    public static IReadOnlyList<OperationTransformationExceptionScenario> Operations { get; } =
    [
        new(
            "transformation-exception.operation.message",
            "operation-kind-and-message",
            OperationKind.Invocation,
            "Invocation lowering is not supported.",
            HasInnerException: false),
        new(
            "transformation-exception.operation.inner",
            "operation-inner-exception-propagation",
            OperationKind.Conversion,
            "Conversion lowering failed.",
            HasInnerException: true)
    ];

    public static IReadOnlyList<SyntaxTransformationExceptionScenario> SyntaxNodes { get; } =
    [
        new(
            "transformation-exception.syntax.message",
            "syntax-kind-and-message",
            SyntaxKind.MethodDeclaration,
            "Method syntax lowering is not supported.",
            HasInnerException: false),
        new(
            "transformation-exception.syntax.inner",
            "syntax-inner-exception-propagation",
            SyntaxKind.PropertyDeclaration,
            "Property syntax lowering failed.",
            HasInnerException: true)
    ];

    public static IReadOnlyList<OperationLocationExceptionScenario> Locations { get; } =
    [
        new(
            "transformation-exception.operation.location.file",
            "file-backed-one-based-source-span",
            OperationLocationExceptionScenarioKind.FileBackedSource,
            "src/Feature.cs",
            "src/Feature.cs",
            "Binary expression lowering failed."),
        new(
            "transformation-exception.operation.location.memory",
            "in-memory-source-path-fallback",
            OperationLocationExceptionScenarioKind.InMemorySource,
            string.Empty,
            "<unknown>",
            "In-memory expression lowering failed."),
        new(
            "transformation-exception.operation.location.null",
            "null-operation-validation",
            OperationLocationExceptionScenarioKind.NullOperation,
            string.Empty,
            string.Empty,
            "Missing operation.")
    ];
}
