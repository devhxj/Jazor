using Acornima;
using System.Collections.Immutable;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerLanguageProtocolScenarioTests
{
    [TestMethod]
    public void Visit_BitFlagAndCounterUpdates_PreservesEveryCompoundOperatorFamily()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static void TestMethod(int flags, int count, int shift, bool enabled)
                {
                    count += 1;
                    flags &= 7;
                    flags |= 8;
                    flags ^= 1;
                    flags <<= shift;
                    flags >>= shift;
                    flags >>>= shift;
                    uint unsignedFlags = 8;
                    unsignedFlags >>>= shift;
                    count -= 1;
                    count *= 2;
                    count /= 2;
                    count %= 3;
                    enabled &= count > 0;
                    enabled |= flags != 0;
                    enabled ^= shift == 0;
                }
            }
            """);

        StringAssert.Contains(script, "&=", StringComparison.Ordinal);
        StringAssert.Contains(script, "|=", StringComparison.Ordinal);
        StringAssert.Contains(script, "^=", StringComparison.Ordinal);
        StringAssert.Contains(script, "<<=", StringComparison.Ordinal);
        StringAssert.Contains(script, ">>>=", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(flags, count, shift, enabled) " + script);
    }

    [TestMethod]
    public void Visit_DefaultEnumWidths_UseNumberAndBigIntZeroCarriers()
    {
        var script = VisitBlock(
            """
            enum SignedByteState : sbyte { None }
            enum UnsignedShortState : ushort { None }
            enum UnsignedIntState : uint { None }
            enum UnsignedLongState : ulong { None }

            static class TestClass
            {
                static void TestMethod()
                {
                    SignedByteState signedByte = default;
                    UnsignedShortState unsignedShort = default;
                    UnsignedIntState unsignedInt = default;
                    UnsignedLongState unsignedLong = default;
                }
            }
            """);

        StringAssert.Contains(script, "let signedByte = 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "let unsignedShort = 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "let unsignedInt = 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "let unsignedLong = 0n", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_StringEnumMembers_EmitTheirConfiguredRuntimeValues()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [String]
            enum PublishState
            {
                Draft,
                Published
            }

            static class TestClass
            {
                static PublishState TestMethod(bool published)
                {
                    return published ? PublishState.Published : PublishState.Draft;
                }
            }
            """);

        StringAssert.Contains(script, "\"published\"", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "\"draft\"", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(published) " + script);
    }

    [TestMethod]
    public void Visit_TypeofMappedRuntimeTypes_UsesStableRuntimeTokens()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static void TestMethod()
                {
                    Type number = typeof(int);
                    Type text = typeof(string);
                    Type current = typeof(TestClass);
                }
            }
            """);

        StringAssert.Contains(script, "let number", StringComparison.Ordinal);
        StringAssert.Contains(script, "let text", StringComparison.Ordinal);
        StringAssert.Contains(script, "let current", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TypeofTypeWithSharedRuntimeAlias_RejectsAmbiguousTypeIdentity()
    {
        var block = GetBlock(
            """
            using System;
            using System.Threading.Tasks;

            static class TestClass
            {
                static Type TestMethod()
                {
                    return typeof(Task);
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "runtime alias 'Promise' is shared", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "precise runtime filtering", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ArrayListPatternWithSlice_PreservesBoundsAndDeclaredValues()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(int[] values)
                {
                    if (values is [var first, > 0, .. var middle, var last])
                        return first < last && middle.Length >= 0;

                    return false;
                }
            }
            """);

        StringAssert.Contains(script, ".length", StringComparison.Ordinal);
        StringAssert.Contains(script, "first", StringComparison.Ordinal);
        StringAssert.Contains(script, "middle", StringComparison.Ordinal);
        StringAssert.Contains(script, "last", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_PositionalRecordPatternWithoutMappedDeconstruct_RejectsAnUnownedProtocol()
    {
        var block = GetBlock(
            """
            using ECMAScript;

            [ECMAScript]
            record Release(string Name, int Priority);

            static class TestClass
            {
                static bool TestMethod(object value)
                {
                    return value is Release(var name, > 3) && name.Length > 0;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Release.Deconstruct", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Only whitelist members", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_InterfacePatterns_FoldsErasedContractsAndUsesMappedRuntimeChecks()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;

            sealed class Resource : IDisposable
            {
                public void Dispose() { }
            }

            static class TestClass
            {
                static bool TestMethod(Resource value)
                {
                    bool disposable = value is IDisposable;
                    bool enumerable = value is IEnumerable<int>;
                    return disposable && !enumerable;
                }
            }
            """);

        StringAssert.Contains(script, "value != null", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.isArray(value)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_ErasedInterfacePatternWithUnknownRuntimeValue_RejectsUnsoundFallback()
    {
        var block = GetBlock(
            """
            using System;

            static class TestClass
            {
                static bool TestMethod(object value)
                {
                    return value is IDisposable;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "interface", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(exception.Message, "IDisposable", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_VueDictionaryLiteralWithStringAndSymbolKeys_UsesObjectAndComputedMembers()
    {
        var script = VisitBlock(
            """
            using ECMAScript;
            using static ECMAScript.Vue;

            static class TestClass
            {
                static void TestMethod(Symbol key)
                {
                    var attributes = new VueDictionary<string>
                    {
                        ["data-release"] = "stable",
                        [key] = "canary"
                    };
                }
            }
            """);

        StringAssert.Contains(script, "data-release", StringComparison.Ordinal);
        StringAssert.Contains(script, "[key]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(key) " + script);
    }

    [TestMethod]
    public void Visit_NonStructuralObjectLiteralHost_UsesTheDeclaredNameBoundary()
    {
        var script = VisitBlock(
            """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            [Description("@#")]
            sealed class RuntimeAttributes
            {
                public string? Title { get; set; }

                public bool Hidden { get; set; }
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    var empty = new RuntimeAttributes();
                    var configured = new RuntimeAttributes
                    {
                        Title = "release",
                        Hidden = true
                    };
                }
            }
            """);

        StringAssert.Contains(script, "let empty = {}", StringComparison.Ordinal);
        StringAssert.Contains(script, "let configured = {", StringComparison.Ordinal);
        StringAssert.Contains(script, "Title: \"release\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "Hidden: true", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NonStructuralObjectLiteralHostWithConstructorArguments_RejectsRuntimeAllocation()
    {
        var block = GetBlock(
            """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            [Description("@#")]
            sealed class RuntimeAttributes
            {
                public RuntimeAttributes(string title)
                {
                }
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    _ = new RuntimeAttributes("release");
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "does not support constructor arguments", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_EcmascriptInlineAndParamsCalls_UsesTemplateAndSpreadProtocols()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserMath
            {
                [ECMAScriptInline("Math.max(__arg1, __arg2)")]
                public static int Max(int left, int right) => 0;

                public static void Report(params string[] messages) { }
            }

            static class TestClass
            {
                static void TestMethod(string[] messages)
                {
                    int largest = BrowserMath.Max(3, 5);
                    BrowserMath.Report(messages);
                }
            }
            """);

        StringAssert.Contains(script, "Math.max(3, 5)", StringComparison.Ordinal);
        StringAssert.Contains(script, "...messages", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(messages) " + script);
    }

    [TestMethod]
    public void Visit_EcmascriptParamsArrayAndCollectionExpressions_ExpandBoundElements()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserTelemetry
            {
                public static void Report(params string[] messages) { }
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    BrowserTelemetry.Report(new[] { "staged", "ready" });
                    BrowserTelemetry.Report(["released"]);
                }
            }
            """);

        StringAssert.Contains(script, "BrowserTelemetry.Report(\"staged\", \"ready\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "BrowserTelemetry.Report(\"released\")", StringComparison.Ordinal);
        Assert.DoesNotContain("[\"staged\", \"ready\"]", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ReorderedEcmascriptParams_PreservesSourceEvaluationBeforeSpread()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserTelemetry
            {
                public static void Record(int sequence, params string[] messages) { }
            }

            static class TestClass
            {
                static int NextSequence() => 3;

                static string[] BuildMessages() => ["staged", "ready"];

                static void TestMethod()
                {
                    BrowserTelemetry.Record(messages: BuildMessages(), sequence: NextSequence());
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.BuildMessages()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.NextSequence()"), script);
        StringAssert.Contains(script, "...v$", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_HostIntrinsicRewrite_ClaimsTheBoundInvocationBeforeFallbackDispatch()
    {
        var block = GetBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserRefresh
            {
                public static void Refresh(int revision) { }
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    BrowserRefresh.Refresh(42);
                }
            }
            """);
        var host = new IntrinsicClaimingHost();

        var script = new SemanticWalker(true) { Host = host }
            .Visit(block, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.AreEqual(1, host.InvocationCount);
        StringAssert.Contains(script, "hostRefresh(42)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_CompileMappedLinqMethodGroup_RemainsAValidBoundDelegateTarget()
    {
        var block = GetBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static void TestMethod()
                {
                    Func<IEnumerable<int>, Func<int, bool>, IEnumerable<int>> filter = Enumerable.Where;
                }
            }
            """);
        var methodReference = block.DescendantsAndSelf()
            .OfType<IMethodReferenceOperation>()
            .Single(static operation => operation.Method.Name == "Where");

        var expression = new SemanticWalker(true).VisitMethodReference(methodReference, new SenseArgument());

        Assert.IsNotNull(expression);
    }

    [TestMethod]
    public void Visit_MetadataEnumConstant_UsesTheScalarFieldFallbackWithoutMaterializingTheEnum()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static AttributeTargets TestMethod()
                {
                    return AttributeTargets.Class;
                }
            }
            """);

        StringAssert.Contains(script, "return 4", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NonModuleInitOnlyPropertyInitializer_UsesTheNormalObjectWritePath()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseOptions
            {
                public int MaxRetries { get; init; }
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    var options = new ReleaseOptions { MaxRetries = 3 };
                }
            }
            """);

        StringAssert.Contains(script, "MaxRetries", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TwoDimensionalIndexerWrite_RejectsTheUnsoundSingleIndexJavaScriptFallback()
    {
        var block = GetBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Matrix
            {
                public int this[int row, int column]
                {
                    get => 0;
                    set { }
                }
            }

            static class TestClass
            {
                static void TestMethod(Matrix matrix)
                {
                    matrix[1, 2] = 3;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "single translated index argument", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_LabelAndGoto_RejectsTheUnsupportedJavaScriptControlFlowModel()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static int TestMethod(int value)
                {
                    var count = 0;

                retry:
                    count++;
                    if (count < value)
                        goto retry;

                    return count;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Goto statements are not supported", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_UnmappedUserDefinedConversion_RejectsRawJavaScriptFallback()
    {
        var block = GetBlock(
            """
            readonly struct ReleaseNumber
            {
                public static explicit operator int(ReleaseNumber value) => 0;
            }

            static class TestClass
            {
                static int TestMethod(ReleaseNumber value)
                {
                    return (int)value;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Conversion operator", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "explicit whitelist/ECMAScript mapping", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_NonArrayListPatternWithSlice_CachesTheHostLengthAndUsesBoundMembers()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class RuntimeList
            {
                public int Length => 0;

                public int this[int index] => 0;

                public RuntimeList Slice(int start, int length) => this;
            }

            static class TestClass
            {
                static RuntimeList ReadValues() => null!;

                static bool TestMethod()
                {
                    return ReadValues() is [var first, .. var middle, var last] &&
                        first <= last && middle.Length >= 0;
                }
            }
            """);

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.ReadValues()"), script);
        StringAssert.Contains(script, "let ", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Slice(", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_DefaultReferenceConstrainedTypeParameter_UsesNullWithoutInventingAClrCarrier()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static T? TestMethod<T>() where T : class
                {
                    return default;
                }
            }
            """);

        StringAssert.Contains(script, "return null", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_DefaultUnconstrainedTypeParameter_RejectsAnUnsoundValueTypeFallback()
    {
        var block = GetBlock(
            """
            static class TestClass
            {
                static T TestMethod<T>()
                {
                    return default;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "default(T)", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "value type", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_InstanceLocalFunctionMethodGroup_UsesTheLexicalFunctionBinding()
    {
        var script = VisitBlock(
            """
            using System;

            sealed class TestClass
            {
                private readonly int _offset = 2;

                int TestMethod(int value)
                {
                    int AddOffset(int next) => _offset + next;
                    Func<int, int> transform = AddOffset;
                    return transform(value);
                }
            }
            """);

        StringAssert.Contains(script, "function AddOffset", StringComparison.Ordinal);
        StringAssert.Contains(script, "AddOffset.bind(this)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_EmptyEcmascriptInlineTemplate_FallsBackToTheBoundRuntimeMethod()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserProjector
            {
                [ECMAScriptInline("")]
                public static int Project(int value) => 0;
            }

            static class TestClass
            {
                static int TestMethod(int value)
                {
                    return BrowserProjector.Project(value);
                }
            }
            """);

        StringAssert.Contains(script, "BrowserProjector.Project(value)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_PropertyPatternWithNonIdentifierRuntimeName_UsesAComputedExistenceCheck()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseView
            {
                [ECMAScriptName("data-release")]
                public string? Name { get; }
            }

            static class TestClass
            {
                static bool TestMethod(ReleaseView release)
                {
                    return release is { Name: "stable" };
                }
            }
            """);

        StringAssert.Contains(script, "\"data-release\" in release", StringComparison.Ordinal);
        StringAssert.Contains(script, "release[\"data-release\"]", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_RuntimeOptionalParameters_PreserveBoundNamedArgumentsAndOmitTrailingDefaults()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class BrowserTelemetry
            {
                public static int Publish(string channel, int retryCount = 3, bool dryRun = false) => 0;
            }

            static class TestClass
            {
                static int TestMethod(string channel)
                {
                    int retry = BrowserTelemetry.Publish(channel, retryCount: 2);
                    return retry + BrowserTelemetry.Publish(channel);
                }
            }
            """);

        StringAssert.Contains(script, "Publish(channel, 2)", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Publish(channel)", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("publish(channel, 2, false)", script, StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(channel) " + script);
    }

    [TestMethod]
    public void Visit_AdjacentInterpolations_PreserveTemplateQuasiBoundaries()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(string channel, int revision)
                {
                    return $"{channel}{revision}";
                }
            }
            """);

        StringAssert.Contains(script, "${channel ?? \"\"}${revision}", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(channel, revision) " + script);
    }

    [TestMethod]
    public void Visit_TextOnlyInterpolatedString_UsesTheStableStringLiteralForm()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod()
                {
                    return $"release-ready";
                }
            }
            """);

        StringAssert.Contains(script, "\"release-ready\"", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_TupleAndEnumFields_PreserveIntrinsicScalarFallbacks()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static int TestMethod((int Revision, int RetryCount) release)
                {
                    int revision = release.Item1;
                    DateTimeKind kind = DateTimeKind.Utc;
                    return revision + (int)kind;
                }
            }
            """);

        StringAssert.Contains(script, "release", StringComparison.Ordinal);
        StringAssert.Contains(script, "let kind", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_TrailingListSlices_PreserveDiscardAndCaptureLengthProtocols()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(int[] values)
                {
                    return values is [var first, ..] &&
                        values is [.. var remaining] &&
                        first >= 0 &&
                        remaining.Length >= 0;
                }
            }
            """);

        StringAssert.Contains(script, ".slice(", StringComparison.Ordinal);
        StringAssert.Contains(script, "remaining", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_UnmatchedComputedAliasSyntax_RemainsAQuotedRuntimeKey()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseMetadata
            {
                [ECMAScriptName("[\"release']")]
                public string? State { get; set; }
            }

            static class TestClass
            {
                static string? TestMethod(ReleaseMetadata metadata)
                {
                    metadata.State = "ready";
                    return metadata.State;
                }
            }
            """);

        StringAssert.Contains(script, "release", StringComparison.Ordinal);
        StringAssert.Contains(script, "ready", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(metadata) " + script);
    }

    [TestMethod]
    public void Visit_TraditionalSwitchWithSharedLabels_AssignsTheBodyToTheLastLabel()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int phase)
                {
                    switch (phase)
                    {
                        case 0:
                        case 1:
                            return 10;
                        case 2:
                            return 20;
                        default:
                            return -1;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "case 0:", StringComparison.Ordinal);
        StringAssert.Contains(script, "case 1:", StringComparison.Ordinal);
        StringAssert.Contains(script, "default:", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(phase) " + script);
    }

    [TestMethod]
    public void Visit_DeconstructionWithDependentFieldPropertyAndParameterTargets_CachesBeforeWrites()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseState
            {
                public int Revision;

                public int RetryCount { get; set; }
            }

            static class TestClass
            {
                static int TestMethod(ReleaseState state, int left, int right)
                {
                    (left, right) = (right, left);
                    (state.Revision, state.RetryCount) = (state.RetryCount, state.Revision);
                    ((int current, int next), int later) = ((left, right), state.RetryCount);
                    return current + next + later + state.Revision + state.RetryCount;
                }
            }
            """);

        StringAssert.Contains(script, "state.Revision", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "state.RetryCount", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "v$0 = right, v$1 = left, left = v$0, right = v$1", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.Revision = v$2, state.RetryCount = v$3", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify(state, left, right) " + script);
    }

    [TestMethod]
    public void Visit_IndexAndRangeArrayAccess_PreservesBoundOffsetAndSliceSemantics()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static int TestMethod(int[] values)
                {
                    Index last = ^1;
                    Range interior = 1..^1;
                    int[] selected = values[interior];
                    return values[last] + selected.Length;
                }
            }
            """);

        StringAssert.Contains(script, "values.slice(", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Offset", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Length", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_NullCoalescingPropertyAssignment_UsesOneGetterAndOneSetterProtocol()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseMetadata
            {
                public string? State { get; set; }
            }

            static class TestClass
            {
                static string TestMethod(ReleaseMetadata metadata)
                {
                    metadata.State ??= "ready";
                    return metadata.State;
                }
            }
            """);

        StringAssert.Contains(script, "metadata.State", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "??", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(metadata) " + script);
    }

    [TestMethod]
    public void Visit_WhitelistedAliasMethod_UsesTheBoundRuntimeMemberName()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static int TestMethod(int[] values)
                {
                    Array.Fill(values, 7);
                    return values[0];
                }
            }
            """);

        StringAssert.Contains(script, "Array.fill(values, 7)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_UserDefinedTrueOperator_UsesJavaScriptBooleanCoercion()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseGate
            {
                public static bool operator true(ReleaseGate value) => true;

                public static bool operator false(ReleaseGate value) => false;
            }

            static class TestClass
            {
                static bool TestMethod(ReleaseGate gate)
                {
                    return gate ? true : false;
                }
            }
            """);

        StringAssert.Contains(script, "!(!gate)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(gate) " + script);
    }

    [TestMethod]
    public void Visit_IfWithoutElse_PreservesTheAbsentAlternateBranch()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int revision)
                {
                    if (revision < 0)
                        return 0;

                    return revision;
                }
            }
            """);

        StringAssert.Contains(script, "if (revision < 0)", StringComparison.Ordinal);
        Assert.DoesNotContain("else", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(revision) " + script);
    }

    [TestMethod]
    public void Visit_UserDefinedIncrementOnProperty_PreservesPrefixAndPostfixWriteBack()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Revision
            {
                public static Revision operator ++(Revision value) => value;
            }

            [ECMAScript]
            sealed class ReleaseState
            {
                public Revision Current { get; set; } = null!;
            }

            static class TestClass
            {
                static Revision TestMethod(ReleaseState state)
                {
                    Revision previous = state.Current++;
                    return ++state.Current;
                }
            }
            """);

        StringAssert.Contains(script, "state.Current", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "previous", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(state) " + script);
    }

    [TestMethod]
    public void Visit_DefaultTupleValue_MaterializesEveryNestedScalarSlot()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod()
                {
                    (int Revision, (bool Active, long Total) Meta) snapshot = default;
                    return snapshot.Revision + (snapshot.Meta.Active ? 1 : 0) + (int)snapshot.Meta.Total;
                }
            }
            """);

        StringAssert.Contains(script, "Revision: 0", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Active: false", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Total: 0n", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_UnmappedCustomCompoundAssignment_RejectsRawJavaScriptFallback()
    {
        var exception = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            sealed class Revision
            {
                public static Revision operator +(Revision left, Revision right) => left;
            }

            static class TestClass
            {
                static void TestMethod(Revision current, Revision delta)
                {
                    current += delta;
                }
            }
            """));

        StringAssert.Contains(exception.Message, "requires an explicit whitelist mapping", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_UnmappedCustomUnaryOperator_RejectsRawJavaScriptFallback()
    {
        var exception = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            sealed class Revision
            {
                public static Revision operator -(Revision value) => value;
            }

            static class TestClass
            {
                static Revision TestMethod(Revision value)
                {
                    return -value;
                }
            }
            """));

        StringAssert.Contains(exception.Message, "requires an explicit whitelist/ECMAScript mapping", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_BaseFieldAccess_RejectsPrototypeUnsafeStateProjection()
    {
        var exception = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            class BaseRelease
            {
                protected int Revision;
            }

            class Release : BaseRelease
            {
                int TestMethod()
                {
                    return base.Revision;
                }
            }
            """));

        StringAssert.Contains(exception.Message, "Base field access", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DefaultUnsupportedValueType_RejectsInventedJavaScriptRepresentation()
    {
        var exception = Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            struct RevisionToken
            {
                public int Value;
            }

            static class TestClass
            {
                static RevisionToken TestMethod()
                {
                    return default;
                }
            }
            """));

        StringAssert.Contains(exception.Message, "cannot be used for default value", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExternalLegacyUnionProjection_RejectsTheUnownedRuntimeProtocol()
    {
        var block = GetExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static string? TestMethod(LegacyRuntimeUnion value)
                {
                    return value.AsString;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External member 'ExternalContracts.LegacyRuntimeUnion.AsString.get' is not supported", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "property access", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExternalLegacyUnionProjectionPattern_RejectsTheUnownedRuntimeProtocol()
    {
        var block = GetExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static bool TestMethod(LegacyRuntimeUnion value)
                {
                    return value is { AsString: "release" };
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External member 'ExternalContracts.LegacyRuntimeUnion.AsString.get' is not supported", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "pattern property access", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExternalMarkedGenericStaticHost_UsesTheDeclaredRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return RuntimeGeneric<string>.Revision;
                }
            }
            """);

        StringAssert.Contains(script, "RuntimeGeneric.revision", StringComparison.OrdinalIgnoreCase);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ExternalUnmarkedSelfTypedGenericStaticHost_RejectsAnUnboundRuntimeConstructor()
    {
        var block = GetExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return PlainRuntime.Revision;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External type 'ExternalContracts.PlainRuntime' is not supported", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "field access", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExternalMarkedSelfTypedGenericStaticHost_RecoversTheConcreteRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using System;
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    Func<int> readRevision = MarkedRuntime.ReadRevision;
                    return MarkedRuntime.Revision + MarkedRuntime.RetryCount + readRevision();
                }
            }
            """);

        StringAssert.Contains(script, "MarkedRuntime.revision", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "MarkedRuntime.retryCount", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "MarkedRuntime.readRevision", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GenericRuntime", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_ExternalOpenGenericStaticHost_UsesTheErasedGenericRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod<T>()
                {
                    return RuntimeGeneric<T>.Revision;
                }
            }
            """);

        StringAssert.Contains(script, "RuntimeGeneric.revision", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_UnmarkedSelfTypedGenericStaticHost_RejectsTheUnownedConstructedHost()
    {
        var block = GetExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return GenericRuntime<PlainRuntime>.Revision;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External type 'ExternalContracts.GenericRuntime<TSelf>' is not supported", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "field access", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_MarkedGenericStaticHostWithInheritedSelfMarker_UsesTheConcreteRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return MarkedGenericRuntime<PlainMarkedRuntime>.Revision;
                }
            }
            """);

        StringAssert.Contains(script, "PlainMarkedRuntime.revision", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MarkedGenericRuntime", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_GenericStaticHostWithOrdinaryConstraint_UsesTheErasedDeclaredRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return ConstraintRuntime<int>.Revision;
                }
            }
            """);

        StringAssert.Contains(script, "ConstraintRuntime.revision", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_MarkedInheritedStaticPropertyAndMethod_UseTheConcreteRuntimeConstructor()
    {
        var script = VisitExternalBlock(
            """
            using ExternalContracts;

            static class TestClass
            {
                static int TestMethod()
                {
                    return MarkedRuntime.RetryCount + MarkedRuntime.ReadRevision();
                }
            }
            """);

        StringAssert.Contains(script, "MarkedRuntime.retryCount", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "MarkedRuntime.readRevision()", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GenericRuntime", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    private static string VisitBlock(string source)
    {
        var block = GetBlock(source);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }

    private static string VisitExternalBlock(string source)
    {
        var block = GetExternalBlock(source);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }

    private static IBlockOperation GetBlock(string source)
        => GetBlock(source, []);

    private static IBlockOperation GetExternalBlock(string source)
        => GetBlock(source, [CreateExternalContractsReference()]);

    private static IBlockOperation GetBlock(string source, ImmutableArray<MetadataReference> additionalReferences)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerLanguageProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerLanguageProtocolScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location))
                .AddRange(additionalReferences),
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

    private static MetadataReference CreateExternalContractsReference()
    {
        const string source = """
            using ECMAScript;

            namespace System.Runtime.CompilerServices
            {
                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            namespace ExternalContracts
            {
                [ECMAScript]
                public readonly struct LegacyRuntimeUnion : System.Runtime.CompilerServices.IUnion
                {
                    public string? AsString => default;

                    public object? Value => default;
                }

                [ECMAScript]
                public class RuntimeGeneric<T>
                {
                    public static int Revision;
                }

                [ECMAScript]
                public abstract class MarkedGenericRuntime<TSelf>
                    where TSelf : MarkedGenericRuntime<TSelf>
                {
                    public static int Revision;
                }

                public sealed class PlainMarkedRuntime : MarkedGenericRuntime<PlainMarkedRuntime>
                {
                }

                [ECMAScript]
                public class ConstraintRuntime<T>
                    where T : System.IComparable<T>
                {
                    public static int Revision;
                }

                public abstract class GenericRuntime<TSelf>
                    where TSelf : GenericRuntime<TSelf>
                {
                    public static int Revision;

                    public static int RetryCount { get; set; }

                    public static int ReadRevision() => Revision;
                }

                public sealed class PlainRuntime : GenericRuntime<PlainRuntime>
                {
                }

                [ECMAScript]
                public sealed class MarkedRuntime : GenericRuntime<MarkedRuntime>
                {
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "ExternalContracts.cs");
        var compilation = CSharpCompilation.Create(
            "ExternalContracts_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var emitResult = compilation.Emit(image);
        Assert.IsTrue(
            emitResult.Success,
            string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return MetadataReference.CreateFromImage(ImmutableArray.CreateRange(image.ToArray()));
    }

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

    private sealed class IntrinsicClaimingHost : SemanticWalkerHost
    {
        public int InvocationCount { get; private set; }

        public override Acornima.Ast.Expression? RewriteInvocationIntrinsic(
            IInvocationOperation operation,
            Acornima.Ast.Expression? instance,
            IReadOnlyList<Acornima.Ast.Expression> arguments,
            SemanticInvocationLoweringContext context)
        {
            if (operation.TargetMethod.Name != "Refresh")
                return null;

            InvocationCount++;
            return new Acornima.Ast.CallExpression(
                new Acornima.Ast.Identifier("hostRefresh"),
                Acornima.Ast.NodeList.From(arguments),
                optional: false);
        }
    }
}
