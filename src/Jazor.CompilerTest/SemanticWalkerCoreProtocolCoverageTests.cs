using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCoreProtocolCoverageTests
{
    [TestMethod]
    public void Visit_StandaloneIndexAndOpenRangeValues_UseClrCarrierMappings()
    {
        var script = VisitBlock(
            """
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    Index fromEnd = ^2;
                    Range all = ..;
                    Range beforeLast = ..^1;
                    Range fromSecond = 2..;
                    Range interior = 2..^1;
                }
            }
            """);

        StringAssert.Contains(script, "let fromEnd =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let all =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let beforeLast =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let fromSecond =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let interior =", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ImplicitIndexers_DistinguishFromStartFromEndAndMaterializedIndex()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            class Buffer
            {
                public int Length => 8;

                public int this[int index]
                {
                    get => 0;
                    set { }
                }
            }

            class TestClass
            {
                void TestMethod(Buffer buffer, Index index)
                {
                    int first = buffer[2];
                    int last = buffer[^2];
                    int selected = buffer[index];
                }
            }
            """);

        StringAssert.Contains(script, "let first = buffer[", StringComparison.Ordinal);
        StringAssert.Contains(script, "let last = buffer[", StringComparison.Ordinal);
        StringAssert.Contains(script, "let selected = buffer[", StringComparison.Ordinal);
        StringAssert.Contains(script, "index, buffer.Length", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_TupleDeconstruction_CachesDependentSlotsBeforeWrites()
    {
        var script = VisitBlock(
            """
            class TestClass
            {
                private static (int Left, (int First, int Second) Pair) Next()
                    => (1, (2, 3));

                void TestMethod(int left, int first, int second)
                {
                    (left, (first, second)) = Next();
                    (left, first) = (first, left);
                    var (outer, (innerFirst, innerSecond)) = Next();
                }
            }
            """);

        StringAssert.Contains(script, "TestClass.Next()", StringComparison.Ordinal);
        StringAssert.Contains(script, "left =", StringComparison.Ordinal);
        StringAssert.Contains(script, "first =", StringComparison.Ordinal);
        StringAssert.Contains(script, "outer, innerFirst, innerSecond", StringComparison.Ordinal);
        Assert.DoesNotContain("left = first, first = left", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_InterpolatedStringAlignment_UsesLeftRightAndIdentityWidths()
    {
        var script = VisitBlock(
            """
            class TestClass
            {
                void TestMethod(string value)
                {
                    string left = $"[{value,6}]";
                    string right = $"[{value,-6}]";
                    string identity = $"[{value,0}]";
                }
            }
            """);

        StringAssert.Contains(script, "let left =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let right =", StringComparison.Ordinal);
        StringAssert.Contains(script, "let identity =", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ObjectCreationFallbacks_SelectBigIntErrorAndTypeErrorContracts()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Numerics;

            class TestClass
            {
                void TestMethod(int value)
                {
                    BigInteger zero = new();
                    BigInteger integer = new(value);
                    Exception plain = new();
                    Exception message = new("failed");
                    InvalidOperationException invalid = new("invalid");
                    ArgumentNullException missing = new("value");
                }
            }
            """);

        StringAssert.Contains(script, "let zero = BigInt();", StringComparison.Ordinal);
        StringAssert.Contains(script, "let integer = BigInt(value);", StringComparison.Ordinal);
        StringAssert.Contains(script, "new Error", StringComparison.Ordinal);
        StringAssert.Contains(script, "new TypeError", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ForEachOverGenericEnumerable_DoesNotRequireAConcreteCollectionCarrier()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;

            class TestClass
            {
                void TestMethod<TCollection>(TCollection values) where TCollection : IEnumerable<int>
                {
                    foreach (var value in values)
                        Console.WriteLine(value);
                }
            }
            """);

        StringAssert.Contains(script, "for (let value of values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "console.log(value)", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ForEachDeconstructionOverCustomType_RejectsAnUnknownRuntimeShape()
    {
        var block = GetBlock(
            """
            using System.Collections.Generic;

            class TestClass
            {
                sealed class Point
                {
                    public void Deconstruct(out int x, out int y)
                    {
                        x = 1;
                        y = 2;
                    }
                }

                void TestMethod(IEnumerable<Point> values)
                {
                    foreach (var (x, y) in values)
                    {
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "does not have a compiler-known structural runtime shape", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DefaultEnumValues_UseTheirRuntimeScalarCarriers()
    {
        var script = VisitBlock(
            """
            enum ByteState : byte
            {
                None
            }

            enum LongState : long
            {
                None
            }

            class TestClass
            {
                void TestMethod()
                {
                    ByteState small = default;
                    LongState large = default;
                }
            }
            """);

        StringAssert.Contains(script, "let small = 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "let large = 0n", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_DefaultHalfAndInt128_UseTheNumberAndBigIntCarriers()
    {
        var script = VisitBlock(
            """
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    Half half = default;
                    Int128 wide = default;
                }
            }
            """);

        StringAssert.Contains(script, "let half = 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "let wide = 0n", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_CompoundAssignments_PreserveArithmeticBitwiseShiftAndUnsignedShiftOperators()
    {
        var script = VisitBlock(
            """
            class TestClass
            {
                void TestMethod(int total, int flags, uint cursor)
                {
                    total += 3;
                    total -= 2;
                    total *= 4;
                    total /= 2;
                    total %= 5;
                    flags &= 15;
                    flags |= 32;
                    flags ^= 4;
                    flags <<= 1;
                    flags >>= 2;
                    cursor >>>= 1;
                }
            }
            """);

        foreach (var @operator in new[] { "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", ">>>=" })
            StringAssert.Contains(script, @operator, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_AllDiscardLabeledStatement_PreservesTheReachableJavaScriptTarget()
    {
        var script = VisitBlock(
            """
            class TestClass
            {
                void TestMethod()
                {
                retry:
                    (_, _) = (1, 2);
                }
            }
            """);

        StringAssert.Contains(script, "retry:", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_UnmappedEcmascriptConditionalProperty_UsesNativeOptionalAccess()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public int Priority { get; set; }
            }

            class TestClass
            {
                int? TestMethod(Release? release)
                {
                    return release?.Priority;
                }
            }
            """);

        StringAssert.Contains(script, "release?.priority", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_DerivedPositionalRecord_UsesInheritedStructuralRuntimeProperties()
    {
        var script = VisitBlock(
            """
            record BaseRelease(string Name);

            record Release(string Name, int Revision) : BaseRelease(Name);

            class TestClass
            {
                Release TestMethod()
                {
                    return new Release("jazor", 7);
                }
            }
            """);

        StringAssert.Contains(script, "name: \"jazor\"", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "revision: 7", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ExplicitRecordConstructorWithoutProperty_UsesTheBoundParameterName()
    {
        var script = VisitBlock(
            """
            record ReleaseEnvelope
            {
                public ReleaseEnvelope(string channel)
                {
                }
            }

            class TestClass
            {
                void TestMethod()
                {
                    var envelope = new ReleaseEnvelope("canary");
                }
            }
            """);

        StringAssert.Contains(script, "channel: \"canary\"", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("new ReleaseEnvelope", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_CapturedForControlVariable_PreservesTheSingleCSharpLoopBinding()
    {
        var script = VisitBlock(
            """
            using System;

            class TestClass
            {
                Func<int>? TestMethod()
                {
                    Func<int>? captured = null;
                    var total = 0;
                    for (var direct = 0; direct < 2; direct++)
                    {
                        total += direct;
                    }

                    for (var index = 0; index < 3; index++)
                    {
                        captured = () => index;
                    }

                    Func<int>? localCaptured = null;
                    for (var localIndex = 0; localIndex < 3; localIndex++)
                    {
                        int Current() => localIndex;
                        localCaptured = Current;
                    }

                    return localCaptured ?? captured;
                }
            }
            """);

        var declarationIndex = script.IndexOf("let index = 0", StringComparison.Ordinal);
        var loopIndex = script.LastIndexOf("for", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, declarationIndex, script);
        Assert.IsGreaterThan(declarationIndex, loopIndex, script);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_VueDictionaryLiteral_UsesTheObjectHostInsteadOfAConstructor()
    {
        var script = VisitBlock(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using System.ComponentModel;

            [ECMAScript]
            [Description("@#")]
            sealed class RuntimeAttributes
            {
                public string? Title { get; set; }
            }

            class TestClass
            {
                void TestMethod()
                {
                    var attributes = new VueDictionary
                    {
                        ["role"] = "button"
                    };
                    var entries = new VueDictionary
                    {
                        { "title", "release" }
                    };
                    var runtimeAttributes = new RuntimeAttributes
                    {
                        Title = "candidate"
                    };
                }
            }
            """);

        StringAssert.Contains(script, "role: \"button\"", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "title: \"release\"", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "title: \"candidate\"", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("new VueDictionary", script, StringComparison.Ordinal);
        Assert.DoesNotContain("new RuntimeAttributes", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_OptionalAndNamedConstructorArguments_UseTheBoundRuntimeConstructorOrder()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public Release(string channel, int retries = 3)
                {
                }
            }

            class TestClass
            {
                void TestMethod()
                {
                    var stable = new Release("stable");
                    var canary = new Release(retries: 5, channel: "canary");
                }
            }
            """);

        StringAssert.Contains(script, "new Release(\"stable\")", StringComparison.Ordinal);
        var retriesEvaluation = script.IndexOf("v$0 = 5", StringComparison.Ordinal);
        var channelEvaluation = script.IndexOf("v$1 = \"canary\"", StringComparison.Ordinal);
        var boundConstructorCall = script.IndexOf(", v$0)", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, retriesEvaluation, script);
        Assert.IsGreaterThan(retriesEvaluation, channelEvaluation, script);
        Assert.IsGreaterThan(channelEvaluation, boundConstructorCall, script);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_UsingGenericInterfaceConstraint_ResolvesTheBoundDisposalContract()
    {
        var script = VisitBlock(
            """
            using System;

            class TestClass
            {
                void TestMethod<T>(T resource) where T : IDisposable
                {
                    using (resource)
                    {
                        Console.WriteLine("released");
                    }
                }
            }
            """);

        StringAssert.Contains(script, "try", StringComparison.Ordinal);
        StringAssert.Contains(script, "finally", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_FormattableEcmascriptValue_UsesItsBoundFormatMethod()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseName : IFormattable
            {
                public string ToString(string? format, IFormatProvider? provider) => "";
            }

            sealed class ReleaseCode
            {
                public override string ToString() => "";
            }

            class TestClass
            {
                string TestMethod(ReleaseName release, ReleaseCode code, string? note)
                {
                    return $"release:{release:display}|code:{code:ignored}|note:{note}|missing:{null}|constant:{5}";
                }
            }
            """);

        StringAssert.Contains(script, "toString(\"display\", null)", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "toString()", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(release, code, note) " + script);
    }

    [TestMethod]
    public void Visit_NonNullRecursivePattern_UsesTheReferenceFallbackMatch()
    {
        var script = VisitBlock(
            """
            class TestClass
            {
                bool TestMethod(object? value, int number, int? nullableNumber)
                {
                    return value is { } && number is { } && nullableNumber is { };
                }
            }
            """);

        StringAssert.Contains(script, "value != null", StringComparison.Ordinal);
        StringAssert.Contains(script, "true", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value, number, nullableNumber) " + script);
    }

    [TestMethod]
    public void Visit_KnownEcmascriptRuntimeHosts_UseTheirNativeTypeChecks()
    {
        var script = VisitBlock(
            """
            using ECMAScript;
            using System.ComponentModel;

            [ECMAScript]
            [Description("@#Date")]
            sealed class RuntimeDate
            {
            }

            [ECMAScript]
            [Description("@#Map")]
            sealed class RuntimeMap
            {
            }

            [ECMAScript]
            [Description("@#Set")]
            sealed class RuntimeSet
            {
            }

            [ECMAScript]
            [Description("@#Boolean")]
            sealed class RuntimeBoolean
            {
            }

            [ECMAScript]
            [Description("@#String")]
            sealed class RuntimeString
            {
            }

            [ECMAScript]
            [Description("@#Array")]
            sealed class RuntimeArray
            {
            }

            [ECMAScript]
            [Description("@#Number")]
            sealed class RuntimeNumber
            {
            }

            [ECMAScript]
            [Description("@#BigInt")]
            sealed class RuntimeBigInt
            {
            }

            [ECMAScript]
            [Description("@#ReleaseRuntime")]
            sealed class ReleaseRuntime
            {
            }

            class TestClass
            {
                bool TestMethod(
                    RuntimeDate date,
                    RuntimeMap map,
                    RuntimeSet set,
                    RuntimeBoolean enabled,
                    RuntimeString text,
                    RuntimeArray items,
                    RuntimeNumber revision,
                    RuntimeBigInt sequence,
                    ReleaseRuntime release)
                {
                    return date is RuntimeDate &&
                        map is RuntimeMap &&
                        set is RuntimeSet &&
                        enabled is RuntimeBoolean &&
                        text is RuntimeString &&
                        items is RuntimeArray &&
                        revision is RuntimeNumber &&
                        sequence is RuntimeBigInt &&
                        release is ReleaseRuntime;
                }
            }
            """);

        StringAssert.Contains(script, "date instanceof Date", StringComparison.Ordinal);
        StringAssert.Contains(script, "map instanceof Map", StringComparison.Ordinal);
        StringAssert.Contains(script, "set instanceof Set", StringComparison.Ordinal);
        StringAssert.Contains(script, "typeof enabled === \"boolean\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "typeof text === \"string\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.isArray(items)", StringComparison.Ordinal);
        StringAssert.Contains(script, "typeof revision === \"number\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "typeof sequence === \"bigint\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "release instanceof ReleaseRuntime", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(date, map, set, enabled, text, items, revision, sequence, release) " + script);
    }

    [TestMethod]
    public void Visit_ObjectMappedRuntimeHostIsType_RejectsTheErasedRuntimeIdentity()
    {
        var block = GetBlock(
            """
            using ECMAScript;
            using System.ComponentModel;

            [ECMAScript]
            [Description("@#Object")]
            sealed class RuntimeObject
            {
            }

            class TestClass
            {
                bool TestMethod(RuntimeObject value)
                {
                    return value is RuntimeObject;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Unsupported type in is-type operation", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "RuntimeObject", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DictionaryWithCustomEqualityKey_RejectsNativeMapIdentityMismatch()
    {
        var block = GetBlock(
            """
            using System;
            using System.Collections.Generic;

            sealed class ReleaseKey : IEquatable<ReleaseKey>
            {
                public bool Equals(ReleaseKey? other) => true;

                public override bool Equals(object? other) => other is ReleaseKey;

                public override int GetHashCode() => 0;
            }

            class TestClass
            {
                void TestMethod()
                {
                    var releases = new Dictionary<ReleaseKey, int>();
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Dictionary<ReleaseKey, int>", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "custom equality semantics", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_HashSetWithRecordElement_RejectsNativeSetIdentityMismatch()
    {
        var block = GetBlock(
            """
            using System.Collections.Generic;

            record ReleaseTag(string Name);

            class TestClass
            {
                void TestMethod()
                {
                    var tags = new HashSet<ReleaseTag>();
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "HashSet<ReleaseTag>", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "record/custom equality semantics", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_NativeMapSetWithIdentityStableKeys_UsesJavaScriptCollections()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;

            sealed class ReleaseKey
            {
            }

            sealed class ReleaseFamily
            {
            }

            sealed class ComparableReleaseKey : System.IComparable<ComparableReleaseKey>
            {
                public int CompareTo(ComparableReleaseKey? other) => 0;
            }

            sealed class CrossEquatableReleaseKey : System.IEquatable<ReleaseFamily>
            {
                public bool Equals(ReleaseFamily? other) => false;
            }

            class TestClass
            {
                void TestMethod()
                {
                    var revisions = new Dictionary<ReleaseKey, int>();
                    var channels = new HashSet<string>();
                    var comparableRevisions = new Dictionary<ComparableReleaseKey, int>();
                    var familyRevisions = new Dictionary<CrossEquatableReleaseKey, int>();
                }
            }
            """);

        StringAssert.Contains(script, "let revisions = createDefault();", StringComparison.Ordinal);
        StringAssert.Contains(script, "let channels = createDefault();", StringComparison.Ordinal);
        StringAssert.Contains(script, "let comparableRevisions = createDefault();", StringComparison.Ordinal);
        StringAssert.Contains(script, "let familyRevisions = createDefault();", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_NestedObjectInitializer_UsesTheHostStructuralPropertyGraph()
    {
        var script = VisitBlock(
            """
            using ECMAScript;
            using System.ComponentModel;

            [ECMAScript]
            [Description("@#")]
            sealed class ReleaseMetadata
            {
                public string? Channel { get; set; }
            }

            [ECMAScript]
            [Description("@#")]
            sealed class ReleaseRequest
            {
                public ReleaseMetadata Metadata { get; } = null!;
            }

            class TestClass
            {
                void TestMethod()
                {
                    var request = new ReleaseRequest
                    {
                        Metadata =
                        {
                            Channel = "canary"
                        }
                    };
                }
            }
            """);

        StringAssert.Contains(script, "metadata:", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "channel: \"canary\"", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("new ReleaseRequest", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_RuntimeClassNestedPropertyAndFieldInitializers_UseMemberWriteProtocols()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class RuntimeMetadata
            {
                public string? Channel { get; set; }
            }

            [ECMAScript]
            sealed class RuntimeRequest
            {
                public RuntimeMetadata Primary { get; } = null!;

                public RuntimeMetadata Secondary = null!;
            }

            class TestClass
            {
                void TestMethod()
                {
                    var request = new RuntimeRequest
                    {
                        Primary =
                        {
                            Channel = "stable"
                        },
                        Secondary =
                        {
                            Channel = "canary"
                        }
                    };
                }
            }
            """);

        StringAssert.Contains(script, "new RuntimeRequest", StringComparison.Ordinal);
        StringAssert.Contains(script, ".primary.channel = \"stable\"", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, ".secondary.channel = \"canary\"", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_LinqArrayAndEnumerablePipelines_PreserveTheirDifferentRuntimeContracts()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            class TestClass
            {
                int TestMethod(int[] releases, IEnumerable<int> queued)
                {
                    var published = releases
                        .Where(release => release > 0)
                        .Select(release => release + 1)
                        .ToArray();
                    var normalizedQueue = queued.ToArray();
                    return published.Length + normalizedQueue.Length;
                }
            }
            """);

        StringAssert.Contains(script, ".filter(", StringComparison.Ordinal);
        StringAssert.Contains(script, ".map(", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(__src)", StringComparison.Ordinal);
        StringAssert.Contains(script, "})(queued)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(releases, queued) " + script);
    }

    [TestMethod]
    public void Visit_DelegateInvocation_UsesTheDelegateValueAsTheCallee()
    {
        var script = VisitBlock(
            """
            using System;

            class TestClass
            {
                int TestMethod()
                {
                    Func<int, int> advance = value => value + 1;
                    Func<int, int> invoke = advance.Invoke;
                    return invoke(4);
                }
            }
            """);

        StringAssert.Contains(script, "invoke(4)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ScalarCarrierConversionsAndNullableClrProperty_PreserveTheBoundRuntimeContracts()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static void TestMethod(int revision, long sequence, DateTime? scheduled)
                {
                    long promoted = revision;
                    int narrowed = (int)sequence;
                    int? day = scheduled?.Day;
                }
            }
            """);

        StringAssert.Contains(script, "let promoted", StringComparison.Ordinal);
        StringAssert.Contains(script, "let narrowed", StringComparison.Ordinal);
        StringAssert.Contains(script, "let day", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(revision, sequence, scheduled) " + script);
    }

    [TestMethod]
    public void Visit_ScalarPropertyAndEmptyListPatterns_UseTheirSafeCarrierChecks()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(string text, int[] values)
                {
                    return text is { Length: > 0 } && values is [];
                }
            }
            """);

        StringAssert.Contains(script, "text.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "values.length === 0", StringComparison.Ordinal);
        Assert.DoesNotContain("\"length\" in text", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(text, values) " + script);
    }

    [TestMethod]
    public void Visit_ErasedInterfaceSwitchPatterns_FoldTheSharedDiscriminantAtBothLanguageEntrypoints()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseLease : IDisposable
            {
                public void Dispose() { }
            }

            static class TestClass
            {
                static bool TestMethod()
                {
                    object first = new ReleaseLease();
                    var expressionMatch = first switch
                    {
                        IDisposable => true,
                        _ => false
                    };

                    object second = new ReleaseLease();
                    switch (second)
                    {
                        case IDisposable:
                            return expressionMatch;
                        default:
                            return false;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "expressionMatch", StringComparison.Ordinal);
        StringAssert.Contains(script, "return expressionMatch", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ComputedRuntimePropertyPatternAliases_UseNumericAndQuotedPropertyKeys()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseMetadata
            {
                [ECMAScriptName("[0]")]
                public string? Channel { get; }

                [ECMAScriptName("[\"release-name\"]")]
                public string? Name { get; }
            }

            static class TestClass
            {
                static bool TestMethod(ReleaseMetadata metadata)
                {
                    return metadata is { Channel: "stable", Name: "jazor" };
                }
            }
            """);

        StringAssert.Contains(script, "metadata[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "metadata[\"release-name\"]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(metadata) " + script);
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
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerCoreProtocolCoverageTests",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
