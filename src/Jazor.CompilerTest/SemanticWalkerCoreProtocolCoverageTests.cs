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
        StringAssert.Contains(script, "index, buffer.length", StringComparison.Ordinal);
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

        StringAssert.Contains(script, "TestClass.next()", StringComparison.Ordinal);
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

    private static string VisitBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerCoreProtocolCoverageTests",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }
}
