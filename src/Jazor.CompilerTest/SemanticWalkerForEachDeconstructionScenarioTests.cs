using Acornima;
using Acornima.Ast;
using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerForEachDeconstructionScenarioTests
{
    public static IEnumerable<TestDataRow<ForEachDeconstructionScenario>> Cases
        => ForEachDeconstructionScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<ForEachDeconstructionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = ForEachDeconstructionScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("semantic.foreach-deconstruction.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(cases.All(static testCase =>
            (testCase.ExpectedPattern is null) != (testCase.ExpectedErrorFragment is null)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void VisitForEachLoop_UsesSourceRuntimeShape(ForEachDeconstructionScenario testCase)
    {
        var operation = GetForEachOperation(testCase);
        var walker = new SemanticWalker(true);

        if (testCase.ExpectedErrorFragment is not null)
        {
            var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
                walker.VisitForEachLoop(operation, new SenseArgument()));
            StringAssert.Contains(exception.Message, testCase.ExpectedErrorFragment, testCase.Id);
            return;
        }

        var node = walker.VisitForEachLoop(operation, new SenseArgument());
        Assert.IsInstanceOfType<ForOfStatement>(node, testCase.Id);
        var forOf = (ForOfStatement)node!;
        Assert.AreEqual(testCase.IsAsynchronous, forOf.Await, testCase.Id);

        Assert.IsInstanceOfType<VariableDeclaration>(forOf.Left, testCase.Id);
        var declaration = (VariableDeclaration)forOf.Left;
        Assert.HasCount(1, declaration.Declarations, testCase.Id);
        AssertPattern(declaration.Declarations[0].Id, testCase.ExpectedPattern!, testCase.Id);

        var script = forOf.ToKnRECMAScript();
        if (testCase.IsAsynchronous)
            _ = new Parser().ParseModule(script);
        else
            _ = new Parser().ParseScript(script);
    }

    private static void AssertPattern(Node node, ForEachPatternSpec expected, string scenarioId)
    {
        switch (expected.Kind)
        {
            case ForEachPatternKind.Object:
                AssertObjectPattern(node, expected, scenarioId);
                break;
            case ForEachPatternKind.Array:
                AssertArrayPattern(node, expected, scenarioId);
                break;
            default:
                Assert.Fail($"{scenarioId}: unsupported pattern kind '{expected.Kind}'.");
                break;
        }
    }

    private static void AssertObjectPattern(Node node, ForEachPatternSpec expected, string scenarioId)
    {
        Assert.IsInstanceOfType<ObjectPattern>(node, scenarioId);
        var pattern = (ObjectPattern)node;
        Assert.HasCount(expected.Entries.Count, pattern.Properties, scenarioId);

        for (var index = 0; index < expected.Entries.Count; index++)
        {
            var expectedEntry = expected.Entries[index];
            Assert.IsFalse(expectedEntry.IsDiscard, scenarioId);
            Assert.IsInstanceOfType<AssignmentProperty>(pattern.Properties[index], scenarioId);
            var property = (AssignmentProperty)pattern.Properties[index];
            Assert.AreEqual(expectedEntry.SourceName, GetPropertyName(property.Key), scenarioId);
            AssertPatternValue(property.Value, expectedEntry, scenarioId);
        }
    }

    private static void AssertArrayPattern(Node node, ForEachPatternSpec expected, string scenarioId)
    {
        Assert.IsInstanceOfType<ArrayPattern>(node, scenarioId);
        var pattern = (ArrayPattern)node;
        Assert.HasCount(expected.Entries.Count, pattern.Elements, scenarioId);

        for (var index = 0; index < expected.Entries.Count; index++)
        {
            var expectedEntry = expected.Entries[index];
            var element = pattern.Elements[index];
            if (expectedEntry.IsDiscard)
            {
                Assert.IsNull(element, scenarioId);
                continue;
            }

            Assert.IsNotNull(element, scenarioId);
            AssertPatternValue(element, expectedEntry, scenarioId);
        }
    }

    private static void AssertPatternValue(Node node, ForEachPatternEntry expected, string scenarioId)
    {
        if (expected.Nested is not null)
        {
            AssertPattern(node, expected.Nested, scenarioId);
            return;
        }

        Assert.IsInstanceOfType<Identifier>(node, scenarioId);
        Assert.AreEqual(expected.TargetName, ((Identifier)node).Name, scenarioId);
    }

    private static string GetPropertyName(Expression key)
        => key switch
        {
            Identifier identifier => identifier.Name,
            StringLiteral literal => literal.Value,
            _ => throw new AssertFailedException($"Unexpected object-pattern key node '{key.Type}'.")
        };

    private static IForEachLoopOperation GetForEachOperation(ForEachDeconstructionScenario testCase)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            testCase.Source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{testCase.Id}.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalker.ForEachDeconstruction.Tests",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{testCase.Id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))}");

        var syntax = syntaxTree.GetRoot().DescendantNodes().OfType<CommonForEachStatementSyntax>().Single();
        var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(syntax);
        Assert.IsInstanceOfType<IForEachLoopOperation>(operation, testCase.Id);
        return (IForEachLoopOperation)operation!;
    }
}

public enum ForEachPatternKind
{
    Object,
    Array
}

public sealed record ForEachPatternEntry(
    string? SourceName,
    string? TargetName,
    ForEachPatternSpec? Nested,
    bool IsDiscard);

public sealed record ForEachPatternSpec(
    ForEachPatternKind Kind,
    IReadOnlyList<ForEachPatternEntry> Entries);

public sealed record ForEachDeconstructionScenario(
    string Id,
    string Dimension,
    string Source,
    ForEachPatternSpec? ExpectedPattern,
    bool IsAsynchronous,
    string? ExpectedErrorFragment);

internal static class ForEachDeconstructionScenarioCatalog
{
    public static IReadOnlyList<ForEachDeconstructionScenario> All { get; } =
    [
        Success(
            "named-tuple-remap",
            "source-tuple-names-drive-object-pattern",
            """
            class TestClass
            {
                void TestMethod((int SourceId, string Label)[] entries)
                {
                    foreach (var (key, value) in entries) { }
                }
            }
            """,
            Object(Property("SourceId", "key"), Property("Label", "value"))),
        Success(
            "unnamed-tuple",
            "default-tuple-slot-names",
            """
            class TestClass
            {
                void TestMethod(System.Collections.Generic.IEnumerable<(int, string)> entries)
                {
                    foreach (var (number, text) in entries) { }
                }
            }
            """,
            Object(Property("Item1", "number"), Property("Item2", "text"))),
        Success(
            "tuple-discard",
            "discarded-object-slot-is-omitted",
            """
            class TestClass
            {
                void TestMethod((int SourceId, string Label, bool Enabled)[] entries)
                {
                    foreach (var (id, _, active) in entries) { }
                }
            }
            """,
            Object(Property("SourceId", "id"), Property("Enabled", "active"))),
        Success(
            "nested-tuple",
            "nested-source-shapes-remap-recursively",
            """
            class TestClass
            {
                void TestMethod(((int Latitude, int Longitude) Coordinates, string Name)[] entries)
                {
                    foreach (var ((lat, lng), label) in entries) { }
                }
            }
            """,
            Object(
                NestedProperty("Coordinates", Object(Property("Latitude", "lat"), Property("Longitude", "lng"))),
                Property("Name", "label"))),
        Success(
            "nested-tuple-discards",
            "nested-discard-prunes-only-targeted-slots",
            """
            class TestClass
            {
                void TestMethod(((int Latitude, int Longitude) Coordinates, string Name)[] entries)
                {
                    foreach (var ((lat, _), _) in entries) { }
                }
            }
            """,
            Object(NestedProperty("Coordinates", Object(Property("Latitude", "lat"))))),
        Success(
            "explicit-target-types",
            "explicitly-typed-deconstruction-target",
            """
            class TestClass
            {
                void TestMethod((int SourceId, string Label)[] entries)
                {
                    foreach ((int id, string text) in entries) { }
                }
            }
            """,
            Object(Property("SourceId", "id"), Property("Label", "text"))),
        Success(
            "positional-record",
            "record-constructor-property-shape",
            """
            public sealed record Entry(int SourceId, string Label);

            class TestClass
            {
                void TestMethod(Entry[] entries)
                {
                    foreach (var (id, text) in entries) { }
                }
            }
            """,
            Object(Property("SourceId", "id"), Property("Label", "text"))),
        Success(
            "record-with-nested-tuple",
            "record-property-and-nested-tuple-shapes",
            """
            public sealed record Entry((int X, int Y) Position, string Label);

            class TestClass
            {
                void TestMethod(Entry[] entries)
                {
                    foreach (var ((left, top), text) in entries) { }
                }
            }
            """,
            Object(
                NestedProperty("Position", Object(Property("X", "left"), Property("Y", "top"))),
                Property("Label", "text"))),
        Success(
            "async-named-tuple",
            "await-foreach-preserves-source-shape",
            """
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Collections.Generic.IAsyncEnumerable<(int SourceId, string Label)> entries)
                {
                    await foreach (var (id, text) in entries) { }
                }
            }
            """,
            Object(Property("SourceId", "id"), Property("Label", "text")),
            isAsynchronous: true),
        Success(
            "dictionary-entry",
            "map-entry-array-pattern",
            """
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<string, int> entries)
                {
                    foreach (var (key, value) in entries) { }
                }
            }
            """,
            Array(Item("key"), Item("value"))),
        Success(
            "dictionary-entry-discard",
            "map-entry-array-pattern-discard",
            """
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<string, int> entries)
                {
                    foreach (var (key, _) in entries) { }
                }
            }
            """,
            Array(Item("key"), Discard())),
        Success(
            "dictionary-nested-tuple-value",
            "map-entry-with-nested-tuple-value",
            """
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<string, (int Count, bool Enabled)> entries)
                {
                    foreach (var (key, (count, active)) in entries) { }
                }
            }
            """,
            Array(
                Item("key"),
                NestedItem(Object(Property("Count", "count"), Property("Enabled", "active"))))),
        Success(
            "tuple-with-nested-key-value-pair",
            "tuple-slot-with-map-entry-array-pattern",
            """
            class TestClass
            {
                void TestMethod(
                    (System.Collections.Generic.KeyValuePair<string, int> Pair, bool Enabled)[] entries)
                {
                    foreach (var ((key, value), active) in entries) { }
                }
            }
            """,
            Object(
                NestedProperty("Pair", Array(Item("key"), Item("value"))),
                Property("Enabled", "active"))),
        Success(
            "record-with-nested-key-value-pair",
            "record-slot-with-map-entry-array-pattern",
            """
            public sealed record Entry(
                System.Collections.Generic.KeyValuePair<string, int> Pair,
                string Label);

            class TestClass
            {
                void TestMethod(Entry[] entries)
                {
                    foreach (var ((key, value), text) in entries) { }
                }
            }
            """,
            Object(
                NestedProperty("Pair", Array(Item("key"), Item("value"))),
                Property("Label", "text"))),
        Success(
            "deep-record-tuple-key-value-pair",
            "recursive-record-tuple-map-entry-shapes-with-discards",
            """
            public sealed record Entry(
                (System.Collections.Generic.KeyValuePair<string, (int Count, bool Enabled)> Pair, int Revision) Data,
                string Label);

            class TestClass
            {
                void TestMethod(Entry[] entries)
                {
                    foreach (var (((key, (count, _)), revision), text) in entries) { }
                }
            }
            """,
            Object(
                NestedProperty(
                    "Data",
                    Object(
                        NestedProperty(
                            "Pair",
                            Array(
                                Item("key"),
                                NestedItem(Object(Property("Count", "count"))))),
                        Property("Revision", "revision"))),
                Property("Label", "text"))),
        Failure(
            "custom-class-deconstruct",
            "unproven-custom-runtime-shape-is-rejected",
            """
            public sealed class Entry
            {
                public int SourceId { get; init; }
                public string Label { get; init; } = string.Empty;

                public void Deconstruct(out int sourceId, out string label)
                {
                    sourceId = SourceId;
                    label = Label;
                }
            }

            class TestClass
            {
                void TestMethod(Entry[] entries)
                {
                    foreach (var (id, text) in entries) { }
                }
            }
            """,
            "does not have a compiler-known structural runtime shape"),
        Failure(
            "nested-custom-deconstruct",
            "unproven-nested-runtime-shape-is-rejected",
            """
            public sealed class Value
            {
                public int Left { get; init; }
                public int Right { get; init; }

                public void Deconstruct(out int left, out int right)
                {
                    left = Left;
                    right = Right;
                }
            }

            class TestClass
            {
                void TestMethod((Value Value, int Id)[] entries)
                {
                    foreach (var ((left, right), id) in entries) { }
                }
            }
            """,
            "Nested for-each deconstruction slot 0")
    ];

    private static ForEachDeconstructionScenario Success(
        string id,
        string dimension,
        string source,
        ForEachPatternSpec expectedPattern,
        bool isAsynchronous = false)
        => new(
            $"semantic.foreach-deconstruction.{id}",
            dimension,
            source,
            expectedPattern,
            isAsynchronous,
            null);

    private static ForEachDeconstructionScenario Failure(
        string id,
        string dimension,
        string source,
        string expectedErrorFragment)
        => new(
            $"semantic.foreach-deconstruction.{id}",
            dimension,
            source,
            null,
            false,
            expectedErrorFragment);

    private static ForEachPatternSpec Object(params ForEachPatternEntry[] entries)
        => new(ForEachPatternKind.Object, entries);

    private static ForEachPatternSpec Array(params ForEachPatternEntry[] entries)
        => new(ForEachPatternKind.Array, entries);

    private static ForEachPatternEntry Property(string sourceName, string targetName)
        => new(sourceName, targetName, null, false);

    private static ForEachPatternEntry NestedProperty(string sourceName, ForEachPatternSpec nested)
        => new(sourceName, null, nested, false);

    private static ForEachPatternEntry Item(string targetName)
        => new(null, targetName, null, false);

    private static ForEachPatternEntry NestedItem(ForEachPatternSpec nested)
        => new(null, null, nested, false);

    private static ForEachPatternEntry Discard()
        => new(null, null, null, true);
}
