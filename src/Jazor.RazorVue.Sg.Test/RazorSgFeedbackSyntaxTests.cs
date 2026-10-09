using Jazor.Common.SourceMaps;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgFeedbackSyntaxTests
{
    private const string CodeBehind = """
        namespace Demo.Pages;
        [ECMAScriptModule("./components/syntax-feedback")]
        public partial class SyntaxFeedback : ComponentBase, IVueComponent
        {
            [Parameter] public bool Show { get; set; }
            [Parameter] public string Value { get; set; } = "";
            [Parameter] public string Next { get; set; } = "";
        }
        """;

    [TestMethod]
    [DataRow("@if (Show)\n{\n    <span>before</span><strong>@Value</strong>\n}")]
    [DataRow("@if (Show)\n{\n    <span>@($\"release:{Value}\")</span>\n}")]
    [DataRow("@if (Show)\n{\n    @: @Value → @Next\n}")]
    [DataRow("@if (Show)\n{\n    <text>@Value → @Next</text>\n}")]
    public async Task OfficialRazor_SyntaxForms_LowerWithoutRewritingTheFrontend(string documentText)
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/SyntaxFeedback.razor"),
            documentText, CodeBehind, "Demo.Pages", "Demo.Pages.SyntaxFeedback");
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        var sourceMap = new SourceMapReader().Read(observation.SourceMapContent);
        Assert.IsTrue(sourceMap.Sources.Any(source => source.Content == documentText));
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/syntax-feedback.js", observation.ModuleText, "syntax-feedback.test.mjs",
            """
            import assert from "node:assert/strict";
            import component from "./components/syntax-feedback.js";
            function text(node) {
              if (node == null) return "";
              if (Array.isArray(node)) return node.map(text).join("");
              if (typeof node !== "object") return String(node);
              return text(node.children);
            }
            const value = text(component.setup({ Show: true, Value: "alpha", Next: "beta" }, { slots: {} })());
            assert.match(value, /alpha/);
            assert.equal(text(component.setup({ Show: false }, { slots: {} })()), "");
            """);
    }

    [TestMethod]
    public async Task OfficialRazor_NakedTextInCodeBlock_ReportsSourceAuthoringDiagnostic()
    {
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/SyntaxFeedback.razor"),
            "@if (Show)\n{\n    @Value → @Next\n}", CodeBehind, "Demo.Pages");
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id is "CS1056" or "RZ1021"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task ExternDomEmptyPropertyPattern_AndPlainNullComparison_HaveAReproducibleBoundary()
    {
        var unsupported = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/DomNullFeedback.razor"),
            "<output>@Read()</output>",
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/dom-null-feedback")]
            public partial class DomNullFeedback : ComponentBase, IVueComponent
            {
                [Parameter] public ECMAScript.HTMLElement? Element { get; set; }
                private string Read() => Element is { } present ? present.Id : "missing";
            }
            """, "Demo.Pages");
        Assert.IsTrue(unsupported.Any(diagnostic => diagnostic.Id == "JAZORVGA026"),
            string.Join(Environment.NewLine, unsupported));
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/DomNullFeedback.razor"),
            "<output>@Read()</output>",
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/dom-null-feedback")]
            public partial class DomNullFeedback : ComponentBase, IVueComponent
            {
                [Parameter] public ECMAScript.HTMLElement? Element { get; set; }
                private string Read()
                {
                    var element = Element;
                    if (element != null) return element.Id;
                    return "missing";
                }
            }
            """,
            "Demo.Pages", "Demo.Pages.DomNullFeedback");
        Assert.IsFalse(observation.ModuleText.Contains("instanceof HTMLElement", StringComparison.Ordinal), observation.ModuleText);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/dom-null-feedback.js", observation.ModuleText, "dom-null-feedback.test.mjs",
            """
            import assert from "node:assert/strict";
            import component from "./components/dom-null-feedback.js";
            const render = Element => component.setup({ Element }, { slots: {} })();
            assert.equal(render({ id: "file-input" }).children, "file-input");
            assert.equal(render(null).children, "missing");
            """);
    }
}
