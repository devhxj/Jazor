using Acornima;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialModuleIntegrityRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorGeneratedModule_PassesFinalAstIntegrityValidation()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ModuleIntegrityAuthoring.razor"),
            documentText:
            """
            <button @onclick="Increment">Save</button>
            <span data-count="@Count">@Count</span>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/module-integrity-authoring")]
            public partial class ModuleIntegrityAuthoring : ComponentBase, IVueComponent
            {
                private int Count { get; set; }

                private void Increment() => Count++;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ModuleIntegrityAuthoring");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        var module = new Parser().ParseModule(observation.ModuleText);
        VueModuleIntegrityValidator.Validate(module);
        CollectionAssert.AreEquivalent(
            Array.Empty<string>(),
            VueModuleIntegrityValidator.FindUnboundIdentifiers(module).ToArray());
    }
}
