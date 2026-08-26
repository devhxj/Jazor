using System.Reflection;
using ECMAScript;
using ECMAScript.VuIcons;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgVuIconsLibraryComponentTests
{
    [TestMethod]
    public void BuildComponent_OfficialRazorVuIcon_RejectsRetiredIconAlias()
    {
        var exception = Assert.Throws<AssertFailedException>(
            () => RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
                documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/RetiredIconAlias.razor"),
                documentText:
                """
                @using ECMAScript.VuIcons

                <VuIcon Icon="@VuIconName.Search" />
                """,
                codeBehindSource:
                """
                namespace Demo.Pages;

                [ECMAScriptModule("./components/retired-icon-alias")]
                public partial class RetiredIconAlias : ComponentBase, IVueComponent;
                """,
                rootNamespace: "Demo.Pages",
                componentMetadataName: "Demo.Pages.RetiredIconAlias")
                .GetAwaiter()
                .GetResult());

        StringAssert.Contains(exception.Message, "Icon", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorVuIcons_UsesStaticAndDynamicImportPaths()
    {
        var staticDescriptor = typeof(VuUser).GetCustomAttribute<ECMAScriptAttribute>();
        var dynamicDescriptor = typeof(VuIcon).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(staticDescriptor);
        Assert.IsNotNull(dynamicDescriptor);
        Assert.AreEqual("vu-icons/VuUser", staticDescriptor!.Import);
        Assert.AreEqual("vu-icons", dynamicDescriptor!.Import);
        Assert.AreEqual(Transform.Component, staticDescriptor.Transform);
        Assert.AreEqual(Transform.Component, dynamicDescriptor.Transform);

        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/Icons.razor"),
            documentText:
            """
            @using ECMAScript.VuIcons

            <VuUser Size="@(18)" Color="#0f766e" ClassName="toolbar-icon" Spin="true" />
            <VuIcon Name="@VuIconName.Search" Size="@(24)" Color="#2563eb" Class="search-icon" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VuIcons;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/icons")]
            public partial class Icons : ComponentBase, IVueComponent;
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.Icons");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VuUser } from \"vu-icons/VuUser\";",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VuIcon } from \"vu-icons\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "size: 18", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "size: 24", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "className: \"toolbar-icon\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "name: \"search\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "class: \"search-icon\"", StringComparison.Ordinal);
    }
}
