using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerLongTailProtocolScenarioTests
{
    [TestMethod]
    public void Visit_MemberFieldTupleSwap_CachesTheSourceBeforeWritingEitherField()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Slots
            {
                public int Left;
                public int Right;
            }

            static class TestClass
            {
                static void TestMethod(Slots slots)
                {
                    (slots.Left, slots.Right) = (slots.Right, slots.Left);
                }
            }
            """);

        StringAssert.Contains(script, "slots.Left", StringComparison.Ordinal);
        StringAssert.Contains(script, "slots.Right", StringComparison.Ordinal);
        Assert.DoesNotContain("slots.left = slots.right, slots.right = slots.left", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(slots) " + script);
    }

    [TestMethod]
    public void Visit_MemberPropertyTupleSwap_CachesGettersBeforeInvokingEitherSetter()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Slots
            {
                public int Left { get; set; }
                public int Right { get; set; }
            }

            static class TestClass
            {
                static void TestMethod(Slots slots)
                {
                    (slots.Left, slots.Right) = (slots.Right, slots.Left);
                }
            }
            """);

        StringAssert.Contains(
            script,
            "v$0 = slots.Right, v$1 = slots.Left, slots.Left = v$0, slots.Right = v$1",
            StringComparison.Ordinal);
        Assert.DoesNotContain("slots.left = slots.right, slots.right = slots.left", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(slots) " + script);
    }

    [TestMethod]
    public void Visit_FormattedEcmascriptHostInterpolation_UsesTheBoundFormattableMember()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseVersion : IFormattable
            {
                public override string ToString() => string.Empty;

                public string ToString(string? format, IFormatProvider? provider) => string.Empty;
            }

            static class TestClass
            {
                static string TestMethod(ReleaseVersion version)
                {
                    return $"release:{version:X}";
                }
            }
            """);

        StringAssert.Contains(script, "ToString(\"X\"", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(version) " + script);
    }

    [TestMethod]
    public void Visit_DefaultIfEmptyMethodGroup_UsesTheBoundDefaultValueRuntimeContract()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static IEnumerable<int> TestMethod(IEnumerable<int> values)
                {
                    Func<IEnumerable<int>, IEnumerable<int>> ensureValue = Enumerable.DefaultIfEmpty;
                    return ensureValue(values);
                }
            }
            """);

        StringAssert.Contains(script, "ensureValue(values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "defaultIfEmpty", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_TypedAndCatchAllClauses_PreserveTheirOrderedRuntimeChecks()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ClientFailure : Exception
            {
            }

            static class TestClass
            {
                static int TestMethod(ClientFailure error)
                {
                    try
                    {
                        throw error;
                    }
                    catch (ClientFailure invalid)
                    {
                        return invalid.Message.Length;
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "catch", StringComparison.Ordinal);
        StringAssert.Contains(script, "instanceof ClientFailure", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(error) " + script);
    }

    [TestMethod]
    public void Visit_OutDeclarationExpression_UsesTheBoundWriteBackIdentifier()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TryRead(string value, out int number)
                {
                    number = value.Length;
                    return number > 0;
                }

                static int TestMethod(string value)
                {
                    return TryRead(value, out var number) ? number : -1;
                }
            }
            """);

        StringAssert.Contains(script, "number", StringComparison.Ordinal);
        StringAssert.Contains(script, "TryRead", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_StaticContextLocalFunctionMethodGroup_UsesTheLexicalFunctionWithoutAnInventedReceiver()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static int TestMethod(int value)
                {
                    int Double(int input) => input * 2;
                    Func<int, int> transform = Double;
                    return transform(value);
                }
            }
            """);

        StringAssert.Contains(script, "function Double", StringComparison.Ordinal);
        StringAssert.Contains(script, "let transform = Double", StringComparison.Ordinal);
        Assert.DoesNotContain("Double.bind", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_StringPropertyPattern_DoesNotUseObjectExistenceChecksForScalarCarriers()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static bool TestMethod(string value)
                {
                    return value is { Length: > 3 };
                }
            }
            """);

        StringAssert.Contains(script, "value.length > 3", StringComparison.Ordinal);
        Assert.DoesNotContain("\"length\" in value", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_GenericInterfaceConstraintPattern_FoldsTheKnownNonNullContract()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static bool TestMethod<T>(T value) where T : class, IDisposable
                {
                    return value is IDisposable;
                }
            }
            """);

        StringAssert.Contains(script, "value != null", StringComparison.Ordinal);
        Assert.DoesNotContain("instanceof IDisposable", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + script);
    }

    [TestMethod]
    public void Visit_ExtensionMethodGroup_UsesTheBoundEnumerableDelegateContract()
    {
        var script = VisitBlock(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            static class TestClass
            {
                static bool TestMethod(IEnumerable<int> values, int expected)
                {
                    Func<int, bool> contains = values.Contains;
                    return contains(expected);
                }
            }
            """);

        StringAssert.Contains(script, "contains(expected)", StringComparison.Ordinal);
        StringAssert.Contains(script, "_e94a7db8306f4e71", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values, expected) " + script);
    }

    [TestMethod]
    public void Visit_TypedCatchClausesWithTheSameBindingName_HoistsOneSharedJavaScriptBinding()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseFailure : Exception
            {
            }

            static class TestClass
            {
                static int TestMethod(ReleaseFailure error, bool firstHandlerAccepts)
                {
                    try
                    {
                        throw error;
                    }
                    catch (ReleaseFailure failure) when (firstHandlerAccepts)
                    {
                        return 1;
                    }
                    catch (ReleaseFailure failure)
                    {
                        return failure.Message.Length;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "failure", StringComparison.Ordinal);
        StringAssert.Contains(script, "instanceof ReleaseFailure", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(error, firstHandlerAccepts) " + script);
    }

    [TestMethod]
    public void Visit_InterpolationOfInheritedSourceToString_UsesTheInheritedRuntimeContract()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            abstract class ReleaseIdentity
            {
                public override string ToString() => "release";
            }

            [ECMAScript]
            sealed class ReleaseTag : ReleaseIdentity
            {
            }

            static class TestClass
            {
                static string TestMethod(ReleaseTag tag)
                {
                    return $"tag:{tag}";
                }
            }
            """);

        StringAssert.Contains(script, ".toString()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(tag) " + script);
    }

    [TestMethod]
    public void Visit_NestedTupleForEachDeconstruction_UsesRecursiveArrayPatterns()
    {
        var script = VisitBlock(
            """
            using System.Collections.Generic;

            static class TestClass
            {
                static int TestMethod(IEnumerable<(int Left, (int First, int Second) Pair)> values)
                {
                    var total = 0;
                    foreach (var (left, (first, second)) in values)
                    {
                        total += left + first + second;
                    }

                    return total;
                }
            }
            """);

        StringAssert.Contains(script, "for (let { Left: left, Pair: { First: first, Second: second } } of values)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_ForLoopWithExpressionInitializer_PreservesTheOrderedInitializerSequence()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int maximum)
                {
                    var total = 0;
                    var index = -1;
                    for (total = 0, index = 0; index < maximum; index++)
                    {
                        total += index;
                    }

                    return total;
                }
            }
            """);

        StringAssert.Contains(script, "for (total = 0, index = 0;", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(maximum) " + script);
    }

    [TestMethod]
    public void Visit_StaticFieldTupleDeconstruction_UsesTheBoundStaticRuntimeHost()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class ReleaseTotals
            {
                public static int Published;
                public static int Queued;
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    (ReleaseTotals.Published, ReleaseTotals.Queued) =
                        (ReleaseTotals.Queued, ReleaseTotals.Published);
                }
            }
            """);

        StringAssert.Contains(script, "ReleaseTotals.Published", StringComparison.Ordinal);
        StringAssert.Contains(script, "ReleaseTotals.Queued", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_RuntimeCollectionInitializer_UsesTheDeclaredAddMemberContract()
    {
        var script = VisitBlock(
            """
            using ECMAScript;
            using System.Collections;
            using System.Collections.Generic;

            [ECMAScript]
            sealed class ReleaseLabels : IEnumerable<string>
            {
                public void Add(string label)
                {
                }

                public IEnumerator<string> GetEnumerator() => throw null!;

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            static class TestClass
            {
                static void TestMethod()
                {
                    var labels = new ReleaseLabels { "canary", "stable" };
                }
            }
            """);

        StringAssert.Contains(script, "new ReleaseLabels", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Add(\"canary\")", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Add(\"stable\")", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_SizeOfCarrierValue_RejectsAnInventedJavaScriptStorageLayout()
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            VisitBlock(
                """
                using System;

                static class TestClass
                {
                    static unsafe int TestMethod()
                    {
                        return sizeof(Half);
                    }
                }
                """,
                allowUnsafe: true));

        StringAssert.Contains(exception.Message, "sizeof is supported only", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ListPatternWithRangeIndexerProperty_RejectsTheUnsupportedSliceProtocol()
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            VisitBlock(
                """
                using System;
                using ECMAScript;

                [ECMAScript]
                sealed class ReleaseWindow
                {
                    public int Length => 0;

                    public int this[Index index] => index.Value;

                    public ReleaseWindow this[System.Range range] => this;
                }

                static class TestClass
                {
                    static bool TestMethod(ReleaseWindow values)
                    {
                        return values is [.. var remaining] && remaining.Length >= 0;
                    }
                }
                """));

        StringAssert.Contains(exception.Message, "Range-based slice property", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_InterpolationOfSourceTypeWithoutTextContract_RejectsObjectPrototypeFallback()
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            VisitBlock(
                """
                sealed class ReleaseCode
                {
                }

                static class TestClass
                {
                    static string TestMethod(ReleaseCode code)
                    {
                        return $"release:{code}";
                    }
                }
                """));

        StringAssert.Contains(exception.Message, "stable string conversion contract", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_InterpolationOfDynamicValue_RejectsTheMissingRuntimeTextContract()
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            VisitBlock(
                """
                static class TestClass
                {
                    static string TestMethod(dynamic value)
                    {
                        return $"release:{value}";
                    }
                }
                """));

        StringAssert.Contains(exception.Message, "stable string conversion contract", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_FilteredCatchWithoutDeclaredType_UsesTheSyntheticRethrowBinding()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(bool accepts)
                {
                    try
                    {
                        throw new System.Exception("release");
                    }
                    catch when (accepts)
                    {
                        return 1;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "catch", StringComparison.Ordinal);
        StringAssert.Contains(script, "throw", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(accepts) " + script);
    }

    [TestMethod]
    public void Visit_CustomListPatternWithSliceMethod_CachesLengthAndPreservesTrailingIndex()
    {
        var script = VisitBlock(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseWindow
            {
                public int Length => 0;

                public int this[Index index] => index.Value;

                public ReleaseWindow Slice(int start, int length) => this;
            }

            static class TestClass
            {
                static bool TestMethod(ReleaseWindow values)
                {
                    return values is [var first, .. var middle, var last] &&
                        first < last && middle is { Length: > 0 };
                }
            }
            """);

        StringAssert.Contains(script, ".Slice(1,", StringComparison.Ordinal);
        StringAssert.Contains(script, "v$0 - 1", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values) " + script);
    }

    [TestMethod]
    public void Visit_NullableValueAndDefaultValue_UseTheBoundClrContracts()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int? candidate)
                {
                    if (candidate.HasValue)
                        return candidate.Value;

                    return candidate.GetValueOrDefault(-1);
                }
            }
            """);

        StringAssert.Contains(script, "candidate !== null && candidate !== undefined", StringComparison.Ordinal);
        StringAssert.Contains(script, "nullable ?? defaultValue", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(candidate) " + script);
    }

    [TestMethod]
    public void Visit_StructuralInitializerWithNumberAndSymbolKeys_PreservesBothKeyKinds()
    {
        var script = VisitBlock(
            """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            [Description("@#")]
            sealed class ReleaseMetadata
            {
                public string this[Number key]
                {
                    set { }
                }

                public string this[Symbol key]
                {
                    set { }
                }
            }

            static class TestClass
            {
                static ReleaseMetadata TestMethod()
                {
                    return new ReleaseMetadata
                    {
                        [(Number)12] = "ready",
                        [Symbol.Iterator] = "iterable"
                    };
                }
            }
            """);

        StringAssert.Contains(script, "12", StringComparison.Ordinal);
        StringAssert.Contains(script, "Symbol.iterator", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_NestedEcmascriptStaticHostMember_UsesTheDeclaredTypePath()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class ReleaseHosts
            {
                [ECMAScript]
                public static class Dashboard
                {
                    public static int Published { get; }
                }
            }

            static class TestClass
            {
                static int TestMethod()
                {
                    return ReleaseHosts.Dashboard.Published;
                }
            }
            """);

        StringAssert.Contains(script, "Published", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_InterpolationWithoutTextSegment_PreservesTheConvertedExpression()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(int release)
                {
                    return $"{release}";
                }
            }
            """);

        StringAssert.Contains(script, "release", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    [TestMethod]
    public void Visit_GenericUsingDisposableConstraint_ResolvesTheInterfaceDisposeContract()
    {
        var script = VisitBlock(
            """
            using System;

            static class TestClass
            {
                static int TestMethod<TLease>(TLease lease)
                    where TLease : IDisposable
                {
                    using (lease)
                    {
                        return 1;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "finally", StringComparison.Ordinal);
        StringAssert.Contains(script, "lease !== null", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(lease) " + script);
    }

    [TestMethod]
    public void Visit_CustomEnumerableForeach_UsesItsDeclaredEnumeratorMembers()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class ReleaseEnumerator
            {
                public int Current { get; }

                public bool MoveNext() => false;
            }

            [ECMAScript]
            sealed class ReleaseSequence
            {
                public ReleaseEnumerator GetEnumerator() => throw null!;
            }

            static class TestClass
            {
                static int TestMethod(ReleaseSequence releases)
                {
                    var total = 0;
                    foreach (var release in releases)
                        total += release;
                    return total;
                }
            }
            """);

        StringAssert.Contains(script, "for (let release of releases)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(releases) " + script);
    }

    [TestMethod]
    public void Visit_EmptyAndFallthroughSwitches_PreserveTheSourceControlFlow()
    {
        var script = VisitBlock(
            """
            static class TestClass
            {
                static int TestMethod(int release)
                {
                    switch (release)
                    {
                    }

                    switch (release)
                    {
                        case 1:
                        case 2:
                            return 20;
                        default:
                            return 0;
                    }
                }
            }
            """);

        StringAssert.Contains(script, "case 1:", StringComparison.Ordinal);
        StringAssert.Contains(script, "case 2:", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(release) " + script);
    }

    private static string VisitBlock(string source, bool allowUnsafe = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerLongTailProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerLongTailProtocolScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: allowUnsafe));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }
}
