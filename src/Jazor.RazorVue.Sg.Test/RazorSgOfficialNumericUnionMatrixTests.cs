using ECMAScript.Vuetify;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNumericUnionMatrixTests
{
    [TestMethod]
    public async Task OfficialAuthoring_ThreeLibraries_AcceptsIntegerVariablesAndNumberUnionLiterals()
    {
        // Ensure the wrapper assembly participates in the host's metadata reference set.
        Assert.IsNotNull(typeof(VAvatar).GetProperty("Rounded"));
        var document = RazorSgTestHost.GetTestDocumentPath("Pages/BindingMatrix.razor");
        var sample = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(document)!,
            "../../../../samples/RazorVue.NumericUnion/BindingMatrix.razor"));
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(document,
            await File.ReadAllTextAsync(sample), await File.ReadAllTextAsync(sample + ".cs"), "Demo.Pages", "Demo.Pages.BindingMatrix");
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync("components/binding-matrix.js", observation.ModuleText,
            "binding-matrix.test.mjs", """
            import assert from "node:assert/strict";
            import component from "./components/binding-matrix.js";
            Deno.test("Razor SG and compiler preserve plain number literals and int/float model values", () => {
                const nodes = component.setup({}, { slots: {} })().children.filter(node => ["ElAvatar", "ElPagination", "VAvatar", "Tag", "span"].includes(node?.name));
                assert.equal(nodes[0].props.size, 32);
                assert.equal(nodes[1].props.size, 32);
                assert.equal(nodes[2].props.size, 32);
                assert.equal(nodes[2].props.rounded, 2);
                assert.equal(nodes[3].props.size, 1.5);
                assert.equal(nodes[3].props.rounded, 32);
                assert.equal(nodes[4].props.maxWidth, 32);
                assert.equal(nodes[5].props.maxWidth, 32);
                assert.equal(nodes[6].props.maxWidth, "32");
                assert.equal(nodes[7].props.currentPage, 32);
                assert.equal(nodes[8].props["data-value"], 0);
                assert.equal(nodes[8].props["data-text"] ?? null, null);
            });
            """, new Dictionary<string, string>
            {
                ["fixtures/components.mjs"] = "export const ElAvatar = 'ElAvatar'; export const ElPagination = 'ElPagination'; export const VAvatar = 'VAvatar'; export const Tag = 'Tag';",
                ["fixtures/empty-style.mjs"] = ""
            }, importSpecifiers: new Dictionary<string, string>
            {
                ["element-plus/es/components/avatar/index.mjs"] = "./fixtures/components.mjs",
                ["element-plus/es/components/pagination/index.mjs"] = "./fixtures/components.mjs",
                ["element-plus/es/components/avatar/style/css.mjs"] = "./fixtures/empty-style.mjs",
                ["element-plus/es/components/pagination/style/css.mjs"] = "./fixtures/empty-style.mjs",
                ["vuetify/components/VAvatar"] = "./fixtures/components.mjs",
                ["vuetify/lib/components/VAvatar/VAvatar.css"] = "./fixtures/empty-style.mjs",
                ["tdesign-vue-next/es/tag/index.mjs"] = "./fixtures/components.mjs",
                ["tdesign-vue-next/es/tag/style/index.css"] = "./fixtures/empty-style.mjs"
            });
    }
}
