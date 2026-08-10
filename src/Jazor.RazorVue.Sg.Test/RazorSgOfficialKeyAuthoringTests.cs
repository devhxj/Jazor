namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialKeyAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorKeyedLoop_EmitsStableVueKeysInsideLoopBody()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\KeyedLoop.razor",
            documentText:
            """
            @foreach (var item in Items)
            {
                <li @key="item.Id" data-id="@item.Id">@item.Name</li>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/keyed-loop")]
            public partial class KeyedLoop : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<KeyedItem> Items { get; set; } = [];
            }

            public sealed record KeyedItem(int Id, string Name);
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.KeyedLoop");

        var generatedSetKey = observation.GeneratedCSharp.IndexOf("__builder.SetKey(", StringComparison.Ordinal);
        var generatedKeyValue = observation.GeneratedCSharp.IndexOf("item.Id", generatedSetKey, StringComparison.Ordinal);
        Assert.IsTrue(generatedSetKey >= 0, observation.GeneratedCSharp);
        Assert.IsTrue(generatedSetKey < generatedKeyValue, observation.GeneratedCSharp);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "Array.from(props.Items ?? [], item =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "key: item.Id", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"data-id\": item.Id", StringComparison.Ordinal);
        StringAssert.Contains(script, "item.Name", StringComparison.Ordinal);

        var dataId = script.IndexOf("\"data-id\": item.Id", StringComparison.Ordinal);
        var key = script.IndexOf("key: item.Id", dataId, StringComparison.Ordinal);
        var content = script.IndexOf("item.Name", dataId, StringComparison.Ordinal);
        Assert.IsTrue(dataId < key, script);
        Assert.IsTrue(key < content, script);

        Assert.IsFalse(script.Contains("SetKey", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
