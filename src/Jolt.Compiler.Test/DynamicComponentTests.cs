using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class DynamicComponentTests
{
    [TestMethod]
    public async Task Compile_DynamicChoice_SelectsTypedVnodesWithAuthoredKey()
    {
        var vue = new NpmVueComponent("Detail", "default", "", "", "Detail.vue", "Detail.vue", new("Detail.vue", 0, 0),
            new Dictionary<string, NpmVueProp> { ["caption"] = new("string", true, [], null) },
            new Dictionary<string, NpmVueEvent>(), new Dictionary<string, NpmVueSlot>())
        {
            LocalNamespace = "App",
            LocalPath = "Detail.vue",
        };
        var result = await JoltProjectCompiler.CompileAsync([
            new("Page.jazor", """
                <component is="@(SelectDetail() ? (Detail) : Summary)" Caption="@Caption" @key="1" />
                @code {
                    private string Caption = "hello";
                    private bool SelectDetail() => Caption == "hello";
                }
                """),
            new("Summary.jazor", "<div>@Caption</div>\n@code { [Parameter] public required string Caption { get; set; } }"),
        ], new JoltCompileOptions { VueComponents = [vue] });

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var page = result.Modules.Single(module => module.Name == "Page");
        StringAssert.Contains(page.Content, "Detail.vue");
        StringAssert.Contains(page.Content, "Summary");
        Assert.AreEqual(1, page.Content.Split(".SelectDetail()").Length - 1);
        Assert.AreEqual(2, page.Content.Split("\"key\": 1").Length - 1);
        Assert.DoesNotContain("\"is\":", page.Content);
    }

    [TestMethod]
    [DataRow("<component />", "JOLT1042")]
    [DataRow("<component is='Summary' />", "JOLT1042")]
    [DataRow("<component is='@(\"Summary\")' />", "JOLT1042")]
    [DataRow("<component is='@(false ? Summary : Missing)' />", "JOLT1042")]
    [DataRow("<component is='@(1 ? Summary : Details)' Caption='text' />", "CS0029")]
    [DataRow("<component is='@(true ? Summary : Details)' />", "JOLT1005")]
    [DataRow("<component is='@(true ? Summary : Details)' Caption='@(42)' />", "CS1503")]
    [DataRow("<component is='@(true ? Summary : Details)' Caption='text'><template #detail>Hi</template></component>", "JOLT1007")]
    public void Analyze_DynamicChoice_ValidatesEachCandidateAtTheAuthorLocation(string markup, string expected)
    {
        var result = JoltProjectCompiler.Analyze([
            new("Page.jazor", markup),
            new("Summary.jazor", "<div><slot name='detail' /></div>\n@code { [Parameter] public string Caption { get; set; } = \"\"; }"),
            new("Details.jazor", "<div />\n@code { [Parameter] public required string Caption { get; set; } }"),
        ]);

        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == expected &&
            diagnostic.Location.GetLineSpan().Path == "Page.jazor" && diagnostic.Location.SourceSpan.Length > 0),
            string.Join(Environment.NewLine, result.Diagnostics));
    }

    [TestMethod]
    public void Analyze_DynamicChoice_InfersSharedGenericSlotChildInEachCandidateScope()
    {
        var result = JoltProjectCompiler.Analyze([
            new("Page.jazor", """
                <component is="@(ShowSummary ? Summary : Details)">
                    <template #detail context="item"><Echo Value="@item.Value" /></template>
                </component>
                @code { private bool ShowSummary = true; }
                """),
            new("Summary.jazor", "<slot name='detail' Value='@(42)' />"),
            new("Details.jazor", "<slot name='detail' Value='caption' />"),
            new("Echo.jazor", """
                @typeparam T
                <div />
                @code { [Parameter] public required T Value { get; set; } }
                """),
        ]);

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
    }
}
