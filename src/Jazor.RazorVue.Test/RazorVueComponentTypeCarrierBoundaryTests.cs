using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueComponentTypeCarrierBoundaryTests
{
    [TestMethod]
    public void RazorVue_Pipeline_WithComponentTypeLocalCarrierUsedAsAttribute_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    var childType = typeof(ChildCard);
                    builder.OpenElement(0, "section");
                    builder.AddAttribute(1, "data-type", childType);
                    builder.CloseElement();
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithComponentTypeLocalCarrierUsedAsKey_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    var childType = typeof(ChildCard);
                    builder.OpenElement(0, "section");
                    builder.SetKey(childType);
                    builder.AddContent(1, "ready");
                    builder.CloseElement();
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithComponentTypeLocalCarrierUsedAsCondition_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    var childType = typeof(ChildCard);
                    if (childType is not null)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithComponentTypeLocalCarrierUsedAsLoopSource_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    var childType = typeof(ChildCard);
                    foreach (var item in new[] { childType })
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, item.Name);
                        builder.CloseElement();
                    }
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithBranchAssignedOpenComponentTypeLocalCarrier_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    Type childType;
                    if (UseAlternate)
                    {
                        childType = typeof(ChildCard);
                    }
                    else
                    {
                        childType = typeof(OtherCard);
                    }

                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithLoopAssignedOpenComponentTypeLocalCarrier_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    Type childType = typeof(ChildCard);
                    for (var index = 0; index < 1; index++)
                    {
                        childType = typeof(OtherCard);
                    }

                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithRefEscapedOpenComponentTypeLocalCarrier_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreePipeline().Execute(CreateContext(
                ComponentTypeCarrierSource(
                    """
                    var childType = typeof(ChildCard);
                    Replace(ref childType);
                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                    """,
                    extraMembers:
                    """
                    private static void Replace(ref Type childType)
                    {
                        childType = typeof(OtherCard);
                    }
                    """))));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentTypeOfLocalCarrier_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                builder.OpenComponent(0, childType);
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
        Assert.IsFalse(artifact.SfcText.Contains("childType", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentDirectTypeOf_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                builder.OpenComponent(0, typeof(ChildCard));
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentTypeOfPropertyCarrier_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                builder.OpenComponent(0, ChildType);
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """,
                extraMembers:
                """
                private Type ChildType => typeof(ChildCard);
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
        Assert.IsFalse(artifact.SfcText.Contains("ChildType", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentTypeOfReadonlyFieldCarrier_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                builder.OpenComponent(0, ChildType);
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """,
                extraMembers:
                """
                private readonly Type ChildType = typeof(ChildCard);
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
        Assert.IsFalse(artifact.SfcText.Contains("ChildType", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentTypeOfLocalForwardedFromMemberCarrier_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = ChildType;
                builder.OpenComponent(0, childType);
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """,
                extraMembers:
                """
                private Type ChildType => typeof(ChildCard);
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
        Assert.IsFalse(artifact.SfcText.Contains("childType", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("ChildType", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersOpenComponentTypeOfMemberForwardingChain_ToStaticComponentTemplate()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                builder.OpenComponent(0, ChildType);
                builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                builder.CloseComponent();
                """,
                extraMembers:
                """
                private Type PrimaryChildType => typeof(ChildCard);
                private Type ChildType => PrimaryChildType;
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, ":title=\"props.title\"");
        Assert.IsFalse(artifact.SfcText.Contains("ChildType", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("PrimaryChildType", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsAttribute_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                builder.OpenElement(0, "section");
                builder.AddAttribute(1, "data-type", childType);
                builder.CloseElement();
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsContent_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                builder.OpenElement(0, "section");
                builder.AddContent(1, childType);
                builder.CloseElement();
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsKey_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                builder.OpenElement(0, "section");
                builder.SetKey(childType);
                builder.AddContent(1, "ready");
                builder.CloseElement();
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsCondition_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                if (childType is not null)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, "ready");
                    builder.CloseElement();
                }
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsLoopSource_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            ComponentTypeCarrierSource(
                """
                var childType = typeof(ChildCard);
                foreach (var item in new[] { childType })
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, item.Name);
                    builder.CloseElement();
                }
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypeCarrierHost");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime value");
    }

    private static RazorVuePipeline CreateBuildRenderTreePipeline()
        => new(BuildRenderTreeTemplateFrontend.Instance);

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ComponentTypeCarrierBoundary.Tests",
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

    private static string ComponentTypeCarrierSource(string renderBody, string extraMembers = "")
        => $$"""
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
               [ECMAScript.ECMAScriptModule("./components/child-card")]
               public class ChildCard : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public string? Title { get; set; }
               }

               [ECMAScript.ECMAScriptModule("./components/other-card")]
               public class OtherCard : ComponentBase, IVueComponent
               {
               }

               [ECMAScript.ECMAScriptModule("./components/type-carrier-host")]
               public class TypeCarrierHost : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public bool UseAlternate { get; set; }

                   [Parameter]
                   public string? Title { get; set; }

                   protected override void BuildRenderTree(RenderTreeBuilder builder)
                   {
           {{Indent(renderBody, "            ")}}
                   }

           {{Indent(extraMembers, "        ")}}
               }
           }
           """;

    private static string Indent(string text, string indent)
        => string.Join(
            Environment.NewLine,
            text.Trim().Split(["\r\n", "\n"], StringSplitOptions.None).Select(line => indent + line));
}
