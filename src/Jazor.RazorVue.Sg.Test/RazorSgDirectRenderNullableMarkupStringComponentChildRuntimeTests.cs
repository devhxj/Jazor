using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderNullableMarkupStringComponentChildRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_DirectRenderNullableMarkupStringChild_ExpandsToZeroOrOneVNodeOnDenoHost()
    {
        const string source = """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Direct;

            [ECMAScriptModule("./components/direct-nullable-markup-child")]
            public sealed class DirectNullableMarkupChild : ComponentBase, IVueComponent
            {
            }

            [ECMAScriptModule("./components/direct-nullable-markup-parent")]
            public sealed class DirectNullableMarkupParent : ComponentBase, IVueComponent
            {
                [Parameter] public MarkupString? Summary { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<DirectNullableMarkupChild>(0);
                    builder.AddContent(1, Summary);
                    builder.CloseComponent();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.DirectNullableMarkupChild.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("Demo.Direct.DirectNullableMarkupParent");
        Assert.IsNotNull(componentSymbol);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var component = binding!.Components.Single();
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(
                binding,
                component,
                out var closure,
                out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(artifact.ModuleText);
        StringAssert.Contains(artifact.ModuleText, "createRawMarkup", StringComparison.Ordinal);
        StringAssert.Contains(artifact.ModuleText, "[].concat", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/direct-nullable-markup-parent.mjs",
            artifact.ModuleText,
            "direct-nullable-markup-child-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/direct-nullable-markup-parent.mjs";
            import child from "./components/direct-nullable-markup-child.mjs";

            test("direct nullable markup component children preserve the RenderTreeBuilder empty-content contract", () => {
                const populated = component.setup(
                    { Summary: "<strong>Release ready</strong>" },
                    { slots: {} })();
                assert.equal(populated.name, child);
                assert.equal(Array.isArray(populated.children), true);
                assert.equal(populated.children.length, 1);
                assert.equal(populated.children[0].name, "__static");
                assert.equal(populated.children[0].props.html, "<strong>Release ready</strong>");

                const empty = component.setup({ Summary: null }, { slots: {} })();
                assert.equal(empty.name, child);
                assert.deepEqual(empty.children, []);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/direct-nullable-markup-child.mjs"] = "export default { name: \"direct-nullable-markup-child\" };"
            });
    }
}
