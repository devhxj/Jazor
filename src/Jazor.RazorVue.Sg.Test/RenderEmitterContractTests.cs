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
            "slots.default",
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
            "Array.from(props.items ?? []",
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
        StringAssert.Contains(output, "Array.from(props.items ?? []", StringComparison.Ordinal);
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
            .Single();
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
            .Single();
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

    private sealed record Fixture(
        Compilation Compilation,
        INamedTypeSymbol Component,
        IMethodSymbol Method,
        IBlockOperation Body);
}
