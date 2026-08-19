using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerTraditionalCoverageBehaviorTests
{
    [TestMethod]
    public void Visit_EnumerableMaterialization_DistinguishesBoundIntrinsicFromSameNamedSourceMethods()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public static class Mimic
            {
                public static int[] ToArray(IEnumerable<int> values) => [];
                public static int[] ToArray(IEnumerable<int> values, int count) => [];
                public static int[] ToArray(int value) => [];
            }

            public sealed class TestClass
            {
                static int[] TestMethod(IEnumerable<int> values, List<int> list, int[] array, int count)
                {
                    var enumerableArray = values.ToArray();
                    var enumerableList = Enumerable.ToList(values);
                    var listArray = Enumerable.ToArray(list);
                    var arrayList = Enumerable.ToList(array);
                    var sameSignature = Mimic.ToArray(values);
                    var extraParameter = Mimic.ToArray(values, count);
                    var differentParameter = Mimic.ToArray(count);
                    return enumerableArray;
                }
            }
            """);

        StringAssert.Contains(script, "Array.from", StringComparison.Ordinal);
        StringAssert.Contains(script, "MarkAsMutableListCarrier", StringComparison.Ordinal);
        StringAssert.Contains(script, "Mimic.ToArray(values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "Mimic.ToArray(values, count)", StringComparison.Ordinal);
        StringAssert.Contains(script, "Mimic.ToArray(count)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CompoundAssignments_PreservesSingleEvaluationForCustomOperatorsAndIndexedTargets()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Quantity
            {
                public int Value;

                public static Quantity operator +(Quantity left, Quantity right) => left;

                public Quantity this[int index]
                {
                    get => this;
                    set => Value = value.Value + index;
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Flag
            {
                public static bool operator true(Flag value) => true;
                public static bool operator false(Flag value) => false;
            }

            public sealed class TestClass
            {
                private static Quantity GetQuantity() => new();
                private static Quantity GetDelta() => new();
                private static int GetIndex() => 1;

                static void TestMethod(Quantity quantity, int index, Flag flag)
                {
                    quantity += GetDelta();
                    GetQuantity().Value += GetDelta().Value;
                    quantity[GetIndex()] += GetDelta();
                    quantity[index] += GetDelta();
                    var truth = flag ? 1 : 0;
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.GetQuantity()"));
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.GetIndex()"));
        Assert.AreEqual(4, CountOccurrences(script, "TestClass.GetDelta()"));
        StringAssert.Contains(script, "+", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CreationInitializers_HandlesNestedArrayElementsAndNativeErrorConstructors()
    {
        var script = VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Leaf
            {
                public int Value;
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                public Leaf[] Nodes = [new Leaf(), new Leaf()];
                public Leaf Child = new();
            }

            public sealed class TestClass
            {
                static object TestMethod()
                {
                    var holder = new Holder
                    {
                        Nodes = { [0] = { Value = 2 } },
                        Child = { Value = 3 }
                    };
                    var stable = new Holder { Nodes = { [1] = { Value = 4 } } };
                    var error = new InvalidOperationException("failed");
                    return holder.Nodes[0].Value + stable.Nodes[1].Value + error.Message.Length;
                }
            }
            """);

        StringAssert.Contains(script, "Nodes[0].Value = 2", StringComparison.Ordinal);
        StringAssert.Contains(script, "Nodes[1].Value = 4", StringComparison.Ordinal);
        StringAssert.Contains(script, "new Error", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ListAndTypePatterns_HandlesCustomSliceShapesAndAssignableInterfaces()
    {
        var script = VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Buffer : IDisposable
            {
                public int Length => 4;
                public int this[int index] => index + 1;
                public int[] Slice(int start, int length) => [];
                public void Dispose() { }
            }

            public sealed class TestClass
            {
                static bool TestMethod(Buffer buffer, object value)
                {
                    var full = buffer is [1, 2, 3, 4];
                    var head = buffer is [.., 4];
                    var tail = buffer is [1, ..];
                    var middle = buffer is [1, .. var values, 4];
                    var interfaceMatch = buffer is IDisposable;
                    var objectMatch = value is object;
                    return full || head || tail || (middle && interfaceMatch && objectMatch);
                }
            }
            """);

        StringAssert.Contains(script, "Length", StringComparison.Ordinal);
        StringAssert.Contains(script, "Slice", StringComparison.Ordinal);
        StringAssert.Contains(script, "!= null", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_NestedTupleDeconstruction_UsesDeclaredAndMemberWriteTargets()
    {
        var script = VisitBlock(
            """
            public sealed class Packet
            {
                public void Deconstruct(out (int Left, int Right) pair, out int code)
                {
                    pair = (1, 2);
                    code = 3;
                }
            }

            public sealed class TestClass
            {
                private int stored;

                static int TestMethod(Packet packet)
                {
                    int left = 0;
                    int right = 0;
                    int code = 0;
                    ((left, right), code) = packet;
                    return left + right + code;
                }
            }
            """);

        StringAssert.Contains(script, "packet.Deconstruct", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "left =", StringComparison.Ordinal);
        StringAssert.Contains(script, "right =", StringComparison.Ordinal);
        StringAssert.Contains(script, "code =", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_TraditionalMutationAndPatternForms_PreservesReceiverAndPatternEvaluation()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Node
            {
                public int Value { get; set; }
                public Node Child { get; } = new();
                public Node[] Nodes { get; } = [new()];
                public int this[int index]
                {
                    get => index;
                    set => Value = value + index;
                }
            }

            public sealed class TestClass
            {
                private static int nextIndex;
                private static Node GetNode() => new();
                private static int GetIndex() => nextIndex++;

                void TestMethod(Node node, int index, object value)
                {
                    node.Value += 1;
                    node[0] += 1;
                    node[GetIndex()] += 1;
                    node.Child.Value += 1;
                    node.Child[0] += 1;
                    GetNode().Value += 1;
                    GetNode()[GetIndex()] += 1;

                    var direct = GetNode().Value is > 0;
                    var indexed = GetNode()[0] is > 0;
                    var computed = GetNode()[GetIndex()] is > 0;
                    var stableComputed = node.Nodes[index].Value is > 0;
                    var nested = value is Node { Child: { Value: > 0 } };
                    var combined = value is Node and not null;
                    _ = (direct, indexed, computed, stableComputed, nested, combined);
                }
            }
            """);

        StringAssert.Contains(script, "GetNode()", StringComparison.Ordinal);
        StringAssert.Contains(script, "GetIndex()", StringComparison.Ordinal);
        StringAssert.Contains(script, "instanceof Node", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TraditionalCustomOperatorFieldTargets_CachesOnlyNonDuplicableReceivers()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Quantity
            {
                public int Value;
                public static Quantity operator +(Quantity left, Quantity right) => left;
            }

            [ECMAScript.ECMAScript]
            public sealed class Box
            {
                public Quantity Amount = new();
                public Box Child = new();
                public Quantity[] Items = [new()];
            }

            public sealed class TestClass
            {
                private static Box GetBox() => new();
                private static Quantity GetQuantity() => new();
                private static int GetIndex() => 0;

                void TestMethod(Box box)
                {
                    box.Amount += GetQuantity();
                    box.Child.Amount += GetQuantity();
                    box.Items[0] += GetQuantity();
                    GetBox().Amount += GetQuantity();
                    GetBox().Child.Amount += GetQuantity();
                    GetBox().Items[GetIndex()] += GetQuantity();
                }
            }
            """);

        Assert.IsGreaterThan(1, CountOccurrences(script, "GetBox()"));
        StringAssert.Contains(script, "GetIndex()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TraditionalTupleForms_HandlesDiscardsNestedTargetsAndEffectfulSources()
    {
        var script = VisitBlock(
            """
            public sealed class Pair
            {
                public void Deconstruct(out int first, out (int Left, int Right) nested)
                {
                    first = 1;
                    nested = (2, 3);
                }
            }

            public sealed class TestClass
            {
                private int stored;
                private static Pair GetPair() => new();
                private static (int Left, int Right) GetTuple() => (4, 5);
                private static (int Left, int Right) GetSimplePair() => (9, 10);

                void TestMethod(Pair pair)
                {
                    var (first, (left, right)) = pair;
                    (stored, (left, right)) = GetPair();
                    var (discarded, _) = GetSimplePair();
                    (left, right) = GetTuple();
                    var ((outerLeft, outerRight), code) = GetTupleWithCode();
                    (outerLeft, outerRight) = (right, left);
                    _ = (first, discarded, left, right, code, outerLeft, outerRight);
                }

                private static ((int Left, int Right) Pair, int Code) GetTupleWithCode()
                    => ((6, 7), 8);
            }
            """);

        StringAssert.Contains(script, "GetPair()", StringComparison.Ordinal);
        StringAssert.Contains(script, "GetTupleWithCode()", StringComparison.Ordinal);
        StringAssert.Contains(script, "Deconstruct", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TraditionalEnumerableGuardFailures_RejectWrongIntrinsicShapesExplicitly()
    {
        const string wrongArity =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class FakeEnumerable
            {
                public static int[] ToArray(IEnumerable<int> values, int count) => [];
            }

            public sealed class TestClass
            {
                void TestMethod(IEnumerable<int> values)
                {
                    var result = FakeEnumerable.ToArray(values, 1);
                }
            }
            """;

        const string wrongParameter =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class FakeEnumerable
            {
                public static int[] ToArray(IList<int> values) => [];
            }

            public sealed class TestClass
            {
                void TestMethod(IList<int> values)
                {
                    var result = FakeEnumerable.ToArray(values);
                }
            }
            """;

        const string wrongOwner =
            """
            using System.Collections.Generic;

            public static class FakeEnumerable
            {
                public static int[] ToArray(IEnumerable<int> values) => [];
            }

            public sealed class TestClass
            {
                void TestMethod(IEnumerable<int> values)
                {
                    var result = FakeEnumerable.ToArray(values);
                }
            }
            """;

        Assert.Throws<OperationTransformationException>(() => VisitBlock(wrongArity));
        Assert.Throws<OperationTransformationException>(() => VisitBlock(wrongParameter));
        Assert.Throws<OperationTransformationException>(() => VisitBlock(wrongOwner));
    }

    [TestMethod]
    public void Visit_TraditionalExceptionAndGenericCreationShapes_UsesExplicitRuntimeContract()
    {
        var errorScript = VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Error : Exception
            {
                public Error(string message) : base(message) { }
            }

            public sealed class TestClass
            {
                static Exception TestMethod()
                {
                    var error = new Error("failed");
                    return error;
                }
            }
            """);

        StringAssert.Contains(errorScript, "new Error", StringComparison.Ordinal);

        var genericFailure = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            public sealed class TestClass
            {
                static T TestMethod<T>() where T : new()
                {
                    return new T();
                }
            }
            """));

        StringAssert.Contains(genericFailure.Message, "new T()", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_TraditionalCreationAndPatternBoundaries_CoversSideEffectfulInitializersAndDefaultArms()
    {
        var script = VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Leaf
            {
                public int Value;
            }

            [ECMAScript.ECMAScript]
            public sealed class ChildHolder
            {
                public Leaf[] Nodes = [new Leaf(), new Leaf()];
                public Leaf Leaf = new();
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                public ChildHolder Child = new();
            }

            public sealed class TestClass
            {
                private static int nextIndex;

                private static int NextIndex() => nextIndex++;

                static int TestMethod(int value)
                {
                    var holder = new Holder
                    {
                        Child =
                        {
                            Nodes =
                            {
                                [^1] = { Value = 2 },
                                [NextIndex()] = { Value = 4 }
                            },
                            Leaf = { Value = 3 }
                        }
                    };
                    var selected = value switch
                    {
                        _ => holder.Child.Nodes[0].Value + holder.Child.Leaf.Value
                    };
                    return selected;
                }
            }
            """);

        StringAssert.Contains(script, "NextIndex()", StringComparison.Ordinal);
        StringAssert.Contains(script, "=>", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TraditionalPatternSwitchBranches_ReportsUnsupportedContinueAndLabeledBreak()
    {
            const string continueSource =
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Entry
            {
                public int Count { get; set; }
            }

            public sealed class TestClass
            {
                static void TestMethod(Entry value)
                {
                    while (true)
                    {
                        switch (value)
                        {
                            case Entry { Count: > 0 }:
                                continue;
                            default:
                                break;
                        }
                        break;
                    }
                }
            }
            """;

        const string labeledSource =
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Entry
            {
                public int Count { get; set; }
            }

            public sealed class TestClass
            {
                static void TestMethod(Entry value)
                {
                    outer:
                    while (true)
                    {
                        switch (value)
                        {
                            case Entry { Count: > 0 }:
                                break outer;
                            default:
                                break;
                        }
                        break;
                    }
                }
            }
            """;

        var continueFailure = Assert.Throws<OperationTransformationException>(() => VisitBlock(continueSource));
        StringAssert.Contains(continueFailure.Message, "Continue statements inside pattern-matching switch", StringComparison.Ordinal);

        var labeledFailure = Assert.Throws<OperationTransformationException>(() => VisitBlock(labeledSource));
        StringAssert.Contains(labeledFailure.Message, "labeled", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void Visit_TraditionalTupleTargets_WritesPropertiesParametersAndStaticFields()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public static class Shared
            {
                public static int Value { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Box
            {
                public int Value { get; set; }
            }

            public sealed class TestClass
            {
                static void TestMethod(Box box, int parameter)
                {
                    (box.Value, parameter) = (1, 2);
                    (Shared.Value, box.Value) = (3, 4);
                    _ = (parameter, Shared.Value, box.Value);
                }
            }
            """);

        StringAssert.Contains(script, "box.Value", StringComparison.Ordinal);
        StringAssert.Contains(script, "Shared.Value", StringComparison.Ordinal);
        StringAssert.Contains(script, "parameter", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_EmptyRecursivePatterns_PreserveNullableAndReferenceNonNullContracts()
    {
        var script = VisitBlock(
            """
            public sealed class TestClass
            {
                static bool TestMethod(int number, int? optional, string? text, object? value)
                {
                    var exactValue = number is { };
                    var nullableValue = optional is { };
                    var textValue = text is { };
                    var objectValue = value is { };
                    return exactValue || nullableValue || textValue || objectValue;
                }
            }
            """);

        StringAssert.Contains(script, "!= null", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_PatternSwitchWithoutDefault_PreservesTheNoMatchFallthrough()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Entry
            {
                public int Count { get; set; }
            }

            public sealed class TestClass
            {
                static int TestMethod(Entry value)
                {
                    switch (value)
                    {
                        case Entry { Count: > 0 }:
                            return 1;
                    }

                    return 0;
                }
            }
            """);

        StringAssert.Contains(script, "=>", StringComparison.Ordinal);
        StringAssert.Contains(script, "return 0", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_UnprovableErasedInterfaceCheck_ReportsTheAuthoringBoundary()
    {
        var failure = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            using System;

            public sealed class TestClass
            {
                static bool TestMethod(object value)
                {
                    return value is IDisposable;
                }
            }
            """));

        StringAssert.Contains(failure.Message, "cannot be statically proven assignable", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ErasedInterfacePatterns_HandleNullableAndNegatedAuthoringForms()
    {
        var script = VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Resource : IDisposable
            {
                public void Dispose() { }
            }

            public sealed class TestClass
            {
                static bool TestMethod(Resource? value)
                {
                    var nullableMatch = value is IDisposable;
                    var negatedMatch = new Resource() is not IDisposable;
                    return nullableMatch || negatedMatch;
                }
            }
            """);

        StringAssert.Contains(script, "!= null", StringComparison.Ordinal);
        StringAssert.Contains(script, "new Resource", StringComparison.Ordinal);
        StringAssert.Contains(script, "true", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ReassignedErasedInterfacePattern_ReportsTheUnprovableBoundary()
    {
        var failure = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            using System;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Resource : IDisposable
            {
                public void Dispose() { }
            }

            public sealed class TestClass
            {
                static bool TestMethod()
                {
                    object value = new Resource();
                    value = new Resource();
                    return value is IDisposable;
                }
            }
            """));

        StringAssert.Contains(failure.Message, "cannot be statically proven assignable", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_TraditionalTupleTargets_WritesInstanceAndStaticFields()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public static class Shared
            {
                public static int Value;
            }

            [ECMAScript.ECMAScript]
            public sealed class Box
            {
                public int Value;
            }

            public sealed class TestClass
            {
                static void TestMethod(Box box)
                {
                    (box.Value, Shared.Value) = (1, 2);
                    _ = (box.Value, Shared.Value);
                }
            }
            """);

        StringAssert.Contains(script, "box.Value", StringComparison.Ordinal);
        StringAssert.Contains(script, "Shared.Value", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ListPatterns_HandlesEmptyAndAllSlicePositions()
    {
        var script = VisitBlock(
            """
            public sealed class TestClass
            {
                static bool TestMethod(int[] values, string? text)
                {
                    var empty = values is [];
                    var onlySlice = values is [.. var allValues];
                    var leading = values is [var first, ..];
                    var trailing = values is [.., var last];
                    var middle = values is [var left, .. var middleValues, var right];
                    var textLength = text is { Length: > 0 };
                    return empty || onlySlice || leading || trailing || middle || textLength;
                }
            }
            """);

        StringAssert.Contains(script, ".slice", StringComparison.Ordinal);
        StringAssert.Contains(script, ".length", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_CollectionInitializers_UsesTheBoundListAndDictionaryContracts()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;

            public sealed class TestClass
            {
                static int TestMethod()
                {
                    var values = new List<int> { 1, 2, 3 };
                    var states = new Dictionary<string, int>
                    {
                        ["ready"] = 1,
                        ["queued"] = 2
                    };
                    return values.Count + states["ready"];
                }
            }
            """);

        StringAssert.Contains(script, "ready", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_SourceDefinedCollectionInitializer_UsesTheDeclaredAddMember()
    {
        var script = VisitBlock(
            """
            using System.Collections;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class ScoreBucket : IEnumerable
            {
                public void Add(int score) { }

                public IEnumerator GetEnumerator()
                {
                    yield break;
                }
            }

            public sealed class TestClass
            {
                static ScoreBucket TestMethod()
                {
                    return new ScoreBucket { 2, 5 };
                }
            }
            """);

        StringAssert.Contains(script, "new ScoreBucket", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Add(2)", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Add(5)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public async Task Convert_ImportedMemberWithUnrelatedMetadata_PreservesTheAuthoredRuntimeExportName()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using ECMAScript;

            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            public sealed class JazorAttribute(int operation, string memberName, string runtimeName) : Attribute { }

            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            public sealed class OtherAttribute : Attribute { }

            [ECMAScriptModule("runtime/external.mjs")]
            public static class ExternalModule
            {
                [Jazor(3, "Demo.ExternalModule.Load(int)", "loadRuntime")]
                [Other]
                public static void Load(int value) { }
            }

            [ECMAScriptModule("runtime/fallback.mjs")]
            public static class FallbackModule
            {
                [Other]
                public static void Observe(int value) { }
            }

            public static class TestModule
            {
                public static void TestMethod()
                {
                    ExternalModule.Load(3);
                    FallbackModule.Observe(5);
                }
            }
            """,
            TestMetadataReferences.PreviewParseOptions,
            path: "CompilerTraditionalImportMetadata.cs");
        var compilation = CSharpCompilation.Create(
            "CompilerTraditionalImportMetadata_" + Guid.NewGuid().ToString("N"),
            [sourceTree],
            TestMetadataReferences.Net11.Add(
                MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var fallbackMethod = compilation.GetTypeByMetadataName("FallbackModule")!
            .GetMembers("Observe")
            .OfType<IMethodSymbol>()
            .Single();
        Assert.IsFalse(Util.TryGetJazorImportMapping(fallbackMethod, out var fallbackMemberName, out var fallbackRuntimeName));
        Assert.AreEqual(string.Empty, fallbackMemberName);
        Assert.AreEqual(string.Empty, fallbackRuntimeName);

        var importedMethod = compilation.GetTypeByMetadataName("ExternalModule")!
            .GetMembers("Load")
            .OfType<IMethodSymbol>()
            .Single();
        Assert.IsTrue(Util.TryGetJazorImportMapping(importedMethod, out var importedMemberName, out var importedRuntimeName));
        Assert.AreEqual("Demo.ExternalModule.Load(int)", importedMemberName);
        Assert.AreEqual("loadRuntime", importedRuntimeName);

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        var output = await new AstConverter(module, semanticModel).Convert();
        var script = output?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "loadRuntime", StringComparison.Ordinal);
        StringAssert.Contains(script, "runtime/external.mjs", StringComparison.Ordinal);
        StringAssert.Contains(script, "Observe", StringComparison.Ordinal);
        StringAssert.Contains(script, "runtime/fallback.mjs", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public void Visit_NestedCustomExceptionNamedError_UsesTheSystemExceptionAncestorFallback()
    {
        var script = VisitBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            namespace Custom
            {
                public class Exception : global::System.Exception
                {
                    public Exception(string message) : base(message) { }
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Error : Custom.Exception
            {
                public Error(string message) : base(message) { }
            }

            public sealed class TestClass
            {
                static Error TestMethod()
                {
                    return new Error("custom");
                }
            }
            """);

        StringAssert.Contains(script, "new Error", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    private static string VisitBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerTraditionalCoverage_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));

        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        _ = new Parser().ParseScript("function verify() " + first);
        return first;
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        for (var offset = 0; (offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0; offset += fragment.Length)
            count++;
        return count;
    }
}
