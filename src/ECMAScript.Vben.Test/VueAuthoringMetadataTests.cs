using System.Linq;
using ECMAScript.VueContract;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis.CSharp;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VueAuthoringMetadataTests
{
    [TestMethod]
    public void RazorVue_Context_DiscoversLibraryComponentDescriptors_FromGeneralVuePropAndVueSlotMetadata()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Ui.Custom
            {
                public sealed record FooterContext;

                [VueLibraryComponent("demo/components", "DemoPanel")]
                [VueProp(nameof(Label), VuePropKind.HtmlLike, Name = "panelLabel", Required = true, DefaultExpression = "'Overview'", AcceptsBinding = true)]
                [VueSlot(nameof(Footer), Name = "actions", Required = true, ContextTypeName = "Demo.Ui.Custom.FooterContext", ContextParameterName = "footer")]
                public sealed class DemoPanel : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Label { get; set; }

                    [Parameter]
                    public EventCallback<string> LabelChanged { get; set; }

                    [Parameter]
                    public RenderFragment<FooterContext>? Footer { get; set; }
                }
            }
            """);

        var descriptor = context.DiscoverLibraryComponents()
            .Single(static descriptor => descriptor.FullName == "Demo.Ui.Custom.DemoPanel");

        var label = descriptor.Props.Single(static prop => prop.PublicName == "Label");
        Assert.AreEqual("panelLabel", label.Name);
        Assert.IsTrue(label.Required);
        Assert.IsTrue(label.AcceptsBinding);
        Assert.AreEqual("'Overview'", label.DefaultExpression);
        Assert.AreEqual(VuePropKind.HtmlLike, label.Kind);

        var footer = descriptor.Slots.Single(static slot => slot.PublicName == "Footer");
        Assert.AreEqual("actions", footer.Name);
        Assert.IsTrue(footer.Required);
        Assert.HasCount(1, footer.Parameters);
        Assert.AreEqual("footer", footer.Parameters[0].Name);
        Assert.AreEqual("Demo.Ui.Custom.FooterContext", footer.Parameters[0].TypeName);
    }

    [TestMethod]
    public void VueContract_ExportsOnlyCanonicalGeneralMetadataAttributes()
    {
        Assert.AreEqual("ECMAScript.VueContract.VuePropAttribute", typeof(VuePropAttribute).FullName);
        Assert.AreEqual("ECMAScript.VueContract.VueSlotAttribute", typeof(VueSlotAttribute).FullName);
        Assert.AreEqual(typeof(Attribute), typeof(VuePropAttribute).BaseType);
        Assert.AreEqual(typeof(Attribute), typeof(VueSlotAttribute).BaseType);
        Assert.IsFalse(typeof(VuePropAttribute).IsDefined(typeof(ObsoleteAttribute), inherit: false));
        Assert.IsFalse(typeof(VueSlotAttribute).IsDefined(typeof(ObsoleteAttribute), inherit: false));
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Vue.Authoring.Metadata.Compatibility.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context!;
    }
}
