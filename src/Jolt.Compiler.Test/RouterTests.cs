using Jolt.Compiler;
using Microsoft.CodeAnalysis;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class RouterTests
{
    [TestMethod]
    public void Router_EmitsCaseSensitiveModulePathsAndTypedPropsWithLayout()
    {
        JoltSource[] sources =
        [
            new("Pages/Sku.jazor", """
                @using App.Shared
                @page "/catalog/{sku}/{id:int}"
                @layout MainLayout
                <b>@SkuName @Sku @Id</b>
                @code {
                    private string SkuName = "SKU";
                    [Parameter] public string Sku { get; set; } = "";
                    [Parameter] public int Id { get; set; }
                }
                """),
            new("Shared/MainLayout.jazor", "<main><slot /></main>"),
        ];
        var result = JoltProjectCompiler.Compile(sources, new() { Router = true });
        Assert.IsEmpty(result.Diagnostics, string.Join("\n", result.Diagnostics));
        Assert.IsNotNull(result.RouterModule);
        StringAssert.Contains(result.RouterModule, "\"./Pages/Sku.mjs\"");
        StringAssert.Contains(result.RouterModule, "\"./Shared/MainLayout.mjs\"");
        StringAssert.Contains(result.RouterModule, "/catalog/:sku/:id(");
        StringAssert.Contains(result.RouterModule, """["Sku"]: route.params["sku"]""");
        StringAssert.Contains(result.RouterModule, """["Id"]: routeInt(route.params["id"])""");
        StringAssert.Contains(result.RouterModule, "default: () => h(component0, attrs)");
        Assert.AreEqual(result.RouterModule, JoltProjectCompiler.Compile(sources.Reverse(), new() { Router = true }).RouterModule);
    }

    [TestMethod]
    [DataRow("@page \"/a/{id:int}\"", "\"/a/{id:int}\"", "requires one public [Parameter]")]
    [DataRow("@page \"/a/{id?}\"", "\"/a/{id?}\"", "Unsupported route segment")]
    [DataRow("@page \"/a\"\n@layout Missing", "Missing", "one local .jazor component")]
    public void Router_InvalidDirectivesPointToAuthoredArgument(string directives, string site, string message)
    {
        var source = new JoltSource("Page.jazor", directives + "\n<p />");
        var result = JoltProjectCompiler.Compile([source], new() { Router = true });
        var diagnostic = result.Diagnostics.Single(item => item.Severity == DiagnosticSeverity.Error);
        Assert.AreEqual("JOLT1043", diagnostic.Id);
        Assert.AreEqual(source.Path, diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(site, source.Content.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
        StringAssert.Contains(diagnostic.GetMessage(), message);
        Assert.IsNull(result.RouterModule);
    }

    [TestMethod]
    public void Router_RejectsAmbiguousShapesAndPreservesDisabledDiagnostics()
    {
        JoltSource[] sources =
        [
            new("First.jazor", "@page \"/A/{value}\"\n<p />\n@code { [Parameter] public string Value { get; set; } = \"\"; }"),
            new("Second.jazor", "@page \"/a/{other}\"\n<p />\n@code { [Parameter] public string Other { get; set; } = \"\"; }"),
        ];
        var disabled = JoltProjectCompiler.Compile(sources);
        Assert.HasCount(2, disabled.Diagnostics.Where(diagnostic => diagnostic.Id == "JOLT1022"));
        var enabled = JoltProjectCompiler.Compile(sources, new() { Router = true });
        var duplicate = enabled.Diagnostics.Single();
        Assert.AreEqual("JOLT1043", duplicate.Id);
        Assert.AreEqual("Second.jazor", duplicate.Location.GetLineSpan().Path);
        StringAssert.Contains(duplicate.GetMessage(), "duplicates another route");
    }
}
