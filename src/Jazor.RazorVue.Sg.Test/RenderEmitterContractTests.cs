using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
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
    public void TryEmitWithDiagnostic_PreservesSuccessAndFailureCategories()
    {
        var success = CreateDirectRenderFixture("builder.AddContent(0, \"ready\");", string.Empty);
        var emitted = RenderEmitter.TryEmitWithDiagnostic(
            success.Compilation,
            success.Component,
            success.Method,
            success.Body,
            declaredNames: null,
            reservedImportNames: ["reservedImport"],
            VueInjectRegistry.ForCompilation(success.Compilation),
            out var result,
            out var diagnostic);

        Assert.IsTrue(emitted);
        Assert.IsNotNull(result);
        Assert.IsNull(diagnostic);

        var directFailure = CreateDirectRenderFixture("string value;", string.Empty);
        emitted = RenderEmitter.TryEmitWithDiagnostic(
            directFailure.Compilation,
            directFailure.Component,
            directFailure.Method,
            directFailure.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(directFailure.Compilation),
            out result,
            out diagnostic);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(RazorVueDiagnosticCategory.DirectRender, diagnostic.Category);
        Assert.AreNotEqual(Microsoft.CodeAnalysis.Location.None, diagnostic.PrimaryLocation);

        var signatureFailure = CreateFixture();
        emitted = RenderEmitter.TryEmitWithDiagnostic(
            signatureFailure.Compilation,
            signatureFailure.Component,
            signatureFailure.Method,
            signatureFailure.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(signatureFailure.Compilation),
            out result,
            out diagnostic);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(RazorVueDiagnosticCategory.DirectRender, diagnostic.Category);
        StringAssert.Contains(diagnostic.Message, "BuildRenderTree(RenderTreeBuilder)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmitWithDiagnostic_PreservesCompilerBridgeFailures()
    {
        // System.IO.Path has no browser surface, so it stays outside the CLR whitelist and keeps
        // this fixture a genuine compiler-bridge failure.
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, global::System.IO.Path.GetTempPath());",
            string.Empty);

        var emitted = RenderEmitter.TryEmitWithDiagnostic(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var diagnostic);

        Assert.IsFalse(emitted);
        Assert.IsNull(result);
        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(RazorVueDiagnosticCategory.CompilerBridge, diagnostic.Category);
        Assert.AreNotEqual(Microsoft.CodeAnalysis.Location.None, diagnostic.PrimaryLocation);
    }

    [TestMethod]
    public void TryEmit_RecognizesEveryScalarConstantAsStaticTextContent()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.AddContent(0, (object?)null);
            builder.AddContent(1, "text");
            builder.AddContent(2, true);
            builder.AddContent(3, 'c');
            builder.AddContent(4, (sbyte)-1);
            builder.AddContent(5, (byte)2);
            builder.AddContent(6, (short)-3);
            builder.AddContent(7, (ushort)4);
            builder.AddContent(8, 5);
            builder.AddContent(9, (uint)6);
            builder.AddContent(10, (long)-7);
            builder.AddContent(11, (ulong)8);
            builder.AddContent(12, 9.0f);
            builder.AddContent(13, 10.0d);
            builder.AddContent(14, 11.0m);
            builder.AddContent(15, StaticTextKind.Second);
            """,
            """
            private enum StaticTextKind
            {
                First,
                Second
            }
            """);

        AssertDirectRenderSuccess(
            fixture,
            "null",
            "text",
            "true",
            "c",
            "11");
    }

    [TestMethod]
    public void TryEmitWithDiagnostic_LowersTypeInferenceHelperInsideRenderFragment()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, CreateFragment());",
            """
            private RenderFragment CreateFragment() => child =>
                TypeInference.CreateWidget_0<string>(child, 0, "value");

            private static class TypeInference
            {
                public static void CreateWidget_0<T>(RenderTreeBuilder __builder, int sequence, T value)
                {
                    __builder.OpenComponent<GenericChild<T>>(sequence);
                    __builder.CloseComponent();
                }
            }

            [ECMAScript.ECMAScriptModule("./components/render-emitter-generic-child")]
            private sealed class GenericChild<T> : ComponentBase, ECMAScript.Vue.IVueComponent { }
            """);

        var emitted = RenderEmitter.TryEmitWithDiagnostic(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var diagnostic);

        Assert.IsTrue(emitted, diagnostic?.Message ?? "TypeInference helper lowering failed.");
        Assert.IsNotNull(result);
        Assert.IsNull(diagnostic);
        var module = result!.RenderExpression.ToKnRECMAScript();
        Assert.IsFalse(module.Contains("__builder", StringComparison.Ordinal), module);
        StringAssert.Contains(module, "h(", StringComparison.Ordinal);
        var imports = string.Join(
            Environment.NewLine,
            result.ImportDeclarations.Select(static declaration => declaration.ToKnRECMAScript()));
        StringAssert.Contains(imports, "render-emitter-generic-child", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmitWithDiagnostic_LowersOpenGenericComponentTypeWithErasedTypeArgument()
    {
        var fixture = CreateGenericDirectRenderFixture();

        var emitted = RenderEmitter.TryEmitWithDiagnostic(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var diagnostic);

        Assert.IsTrue(emitted, diagnostic?.Message ?? "Open generic component lowering failed.");
        Assert.IsNotNull(result);
        Assert.IsNull(diagnostic);
        var module = result!.RenderExpression.ToKnRECMAScript();
        Assert.IsFalse(module.Contains("builder.OpenComponent", StringComparison.Ordinal), module);
        StringAssert.Contains(module, "h(", StringComparison.Ordinal);
        var imports = string.Join(
            Environment.NewLine,
            result.ImportDeclarations.Select(static declaration => declaration.ToKnRECMAScript()));
        StringAssert.Contains(imports, "render-emitter-open-generic-child", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmitWithDiagnostic_LowersOpenGenericComponentTypeFromInheritedBuildRenderTree()
    {
        var fixture = CreateInheritedGenericDirectRenderFixture();

        var emitted = RenderEmitter.TryEmitWithDiagnostic(
            fixture.Compilation,
            fixture.Component,
            fixture.Method,
            fixture.Body,
            declaredNames: null,
            reservedImportNames: null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            out var result,
            out var diagnostic);

        Assert.IsTrue(emitted, diagnostic?.Message ?? "Inherited open generic component lowering failed.");
        Assert.IsNotNull(result);
        Assert.IsNull(diagnostic);
        var module = result!.RenderExpression.ToKnRECMAScript();
        Assert.IsFalse(module.Contains("builder.", StringComparison.Ordinal), module);
        StringAssert.Contains(module, "h(", StringComparison.Ordinal);
        var imports = string.Join(
            Environment.NewLine,
            result.ImportDeclarations.Select(static declaration => declaration.ToKnRECMAScript()));
        StringAssert.Contains(imports, "render-emitter-inherited-generic-child", StringComparison.Ordinal);
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
        Assert.IsTrue(result.UsesRawMarkupRuntime);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "__jazor$createRawMarkup", StringComparison.Ordinal);
        StringAssert.Contains(output, "Array.from(props.Items ?? []", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_TracksStaticVNodeFactsAcrossConditionalBranchOrder()
    {
        // Each source shape proves one short-circuit order. Together they ensure a static
        // hoist from either branch, or from preceding content, survives aggregate rendering.
        // 三种顺序覆盖 true/false 分支和已有 hoist，避免聚合标记随条件分支丢失。
        AssertStaticConditional(
            """
            if (Enabled)
                builder.AddMarkupContent(0, "<strong>true branch</strong>");
            else
                builder.AddContent(1, "fallback");
            """);
        AssertStaticConditional(
            """
            if (Enabled)
                builder.AddContent(0, "fallback");
            else
                builder.AddMarkupContent(1, "<strong>false branch</strong>");
            """);
        AssertStaticConditional(
            """
            builder.AddMarkupContent(0, "<strong>before conditional</strong>");
            if (Enabled)
                builder.AddContent(1, "first");
            else
                builder.AddContent(2, "second");
            """);

        static void AssertStaticConditional(string body)
        {
            var fixture = CreateDirectRenderFixture(
                body,
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
            Assert.IsTrue(result.UsesStaticVNode);
            Assert.IsTrue(result.ModuleHoists.Any(static hoist =>
                hoist.Initializer.ToKnRECMAScript().Contains("createStaticVNode", StringComparison.Ordinal)));
        }
    }

    [TestMethod]
    public void TryEmit_StaticMultiRootMarkup_UsesExactHtmlFragmentCardinality()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddMarkupContent(0, \"<strong>one</strong><em>two</em>\");",
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
        Assert.IsTrue(result.UsesStaticVNode);
        Assert.IsFalse(result.UsesRawMarkupRuntime);
        var hoist = result.ModuleHoists.Single();
        Assert.AreEqual(
            "createStaticVNode(\"<strong>one</strong><em>two</em>\", 2)",
            hoist.Initializer.ToKnRECMAScript());
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
    public void TryEmit_LowersRenderFragmentPropertyWithStraightLineLocalProvenance()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, LocalFragment);",
            """
            private RenderFragment LocalFragment
            {
                get
                {
                    RenderFragment header = child => child.AddContent(0, "property-local-fragment");
                    RenderFragment alias = (RenderFragment)header;
                    return alias;
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
        StringAssert.Contains(
            result.RenderExpression.ToKnRECMAScript(),
            "property-local-fragment",
            StringComparison.Ordinal);
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
    public void TryEmit_LowersBlockBodiedRenderFragmentFactoryWithLocalFragment()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.AddContent(0, CreateHeader());",
            """
            private RenderFragment CreateHeader()
            {
                RenderFragment header = child => child.AddContent(0, "block-factory-header");
                return header;
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
        StringAssert.Contains(
            result.RenderExpression.ToKnRECMAScript(),
            "block-factory-header",
            StringComparison.Ordinal);
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
        Assert.IsTrue(result.UsesRawMarkupRuntime);
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
                {
                }
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
        var hoists = string.Join(
            Environment.NewLine,
            result.ModuleHoists.Select(static hoist => hoist.Initializer.ToKnRECMAScript()));
        StringAssert.Contains(output, "__jazor$hoistedProps0", StringComparison.Ordinal);
        StringAssert.Contains(hoists, "data-indexer", StringComparison.Ordinal);
        StringAssert.Contains(hoists, "indexer-value", StringComparison.Ordinal);
        StringAssert.Contains(hoists, "data-add", StringComparison.Ordinal);
        StringAssert.Contains(hoists, "add-value", StringComparison.Ordinal);
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
    public void TryEmit_LowersConditionalRenderFragmentComponentParametersAsDynamicSlots()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenComponent<ChildComponent>(1);
            if (Enabled)
            {
                builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(child => child.AddContent(0, "conditional-true")));
            }
            else
            {
                builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(child => child.AddContent(0, "conditional-false")));
            }
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesCreateSlots);
        Assert.IsTrue(result.UsesWithCtx);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "createSlots", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Enabled", StringComparison.Ordinal);
        StringAssert.Contains(output, "conditional-true", StringComparison.Ordinal);
        StringAssert.Contains(output, "conditional-false", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersConditionalRenderFragmentSlotWithAnOmittedElseBranch()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.OpenComponent<ChildComponent>(1);
            if (Enabled)
            {
                builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(child => child.AddContent(0, "conditional-present")));
            }
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesCreateSlots);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "props.Enabled", StringComparison.Ordinal);
        StringAssert.Contains(output, "conditional-present", StringComparison.Ordinal);
        StringAssert.Contains(output, "null", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_UsesConditionalComponentAttributesForNonFragmentValuesAndAddAttributeSlots()
    {
        var propFixture = CreateDirectRenderFixture(
            """
            builder.OpenComponent<ChildComponent>(1);
            if (Enabled)
            {
                builder.AddComponentParameter(2, "Title", "enabled");
            }
            else
            {
                builder.AddComponentParameter(3, "Title", "disabled");
            }
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
            {
                [Parameter] public string? Title { get; set; }
            }
            """);
        var propEmitted = RenderEmitter.TryEmit(
            propFixture.Compilation,
            propFixture.Component,
            propFixture.Method,
            propFixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(propFixture.Compilation),
            out var propResult,
            out var propFailure);

        Assert.IsTrue(propEmitted, propFailure);
        Assert.IsNotNull(propResult);
        Assert.IsTrue(propResult.UsesProps);
        StringAssert.Contains(propResult.RenderExpression.ToKnRECMAScript(), "enabled", StringComparison.Ordinal);

        var slotFixture = CreateDirectRenderFixture(
            """
            builder.OpenComponent<ChildComponent>(1);
            if (Enabled)
            {
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(child => child.AddContent(0, "attribute-true")));
            }
            else
            {
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(child => child.AddContent(0, "attribute-false")));
            }
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
            {
                [Parameter] public RenderFragment? ChildContent { get; set; }
            }
            """);
        var slotEmitted = RenderEmitter.TryEmit(
            slotFixture.Compilation,
            slotFixture.Component,
            slotFixture.Method,
            slotFixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(slotFixture.Compilation),
            out var slotResult,
            out var slotFailure);

        Assert.IsTrue(slotEmitted, slotFailure);
        Assert.IsNotNull(slotResult);
        Assert.IsTrue(slotResult.UsesCreateSlots);
        var slotOutput = slotResult.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(slotOutput, "attribute-true", StringComparison.Ordinal);
        StringAssert.Contains(slotOutput, "attribute-false", StringComparison.Ordinal);
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
        Assert.IsFalse(output.Contains("renderList", StringComparison.Ordinal), output);
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
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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
        var hoists = string.Join(
            Environment.NewLine,
            result.ModuleHoists.Select(static hoist => hoist.Initializer.ToKnRECMAScript()));
        StringAssert.Contains(hoists, "typeof-component", StringComparison.Ordinal);
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
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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
    public void TryEmit_LowersLoopBranchesAfterCompletedRenderSegments()
    {
        var fixture = CreateDirectRenderFixture(
            "while (Enabled) { builder.AddContent(0, \"loop\"); break; }",
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
        StringAssert.Contains(output, "while (", StringComparison.Ordinal);
        StringAssert.Contains(output, "break;", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_ResolvesComponentRenderFragmentPropertiesAndObjectInitializers()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.AddContent(0, ExpressionFragment);
            builder.AddContent(1, AccessorFragment);
            var fragments = new FragmentHolder
            {
                Header = child => child.AddContent(0, "initializer-fragment")
            };
            builder.AddContent(2, fragments.Header);
            """,
            """
            private RenderFragment ExpressionFragment => child => child.AddContent(0, "expression-fragment");

            private RenderFragment AccessorFragment
            {
                get
                {
                    return child => child.AddContent(0, "accessor-fragment");
                }
            }

            private sealed class FragmentHolder
            {
                public RenderFragment Header { get; set; } = default!;
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
        StringAssert.Contains(output, "expression-fragment", StringComparison.Ordinal);
        StringAssert.Contains(output, "accessor-fragment", StringComparison.Ordinal);
        StringAssert.Contains(output, "initializer-fragment", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersBranchingLoopSideEffectsAndConditionalAlternativesInSourceOrder()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var total = 0;
            foreach (var item in Items)
            {
                if (item == "skip")
                {
                    total++;
                    continue;
                }

                builder.AddContent(0, "before:" + item);
                total += item.Length;
                if (item == "stop")
                {
                    builder.AddContent(1, "stop:" + item);
                    break;
                }
                else
                {
                    builder.AddContent(2, "after:" + item);
                }
            }

            builder.AddContent(3, total);
            """,
            "[Parameter] public string[] Items { get; set; } = []; ");

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
        StringAssert.Contains(output, "continue;", StringComparison.Ordinal);
        StringAssert.Contains(output, "break;", StringComparison.Ordinal);
        StringAssert.Contains(output, "before:", StringComparison.Ordinal);
        StringAssert.Contains(output, "after:", StringComparison.Ordinal);
        StringAssert.Contains(output, "total", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersBranchingLoopsWithIterationLocalsAndCompoundUpdates()
    {
        var forFixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < 4; index += 1)
            {
                var current = index;
                if (current == 0)
                {
                    continue;
                }

                builder.AddContent(0, current);
                if (current == 2)
                {
                    break;
                }
            }
            """,
            string.Empty);
        var forEachFixture = CreateDirectRenderFixture(
            """
            foreach (var item in Items)
            {
                var current = item;
                if (current == "skip")
                {
                    continue;
                }

                builder.AddContent(0, current);
                break;
            }
            """,
            "[Parameter] public string[] Items { get; set; } = []; ");

        AssertBranchingLoopWithIterationLocal(forFixture, "for");
        AssertBranchingLoopWithIterationLocal(forEachFixture, "for");

        static void AssertBranchingLoopWithIterationLocal(Fixture fixture, string loopToken)
        {
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
            StringAssert.Contains(output, loopToken, StringComparison.Ordinal);
            StringAssert.Contains(output, "const current", StringComparison.Ordinal);
            StringAssert.Contains(output, "continue;", StringComparison.Ordinal);
            StringAssert.Contains(output, "break;", StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void TryEmit_LowersKeyedForAndWhileLoopsWithOrdinaryBranchTargets()
    {
        var forFixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < 3; index++)
            {
                builder.OpenElement(0, "li");
                builder.SetKey(index);
                builder.AddContent(1, index);
                builder.CloseElement();
                if (index == 1)
                {
                    break;
                }
            }
            """,
            string.Empty);
        var whileFixture = CreateDirectRenderFixture(
            """
            var index = 0;
            while (index < 3)
            {
                index++;
                builder.OpenElement(0, "li");
                builder.SetKey(index);
                builder.AddContent(1, index);
                builder.CloseElement();
                if (index == 1)
                {
                    continue;
                }
                break;
            }
            """,
            string.Empty);

        AssertBranchingKeyedLoop(forFixture, "for", expectsContinue: false);
        AssertBranchingKeyedLoop(whileFixture, "while", expectsContinue: true);

        static void AssertBranchingKeyedLoop(Fixture fixture, string loopKind, bool expectsContinue)
        {
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
            StringAssert.Contains(output, loopKind, StringComparison.Ordinal);
            if (expectsContinue)
                StringAssert.Contains(output, "continue;", StringComparison.Ordinal);
            StringAssert.Contains(output, "break;", StringComparison.Ordinal);
            StringAssert.Contains(output, "128", StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void TryEmit_RejectsAsyncForeachBeforeEnteringTheSynchronousRenderProtocol()
    {
        var fixture = CreateDirectRenderFixture(
            """
            await foreach (var item in Items)
            {
                builder.AddContent(0, item);
            }
            """,
            "[Parameter] public System.Collections.Generic.IAsyncEnumerable<string> Items { get; set; } = default!;",
            isAsync: true);

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
            "Async foreach cannot execute inside Razor's synchronous BuildRenderTree contract.",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersKeyedAndDeconstructedBranchingForeachLoops()
    {
        var keyedFixture = CreateDirectRenderFixture(
            """
            foreach (var item in Items)
            {
                builder.OpenElement(0, "li");
                builder.SetKey(item);
                builder.AddContent(1, item);
                builder.CloseElement();
                if (item == "stop")
                {
                    break;
                }
            }
            """,
            "[Parameter] public string[] Items { get; set; } = []; ");
        var deconstructedFixture = CreateDirectRenderFixture(
            """
            foreach (var (label, value) in Items)
            {
                if (value == 0)
                {
                    continue;
                }
                builder.OpenElement(0, "li");
                builder.SetKey(label);
                builder.AddContent(1, value);
                builder.CloseElement();
            }
            """,
            "[Parameter] public (string Label, int Value)[] Items { get; set; } = []; ");

        AssertBranchingForeach(keyedFixture, "break;");
        AssertBranchingForeach(deconstructedFixture, "continue;");

        static void AssertBranchingForeach(Fixture fixture, string branch)
        {
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
            StringAssert.Contains(output, "for", StringComparison.Ordinal);
            StringAssert.Contains(output, branch, StringComparison.Ordinal);
            StringAssert.Contains(output, "128", StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void TryEmit_RejectsNormalLoopBodiesThatLeaveBuilderFramesOpen()
    {
        AssertDirectRenderFailure(
            """
            var index = 0;
            while (index < 1)
            {
                builder.OpenElement(0, "div");
                index++;
            }
            """,
            "Loop render content left unclosed ElementFrame(");
    }

    [TestMethod]
    public void TryEmit_RejectsBranchingLoopStatementsThatCrossOpenBuilderFrames()
    {
        AssertDirectRenderFailure(
            """
            foreach (var item in Items)
            {
                builder.OpenElement(0, "div");
                if (item == "stop")
                {
                    break;
                }
                builder.CloseElement();
            }
            """,
            "Loop break/continue cannot leave an open RenderTreeBuilder frame.",
            "[Parameter] public string[] Items { get; set; } = []; ");
        AssertDirectRenderFailure(
            """
            foreach (var item in Items)
            {
                builder.OpenElement(0, "div");
                break;
            }
            """,
            "Loop control flow left an open ElementFrame(",
            "[Parameter] public string[] Items { get; set; } = []; ");
        AssertDirectRenderFailure(
            """
            var count = 0;
            foreach (var item in Items)
            {
                builder.OpenElement(0, "div");
                count++;
                builder.CloseElement();
                break;
            }
            """,
            "ordinary loop side effect cannot be moved across an open RenderTreeBuilder frame.",
            "[Parameter] public string[] Items { get; set; } = []; ");
    }

    [TestMethod]
    public void TryEmit_RejectsDirectRenderFrameAndMetadataBoundaryShapes()
    {
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
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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
        Assert.IsTrue(result.UsesRawMarkupRuntime);
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
        var hoists = string.Join(
            Environment.NewLine,
            result.ModuleHoists.Select(static hoist => hoist.Initializer.ToKnRECMAScript()));
        StringAssert.Contains(hoists, "<i>first</i>", StringComparison.Ordinal);
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

    [TestMethod]
    public void TryEmit_TracksRenderLocalEventHandlerInDynamicProps()
    {
        var fixture = CreateDirectRenderFixture(
            """
            System.Action handler = () => OnClick();
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "onclick", handler);
            builder.AddAttribute(2, "value", Value);
            builder.CloseElement();
            """,
            """
            [Parameter] public string Value { get; set; } = string.Empty;

            private void OnClick()
            {
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
        StringAssert.Contains(
            result.RenderExpression.ToKnRECMAScript(),
            "createElementBlock(\"input\", { onClick: handler, value: props.Value }, null, 8, [\"onClick\", \"value\"])",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_TracksGenericRenderFragmentParameterEventHandlerInDynamicProps()
    {
        var fixture = CreateDirectRenderFixture(
            "RenderFragment<System.Action> template = callback => child => { child.OpenElement(0, \"button\"); child.AddAttribute(1, \"onclick\", callback); child.CloseElement(); }; builder.AddContent(0, template.Invoke(OnClick));",
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
        StringAssert.Contains(output, "onClick: callback", StringComparison.Ordinal);
        StringAssert.Contains(output, "[\"onClick\"]", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_UsesComponentPropsPatchFlagForDynamicClassAndStyle()
    {
        var fixture = CreateDirectRenderFixture(
            "builder.OpenComponent<ChildComponent>(0); builder.AddComponentParameter(1, \"Class\", CssClass); builder.AddComponentParameter(2, \"Style\", CssStyle); builder.CloseComponent();",
            """
            [Parameter] public string CssClass { get; set; } = string.Empty;
            [Parameter] public string CssStyle { get; set; } = string.Empty;

            [global::ECMAScript.ECMAScriptModule("./components/patch-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
            {
                [Parameter, global::ECMAScript.ECMAScriptName("class")] public string Class { get; set; } = string.Empty;
                [Parameter, global::ECMAScript.ECMAScriptName("style")] public string Style { get; set; } = string.Empty;
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
        StringAssert.Contains(output, "createBlock(", StringComparison.Ordinal);
        StringAssert.Contains(
            output,
            "{ class: props.CssClass, style: props.CssStyle }, null, 8, [\"class\", \"style\"])",
            StringComparison.Ordinal);
        Assert.DoesNotContain(", null, 6", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersForLoopWithExpressionInitializerAndNoCondition()
    {
        // `for (;; update)` has no loop-owned C# local or condition. It exercises the
        // separate initializer statement path without changing the runtime contract.
        var fixture = CreateDirectRenderFixture(
            """
            var index = 0;
            for (index = 0; ; index++)
            {
                builder.AddContent(0, index);
            }
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
        Assert.IsTrue(result.UsesFragment);
        StringAssert.Contains(result.RenderExpression.ToKnRECMAScript(), "index++", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersLoopSideEffectsAtIterationBoundaries()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var index = 0;
            while (index < 1)
            {
                index++;
                builder.AddContent(0, index);
                index++;
            }
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
        StringAssert.Contains(output, "__jazor$loopVNode", StringComparison.Ordinal);
        StringAssert.Contains(output, "index++", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersSingleStatementDoWhileRenderBody()
    {
        var fixture = CreateDirectRenderFixture(
            """
            do
                builder.AddContent(0, "once");
            while (false);
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
        StringAssert.Contains(result.RenderExpression.ToKnRECMAScript(), "do", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_RejectsLoopBodiesWithoutRenderContentOrWithInterleavedEffects()
    {
        AssertDirectRenderFailure(
            """
            var index = 0;
            while (index < 1)
            {
                index++;
            }
            """,
            "Loop direct render lowering requires RenderTreeBuilder content in the loop body.");
        AssertDirectRenderFailure(
            """
            var index = 0;
            while (index < 1)
            {
                builder.AddContent(0, "first");
                index++;
                builder.AddContent(1, "second");
            }
            """,
            "Loop direct render lowering only supports ordinary statements before or after a complete RenderTreeBuilder content segment.");
    }

    [TestMethod]
    public void TryEmit_RejectsLoopControlShapesThatNeedCompilerTemporaryDeclarations()
    {
        AssertDirectRenderFailure(
            """
            while (Items is [var first, ..])
            {
                builder.AddContent(0, first);
            }
            """,
            "While direct render lowering does not support conditions that require compiler temporary declarations.",
            "[Parameter] public int[] Items { get; set; } = [];");
        AssertDirectRenderFailure(
            """
            for (var index = 0; Items is [var first, ..]; index++)
            {
                builder.AddContent(0, first + index);
            }
            """,
            "For direct render lowering does not support initializer, condition, or update expressions that require compiler temporary declarations.",
            "[Parameter] public int[] Items { get; set; } = [];");
        AssertDirectRenderFailure(
            """
            for (int unused, index = 0; index < 1; index++)
            {
                builder.AddContent(0, index);
            }
            """,
            "For direct render lowering requires initialized local control variables.");
    }

    [TestMethod]
    public void TryEmit_UsesIterationRootKeysToChooseFragmentIdentity()
    {
        var keyed = CreateDirectRenderFixture(
            """
            var index = 0;
            while (index < 1)
            {
                builder.OpenElement(0, "li");
                builder.SetKey(index);
                builder.AddContent(1, index);
                builder.CloseElement();
                index++;
            }
            """,
            string.Empty);
        var unkeyed = CreateDirectRenderFixture(
            """
            var index = 0;
            while (index < 1)
            {
                builder.OpenElement(0, "li");
                builder.AddContent(1, index);
                builder.CloseElement();
                index++;
            }
            """,
            string.Empty);

        AssertLoopFragmentFlag(keyed, "128");
        AssertLoopFragmentFlag(unkeyed, "256");

        static void AssertLoopFragmentFlag(Fixture fixture, string expectedPatchFlag)
        {
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
            Assert.IsTrue(result.UsesBlockTree);
            StringAssert.Contains(result.RenderExpression.ToKnRECMAScript(), expectedPatchFlag, StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void TryEmit_LowersKeyedForLoopWithMultipleUpdates()
    {
        var fixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < 2; index++, index += 0)
            {
                builder.OpenElement(0, "li");
                builder.SetKey(index);
                builder.AddContent(1, index);
                builder.CloseElement();
            }
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
        Assert.IsTrue(result.UsesBlockTree);
        Assert.IsTrue(result.UsesFragment);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "for", StringComparison.Ordinal);
        StringAssert.Contains(output, "128", StringComparison.Ordinal);
        StringAssert.Contains(output, "index++", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_KeepsNonVNodeForeachBodiesOnArrayFromFallback()
    {
        var fixture = CreateDirectRenderFixture(
            """
            foreach (var item in Items)
            {
                builder.AddContent(0, item);
            }
            """,
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
        Assert.IsFalse(result.UsesRenderList);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "Array.from", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Items", StringComparison.Ordinal);
        Assert.DoesNotContain("renderList", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_HandlesEmptyAndCommentLeadingStaticMarkupWithoutPhantomVNodes()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.AddMarkupContent(0, "");
            builder.AddMarkupContent(1, "<!-- Razor marker -->");
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
        Assert.IsFalse(result.UsesStaticVNode);
        Assert.IsTrue(result.UsesRawMarkupRuntime);
        var output = result.RenderExpression.ToKnRECMAScript();
        var hoists = string.Join(
            Environment.NewLine,
            result.ModuleHoists.Select(static hoist => hoist.Initializer.ToKnRECMAScript()));
        StringAssert.Contains(output, "__jazor$hoistedRawMarkup", StringComparison.Ordinal);
        StringAssert.Contains(hoists, "<!-- Razor marker -->", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_ProjectsMarkupStringPayloadsThroughStaticAndRuntimePaths()
    {
        var fixture = CreateDirectRenderFixture(
            """
            builder.AddContent(0, new MarkupString(""));
            builder.AddContent(1, new MarkupString("<!-- component marker -->"));
            builder.AddMarkupContent(2, RawMarkup);
            """,
            "[Parameter] public string RawMarkup { get; set; } = string.Empty;");

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
        Assert.IsTrue(result.UsesRawMarkupRuntime);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "__jazor$hoistedRawMarkup", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.RawMarkup", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_TracksStaticVNodeFactsAcrossForeachForAndWhileBodies()
    {
        AssertStaticLoopBody(
            "foreach (var item in Items) { builder.AddMarkupContent(0, \"<i>foreach</i>\"); }",
            "[Parameter] public string[] Items { get; set; } = [];",
            "foreach");
        AssertStaticLoopBody(
            "for (var index = 0; index < 1; index++) { builder.AddMarkupContent(0, \"<i>for</i>\"); }",
            string.Empty,
            "for");
        AssertStaticLoopBody(
            "var index = 0; while (index < 1) { builder.AddMarkupContent(0, \"<i>while</i>\"); index++; }",
            string.Empty,
            "while");

        static void AssertStaticLoopBody(string body, string members, string label)
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

            Assert.IsTrue(emitted, label + ": " + failure);
            Assert.IsNotNull(result);
            // Loop-local output is intentionally non-hoistable: its helper must retain the
            // iteration lifecycle even when the raw markup text itself is static.
            Assert.IsFalse(result.UsesStaticVNode, label);
            Assert.IsTrue(result.UsesRawMarkupRuntime, label);
            StringAssert.Contains(
                result.RenderExpression.ToKnRECMAScript(),
                "__jazor$createRawMarkup",
                StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void TryEmit_LowersConditionalNonGenericRenderFragments()
    {
        var fixture = CreateDirectRenderFixture(
            """
            RenderFragment content = Enabled
                ? child => child.AddContent(0, "enabled fragment")
                : child => child.AddContent(0, "disabled fragment");
            builder.AddContent(0, content);
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
        StringAssert.Contains(output, "enabled fragment", StringComparison.Ordinal);
        StringAssert.Contains(output, "disabled fragment", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_LowersConditionalLocalRenderFragmentAsADynamicComponentSlot()
    {
        var fixture = CreateDirectRenderFixture(
            """
            RenderFragment content = Enabled
                ? child => child.AddContent(0, "enabled slot")
                : child => child.AddContent(0, "disabled slot");
            builder.OpenComponent<ChildComponent>(0);
            builder.AddComponentParameter(1, "ChildContent", content);
            builder.CloseComponent();
            """,
            """
            [Parameter] public bool Enabled { get; set; }

            [global::ECMAScript.ECMAScriptModule("./components/conditional-slot-child")]
            private sealed class ChildComponent : ComponentBase, ECMAScript.Vue.IVueComponent
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

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UsesCreateSlots);
        Assert.IsTrue(result.UsesWithCtx);
        var output = result.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "createSlots", StringComparison.Ordinal);
        StringAssert.Contains(output, "props.Enabled", StringComparison.Ordinal);
        StringAssert.Contains(output, "enabled slot", StringComparison.Ordinal);
        StringAssert.Contains(output, "disabled slot", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_PrunesUnreferencedLocalRenderFragmentDeclarations()
    {
        var fixture = CreateDirectRenderFixture(
            """
            RenderFragment unused = child => child.AddContent(0, "unused fragment");
            RenderFragment used = child => child.AddContent(0, "retained fragment");
            builder.AddContent(0, used);
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
        StringAssert.Contains(output, "retained fragment", StringComparison.Ordinal);
        Assert.DoesNotContain("unused fragment", output, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_DropsDoctypeOnlyMarkupAndUpdatesKnownAttributeValues()
    {
        var doctypeFixture = CreateDirectRenderFixture(
            "builder.AddMarkupContent(0, \"<!doctype html>\");",
            string.Empty);
        var doctypeEmitted = RenderEmitter.TryEmit(
            doctypeFixture.Compilation,
            doctypeFixture.Component,
            doctypeFixture.Method,
            doctypeFixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(doctypeFixture.Compilation),
            out var doctypeResult,
            out var doctypeFailure);

        Assert.IsTrue(doctypeEmitted, doctypeFailure);
        Assert.IsNotNull(doctypeResult);
        Assert.IsFalse(doctypeResult.UsesStaticVNode);
        Assert.IsFalse(doctypeResult.UsesRawMarkupRuntime);
        Assert.IsEmpty(doctypeResult.ModuleHoists);

        var attributeFixture = CreateDirectRenderFixture(
            """
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "data-title", "before");
            builder.SetAttributeValue(2, "after");
            builder.SetKey("stable-input");
            builder.CloseElement();
            """,
            string.Empty);
        var attributeEmitted = RenderEmitter.TryEmit(
            attributeFixture.Compilation,
            attributeFixture.Component,
            attributeFixture.Method,
            attributeFixture.Body,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(attributeFixture.Compilation),
            out var attributeResult,
            out var attributeFailure);

        Assert.IsTrue(attributeEmitted, attributeFailure);
        Assert.IsNotNull(attributeResult);
        var output = attributeResult.RenderExpression.ToKnRECMAScript();
        StringAssert.Contains(output, "\"data-title\": \"after\"", StringComparison.Ordinal);
        StringAssert.Contains(output, "key: \"stable-input\"", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryEmit_PreservesNoUpdateForLoopsAndRuntimeLocalsAfterCompletedContent()
    {
        var loopFixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < 1;)
            {
                builder.AddContent(0, index);
                break;
            }
            """,
            string.Empty);
        var localFixture = CreateDirectRenderFixture(
            """
            builder.AddContent(0, "before");
            var suffix = "after";
            builder.AddContent(1, suffix);
            """,
            string.Empty);

        AssertDirectRenderSuccess(loopFixture, "for", "break;");
        AssertDirectRenderSuccess(localFixture, "const suffix", "before", "after");
    }

    [TestMethod]
    public void TryEmit_CoversForIterationDeclarationsForeachBindingAndLoopPreludeState()
    {
        var fixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < 2; index++)
            {
                var prefix = "item:";
                builder.AddContent(0, prefix + index);
                if (index == 1)
                    continue;
                builder.AddContent(1, "first");
            }
            foreach (var item in Items)
            {
                builder.AddContent(2, item);
                break;
            }
            """,
            "[Parameter] public string[] Items { get; set; } = [];" );

        AssertDirectRenderSuccess(fixture, "item:", "first", "props.Items");
    }

    [TestMethod]
    public void TryEmit_LowersReferenceEqualsInsideRenderFragmentHelperArguments()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var items = new[] { new Item(), new Item() };
            var lastItem = items[^1];
            foreach (var item in items)
            {
                builder.AddContent(0, RenderItem(item, ReferenceEquals(item, lastItem)));
            }
            """,
            """
            private RenderFragment RenderItem(Item item, bool isCurrent) => child =>
            {
                child.OpenElement(0, "span");
                child.AddAttribute(1, "data-current", isCurrent);
                child.AddContent(2, item.Name);
                child.CloseElement();
            };

            private sealed class Item
            {
                public string Name { get; set; } = "item";
            }
            """);

        AssertDirectRenderSuccess(fixture, "item === lastItem");
    }

    [TestMethod]
    public void TryEmit_PreservesPatternLocalsAcrossLoopBodiesWithAndWithoutBranches()
    {
        var fixture = CreateDirectRenderFixture(
            """
            for (var index = 0; index < Items.Length; index++)
            {
                if (Items[index] is string selected)
                {
                    builder.AddContent(0, selected);
                    continue;
                }

                builder.AddContent(1, "missing");
            }

            for (var index = 0; index < Items.Length; index++)
            {
                builder.AddContent(2, Items[index] is string selected ? selected : "missing");
            }

            var renderCount = 0;
            for (var index = 0; index < Items.Length; index++)
            {
                renderCount++;
                builder.AddContent(3, Items[index] is string selected ? selected : "missing");
                renderCount++;
            }
            """,
            "[Parameter] public string?[] Items { get; set; } = [];" );

        AssertDirectRenderSuccess(fixture, "continue;", "__jazor$loopVNode", "let ");
    }

    [TestMethod]
    public void TryEmit_PreservesLoopLocalDeclarationsAndConvertedSideEffectsInIterationOrder()
    {
        var fixture = CreateDirectRenderFixture(
            """
            var renderCount = 0;
            for (var index = 0; index < Items.Length; index++)
            {
                renderCount++;
                var item = Items[index];
                builder.OpenElement(0, "li");
                builder.AddContent(1, item);
                builder.CloseElement();
                renderCount++;
            }
            """,
            "[Parameter] public string[] Items { get; set; } = [];" );

        AssertDirectRenderSuccess(
            fixture,
            "__jazor$loopVNode",
            "const item",
            "renderCount++",
            "createElementBlock");
    }

    [TestMethod]
    public void TryEmit_MaterializesLoopSideEffectConversionTemporariesBeforeRenderContent()
    {
        var fixture = CreateDirectRenderFixture(
            """
            string? selected = null;
            for (var index = 0; index < Items.Length; index++)
            {
                selected = Items[index] as string;
                builder.AddContent(0, selected ?? "missing");
            }
            """,
            "[Parameter] public object?[] Items { get; set; } = [];" );

        AssertDirectRenderSuccess(fixture, "let ", "selected", "missing");
    }

    [TestMethod]
    public void TryEmit_RejectsUnmodeledThrowStatementsBeforeJavaScriptEmission()
    {
        AssertDirectRenderFailure(
            "throw new global::System.InvalidOperationException();",
            "only supports straight-line RenderTreeBuilder statements");
    }

    private static void AssertDirectRenderSuccess(Fixture fixture, params string[] expectedTokens)
    {
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
        foreach (var expectedToken in expectedTokens)
            StringAssert.Contains(output, expectedToken, StringComparison.Ordinal);
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

    private static Fixture CreateDirectRenderFixture(string body, string members, bool isAsync = false)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            $$"""
            #nullable enable
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RenderEmitter.Contract;

            public sealed class ContractComponent : ComponentBase
            {
                protected override {{(isAsync ? "async " : string.Empty)}}void BuildRenderTree(RenderTreeBuilder builder)
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

    private static Fixture CreateGenericDirectRenderFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            #nullable enable
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RenderEmitter.Contract;

            public sealed class ContractComponent<T> : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<GenericChild<T>>(0);
                    builder.CloseComponent();
                }
            }

            [ECMAScriptModule("./components/render-emitter-open-generic-child")]
            public sealed class GenericChild<TValue> : ComponentBase, ECMAScript.Vue.IVueComponent { }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterOpenGenericContract.cs");
        var compilation = CSharpCompilation.Create(
            "RenderEmitter.OpenGenericContract.Tests",
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

    private static Fixture CreateInheritedGenericDirectRenderFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            #nullable enable
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RenderEmitter.Contract;

            public abstract class GenericBase<T> : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<GenericChild<T>>(0);
                    builder.CloseComponent();
                }
            }

            [ECMAScriptModule("./components/render-emitter-inherited-generic-host")]
            public sealed class ContractComponent : GenericBase<string> { }

            [ECMAScriptModule("./components/render-emitter-inherited-generic-child")]
            public sealed class GenericChild<TValue> : ComponentBase, ECMAScript.Vue.IVueComponent { }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterInheritedGenericContract.cs");
        var compilation = CSharpCompilation.Create(
            "RenderEmitter.InheritedGenericContract.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var model = compilation.GetSemanticModel(sourceTree);
        var componentDeclaration = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "ContractComponent");
        var component = model.GetDeclaredSymbol(componentDeclaration);
        Assert.IsNotNull(component);
        var baseType = component!.BaseType;
        Assert.IsNotNull(baseType);
        var method = baseType!.GetMembers("BuildRenderTree")
            .OfType<IMethodSymbol>()
            .Single(static candidate => candidate.Parameters.Length == 1);
        Assert.IsFalse(SymbolEqualityComparer.Default.Equals(method, method.OriginalDefinition));

        var methodDeclaration = method.OriginalDefinition.DeclaringSyntaxReferences
            .Single()
            .GetSyntax() as MethodDeclarationSyntax;
        Assert.IsNotNull(methodDeclaration);
        var operation = model.GetOperation(methodDeclaration!.Body!) as IBlockOperation;
        Assert.IsNotNull(operation);

        return new Fixture(compilation, component, method, operation!);
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
