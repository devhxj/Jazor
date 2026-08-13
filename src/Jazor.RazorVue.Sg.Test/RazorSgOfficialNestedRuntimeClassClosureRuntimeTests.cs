namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNestedRuntimeClassClosureRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNestedRuntimeClass_ProjectsFieldPropertyAndHelperInvocationOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NestedRuntimeClassClosureRuntime.razor",
            documentText:
            """
            <article data-release="@formatter.Key">@formatter.Format(Title)</article>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nested-runtime-class-closure-runtime")]
            public partial class NestedRuntimeClassClosureRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = string.Empty;

                private readonly ReleaseFormatter formatter = new("release");

                private sealed class ReleaseFormatter
                {
                    private readonly string prefix;

                    public ReleaseFormatter(string prefix)
                    {
                        this.prefix = prefix;
                    }

                    public string Key => prefix + "-key";

                    public string Format(string title)
                    {
                        return Combine(title);
                    }

                    private string Combine(string title)
                    {
                        return prefix + ": " + title;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NestedRuntimeClassClosureRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "formatter.Key", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "formatter.Format(Title)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "class ReleaseFormatter", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "new ReleaseFormatter(\"release\")", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Combine(title)", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nested-runtime-class-closure-runtime.mjs",
            observation.ModuleText,
            "official-nested-runtime-class-closure-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nested-runtime-class-closure-runtime.mjs";

            test("official Razor nested runtime classes retain reachable field, property, and helper semantics", () => {
                const render = component.setup({ Title: "Deploy API" }, { slots: {} });
                const article = render();

                assert.equal(article.name, "article");
                assert.equal(article.props["data-release"], "release-key");
                assert.equal(article.children, "release: Deploy API");
            });
            """);
    }
}
