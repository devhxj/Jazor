using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrCompilerExpressionBridgeTests
{
    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersWhitelistedTemplateExpressions_UsingCompilerSemantics()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using System
            <section>@Math.Abs(Value) @DateOnly.Parse(RawDate).ToString()</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.CompilerExpressionBridge.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ModuleCode, "from \"System/DateOnlyModule.js\";");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }
}
