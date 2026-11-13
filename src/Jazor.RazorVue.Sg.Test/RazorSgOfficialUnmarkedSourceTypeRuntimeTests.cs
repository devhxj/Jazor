namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialUnmarkedSourceTypeRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_UnmarkedTopLevelHelper_ExecutesFromDirectRender()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SourceHelper.razor"),
            documentText: "<p>@Formatter.Format(Title)</p>",
            codeBehindSource: """
                namespace Demo.Pages;
                [ECMAScriptModule("./components/source-helper")]
                public partial class SourceHelper : ComponentBase, IVueComponent
                {
                    [Parameter] public string Title { get; set; } = "";
                }
                internal static class Formatter
                {
                    public static string Format(string title) => Prefix.Value() + title;
                }
                internal static class Prefix
                {
                    public static string Value() => "release: ";
                }
                """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SourceHelper");
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "class Formatter", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "class Prefix", StringComparison.Ordinal);
        await RazorSgOfficialNodeRuntimeTestHost.RunModuleTestAsync(
            "components/source-helper.js", observation.ModuleText, "source-helper.test.mjs", """
                import assert from 'node:assert/strict';
                import component from './components/source-helper.js';
                globalThis.__jazorTest('unmarked source helpers in direct render', () => {
                  const render = component.setup({ Title: 'ready' }, { slots: {} });
                  assert.equal(render().children, 'release: ready');
                });
                """);
    }
}
