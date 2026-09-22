using System.Reflection;
using ECMAScript;
using ECMAScript.Lucide;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgLucideLibraryComponentTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLucide_UsesNamedExportAndTypedParameters()
    {
        var descriptor = typeof(User).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(descriptor);
        Assert.AreEqual("lucide-vue-next", descriptor!.Import);
        Assert.AreEqual("User", typeof(User).GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/Lucide.razor"),
            documentText:
            """
            @using ECMAScript.Lucide

            <User Size="@(24)" StrokeWidth="@(2)" aria-label="Account" />
            """,
            codeBehindSource:
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/lucide")]
            public partial class Lucide : ComponentBase, IVueComponent;
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.Lucide");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { User } from \"lucide-vue-next\";", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "size: 24", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "strokeWidth: 2", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"aria-label\": \"Account\"", StringComparison.Ordinal);
    }
}
