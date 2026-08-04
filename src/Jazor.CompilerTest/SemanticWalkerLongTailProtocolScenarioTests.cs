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

        StringAssert.Contains(script, "slots.left", StringComparison.Ordinal);
        StringAssert.Contains(script, "slots.right", StringComparison.Ordinal);
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
            "v$0 = slots.right, v$1 = slots.left, slots.left = v$0, slots.right = v$1",
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

        StringAssert.Contains(script, "toString(\"X\"", StringComparison.Ordinal);
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
        StringAssert.Contains(script, "tryRead", StringComparison.Ordinal);
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

    private static string VisitBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerLongTailProtocolScenario.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerLongTailProtocolScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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
