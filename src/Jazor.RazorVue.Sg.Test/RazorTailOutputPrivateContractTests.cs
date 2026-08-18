using System.Reflection;
using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorTailOutputPrivateContractTests
{
    [TestMethod]
    public void TailOutput_LeavesNonRazorVueCompilationsWithoutCatalogOrDiagnostics()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.TailOutput.NoComponents",
            [CSharpSyntaxTree.ParseText(
                "namespace Demo; public sealed class PlainComponent { }",
                new CSharpParseOptions(LanguageVersion.Preview),
                path: "PlainComponent.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.IsTrue(RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics));
        Assert.IsNull(catalogSource);
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public void TryBuildVueRenderArtifacts_AcceptsAnEmptyBinding()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.TailOutput.EmptyBinding",
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var binding = new GeneratedCSharpBinding(
            compilation,
            ImmutableArray<GeneratedDocument>.Empty,
            ImmutableArray<BoundComponent>.Empty);
        var method = typeof(RazorTailOutput).GetMethod(
            "TryBuildVueRenderArtifacts",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        var arguments = new object?[] { CancellationToken.None, binding, null, null };

        Assert.IsTrue((bool)method.Invoke(null, arguments)!);
        Assert.IsEmpty((ImmutableArray<VueModuleArtifact>)arguments[2]!);
        Assert.IsEmpty((ImmutableArray<RazorVueDiagnosticInfo>)arguments[3]!);
    }

    [TestMethod]
    public void EscapeCSharpString_EncodesAllControlAndDelimiterCharacters()
    {
        var escaped = InvokeEscape("plain\\\"\0\a\b\f\n\r\t\v");

        Assert.AreEqual("\"plain\\\\\\\"\\0\\a\\b\\f\\n\\r\\t\\v\"", escaped);
    }

    [TestMethod]
    public void EscapeJsonString_EncodesHmrPayloadControlAndDelimiterCharacters()
    {
        var escaped = InvokeEscapeJson("plain\\\"\0\b\f\n\r\t\u001f");

        Assert.AreEqual("\"plain\\\\\\\"\\u0000\\b\\f\\n\\r\\t\\u001f\"", escaped);
    }

    [TestMethod]
    public void TryBuildVueRenderArtifacts_StopsWhenMemberClosureRootIsInvalid()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.TailOutput.InvalidClosure",
            [CSharpSyntaxTree.ParseText(
                """
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Rendering;
                using static ECMAScript.Vue;

                namespace Demo;

                [ECMAScriptModule("./components/counter")]
                public sealed class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "counter");
                    }

                    public void NotRender()
                    {
                    }
                }
                """,
                parseOptions,
                "Counter.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("Demo.Counter");
        Assert.IsNotNull(componentSymbol);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(componentSymbol!),
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);

        var invalidRoot = componentSymbol.GetMembers("NotRender").OfType<IMethodSymbol>().Single();
        var invalidComponent = binding!.Components.Single() with { BuildRenderTreeMethod = invalidRoot };
        var invalidBinding = binding with { Components = ImmutableArray.Create(invalidComponent) };
        var method = typeof(RazorTailOutput).GetMethod(
            "TryBuildVueRenderArtifacts",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        var arguments = new object?[] { CancellationToken.None, invalidBinding, null, null };

        var built = (bool)method.Invoke(null, arguments)!;

        Assert.IsFalse(built);
        Assert.IsTrue(((ImmutableArray<VueModuleArtifact>)arguments[2]!).IsDefaultOrEmpty);
        var diagnostics = (ImmutableArray<RazorVueDiagnosticInfo>)arguments[3]!;
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.MemberClosure, diagnostics[0].Category);
        StringAssert.Contains(diagnostics[0].Message, "not BuildRenderTree(RenderTreeBuilder)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void WorkerAndDiagnosticHelpers_KeepTailOutputSchedulingAndOrderingDeterministic()
    {
        Assert.AreEqual(0, Invoke<int>("GetArtifactBuildWorkerCount", 0));
        Assert.AreEqual(1, Invoke<int>("GetArtifactBuildWorkerCount", 1));
        Assert.AreEqual(4, Invoke<int>("GetArtifactBuildWorkerCount", 4));
        Assert.AreEqual(4, Invoke<int>("GetArtifactBuildWorkerCount", 9));

        var sourceTree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class Component { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Component.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.TailOutput.HelperContracts",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Component");
        Assert.IsNotNull(component);

        Assert.AreEqual(
            Location.None,
            Invoke<Location>("GetFirstComponentLocation", ImmutableArray<INamedTypeSymbol>.Empty));
        var sourceLocation = Invoke<Location>(
            "GetFirstComponentLocation",
            ImmutableArray.Create(
                compilation.GetSpecialType(SpecialType.System_String),
                component!));
        Assert.AreEqual("Pages/Component.razor.cs", sourceLocation.GetLineSpan().Path);

        var diagnostics = ImmutableArray.CreateBuilder<RazorVueDiagnosticInfo>();
        diagnostics.Add(RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.VueModule,
            "later",
            component: component));
        diagnostics.Add(RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.ComponentBinding,
            "first"));
        var ordered = Invoke<ImmutableArray<RazorVueDiagnosticInfo>>("OrderDiagnostics", diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.ComponentBinding, ordered[0].Category);
        Assert.AreEqual(RazorVueDiagnosticCategory.VueModule, ordered[1].Category);
    }

    private static string InvokeEscape(string value)
    {
        var method = typeof(RazorTailOutput).GetMethod(
            "EscapeCSharpString",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, [value])!;
    }

    private static string InvokeEscapeJson(string value)
    {
        var method = typeof(RazorTailOutput).GetMethod(
            "EscapeJsonString",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, [value])!;
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(RazorTailOutput)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }
}
