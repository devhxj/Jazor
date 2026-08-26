using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerBoundaryProtocolScenarioTests
{
    [TestMethod]
    public void Visit_AwaitedObjectInitializer_UsesAnAsyncIifeForTheInitializerLifetime()
    {
        var script = VisitBlock(
            """
            using System.Threading.Tasks;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseOptions
            {
                public int Revision { get; set; }
            }

            static class TestClass
            {
                static async Task<int> ReadRevisionAsync() => await Task.FromResult(3);

                static async Task<ReleaseOptions> TestMethod()
                {
                    return new ReleaseOptions { Revision = await ReadRevisionAsync() };
                }
            }
            """);

        StringAssert.Contains(script, "async () =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "ReadRevisionAsync", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("async function verify() " + script);
    }

    [TestMethod]
    public void Visit_NestedMemberInitializer_PreservesTheBoundPropertyAndFieldPaths()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseClient
            {
                public ReleaseSettings Settings { get; } = null!;
            }

            [ECMAScript]
            sealed class ReleaseSettings
            {
                public string? Channel { get; set; }
                public int Retries;
            }

            static class TestClass
            {
                static string ReadChannel() => "stable";

                static ReleaseClient TestMethod()
                {
                    return new ReleaseClient
                    {
                        Settings =
                        {
                            Channel = ReadChannel(),
                            Retries = 2
                        }
                    };
                }
            }
            """);

        StringAssert.Contains(script, "ReadChannel", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Retries", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StructuralRecordIndexerInitializer_UsesTheBoundStringKeyAsAnObjectProperty()
    {
        var script = VisitBlock(
            """
            record ReleaseAttributes
            {
                public string? this[string key]
                {
                    get => null;
                    init { }
                }
            }

            static class TestClass
            {
                static ReleaseAttributes TestMethod()
                {
                    return new ReleaseAttributes
                    {
                        ["data-release"] = "stable"
                    };
                }
            }
            """);

        StringAssert.Contains(script, "data-release", StringComparison.Ordinal);
        StringAssert.Contains(script, "stable", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StructuralRecordCollectionInitializer_UsesTheBoundAddKeyValueContract()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections;

            record ReleaseHeaders : IEnumerable
            {
                public IEnumerator GetEnumerator() => Array.Empty<string>().GetEnumerator();

                public void Add(string key, string value) { }
            }

            static class TestClass
            {
                static ReleaseHeaders TestMethod()
                {
                    return new ReleaseHeaders
                    {
                        { "channel", "canary" }
                    };
                }
            }
            """);

        StringAssert.Contains(script, "channel", StringComparison.Ordinal);
        StringAssert.Contains(script, "canary", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StructuralRecordDynamicIndexerInitializer_RejectsTheUnsoundObjectLiteralFallback()
    {
        var block = GetBlock(
            """
            record ReleaseAttributes
            {
                public string? this[string key]
                {
                    get => null;
                    init { }
                }
            }

            static class TestClass
            {
                static ReleaseAttributes TestMethod(string key)
                {
                    return new ReleaseAttributes
                    {
                        [key] = "stable"
                    };
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "dynamic object key", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_InstanceMethodGroupWithEvaluatedReceiver_CachesTheReceiverBeforeBinding()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                sealed class Projector
                {
                    public int Project(int value) => value * 2;
                }

                static Projector ReadProjector() => new();

                static int TestMethod()
                {
                    Func<int, int> project = ReadProjector().Project;
                    return project(3);
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadProjector()"), script);
        StringAssert.Contains(script, ".bind(v$", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ExtensionMethodGroupWithEvaluatedReceiver_CachesTheReceiverBeforeBuildingTheProxy()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static IEnumerable<int> ReadValues() => new[] { 1, 2, 3 };

                static bool TestMethod()
                {
                    Func<int, bool> contains = ReadValues().Contains;
                    return contains(2);
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadValues()"), script);
        StringAssert.Contains(script, "contains(2)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ExtensionMethodGroupWithIdentifierReceiver_UsesTheBoundDelegateWithoutASetupSequence()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static bool TestMethod(IEnumerable<int> values)
                {
                    Func<int, bool> contains = values.Contains;
                    return contains(2);
                }
            }
            """);

        StringAssert.Contains(script, "values", StringComparison.Ordinal);
        StringAssert.Contains(script, "contains(2)", StringComparison.Ordinal);
        Assert.AreEqual(1, CountOccurrences(script, "values"), script);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_StaticExtensionMethodGroup_LeavesTheSourceSequenceInTheDelegateSignature()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static bool TestMethod(IEnumerable<int> values)
                {
                    Func<IEnumerable<int>, int, bool> contains = Enumerable.Contains;
                    return contains(values, 2);
                }
            }
            """);

        StringAssert.Contains(script, "contains(values, 2)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_BoundHostExtensionMethodGroup_PreservesTheReceiverForAliasDispatch()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseSet
            {
            }

            [ECMAScript]
            static class ReleaseSetExtensions
            {
                [ECMAScriptName("contains")]
                public static bool Contains(this ReleaseSet releases, int revision) => false;
            }

            static class TestClass
            {
                static ReleaseSet ReadReleases() => null!;

                static bool TestMethod()
                {
                    Func<int, bool> contains = ReadReleases().Contains;
                    return contains(7);
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadReleases()"), script);
        StringAssert.Contains(script, "ReleaseSetExtensions.contains", StringComparison.Ordinal);
        Assert.DoesNotContain("v$0$1", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_BoundExtensionMethodGroupHostRewrite_PreservesCreationTimeReceiverEvaluation()
    {
        var block = GetBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseSet
            {
            }

            [ECMAScript]
            static class ReleaseSetExtensions
            {
                [ECMAScriptName("contains")]
                public static bool Contains(this ReleaseSet releases, int revision) => false;
            }

            static class TestClass
            {
                static ReleaseSet ReadReleases() => null!;

                static bool TestMethod()
                {
                    Func<int, bool> contains = ReadReleases().Contains;
                    return contains(7);
                }
            }
            """);
        var host = new BoundExtensionMethodReferenceHost();

        var script = new SemanticWalker(true) { Host = host }
            .Visit(block, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.AreEqual(1, host.RewriteCount);
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadReleases()"), script);
        StringAssert.Contains(script, "hostContains", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_BoundCurrentModuleExtensionMethodGroup_CachesItsReceiverBeforeStaticDispatch()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public int Revision { get; init; }
            }

            [ECMAScript]
            static class ReleaseExtensions
            {
                public static bool Matches(this Release release, int revision)
                {
                    return release.Revision == revision;
                }
            }

            static class TestClass
            {
                static Release ReadRelease() => new();

                static bool TestMethod()
                {
                    Func<int, bool> matches = ReadRelease().Matches;
                    return matches(7);
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadRelease()"), script);
        StringAssert.Contains(script, "ReleaseExtensions.Matches", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_BoundCurrentModuleExtensionMethodGroup_WithIdentifierReceiverUsesNoSetupSequence()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public int Revision { get; init; }
            }

            [ECMAScript]
            static class ReleaseExtensions
            {
                public static bool Matches(this Release release, int revision)
                {
                    return release.Revision == revision;
                }
            }

            static class TestClass
            {
                static bool TestMethod(Release release)
                {
                    Func<int, bool> matches = release.Matches;
                    return matches(7);
                }
            }
            """);

        StringAssert.Contains(script, "ReleaseExtensions.Matches", StringComparison.Ordinal);
        Assert.DoesNotContain("TestClass.ReadRelease", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_StaticFieldTupleDeconstruction_UsesTheBoundStaticWriteTargets()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class ReleaseState
            {
                public static int Current;
                public static int Previous;
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    (ReleaseState.Current, ReleaseState.Previous) = (3, 2);
                }
            }
            """);

        StringAssert.Contains(script, "ReleaseState.Current", StringComparison.Ordinal);
        StringAssert.Contains(script, "ReleaseState.Previous", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NullInterfacePattern_FoldsToFalseWithoutAnErasedRuntimeHeuristic()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                sealed class DisposableRelease : IDisposable
                {
                    public void Dispose() { }
                }

                static bool TestMethod()
                {
                    DisposableRelease? release = default;
                    return release is IDisposable;
                }
            }
            """);

        StringAssert.Contains(script, "return false", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NonNullableValueInterfacePattern_FoldsToTrueWithoutABoxingProtocol()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                readonly struct DisposableRevision : IDisposable
                {
                    public void Dispose() { }
                }

                static bool TestMethod(DisposableRevision revision)
                {
                    return revision is IDisposable;
                }
            }
            """);

        StringAssert.Contains(script, "return true", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(revision) " + script);
    }

    [TestMethod]
    public void Visit_NullPattern_UsesTheDirectNullComparisonContract()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(string? value)
                {
                    return value is null;
                }
            }
            """);

        StringAssert.Contains(script, "value == null", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_ExplicitComputedPropertyAliases_UseBoundNumericAndStringKeys()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseMetadata
            {
                [ECMAScriptName("[1]")]
                public string? Primary { get; }

                [ECMAScriptName("['data-release']")]
                public string? Channel { get; }
            }

            static class TestClass
            {
                static string? TestMethod(ReleaseMetadata metadata)
                {
                    return metadata.Primary ?? metadata.Channel;
                }
            }
            """);

        StringAssert.Contains(script, "metadata[1]", StringComparison.Ordinal);
        StringAssert.Contains(script, "metadata[\"data-release\"]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(metadata) " + script);
    }

    [TestMethod]
    public void Visit_NonIdentifierPropertyAlias_UsesAQuotedPropertyAccess()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseMetadata
            {
                [ECMAScriptName("data-release")]
                public string? Channel { get; }
            }

            static class TestClass
            {
                static string? TestMethod(ReleaseMetadata metadata)
                {
                    return metadata.Channel;
                }
            }
            """);

        StringAssert.Contains(script, "metadata[\"data-release\"]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(metadata) " + script);
    }

    [TestMethod]
    public void Visit_EcmascriptPreserveAttribute_DoesNotExpandTheBoundArrayArgument()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserTelemetry
            {
                public static void Report([Preserve] params string[] messages) { }
            }

            static class TestClass
            {
                static void TestMethod(string[] messages)
                {
                    BrowserTelemetry.Report(messages);
                }
            }
            """);

        StringAssert.Contains(script, "BrowserTelemetry.Report(messages)", StringComparison.Ordinal);
        Assert.DoesNotContain("...messages", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(messages) " + script);
    }

    [TestMethod]
    public void Visit_UnsignedHexInterpolation_UsesTheUnsignedNumberFormattingPath()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(uint revision)
                {
                    return $"release:{revision:x}";
                }
            }
            """);

        StringAssert.Contains(script, "toString(16)", StringComparison.Ordinal);
        StringAssert.Contains(script, "toLowerCase()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(revision) " + script);
    }

    [TestMethod]
    public void Visit_SignedUppercaseHexInterpolation_UsesTheUnsignedBitPatternBeforeFormatting()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(int revision)
                {
                    return $"release:{revision:X}";
                }
            }
            """);

        StringAssert.Contains(script, ">>> 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "toUpperCase()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(revision) " + script);
    }

    [TestMethod]
    public void Visit_UnsupportedNumericInterpolationFormat_ReportsTheMissingRuntimeContract()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static string TestMethod(uint revision)
                {
                    return $"release:{revision:D}";
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "requires a supported CLR mapping", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_LongHexInterpolation_ReportsTheUnsupportedNumericRuntimeContract()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static string TestMethod(long revision)
                {
                    return $"release:{revision:X}";
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "requires a supported CLR mapping", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_RuntimeFieldsAndConstants_UseTheirBoundInstanceStaticAndConstContracts()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public int Revision;
            }

            [ECMAScript]
            static class ReleaseRuntime
            {
                public static int Current;
                public const string Channel = "stable";
            }

            static class TestClass
            {
                static string TestMethod(Release release)
                {
                    return release.Revision + ReleaseRuntime.Current + ReleaseRuntime.Channel;
                }
            }
            """);

        StringAssert.Contains(script, "release.Revision", StringComparison.Ordinal);
        StringAssert.Contains(script, "ReleaseRuntime.Current", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"stable\"", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_ExplicitComputedMemberAliases_PreserveNumericQuotedAndOrdinaryMemberKeys()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseHeaders
            {
                [ECMAScriptName("[0]")]
                public string First { get; set; } = string.Empty;

                [ECMAScriptName("['x-release']")]
                public string Release { get; set; } = string.Empty;

                [ECMAScriptName("[\"release-window\"]")]
                public string Window { get; set; } = string.Empty;

                [ECMAScriptName("[]")]
                public string EmptyKey { get; set; } = string.Empty;

                [ECMAScriptName("channel")]
                public string Channel { get; set; } = string.Empty;
            }

            static class TestClass
            {
                static string TestMethod(ReleaseHeaders headers)
                {
                    return headers.First + headers.Release + headers.Window + headers.EmptyKey + headers.Channel;
                }
            }
            """);

        StringAssert.Contains(script, "headers[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "headers[\"x-release\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "headers[\"release-window\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "headers[\"[]\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "headers.channel", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(headers) " + script);
    }

    [TestMethod]
    public void Visit_FieldAndParameterTupleSwap_CachesTheFieldValueBeforeItsWriteBack()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseState
            {
                public int Revision;
            }

            static class TestClass
            {
                static void TestMethod(ReleaseState state, int revision)
                {
                    (state.Revision, revision) = (revision, state.Revision);
                }
            }
            """);

        StringAssert.Contains(script, "state.Revision", StringComparison.Ordinal);
        StringAssert.Contains(script, "revision =", StringComparison.Ordinal);
        Assert.DoesNotContain("state.Revision = revision, revision = state.Revision", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(state, revision) " + script);
    }

    [TestMethod]
    public void Visit_CompoundIndexerWrites_CacheDynamicKeysAndReuseStableKeys()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseCounters
            {
                public int this[int index]
                {
                    get => 0;
                    set { }
                }
            }

            static class TestClass
            {
                static int NextIndex() => 1;

                static void TestMethod(ReleaseCounters counters, int index)
                {
                    counters[index] += 1;
                    counters[0] += 2;
                    counters[NextIndex()] += 3;
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.NextIndex()"), script);
        StringAssert.Contains(script, "counters[index]", StringComparison.Ordinal);
        StringAssert.Contains(script, "counters[0]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(counters, index) " + script);
    }

    [TestMethod]
    public void Visit_ErasedInterfacePatternWithNullableStaticType_PreservesTheNonNullFold()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                sealed class DisposableRelease : IDisposable
                {
                    public void Dispose() { }
                }

                static bool TestMethod(DisposableRelease? release)
                {
                    return release is IDisposable;
                }
            }
            """);

        StringAssert.Contains(script, "release != null", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_TypeOnlyPattern_PreservesTheRuntimeCheckWithoutDeclaringAValue()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(string value)
                {
                    return value is string;
                }
            }
            """);

        StringAssert.Contains(script, "typeof value", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_VarPattern_DeclaresTheBoundValueWithoutInventingATypeTest()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int value)
                {
                    return value is var captured ? captured : -1;
                }
            }
            """);

        StringAssert.Contains(script, "captured", StringComparison.Ordinal);
        Assert.DoesNotContain("typeof value", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_CustomListPattern_UsesTheBoundCountIndexerAndSliceMembers()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseList
            {
                public int Count => 0;

                public int this[int index] => 0;

                public ReleaseList Slice(int start, int length) => this;
            }

            static class TestClass
            {
                static bool TestMethod(ReleaseList releases)
                {
                    return releases is [var first, .. var middle, var last] &&
                        first <= last && middle.Count >= 0;
                }
            }
            """);

        StringAssert.Contains(script, ".Count", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Slice(", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(releases) " + script);
    }

    [TestMethod]
    public void Visit_EcmascriptTruthyOperator_UsesTheJavaScriptBooleanCoercionContract()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            readonly struct ReleaseGate
            {
                public static bool operator true(ReleaseGate value) => true;

                public static bool operator false(ReleaseGate value) => false;
            }

            static class TestClass
            {
                static int TestMethod(ReleaseGate gate)
                {
                    if (gate)
                        return 1;

                    return 0;
                }
            }
            """);

        StringAssert.Contains(script, "!(!gate)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(gate) " + script);
    }

    [TestMethod]
    public void Visit_ConditionalMethodAccess_CachesTheReceiverBeforeTheShortCircuitCall()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Release
            {
                public int ReadRevision() => 0;
            }

            static class TestClass
            {
                static int? TestMethod(Release? release)
                {
                    return release?.ReadRevision();
                }
            }
            """);

        StringAssert.Contains(script, "== null", StringComparison.Ordinal);
        StringAssert.Contains(script, "undefined", StringComparison.Ordinal);
        StringAssert.Contains(script, "ReadRevision", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_TypeParameterObjectCreation_RejectsTheMissingJavaScriptRuntimeConstructor()
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
        StringAssert.Contains(exception.Message, "runtime constructor binding", StringComparison.Ordinal);
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
            path: "SemanticWalkerBoundaryProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerBoundaryProtocolScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
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

    private sealed class BoundExtensionMethodReferenceHost : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public override Acornima.Ast.Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Acornima.Ast.Expression? instance)
        {
            if (operation.Method.Name != "Contains")
                return null;

            RewriteCount++;
            return new Acornima.Ast.Identifier("hostContains");
        }
    }
}
