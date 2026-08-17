using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerLoweringCoverageRegressionTests
{
    [TestMethod]
    public void Visit_ExtensionInvocation_UsesTheBoundStaticExtensionHost()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
            }

            [ECMAScript.ECMAScript]
            public static class Extensions
            {
                public static int Read(this Host host) => 1;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    Host host = new();
                    var value = host.Read();
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Read(host)");
        Assert.IsFalse(script.Contains("host.Read()", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Visit_PropertyAssignments_CoversBoundInstanceAndStaticTargets()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
                public static int StaticValue { get; set; }
                public int Value { get; set; }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    Host.StaticValue = 1;
                    var host = new Host();
                    host.Value = 2;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "StaticValue");
        StringAssert.Contains(script, "host.Value = 2");
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_WritesInstanceAndStaticFieldsThroughTheirBoundTargets()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                private int left;
                private int right;
                private static int staticLeft;
                private static int staticRight;

                void TestMethod()
                {
                    (left, right) = (1, 2);
                    (staticLeft, staticRight) = (3, 4);
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "left");
        StringAssert.Contains(script, "staticLeft");
    }

    [TestMethod]
    public void PrimaryConstructorInitializerDiscovery_CollectsOnlyInstanceFieldAndAutoPropertyInitializers()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public sealed class Primary(int seed)
                {
                    public int Field = seed;
                    public int Uninitialized;
                    public static int StaticField = 1;
                    public int Auto { get; } = seed;
                    public int Plain { get; }
                    public static int StaticAuto { get; } = 1;
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var primary = module.GetTypeMembers("Primary").Single();
        var converter = new AstConverter(module, model);
        var method = typeof(AstConverter).GetMethod(
            "GetPrimaryConstructorInitializers",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var result = ((System.Collections.IEnumerable)method.Invoke(converter, [primary])!)
            .Cast<object>()
            .ToArray();

        Assert.HasCount(2, result);
    }

    private static string VisitBlock(IBlockOperation block)
    {
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        _ = new Parser().ParseScript(first);
        return first;
    }

    private static IBlockOperation CreateBlock(string source)
    {
        var compilation = CreateCompilation(source);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerLoweringCoverage_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }
}
