using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerSourceMapEmissionTest
{
    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_InheritsParentOriginForUntaggedChildNodes()
    {
        var expression = new NonLogicalBinaryExpression(
            Operator.Addition,
            new Identifier("a"),
            new Identifier("b"));
        expression.UserData = CreateSourceOrigin(
            sourcePath: "Demo/Inherit.cs",
            startLine: 9,
            startColumn: 4,
            endLine: 9,
            endColumn: 9);

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "inherit.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        Assert.AreEqual("inherit.mjs", sourceMap.GetProperty("file").GetString());
        Assert.AreEqual("Demo/Inherit.cs", sourceMap.GetProperty("sources")[0].GetString());

        var mappings = sourceMap.GetProperty("mappings").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(mappings));
        StringAssert.Contains(mappings!, ",");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_SyntheticNodeDoesNotBreakInheritedOriginForDescendants()
    {
        var syntheticWrapper = new MemberExpression(
            new Identifier("obj"),
            new Identifier("tmp"),
            computed: false,
            optional: false);
        syntheticWrapper.UserData = CreateSourceOrigin(
            sourcePath: "Demo/SyntheticInherit.cs",
            startLine: 80,
            startColumn: 3,
            endLine: 80,
            endColumn: 7,
            isSynthetic: true);

        var expression = new NonLogicalBinaryExpression(
            Operator.Addition,
            syntheticWrapper,
            new Identifier("value"));
        expression.UserData = CreateSourceOrigin(
            sourcePath: "Demo/SyntheticInherit.cs",
            startLine: 4,
            startColumn: 2,
            endLine: 4,
            endColumn: 18);

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "synthetic-inherit.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "SyntheticInherit.cs");
        var segments = DecodeSegments(sourceMap);
        var tmpPosition = GetLineColumnContaining(artifact.Content, "tmp");

        Assert.IsTrue(
            segments.Any(segment =>
                segment.GeneratedLine == tmpPosition.Line &&
                segment.GeneratedColumn == tmpPosition.Column &&
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == 4),
            "Expected child token inside synthetic wrapper to inherit non-synthetic parent origin.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_SameGeneratedPositionPrefersInnermostTaggedOrigin()
    {
        var left = new Identifier("left");
        left.UserData = CreateSourceOrigin(
            sourcePath: "Demo/SpecificOrigin.cs",
            startLine: 20,
            startColumn: 3,
            endLine: 20,
            endColumn: 7);

        var expression = new NonLogicalBinaryExpression(
            Operator.Addition,
            left,
            new Identifier("right"));
        expression.UserData = CreateSourceOrigin(
            sourcePath: "Demo/SpecificOrigin.cs",
            startLine: 4,
            startColumn: 1,
            endLine: 4,
            endColumn: 14);

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "specific-origin.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "SpecificOrigin.cs");
        var leftPosition = GetLineColumnContaining(artifact.Content, "left");
        var segments = DecodeSegments(sourceMap);

        Assert.IsTrue(
            segments.Any(segment =>
                segment.GeneratedLine == leftPosition.Line &&
                segment.GeneratedColumn == leftPosition.Column &&
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == 20),
            "Expected same generated position to prefer innermost child origin.");
        Assert.IsFalse(
            segments.Any(segment =>
                segment.GeneratedLine == leftPosition.Line &&
                segment.GeneratedColumn == leftPosition.Column &&
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == 4),
            "Did not expect parent origin to override child origin at same generated position.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_AbsolutePathsWithSameFileName_AreNotCollapsed()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.SourceMap.PathCollision");
        var firstPath = Path.Combine(root, "FeatureA", "Shared.cs");
        var secondPath = Path.Combine(root, "FeatureB", "Shared.cs");

        var first = new Identifier("first");
        first.UserData = CreateSourceOrigin(
            sourcePath: firstPath,
            startLine: 10,
            startColumn: 2,
            endLine: 10,
            endColumn: 7);

        var second = new Identifier("second");
        second.UserData = CreateSourceOrigin(
            sourcePath: secondPath,
            startLine: 30,
            startColumn: 1,
            endLine: 30,
            endColumn: 7);

        var sequence = new SequenceExpression(NodeList.From(new Expression[] { first, second }));
        sequence.UserData = CreateSourceOrigin(
            sourcePath: firstPath,
            startLine: 1,
            startColumn: 0,
            endLine: 1,
            endColumn: 5);

        var artifact = sequence.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "path-collision.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sources = sourceMap.GetProperty("sources");
        Assert.AreEqual(2, sources.GetArrayLength(), "Expected same filename from different absolute paths to remain distinct sources.");

        var firstSourceIndex = FindSourceIndexContaining(sourceMap, "FeatureA/Shared.cs");
        var secondSourceIndex = FindSourceIndexContaining(sourceMap, "FeatureB/Shared.cs");
        var segments = DecodeSegments(sourceMap);
        var firstPosition = GetLineColumnContaining(artifact.Content, "first");

        Assert.IsTrue(
            segments.Any(segment =>
                segment.GeneratedLine == firstPosition.Line &&
                segment.GeneratedColumn == firstPosition.Column &&
                segment.SourceIndex == firstSourceIndex),
            "Expected first token to map to FeatureA source.");
        Assert.IsTrue(
            segments.Any(segment =>
                segment.SourceIndex == secondSourceIndex),
            "Expected source-map segments to include FeatureB source.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_FileUriUnderSourceRoot_NormalizesToRootRelativeSource()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.SourceMap.UriRoot");
        var sourcePath = Path.Combine(root, "FeatureA", "UriCase.cs");
        var fileUri = new Uri(sourcePath).AbsoluteUri;

        var expression = new Identifier("value");
        expression.UserData = CreateSourceOrigin(
            sourcePath: fileUri,
            startLine: 5,
            startColumn: 1,
            endLine: 5,
            endColumn: 6);

        var artifact = expression.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "uri-root.mjs",
            includeSourcesContent: false,
            sourceRootPath: root);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var source = sourceMap.GetProperty("sources")[0].GetString();
        Assert.AreEqual("FeatureA/UriCase.cs", source);
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_AbsolutePathAndFileUriOfSameFile_AreDeduplicated()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.SourceMap.UriDedup");
        var sourcePath = Path.Combine(root, "FeatureA", "Dedup.cs");
        var fileUri = new Uri(sourcePath).AbsoluteUri;

        var first = new Identifier("first");
        first.UserData = CreateSourceOrigin(
            sourcePath: sourcePath,
            startLine: 10,
            startColumn: 1,
            endLine: 10,
            endColumn: 6);

        var second = new Identifier("second");
        second.UserData = CreateSourceOrigin(
            sourcePath: fileUri,
            startLine: 11,
            startColumn: 1,
            endLine: 11,
            endColumn: 7);

        var sequence = new SequenceExpression(NodeList.From(new Expression[] { first, second }));
        sequence.UserData = CreateSourceOrigin(
            sourcePath: sourcePath,
            startLine: 1,
            startColumn: 0,
            endLine: 1,
            endColumn: 6);

        var artifact = sequence.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "uri-dedup.mjs",
            includeSourcesContent: false,
            sourceRootPath: root);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sources = sourceMap.GetProperty("sources");

        Assert.AreEqual(1, sources.GetArrayLength(), "Expected same file in absolute path and file URI form to deduplicate into one source entry.");
        Assert.AreEqual("FeatureA/Dedup.cs", sources[0].GetString());
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_TupleSwapLowering_MapsLoweredSegmentsBackToSwapLine()
    {
        const string sourcePath = "Demo/TupleSwap.cs";
        const string source = """
            class TestClass
            {
                void M()
                {
                    int a = 1, b = 2;
                    (a, b) = (b, a);
                    _ = a + b;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "tuple-swap.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "TupleSwap.cs");
        var swapSourceLine = GetLineIndexContaining(source, "(a, b) = (b, a);");
        var swapGeneratedLine = GetLineIndexContaining(artifact.Content, "v$0 = b, v$1 = a, a = v$0, b = v$1;");
        var decodedLines = DecodeGeneratedLineToSourceLocation(sourceMap);

        Assert.IsTrue(
            decodedLines.TryGetValue(swapGeneratedLine, out var swapLocation),
            "Expected lowered tuple swap line to have source-map mapping.");
        Assert.AreEqual(sourceIndex, swapLocation.SourceIndex);
        Assert.AreEqual(swapSourceLine, swapLocation.SourceLine);

        var segmentsOnSwapLine = DecodeSegments(sourceMap)
            .Where(segment => segment.GeneratedLine == swapGeneratedLine &&
                              segment.SourceIndex == sourceIndex &&
                              segment.SourceLine == swapSourceLine)
            .ToArray();
        Assert.IsTrue(
            segmentsOnSwapLine.Length >= 2,
            "Expected multiple lowered segments on tuple swap line to map back to the source swap statement.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_TryCatchLowering_MapsCatchTokenBackToCatchClauseLine()
    {
        const string sourcePath = "Demo/TryCatch.cs";
        const string source = """
            class TestClass
            {
                void M()
                {
                    try
                    {
                        throw new System.Exception("boom");
                    }
                    catch (System.Exception ex)
                    {
                        _ = ex.Message;
                    }
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "try-catch.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "TryCatch.cs");
        var catchSourceLine = GetLineIndexContaining(source, "catch (System.Exception ex)");
        var catchPosition = GetLineColumnContaining(artifact.Content, "catch");
        var segments = DecodeSegments(sourceMap);

        var catchLineSegments = segments
            .Where(segment =>
                segment.GeneratedLine == catchPosition.Line &&
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == catchSourceLine)
            .ToArray();

        Assert.IsTrue(catchLineSegments.Length > 0, "Expected catch line to map back to the C# catch clause line.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_PatternLowering_MapsPatternLineWithMultipleSegments()
    {
        const string sourcePath = "Demo/Pattern.cs";
        const string source = """
            class TestClass
            {
                bool M()
                {
                    var tuple = (1, "hello");
                    bool result = tuple is (int x, string s);
                    return result;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "pattern.mjs",
            includeSourcesContent: false);

        StringAssert.Contains(artifact.Content, "typeof");

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "Pattern.cs");
        var patternSourceLine = GetLineIndexContaining(source, "bool result = tuple is (int x, string s);");

        var patternSegments = DecodeSegments(sourceMap)
            .Where(segment =>
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == patternSourceLine)
            .ToArray();

        Assert.IsTrue(patternSegments.Length >= 2, "Expected lowered pattern logic to emit multiple mappings for the same source pattern line.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_ControlFlowLowering_MapsLoopSwitchAndBranchBodies()
    {
        const string sourcePath = "Demo/ControlFlow.cs";
        const string source = """
            class TestClass
            {
                int M(int[] values)
                {
                    int total = 0;
                    for (int i = 0; i < values.Length; i++)
                    {
                        total += values[i];
                    }
                    foreach (var value in values)
                    {
                        total += value;
                    }
                    while (total < 20)
                    {
                        total++;
                    }
                    do
                    {
                        total--;
                    }
                    while (total > 10);
                    switch (total)
                    {
                        case 1:
                            total += 100;
                            break;
                        case 2:
                        case 3:
                            total += 200;
                            break;
                        default:
                            total += 300;
                            break;
                    }

                    return total;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "control-flow.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "ControlFlow.cs");
        var segments = DecodeSegments(sourceMap);

        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "for (int i = 0; i < values.Length; i++)"),
            GetLineIndexContaining(source, "foreach (var value in values)"),
            GetLineIndexContaining(source, "while (total < 20)"),
            GetLineIndexContaining(source, "do"),
            GetLineIndexContaining(source, "while (total > 10);"),
            GetLineIndexContaining(source, "switch (total)"),
            GetLineIndexContaining(source, "total += 100;"),
            GetLineIndexContaining(source, "total += 200;"),
            GetLineIndexContaining(source, "total += 300;"),
            GetLineIndexContaining(source, "return total;"));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_SwitchExpressionPatternLowering_MapsEachArmLine()
    {
        const string sourcePath = "Demo/SwitchExpressionPattern.cs";
        const string source = """
            class TestClass
            {
                string M(object value)
                {
                    return value switch
                    {
                        int i when i > 0 => "positive-int",
                        string s => s,
                        null => "null",
                        _ => "other"
                    };
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "switch-expression-pattern.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "SwitchExpressionPattern.cs");
        var segments = DecodeSegments(sourceMap);

        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "return value switch"),
            GetLineIndexContaining(source, "int i when i > 0 => \"positive-int\","),
            GetLineIndexContaining(source, "string s => s,"),
            GetLineIndexContaining(source, "null => \"null\","),
            GetLineIndexContaining(source, "_ => \"other\""));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_NestedTupleDeconstruction_MapsMultipleLoweredSegmentsBackToAssignmentLine()
    {
        const string sourcePath = "Demo/NestedDeconstruct.cs";
        const string source = """
            class TestClass
            {
                (int Left, (int Mid, int Right)) GetTuple()
                {
                    return (1, (2, 3));
                }

                int M()
                {
                    int a = 0, b = 0, c = 0;
                    (a, (b, c)) = GetTuple();
                    return a + b + c;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "nested-deconstruct.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "NestedDeconstruct.cs");
        var segments = DecodeSegments(sourceMap);
        var deconstructSourceLine = GetLineIndexContaining(source, "(a, (b, c)) = GetTuple();");

        var deconstructSegments = segments
            .Where(segment =>
                segment.SourceIndex == sourceIndex &&
                segment.SourceLine == deconstructSourceLine)
            .ToArray();

        Assert.IsTrue(
            deconstructSegments.Length >= 3,
            "Expected nested deconstruction lowering to emit multiple mappings back to the source deconstruction line.");
        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "return a + b + c;"));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_ConditionalAccessLowering_MapsConditionalAccessLines()
    {
        const string sourcePath = "Demo/ConditionalAccess.cs";
        const string source = """
            class TestClass
            {
                int M(int[] values)
                {
                    var length = values?.Length ?? 0;
                    var first = values?[0] ?? -1;
                    return length + first;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "conditional-access.mjs",
            includeSourcesContent: false);

        StringAssert.Contains(artifact.Content, "?.");

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "ConditionalAccess.cs");
        var segments = DecodeSegments(sourceMap);

        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "var length = values?.Length ?? 0;"),
            GetLineIndexContaining(source, "var first = values?[0] ?? -1;"),
            GetLineIndexContaining(source, "return length + first;"));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_AwaitAndLambdaLowering_MapsAwaitLambdaAndReturnLines()
    {
        const string sourcePath = "Demo/AwaitLambda.cs";
        const string source = """
            class TestClass
            {
                async Task<int> M(int[] values)
                {
                    Func<int, int> inc = x => x + 1;
                    var next = await Task.FromResult(inc(values.Length));
                    return next;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "await-lambda.mjs",
            includeSourcesContent: false);

        StringAssert.Contains(artifact.Content, "await");

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "AwaitLambda.cs");
        var segments = DecodeSegments(sourceMap);

        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "Func<int, int> inc = x => x + 1;"),
            GetLineIndexContaining(source, "var next = await Task.FromResult(inc(values.Length));"),
            GetLineIndexContaining(source, "return next;"));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_CreationInterpolationWithAndCollectionSpread_MapsEachStatementLine()
    {
        const string sourcePath = "Demo/CreationStringWithCollection.cs";
        const string source = """
            class TestClass
            {
                record Person(string Name, int Age);

                Person M(Person person, int[] values)
                {
                    var anon = new { Name = person.Name, Count = values.Length };
                    var arr = new[] { values.Length, anon.Count };
                    var msg = $"N={anon.Name},C={anon.Count}";
                    var clone = person with { Age = person.Age + 1 };
                    int[] numbers = [1, ..arr, 5];
                    return clone;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "creation-string-with-collection.mjs",
            includeSourcesContent: false);

        StringAssert.Contains(artifact.Content, "...");
        StringAssert.Contains(artifact.Content, "`");

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "CreationStringWithCollection.cs");
        var segments = DecodeSegments(sourceMap);

        AssertSourceLinesHaveSegments(
            segments,
            sourceIndex,
            GetLineIndexContaining(source, "var anon = new { Name = person.Name, Count = values.Length };"),
            GetLineIndexContaining(source, "var arr = new[] { values.Length, anon.Count };"),
            GetLineIndexContaining(source, "var msg = $\"N={anon.Name},C={anon.Count}\";"),
            GetLineIndexContaining(source, "var clone = person with { Age = person.Age + 1 };"),
            GetLineIndexContaining(source, "int[] numbers = [1, ..arr, 5];"),
            GetLineIndexContaining(source, "return clone;"));
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_PreservesKnROutputAndEmitsMap()
    {
        const string sourcePath = "Demo/SourceMapCase.cs";
        const string source = """
            class TestClass
            {
                int M(int p)
                {
                    var x = p + 1;
                    return x;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var plain = node.ToKnRECMAScript();
        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "demo.module.mjs",
            includeSourcesContent: true,
            readSourceContent: static _ => source);

        Assert.AreEqual(plain, artifact.Content);
        Assert.AreEqual(ComputeSha256Hex(artifact.Content), artifact.JsHash);
        Assert.AreEqual(ComputeSha256Hex(artifact.SourceMapContent!), artifact.MapHash);
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.SourceMapContent));

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;

        Assert.AreEqual(3, sourceMap.GetProperty("version").GetInt32());
        Assert.AreEqual("demo.module.mjs", sourceMap.GetProperty("file").GetString());
        Assert.IsTrue(sourceMap.GetProperty("sources").GetArrayLength() > 0);
        Assert.AreNotEqual(string.Empty, sourceMap.GetProperty("mappings").GetString());

        var sourceIndex = FindSourceIndexContaining(sourceMap, "SourceMapCase.cs");
        Assert.AreEqual(source, sourceMap.GetProperty("sourcesContent")[sourceIndex].GetString());

        var decoded = DecodeGeneratedLineToSourceLocation(sourceMap);
        Assert.IsTrue(decoded.Count > 0);
        AssertGeneratedLineMapsToSource(
            artifact.Content,
            "return x;",
            sourceMap,
            "SourceMapCase.cs",
            source,
            "return x;",
            decoded);
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_CanDisableSourcesContent()
    {
        const string sourcePath = "Demo/NoSourcesContent.cs";
        const string source = """
            class TestClass
            {
                int M(int p)
                {
                    return p + 1;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "no-content.mjs",
            includeSourcesContent: false,
            readSourceContent: static _ => source);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        Assert.IsFalse(sourceMap.TryGetProperty("sourcesContent", out _));
        Assert.IsTrue(sourceMap.GetProperty("sources").GetArrayLength() > 0);
        Assert.AreNotEqual(string.Empty, sourceMap.GetProperty("mappings").GetString());
    }

    [TestMethod]
    public void ToECMAScriptWithSourceMap_PreservesDefaultWriterOutput()
    {
        const string sourcePath = "Demo/DefaultWriter.cs";
        const string source = """
            class TestClass
            {
                int M(int p)
                {
                    var x = p + 2;
                    return x;
                }
            }
            """;

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var plain = node.ToECMAScript();
        var artifact = node.ToECMAScriptWithSourceMap(
            generatedFileName: "default-writer.js",
            includeSourcesContent: false);

        Assert.AreEqual(plain, artifact.Content);
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.SourceMapContent));

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        Assert.AreEqual("default-writer.js", sourceMap.GetProperty("file").GetString());
        Assert.AreNotEqual(string.Empty, sourceMap.GetProperty("mappings").GetString());
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_CRLFSource_MapsReturnLine()
    {
        const string sourcePath = "Demo/CrLfCase.cs";
        const string sourceLf = """
            class TestClass
            {
                int M(int p)
                {
                    var x = p + 1;
                    return x;
                }
            }
            """;
        var source = sourceLf.Replace("\n", "\r\n", StringComparison.Ordinal);

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "crlf.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "CrLfCase.cs");
        var decoded = DecodeGeneratedLineToSourceLocation(sourceMap);
        var generatedReturnLine = GetLineIndexContaining(artifact.Content, "return x;");
        var sourceReturnLine = GetLineIndexContaining(source, "return x;");

        Assert.IsTrue(
            decoded.TryGetValue(generatedReturnLine, out var mapped),
            "Expected generated return line to have source-map mapping.");
        Assert.AreEqual(sourceIndex, mapped.SourceIndex);
        Assert.AreEqual(sourceReturnLine, mapped.SourceLine);
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_LargeMethod_MapsTailStatementAndReturn()
    {
        const string sourcePath = "Demo/LargeMethod.cs";
        const int statementCount = 900;
        var source = BuildLargeMethodSource(statementCount);

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "large-method.mjs",
            includeSourcesContent: false);

        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "LargeMethod.cs");
        var segments = DecodeSegments(sourceMap);
        var tailSourceLine = GetLineIndexContaining(source, $"sum += {statementCount - 1};");
        var returnSourceLine = GetLineIndexContaining(source, "return sum;");

        AssertSourceLinesHaveSegments(segments, sourceIndex, tailSourceLine, returnSourceLine);

        var mappedLineCoverage = segments
            .Where(segment => segment.SourceIndex == sourceIndex)
            .Select(segment => segment.SourceLine)
            .Distinct()
            .Count();
        Assert.IsTrue(
            mappedLineCoverage > statementCount / 2,
            $"Expected broad source-map coverage for a large method body. Actual mapped source-line count: {mappedLineCoverage}.");
    }

    [TestMethod]
    public void ToKnRECMAScriptWithSourceMap_LargeMethod_PerformanceBaselineWithinBudget()
    {
        const string sourcePath = "Demo/LargeMethodPerformance.cs";
        const int statementCount = 2500;
        var source = BuildLargeMethodSource(statementCount);

        var block = GetBlockOperation(source, sourcePath);
        var node = new SemanticWalker(true).Visit(block, new());
        Assert.IsNotNull(node);

        _ = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "large-method-perf-warmup.mjs",
            includeSourcesContent: false);

        var stopwatch = Stopwatch.StartNew();
        var artifact = node.ToKnRECMAScriptWithSourceMap(
            generatedFileName: "large-method-perf.mjs",
            includeSourcesContent: false);
        stopwatch.Stop();

        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.SourceMapContent));
        using var parsed = JsonDocument.Parse(artifact.SourceMapContent!);
        var sourceMap = parsed.RootElement;
        var sourceIndex = FindSourceIndexContaining(sourceMap, "LargeMethodPerformance.cs");
        var segments = DecodeSegments(sourceMap);
        var tailSourceLine = GetLineIndexContaining(source, $"sum += {statementCount - 1};");
        var returnSourceLine = GetLineIndexContaining(source, "return sum;");
        AssertSourceLinesHaveSegments(segments, sourceIndex, tailSourceLine, returnSourceLine);

        var elapsedBudget = TimeSpan.FromSeconds(10);
        Assert.IsTrue(
            stopwatch.Elapsed <= elapsedBudget,
            $"SourceMap generation for {statementCount} statements exceeded budget {elapsedBudget.TotalMilliseconds}ms; actual {stopwatch.Elapsed.TotalMilliseconds:F2}ms.");
    }

    private static string BuildLargeMethodSource(int statementCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("class TestClass");
        builder.AppendLine("{");
        builder.AppendLine("    int M()");
        builder.AppendLine("    {");
        builder.AppendLine("        int sum = 0;");
        for (var index = 0; index < statementCount; index++)
            builder.AppendLine($"        sum += {index};");
        builder.AppendLine("        return sum;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static IBlockOperation GetBlockOperation(string code, string sourcePath, string methodName = "M")
    {
        var usings = """
            global using System;
            global using System.Collections.Generic;
            global using System.Linq;
            global using System.Numerics;
            global using System.Threading.Tasks;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var references = Basic.Reference.Assemblies.Net100.References.All
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, parseOptions, path: "__global_usings__.cs"),
                CSharpSyntaxTree.ParseText(code, parseOptions, path: sourcePath)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Join(
                Environment.NewLine,
                errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException($"Method '{methodName}' was not found or has no analyzable block body.");
    }

    private static string ComputeSha256Hex(string value)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("X2"));

        return builder.ToString();
    }

    private static void AssertSourceLinesHaveSegments(
        IReadOnlyList<SourceMapSegment> segments,
        int sourceIndex,
        params int[] sourceLines)
    {
        foreach (var sourceLine in sourceLines)
        {
            Assert.IsTrue(
                segments.Any(segment =>
                    segment.SourceIndex == sourceIndex &&
                    segment.SourceLine == sourceLine),
                $"Expected source-map segments to include source line {sourceLine}.");
        }
    }

    private static object CreateSourceOrigin(
        string sourcePath,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn,
        bool isSynthetic = false)
    {
        var sourceOriginType = typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.SourceOrigin", throwOnError: true)!;
        return Activator.CreateInstance(
            sourceOriginType,
            sourcePath,
            startLine,
            startColumn,
            endLine,
            endColumn,
            null,
            isSynthetic)!;
    }
}
