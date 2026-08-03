namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialGroupedLoopAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorGroupedLoop_UsesCompilerOwnedGroupingContracts()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseBucketSummary.razor",
            documentText:
            """
            @using System.Linq

            <ul data-summary="release-buckets">
                @foreach (var bucket in Values.GroupBy(value => value % 2, value => value * 10))
                {
                    <li data-key="@bucket.Key">@bucket.Key: @bucket.Count()</li>
                }
            </ul>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-bucket-summary")]
            public partial class ReleaseBucketSummary : ComponentBase, IVueComponent
            {
                private readonly int[] Values = [1, 2, 3, 4];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseBucketSummary");

        StringAssert.Contains(observation.GeneratedCSharp, "GroupBy", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "System/Linq/EnumerableModule.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "System/Linq/GroupingT2Module.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "_e62121525c074f74", StringComparison.Ordinal);
        StringAssert.Contains(script, "_44a1c9f2c4f246e9", StringComparison.Ordinal);
        StringAssert.Contains(script, "_1cb3ec9a7fb8aaab", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(", StringComparison.Ordinal);
        StringAssert.Contains(script, "data-key", StringComparison.Ordinal);
    }
}
