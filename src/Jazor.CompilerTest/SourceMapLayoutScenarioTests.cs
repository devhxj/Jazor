using System.Text.Json;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SourceMapLayoutScenarioTests
{
    public static IEnumerable<TestDataRow<SourceMapLayoutCase>> LayoutCases
        => SourceMapLayoutCaseCatalog.All.Select(static testCase =>
            new TestDataRow<SourceMapLayoutCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SourceMapValidationCase>> ValidationCases
        => SourceMapValidationCaseCatalog.All.Select(static testCase =>
            new TestDataRow<SourceMapValidationCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsAndDimensions()
    {
        var layoutCases = SourceMapLayoutCaseCatalog.All;
        var validationCases = SourceMapValidationCaseCatalog.All;
        var allIds = layoutCases.Select(static testCase => testCase.Id)
            .Concat(validationCases.Select(static testCase => testCase.Id))
            .ToArray();

        Assert.IsNotEmpty(layoutCases);
        Assert.IsNotEmpty(validationCases);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("source-map-layout.", StringComparison.Ordinal)));
        Assert.IsTrue(layoutCases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(validationCases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(LayoutCases))]
    public void Emit_MatchesLayoutScenarioContract(SourceMapLayoutCase testCase)
    {
        switch (testCase.Kind)
        {
            case SourceMapLayoutScenarioKind.ParentAndChildren:
                AssertParentAndChildPositions(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.MultilineBlock:
                AssertMultilineBlockPositions(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.SharedNode:
                AssertSharedNodeUsesFirstPosition(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.CombinedArtifact:
                AssertCombinedArtifactAndLayout(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.SyntheticRoot:
                AssertSyntheticRootHasLayoutWithoutMappings(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.ThrowingSourceReader:
                AssertThrowingSourceReaderOmitsContent(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.RelativeSourcePath:
                AssertRelativeSourcePathIsNormalized(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.InvalidAbsoluteSourcePath:
                AssertInvalidAbsoluteSourcePathIsPreserved(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.SourcePathEqualsRoot:
                AssertSourcePathEqualToRootIsStable(testCase.Id);
                break;
            case SourceMapLayoutScenarioKind.CarriageReturnComment:
                AssertCarriageReturnCommentTracksFollowingNode(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported scenario kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Emit_RejectsInvalidArguments(SourceMapValidationCase testCase)
    {
        var node = new Identifier("value");

        Exception exception = testCase.Kind switch
        {
            SourceMapValidationScenarioKind.NullArtifactNode => Assert.ThrowsExactly<ArgumentNullException>(() =>
                SourceMapEmitter.Emit(
                    null!,
                    KnRJavaScriptTextFormatterOptions.Default,
                    AstToJavaScriptOptions.Default,
                    "module.mjs",
                    includeSourcesContent: false,
                    sourceRootPath: null,
                    readSourceContent: null)),
            SourceMapValidationScenarioKind.NullWriterOptions => Assert.ThrowsExactly<ArgumentNullException>(() =>
                SourceMapEmitter.Emit(
                    node,
                    null!,
                    AstToJavaScriptOptions.Default,
                    "module.mjs",
                    includeSourcesContent: false,
                    sourceRootPath: null,
                    readSourceContent: null)),
            SourceMapValidationScenarioKind.NullAstOptions => Assert.ThrowsExactly<ArgumentNullException>(() =>
                SourceMapEmitter.Emit(
                    node,
                    KnRJavaScriptTextFormatterOptions.Default,
                    null!,
                    "module.mjs",
                    includeSourcesContent: false,
                    sourceRootPath: null,
                    readSourceContent: null)),
            SourceMapValidationScenarioKind.BlankGeneratedFileName => Assert.ThrowsExactly<ArgumentException>(() =>
                SourceMapEmitter.Emit(
                    node,
                    KnRJavaScriptTextFormatterOptions.Default,
                    AstToJavaScriptOptions.Default,
                    " ",
                    includeSourcesContent: false,
                    sourceRootPath: null,
                    readSourceContent: null)),
            SourceMapValidationScenarioKind.NullNodeLayoutNode => Assert.ThrowsExactly<ArgumentNullException>(() =>
                SourceMapEmitter.EmitNodeLayout(
                    null!,
                    KnRJavaScriptTextFormatterOptions.Default,
                    AstToJavaScriptOptions.Default)),
            _ => throw new InvalidOperationException(
                $"{testCase.Id}: unsupported validation kind '{testCase.Kind}'.")
        };

        Assert.AreEqual(testCase.ExpectedParameterName, (exception as ArgumentException)?.ParamName, testCase.Id);
    }

    private static void AssertParentAndChildPositions(string scenarioId)
    {
        var left = new Identifier("left");
        var right = new Identifier("right");
        var expression = new NonLogicalBinaryExpression(Operator.Addition, left, right);

        var layout = expression.ToKnRECMAScriptWithNodePositions();

        Assert.AreEqual("left + right", layout.Content, scenarioId);
        AssertPosition(layout, expression, 0, 0, scenarioId);
        AssertPosition(layout, left, 0, 0, scenarioId);
        AssertPosition(layout, right, 0, 6, scenarioId);
    }

    private static void AssertMultilineBlockPositions(string scenarioId)
    {
        var first = new Identifier("first");
        var second = new Identifier("second");
        var block = new NestedBlockStatement(NodeList.From<Statement>(
            new NonSpecialExpressionStatement(first),
            new NonSpecialExpressionStatement(second)));

        var layout = block.ToKnRECMAScriptWithNodePositions();
        var firstPosition = GetLineColumnContaining(layout.Content, "first");
        var secondPosition = GetLineColumnContaining(layout.Content, "second");

        AssertPosition(layout, block, 0, 0, scenarioId);
        AssertPosition(layout, first, firstPosition.Line, firstPosition.Column, scenarioId);
        AssertPosition(layout, second, secondPosition.Line, secondPosition.Column, scenarioId);
        Assert.IsGreaterThan(firstPosition.Line, secondPosition.Line, scenarioId);
    }

    private static void AssertSharedNodeUsesFirstPosition(string scenarioId)
    {
        var shared = new Identifier("shared");
        var sequence = new SequenceExpression(NodeList.From<Expression>(shared, shared));

        var layout = sequence.ToKnRECMAScriptWithNodePositions();
        var firstPosition = GetLineColumnContaining(layout.Content, "shared");

        Assert.AreEqual("shared, shared", layout.Content, scenarioId);
        AssertPosition(layout, shared, firstPosition.Line, firstPosition.Column, scenarioId);
        Assert.HasCount(2, layout.NodePositions, scenarioId);
    }

    private static void AssertCombinedArtifactAndLayout(string scenarioId)
    {
        const string sourcePath = "Demo/CombinedLayout.cs";
        const string sourceContent = "int total = left + right;";
        var left = new Identifier("left");
        var right = new Identifier("right");
        var expression = new NonLogicalBinaryExpression(Operator.Addition, left, right)
        {
            UserData = new SourceOrigin(sourcePath, 0, 12, 0, 24)
        };

        var layout = expression.ToKnRECMAScriptWithSourceMapAndNodePositions(
            generatedFileName: "combined-layout.mjs",
            includeSourcesContent: true,
            readSourceContent: path => path == sourcePath ? sourceContent : null);

        Assert.AreEqual(expression.ToKnRECMAScript(), layout.Artifact.Content, scenarioId);
        AssertPosition(layout, expression, 0, 0, scenarioId);
        AssertPosition(layout, left, 0, 0, scenarioId);
        AssertPosition(layout, right, 0, 6, scenarioId);

        using var parsed = JsonDocument.Parse(layout.Artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        Assert.AreEqual("combined-layout.mjs", sourceMap.GetProperty("file").GetString(), scenarioId);
        Assert.AreEqual(sourceContent, sourceMap.GetProperty("sourcesContent")[0].GetString(), scenarioId);
        Assert.IsNotEmpty(DecodeSegments(sourceMap), scenarioId);
    }

    private static void AssertSyntheticRootHasLayoutWithoutMappings(string scenarioId)
    {
        var expression = new Identifier("synthetic")
        {
            UserData = new SourceOrigin(
                "Demo/Synthetic.cs",
                10,
                2,
                10,
                11,
                IsSynthetic: true)
        };

        var layout = expression.ToKnRECMAScriptWithSourceMapAndNodePositions(
            generatedFileName: "synthetic.mjs",
            includeSourcesContent: false);

        AssertPosition(layout, expression, 0, 0, scenarioId);
        using var parsed = JsonDocument.Parse(layout.Artifact.SourceMapContent!);
        Assert.AreEqual(0, parsed.RootElement.GetProperty("sources").GetArrayLength(), scenarioId);
        Assert.AreEqual(string.Empty, parsed.RootElement.GetProperty("mappings").GetString(), scenarioId);
    }

    private static void AssertThrowingSourceReaderOmitsContent(string scenarioId)
    {
        var expression = new Identifier("value")
        {
            UserData = new SourceOrigin("Demo/Unavailable.cs", 2, 1, 2, 6)
        };

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "unavailable.mjs",
            includeSourcesContent: true,
            readSourceContent: static _ => throw new IOException("source unavailable"));

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        Assert.AreEqual(1, sourceMap.GetProperty("sources").GetArrayLength(), scenarioId);
        Assert.IsFalse(sourceMap.TryGetProperty("sourcesContent", out _), scenarioId);
        Assert.IsNotEmpty(DecodeSegments(sourceMap), scenarioId);
    }

    private static void AssertRelativeSourcePathIsNormalized(string scenarioId)
    {
        var expression = new Identifier("relative")
        {
            UserData = new SourceOrigin("./Demo/Relative.cs", 4, 3, 4, 11)
        };

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "relative.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        Assert.AreEqual("Demo/Relative.cs", parsed.RootElement.GetProperty("sources")[0].GetString(), scenarioId);
    }

    private static void AssertInvalidAbsoluteSourcePathIsPreserved(string scenarioId)
    {
        const string sourcePath = "C:\\invalid\0source.cs";
        var expression = new Identifier("value")
        {
            UserData = new SourceOrigin(sourcePath, 1, 0, 1, 5)
        };

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "invalid-path.mjs",
            includeSourcesContent: false,
            sourceRootPath: "C:\\source");

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        Assert.AreEqual("C:/invalid\0source.cs", parsed.RootElement.GetProperty("sources")[0].GetString(), scenarioId);
        Assert.IsNotEmpty(DecodeSegments(parsed.RootElement), scenarioId);
    }

    private static void AssertSourcePathEqualToRootIsStable(string scenarioId)
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "jazor-source-root"));
        var expression = new Identifier("value")
        {
            UserData = new SourceOrigin(sourceRoot, 1, 0, 1, 5)
        };

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "root-source.mjs",
            includeSourcesContent: false,
            sourceRootPath: sourceRoot);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        Assert.AreEqual(sourceRoot.Replace('\\', '/'), parsed.RootElement.GetProperty("sources")[0].GetString(), scenarioId);
    }

    private static void AssertCarriageReturnCommentTracksFollowingNode(string scenarioId)
    {
        var value = new Identifier("value");
        var block = new NestedBlockStatement(NodeList.From<Statement>(
            new BlockComment("first\rsecond"),
            new NonSpecialExpressionStatement(value)));

        var layout = block.ToKnRECMAScriptWithNodePositions();
        var valuePosition = GetLineColumnContaining(layout.Content, "value");

        AssertPosition(layout, value, valuePosition.Line, valuePosition.Column, scenarioId);
        Assert.IsGreaterThanOrEqualTo(2, valuePosition.Line, scenarioId);
    }

    private static void AssertPosition(
        GeneratedJavaScriptNodeLayout layout,
        Node node,
        int expectedLine,
        int expectedColumn,
        string scenarioId)
    {
        Assert.IsTrue(layout.NodePositions.TryGetValue(node, out var position), scenarioId);
        Assert.AreEqual(expectedLine, position.Line, scenarioId);
        Assert.AreEqual(expectedColumn, position.Column, scenarioId);
    }

    private static void AssertPosition(
        GeneratedJavaScriptLayout layout,
        Node node,
        int expectedLine,
        int expectedColumn,
        string scenarioId)
    {
        Assert.IsTrue(layout.NodePositions.TryGetValue(node, out var position), scenarioId);
        Assert.AreEqual(expectedLine, position.Line, scenarioId);
        Assert.AreEqual(expectedColumn, position.Column, scenarioId);
    }
}

public enum SourceMapLayoutScenarioKind
{
    ParentAndChildren,
    MultilineBlock,
    SharedNode,
    CombinedArtifact,
    SyntheticRoot,
    ThrowingSourceReader,
    RelativeSourcePath,
    InvalidAbsoluteSourcePath,
    SourcePathEqualsRoot,
    CarriageReturnComment
}

public sealed record SourceMapLayoutCase(
    string Id,
    string Dimension,
    SourceMapLayoutScenarioKind Kind);

internal static class SourceMapLayoutCaseCatalog
{
    public static IReadOnlyList<SourceMapLayoutCase> All { get; } =
    [
        Case("source-map-layout.parent-and-children", "nested-node-visit-entry-coordinates", SourceMapLayoutScenarioKind.ParentAndChildren),
        Case("source-map-layout.multiline-block", "line-and-column-tracking", SourceMapLayoutScenarioKind.MultilineBlock),
        Case("source-map-layout.shared-node", "first-visit-position-stability", SourceMapLayoutScenarioKind.SharedNode),
        Case("source-map-layout.combined-artifact", "source-map-and-node-entry-consistency", SourceMapLayoutScenarioKind.CombinedArtifact),
        Case("source-map-layout.synthetic-root", "synthetic-origin-suppression", SourceMapLayoutScenarioKind.SyntheticRoot),
        Case("source-map-layout.throwing-source-reader", "source-content-error-isolation", SourceMapLayoutScenarioKind.ThrowingSourceReader),
        Case("source-map-layout.relative-source-path", "relative-source-normalization", SourceMapLayoutScenarioKind.RelativeSourcePath),
        Case("source-map-layout.invalid-absolute-source-path", "invalid-absolute-path-error-isolation", SourceMapLayoutScenarioKind.InvalidAbsoluteSourcePath),
        Case("source-map-layout.source-equals-root", "root-equality-normalization", SourceMapLayoutScenarioKind.SourcePathEqualsRoot),
        Case("source-map-layout.carriage-return-comment", "carriage-return-coordinate-tracking", SourceMapLayoutScenarioKind.CarriageReturnComment)
    ];

    private static SourceMapLayoutCase Case(
        string id,
        string dimension,
        SourceMapLayoutScenarioKind kind)
        => new(id, dimension, kind);
}

public enum SourceMapValidationScenarioKind
{
    NullArtifactNode,
    NullWriterOptions,
    NullAstOptions,
    BlankGeneratedFileName,
    NullNodeLayoutNode
}

public sealed record SourceMapValidationCase(
    string Id,
    string Dimension,
    SourceMapValidationScenarioKind Kind,
    string ExpectedParameterName);

internal static class SourceMapValidationCaseCatalog
{
    public static IReadOnlyList<SourceMapValidationCase> All { get; } =
    [
        Case("source-map-layout.validation.null-artifact-node", "artifact-node-required", SourceMapValidationScenarioKind.NullArtifactNode, "node"),
        Case("source-map-layout.validation.null-writer-options", "writer-options-required", SourceMapValidationScenarioKind.NullWriterOptions, "writerOptions"),
        Case("source-map-layout.validation.null-ast-options", "ast-options-required", SourceMapValidationScenarioKind.NullAstOptions, "astOptions"),
        Case("source-map-layout.validation.blank-generated-file", "generated-file-required", SourceMapValidationScenarioKind.BlankGeneratedFileName, "generatedFileName"),
        Case("source-map-layout.validation.null-layout-node", "layout-node-required", SourceMapValidationScenarioKind.NullNodeLayoutNode, "node")
    ];

    private static SourceMapValidationCase Case(
        string id,
        string dimension,
        SourceMapValidationScenarioKind kind,
        string expectedParameterName)
        => new(id, dimension, kind, expectedParameterName);
}
