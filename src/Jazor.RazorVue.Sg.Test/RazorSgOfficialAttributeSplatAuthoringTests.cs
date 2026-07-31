namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialAttributeSplatAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorAttributeSplat_PreservesExplicitAttributePrecedence()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\AttributeSplat.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @attributes="InputAttributes" class="form-control" data-role="account-name" />
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/attribute-splat")]
            public partial class AttributeSplat : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AttributeSplat");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddAttribute", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"vue\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "mergeProps(props.inputAttributes, {", StringComparison.Ordinal);
        StringAssert.Contains(script, "class: \"form-control\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"data-role\": \"account-name\"", StringComparison.Ordinal);

        var splat = script.IndexOf("mergeProps(props.inputAttributes, {", StringComparison.Ordinal);
        var classAttribute = script.IndexOf("class: \"form-control\"", splat, StringComparison.Ordinal);
        var dataRole = script.IndexOf("\"data-role\": \"account-name\"", classAttribute, StringComparison.Ordinal);
        Assert.IsTrue(splat < classAttribute, script);
        Assert.IsTrue(classAttribute < dataRole, script);

        Assert.IsFalse(script.Contains("AddMultipleAttributes", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddAttribute", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
