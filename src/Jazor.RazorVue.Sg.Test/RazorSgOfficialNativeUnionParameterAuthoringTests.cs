namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNativeUnionParameterAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNativeUnionParameter_BindsAndLowers()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NativeUnionParent.razor",
            documentText:
            """
            <UnionChild Mode="@Mode" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/native-union-parent")]
            public partial class NativeUnionParent : ComponentBase, IVueComponent
            {
                [Parameter]
                public ECMAScript.Vue.VueBooleanStringValue Mode { get; set; } = true;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NativeUnionParent",
            supportingSources: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [@"D:\repo\Demo\Components\UnionChild.cs"] =
                """
                namespace Demo.Pages;

                [ECMAScriptModule("./components/native-union-child")]
                public sealed class UnionChild : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public ECMAScript.Vue.VueBooleanStringValue Mode { get; set; }
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "UnionChild", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "Mode", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "native-union-child", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("VueBooleanStringValue", StringComparison.Ordinal));
    }
}
