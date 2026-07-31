using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCreationAndIndexerProtocolTests
{
    [TestMethod]
    public void Visit_NestedPropertyMemberInitializer_UsesStructuredInitializerProtocol()
    {
        var block = GetBlockOperation("var holder = new Holder { Nested = { Value = 1 } };");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "new Holder");
        StringAssert.Contains(script, ".nested.value = 1");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_NestedFieldMemberInitializer_UsesStructuredInitializerProtocol()
    {
        var block = GetBlockOperation("var holder = new Holder { FieldNested = { Value = 2 } };");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "new Holder");
        StringAssert.Contains(script, ".fieldNested.value = 2");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_MemberInitializerProperty_ProducesObjectProperty()
    {
        var block = GetBlockOperation("var holder = new Holder { Nested = { Value = 3 } };");
        var operation = FindOperation<IMemberInitializerOperation>(block);

        var script = new SemanticWalker(true).VisitMemberInitializer(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual("nested: (v$0.value = 3)", script);
        _ = new Parser().ParseExpression($"({{{script}}})");
    }

    [TestMethod]
    public void Visit_MemberInitializerField_ProducesObjectProperty()
    {
        var block = GetBlockOperation("var holder = new Holder { FieldNested = { Value = 4 } };");
        var operation = FindOperation<IMemberInitializerOperation>(block);

        var script = new SemanticWalker(true).VisitMemberInitializer(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual("fieldNested: (v$0.value = 4)", script);
        _ = new Parser().ParseExpression($"({{{script}}})");
    }

    [TestMethod]
    public void Visit_NestedIndexerMemberInitializer_EvaluatesIndexOnce()
    {
        var block = GetBlockOperation("var holder = new IndexedHolder { [NextIndex()] = { Value = 5 } };");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "v$1 = v$0[TestClass.nextIndex()]");
        StringAssert.Contains(script, "v$1.value = 5");
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.nextIndex()"));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_IndexedObjectInitializer_EvaluatesIndexOnceAndCompletesValueBeforeAssignment()
    {
        var block = GetBlockOperation("var holder = new IndexedHolder { [NextIndex()] = new Nested { Value = 6 } };");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let holder = (() => {
                let v$0 = new IndexedHolder;
                v$0[TestClass.nextIndex()] = (() => {
                  let v$1 = new Nested;
                  v$1.value = 6;
                  return v$1;
                })();
                return v$0;
              })();
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.nextIndex()"));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_ListIndexedObjectInitializer_CompletesValueThroughMappedSetter()
    {
        var block = GetBlockOperation("""
            var values = new List<Nested>(new[] { new Nested() })
            {
                [NextIndex()] = new Nested { Value = 9 }
            };
            """);

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let values = (() => {
                let v$0 = Array.from([new Nested]);
                _c16a7960302ea054(v$0, TestClass.nextIndex(), (() => {
                  let v$1 = new Nested;
                  v$1.value = 9;
                  return v$1;
                })());
                return v$0;
              })();
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.nextIndex()"));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_MultiParameterIndexerMemberInitializer_RejectsUnrepresentableJavaScriptTarget()
    {
        var block = GetBlockOperation("var holder = new MatrixHolder { [0, 1] = { Value = 7 } };");

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "only supports a single translated index argument");
    }

    [TestMethod]
    public void Visit_MultiParameterIndexerAssignment_RejectsUnrepresentableJavaScriptTarget()
    {
        var block = GetBlockOperation(
            "var holder = new MatrixSetter { [0, 1] = new Nested { Value = 7 } };");

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Indexed initializer target could not be translated");
    }

    [TestMethod]
    public void Visit_DictionaryIndexerMemberInitializer_UsesMappedGetterAfterInsertion()
    {
        var block = GetBlockOperation("""
            var map = new System.Collections.Generic.Dictionary<string, Nested>
            {
                ["primary"] = new Nested(),
                ["primary"] = { Value = 8 }
            };
            """);

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let map = (() => {
                let v$1;
                let v$0 = new Map;
                v$0.set("primary", new Nested);
                v$1 = _e73dbdff85c46ddc(v$0, "primary");
                v$1.value = 8;
                return v$0;
              })();
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        Assert.AreEqual(1, CountOccurrences(script, "_e73dbdff85c46ddc(v$0, \"primary\")"));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_DictionaryMemberCollectionInitializer_UsesMappedAddInSourceOrder()
    {
        var block = GetBlockOperation("""
            var holder = new CollectionHolder
            {
                Map =
                {
                    { "one", 1 },
                    { "two", 2 }
                }
            };
            """);

        var script = VisitBlock(block);

        var firstAdd = "_39d6e632c4c102f9(v$0.map, \"one\", 1)";
        var secondAdd = "_39d6e632c4c102f9(v$0.map, \"two\", 2)";
        StringAssert.Contains(script, firstAdd);
        StringAssert.Contains(script, secondAdd);
        Assert.IsLessThan(script.IndexOf(secondAdd, StringComparison.Ordinal), script.IndexOf(firstAdd, StringComparison.Ordinal));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_SourceCollectionInitializer_CallsDeclaredAddInSourceOrder()
    {
        var block = GetBlockOperation("var bag = new SourceBag { 1, 2 }; ");

        var script = VisitBlock(block);

        var firstAdd = "v$0.add(1)";
        var secondAdd = "v$0.add(2)";
        StringAssert.Contains(script, firstAdd);
        StringAssert.Contains(script, secondAdd);
        Assert.IsLessThan(script.IndexOf(secondAdd, StringComparison.Ordinal), script.IndexOf(firstAdd, StringComparison.Ordinal));
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_StructuralRecordPrimaryConstructorSpreadLiteral_FlattensMembersInOrder()
    {
        var block = GetBlockOperation(
            "var payload = new SpreadEnvelope(\"x\", new ChildProps { Name = \"John\", Age = 30 });");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let payload = {
                prefix: "x",
                name: "John",
                age: 30
              };
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_StructuralRecordNestedPropertyInitializer_ProducesNestedObjectLiteral()
    {
        var block = GetBlockOperation(
            "var payload = new PropertyEnvelope { Child = { Name = \"John\", Age = 30 } };");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let payload = { child: { name: "John", age: 30 } };
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_StructuralRecordNestedFieldInitializer_ProducesNestedObjectLiteral()
    {
        var block = GetBlockOperation(
            "var payload = new FieldEnvelope { Child = { Name = \"John\", Age = 30 } };");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let payload = { child: { name: "John", age: 30 } };
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_StructuralRecordFieldAssignment_ProducesObjectLiteralMember()
    {
        var block = GetBlockOperation(
            "var payload = new FieldValueEnvelope { Label = \"ready\" }; ");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let payload = { label: "ready" };
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        ParseScript(script);
    }

    [TestMethod]
    public void Operation_ObjectLiteralInitializers_ExposeBoundReceiversAndParameters()
    {
        var block = GetBlockOperation(
            """
            var payload = new PropertyEnvelope { Child = { Name = "John", Age = 30 } };
            var attrs = new VueDictionary { ["role"] = "banner" };
            var entries = new VueDictionary { { "title", "hello" } };
            """);

        var memberInitializer = block.DescendantsAndSelf()
            .OfType<IMemberInitializerOperation>()
            .Single();
        var initializedMember = Assert.IsInstanceOfType<IMemberReferenceOperation>(memberInitializer.InitializedMember);
        Assert.IsNotNull(initializedMember.Instance);
        Assert.IsNotNull(initializedMember.Instance.Type);

        var dictionaryCreations = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Where(static creation => creation.Type?.Name == "VueDictionary")
            .ToArray();
        Assert.HasCount(2, dictionaryCreations);
        var indexerInitializer = Assert.IsInstanceOfType<IObjectOrCollectionInitializerOperation>(dictionaryCreations[0].Initializer);
        var indexerAssignment = indexerInitializer.Initializers
            .OfType<ISimpleAssignmentOperation>()
            .Single();
        var indexer = Assert.IsInstanceOfType<IPropertyReferenceOperation>(indexerAssignment.Target);
        Assert.IsNotNull(indexer.Instance);
        Assert.IsNotNull(indexer.Instance.Type);
        Assert.IsNotNull(indexer.Arguments[0].Parameter);

        var addInitializer = Assert.IsInstanceOfType<IObjectOrCollectionInitializerOperation>(dictionaryCreations[1].Initializer);
        var addInvocation = addInitializer.Initializers
            .OfType<IInvocationOperation>()
            .Single();
        Assert.IsNotNull(addInvocation.Instance);
        Assert.IsNotNull(addInvocation.Instance.Type);
        Assert.IsTrue(addInvocation.Arguments.All(static argument => argument.Parameter is not null));

        var script = VisitBlock(block);
        Assert.AreEqual(
            """
            {
              let payload = { child: { name: "John", age: 30 } };
              let attrs = { role: "banner" };
              let entries = { title: "hello" };
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_MultiDimensionalArrayAllocation_AllocatesIndependentNestedArrays()
    {
        var block = GetBlockOperation("var grid = new int[2, 3];");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "new Array(2).fill().map(() => new Array(3))");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_ThreeDimensionalArrayAllocation_RecursivelyAllocatesIndependentNestedArrays()
    {
        var block = GetBlockOperation("var cube = new int[2, 3, 4];");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "new Array(2).fill().map(() => new Array(3).fill().map(() => new Array(4)))");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_CustomIndexerFromEndRead_UsesLengthAndSingleIndexerAccess()
    {
        var block = GetBlockOperation("var buffer = new Buffer(); var last = buffer[^1];");

        var script = VisitBlock(block);

        StringAssert.Contains(script, "buffer.length - 1");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_CustomIndexerFromEndCompoundAssignment_CachesReadWriteTarget()
    {
        var block = GetBlockOperation("var buffer = new Buffer(); buffer[^1] += 2;");

        var script = VisitBlock(block);

        StringAssert.Contains(
            script,
            "v$0 = buffer.length - 1, v$1 = buffer[v$0] + 2, buffer[v$0] = v$1, v$1");
        ParseScript(script);
    }

    [TestMethod]
    public void Visit_CustomIndexerRangeRead_RejectsUnsupportedRangeIndexer()
    {
        var block = GetBlockOperation("var buffer = new Buffer(); var tail = buffer[1..^1];");

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Range-based indexer");
        StringAssert.Contains(exception.Message, "Expose an int-based slice member");
    }

    private static string VisitBlock(IBlockOperation block)
    {
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        return first;
    }

    private static void ParseScript(string script)
        => _ = new Parser().ParseScript(script);

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }

        return count;
    }

    private static TOperation FindOperation<TOperation>(IOperation root)
        where TOperation : class, IOperation
        => root.DescendantsAndSelf().OfType<TOperation>().First();

    private static IBlockOperation GetBlockOperation(string body)
    {
        var source = $$"""
            using System;
            using System.Collections.Generic;
            using System.ComponentModel;
            using static ECMAScript.Vue3;

            public sealed class TestClass
            {
                public sealed class Nested
                {
                    public int Value { get; set; }
                }

                public sealed class Holder
                {
                    public Nested Nested { get; } = new();
                    public Nested FieldNested = new();
                }

                public sealed class IndexedHolder
                {
                    private readonly Nested[] values = [new(), new()];

                    public Nested this[int index]
                    {
                        get => values[index];
                        set => values[index] = value;
                    }
                }

                public sealed class MatrixHolder
                {
                    public Nested this[int row, int column] => new();
                }

                public sealed class MatrixSetter
                {
                    public Nested this[int row, int column]
                    {
                        set { }
                    }
                }

                public sealed class CollectionHolder
                {
                    public Dictionary<string, int> Map { get; } = new();
                }

                public sealed class SourceBag : IEnumerable<int>
                {
                    public void Add(int value) { }

                    public IEnumerator<int> GetEnumerator()
                        => ((IEnumerable<int>)Array.Empty<int>()).GetEnumerator();

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => GetEnumerator();
                }

                public sealed record ChildProps
                {
                    [Description("@#name")]
                    public string? Name { get; set; }

                    [Description("@#age")]
                    public int Age { get; set; }
                }

                public sealed record SpreadEnvelope(
                    [property: Description("@#prefix")] string Prefix,
                    [property: ECMAScript.Spread] ChildProps Child);

                public sealed record PropertyEnvelope
                {
                    [Description("@#child")]
                    public ChildProps Child { get; init; } = new();
                }

                public sealed record FieldEnvelope
                {
                    public ChildProps Child = new();
                }

                public sealed record FieldValueEnvelope
                {
                    public string? Label;
                }

                public sealed class Buffer
                {
                    public int Length => 3;

                    public int this[int index]
                    {
                        get => index;
                        set { }
                    }

                    public int[] this[Range range] => [];
                }

                private static int NextIndex() => 0;

                public void TestMethod()
                {
                    {{body}}
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "CreationAndIndexerProtocolScenarios",
            syntaxTrees: [syntaxTree],
            references:
            [
                .. TestMetadataReferences.Net11,
                MetadataReference.CreateFromFile(typeof(ECMAScript.SpreadAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
