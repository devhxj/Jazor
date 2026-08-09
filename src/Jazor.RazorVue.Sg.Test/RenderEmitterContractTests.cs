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

    private sealed record Fixture(
        Compilation Compilation,
        INamedTypeSymbol Component,
        IMethodSymbol Method,
        IBlockOperation Body);
}
