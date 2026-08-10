using Jazor.Common.SourceMaps;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialStaticSourceMapTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialStaticRazor_MapsGeneratedArtifactBackToOriginalDocument()
    {
        const string documentText =
            """
            <article data-page="release-notes">
                <h1>Release notes</h1>
                <p>Production deployment is scheduled.</p>
            </article>
            """;
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseNotes.razor",
            documentText: documentText,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-notes")]
            public partial class ReleaseNotes : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseNotes");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Release notes", StringComparison.Ordinal);
        StringAssert.Contains(observation.SourceMapContent, "\"file\": \"components/release-notes.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.SourceMapContent, "Pages/ReleaseNotes.razor", StringComparison.Ordinal);
        Assert.IsFalse(observation.SourceMapContent.Contains(".razor.g.cs", StringComparison.Ordinal), observation.SourceMapContent);

        var sourceMap = new SourceMapReader().Read(observation.SourceMapContent);
        var razorSource = sourceMap.Sources.Single(source =>
            string.Equals(source.Path, "Pages/ReleaseNotes.razor", StringComparison.Ordinal));
        Assert.AreEqual(documentText.ReplaceLineEndings("\n"), razorSource.Content?.ReplaceLineEndings("\n"));
    }
}
