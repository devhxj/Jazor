using System.Text.RegularExpressions;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRenderTreeBuilderHostTest
{
    [TestMethod]
    public void RewriteInvocation_MinimalElementSurface_EmitsRenderContextCalls()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "p");
                    builder.AddAttribute(1, "hidden");
                    builder.AddAttribute(2, "class", "lead");
                    builder.AddContent(3, "Hello");
                    var count = 42;
                    builder.AddContent(4, count);
                    builder.CloseElement();
                }
            }
            """);

        StringAssert.Contains(script, "builder.openElement(\"p\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addAttribute(\"hidden\", true);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addAttribute(\"class\", \"lead\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addContent(\"Hello\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addContent(count);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeElement();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("OpenElement", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddAttribute", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddContent", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CloseElement", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_ConditionalBranch_KeepsCallOrderInsideBranches()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, bool enabled)
                {
                    if (enabled)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, "on");
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddContent(2, "off");
                    }
                }
            }
            """);

        StringAssert.Contains(script, "if (enabled)", StringComparison.Ordinal);
        AssertCallOrder(
            script,
            "builder.openElement(\"span\");",
            "builder.addContent(\"on\");",
            "builder.closeElement();",
            "builder.addContent(\"off\");");
    }

    [TestMethod]
    public void RewriteInvocation_AttributeEventLambda_LowersValueThroughCompilerMainline()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void Increment()
                {
                }

                void TestMethod(RenderTreeBuilder builder)
                {
                    Action handler = () => Increment();
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", handler);
                    builder.CloseElement();
                }
            }
            """);

        StringAssert.Contains(script, "let handler = () => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "this.increment();", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addAttribute(\"onclick\", handler);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("RenderTreeBuilder", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_AddMultipleAttributes_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(
                    RenderTreeBuilder builder,
                    System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> attributes)
                {
                    builder.OpenElement(0, "button");
                    builder.AddMultipleAttributes(NextSequence(), attributes);
                    builder.CloseElement();
                }

                int NextSequence() => 1;
            }
            """);

        StringAssert.Contains(script, "builder.openElement(\"button\");", StringComparison.Ordinal);
        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1) => __rtb.addMultipleAttributes(__arg1))(builder, this.nextSequence(), attributes);",
            StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeElement();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddMultipleAttributes", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_SetKey_LowersToRenderContextKeyProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, int id)
                {
                    builder.OpenElement(0, "li");
                    builder.SetKey(id);
                    builder.AddContent(1, id);
                    builder.CloseElement();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"li\");",
            "builder.setKey(id);",
            "builder.addContent(id);",
            "builder.closeElement();");
        Assert.IsFalse(script.Contains("SetKey", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_SetUpdatesAttributeName_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "value", "ready");
                    builder.SetUpdatesAttributeName("value");
                    builder.CloseElement();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"input\");",
            "builder.addAttribute(\"value\", \"ready\");",
            "builder.setUpdatesAttributeName(\"value\");",
            "builder.closeElement();");
        Assert.IsFalse(script.Contains("SetUpdatesAttributeName", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_ErasedSequenceSideEffects_RunBeforeContentValues()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;

                string NextName() => "class";

                string NextValue() => "lead";

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddAttribute(NextSequence(), NextName(), NextValue());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1, __arg2) => __rtb.addAttribute(__arg1, __arg2))(builder, this.nextSequence(), this.nextName(), this.nextValue());",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_ComplexReceiverWithErasedSequence_EvaluatesReceiverOnceBeforeSequence()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                RenderTreeBuilder GetBuilder() => throw new Exception();

                int NextSequence() => 0;

                string NextName() => "p";

                void TestMethod()
                {
                    GetBuilder().OpenElement(NextSequence(), NextName());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1) => __rtb.openElement(__arg1))(this.getBuilder(), this.nextSequence(), this.nextName());",
            StringComparison.Ordinal);
        Assert.AreEqual(1, Regex.Matches(script, "this\\.getBuilder\\(\\)").Count, script);
    }

    [TestMethod]
    public void RewriteInvocation_RepeatedCompilation_EmitsStableTempNamesAndCallOrder()
    {
        const string code =
            """
            class TestClass
            {
                RenderTreeBuilder GetBuilder() => throw new Exception();

                int NextSequence() => 0;

                string NextName() => "button";

                string NextText() => "Count";

                void TestMethod()
                {
                    GetBuilder().OpenElement(NextSequence(), NextName());
                    GetBuilder().AddContent(NextSequence(), NextText());
                    GetBuilder().CloseElement();
                }
            }
            """;

        var first = CompileWithRenderTreeBuilderHost(code);
        var second = CompileWithRenderTreeBuilderHost(code);

        Assert.AreEqual(first, second);
        AssertCallOrder(
            first,
            "openElement",
            "addContent",
            "closeElement");
    }

    [TestMethod]
    public void RewriteInvocation_ConstantAddMarkupContent_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddMarkupContent(0, "<strong>raw</strong>");
                }
            }
            """);

        StringAssert.Contains(script, "builder.addMarkupContent(\"<strong>raw</strong>\");", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddMarkupContent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenRegion_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenRegion(0);
                    builder.AddContent(1, "region");
                    builder.CloseRegion();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openRegion();",
            "builder.addContent(\"region\");",
            "builder.closeRegion();");
        Assert.IsFalse(script.Contains("OpenRegion", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CloseRegion", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_RenderFragmentAddContent_InvokesFragmentAgainstCurrentBuilder()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    RenderFragment fragment = child =>
                    {
                        child.OpenElement(0, "span");
                        child.AddContent(1, "nested");
                        child.CloseElement();
                    };
                    builder.OpenElement(2, "div");
                    builder.AddContent(3, fragment);
                    builder.CloseElement();
                }
            }
            """);

        StringAssert.Contains(script, "builder.openElement(\"div\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "fragment(builder);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeElement();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddContent(3, fragment)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithModuleAttribute_LowersToDefaultImportAndProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, string title)
                {
                    builder.OpenComponent<Child>(0);
                    builder.AddComponentParameter(1, "Title", title);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "import ", StringComparison.Ordinal);
        StringAssert.Contains(script, "./components/child.mjs", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentParameter(\"Title\", title);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeComponent();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("OpenComponent", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddComponentParameter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CloseComponent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_RenderFragmentComponentParameter_LowersToNamedSlotProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    RenderFragment header = child =>
                    {
                        child.OpenElement(0, "h1");
                        child.AddContent(1, "Header");
                        child.CloseElement();
                    };
                    builder.OpenComponent<Child>(2);
                    builder.AddComponentParameter(3, "Header", header);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentSlot(\"Header\", header);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeComponent();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_GenericRenderFragmentComponentParameter_FailsWithTypedSlotDiagnostic()
    {
        var block = GetBlockOperation(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    RenderFragment<string> header = value => child =>
                    {
                        child.AddContent(0, value);
                    };
                    builder.OpenComponent<Child>(1);
                    builder.AddComponentParameter(2, "Header", header);
                    builder.CloseComponent();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "AddComponentParameter", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Header", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "RenderFragment<T>", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "typed slot descriptor", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithoutModuleAttribute_FailsWithActionableDiagnostic()
    {
        var block = GetBlockOperation(
            """
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Child>(0);
                    builder.CloseComponent();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "OpenComponent", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "ECMAScriptModule", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Child", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_DynamicAddMarkupContent_FailsBeforeArgumentLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                int NextSequence() => 0;

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddMarkupContent(NextSequence(), System.IO.File.ReadAllText("raw.html"));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMarkupContent(int, string)", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "render-context v1", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "constant AddMarkupContent", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_MarkupStringContent_FailsUntilRawMarkupSurfaceIsDesigned()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, new MarkupString("<strong>raw</strong>"));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, Microsoft.AspNetCore.Components.MarkupString)", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "render-context v1", StringComparison.Ordinal);
    }

    private static string CompileWithRenderTreeBuilderHost(string code)
    {
        var block = GetBlockOperation(code);
        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };
        var argument = new SenseArgument(UseImportAliases: true);
        var node = walker.Visit(block, argument);
        var body = node?.ToKnRECMAScript()?.ReplaceLineEndings("\n");
        Assert.IsNotNull(body);

        var imports = argument.FlushImportSpecifiers()
            .Select(static pair =>
            {
                var names = string.Join(
                    ", ",
                    pair.Value.Select(static specifier => specifier.ToECMAScript()));
                return "import " + names + " from \"" + pair.Key + "\";";
            });
        var script = string.Join("\n", imports.Concat([body!])).ReplaceLineEndings("\n");
        return script;
    }

    private static IBlockOperation GetBlockOperation(string code)
    {
        var usings =
            """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            global using Microsoft.AspNetCore.Components;
            global using Microsoft.AspNetCore.Components.Rendering;
            """;

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, TestMetadataReferences.PreviewParseOptions),
                CSharpSyntaxTree.ParseText(code, TestMetadataReferences.PreviewParseOptions)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join("\n", errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(static method => method.Identifier.ValueText == "TestMethod");

        return semanticModel.GetOperation(methodDeclaration.Body!) as IBlockOperation
            ?? throw new InvalidOperationException("Method body operation was not available.");
    }

    private static void AssertCallOrder(string script, params string[] snippets)
    {
        var lastIndex = -1;
        foreach (var snippet in snippets)
        {
            var index = script.IndexOf(snippet, StringComparison.Ordinal);
            Assert.IsTrue(index > lastIndex, $"Expected '{snippet}' after index {lastIndex} in:\n{script}");
            lastIndex = index;
        }
    }
}
