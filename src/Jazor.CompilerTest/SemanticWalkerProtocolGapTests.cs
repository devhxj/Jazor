using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerProtocolGapTests
{
    [TestMethod]
    public void VisitLock_EmbeddedStatementBodyUsesTheNonBlockLoweringPath()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                void TestMethod(object gate)
                {
                    lock (gate)
                        Console.WriteLine("ready");
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "console.log(\"ready\")", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_NotEqualsPreservesTupleSlotComparison()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                bool TestMethod()
                {
                    var equal = (1, 2) == (1, 2);
                    var different = (1, 2) != (1, 3);
                    return equal && different;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "!==", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void VisitInterpolatedString_FormattableSourceTypeRejectsUnsupportedRuntimeDispatch()
    {
        var block = GetBlock(
            """
            using System;

            class FormattableValue : IFormattable
            {
                public string ToString(string? format, IFormatProvider? formatProvider)
                    => format ?? string.Empty;

                public override string ToString() => "value";
            }

            class TestClass
            {
                string TestMethod(FormattableValue value)
                {
                    return $"[{value:U}]";
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "FormattableValue", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "method invocation", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitSlicePattern_DirectlyHandlesEmptyAndBoundedSlicePatterns()
    {
        var emptyBlock = GetBlock(
            """
            using System;

            class TestClass
            {
                bool TestMethod(int[] values)
                {
                    return values is [..];
                }
            }
            """);
        var boundedBlock = GetBlock(
            """
            class TestClass
            {
                bool TestMethod(int[] values)
                {
                    return values is [.. var rest];
                }
            }
            """);

        var emptySlice = emptyBlock.DescendantsAndSelf()
            .OfType<ISlicePatternOperation>()
            .Single();
        var boundedSlice = boundedBlock.DescendantsAndSelf()
            .OfType<ISlicePatternOperation>()
            .Single();
        var walker = new SemanticWalker(true);

        Assert.IsNull(walker.VisitSlicePattern(emptySlice, new SenseArgument()));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.VisitSlicePattern(boundedSlice, new SenseArgument()));
        Assert.IsNotNull(walker.VisitSlicePattern(
            boundedSlice,
            new SenseArgument(PatternInput: new Acornima.Ast.Identifier("values"))));
    }

    [TestMethod]
    public void TranslateStatementSequence_EmptySequenceProducesNoSyntheticStatements()
    {
        var statements = new SemanticWalker(true)
            .TranslateStatementSequence(Array.Empty<IOperation>(), new SenseArgument());

        Assert.HasCount(0, statements);
    }

    [TestMethod]
    public void UnsupportedRange_WithReportCallbackPublishesLoweringDiagnostic()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                sealed class Buffer
                {
                    public int Length => 4;
                    public int this[int index]
                    {
                        get => index;
                        set { }
                    }

                    public int[] this[Range range] => [];
                }

                void TestMethod()
                {
                    var buffer = new Buffer();
                    var value = buffer[1..^1];
                }
            }
            """);
        var messages = new List<string?>();
        var walker = new SemanticWalker((_, message) => messages.Add(message));

        Assert.ThrowsExactly<OperationTransformationException>(() =>
            walker.Visit(block, new SenseArgument()));
        Assert.IsNotEmpty(messages);
        Assert.IsTrue(messages.Any(static message =>
            message?.Contains("Range", StringComparison.Ordinal) == true));
    }

    [TestMethod]
    public void VisitInterfacePattern_PatternVariableLocalKeepsUnknownRuntimeTypeExplicit()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                bool TestMethod(object candidate)
                {
                    if (candidate is object local)
                        return local is IComparable;

                    return false;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "source static type 'object'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitInterfacePattern_ForeachLocalKeepsUnknownRuntimeTypeExplicit()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                bool TestMethod(object[] values)
                {
                    foreach (object value in values)
                    {
                        if (value is IComparable)
                            return true;
                    }

                    return false;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "source static type 'object'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitInterfacePattern_LocalMutationFormsUseStaticValueTypeContract()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                bool TestMethod()
                {
                    int compound = 1;
                    compound += 2;
                    bool compoundResult = compound is IComparable;

                    int incremented = 1;
                    incremented++;
                    bool incrementResult = incremented is IComparable;

                    int deconstructed = 1;
                    (deconstructed, _) = (2, 3);
                    bool deconstructionResult = deconstructed is IComparable;

                    return compoundResult && incrementResult && deconstructionResult;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let compoundResult = true", StringComparison.Ordinal);
        StringAssert.Contains(script, "let incrementResult = true", StringComparison.Ordinal);
        StringAssert.Contains(script, "let deconstructionResult = true", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitInterfacePattern_UnrelatedArgumentWritesDoNotInvalidateInitializerProof()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                void Rewrite(out object value) => value = new object();
                void Observe(object value) { }

                bool TestMethod()
                {
                    object candidate = "ready";
                    object unrelated;
                    Rewrite(out unrelated);
                    Observe(unrelated);
                    return candidate is IComparable;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return true", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitDeconstruction_StaticFieldsAndParametersUseTheirBoundTargets()
    {
        var block = GetBlock(
            """
            class TestClass
            {
                private static int _left;
                private static int _right;

                int TestMethod((int First, int Second) pair, int left, int right)
                {
                    (_left, _right) = pair;
                    (left, right) = pair;
                    return _left + _right + left + right;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "TestClass._left = pair.First", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "left = pair.First", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "right = pair.Second", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void VisitDeconstruction_ExtensionMethodReportsUnsupportedRuntimeReceiverProtocol()
    {
        var block = GetBlock(
            """
            class Point { }

            static class PointExtensions
            {
                public static void Deconstruct(this Point value, out int x, out int y)
                {
                    x = 1;
                    y = 2;
                }
            }

            class TestClass
            {
                int TestMethod(Point point)
                {
                    var (x, y) = point;
                    return x + y;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Extension Deconstruct", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitDeconstruction_StructMethodReportsMissingRuntimeDeclarationProtocol()
    {
        var block = GetBlock(
            """
            readonly struct Point
            {
                public void Deconstruct(out int x, out int y)
                {
                    x = 1;
                    y = 2;
                }
            }

            class TestClass
            {
                int TestMethod(Point point)
                {
                    var (x, y) = point;
                    return x + y;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "struct runtime declarations are not emitted", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitArrayRank_MultidimensionalArrayUsesBoundConstantRank()
    {
        var block = GetBlock(
            """
            class TestClass
            {
                int TestMethod(int[,,] values)
                {
                    return values.Rank;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return 3", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitNumericToString_HexCaseUsesBoundIntAndUIntIntrinsics()
    {
        var block = GetBlock(
            """
            class TestClass
            {
                string TestMethod(int signed, uint unsigned)
                {
                    string upper = signed.ToString("X");
                    string lower = unsigned.ToString("x");
                    return upper + lower;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "toUpperCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "toLowerCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "signed >>> 0", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitNumericToString_NonHexFormatReportsTheUnsupportedRuntimeContract()
    {
        var block = GetBlock(
            """
			class TestClass
			{
				string TestMethod(int value)
				{
					return value.ToString("D");
				}
			}
			""");

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "ToString", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "method invocation", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitMaterializedIndexAndRange_UsesClrCarrierMappingsAtValueBoundaries()
    {
        var block = GetBlock(
            """
            using System;

            class TestClass
            {
                int TestMethod()
                {
                    Index tail = ^2;
                    Range all = ..;
                    Range from = 1..;
                    Range until = ..^1;
                    Range middle = 1..^1;
                    return tail.Value + all.Start.Value + from.Start.Value + until.End.Value + middle.End.Value;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "_ce8b9229a41c8545(2)", StringComparison.Ordinal);
        StringAssert.Contains(script, "_c6ec2b575aff2e24()", StringComparison.Ordinal);
        StringAssert.Contains(script, "_0ba7c760bb17a58f()", StringComparison.Ordinal);
    }

    [TestMethod]
    public void VisitImplicitIndexer_DirectAndMaterializedBoundariesPreserveSliceProtocol()
    {
        var block = GetBlock(
            """
            using System;

            [global::ECMAScript.ECMAScript]
            class Buffer
            {
                public int Length => 8;
                public int this[int index] => index;
                public int[] Slice(int start, int length) => [];
            }

            class TestClass
            {
                int TestMethod(Buffer buffer, Index index, Range range)
                {
                    int tail = buffer[^1];
                    int materialized = buffer[index];
                    int[] all = buffer[..];
                    int[] middle = buffer[1..^1];
                    int[] stored = buffer[range];
                    return tail + materialized + all.Length + middle.Length + stored.Length;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, ".Slice", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "buffer.Length", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void VisitDeconstruction_ExistingTargetsSupportStructuralAndCustomProtocols()
    {
        var structuralBlock = GetBlock(
            """
			record Release(int Id, string Name);

			class TestClass
			{
				string TestMethod(Release release)
				{
					var (declaredId, declaredName) = release;
					int id = 0;
					string name = "";
					(id, name) = release;
					return declaredId + declaredName + id + name;
				}
			}
			""");
        var customBlock = GetBlock(
            """
			class Point
			{
				public void Deconstruct(out int x, out int y)
				{
					x = 1;
					y = 2;
				}
			}

			class TestClass
			{
				int TestMethod(Point point)
				{
					var (declaredX, declaredY) = point;
					int x = 0;
					int y = 0;
					(x, y) = point;
					return declaredX + declaredY + x + y;
				}
			}
			""");

        var structuralScript = new SemanticWalker(true).Visit(structuralBlock, new SenseArgument())?.ToKnRECMAScript();
        var customScript = new SemanticWalker(true).Visit(customBlock, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(structuralScript);
        Assert.IsNotNull(customScript);
        StringAssert.Contains(structuralScript, "id = release.Id", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(structuralScript, "name = release.Name", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(customScript, "point.Deconstruct", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(customScript, "x =", StringComparison.Ordinal);
        StringAssert.Contains(customScript, "y =", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + structuralScript);
        _ = new Parser().ParseScript("function verify() " + customScript);
    }

    [TestMethod]
    public void BoundDeconstruction_DeclarationAndAssignmentExposeTupleTargets()
    {
        var block = GetBlock(
            """
			class TestClass
			{
				void TestMethod((int Left, int Right) pair)
				{
					var (declaredLeft, declaredRight) = pair;
					int assignedLeft = 0;
					int assignedRight = 0;
					(assignedLeft, assignedRight) = pair;
				}
			}
			""");
        var assignments = block.DescendantsAndSelf()
            .OfType<IDeconstructionAssignmentOperation>()
            .ToArray();

        Assert.HasCount(2, assignments);
        Assert.IsInstanceOfType<IDeclarationExpressionOperation>(assignments[0].Target);
        Assert.IsInstanceOfType<ITupleOperation>(assignments[1].Target);
    }

    [TestMethod]
    public void VisitStringEnum_NullExplicitNameUsesTheEmptyWireValue()
    {
        var block = GetBlock(
            """
			using ECMAScript;

			[String]
			enum WireValue
			{
				[ECMAScriptName(null)]
				Empty
			}

			class TestClass
			{
				WireValue TestMethod()
				{
					return WireValue.Empty;
				}
			}
			""");

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return \"\"", StringComparison.Ordinal);
    }

    private static IBlockOperation GetBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerProtocolGapTests_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(
                MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
