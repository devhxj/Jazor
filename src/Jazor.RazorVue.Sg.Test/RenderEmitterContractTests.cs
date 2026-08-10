using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RenderEmitterContractTests
{
    [TestMethod]
    public void TryEmit_RejectsNullRequiredArgumentsAndInvalidBuildRenderTreeSignature()
    {
        var fixture = CreateFixture();

        Assert.AreEqual(
            "compilation",
            Assert.Throws<ArgumentNullException>(() =>
                RenderEmitter.TryEmit(null!, null!, null!, null!, null, null!, out _, out _)).ParamName);
        Assert.AreEqual(
            "componentSymbol",
            Assert.Throws<ArgumentNullException>(() =>
                RenderEmitter.TryEmit(fixture.Compilation, null!, null!, null!, null, null!, out _, out _)).ParamName);
        Assert.AreEqual(
            "buildRenderTreeMethod",
            Assert.Throws<ArgumentNullException>(() =>
                RenderEmitter.TryEmit(fixture.Compilation, fixture.Component, null!, null!, null, null!, out _, out _)).ParamName);
        Assert.AreEqual(
            "buildRenderTreeBody",
            Assert.Throws<ArgumentNullException>(() =>
                RenderEmitter.TryEmit(fixture.Compilation, fixture.Component, fixture.Method, null!, null, null!, out _, out _)).ParamName);
        Assert.AreEqual(
            "injectRegistry",
            Assert.Throws<ArgumentNullException>(() =>
                RenderEmitter.TryEmit(fixture.Compilation, fixture.Component, fixture.Method, fixture.Body, null, null!, out _, out _)).ParamName);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "RazorVue direct render operation lowering requires BuildRenderTree(RenderTreeBuilder).",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersComponentSlotDirectInvokeAsVueSlotSequence()
    {
        var fixture = CreateDirectRenderFixture(
            "ChildContent.Invoke(builder);",
            "[Parameter] public RenderFragment? ChildContent { get; set; }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesSlots);
        StringAssert.Contains(
            result.RenderExpression.ToKnRECMAScript(),
            "slots.ChildContent",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsDynamicLocalRenderFragmentInvoke()
    {
        var fixture = CreateDirectRenderFixture(
            "RenderFragment fragment = CreateFragment(); fragment.Invoke(builder);",
            "private RenderFragment CreateFragment() { var prefix = \"dynamic\"; return child => child.AddContent(0, prefix); }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "RenderFragment.Invoke direct lowering requires a known inline, slot, or component-local RenderFragment source.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_MarksForeachWithMultipleRootsAsFragment()
    {
        var fixture = CreateDirectRenderFixture(
            "foreach (var item in Items) { builder.AddContent(0, \"first:\" + item); builder.AddContent(1, \"second:\" + item); }",
            "[Parameter] public string[] Items { get; set; } = [];");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesFragment);
        StringAssert.Contains(
            result.RenderExpression.ToKnRECMAScript(),
            "Array.from(props.Items ?? []",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_PreservesAggregateRenderHintsAfterPriorFragmentAndStaticOutput()
    {
        var fixture = CreateDirectRenderFixture(
            """
            if (Enabled)
            {
                builder.AddContent(0, "conditional-first");
                builder.AddContent(1, "conditional-second");
            }
            else
            {
                builder.AddContent(2, "conditional-fallback");
            }

            builder.AddMarkupContent(3, "<strong>static-before</strong>");
            RenderFragment fragment = child =>
            {
                child.AddContent(0, "fragment-first");
                child.AddContent(1, "fragment-second");
                child.AddMarkupContent(2, "<em>fragment-static</em>");
            };
            fragment.Invoke(builder);

            foreach (var item in Items)
            {
                builder.AddContent(0, "loop-first:" + item);
                builder.AddContent(1, "loop-second:" + item);
            }
            """,
            "[Parameter] public bool Enabled { get; set; } [Parameter] public string[] Items { get; set; } = [];");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesFragment);
        Assert.IsTrue(result.UsesStaticVNode);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "createStaticVNode", StringComparison.Ordinal);
        StringAssert.Contains(output, "Array.from(props.Items ?? []", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersStaticAndExpressionBodiedRenderFragmentMethodGroups()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, (RenderFragment)RenderStaticHeader); builder.AddContent(1, (RenderFragment)RenderExpressionHeader);",
            """
            private static void RenderStaticHeader(RenderTreeBuilder child) => child.AddContent(0, "static-method-group");
            private void RenderExpressionHeader(RenderTreeBuilder child) => child.AddContent(0, "expression-method-group");
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "static-method-group", StringComparison.Ordinal);
        StringAssert.Contains(output, "expression-method-group", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsRecursiveRenderFragmentMethodGroup()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, (RenderFragment)RecursiveHeader);",
            """
            private void RecursiveHeader(RenderTreeBuilder child)
            {
                RecursiveHeader(child);
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "Recursive RenderFragment method group 'RecursiveHeader' is not supported by direct render operation lowering.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersRenderFragmentPropertiesAcrossGetterShapes()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, ExpressionFragment); builder.AddContent(1, BlockFragment); builder.AddContent(2, StaticFragment);",
            """
            private RenderFragment ExpressionFragment => child => child.AddContent(0, "expression-property");

            private RenderFragment BlockFragment
            {
                get
                {
                    return child => child.AddContent(0, "block-property");
                }
            }

            private static RenderFragment StaticFragment => child => child.AddContent(0, "static-property");
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "expression-property", StringComparison.Ordinal);
        StringAssert.Contains(output, "block-property", StringComparison.Ordinal);
        StringAssert.Contains(output, "static-property", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsRecursiveRenderFragmentProperty()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, RecursiveFragment);",
            "private RenderFragment RecursiveFragment => RecursiveFragment;");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "Recursive RenderFragment property 'RecursiveFragment' is not supported by direct render operation lowering.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersObjectCarriedFragmentsFromConstructorInitializerAndHelpers()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var fromConstructor = CreateConstructorCarrier();
            var fromInitializer = CreateInitializerCarrier();
            builder.AddContent(0, fromConstructor.Header);
            builder.AddContent(1, fromInitializer.Header);
            """,
            """
            private sealed class FragmentCarrier
            {
                public FragmentCarrier()
                {
                }

                public FragmentCarrier(RenderFragment header)
                {
                    Header = header;
                }

                public RenderFragment Header { get; set; } = default!;
            }

            private FragmentCarrier CreateConstructorCarrier()
            {
                RenderFragment fragment = child => child.AddContent(0, "constructor-carrier");
                return new FragmentCarrier(fragment);
            }

            private FragmentCarrier CreateInitializerCarrier() => new FragmentCarrier
            {
                Header = child => child.AddContent(0, "initializer-carrier")
            };
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "constructor-carrier", StringComparison.Ordinal);
        StringAssert.Contains(output, "initializer-carrier", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_HoistsRecursiveRenderFragmentHelperWithItsCallArguments()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, RecursiveFragment(1));",
            """
            private RenderFragment RecursiveFragment(int depth) => child =>
            {
                child.AddContent(0, "recursive-depth:" + depth);
                if (depth > 0)
                {
                    child.AddContent(1, RecursiveFragment(depth - 1));
                }
            };
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = string.Join(
            Environment.NewLine,
            result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript()));
        StringAssert.Contains(output, "recursive-depth:", StringComparison.Ordinal);
        StringAssert.Contains(output, "depth - 1", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersExplicitlyConvertedLocalGenericRenderFragmentInvocation()
    {
        var fixture = CreateDirectRenderFixture(
            "RenderFragment<string> template = value => child => child.AddContent(0, \"converted-generic:\" + value); builder.AddContent(0, ((RenderFragment<string>)template).Invoke(Text));",
            "[Parameter] public string Text { get; set; } = \"\";");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "converted-generic:", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Text", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersStaticWebRenderTreeBuilderExtensionMetadataCalls()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", (System.Action)OnClick);
            Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
            Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(builder, 3, "onclick", false);
            builder.AddContent(4, "static-extension-metadata");
            builder.CloseElement();
            """,
            "private void OnClick() { }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "static-extension-metadata", StringComparison.Ordinal);
        StringAssert.Contains(output, "preventDefault", StringComparison.Ordinal);
        Assert.DoesNotContain("stopPropagation", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersInstanceAndExternalStaticBuilderHelpers()
    {
        var fixture = CreateDirectRenderFixture(
            "AddInstanceText(builder, \"instance-builder-helper\"); ExternalBuilderHelpers.AddStaticText(builder, \"static-builder-helper\");",
            """
            private void AddInstanceText(RenderTreeBuilder child, string text)
            {
                child.AddContent(0, text);
            }

            private static class ExternalBuilderHelpers
            {
                public static void AddStaticText(RenderTreeBuilder child, string text)
                {
                    child.AddContent(0, text);
                }
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "instance-builder-helper", StringComparison.Ordinal);
        StringAssert.Contains(output, "static-builder-helper", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersGenericComponentSlotsAndNullableMarkupContent()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, ItemTemplate, Text); builder.AddContent(1, Markup);",
            """
            [Parameter] public string Text { get; set; } = "";
            [Parameter] public RenderFragment<string>? ItemTemplate { get; set; }
            [Parameter] public MarkupString? Markup { get; set; }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesSlots);
        Assert.IsTrue(result.UsesStaticVNode);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "slots.ItemTemplate", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Markup", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_HoistsRecursiveGenericRenderFragmentHelpersWithScopedValues()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, RecursiveItemTemplate(1), Text);",
            """
            [Parameter] public string Text { get; set; } = "";

            private RenderFragment<string> RecursiveItemTemplate(int depth) => value => child =>
            {
                child.AddContent(0, "recursive-item:" + value + depth);
                if (depth > 0)
                {
                    child.AddContent(1, RecursiveItemTemplate(depth - 1), value);
                }
            };
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var prelude = string.Join(
            Environment.NewLine,
            result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript()));
        StringAssert.Contains(prelude, "recursive-item:", StringComparison.Ordinal);
        StringAssert.Contains(prelude, "depth - 1", StringComparison.Ordinal);
        StringAssert.Contains(prelude, "value", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersRenderObjectHelpersWithLocalFragmentProvenance()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, CreateCarrier(Text).Header);",
            """
            [Parameter] public string Text { get; set; } = "";

            private sealed class FragmentCarrier
            {
                public FragmentCarrier(RenderFragment header)
                {
                    Header = header;
                }

                public RenderFragment Header { get; set; } = default!;
            }

            private FragmentCarrier CreateCarrier(string text)
            {
                RenderFragment header = child => child.AddContent(0, "object-helper:" + text);
                return new FragmentCarrier(header);
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "object-helper:", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Text", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersBlockBodiedRenderFragmentFactoriesWithLocalProvenance()
    {
        var fixture = CreateDirectRenderFixture(
            "RenderFragment<string> template = CreateTemplate(); builder.AddContent(0, CreateFragment(Text)); builder.AddContent(1, template.Invoke(Text));",
            """
            [Parameter] public string Text { get; set; } = "";

            private RenderFragment CreateFragment(string value)
            {
                RenderFragment local = child => child.AddContent(0, "factory-local:" + value);
                return child =>
                {
                    child.AddContent(0, local);
                    child.AddContent(1, "factory-fragment:" + value);
                };
            }

            private RenderFragment<string> CreateTemplate()
            {
                RenderFragment<string> local = value => child => child.AddContent(0, "factory-template-local:" + value);
                return value => child => child.AddContent(0, local(value));
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "factory-fragment:", StringComparison.Ordinal);
        StringAssert.Contains(output, "factory-template-local:", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Text", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersBulkAttributesFromIndexerAndCollectionAddInitializers()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object?> { ["data-indexer"] = "indexer-value" });
            builder.AddMultipleAttributes(2, new System.Collections.Generic.Dictionary<string, object?> { { "data-add", "add-value" } });
            builder.CloseElement();
            """,
            string.Empty);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "data-indexer", StringComparison.Ordinal);
        StringAssert.Contains(output, "indexer-value", StringComparison.Ordinal);
        StringAssert.Contains(output, "data-add", StringComparison.Ordinal);
        StringAssert.Contains(output, "add-value", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsConditionalDynamicAttributeNames()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "button");
            if (Enabled)
            {
                builder.AddAttribute(1, AttributeName, "dynamic-name");
            }
            else
            {
                builder.AddAttribute(2, "data-fallback", "fallback-name");
            }
            builder.CloseElement();
            """,
            "[Parameter] public bool Enabled { get; set; } [Parameter] public string AttributeName { get; set; } = \"\";");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(failure, "Attribute names must be compile-time strings for direct render lowering.", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsConditionalRenderFragmentComponentParameters()
    {
        var fixture = CreateDirectRenderFixture(
            """
            RenderFragment content = child => child.AddContent(0, "conditional-child");
            builder.OpenComponent<ChildComponent>(1);
            if (Enabled)
            {
                builder.AddComponentParameter(2, "ChildContent", content);
            }
            else
            {
                builder.AddComponentParameter(3, "ChildContent", content);
            }
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-child")]
            private sealed class ChildComponent : ComponentBase
            {
                [Parameter] public RenderFragment? ChildContent { get; set; }
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "Conditional RenderFragment component parameters are not supported by direct render lowering.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersConditionalElementAttributeBranches()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "button");
            if (Enabled)
            {
                builder.AddAttribute(1, "data-state", "enabled");
                builder.AddAttribute(2, "disabled");
            }
            else
            {
                builder.AddAttribute(3, "data-state", "disabled");
            }
            builder.AddContent(4, "conditional-attributes");
            builder.CloseElement();
            """,
            "[Parameter] public bool Enabled { get; set; }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "props.Enabled", StringComparison.Ordinal);
        StringAssert.Contains(output, "data-state", StringComparison.Ordinal);
        StringAssert.Contains(output, "conditional-attributes", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersEarlyReturnConditionalAsRenderGuard()
    {
        var fixture = CreateDirectRenderFixture(
            """
            if (!Enabled)
            {
                return;
            }

            builder.AddContent(0, "guarded-direct-render");
            """,
            "[Parameter] public bool Enabled { get; set; }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "guarded-direct-render", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Enabled", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersDeconstructedForeachBindings()
    {
        var fixture = CreateDirectRenderFixture(
            """
            foreach (var (key, value) in Items)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "data-key", key);
                builder.AddContent(2, value);
                builder.CloseElement();
            }
            """,
            "[Parameter] public (string Key, string Value)[] Items { get; set; } = [];");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "Array.from(props.Items ?? []", StringComparison.Ordinal);
        StringAssert.Contains(output, "data-key", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersTypeOfComponentAliasAndIgnoresSecondaryBuilderCalls()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var childType = typeof(ChildComponent);
            var secondaryBuilder = builder;
            builder.OpenComponent(0, childType);
            secondaryBuilder.AddContent(1, "secondary-builder-content");
            builder.AddComponentParameter(2, "Title", "typeof-component");
            builder.CloseComponent();
            """,
            """
            [global::ECMAScript.ECMAScriptModule("./components/typeof-child")]
            private sealed class ChildComponent : ComponentBase
            {
                [Parameter] public string Title { get; set; } = "";
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "typeof-component", StringComparison.Ordinal);
        var imports = string.Join(
            Environment.NewLine,
            result.ImportDeclarations.Select(static declaration => declaration.ToKnRECMAScript()));
        StringAssert.Contains(imports, "typeof-child", StringComparison.Ordinal);
        Assert.DoesNotContain("secondary-builder-content", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_ErasesPureDiscardRazorMetadataDeconstruction()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var (_, _) = (nameof(Text), 0);
            builder.AddContent(0, "discard-metadata");
            """,
            "[Parameter] public string Text { get; set; } = \"\";");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "discard-metadata", StringComparison.Ordinal);
        Assert.DoesNotContain("nameof", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_NormalizesRuntimeComponentAttributeBags()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenComponent<ChildComponent>(0);
            builder.AddMultipleAttributes(1, Attributes);
            builder.CloseComponent();
            """,
            """
            [Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object?> Attributes { get; set; } = new System.Collections.Generic.Dictionary<string, object?>();

            [global::ECMAScript.ECMAScriptModule("./components/runtime-attributes-child")]
            private sealed class ChildComponent : ComponentBase
            {
                [Parameter] public string Title { get; set; } = "";
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var prelude = string.Join(
            Environment.NewLine,
            result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript()));
        StringAssert.Contains(prelude, "normalizeComponentAttributes", StringComparison.Ordinal);
        StringAssert.Contains(result.RenderExpression.ToKnRECMAScript(), "props.Attributes", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsForeachAllDiscardDeconstruction()
    {
        var fixture = CreateDirectRenderFixture(
            "foreach (var (_, _) in Items) { builder.AddContent(0, \"ignored\"); }",
            "[Parameter] public (string Key, string Value)[] Items { get; set; } = [];");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(
            failure,
            "Foreach direct render lowering requires a local loop variable or a local deconstruction target.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsUnsupportedDirectRenderOperationShapes()
    {
        AssertDirectRenderFailure(
            "string value;",
            "Local declarations in direct render lowering must have an initializer.");
        AssertDirectRenderFailure(
            "Text = \"changed\";",
            "RazorVue direct render operation lowering only supports invocation statements.",
            "[Parameter] public string Text { get; set; } = \"\";");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\");",
            "RazorVue direct render operation lowering found unclosed RenderTreeBuilder frames.");
    }

    [TestMethod]
    public void TryEmit_RejectsNestedRuntimeLocalsAndUnclosedConditionalFrames()
    {
        AssertDirectRenderFailure(
            """
            builder.OpenElement(0, "section");
            var value = Text;
            builder.CloseElement();
            """,
            "Runtime local declarations in direct render lowering are only supported outside open RenderTreeBuilder frames.",
            "[Parameter] public string Text { get; set; } = \"\";");
        AssertDirectRenderFailure(
            """
            if (Enabled)
            {
                builder.OpenElement(0, "section");
            }
            else
            {
                builder.AddContent(1, "fallback");
            }
            """,
            "Structured direct render lowering left unclosed ElementFrame",
            "[Parameter] public bool Enabled { get; set; }");
    }

    [TestMethod]
    public void TryEmit_RejectsInvalidFrameSpecificParametersAndAttributes()
    {
        AssertDirectRenderFailure(
            """
            builder.OpenElement(0, "div");
            if (Enabled)
            {
                builder.AddComponentParameter(1, "Title", "enabled");
            }
            else
            {
                builder.AddComponentParameter(2, "Title", "disabled");
            }
            builder.CloseElement();
            """,
            "AddComponentParameter requires an open component.",
            "[Parameter] public bool Enabled { get; set; }");
        AssertDirectRenderFailure(
            """
            builder.OpenElement(0, "div");
            builder.AddContent(1, "content");
            builder.AddAttribute(2, "data-late", "late");
            builder.CloseElement();
            """,
            "Attributes must be added before children on an open element or component:");
    }

    [TestMethod]
    public void TryEmit_RejectsUnresolvableRenderFragmentParameterValues()
    {
        AssertDirectRenderFailure(
            """
            builder.OpenComponent<ChildComponent>(0);
            builder.AddComponentParameter(1, "ChildContent", "not-a-fragment");
            builder.CloseComponent();
            """,
            "ChildContent component parameter must be a RenderFragment for direct render lowering.",
            """
            [global::ECMAScript.ECMAScriptModule("./components/parameter-child")]
            private sealed class ChildComponent : ComponentBase
            {
                [Parameter] public RenderFragment? ChildContent { get; set; }
            }
            """);
        AssertDirectRenderFailure(
            """
            builder.OpenComponent<ChildComponent>(0);
            builder.AddComponentParameter(1, "Header", UnknownFragment);
            builder.CloseComponent();
            """,
            "RenderFragment component parameters require a resolvable inline, local, helper, or component-slot source.",
            """
            private static RenderFragment UnknownFragment = default!;

            [global::ECMAScript.ECMAScriptModule("./components/parameter-child")]
            private sealed class ChildComponent : ComponentBase
            {
                [Parameter] public RenderFragment? Header { get; set; }
            }
            """);
        AssertDirectRenderFailure(
            "builder.AddContent(0, UnknownTemplate, Text);",
            "AddContent<TValue> requires a resolvable RenderFragment<TValue> source.",
            """
            private static RenderFragment<string> UnknownTemplate = default!;

            [Parameter] public string Text { get; set; } = "";
            """);
    }

    [TestMethod]
    public void TryEmit_PreservesDiscardedPreludeCallsAndRejectsErasedDescriptorValues()
    {
        var fixture = CreateDirectRenderFixture(
            "_ = (object)CreateValue(); builder.AddContent(0, \"after-discard\");",
            "private static string CreateValue() => \"discarded\";");
        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var prelude = string.Join(
            Environment.NewLine,
            result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript()));
        StringAssert.Contains(prelude, "CreateValue()", StringComparison.Ordinal);

        AssertDirectRenderFailure(
            """
            var descriptor = new FragmentDescriptor
            {
                Header = child => child.AddContent(0, "descriptor-header")
            };
            builder.AddContent(1, descriptor);
            """,
            "RenderFragment descriptor local 'descriptor' can only be consumed through a resolved RenderFragment member in direct render lowering.",
            """
            private sealed class FragmentDescriptor
            {
                public RenderFragment Header { get; set; } = default!;
            }
            """);
    }

    [TestMethod]
    public void TryEmit_RejectsDirectRenderFrameAndMetadataBoundaryShapes()
    {
        AssertDirectRenderFailure(
            "while (Enabled) { builder.AddContent(0, \"loop\"); }",
            "only supports straight-line RenderTreeBuilder statements in this slice.",
            "[Parameter] public bool Enabled { get; set; }");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, Tag); builder.CloseElement();",
            "OpenElement tag names must be compile-time strings for direct render lowering.",
            "[Parameter] public string Tag { get; set; } = \"div\";");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, Name, \"value\"); builder.CloseElement();",
            "Attribute names must be compile-time strings for direct render lowering.",
            "[Parameter] public string Name { get; set; } = \"data-name\";");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"input\"); builder.SetAttributeValue(1, \"value\"); builder.CloseElement();",
            "SetAttributeValue requires a known preceding attribute in direct render lowering.");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddContent(1, \"content\"); builder.SetKey(\"key\"); builder.CloseElement();",
            "SetKey must target an open element or component before children.");
        AssertDirectRenderFailure(
            "builder.OpenComponent<ChildComponent>(0); builder.SetUpdatesAttributeName(\"value\"); builder.CloseComponent();",
            "SetUpdatesAttributeName must target an open element before children.",
            """
            [global::ECMAScript.ECMAScriptModule("./components/frame-child")]
            private sealed class ChildComponent : ComponentBase
            {
            }
            """);
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"form\"); builder.AddNamedEvent(\"\", \"submit\"); builder.CloseElement();",
            "Named event metadata requires compile-time event names for direct render lowering.");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddContent(1, \"content\"); builder.AddMultipleAttributes(2, Attributes); builder.CloseElement();",
            "Multiple attributes must be added before children on an open element or component.",
            "private static System.Collections.Generic.Dictionary<string, object> Attributes = [];");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddContent(1, \"content\"); builder.AddElementReferenceCapture(2, value => { }); builder.CloseElement();",
            "Element reference captures require the current open element before children.");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddComponentReferenceCapture(1, value => { }); builder.CloseElement();",
            "Component reference captures require the current open component before children.");
        AssertDirectRenderFailure(
            "builder.OpenElement(0, \"div\"); builder.AddComponentRenderMode(RenderMode(null!)); builder.CloseElement();",
            "Component render mode metadata requires the current open component before children.",
            "private static IComponentRenderMode RenderMode(object? value) => default!;");
    }

    [TestMethod]
    public void TryEmit_LowersConditionalGenericFragmentsAndMixedRenderObjectProperties()
    {
        var fixture = CreateDirectRenderFixture(
            """
            RenderFragment<string> template = Enabled
                ? value => child =>
                {
                    child.AddContent(0, "conditional:true:" + value);
                    child.AddMarkupContent(1, "<i>true</i>");
                }
                : value => child =>
                {
                    child.AddContent(0, "conditional:false:" + value);
                    child.AddMarkupContent(1, "<i>false</i>");
                };
            builder.AddContent(0, template, Text);
            builder.AddContent(1, template.Invoke(Text));
            builder.AddContent(2, CreateCarrier(Text).Header);
            """,
            """
            [Parameter] public bool Enabled { get; set; }
            [Parameter] public string Text { get; set; } = "";

            private sealed class FragmentCarrier
            {
                public FragmentCarrier(int ignored, RenderFragment header)
                {
                    Header = header;
                }

                public string Name { get; set; } = string.Empty;
                public RenderFragment Header { get; set; } = default!;
            }

            private FragmentCarrier CreateCarrier(string text)
            {
                var ignored = 0;
                RenderFragment header = child => child.AddContent(0, "mixed-carrier:" + text);
                return new FragmentCarrier(ignored, header)
                {
                    Name = "ignored"
                };
            }
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesFragment);
        Assert.IsTrue(result.UsesStaticVNode);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "conditional:true:", StringComparison.Ordinal);
        StringAssert.Contains(output, "conditional:false:", StringComparison.Ordinal);
        StringAssert.Contains(output, "mixed-carrier:", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsUnclosedRenderFragmentHelperFrames()
    {
        AssertDirectRenderFailure(
            "builder.AddContent(0, CreateOpenFragment());",
            "RenderFragment helper 'CreateOpenFragment' left unclosed ElementFrame('div') frames.",
            """
            private RenderFragment CreateOpenFragment() => child =>
            {
                child.OpenElement(0, "div");
            };
            """);
    }

    [TestMethod]
    public void TryEmit_HoistsRecursiveStaticMultiRootRenderFragmentHelpers()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, RecursiveMarkup(1));",
            """
            private RenderFragment RecursiveMarkup(int depth) => child =>
            {
                child.AddMarkupContent(0, "<i>first</i>");
                child.AddMarkupContent(1, "<i>second</i>");
                if (depth > 0)
                {
                    child.AddContent(2, RecursiveMarkup(depth - 1));
                }
            };
            """);

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesFragment);
        Assert.IsTrue(result.UsesStaticVNode);
        var prelude = string.Join(
            Environment.NewLine,
            result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript()));
        StringAssert.Contains(prelude, "<i>first</i>", StringComparison.Ordinal);
        StringAssert.Contains(prelude, "depth - 1", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersDirectAndConditionalBooleanAttributes()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "disabled");
            if (Enabled)
            {
                builder.AddAttribute(2, "required");
            }
            else
            {
                builder.AddAttribute(3, "data-state", "fallback");
            }
            builder.CloseElement();
            """,
            "[Parameter] public bool Enabled { get; set; }");

        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "disabled", StringComparison.Ordinal);
        StringAssert.Contains(output, "required", StringComparison.Ordinal);
        StringAssert.Contains(output, "data-state", StringComparison.Ordinal);
    }

    private static Fixture CreateFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            namespace RenderEmitter.Contract;

            public sealed class ContractComponent
            {
                public void Build()
                {
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterContract.cs");
        var compilation = CSharpCompilation.Create(
            "RenderEmitter.Contract.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var model = compilation.GetSemanticModel(sourceTree);
        var declaration = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "ContractComponent");
        var component = model.GetDeclaredSymbol(declaration);
        Assert.IsNotNull(component);
        var methodDeclaration = declaration.Members.OfType<MethodDeclarationSyntax>().Single();
        var method = model.GetDeclaredSymbol(methodDeclaration);
        Assert.IsNotNull(method);
        var body = model.GetOperation(methodDeclaration.Body!) as IBlockOperation;
        Assert.IsNotNull(body);

        return new Fixture(compilation, component!, method!, body!);
    }

    private static Fixture CreateDirectRenderFixture(string body, string members)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            $$"""
            #nullable enable
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RenderEmitter.Contract;

            public sealed class ContractComponent : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    {{body}}
                }

                {{members}}
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterDirectContract.cs");
        var compilation = CSharpCompilation.Create(
            "RenderEmitter.DirectContract.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var model = compilation.GetSemanticModel(sourceTree);
        var declaration = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "ContractComponent");
        var component = model.GetDeclaredSymbol(declaration);
        Assert.IsNotNull(component);
        var methodDeclaration = declaration.Members.OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "BuildRenderTree");
        var method = model.GetDeclaredSymbol(methodDeclaration);
        Assert.IsNotNull(method);
        var operation = model.GetOperation(methodDeclaration.Body!) as IBlockOperation;
        Assert.IsNotNull(operation);

        return new Fixture(compilation, component!, method!, operation!);
    }

    private static void AssertDirectRenderFailure(string body, string expectedFailure, string members = "")
    {
        var fixture = CreateDirectRenderFixture(body, members);
        var emitted = RenderEmitter.TryEmit(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        StringAssert.Contains(failure, expectedFailure, StringComparison.Ordinal);
    }

    private sealed record Fixture(
        Compilation Compilation,
        INamedTypeSymbol Component,
        IMethodSymbol Method,
        IBlockOperation Body);
}
