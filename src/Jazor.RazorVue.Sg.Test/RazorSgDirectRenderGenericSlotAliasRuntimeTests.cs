using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderGenericSlotAliasRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_DirectRenderGenericSlotAlias_ExpandsTemplateResultOnDenoHost()
    {
        const string source = """
            using ECMAScript;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Direct;

            [ECMAScriptModule("./components/direct-generic-slot-alias-runtime")]
            public sealed class DirectGenericSlotAlias : ComponentBase, IVueComponent
            {
                [ECMAScriptName("item")]
                [Parameter] public RenderFragment<string> ItemTemplate { get; set; } = default!;

                private string Current { get; } = "Deploy API";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    var template = ItemTemplate;
                    builder.OpenElement(0, "section");
                    builder.AddAttribute(1, "data-template-source", "local");
                    builder.AddContent(2, template, Current);
                    builder.CloseElement();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.DirectGenericSlotAlias.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("Demo.Direct.DirectGenericSlotAlias");
        Assert.IsNotNull(componentSymbol);
        Assert.IsTrue(
            RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var component = binding!.Components.Single();
        Assert.IsTrue(
            RazorSgComponentMemberClosureBuilder.TryBuild(
                binding,
                component,
                out var closure,
                out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(binding, component, closure!);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(artifact.ModuleText);
        StringAssert.Contains(artifact.ModuleText, "slots.item", StringComparison.Ordinal);
        StringAssert.Contains(artifact.ModuleText, "[].concat", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/direct-generic-slot-alias-runtime.mjs",
            artifact.ModuleText,
            "direct-generic-slot-alias-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/direct-generic-slot-alias-runtime.mjs";

            test("direct generic slot aliases expand the returned VNode sequence", () => {
                const section = component.setup({}, {
                    slots: {
                        item: item => [
                            {
                                name: "strong",
                                props: { "data-item": item },
                                children: ["Template: " + item]
                            }
                        ]
                    }
                })();

                assert.equal(section.name, "section");
                assert.equal(section.props["data-template-source"], "local");
                assert.equal(section.children.length, 1);
                assert.equal(section.children[0].name, "strong");
                assert.equal(section.children[0].props["data-item"], "Deploy API");
                assert.deepEqual(section.children[0].children, ["Template: Deploy API"]);
            });
            """);
    }
}
