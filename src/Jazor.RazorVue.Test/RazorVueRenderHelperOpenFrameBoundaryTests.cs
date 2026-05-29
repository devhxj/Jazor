using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueRenderHelperOpenFrameBoundaryTests
{
    [TestMethod]
    public void CreateRenderTree_WithRefParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                var title = Title;
                RenderBody(builder, ref title);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, ref string? title)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "parameter");
    }

    [TestMethod]
    public void CreateRenderTree_WithOutParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                RenderBody(builder, out _);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, out string? title)
                {
                    title = "fallback";
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "out", "parameter");
    }

    [TestMethod]
    public void CreateRenderTree_WithByReferenceRenderTreeBuilderParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                RenderBody(ref builder);
                """,
                """
                private void RenderBody(ref RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, Title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "RenderTreeBuilder");
        StringAssert.Contains(exception.Issue.Message, "by-value");
    }

    [TestMethod]
    public void CreateRenderTree_WithInRenderTreeBuilderParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                RenderBody(in builder);
                """,
                """
                private void RenderBody(in RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, Title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "in", "RenderTreeBuilder");
        StringAssert.Contains(exception.Issue.Message, "by-value");
    }

    [TestMethod]
    public void CreateRenderTree_WithRecursiveRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                RenderBody(builder);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    RenderBody(builder);
                }
                """));

        AssertRenderHelperBoundary(exception, "recursive");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRecursiveRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertSfcThrows(
            RenderHelperSource(
                """
                RenderBody(builder);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    RenderBody(builder);
                }
                """));

        AssertRenderHelperBoundary(exception, "recursive");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRefParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertPipelineFails(
            RenderHelperSource(
                """
                var title = Title;
                RenderBody(builder, ref title);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, ref string? title)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "parameter");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperLeavingElementOpen_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "frame stack unbalanced");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperClosingCallerElement_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "frame stack unbalanced");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperLeavingRegionOpen_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.OpenRegion(1);
                    builder.OpenElement(2, "span");
                    builder.AddContent(3, title);
                    builder.CloseElement();
                }
                """,
                includePanel: true));

        AssertRenderHelperBoundary(exception, "caller-owned", "frame stack unbalanced");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperChangingActiveFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.CloseElement();
                    builder.OpenElement(1, "article");
                    builder.AddContent(2, title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "active caller-owned node");
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedHelperLeavingElementOpen_ThrowsCanonicalizationFailed()
    {
        var exception = AssertPipelineFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "frame stack unbalanced");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithGenericHelperClassInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertSfcThrows(
            RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperBox<string>(Title);
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.Value);
                    builder.CloseElement();
                }
                """,
                """
                private sealed class HelperBox<T>
                {
                    public HelperBox(T value)
                    {
                        Value = value;
                    }

                    public T Value { get; }
                }
                """));

        AssertHelperClassBoundary(exception, "HelperBox", "generic helper classes are not supported");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRecordHelperTypeInImperativeRender_LowersStructurallyWithoutRuntimeHelperClass()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperRecord(Title);
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.Value);
                    builder.CloseElement();
                }
                """,
                """
                private sealed record HelperRecord(string? Value);
                """)))
            .Artifacts
            .Single();

        Assert.IsFalse(artifact.ModuleCode.Contains("class HelperRecord", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "let helper = { value: props.title };");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(helper.value);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithStructHelperTypeInImperativeRender_LowersStructurallyWithoutRuntimeHelperClass()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperStruct(Title);
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.Value);
                    builder.CloseElement();
                }
                """,
                """
                private readonly struct HelperStruct
                {
                    public HelperStruct(string? value)
                    {
                        Value = value;
                    }

                    public string? Value { get; }
                }
                """)))
            .Artifacts
            .Single();

        Assert.IsFalse(artifact.ModuleCode.Contains("class HelperStruct", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "let helper = { value: props.title };");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(helper.value);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithECMAScriptHostDataCarrierInImperativeRender_LowersStructurallyWithoutRuntimeHelperClass()
    {
        var context = CreateContext(RenderHelperSource(
            """
            lock (this)
            {
                var helper = new ExternalHelper(Title);
                builder.OpenElement(0, "section");
                builder.AddContent(1, helper.Value);
                builder.CloseElement();
            }
            """,
            """
            [ECMAScript.ECMAScriptModule("./helpers/external-helper")]
            private sealed class ExternalHelper
            {
                public ExternalHelper(string? value)
                {
                    Value = value;
                }

                public string? Value { get; }
            }
            """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        Assert.IsFalse(artifact.SfcText.Contains("class ExternalHelper", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("from \"./helpers/external-helper\"", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let helper = { value: props.title };");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(helper.value);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithComponentHelperClassInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertPipelineThrows(
            RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperComponent();
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.GetType().Name);
                    builder.CloseElement();
                }
                """,
                """
                [ECMAScript.ECMAScriptModule("./components/helper-component")]
                private sealed class HelperComponent : ComponentBase, IVueComponent
                {
                }
                """));

        AssertHelperClassBoundary(exception, "HelperComponent");
    }

    private static RazorVueCompilationIssueException AssertCreateRenderTreeFails(string source)
    {
        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        return exception;
    }

    private static RazorVueCompilationIssueException AssertSfcFails(string source)
    {
        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        return exception;
    }

    private static RazorVueCompilationIssueException AssertSfcThrows(string source)
    {
        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        return Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot));
    }

    private static RazorVueCompilationIssueException AssertPipelineFails(string source)
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(CreateContext(source)));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        return exception;
    }

    private static RazorVueCompilationIssueException AssertPipelineThrows(string source)
        => Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(CreateContext(source)));

    private static void AssertRenderHelperBoundary(RazorVueCompilationIssueException exception, params string[] expectedFragments)
    {
        StringAssert.Contains(exception.Issue.Message, "helper method");
        foreach (var fragment in expectedFragments)
            StringAssert.Contains(exception.Issue.Message, fragment);
    }

    private static void AssertHelperClassBoundary(RazorVueCompilationIssueException exception, params string[] expectedFragments)
    {
        Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "helper type");
        foreach (var fragment in expectedFragments)
            StringAssert.Contains(exception.Issue.Message, fragment);
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RenderHelperOpenFrameBoundary.Tests",
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

    private static string RenderHelperSource(
        string renderBody,
        string extraMembers,
        bool includePanel = false)
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
           {{(includePanel ? """
               [ECMAScript.ECMAScriptModule("./components/panel")]
               public class Panel : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public RenderFragment? ChildContent { get; set; }
               }

           """ : string.Empty)}}    [ECMAScript.ECMAScriptModule("./components/render-helper-host")]
               public class RenderHelperHost : ComponentBase, IVueComponent
               {
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
