using Acornima;
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

            static class TestClass
            {
                static Type TestMethod()
                {
                    return typeof(InvalidOperationException);
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "runtime alias 'Error' is shared", StringComparison.Ordinal);
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
            using static ECMAScript.Vue3;

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
        StringAssert.Contains(script, "title: \"release\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "hidden: true", StringComparison.Ordinal);
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

        StringAssert.Contains(script, "BrowserTelemetry.report(\"staged\", \"ready\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "BrowserTelemetry.report(\"released\")", StringComparison.Ordinal);
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

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.buildMessages()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "TestClass.nextSequence()"), script);
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

        StringAssert.Contains(script, "maxRetries", StringComparison.OrdinalIgnoreCase);
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

        Assert.AreEqual(1, CountOccurrences(script, "TestClass.readValues()"), script);
        StringAssert.Contains(script, "let ", StringComparison.Ordinal);
        StringAssert.Contains(script, ".slice(", StringComparison.Ordinal);
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

        StringAssert.Contains(script, "BrowserProjector.project(value)", StringComparison.Ordinal);
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
            path: "SemanticWalkerLanguageProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerLanguageProtocolScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location)),
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
