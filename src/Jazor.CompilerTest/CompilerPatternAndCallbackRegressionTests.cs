using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerPatternAndCallbackRegressionTests
{
    [TestMethod]
    public void OptionalHostCallback_NormalizesBothSidesAndCapturesFactoryOnce()
    {
        var script = Lower("""
            using System;
            using ECMAScript;

            [ECMAScript]
            class Host
            {
                public extern int Register([ECMAScriptOptionalCallback] Func<string?, string?> callback);
            }

            static class Consumer
            {
                static int Test(Host host, Func<Func<string?, string?>> factory)
                {
                    return host.Register(factory());
                }
            }
            """);

        StringAssert.Contains(script, "$value ?? null");
        StringAssert.Contains(script, "?? void 0");
        Assert.AreEqual(1, script.Split("factory()", StringSplitOptions.None).Length - 1, script);
        _ = new Parser().ParseScript("function test(host, factory) " + script);
    }

    [TestMethod]
    public void NullableValueThroughBoxing_FoldsInterfacePatternWithNullCheck()
    {
        var script = Lower("""
            using System;

            static class Consumer
            {
                static bool Test(int? item)
                {
                    object boxed = item;
                    return boxed is IComparable;
                }
            }
            """);

        StringAssert.Contains(script, "boxed != null");
        Assert.DoesNotContain("instanceof", script);
    }

    [TestMethod]
    public void CombinedPatternLabels_PreserveDefaultBranch()
    {
        var script = Lower("""
            static class Consumer
            {
                static int Test(int value)
                {
                    int result;
                    switch (value)
                    {
                        case > 10:
                        case < 0:
                            result = 1;
                            break;
                        default:
                            result = 2;
                            break;
                    }
                    return result;
                }
            }
            """);

        StringAssert.Contains(script, "||");
        StringAssert.Contains(script, "result = 1");
        StringAssert.Contains(script, "result = 2");
        _ = new Parser().ParseScript("function test(value) " + script);
    }

    [TestMethod]
    public void EnumerableToArray_UsesFreshHostArrayWithoutCopying()
    {
        var script = Lower("""
            using ECMAScript;
            using System.Collections.Generic;
            using System.Linq;

            [ECMAScript]
            static class Host
            {
                [ECMAScriptInline("Array.of(1, 2)")]
                public static extern IEnumerable<int> Fresh();
            }

            static class Consumer
            {
                static int[] Test()
                {
                    return Host.Fresh().ToArray();
                }
            }
            """);

        StringAssert.Contains(script, "Array.of(1, 2)");
        Assert.DoesNotContain("Array.from", script);
        _ = new Parser().ParseScript("function test() " + script);
    }

    private static string Lower(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("PatternAndCallbackRegression", [tree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(candidate => candidate.Identifier.ValueText == "Test");
        var operation = compilation.GetSemanticModel(tree).GetOperation(method.Body!);
        var script = new SemanticWalker(true).Visit(operation!, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }
}
