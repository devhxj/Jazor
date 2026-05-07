using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueCompilerExpressionBridgeTests
{
    [TestMethod]
    public void RazorVue_Pipeline_LowersWhitelistedTemplateExpressions_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Math.Abs(Value));
                        builder.AddContent(2, DateOnly.Parse(RawDate).ToString());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ModuleCode, "from \"System/DateOnlyModule.js\";");
        StringAssert.Contains(artifact.ModuleCode, "(props.rawDate)");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_CarriesCompilerImports_ForTemplateExpressions()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Math.Abs(Value));
                        builder.AddContent(2, DateOnly.Parse(RawDate).ToString());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "from \"System/DateOnlyModule.js\";");
        StringAssert.Contains(artifact.ScriptSetupText, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ScriptSetupText, "(props.rawDate)");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ExpressionBridge.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }
}
