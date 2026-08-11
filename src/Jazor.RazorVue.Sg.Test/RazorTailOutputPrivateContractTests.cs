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
    public void EscapeCSharpString_EncodesAllControlAndDelimiterCharacters()
    {
        var escaped = InvokeEscape("plain\\\"\0\a\b\f\n\r\t\v");

        Assert.AreEqual("\"plain\\\\\\\"\\0\\a\\b\\f\\n\\r\\t\\v\"", escaped);
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
        StringAssert.Contains(arguments[3] as string, "not BuildRenderTree(RenderTreeBuilder)", StringComparison.Ordinal);
    }

    private static string InvokeEscape(string value)
    {
        var method = typeof(RazorTailOutput).GetMethod(
            "EscapeCSharpString",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, [value])!;
    }
}
