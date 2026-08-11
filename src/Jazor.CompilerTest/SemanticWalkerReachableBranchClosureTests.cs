using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerReachableBranchClosureTests
{
    [TestMethod]
    public void Visit_NestedObjectInitializers_ResolvePropertyIndexerAndFieldReceivers()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class NestedItem
            {
                public int Value { get; set; }
            }

            [ECMAScript]
            sealed class Bucket
            {
                public NestedItem Child { get; } = new();
                public NestedItem this[int index] => Child;
                public NestedItem Field = new();
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    var bucket = new Bucket
                    {
                        Child = { Value = 1 },
                        [0] = { Value = 2 },
                        Field = { Value = 3 }
                    };
                }
            }
            """);

        StringAssert.Contains(script, ".Child.Value = 1", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "[0].Value = 2", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, ".Field.Value = 3", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_IndexAndRangeImplicitConversions_EraseCarrierConstructionAtTheUsageSite()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static void TestMethod()
                {
                    Index fromStart = 2;
                    Index fromEnd = ^2;
                    Range interior = fromStart..fromEnd;
                }
            }
            """);

        StringAssert.Contains(script, "let fromStart = _", StringComparison.Ordinal);
        StringAssert.Contains(script, "let fromEnd = _", StringComparison.Ordinal);
        StringAssert.Contains(script, "let interior = _", StringComparison.Ordinal);
        Assert.DoesNotContain("new Index", script, StringComparison.Ordinal);
        Assert.DoesNotContain("new Range", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TypedArrayUsingStatic_RecoversTheConcreteSelfTypedRuntimeHost()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            static class TestClass
            {
                static void TestMethod()
                {
                    Number width = TypedArray<byte, Uint8Array>.BYTES_PER_ELEMENT;
                    Uint8Array bytes = TypedArray<byte, Uint8Array>.Of(1, 2, 3);
                }
            }
            """);

        StringAssert.Contains(script, "Uint8Array.BYTES_PER_ELEMENT", StringComparison.Ordinal);
        StringAssert.Contains(script, "Uint8Array.of(1, 2, 3)", StringComparison.Ordinal);
        Assert.DoesNotContain("TypedArray", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_RenamedTupleReturnedByInvocation_CachesTheSourceBeforeProjectingSlots()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static (int Left, int Right) ReadPair() => (1, 2);

                static (int First, int Second) TestMethod()
                {
                    return ReadPair();
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadPair()"), script);
        StringAssert.Contains(script, "First:", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Second:", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_RenamedNestedTupleFromField_ProjectsEveryNestedRuntimeName()
    {
        var script = VisitBlock(
            """
            sealed class TestClass
            {
                private ((int Left, int Right) Pair, int Tail) _value;

                ((int First, int Second) Pair, int Last) TestMethod()
                {
                    return _value;
                }
            }
            """);

        StringAssert.Contains(script, "First:", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Second:", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Last:", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_GenericNewConstraint_RejectsMissingRuntimeConstructorBinding()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static T TestMethod<T>() where T : new()
                {
                    return new T();
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Type-parameter object creation", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "no runtime constructor binding", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_EmptyObjectLiteralInitializer_ProducesAnEmptyHostObject()
    {
        var script = VisitBlock(
            """
            using static ECMAScript.Vue;

            static class TestClass
            {
                static void TestMethod()
                {
                    var attributes = new VueDictionary { };
                }
            }
            """);

        StringAssert.Contains(script, "let attributes = {}", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ObjectLiteralHostWithoutInitializer_ProducesAnEmptyHostObject()
    {
        var script = VisitBlock(
            """
            using static ECMAScript.Vue;

            static class TestClass
            {
                static void TestMethod()
                {
                    var attributes = new VueDictionary();
                }
            }
            """);

        StringAssert.Contains(script, "let attributes = {}", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NonArrayPropertyNamedRank_UsesNormalPropertyAccess()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Matrix
            {
                public int Rank { get; set; }
            }

            static class TestClass
            {
                static int TestMethod(Matrix matrix)
                {
                    return matrix.Rank;
                }
            }
            """);

        StringAssert.Contains(script, "return matrix.Rank;", StringComparison.Ordinal);
        Assert.DoesNotContain("return 1;", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StaticHostPropertyNamedRank_UsesNormalStaticPropertyAccess()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class Matrix
            {
                public static int Rank { get; }
            }

            static class TestClass
            {
                static int TestMethod()
                {
                    return Matrix.Rank;
                }
            }
            """);

        StringAssert.Contains(script, "return Matrix.Rank;", StringComparison.Ordinal);
        Assert.DoesNotContain("return 1;", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StaticEnumerablePipeline_LowersCallbacksAndMaterializesEnumerableContracts()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static int[] TestMethod(IEnumerable<int> source)
                {
                    var positive = Enumerable.Where(source, value => value > 0);
                    var doubled = Enumerable.Select(positive, value => value * 2);
                    return Enumerable.ToArray(doubled);
                }
            }
            """);

        StringAssert.Contains(script, "Array.from(__src).filter(__callback)", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(__src).map(__callback)", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (__src == null)", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (__callback == null)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(source) " + script);
    }

    [TestMethod]
    public void Visit_StaticEnumerableMaterializers_KeepFreshArraysAndMarkMutableLists()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static List<int> TestMethod()
                {
                    var copy = Enumerable.ToArray(new[] { 1, 2, 3 });
                    return Enumerable.ToList(copy);
                }
            }
            """);

        StringAssert.Contains(script, "let copy =", StringComparison.Ordinal);
        StringAssert.Contains(script, "MarkAsMutableListCarrier", StringComparison.Ordinal);
        Assert.AreEqual(1, CountOccurrences(script, "Array.from(__src)"), script);
        StringAssert.Contains(script, "return __src;", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_UInt32UpperHexFormat_DoesNotApplySignedIntegerNormalization()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(uint value)
                {
                    return value.ToString("X");
                }
            }
            """);

        StringAssert.Contains(script, "value.toString(16).toUpperCase()", StringComparison.Ordinal);
        Assert.DoesNotContain(">>> 0", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_StaticLocalFunctionMethodGroup_PreservesLexicalBinding()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static void TestMethod()
                {
                    static void Apply() { }
                    Action callback = Apply;
                    callback();
                }
            }
            """);

        StringAssert.Contains(script, "function Apply()", StringComparison.Ordinal);
        StringAssert.Contains(script, "let callback = Apply;", StringComparison.Ordinal);
        StringAssert.Contains(script, "callback();", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_MethodGroups_PreserveExtensionInstanceAndDelegateCallTargets()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Linq;

            sealed class TestClass
            {
                private static int AddOne(int value) => value + 1;

                private int Double(int value) => value * 2;

                static int TestMethod(TestClass receiver, Func<int, int> callback, int[] values)
                {
                    Func<int, int> staticMethod = AddOne;
                    Func<int, int> instanceMethod = receiver.Double;
                    Func<int, bool> contains = values.Contains;
                    return staticMethod(callback(1)) + instanceMethod(2) + (contains(3) ? 1 : 0);
                }
            }
            """);

        StringAssert.Contains(script, "staticMethod(callback(1))", StringComparison.Ordinal);
        StringAssert.Contains(script, ".bind(receiver)", StringComparison.Ordinal);
        StringAssert.Contains(script, "contains(3)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(receiver, callback, values) " + script);
    }

    [TestMethod]
    public void Visit_StringForEachWithCapturedIterationValue_PreservesUtf16IterationAndClosureScope()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static void TestMethod(string value)
                {
                    foreach (var character in value)
                    {
                        Action report = () => Console.WriteLine(character);
                        report();
                    }
                }
            }
            """);

        StringAssert.Contains(script, "value.split(\"\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "() =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "report()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_CustomRangeIndexerWithExplicitRangeBoundary_RejectsUnsupportedSliceProtocol()
    {
        var block = GetBlock(
            """
            using System;

            sealed class Buffer
            {
                public int this[Range range] => 0;
            }

            static class TestClass
            {
                static int TestMethod(Buffer buffer)
                {
                    return buffer[(Range)(1..^1)];
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Range-based indexer", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "int-based slice member", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_SourceConstructorWithRefParameter_RejectsMissingModuleSinkProtocol()
    {
        var block = GetBlock(
            """
            sealed class TestClass
            {
                sealed class Payload
                {
                    public Payload(ref int value) => value++;
                }

                void TestMethod()
                {
                    int value = 1;
                    _ = new Payload(ref value);
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "ref/out parameters", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "constructor sink protocol", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_PatternSwitchContinue_RejectsCrossingTheGeneratedIifeBoundary()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static void TestMethod(int value)
                {
                    while (true)
                    {
                        switch (value)
                        {
                            case > 0:
                                continue;
                            default:
                                return;
                        }
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Continue statements inside pattern-matching switch", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ImplicitObjectInitializerFieldReference_UsesTheBoundMemberName()
    {
        var block = GetBlock(
            """
            sealed class TestClass
            {
                sealed class Payload
                {
                    public int Value;
                }

                void TestMethod()
                {
                    _ = new Payload { Value = 3 };
                }
            }
            """);
        var field = block.DescendantsAndSelf()
            .OfType<IFieldReferenceOperation>()
            .Single(static operation => operation.Field.Name == "Value");

        var expression = new SemanticWalker(true).VisitFieldReference(field, new SenseArgument());

        Assert.AreEqual("Value", expression?.ToKnRECMAScript());
    }

    [TestMethod]
    public void Visit_TypedHostClaimsCreationAndReferenceOperations_UsesOnlyHostAstValues()
    {
        var block = GetBlock(
            """
            using System;

            sealed class TestClass
            {
                private int _field;
                private int Value { get; set; }

                int TestMethod()
                {
                    var payload = new Payload();
                    Action callback = this.Touch;
                    this.Touch();
                    return this._field + this.Value + payload.Code;
                }

                void Touch() { }

                sealed class Payload
                {
                    public int Code { get; set; }
                }
            }
            """);
        var host = new ClaimingHost();

        var script = new SemanticWalker(true) { Host = host }
            .Visit(block, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "hostCreated", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostMethod", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostCall", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostField", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostProperty", StringComparison.Ordinal);
        Assert.IsGreaterThan(0, host.InstanceReferenceCount);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_EcmascriptOptionalArgument_OmitsTheRoslynDefaultPlaceholder()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            static class TestClass
            {
                static void TestMethod()
                {
                    System.Console.Count();
                }
            }
            """);

        StringAssert.Contains(script, "console.count()", StringComparison.Ordinal);
        Assert.DoesNotContain("null", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_DictionaryIndexerCompoundAssignment_UsesMappedGetterSetterAndSingleEvaluation()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;

            static class TestClass
            {
                static int TestMethod(Dictionary<string, int> values)
                {
                    values[ReadKey()] += ReadValue();
                    return values["result"];
                }

                static string ReadKey() => "result";
                static int ReadValue() => 2;
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "ReadKey()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "ReadValue()"), script);
        StringAssert.Contains(script, "let ", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void VisitArgument_OutParameter_UsesOutParameterSenseWithoutChangingTheBinding()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static void TestMethod()
                {
                    Assign(out var value);
                }

                static void Assign(out int value) => value = 1;
            }
            """);
        var outArgument = block.DescendantsAndSelf()
            .OfType<IArgumentOperation>()
            .Single(static argument => argument.Parameter?.RefKind == RefKind.Out);

        var expression = new SemanticWalker(true).VisitArgument(outArgument, new SenseArgument());

        Assert.AreEqual("value", expression?.ToKnRECMAScript());
    }

    [TestMethod]
    public void Visit_RefReturnArgument_RejectsTheInvocationAsAnUnassignableWriteBackLocation()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                private static int _revision;

                static ref int CurrentRevision() => ref _revision;

                static void Advance(ref int revision) => revision++;

                static void TestMethod()
                {
                    Advance(ref CurrentRevision());
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "requires an assignable JavaScript location", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "ref/out argument", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_StructDeconstruction_RejectsTheMissingRuntimeStructDeclarationProtocol()
    {
        var block = GetBlock(
            """
            struct RevisionPair
            {
                public void Deconstruct(out int current, out int next)
                {
                    current = 1;
                    next = 2;
                }
            }

            static class TestClass
            {
                static int TestMethod(RevisionPair pair)
                {
                    var (current, next) = pair;
                    return current + next;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Custom Deconstruct on struct type", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "member struct runtime declarations", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExtensionDeconstruction_RejectsTheMissingReceiverRuntimeSlot()
    {
        var block = GetBlock(
            """
            sealed class RevisionBox
            {
            }

            static class RevisionBoxExtensions
            {
                public static void Deconstruct(this RevisionBox box, out int revision, out int next)
                {
                    revision = 1;
                    next = 2;
                }
            }

            static class TestClass
            {
                static int TestMethod(RevisionBox box)
                {
                    var (revision, _) = box;
                    return revision;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Extension Deconstruct method", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "receiver-member runtime slot", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ArrayElementDeconstructionTarget_RejectsTheUnsupportedWriteBackShape()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static void TestMethod(int[] values)
                {
                    (values[0], values[1]) = (1, 2);
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "ArrayElementReference", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "DeconstructionAssignment", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CustomClassDeconstruction_UsesTheBoundMemberResultProtocol()
    {
        var script = VisitBlock(
            """
            sealed class RevisionPair
            {
                public void Deconstruct(out int current, out int next)
                {
                    current = 1;
                    next = 2;
                }
            }

            static class TestClass
            {
                static int TestMethod(RevisionPair pair)
                {
                    var (current, next) = pair;
                    return current + next;
                }
            }
            """);

        StringAssert.Contains(script, "pair.Deconstruct(current, next)", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "current = v$0[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "next = v$0[1]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(pair) " + script);
    }

    [TestMethod]
    public void Visit_ErasedInterfacePatterns_FoldKnownAssignableAndIncompatibleObjectCreations()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class DisposableRelease : IDisposable
            {
                public void Dispose() { }
            }

            [ECMAScript]
            sealed class PendingRelease
            {
            }

            static class TestClass
            {
                static bool TestMethod()
                {
                    return ((object)new DisposableRelease()) is IDisposable &&
                        !(((object)new PendingRelease()) is IDisposable);
                }
            }
            """);

        StringAssert.Contains(script, "true", StringComparison.Ordinal);
        StringAssert.Contains(script, "false", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ErasedInterfacePatterns_FoldKnownNullAndConstrainedValueSources()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static bool TestMethod<T>(T resource)
                    where T : struct, IDisposable
                {
                    object? missing = null;
                    return missing is IDisposable || resource is IDisposable;
                }
            }
            """);

        StringAssert.Contains(script, "false", StringComparison.Ordinal);
        StringAssert.Contains(script, "true", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(resource) " + script);
    }

    [TestMethod]
    public void Visit_NestedCustomDeconstruction_UsesTheBoundNestedWriteBackProtocol()
    {
        var script = VisitBlock(
            """
            sealed class RevisionPair
            {
                public void Deconstruct(out (int Major, int Minor) version, out int build)
                {
                    version = (1, 2);
                    build = 3;
                }
            }

            static class TestClass
            {
                static int TestMethod(RevisionPair pair)
                {
                    var ((major, minor), build) = pair;
                    return major + minor + build;
                }
            }
            """);

        StringAssert.Contains(script, "pair.Deconstruct", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "major = v$0.Major", StringComparison.Ordinal);
        StringAssert.Contains(script, "minor = v$0.Minor", StringComparison.Ordinal);
        StringAssert.Contains(script, "build =", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(pair) " + script);
    }

    [TestMethod]
    public void Visit_DeconstructionIntoCurrentModuleStaticField_UsesTheDeclaredHostTarget()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                private static int Revision;

                static int TestMethod()
                {
                    int build;
                    (Revision, build) = (1, 2);
                    return Revision + build;
                }
            }
            """);

        StringAssert.Contains(script, "TestClass.Revision = 1", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "build = 2", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_CurrentModuleIndexerCompoundAssignment_UsesTheBoundGetterSetterBridge()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseSlots
            {
                public int this[int index]
                {
                    get => index;
                    set { }
                }
            }

            static class TestClass
            {
                static ReleaseSlots GetSlots() => new();
                static int GetIndex() => 1;
                static int GetDelta() => 2;

                static void TestMethod()
                {
                    GetSlots()[GetIndex()] |= GetDelta();
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.GetSlots()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.GetIndex()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.GetDelta()"), script);
        StringAssert.Contains(script, "|", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StaticSourceType_NotifiesTheHostThroughNormalTypeLowering()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                private static class SourceHost
                {
                    public static void Touch() { }
                }

                static void TestMethod()
                {
                    SourceHost.Touch();
                }
            }
            """);
        var host = new TypeObservingHost();

        var script = new SemanticWalker(true) { Host = host }
            .Visit(block, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.IsNotNull(script);
        CollectionAssert.Contains(host.TypeNames, "SourceHost");
        StringAssert.Contains(script, "SourceHost.Touch()", StringComparison.Ordinal);
    }

    private static string VisitBlock(string source)
    {
        var block = GetBlock(source);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }

    private static IBlockOperation GetBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerReachableBranchClosure.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerReachableBranchClosure_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        for (var offset = 0; (offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0; offset += fragment.Length)
            count++;
        return count;
    }

    private sealed class ClaimingHost : SemanticWalkerHost
    {
        public int InstanceReferenceCount { get; private set; }

        public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
            => operation.Type?.Name == "Payload";

        public override Expression? RewriteObjectCreation(
            IObjectCreationOperation operation,
            SenseArgument argument,
            IReadOnlyList<Expression> arguments)
            => new Identifier("hostCreated");

        public override Expression? RewriteFieldReference(
            IFieldReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => operation.Field.Name == "_field" ? new Identifier("hostField") : null;

        public override Expression? RewritePropertyReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => operation.Property.Name == "Value" ? new Identifier("hostProperty") : null;

        public override Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => operation.Method.Name == "Touch" ? new Identifier("hostMethod") : null;

        public override Expression? RewriteInvocation(
            IInvocationOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => operation.TargetMethod.Name == "Touch" ? new Identifier("hostCall") : null;

        public override Expression? RewriteInstanceReference(
            IInstanceReferenceOperation operation,
            SenseArgument argument)
        {
            InstanceReferenceCount++;
            return new Identifier("hostThis");
        }
    }

    private sealed class TypeObservingHost : SemanticWalkerHost
    {
        public List<string> TypeNames { get; } = [];

        public override void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
            => TypeNames.Add(type.Name);
    }
}
