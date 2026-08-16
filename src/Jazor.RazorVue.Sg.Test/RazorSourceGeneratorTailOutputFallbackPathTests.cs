using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorTailOutputFallbackPathTests
{
    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ModuleAttributeWithoutPath_UsesStableAssemblyAndNamespaceArtifactPaths()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "ReleaseConsole",
            [CSharpSyntaxTree.ParseText(
                """
                global using ECMAScript;
                global using Microsoft.AspNetCore.Components;
                global using Microsoft.AspNetCore.Components.Rendering;
                global using static ECMAScript.Vue;

                [ECMAScriptModule]
                public sealed class ReleaseShell : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "shell");
                    }
                }

                namespace Demo.Pages
                {
                    [ECMAScriptModule]
                    public sealed class ReleaseStatus : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.OpenElement(0, "output");
                            builder.AddAttribute(1, "data-status", "ready");
                            builder.AddContent(2, "ready");
                            builder.CloseElement();
                        }
                    }
                }
                """,
                parseOptions,
                "Pages/ReleaseStatus.razor.g.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsTrue(result, DescribeDiagnostics(diagnostics));
        Assert.IsEmpty(diagnostics, DescribeDiagnostics(diagnostics));
        Assert.IsNotNull(catalogSource);
        StringAssert.Contains(catalogSource, "ReleaseConsole/ReleaseShell.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "ReleaseConsole/Demo/Pages/ReleaseStatus.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "internal static partial class ArtifactCatalog", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "ProducerId = \"jazor.vue\"", StringComparison.Ordinal);
        Assert.IsFalse(catalogSource.Contains("\"module-source\"", StringComparison.Ordinal), catalogSource);
        Assert.IsFalse(catalogSource.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), catalogSource);
        Assert.IsFalse(catalogSource.Contains("builder.finish()", StringComparison.Ordinal), catalogSource);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_SharedVueSfcInput_DeduplicatesAndOrdersAssets()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "ReleaseConsole",
            [CSharpSyntaxTree.ParseText(
                """
                global using ECMAScript;
                global using Microsoft.AspNetCore.Components;
                global using Microsoft.AspNetCore.Components.Rendering;
                global using static ECMAScript.Vue;

                namespace Demo.Components
                {
                    [ECMAScriptModule("./components/badges/ReleaseBadge.vue")]
                    public sealed class ReleaseBadge : ComponentBase, IVueComponent
                    {
                    }

                    [ECMAScriptModule("./components/cards/ReleaseCard.vue")]
                    public sealed class ReleaseCard : ComponentBase, IVueComponent
                    {
                    }
                }

                namespace Demo.Pages
                {
                    using Demo.Components;

                    [ECMAScriptModule("./components/pages/release-dashboard")]
                    public sealed class ReleaseDashboard : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.OpenComponent<ReleaseCard>(0);
                            builder.CloseComponent();
                            builder.OpenComponent<ReleaseBadge>(1);
                            builder.CloseComponent();
                        }
                    }

                    [ECMAScriptModule("./components/pages/release-history")]
                    public sealed class ReleaseHistory : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.OpenComponent<ReleaseCard>(0);
                            builder.CloseComponent();
                        }
                    }
                }
                """,
                parseOptions,
                "Pages/ReleaseDashboard.razor.g.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsEmpty(diagnostics, DescribeDiagnostics(diagnostics));
        Assert.IsNotNull(catalogSource);
        StringAssert.Contains(catalogSource, "components/pages/release-dashboard.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "components/pages/release-history.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "../badges/ReleaseBadge.vue.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "../cards/ReleaseCard.vue.mjs", StringComparison.Ordinal);
        var badgeAssetIndex = catalogSource.IndexOf(
            "artifactPath: \"components/badges/ReleaseBadge.vue\"",
            StringComparison.Ordinal);
        var cardAssetIndex = catalogSource.IndexOf(
            "artifactPath: \"components/cards/ReleaseCard.vue\"",
            StringComparison.Ordinal);
        Assert.IsTrue(badgeAssetIndex >= 0, catalogSource);
        Assert.IsTrue(cardAssetIndex > badgeAssetIndex, catalogSource);
        Assert.AreEqual(
            1,
            CountOccurrences(catalogSource, "artifactPath: \"components/cards/ReleaseCard.vue\""),
            catalogSource);
        Assert.IsFalse(catalogSource.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), catalogSource);
        Assert.IsFalse(catalogSource.Contains("builder.finish()", StringComparison.Ordinal), catalogSource);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
    }

    private static string DescribeDiagnostics(IEnumerable<RazorVueDiagnosticInfo> diagnostics)
        => string.Join(
            Environment.NewLine,
            diagnostics.Select(static diagnostic => diagnostic.Message));
}
