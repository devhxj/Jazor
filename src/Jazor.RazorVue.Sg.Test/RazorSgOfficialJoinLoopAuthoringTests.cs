namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialJoinLoopAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorJoinLoops_UseCompilerOwnedEnumerableContracts()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseJoinSummary.razor",
            documentText:
            """
            @using System.Linq

            <ul data-summary="release-joins">
                @foreach (var match in OuterValues.Join(InnerValues, outer => outer % 2, inner => inner % 2, (outer, inner) => outer * 100 + inner))
                {
                    <li data-kind="match">@match</li>
                }
                @foreach (var summary in OuterValues.GroupJoin(InnerValues, outer => outer % 2, inner => inner % 2, (outer, matches) => outer * 10 + matches.Count()))
                {
                    <li data-kind="group">@summary</li>
                }
            </ul>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-join-summary")]
            public partial class ReleaseJoinSummary : ComponentBase, IVueComponent
            {
                private readonly int[] OuterValues = [1, 2, 3];
                private readonly int[] InnerValues = [10, 11, 12];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseJoinSummary");

        StringAssert.Contains(observation.GeneratedCSharp, ".Join(", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, ".GroupJoin(", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "System/Linq/EnumerableModule.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "_f10104b4c52b4f96", StringComparison.Ordinal);
        StringAssert.Contains(script, "_b61f41d1ac124b69", StringComparison.Ordinal);
        StringAssert.Contains(script, "_1cb3ec9a7fb8aaab", StringComparison.Ordinal);
        StringAssert.Contains(script, "data-kind", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(", StringComparison.Ordinal);
    }
}
