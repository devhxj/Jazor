using System.Text.RegularExpressions;
using System.Reflection;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class SemanticWalkerRenderTreeBuilderHostTest
{
    [TestMethod]
    public void RenderTreeBuilderPublicSurface_MatchesSupportedRenderContextHostSurface()
    {
        var actual = typeof(RenderTreeBuilder)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(GetMethodSurfaceSignature)
            .Concat(
                typeof(RenderTreeBuilder)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(static _ => "RenderTreeBuilder()"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
            {
                "AddAttribute(int, Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame)",
                "AddAttribute(int, string)",
                "AddAttribute(int, string, Microsoft.AspNetCore.Components.EventCallback)",
                "AddAttribute(int, string, object)",
                "AddAttribute(int, string, string)",
                "AddAttribute(int, string, System.MulticastDelegate)",
                "AddAttribute(int, string, bool)",
                "AddAttribute<TArgument>(int, string, Microsoft.AspNetCore.Components.EventCallback<TArgument>)",
                "AddComponentParameter(int, string, object)",
                "AddComponentReferenceCapture(int, System.Action<object>)",
                "AddComponentRenderMode(Microsoft.AspNetCore.Components.IComponentRenderMode)",
                "AddContent(int, Microsoft.AspNetCore.Components.MarkupString)",
                "AddContent(int, Microsoft.AspNetCore.Components.RenderFragment)",
                "AddContent(int, System.Nullable<Microsoft.AspNetCore.Components.MarkupString>)",
                "AddContent(int, object)",
                "AddContent(int, string)",
                "AddContent<TValue>(int, Microsoft.AspNetCore.Components.RenderFragment<TValue>, TValue)",
                "AddElementReferenceCapture(int, System.Action<Microsoft.AspNetCore.Components.ElementReference>)",
                "AddMarkupContent(int, string)",
                "AddMultipleAttributes(int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>)",
                "AddNamedEvent(string, string)",
                "Clear()",
                "CloseComponent()",
                "CloseElement()",
                "CloseRegion()",
                "Dispose()",
                "GetFrames()",
                "OpenComponent(int, System.Type)",
                "OpenComponent<TComponent>(int)",
                "OpenElement(int, string)",
                "OpenRegion(int)",
                "RenderTreeBuilder()",
                "SetAttributeValue(int, object)",
                "SetKey(object)",
                "SetUpdatesAttributeName(string)"
            }
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
    }

    [TestMethod]
    public void WebRenderTreeBuilderExtensionsPublicSurface_MatchesSupportedRenderContextHostSurface()
    {
        var actual = typeof(Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(static method => method.Name + "(" + string.Join(", ", method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType))) + ")")
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = new[]
            {
                "AddEventPreventDefaultAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)",
                "AddEventStopPropagationAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)"
            }
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
    }

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
    public void RewriteInvocation_AddAttributeAndAddContentOverloadMatrix_EmitsRenderContextCalls()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(
                    RenderTreeBuilder builder,
                    bool enabled,
                    string text,
                    object value,
                    MulticastDelegate handler,
                    EventCallback callback,
                    EventCallback<int> typedCallback)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "required");
                    builder.AddAttribute(2, "disabled", enabled);
                    builder.AddAttribute(3, "title", text);
                    builder.AddAttribute(4, "data-value", value);
                    builder.AddAttribute(5, "onclick", handler);
                    builder.AddAttribute(6, "onchange", callback);
                    builder.AddAttribute(7, "oninput", typedCallback);
                    builder.AddContent(8, text);
                    builder.AddContent(9, value);
                    builder.CloseElement();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"input\");",
            "builder.addAttribute(\"required\", true);",
            "builder.addAttribute(\"disabled\", enabled);",
            "builder.addAttribute(\"title\", text);",
            "builder.addAttribute(\"data-value\", value);",
            "builder.addAttribute(\"onclick\", handler);",
            "builder.addAttribute(\"onchange\", callback);",
            "builder.addAttribute(\"oninput\", typedCallback);",
            "builder.addContent(text);",
            "builder.addContent(value);",
            "builder.closeElement();");
        Assert.IsFalse(script.Contains("AddAttribute", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddContent", StringComparison.Ordinal), script);
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
        StringAssert.Contains(script, "this.Increment();", StringComparison.Ordinal);
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
            "((__rtb, __arg0, __arg1) => __rtb.addMultipleAttributes(__arg1))(builder, this.NextSequence(), attributes);",
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
    public void RewriteInvocation_AddNamedEvent_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "form");
                    builder.AddNamedEvent("onsubmit", "checkout");
                    builder.CloseElement();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"form\");",
            "builder.addNamedEvent(\"onsubmit\", \"checkout\");",
            "builder.closeElement();");
        Assert.IsFalse(script.Contains("AddNamedEvent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_AddNamedEvent_PreservesArgumentEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                string NextEventType() => "onsubmit";

                string NextAssignedName() => "checkout";

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddNamedEvent(NextEventType(), NextAssignedName());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "builder.addNamedEvent(this.NextEventType(), this.NextAssignedName());",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_EventModifiers_LowerExtensionMethodsToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "form");
                    builder.AddAttribute(1, "onsubmit", () => { });
                    builder.AddEventPreventDefaultAttribute(2, "onsubmit", true);
                    builder.AddEventStopPropagationAttribute(3, "onsubmit", true);
                    builder.CloseElement();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"form\");",
            "builder.addAttribute(\"onsubmit\", () => {",
            "builder.addEventPreventDefaultAttribute(\"onsubmit\", true);",
            "builder.addEventStopPropagationAttribute(\"onsubmit\", true);",
            "builder.closeElement();");
        Assert.IsFalse(script.Contains("AddEventPreventDefaultAttribute", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddEventStopPropagationAttribute", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_EventModifiers_PreserveErasedSequenceAndValueEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;

                string NextEventName() => "onclick";

                bool NextValue() => true;

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddEventPreventDefaultAttribute(NextSequence(), NextEventName(), NextValue());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__arg0, __arg1, __arg2, __arg3) => __arg0.addEventPreventDefaultAttribute(__arg2, __arg3))(builder, this.NextSequence(), this.NextEventName(), this.NextValue());",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_ReferenceCaptures_LowerToRenderContextProtocol()
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
                    object component = null;

                    builder.OpenElement(0, "input");
                    builder.AddElementReferenceCapture(1, value => { });
                    builder.CloseElement();

                    builder.OpenComponent<Child>(2);
                    builder.AddComponentReferenceCapture(3, value => component = value);
                    builder.CloseComponent();
                }
            }
            """);

        AssertCallOrder(
            script,
            "builder.openElement(\"input\");",
            "builder.addElementReferenceCapture(value => {",
            "builder.closeElement();",
            "builder.openComponent(",
            "builder.addComponentReferenceCapture(value => {",
            "component = value;",
            "builder.closeComponent();");
        Assert.IsFalse(script.Contains("AddElementReferenceCapture", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddComponentReferenceCapture", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_ElementReferenceCapture_PreservesErasedSequenceEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddElementReferenceCapture(NextSequence(), value => { });
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1) => __rtb.addElementReferenceCapture(__arg1))(builder, this.NextSequence(), value => {",
            StringComparison.Ordinal);
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
            "((__rtb, __arg0, __arg1, __arg2) => __rtb.addAttribute(__arg1, __arg2))(builder, this.NextSequence(), this.NextName(), this.NextValue());",
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
            "((__rtb, __arg0, __arg1) => __rtb.openElement(__arg1))(this.GetBuilder(), this.NextSequence(), this.NextName());",
            StringComparison.Ordinal);
        Assert.AreEqual(1, Regex.Matches(script, "this\\.GetBuilder\\(\\)").Count, script);
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
        StringAssert.Contains(script, "fragment?.(builder);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeElement();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddContent(3, fragment)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_NullableRenderFragmentPropertyAddContent_EmitsNullNoOp()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScript]
            class HeaderState
            {
                public RenderFragment? Extra { get; set; }
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, HeaderState header)
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, header.Extra);
                    builder.CloseElement();
                }
            }
            """);

        StringAssert.Contains(script, "header.Extra?.(builder);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("header.Extra(builder)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_GenericRenderFragmentAddContent_InvokesFragmentFactoryAgainstCurrentBuilder()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, string value)
                {
                    RenderFragment<string> template = item => child =>
                    {
                        child.OpenElement(0, "span");
                        child.AddContent(1, item);
                        child.CloseElement();
                    };
                    builder.OpenElement(2, "div");
                    builder.AddContent(3, template, value);
                    builder.CloseElement();
                }
            }
            """);

        StringAssert.Contains(script, "builder.openElement(\"div\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "template?.(value)?.(builder);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeElement();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddContent(3, template, value)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_GenericRenderFragmentAddContent_PreservesErasedSequenceAndValueEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;

                string NextValue() => "next";

                void TestMethod(RenderTreeBuilder builder)
                {
                    RenderFragment<string> template = item => child =>
                    {
                        child.AddContent(0, item);
                    };
                    builder.AddContent(NextSequence(), template, NextValue());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1, __arg2) => __arg1?.(__arg2)?.(__rtb))(builder, this.NextSequence(), template, this.NextValue());",
            StringComparison.Ordinal);
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
    public void RewriteInvocation_OpenComponentWithVueLibraryComponent_LowersToNamedImportAndProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScript.VueContract.VueLibraryComponent("tdesign-vue-next", "Layout")]
            class TLayout : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<TLayout>(0);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "import ", StringComparison.Ordinal);
        StringAssert.Contains(script, "from \"tdesign-vue-next\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "Layout", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("OpenComponent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentTypeOfWithVueLibraryComponent_LowersToNamedImportAndProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScript.VueContract.VueLibraryComponent("tdesign-vue-next", "Header")]
            class THeader : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenComponent(0, typeof(THeader));
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "from \"tdesign-vue-next\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "Header", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("OpenComponent", StringComparison.Ordinal), script);
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
    public void RewriteInvocation_GenericRenderFragmentComponentParameter_LowersToScopedSlotProtocol()
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

        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentScopedSlot(\"Header\", header);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeComponent();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_RenderFragmentComponentParameter_UsesMemberNameMap()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
                [Parameter]
                [ECMAScriptName("title")]
                public RenderFragment<string> TitleContent { get; set; }
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    RenderFragment<string> title = value => child =>
                    {
                        child.AddContent(0, value);
                    };
                    builder.OpenComponent<Child>(1);
                    builder.AddComponentParameter(2, "TitleContent", title);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "\"TitleContent\": \"title\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentScopedSlot(\"TitleContent\", title);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("titleContent", StringComparison.Ordinal), script);
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
    public void RewriteInvocation_OpenComponentWithLocalTypeOf_LowersToDefaultImportAndProtocol()
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
                    var childType = typeof(Child);
                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeComponent();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("OpenComponent", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithConvertedTypeOf_LowersToDefaultImportAndProtocol()
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
                    builder.OpenComponent(0, (Type)typeof(Child));
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.closeComponent();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("typeof", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithConvertedLocalTypeOf_LowersToDefaultImportAndProtocol()
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
                    Type childType = (Type)typeof(Child);
                    builder.OpenComponent(0, (Type)childType);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(script, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("typeof", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithNullType_FailsWithActionableDiagnostic()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.OpenComponent(0, null);
                    builder.CloseComponent();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "Dynamic Type OpenComponent", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithLocalTypeOf_PreservesErasedSequenceEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                int NextSequence() => 0;

                void TestMethod(RenderTreeBuilder builder)
                {
                    var childType = typeof(Child);
                    builder.OpenComponent(NextSequence(), childType);
                    builder.CloseComponent();
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0) => __rtb.openComponent(",
            StringComparison.Ordinal);
        StringAssert.Contains(script, ")(builder, this.NextSequence());", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithReassignedLocalType_FailsWithActionableDiagnostic()
    {
        var block = GetBlockOperation(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            [ECMAScriptModule("./components/other")]
            class Other : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    var childType = typeof(Child);
                    childType = typeof(Other);
                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "Dynamic Type OpenComponent", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_OpenComponentWithParameterType_FailsWithActionableDiagnostic()
    {
        var block = GetBlockOperation(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, Type childType)
                {
                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message, "Dynamic Type OpenComponent", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_DynamicAddMarkupContent_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;
                string ReadMarkup() => "<strong>raw</strong>";

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddMarkupContent(NextSequence(), ReadMarkup());
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1) => __rtb.addMarkupContent(__arg1))(builder, this.NextSequence(), this.ReadMarkup());",
            StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddMarkupContent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_MarkupStringContent_LowersConstantMarkupToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, new MarkupString("<strong>raw</strong>"));
                }
            }
            """);

        StringAssert.Contains(script, "builder.addMarkupContent(\"<strong>raw</strong>\");", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("MarkupString", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_MarkupStringContent_PreservesErasedSequenceEvaluation()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                int NextSequence() => 0;

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddContent(NextSequence(), new MarkupString("<em>raw</em>"));
                }
            }
            """);

        StringAssert.Contains(
            script,
            "((__rtb, __arg0, __arg1) => __rtb.addMarkupContent(__arg1))(builder, this.NextSequence(), \"<em>raw</em>\");",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteInvocation_DynamicMarkupStringContent_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                string ReadMarkup() => "<strong>raw</strong>";

                void TestMethod(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, new MarkupString(ReadMarkup()));
                }
            }
            """);

        StringAssert.Contains(script, "builder.addMarkupContent(this.ReadMarkup());", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("MarkupString", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_NullableMarkupStringContent_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            class TestClass
            {
                void TestMethod(RenderTreeBuilder builder, MarkupString? markup)
                {
                    builder.AddContent(0, markup);
                }
            }
            """);

        StringAssert.Contains(script, "builder.addMarkupContent(markup);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddContent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteInvocation_RemainingRenderTreeBuilderSurface_LowersToRenderContextProtocol()
    {
        var script = CompileWithRenderTreeBuilderHost(
            """
            [ECMAScriptModule("./components/child")]
            class Child : ComponentBase
            {
            }

            class TestClass
            {
                void TestMethod(
                    RenderTreeBuilder builder,
                    Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame frame,
                    IComponentRenderMode renderMode)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "value", "before");
                    builder.SetAttributeValue(2, "after");
                    builder.AddAttribute(3, frame);
                    builder.CloseElement();
                    builder.OpenComponent(2, typeof(Child));
                    builder.AddAttribute(3, "Title", "from attribute");
                    builder.AddComponentRenderMode(renderMode);
                    builder.CloseComponent();
                    builder.GetFrames();
                    builder.Clear();
                    builder.Dispose();
                    var nested = new RenderTreeBuilder();
                    nested.AddContent(0, "local");
                }
            }
            """);

        StringAssert.Contains(script, "builder.addAttributeFrame(frame);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.setAttributeValue(\"after\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addAttribute(\"Title\", \"from attribute\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentRenderMode(renderMode);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.getFrames();", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.clear();", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.dispose();", StringComparison.Ordinal);
        StringAssert.Contains(script, "@jazor/vue-runtime/render-context.mjs", StringComparison.Ordinal);
        StringAssert.Contains(script, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "let nested = createRenderContext(h);", StringComparison.Ordinal);
        StringAssert.Contains(script, "nested.addContent(\"local\");", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("AddComponentRenderMode", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("GetFrames", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("new RenderTreeBuilder", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("from \"./components/child\";", StringComparison.Ordinal), script);
    }

    private static string GetMethodSurfaceSignature(MethodInfo method)
    {
        var genericArguments = method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(static argument => argument.Name)) + ">"
            : string.Empty;
        var parameters = string.Join(
            ", ",
            method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType)));
        return method.Name + genericArguments + "(" + parameters + ")";
    }

    private static string GetTypeSurfaceName(Type type)
    {
        if (type.IsGenericParameter)
            return type.Name;

        if (!type.IsGenericType)
            return GetNonGenericTypeSurfaceName(type);

        var definition = type.GetGenericTypeDefinition();
        var definitionName = definition.FullName ?? definition.Name;
        var tickIndex = definitionName.IndexOf('`');
        if (tickIndex >= 0)
            definitionName = definitionName.Substring(0, tickIndex);

        var arguments = string.Join(
            ", ",
            type.GetGenericArguments().Select(static argument => GetTypeSurfaceName(argument)));
        return definitionName.Replace('+', '.') + "<" + arguments + ">";
    }

    private static string GetNonGenericTypeSurfaceName(Type type)
        => type == typeof(int)
            ? "int"
            : type == typeof(string)
                ? "string"
                : type == typeof(bool)
                    ? "bool"
                    : type == typeof(object)
                        ? "object"
                        : (type.FullName ?? type.Name).Replace('+', '.');

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
            global using Microsoft.AspNetCore.Components.Web;
            """;

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.VueLibraryComponentAttribute).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Components.Web.MouseEventArgs).Assembly.Location));
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
