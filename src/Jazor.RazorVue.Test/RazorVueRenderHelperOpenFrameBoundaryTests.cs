using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueRenderHelperOpenFrameBoundaryTests
{
    [TestMethod]
    public void CreateRenderTree_WithWritingRefParameterRenderHelper_ThrowsCanonicalizationFailed()
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
                    title = "updated";
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "writeback");
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
    public void CreateRenderTree_WithReadOnlyRefParameterRenderHelper_ProducesStructuredNodes()
    {
        var context = CreateContext(
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

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length);
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("title", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(local.Initializer);

        var templateScope = renderTree.Children[1] as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithReadOnlyRefParameterRenderHelper_LowersTemplateScope()
    {
        var context = CreateContext(
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
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithWritingRefParameterRenderHelper_ThrowsCanonicalizationFailed()
    {
        var exception = AssertSfcFails(
            RenderHelperSource(
                """
                var title = Title;
                RenderBody(builder, ref title);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, ref string? title)
                {
                    title = "updated";
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "writeback");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithCallerOwnedHelperChildEmission_LowersTemplateReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, Title);
                    builder.CloseElement();
                }
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.title }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithCallerOwnedExpressionBodiedHelperAttributeMutation_LowersTemplateReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                    => builder.AddAttribute(1, "class", title);
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        // A caller-owned replay that mutates a captured-value attribute uses a synthetic
        // __jazor$ binding slot; assert the combined SFC text rather than TemplateText,
        // since the captured binding may be rendered through the render-function carrier.
        StringAssert.Contains(artifact.SfcText, "section");
        StringAssert.Contains(artifact.SfcText, ":class");
        Assert.IsFalse(artifact.SfcText.Contains("RenderBody", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithCallerOwnedConditionalHelperChildEmission_LowersTemplateReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
                """,
                includeRenderFlags: true));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        // Conditional caller-owned child replay with captured values lowers through the
        // render-function carrier; assert the combined SFC text for the replayed structure.
        StringAssert.Contains(artifact.SfcText, "section");
        StringAssert.Contains(artifact.SfcText, "span");
        Assert.IsFalse(artifact.SfcText.Contains("RenderBody", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithCallerOwnedGuardReturnHelperAttributeMutation_LowersTemplateReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                    }

                    builder.AddAttribute(1, "class", title);
                }
                """,
                includeRenderFlags: true));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        // The guard-return caller-owned replay lowers through the render-function carrier:
        // enter section, capture showPrimary, guard with !showPrimary, then setAttribute class.
        StringAssert.Contains(artifact.SfcText, "enterElement(\"section\")");
        StringAssert.Contains(artifact.SfcText, "setAttribute(\"class\"");
        StringAssert.Contains(artifact.SfcText, "if (!showPrimary)");
        Assert.IsFalse(artifact.SfcText.Contains("RenderBody", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVuePipeline_WithReadOnlyRefParameterRenderHelper_LowersRenderFunction()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
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
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const title = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorNodes.push(((title) => h(\"section\", null, title))(title));");
    }

    [TestMethod]
    public void CreateRenderTree_WithRefParameterRenderHelperForwardingByReference_ThrowsCanonicalizationFailed()
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
                    ConsumeByRef(ref title);
                    builder.AddContent(1, title);
                }

                private static void ConsumeByRef(ref string? value)
                {
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "by-reference invocation");
    }

    [TestMethod]
    public void CreateRenderTree_WithReadOnlyRefParameterRenderHelperAndUnusedNestedByRefLocalFunction_ProducesStructuredNodes()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                var title = Title;
                RenderBody(builder, ref title);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, ref string? title)
                {
                    void Unused(ref string? value)
                    {
                        ConsumeByRef(ref value);
                    }

                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }

                private static void ConsumeByRef(ref string? value)
                {
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length);
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        var templateScope = renderTree.Children[1] as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("section", ((RazorVueElementNode)templateScope.Children.Children.Single()).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithRefParameterRenderHelperIncrement_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                var count = 1;
                RenderBody(builder, ref count);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, ref int count)
                {
                    count++;
                    builder.AddContent(1, count);
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "writeback");
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
    public void CreateRenderTree_WithTerminatingRecursiveRenderHelper_ProducesImperativeMethodBody()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                RenderBody(builder, 2);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    builder.OpenElement(0, "span");
                    builder.AddContent(1, count);
                    builder.CloseElement();
                    RenderBody(builder, count - 1);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var imperative = renderTree.Children.Single() as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
    }

    [TestMethod]
    public void RazorVuePipeline_WithTerminatingRecursiveRenderHelper_LowersRenderHelperFunction()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                RenderBody(builder, 2);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    builder.OpenElement(0, "span");
                    builder.AddContent(1, count);
                    builder.CloseElement();
                    RenderBody(builder, count - 1);
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "function __jazorRenderHelper_renderBody_");
        StringAssert.Contains(artifact.ModuleCode, "(__jazorRenderContext, count) {");
        StringAssert.Contains(artifact.ModuleCode, "if (count <= 0)");
        StringAssert.Contains(artifact.ModuleCode, "return;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"span\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(count);");
        StringAssert.Contains(artifact.ModuleCode, "(__jazorRenderContext, count - 1);");
        StringAssert.Contains(artifact.ModuleCode, "(__jazorRenderContext, 2);");

        var helperStart = artifact.ModuleCode.IndexOf("function __jazorRenderHelper_renderBody_", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, helperStart);
        var componentStateStart = artifact.ModuleCode.IndexOf("const __jazorComponent", helperStart, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, componentStateStart);
        var helperBody = artifact.ModuleCode.Substring(helperStart, componentStateStart - helperStart);
        Assert.IsFalse(helperBody.Contains("return __jazorRenderContext.finish();", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithTerminatingRecursiveRenderHelper_LowersRenderHelperFunction()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                RenderBody(builder, 2);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    builder.OpenElement(0, "span");
                    builder.AddContent(1, count);
                    builder.CloseElement();
                    RenderBody(builder, count - 1);
                }
                """));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.SfcText, "function __jazorRenderHelper_renderBody_");
        StringAssert.Contains(artifact.SfcText, "(__jazorRenderContext, count) {");
        StringAssert.Contains(artifact.SfcText, "(__jazorRenderContext, count - 1);");
        StringAssert.Contains(artifact.SfcText, "(__jazorRenderContext, 2);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithOverloadedRecursiveRenderHelper_UsesDistinctStableHelperAliases()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                RenderBody(builder, 2);
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    builder.OpenElement(0, "span");
                    builder.AddContent(1, count);
                    builder.CloseElement();
                    RenderBody(builder, count.ToString());
                    RenderBody(builder, count - 1);
                }

                private void RenderBody(RenderTreeBuilder builder, string text)
                {
                    builder.OpenElement(2, "em");
                    builder.AddContent(3, text);
                    builder.CloseElement();
                }
                """)))
            .Artifacts
            .Single();

        var helperPrefix = "function __jazorRenderHelper_renderBody_";
        var firstHelperIndex = artifact.ModuleCode.IndexOf(helperPrefix, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, firstHelperIndex);
        var secondHelperIndex = artifact.ModuleCode.IndexOf(helperPrefix, firstHelperIndex + helperPrefix.Length, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, secondHelperIndex);
        Assert.AreNotEqual(firstHelperIndex, secondHelperIndex);
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"span\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"em\");");
    }

    [TestMethod]
    public void CreateRenderTree_WithTerminatingRecursiveLocalFunctionRenderHelper_ProducesImperativeLocalFunctionSegment()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    localBuilder.OpenElement(0, "span");
                    localBuilder.AddContent(1, count);
                    localBuilder.CloseElement();
                    RenderBody(localBuilder, count - 1);
                }

                RenderBody(builder, 2);
                """,
                ""));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var imperative = renderTree.Children.Single() as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        Assert.IsInstanceOfType<ILocalFunctionOperation>(imperative.Operations[0]);
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IExpressionStatementOperation));
    }

    [TestMethod]
    public void RazorVuePipeline_WithTerminatingRecursiveLocalFunctionRenderHelper_LowersLocalFunction()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    localBuilder.OpenElement(0, "span");
                    localBuilder.AddContent(1, count);
                    localBuilder.CloseElement();
                    RenderBody(localBuilder, count - 1);
                }

                RenderBody(builder, 2);
                """,
                string.Empty)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "function RenderBody(localBuilder, count)");
        StringAssert.Contains(artifact.ModuleCode, "if (count <= 0)");
        StringAssert.Contains(artifact.ModuleCode, "return;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"span\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(count);");
        StringAssert.Contains(artifact.ModuleCode, "RenderBody(__jazorRenderContext, count - 1);");
        StringAssert.Contains(artifact.ModuleCode, "RenderBody(__jazorRenderContext, 2);");
    }

    [TestMethod]
    public void CreateRenderTree_WithRecursiveCallerOwnedOpenFrameMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, 2);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    builder.AddAttribute(1, "data-count", count);
                    RenderBody(builder, count - 1);
                }
                """));

        AssertRenderHelperBoundary(exception, "recursive", "caller-owned");
    }

    [TestMethod]
    public void CreateRenderTree_WithRecursiveLocalFunctionCallerOwnedOpenFrameMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, int count)
                {
                    if (count <= 0)
                    {
                        return;
                    }

                    localBuilder.AddAttribute(1, "data-count", count);
                    RenderBody(localBuilder, count - 1);
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, 2);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "recursive", "caller-owned");
    }

    [TestMethod]
    public void RazorVuePipeline_WithWritingRefParameterRenderHelper_ThrowsCanonicalizationFailed()
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
                    title = "updated";
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "ref", "writeback");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNoExtraParameterHelperAddingChild_ProducesChildReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, Title);
                    builder.CloseElement();
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var span = section.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);

        var childReplay = section.ReplayOperations.OfType<RazorVueOpenNodeChildReplayOperation>().Single();
        var replaySpan = childReplay.Child as RazorVueElementNode;
        Assert.IsNotNull(replaySpan);
        Assert.AreEqual("span", replaySpan.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedExpressionBodiedHelperAttributeMutation_PreservesReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                    => builder.AddAttribute(1, "class", title);
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var attribute = section.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var attributeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var replayAttribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(replayAttribute);
        Assert.AreEqual("class", replayAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(replayAttribute.Value);
        Assert.IsTrue(replayAttribute.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedBuilderAliasHelperAttributeMutation_PreservesReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                var alias = builder;
                alias.OpenElement(0, "section");
                RenderBody(alias, Title);
                alias.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    var alias = builder;
                    alias.AddAttribute(1, "class", title);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var attribute = section.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var attributeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var replayAttribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(replayAttribute);
        Assert.AreEqual("class", replayAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(replayAttribute.Value);
        Assert.IsTrue(replayAttribute.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedThisQualifiedHelperAttributeMutation_PreservesReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                this.RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    builder.AddAttribute(1, "class", title);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var attribute = section.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var attributeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var replayAttribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(replayAttribute);
        Assert.AreEqual("class", replayAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(replayAttribute.Value);
        Assert.IsTrue(replayAttribute.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithNestedCallerOwnedHelpers_PreservesFrameAndCapturedReplayOrder()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderOuter(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderOuter(RenderTreeBuilder builder, string? title)
                {
                    builder.AddAttribute(1, "class", title);
                    RenderInner(builder, title);
                }

                private void RenderInner(RenderTreeBuilder builder, string? title)
                {
                    builder.OpenElement(2, "span");
                    builder.AddContent(3, title);
                    builder.CloseElement();
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(1, section.Attributes.Length);

        var attribute = section.Attributes[0] as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.CapturedBindings[0].Initializer);

        var span = section.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);

        Assert.AreEqual(1, section.ReplayOperations.Length);
        var outerScopedReplay = section.ReplayOperations[0] as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(outerScopedReplay);
        Assert.AreEqual(1, outerScopedReplay.CapturedBindings.Length);
        Assert.AreEqual("title", outerScopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScopedReplay.CapturedBindings[0].Initializer);

        Assert.AreEqual(2, outerScopedReplay.Operations.Length);
        Assert.IsInstanceOfType<RazorVueOpenNodeAttributeReplayOperation>(outerScopedReplay.Operations[0]);
        Assert.IsInstanceOfType<RazorVueOpenNodeScopedReplayOperation>(outerScopedReplay.Operations[1]);
        var innerScopedReplay = (RazorVueOpenNodeScopedReplayOperation)outerScopedReplay.Operations[1];
        Assert.AreEqual(1, innerScopedReplay.CapturedBindings.Length);
        Assert.AreEqual("title", innerScopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(innerScopedReplay.CapturedBindings[0].Initializer);
        var childReplay = innerScopedReplay.Operations.Single() as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(childReplay);
        var replaySpan = childReplay.Child as RazorVueElementNode;
        Assert.IsNotNull(replaySpan);
        Assert.AreEqual("span", replaySpan.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperAttributeMutation_PreservesConditionalReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsTrue(section.Attributes.IsDefaultOrEmpty);
        Assert.IsTrue(section.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        Assert.AreEqual(2, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("showPrimary", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[1].ParameterSymbol.Name);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        var attributeReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var attribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.IsTrue(attribute.CapturedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedGenericHelperAttributeMutation_PreservesCapturedReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody<TTitle>(RenderTreeBuilder builder, TTitle title)
                {
                    builder.AddAttribute(1, "class", title);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var attribute = section.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.CapturedBindings[0].Initializer);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        Assert.AreEqual(1, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        var attributeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var replayAttribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(replayAttribute);
        Assert.AreEqual("class", replayAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(replayAttribute.Value);
        Assert.IsTrue(replayAttribute.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperTypeParameterToken_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            builder.AddAttribute(1, "data-type", typeof(TTitle).Name);
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperDefaultTypeParameterValue_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            builder.AddAttribute(1, "data-value", default(TTitle));
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperNewTypeParameterValue_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            builder.AddAttribute(1, "data-value", new TTitle());
            """,
            renderInvocation: "RenderBody<TestTitle>(builder);",
            constraintClause: "where TTitle : new()",
            extraMembersPrefix: """
            private sealed class TestTitle
            {
            }
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperIsTypeParameterCheck_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            builder.AddAttribute(1, "data-match", Title is TTitle);
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperTypeParameterDeclarationPattern_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            if (Title is TTitle value)
            {
                builder.AddAttribute(1, "data-value", value);
            }
            """);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedGenericHelperAndUnusedTypeParameterLocalFunction_PreservesCapturedReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody<TTitle>(RenderTreeBuilder builder, TTitle title)
                {
                    string GetTypeName()
                    {
                        return typeof(TTitle).Name;
                    }

                    builder.AddAttribute(1, "class", title);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var attribute = section.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedGenericHelperAndUnusedTypeParameterLambda_ThrowsCallableLocalBoundaryWithoutScanningLambdaBody()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody<TTitle>(RenderTreeBuilder builder, TTitle title)
                {
                    Func<string> getTypeName = () => typeof(TTitle).Name;

                    builder.AddAttribute(1, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "callable template state");
        Assert.IsFalse(exception.Issue.Message.Contains("runtime generic type-parameter semantics", StringComparison.Ordinal), exception.Issue.Message);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperInvokedTypeParameterLocalFunction_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            string GetValue()
            {
                var value = default(TTitle);
                return value?.ToString() ?? "";
            }

            builder.AddAttribute(1, "data-type", GetValue());
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperInvokedTypeParameterLambda_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            Func<string> getValue = () =>
            {
                var value = default(TTitle);
                return value?.ToString() ?? "";
            };

            builder.AddAttribute(1, "data-type", getValue());
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGenericHelperInvokedInlineTypeParameterLambda_ThrowsCanonicalizationFailed()
    {
        AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
            """
            builder.AddAttribute(1, "data-type", ((Func<string>)(() => typeof(TTitle).Name))());
            """);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperAttributeMutation_LowersConditionalReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "((showPrimary) => {");
        StringAssert.Contains(artifact.ModuleCode, "((title) => {");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "})(nextShowPrimary());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var conditionalReplayIndex = artifact.ModuleCode.IndexOf("if (showPrimary) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, conditionalReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", conditionalReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperAttributeMutation_PreservesBothBranchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                    else
                    {
                        builder.AddAttribute(2, "hidden", true);
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);

        var classReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        var classAttribute = classReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(classAttribute);
        Assert.AreEqual("class", classAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(classAttribute.Value);

        var hiddenReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperAttributeMutation_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                    else
                    {
                        builder.AddAttribute(2, "hidden", true);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperChildEmission_PreservesConditionalChildReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsTrue(section.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var childReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(childReplay);
        var span = childReplay.Child as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperChildEmission_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"span\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "})(nextShowPrimary());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var childReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, childReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", childReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperChildEmission_PreservesBothBranchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.OpenElement(3, "em");
                        builder.AddContent(4, title);
                        builder.CloseElement();
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var trueChildReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(trueChildReplay);
        Assert.AreEqual("span", ((RazorVueElementNode)trueChildReplay.Child).TagName);

        var falseChildReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(falseChildReplay);
        Assert.AreEqual("em", ((RazorVueElementNode)falseChildReplay.Child).TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperChildEmission_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(5, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.OpenElement(3, "em");
                        builder.AddContent(4, title);
                        builder.CloseElement();
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"span\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"em\", null, title));");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperAttributeAndChildEmission_PreservesBranchReplayOrder()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddAttribute(4, "hidden", true);
                        builder.OpenElement(5, "em");
                        builder.AddContent(6, title);
                        builder.CloseElement();
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.AreEqual(2, conditionalReplay.WhenTrue.Length);
        Assert.AreEqual(2, conditionalReplay.WhenFalse.Length);

        var trueAttributeReplay = conditionalReplay.WhenTrue[0] as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(trueAttributeReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)trueAttributeReplay.Attribute).Name);
        var trueChildReplay = conditionalReplay.WhenTrue[1] as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(trueChildReplay);
        Assert.AreEqual("span", ((RazorVueElementNode)trueChildReplay.Child).TagName);

        var falseAttributeReplay = conditionalReplay.WhenFalse[0] as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(falseAttributeReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)falseAttributeReplay.Attribute).Name);
        var falseChildReplay = conditionalReplay.WhenFalse[1] as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(falseChildReplay);
        Assert.AreEqual("em", ((RazorVueElementNode)falseChildReplay.Child).TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperAttributeAndChildEmission_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(7, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddAttribute(4, "hidden", true);
                        builder.OpenElement(5, "em");
                        builder.AddContent(6, title);
                        builder.CloseElement();
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"span\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"em\", null, title));");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperEventModifier_PreservesConditionalModifierReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, ShowPrimary, PreventClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool preventClick)
                {
                    if (showPrimary)
                    {
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                    }
                }
                """,
                includeEventCallback: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNull(attribute.EventModifiers.PreventDefault);

        var scopedReplay = button.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.AreEqual(2, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("showPrimary", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.AreEqual("preventClick", scopedReplay.CapturedBindings[1].ParameterSymbol.Name);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);

        var modifierReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeEventModifierReplayOperation;
        Assert.IsNotNull(modifierReplay);
        Assert.AreEqual("onclick", modifierReplay.EventHandlerName);
        Assert.IsNotNull(modifierReplay.EventModifiers.PreventDefault);
        Assert.IsTrue(modifierReplay.EventModifiers.PreventDefault.CapturedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperEventModifier_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, NextShowPrimary(), NextPrevent());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool preventClick)
                {
                    if (showPrimary)
                    {
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private bool NextPrevent()
                {
                    return PreventClick;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeEventCallback: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"button\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"onclick\", () => emit(\"click\"));");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextPrevent());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var conditionalIndex = artifact.ModuleCode.IndexOf("if (showPrimary) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, conditionalIndex);
        var modifierIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });", conditionalIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, modifierIndex);
        var helperPreventInvocationIndex = artifact.ModuleCode.IndexOf("})(nextPrevent());", modifierIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, helperPreventInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", helperPreventInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperEventModifier_PreservesBothBranchModifierReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, ShowPrimary, PreventClick, StopClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool preventClick, bool stopClick)
                {
                    if (showPrimary)
                    {
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                    }
                    else
                    {
                        WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(builder, 3, "onclick", stopClick);
                    }
                }
                """,
                includeEventCallback: true,
                includeStopClick: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var scopedReplay = button.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var preventReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeEventModifierReplayOperation;
        Assert.IsNotNull(preventReplay);
        Assert.IsNotNull(preventReplay.EventModifiers.PreventDefault);
        Assert.IsNull(preventReplay.EventModifiers.StopPropagation);

        var stopReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeEventModifierReplayOperation;
        Assert.IsNotNull(stopReplay);
        Assert.IsNull(stopReplay.EventModifiers.PreventDefault);
        Assert.IsNotNull(stopReplay.EventModifiers.StopPropagation);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperEventModifier_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, NextShowPrimary(), NextPrevent(), NextStop());
                builder.AddAttribute(4, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool preventClick, bool stopClick)
                {
                    if (showPrimary)
                    {
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                    }
                    else
                    {
                        WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(builder, 3, "onclick", stopClick);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private bool NextPrevent()
                {
                    return PreventClick;
                }

                private bool NextStop()
                {
                    return StopClick;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeEventCallback: true,
                includeStopClick: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: false, stopPropagation: stopClick });");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextPrevent());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextStop());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperSetKey_PreservesConditionalKeyReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.SetKey(title);
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsNull(section.Key);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var keyReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeKeyReplayOperation;
        Assert.IsNotNull(keyReplay);
        Assert.IsNotNull(keyReplay.Key);
        Assert.IsTrue(keyReplay.KeyAssigned);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(keyReplay.Key.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperSetKey_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.SetKey(title);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setKey(title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var keyIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setKey(title);", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, keyIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", keyIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperSetKey_PreservesBothBranchKeyReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.SetKey(title);
                    }
                    else
                    {
                        builder.SetKey("fallback");
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var trueKeyReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeKeyReplayOperation;
        Assert.IsNotNull(trueKeyReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(trueKeyReplay.Key?.Expression);

        var falseKeyReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeKeyReplayOperation;
        Assert.IsNotNull(falseKeyReplay);
        Assert.IsNotNull(falseKeyReplay.Key);
        Assert.AreEqual("fallback", falseKeyReplay.Key.Expression.ConstantValue.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperSetKey_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.SetKey(title);
                    }
                    else
                    {
                        builder.SetKey("fallback");
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setKey(title);");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setKey(\"fallback\");");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperAddMultipleAttributes_PreservesConditionalSpreadReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, AdditionalAttributes);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, IReadOnlyDictionary<string, object?>? attributes)
                {
                    if (showPrimary)
                    {
                        builder.AddMultipleAttributes(1, attributes);
                    }
                }
                """,
                includeRenderFlags: true,
                includeAdditionalAttributes: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsTrue(section.Attributes.IsDefaultOrEmpty);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var spreadReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(spreadReplay);
        var spread = spreadReplay.Attribute as RazorVueAttributeSpreadNode;
        Assert.IsNotNull(spread);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(spread.Expression);
        Assert.IsTrue(spread.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperAddMultipleAttributes_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextAttributes());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, IReadOnlyDictionary<string, object?>? attributes)
                {
                    if (showPrimary)
                    {
                        builder.AddMultipleAttributes(1, attributes);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private IReadOnlyDictionary<string, object?>? NextAttributes()
                {
                    return AdditionalAttributes;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true,
                includeAdditionalAttributes: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.mergeAttributes(attributes);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextAttributes());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var spreadIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.mergeAttributes(attributes);", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, spreadIndex);
        var scopedAttributesInvocationIndex = artifact.ModuleCode.IndexOf("})(nextAttributes());", spreadIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedAttributesInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedAttributesInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperAddMultipleAttributes_PreservesBothBranchSpreadReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, AdditionalAttributes);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, IReadOnlyDictionary<string, object?>? attributes)
                {
                    if (showPrimary)
                    {
                        builder.AddMultipleAttributes(1, attributes);
                    }
                    else
                    {
                        builder.AddMultipleAttributes(2, attributes);
                    }
                }
                """,
                includeRenderFlags: true,
                includeAdditionalAttributes: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var trueSpreadReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        var trueSpread = trueSpreadReplay?.Attribute as RazorVueAttributeSpreadNode;
        Assert.IsNotNull(trueSpread);
        var falseSpreadReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        var falseSpread = falseSpreadReplay?.Attribute as RazorVueAttributeSpreadNode;
        Assert.IsNotNull(falseSpread);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperAddMultipleAttributes_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextAttributes());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, IReadOnlyDictionary<string, object?>? attributes)
                {
                    if (showPrimary)
                    {
                        builder.AddMultipleAttributes(1, attributes);
                    }
                    else
                    {
                        builder.AddMultipleAttributes(2, attributes);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private IReadOnlyDictionary<string, object?>? NextAttributes()
                {
                    return AdditionalAttributes;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true,
                includeAdditionalAttributes: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        Assert.AreEqual(
            2,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.mergeAttributes(attributes);"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextAttributes());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperAmbientDefaultSlotChild_PreservesConditionalAmbientChildReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
                """,
                includePanel: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.IsTrue(component.AmbientDefaultSlotChildren.Children.IsDefaultOrEmpty);

        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var slotReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation;
        Assert.IsNotNull(slotReplay);
        var span = slotReplay.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperAmbientDefaultSlotChild_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includePanel: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(PanelComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var componentIndex = artifact.ModuleCode.IndexOf("enterComponent(PanelComponent", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, componentIndex);
        var slotReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));", componentIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, slotReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", slotReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var closeComponentIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.leaveComponent();", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, closeComponentIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperRegionAmbientDefaultSlotChild_PreservesConditionalAmbientChildReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenRegion(1);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                        builder.CloseRegion();
                    }
                }
                """,
                includePanel: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.IsTrue(component.AmbientDefaultSlotChildren.Children.IsDefaultOrEmpty);

        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var slotReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation;
        Assert.IsNotNull(slotReplay);
        var span = slotReplay.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperRegionAmbientDefaultSlotChild_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.OpenRegion(1);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                        builder.CloseRegion();
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includePanel: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(PanelComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("__jazorRenderContext.openRegion();", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("__jazorRenderContext.closeRegion();", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var componentIndex = artifact.ModuleCode.IndexOf("enterComponent(PanelComponent", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, componentIndex);
        var slotReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));", componentIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, slotReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", slotReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var closeComponentIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.leaveComponent();", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, closeComponentIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperImplicitDefaultSlotAssignment_PreservesConditionalDefaultSlotReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(2, "span");
                            childBuilder.AddContent(3, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }
                """,
                includePanel: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.IsTrue(component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty);
        Assert.IsTrue(component.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var slotReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation;
        Assert.IsNotNull(slotReplay);
        var span = slotReplay.Assignment.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperImplicitDefaultSlotAssignment_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(2, "span");
                            childBuilder.AddContent(3, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includePanel: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(PanelComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var componentIndex = artifact.ModuleCode.IndexOf("enterComponent(PanelComponent", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, componentIndex);
        var slotReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));", componentIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, slotReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", slotReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var closeComponentIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.leaveComponent();", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, closeComponentIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperImplicitDefaultSlotAssignment_PreservesBothBranchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(2, "span");
                            childBuilder.AddContent(3, title);
                            childBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(4, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(5, "em");
                            childBuilder.AddContent(6, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }
                """,
                includePanel: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.IsTrue(component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty);
        Assert.IsTrue(component.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var trueSlotReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation;
        Assert.IsNotNull(trueSlotReplay);
        Assert.AreEqual("span", ((RazorVueElementNode)trueSlotReplay.Assignment.Children.Children.Single()).TagName);

        var falseSlotReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation;
        Assert.IsNotNull(falseSlotReplay);
        Assert.AreEqual("em", ((RazorVueElementNode)falseSlotReplay.Assignment.Children.Children.Single()).TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperImplicitDefaultSlotAssignment_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(2, "span");
                            childBuilder.AddContent(3, title);
                            childBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(4, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(5, "em");
                            childBuilder.AddContent(6, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includePanel: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(PanelComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"em\", null, title));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"em\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperComponentAttributeAndDefaultSlot_PreservesBranchReplayOrder()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Title", title);
                        builder.AddAttribute(2, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(3, "span");
                            childBuilder.AddContent(4, title);
                            childBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(5, "Title", "fallback");
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(7, "em");
                            childBuilder.AddContent(8, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }
                """,
                includePanel: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.AreEqual(2, conditionalReplay.WhenTrue.Length);
        Assert.AreEqual(2, conditionalReplay.WhenFalse.Length);

        var trueTitleReplay = conditionalReplay.WhenTrue[0] as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(trueTitleReplay);
        Assert.AreEqual("Title", ((RazorVueAttributeNode)trueTitleReplay.Attribute).Name);
        var trueSlotReplay = conditionalReplay.WhenTrue[1] as RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation;
        Assert.IsNotNull(trueSlotReplay);
        Assert.AreEqual("span", ((RazorVueElementNode)trueSlotReplay.Assignment.Children.Children.Single()).TagName);

        var falseTitleReplay = conditionalReplay.WhenFalse[0] as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(falseTitleReplay);
        Assert.AreEqual("Title", ((RazorVueAttributeNode)falseTitleReplay.Attribute).Name);
        var falseSlotReplay = conditionalReplay.WhenFalse[1] as RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation;
        Assert.IsNotNull(falseSlotReplay);
        Assert.AreEqual("em", ((RazorVueElementNode)falseSlotReplay.Assignment.Children.Children.Single()).TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperComponentAttributeAndDefaultSlot_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Title", title);
                        builder.AddAttribute(2, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(3, "span");
                            childBuilder.AddContent(4, title);
                            childBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(5, "Title", "fallback");
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(7, "em");
                            childBuilder.AddContent(8, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includePanel: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"Title\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"span\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"Title\", \"fallback\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () => h(\"em\", null, title));");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConditionalHelperNamedAndTypedSlotAssignments_PreservesConditionalSlotReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<ListCard>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, title);
                            headerBuilder.CloseElement();
                        }));
                        builder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, title);
                            itemBuilder.AddContent(7, " ");
                            itemBuilder.AddContent(8, item);
                            itemBuilder.CloseElement();
                        }));
                    }
                }
                """,
                includeListCard: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ListCard", component.ComponentName);
        Assert.IsTrue(component.SlotTemplates.IsDefaultOrEmpty);
        Assert.IsTrue(component.Attributes.IsDefaultOrEmpty);
        Assert.IsTrue(component.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);
        Assert.AreEqual(2, conditionalReplay.WhenTrue.Length);

        var headerReplay = conditionalReplay.WhenTrue[0] as RazorVueOpenNodeSlotTemplateReplayOperation;
        Assert.IsNotNull(headerReplay);
        Assert.AreEqual("Header", headerReplay.SlotTemplate.PublicName);
        Assert.IsNull(headerReplay.SlotTemplate.ParameterName);
        var header = headerReplay.SlotTemplate.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(header);
        Assert.AreEqual("h1", header.TagName);
        var headerExpression = header.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(headerExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(headerExpression.Expression);

        var itemReplay = conditionalReplay.WhenTrue[1] as RazorVueOpenNodeSlotTemplateReplayOperation;
        Assert.IsNotNull(itemReplay);
        Assert.AreEqual("ItemTemplate", itemReplay.SlotTemplate.PublicName);
        Assert.AreEqual("item", itemReplay.SlotTemplate.ParameterName);
        var paragraph = itemReplay.SlotTemplate.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual(3, paragraph.Children.Children.Length);
        var titleExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(titleExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(titleExpression.Expression);
        var itemExpression = paragraph.Children.Children[2] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConditionalHelperNamedAndTypedSlotAssignments_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<ListCard>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, title);
                            headerBuilder.CloseElement();
                        }));
                        builder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, title);
                            itemBuilder.AddContent(7, " ");
                            itemBuilder.AddContent(8, item);
                            itemBuilder.CloseElement();
                        }));
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeListCard: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(ListCardComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"Header\", () => h(\"h1\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", (item) => h(\"p\", null, [title, \" \", item]));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"h1\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"p\", null, [title, \" \", item]));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var componentIndex = artifact.ModuleCode.IndexOf("enterComponent(ListCardComponent", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, componentIndex);
        var headerReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setComponentParameter(\"Header\", () => h(\"h1\", null, title));", componentIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, headerReplayIndex);
        var itemReplayIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setComponentParameter(\"ItemTemplate\", (item) => h(\"p\", null, [title, \" \", item]));", headerReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, itemReplayIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", itemReplayIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var closeComponentIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.leaveComponent();", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, closeComponentIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedIfElseHelperNamedAndTypedSlotAssignments_PreservesBothBranchSlotReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenComponent<ListCard>(0);
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, title);
                            headerBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, title);
                            itemBuilder.AddContent(7, " ");
                            itemBuilder.AddContent(8, item);
                            itemBuilder.CloseElement();
                        }));
                    }
                }
                """,
                includeListCard: true,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var scopedReplay = component.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);

        var headerReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeSlotTemplateReplayOperation;
        Assert.IsNotNull(headerReplay);
        Assert.AreEqual("Header", headerReplay.SlotTemplate.PublicName);

        var itemReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeSlotTemplateReplayOperation;
        Assert.IsNotNull(itemReplay);
        Assert.AreEqual("ItemTemplate", itemReplay.SlotTemplate.PublicName);
        Assert.AreEqual("item", itemReplay.SlotTemplate.ParameterName);
        var paragraph = itemReplay.SlotTemplate.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        var itemExpression = paragraph.Children.Children[2] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedIfElseHelperNamedAndTypedSlotAssignments_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenComponent<ListCard>(0);
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, title);
                            headerBuilder.CloseElement();
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, title);
                            itemBuilder.AddContent(7, " ");
                            itemBuilder.AddContent(8, item);
                            itemBuilder.CloseElement();
                        }));
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeListCard: true,
                includeRenderFlags: true)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "enterComponent(ListCardComponent");
        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"Header\", () => h(\"h1\", null, title));");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", (item) => h(\"p\", null, [title, \" \", item]));");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"h1\", null, title));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.append(h(\"p\", null, [title, \" \", item]));", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedGuardReturnHelperAttributeMutation_PreservesConditionalReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                    }

                    builder.AddAttribute(1, "class", title);
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsTrue(section.Attributes.IsDefaultOrEmpty);
        Assert.IsTrue(section.Children.Children.IsDefaultOrEmpty);

        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        Assert.AreEqual(2, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("showPrimary", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[1].ParameterSymbol.Name);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IUnaryOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenTrue.IsDefaultOrEmpty);

        var attributeReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        var attribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.IsTrue(attribute.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedGuardReturnHelperAttributeMutation_LowersConditionalReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                    }

                    builder.AddAttribute(1, "class", title);
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "((showPrimary) => {");
        StringAssert.Contains(artifact.ModuleCode, "((title) => {");
        StringAssert.Contains(artifact.ModuleCode, "if (!showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "})(nextShowPrimary());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNoExtraParameterGuardReturnHelperAttributeMutation_LowersConditionalReplay()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    if (!ShowPrimary)
                    {
                        return;
                    }

                    builder.AddAttribute(1, "class", Title);
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "if (!props.showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", props.title);");
        Assert.AreEqual(
            0,
            CountOccurrences(artifact.ModuleCode, "((showPrimary) =>"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedTerminalReturnBranchMutation_PreservesBranchAndTailReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        builder.AddAttribute(1, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(2, "class", title);
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IUnaryOperation>(conditionalReplay.Condition);

        var hiddenReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        var hiddenAttribute = hiddenReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(hiddenAttribute);
        Assert.AreEqual("hidden", hiddenAttribute.Name);

        var classReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        var classAttribute = classReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(classAttribute);
        Assert.AreEqual("class", classAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(classAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedTerminalReturnBranchMutation_LowersBranchAndTailReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        builder.AddAttribute(1, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(2, "class", title);
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (!showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedBothBranchesTerminalReturnMutation_PreservesBranchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                        return;
                    }
                    else
                    {
                        builder.AddAttribute(2, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(3, "title", title);
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);

        var classReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        var classAttribute = classReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(classAttribute);
        Assert.AreEqual("class", classAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(classAttribute.Value);

        var hiddenReplay = conditionalReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedBothBranchesTerminalReturnMutation_LowersBranchesAndSkipsUnreachableTail()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                        return;
                    }
                    else
                    {
                        builder.AddAttribute(2, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(3, "title", title);
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("__jazorRenderContext.setAttribute(\"title\", title);", StringComparison.Ordinal),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var conditionalIndex = artifact.ModuleCode.IndexOf("if (showPrimary) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, conditionalIndex);
        var classIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"class\", title);", conditionalIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, classIndex);
        var hiddenIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"hidden\", true);", classIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, hiddenIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedConsecutiveGuardReturnHelperAttributeMutation_PreservesNestedConditionalReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                    }

                    if (title == null)
                    {
                        builder.AddAttribute(1, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(2, "class", title);
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var outerReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(outerReplay);
        Assert.IsInstanceOfType<IUnaryOperation>(outerReplay.Condition);
        Assert.IsTrue(outerReplay.WhenTrue.IsDefaultOrEmpty);

        var innerReplay = outerReplay.WhenFalse.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(innerReplay);
        Assert.IsInstanceOfType<IBinaryOperation>(innerReplay.Condition);

        var hiddenReplay = innerReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);

        var classReplay = innerReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        var classAttribute = classReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(classAttribute);
        Assert.AreEqual("class", classAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(classAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedConsecutiveGuardReturnHelperAttributeMutation_LowersNestedConditionalReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                    }

                    if (title == null)
                    {
                        builder.AddAttribute(1, "hidden", true);
                        return;
                    }

                    builder.AddAttribute(2, "class", title);
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (!showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "} else {");
        StringAssert.Contains(artifact.ModuleCode, "if ((title === null)) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var outerConditionalIndex = artifact.ModuleCode.IndexOf("if (!showPrimary) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, outerConditionalIndex);
        var innerConditionalIndex = artifact.ModuleCode.IndexOf("if ((title === null)) {", outerConditionalIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, innerConditionalIndex);
        var hiddenIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"hidden\", true);", innerConditionalIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, hiddenIndex);
        var classIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"class\", title);", hiddenIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, classIndex);
        var laterTitleIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", classIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterTitleIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNestedIfElseHelperAttributeMutation_PreservesNestedConditionalReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, ShowSecondary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool showSecondary, string? title)
                {
                    if (showPrimary)
                    {
                        if (showSecondary)
                        {
                            builder.AddAttribute(1, "data-mode", "secondary");
                        }
                        else
                        {
                            builder.AddAttribute(2, "class", title);
                        }
                    }
                    else
                    {
                        builder.AddAttribute(3, "hidden", true);
                    }
                }
                """,
                includeRenderFlags: true,
                includeSecondaryFlag: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var outerReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(outerReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(outerReplay.Condition);

        var innerReplay = outerReplay.WhenTrue.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(innerReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(innerReplay.Condition);
        var secondaryReplay = innerReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(secondaryReplay);
        Assert.AreEqual("data-mode", ((RazorVueAttributeNode)secondaryReplay.Attribute).Name);
        var classReplay = innerReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)classReplay.Attribute).Name);

        var hiddenReplay = outerReplay.WhenFalse.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNestedIfElseHelperAttributeMutation_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextShowSecondary(), NextTitle());
                builder.AddAttribute(4, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, bool showSecondary, string? title)
                {
                    if (showPrimary)
                    {
                        if (showSecondary)
                        {
                            builder.AddAttribute(1, "data-mode", "secondary");
                        }
                        else
                        {
                            builder.AddAttribute(2, "class", title);
                        }
                    }
                    else
                    {
                        builder.AddAttribute(3, "hidden", true);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private bool NextShowSecondary()
                {
                    return ShowSecondary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true,
                includeSecondaryFlag: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "if (showSecondary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-mode\", \"secondary\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowSecondary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNonTerminalReturnBranchMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (!showPrimary)
                    {
                        return;
                        builder.AddAttribute(1, "class", title);
                    }

                    builder.AddAttribute(2, "title", title);
                }
                """,
                includeRenderFlags: true));

        AssertRenderHelperBoundary(exception, "return");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNamedArgumentsInParameterOrderHelperAttributeMutation_PreservesReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder: builder, showPrimary: ShowPrimary, title: Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        Assert.AreEqual(2, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("showPrimary", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[1].ParameterSymbol.Name);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        var attributeReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(attributeReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)attributeReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNamedArgumentsInParameterOrderHelperAttributeMutation_EvaluatesArgumentsOnce()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder: builder, showPrimary: NextShowPrimary(), title: NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNamedArgumentReorderedHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(title: Title, builder: builder, showPrimary: ShowPrimary);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
                """,
                includeRenderFlags: true));

        AssertRenderHelperBoundary(exception, "argument", "source order");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedOmittedOptionalHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title, string cssClass = "primary")
                {
                    builder.AddAttribute(1, "class", cssClass);
                    builder.AddAttribute(2, "title", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "optional", "argument");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedUnusedOmittedOptionalHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title, string cssClass = "primary")
                {
                    builder.AddAttribute(1, "title", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "optional", "argument");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedAsyncVoidHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private async void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                    builder.AddAttribute(1, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "async", "synchronous");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedAsyncTaskHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private async System.Threading.Tasks.Task RenderBody(RenderTreeBuilder builder, string? title)
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                    builder.AddAttribute(1, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "async", "synchronous");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedAsyncValueTaskHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private async System.Threading.Tasks.ValueTask RenderBody(RenderTreeBuilder builder, string? title)
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                    builder.AddAttribute(1, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "async", "synchronous");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedAsyncTaskLocalFunctionHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();

                async System.Threading.Tasks.Task RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                    localBuilder.AddAttribute(1, "class", title);
                }
                """,
                ""));

        AssertRenderHelperBoundary(exception, "async", "synchronous");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedAsyncValueTaskLocalFunctionHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();

                async System.Threading.Tasks.ValueTask RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                    localBuilder.AddAttribute(1, "class", title);
                }
                """,
                ""));

        AssertRenderHelperBoundary(exception, "async", "synchronous");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedSwitchHelperAttributeMutation_PreservesSwitchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    switch (title)
                    {
                        case "primary":
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var switchReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeSwitchReplayOperation;
        Assert.IsNotNull(switchReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(switchReplay.Value);
        Assert.AreEqual(2, switchReplay.Sections.Length);

        var primarySection = switchReplay.Sections[0];
        Assert.AreEqual(1, primarySection.Labels.Length);
        Assert.IsFalse(primarySection.Labels[0].IsDefault);
        Assert.IsNotNull(primarySection.Labels[0].Value);
        var classReplay = primarySection.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)classReplay.Attribute).Name);

        var defaultSection = switchReplay.Sections[1];
        Assert.AreEqual(1, defaultSection.Labels.Length);
        Assert.IsTrue(defaultSection.Labels[0].IsDefault);
        Assert.IsNull(defaultSection.Labels[0].Value);
        var hiddenReplay = defaultSection.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedSwitchHelperAttributeMutation_EvaluatesValueAndArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    switch (title)
                    {
                        case "primary":
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "((title) => {");
        StringAssert.Contains(artifact.ModuleCode, "switch (title) {");
        StringAssert.Contains(artifact.ModuleCode, "case \"primary\":");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "default:");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var switchIndex = artifact.ModuleCode.IndexOf("switch (title) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, switchIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", switchIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedSwitchHelperMultipleValueLabels_PreservesSharedSectionReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    switch (title)
                    {
                        case "primary":
                        case "accent":
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var switchReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeSwitchReplayOperation;
        Assert.IsNotNull(switchReplay);
        Assert.AreEqual(2, switchReplay.Sections.Length);

        var sharedSection = switchReplay.Sections[0];
        Assert.AreEqual(2, sharedSection.Labels.Length);
        Assert.IsFalse(sharedSection.Labels[0].IsDefault);
        Assert.IsFalse(sharedSection.Labels[1].IsDefault);
        Assert.IsFalse(sharedSection.Labels[0].IsCondition);
        Assert.IsFalse(sharedSection.Labels[1].IsCondition);
        var classReplay = sharedSection.Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)classReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedSwitchHelperMultipleValueLabels_EmitsStackedCases()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    switch (title)
                    {
                        case "primary":
                        case "accent":
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "switch (title) {");
        StringAssert.Contains(artifact.ModuleCode, "case \"primary\":");
        StringAssert.Contains(artifact.ModuleCode, "case \"accent\":");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchHelperAttributeMutation_PreservesConditionReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title, ShowPrimary);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title, bool showPrimary)
                {
                    switch (title)
                    {
                        case { Length: > 3 } when showPrimary:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var switchReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeSwitchReplayOperation;
        Assert.IsNotNull(switchReplay);
        Assert.AreEqual(2, switchReplay.Sections.Length);
        Assert.AreEqual(1, switchReplay.Sections[0].Labels.Length);
        Assert.IsTrue(switchReplay.Sections[0].Labels[0].IsCondition);
        Assert.IsInstanceOfType<IPatternCaseClauseOperation>(switchReplay.Sections[0].Labels[0].Value);
        var classReplay = switchReplay.Sections[0].Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)classReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedPatternSwitchHelperAttributeMutation_EvaluatesValueAndArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle(), NextShowPrimary());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title, bool showPrimary)
                {
                    switch (title)
                    {
                        case { Length: > 3 } when showPrimary:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const __jazorImperativeSwitchValue");
        StringAssert.Contains(artifact.ModuleCode, "if (");
        StringAssert.Contains(artifact.ModuleCode, "showPrimary");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "else {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var switchValueIndex = artifact.ModuleCode.IndexOf("const __jazorImperativeSwitchValue", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, switchValueIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", switchValueIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedPatternLocalSwitchHelperConditionOnly_LowersConditionReplay()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    switch (title)
                    {
                        case string value when value.Length > 3:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const __jazorImperativeSwitchValue");
        StringAssert.Contains(artifact.ModuleCode, "let value;");
        StringAssert.Contains(artifact.ModuleCode, "value.length > 3");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var switchValueIndex = artifact.ModuleCode.IndexOf("const __jazorImperativeSwitchValue", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, switchValueIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", switchValueIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchHelperLocalGuard_PreservesLocalPreludeReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    var minLength = 3;
                    switch (title)
                    {
                        case string value when value.Length > minLength:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        Assert.AreEqual(2, scopedReplay.Operations.Length);
        var localReplay = scopedReplay.Operations[0] as RazorVueOpenNodeLocalDeclarationReplayOperation;
        Assert.IsNotNull(localReplay);
        Assert.AreEqual("minLength", localReplay.LocalSymbol.Name);
        var switchReplay = scopedReplay.Operations[1] as RazorVueOpenNodeSwitchReplayOperation;
        Assert.IsNotNull(switchReplay);
        Assert.IsTrue(switchReplay.Sections[0].Labels[0].IsCondition);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedPatternSwitchHelperLocalGuard_LowersLocalPreludeBeforeCondition()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    var minLength = 3;
                    switch (title)
                    {
                        case string value when value.Length > minLength:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const minLength = 3;");
        StringAssert.Contains(artifact.ModuleCode, "const __jazorImperativeSwitchValue");
        StringAssert.Contains(artifact.ModuleCode, "value.length > minLength");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var localIndex = artifact.ModuleCode.IndexOf("const minLength = 3;", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, localIndex);
        var switchValueIndex = artifact.ModuleCode.IndexOf("const __jazorImperativeSwitchValue", localIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, switchValueIndex);
        var conditionIndex = artifact.ModuleCode.IndexOf("value.length > minLength", switchValueIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, conditionIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", conditionIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchHelperMutatedLocalGuard_ThrowsCanonicalizationFailed()
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
                    var minLength = 3;
                    minLength++;
                    switch (title)
                    {
                        case string value when value.Length > minLength:
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "local", "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchLocalFunctionHelperOuterLocalGuard_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                var minLength = 3;

                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    switch (title)
                    {
                        case string value when value.Length > minLength:
                            localBuilder.AddAttribute(1, "class", title);
                            break;
                        default:
                            localBuilder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "switch control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchHelperSideEffectGuard_ThrowsCanonicalizationFailed()
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
                    switch (title)
                    {
                        case string value when value.Length > 3 && NextShowPrimary():
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }
                """,
                includeRenderFlags: true));

        AssertRenderHelperBoundary(exception, "caller-owned", "switch control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedPatternSwitchHelperDeclaredPatternLocal_ThrowsCanonicalizationFailed()
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
                    switch (title)
                    {
                        case string value when value.Length > 3:
                            builder.AddAttribute(1, "class", value);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "switch control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedSwitchGotoCaseHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    switch (title)
                    {
                        case "primary":
                            goto case "accent";
                        case "accent":
                            builder.AddAttribute(1, "class", title);
                            break;
                        default:
                            builder.AddAttribute(2, "hidden", true);
                            break;
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "switch control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedSwitchGotoDefaultHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    switch (title)
                    {
                        case "primary":
                            goto default;
                        default:
                            builder.AddAttribute(1, "hidden", true);
                            break;
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "switch control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedForEachHelperAttributeMutation_PreservesImperativeLoopReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    foreach (var suffix in new[] { "a", "b" })
                    {
                        builder.AddAttribute(1, "data-suffix", suffix);
                        builder.AddAttribute(2, "title", title);
                    }
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperativeReplay.Kind);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedForHelperAttributeMutation_LowersImperativeLoopReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    for (var index = 0; index < 2; index++)
                    {
                        builder.AddAttribute(1, "data-index", index);
                        builder.AddAttribute(2, "data-title", title);
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < 2; index++)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-index\", index);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-title\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var loopIndex = artifact.ModuleCode.IndexOf("for (let index = 0; index < 2; index++)", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, loopIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", loopIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedWhileHelperAttributeMutation_LowersImperativeLoopReplay()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    var index = 0;
                    while (index < 2)
                    {
                        builder.AddAttribute(1, "data-index", index);
                        builder.AddAttribute(2, "data-title", title);
                        index++;
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        StringAssert.Contains(artifact.ModuleCode, "while (index < 2)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-index\", index);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-title\", title);");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedForEachHelperChildEmission_ThrowsCanonicalizationFailed()
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
                    foreach (var suffix in new[] { "a", "b" })
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.AddContent(3, suffix);
                        builder.CloseElement();
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "loop control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionForEachHelperChildEmission_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    foreach (var suffix in new[] { "a", "b" })
                    {
                        localBuilder.OpenElement(1, "span");
                        localBuilder.AddContent(2, title);
                        localBuilder.AddContent(3, suffix);
                        localBuilder.CloseElement();
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "loop control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNestedBlockForEachHelperChildEmission_ThrowsCanonicalizationFailed()
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
                    {
                        foreach (var suffix in new[] { "a", "b" })
                        {
                            builder.OpenElement(1, "span");
                            builder.AddContent(2, title);
                            builder.AddContent(3, suffix);
                            builder.CloseElement();
                        }
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "loop control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNestedConditionalSwitchHelperAttributeMutation_PreservesNestedSwitchReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, ShowPrimary, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        switch (title)
                        {
                            case "primary":
                                builder.AddAttribute(1, "class", title);
                                break;
                            default:
                                builder.AddAttribute(2, "hidden", true);
                                break;
                        }
                    }
                }
                """,
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);

        var conditionalReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeConditionalReplayOperation;
        Assert.IsNotNull(conditionalReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(conditionalReplay.Condition);
        Assert.IsTrue(conditionalReplay.WhenFalse.IsDefaultOrEmpty);

        var switchReplay = conditionalReplay.WhenTrue.Single() as RazorVueOpenNodeSwitchReplayOperation;
        Assert.IsNotNull(switchReplay);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(switchReplay.Value);
        Assert.AreEqual(2, switchReplay.Sections.Length);
        var classReplay = switchReplay.Sections[0].Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(classReplay);
        Assert.AreEqual("class", ((RazorVueAttributeNode)classReplay.Attribute).Name);
        var hiddenReplay = switchReplay.Sections[1].Operations.Single() as RazorVueOpenNodeAttributeReplayOperation;
        Assert.IsNotNull(hiddenReplay);
        Assert.AreEqual("hidden", ((RazorVueAttributeNode)hiddenReplay.Attribute).Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNestedConditionalSwitchHelperAttributeMutation_EvaluatesArgumentsOnceInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    if (showPrimary)
                    {
                        switch (title)
                        {
                            case "primary":
                                builder.AddAttribute(1, "class", title);
                                break;
                            default:
                                builder.AddAttribute(2, "hidden", true);
                                break;
                        }
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (showPrimary) {");
        StringAssert.Contains(artifact.ModuleCode, "switch (title) {");
        StringAssert.Contains(artifact.ModuleCode, "case \"primary\":");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "default:");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"hidden\", true);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var conditionalIndex = artifact.ModuleCode.IndexOf("if (showPrimary) {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, conditionalIndex);
        var switchIndex = artifact.ModuleCode.IndexOf("switch (title) {", conditionalIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, switchIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", switchIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedTryFinallyHelperAttributeMutation_PreservesImperativeTryReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    try
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                    finally
                    {
                        builder.AddAttribute(2, "title", title);
                    }
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperativeReplay.Kind);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedTryFinallyHelperAttributeMutation_LowersImperativeTryReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(3, "data-tail", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    try
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                    finally
                    {
                        builder.AddAttribute(2, "title", title);
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var tryIndex = artifact.ModuleCode.IndexOf("try {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, tryIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", tryIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedTryCatchHelperAttributeMutation_PreservesImperativeTryReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    try
                    {
                        throw new InvalidOperationException(title);
                    }
                    catch (Exception ex)
                    {
                        builder.AddAttribute(1, "data-error", ex);
                    }
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperativeReplay.Kind);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedTryCatchFilterHelperAttributeMutation_LowersImperativeTryReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextShowPrimary(), NextTitle());
                builder.AddAttribute(2, "data-tail", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool showPrimary, string? title)
                {
                    try
                    {
                        throw new InvalidOperationException(title);
                    }
                    catch (Exception ex) when (showPrimary)
                    {
                        builder.AddAttribute(1, "data-error", ex);
                    }
                }

                private bool NextShowPrimary()
                {
                    return ShowPrimary;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeRenderFlags: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "throw new Error(title);");
        StringAssert.Contains(artifact.ModuleCode, "} catch (ex) {");
        StringAssert.Contains(artifact.ModuleCode, "if (!showPrimary)");
        StringAssert.Contains(artifact.ModuleCode, "throw ex;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-error\", ex);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "})(nextShowPrimary());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextShowPrimary());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var tryIndex = artifact.ModuleCode.IndexOf("try {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, tryIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", tryIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var scopedShowInvocationIndex = artifact.ModuleCode.IndexOf("})(nextShowPrimary());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedShowInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());", scopedShowInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedTryFinallyHelperChildEmission_ThrowsCanonicalizationFailed()
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
                    try
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                    finally
                    {
                        builder.AddAttribute(3, "title", title);
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "try/catch/finally control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionTryFinallyHelperChildEmission_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    try
                    {
                        localBuilder.OpenElement(1, "span");
                        localBuilder.AddContent(2, title);
                        localBuilder.CloseElement();
                    }
                    finally
                    {
                        localBuilder.AddAttribute(3, "title", title);
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "try/catch/finally control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedDefaultUsingHelperAttributeMutation_PreservesImperativeUsingReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    using (default(IDisposable))
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperativeReplay.Kind);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNullUsingHelperAttributeMutation_LowersImperativeUsingReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(2, "data-tail", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    using ((IDisposable?)null)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "let ");
        StringAssert.Contains(artifact.ModuleCode, " = null;");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, " !== null)");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var tryIndex = artifact.ModuleCode.IndexOf("try {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, tryIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", tryIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedDefaultUsingDeclarationHelperAttributeMutation_PreservesImperativeUsingDeclarationReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    using var disposable = default(IDisposable);
                    builder.AddAttribute(1, "class", title);
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperativeReplay.Kind);
        Assert.AreEqual(2, imperativeReplay.Operations.Length);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNullUsingDeclarationHelperAttributeMutation_LowersImperativeUsingDeclarationReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(2, "data-tail", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    using var disposable = (IDisposable?)null;
                    builder.AddAttribute(1, "class", title);
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "let disposable = null;");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "if (disposable !== null)");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);

        var tryIndex = artifact.ModuleCode.IndexOf("try {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, tryIndex);
        var scopedTitleInvocationIndex = artifact.ModuleCode.IndexOf("})(nextTitle());", tryIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopedTitleInvocationIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());", scopedTitleInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedUsingDeclarationHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    using var disposable = new NoopDisposable();
                    builder.AddAttribute(1, "class", title);
                }

                private sealed class NoopDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using declaration control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionUsingDeclarationHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    using var disposable = new NoopDisposable();
                    localBuilder.AddAttribute(1, "class", title);
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private sealed class NoopDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using declaration control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedMixedUsingDeclarationResources_ThrowsCanonicalizationFailed()
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
                    using IDisposable? first = null, second = new NoopDisposable();
                    builder.AddAttribute(1, "class", title);
                }

                private sealed class NoopDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using declaration control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedDefaultUsingDeclarationHelperFrameDepthMutation_ThrowsCanonicalizationFailed()
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
                    using var disposable = default(IDisposable);
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, title);
                    builder.CloseElement();
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using declaration control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionDefaultUsingDeclarationHelperFrameDepthMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    using var disposable = default(IDisposable);
                    localBuilder.OpenElement(1, "span");
                    localBuilder.AddContent(2, title);
                    localBuilder.CloseElement();
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "using declaration control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedUsingHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    using (var disposable = new NoopDisposable())
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }

                private sealed class NoopDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionUsingHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    using (var disposable = new NoopDisposable())
                    {
                        localBuilder.AddAttribute(1, "class", title);
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private sealed class NoopDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedDefaultUsingHelperFrameDepthMutation_ThrowsCanonicalizationFailed()
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
                    using (default(IDisposable))
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "using control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionDefaultUsingHelperFrameDepthMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    using (default(IDisposable))
                    {
                        localBuilder.OpenElement(1, "span");
                        localBuilder.AddContent(2, title);
                        localBuilder.CloseElement();
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "using control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLockHelperAttributeMutation_PreservesImperativeLockReplay()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    lock (this)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
                """));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var scopedReplay = section.ReplayOperations.Single() as RazorVueOpenNodeScopedReplayOperation;
        Assert.IsNotNull(scopedReplay);
        var imperativeReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeImperativeReplayOperation;
        Assert.IsNotNull(imperativeReplay);
        Assert.AreEqual(RazorVueImperativeBlockKind.LockBlock, imperativeReplay.Kind);
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "builder"));
        Assert.IsTrue(imperativeReplay.VisibleParameters.Any(static parameter => parameter.Name == "title"));
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedLockHelperAttributeMutation_LowersImperativeLockReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder, NextTitle());
                builder.AddAttribute(2, "data-tail", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, string? title)
                {
                    lock (this)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "if (__jazorComponent == null)");
        StringAssert.Contains(artifact.ModuleCode, "throw new TypeError(\"obj\");");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "})(nextTitle());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"data-tail\", nextTitle());");

        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextTitle());"),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLockHelperChildEmission_ThrowsCanonicalizationFailed()
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
                    lock (this)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "lock control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionLockHelperChildEmission_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    lock (this)
                    {
                        localBuilder.OpenElement(1, "span");
                        localBuilder.AddContent(2, title);
                        localBuilder.CloseElement();
                    }
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "lock control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedGotoHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    goto renderAttributes;

                renderAttributes:
                    builder.AddAttribute(1, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "goto control flow", "replay order", "evaluation count");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedTryGotoHelperAttributeMutation_ThrowsCanonicalizationFailed()
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
                    try
                    {
                        goto renderAttributes;
                    }
                    catch (Exception)
                    {
                        builder.AddAttribute(1, "data-error", true);
                    }

                renderAttributes:
                    builder.AddAttribute(2, "class", title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "try/catch/finally control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionTryGotoHelperAttributeMutation_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    try
                    {
                        goto renderAttributes;
                    }
                    catch (Exception)
                    {
                        localBuilder.AddAttribute(1, "data-error", true);
                    }

                renderAttributes:
                    localBuilder.AddAttribute(2, "class", title);
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "try/catch/finally control flow", "frame depth");
    }

    [TestMethod]
    public void CreateRenderTree_WithNormalOpenElementConditionalChild_PreservesDeclarativeConditionalNode()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                if (ShowPrimary)
                {
                    builder.OpenElement(1, "span");
                    builder.AddContent(2, Title);
                    builder.CloseElement();
                }

                builder.CloseElement();
                """,
                "",
                includeRenderFlags: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.IsFalse(section.ReplayOperations.OfType<RazorVueOpenNodeConditionalReplayOperation>().Any());

        var conditional = section.Children.Children.Single() as RazorVueConditionalNode;
        Assert.IsNotNull(conditional);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);
        Assert.IsTrue(conditional.WhenFalse.Children.IsDefaultOrEmpty);

        var span = conditional.WhenTrue.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsFalse(span.ReplayOperations.OfType<RazorVueOpenNodeConditionalReplayOperation>().Any());
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperEventModifierAfterCallerEventAttribute_PreservesModifierMetadata()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, PreventClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                }
                """,
                includeEventCallback: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNotNull(attribute.EventModifiers.PreventDefault);
        Assert.IsNull(attribute.EventModifiers.StopPropagation);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.EventModifiers.PreventDefault.Value);
        Assert.AreEqual(1, attribute.EventModifiers.PreventDefault.CapturedBindings.Length);
        Assert.AreEqual("preventClick", attribute.EventModifiers.PreventDefault.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.EventModifiers.PreventDefault.CapturedBindings[0].Initializer);

        var scopedReplay = button.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.AreEqual(1, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("preventClick", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        var modifierReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeEventModifierReplayOperation;
        Assert.IsNotNull(modifierReplay);
        Assert.IsNotNull(modifierReplay.EventModifiers.PreventDefault);
        Assert.IsTrue(modifierReplay.EventModifiers.PreventDefault.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperEventModifierBeforeHelperEventAttribute_StripsReplayModifierCapturedBindings()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                RenderBody(builder, PreventClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 1, "onclick", preventClick);
                    builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, OnClick));
                }
                """,
                includeEventCallback: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNotNull(attribute.EventModifiers.PreventDefault);
        Assert.AreEqual(1, attribute.EventModifiers.PreventDefault.CapturedBindings.Length);
        Assert.AreEqual("preventClick", attribute.EventModifiers.PreventDefault.CapturedBindings[0].ParameterSymbol.Name);

        var scopedReplay = button.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.AreEqual(1, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("preventClick", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);

        var modifierReplay = scopedReplay.Operations.OfType<RazorVueOpenNodeEventModifierReplayOperation>().Single();
        Assert.IsNotNull(modifierReplay.EventModifiers.PreventDefault);
        Assert.IsTrue(modifierReplay.EventModifiers.PreventDefault.CapturedBindings.IsDefaultOrEmpty);

        var attributeReplay = scopedReplay.Operations.OfType<RazorVueOpenNodeAttributeReplayOperation>().Single();
        var replayAttribute = attributeReplay.Attribute as RazorVueAttributeNode;
        Assert.IsNotNull(replayAttribute);
        Assert.IsNotNull(replayAttribute.EventModifiers.PreventDefault);
        Assert.IsTrue(replayAttribute.EventModifiers.PreventDefault.CapturedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperEventModifierAfterCallerModifier_PreservesExistingModifierMetadata()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
                RenderBody(builder, StopClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool stopClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(builder, 3, "onclick", stopClick);
                }
                """,
                includeEventCallback: true,
                includeStopClick: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNotNull(attribute.EventModifiers.PreventDefault);
        Assert.IsNotNull(attribute.EventModifiers.StopPropagation);
        Assert.IsTrue(attribute.EventModifiers.PreventDefault.Value.ConstantValue.HasValue);
        Assert.AreEqual(true, attribute.EventModifiers.PreventDefault.Value.ConstantValue.Value);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.EventModifiers.StopPropagation.Value);
        Assert.AreEqual(1, attribute.EventModifiers.StopPropagation.CapturedBindings.Length);
        Assert.AreEqual("stopClick", attribute.EventModifiers.StopPropagation.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.EventModifiers.StopPropagation.CapturedBindings[0].Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperClearingCallerEventModifier_ClearsModifierMetadata()
    {
        var context = CreateContext(
            RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 3, "onclick", false);
                }
                """,
                includeEventCallback: true));

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNull(attribute.EventModifiers.PreventDefault);
        Assert.IsNull(attribute.EventModifiers.StopPropagation);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedHelperEventModifierAfterCallerEventAttribute_LowersModifierInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, PreventClick);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                }
                """,
                includeEventCallback: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"button\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"onclick\", () => emit(\"click\"));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });");

        var scopeIndex = artifact.ModuleCode.IndexOf("((preventClick) =>", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopeIndex);
        var eventModifierIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });", scopeIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, eventModifierIndex);
        var scopeInvocationIndex = artifact.ModuleCode.IndexOf("})(props.preventClick)", eventModifierIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, scopeInvocationIndex);
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedHelperEventModifierUsingSideEffectArgument_EvaluatesArgumentOnceAtCallSite()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                RenderBody(builder, NextPrevent());
                builder.AddAttribute(3, "title", NextTitle());
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", preventClick);
                }

                private bool NextPrevent()
                {
                    return PreventClick;
                }

                private string? NextTitle()
                {
                    return Title;
                }
                """,
                includeEventCallback: true)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setEventModifiers(\"onclick\", { preventDefault: preventClick, stopPropagation: false });");
        StringAssert.Contains(artifact.ModuleCode, "})(nextPrevent());");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "})(nextPrevent());"),
            artifact.ModuleCode);
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"title\", nextTitle());"),
            artifact.ModuleCode);

        var modifierArgumentIndex = artifact.ModuleCode.IndexOf("})(nextPrevent());", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, modifierArgumentIndex);
        var laterAttributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"title\", nextTitle());", modifierArgumentIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, laterAttributeIndex);
        Assert.IsFalse(
            artifact.ModuleCode.Contains("preventDefault: nextPrevent()", StringComparison.Ordinal),
            artifact.ModuleCode);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedHelperEventModifierOnComponentFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenComponent<Panel>(0);
                RenderBody(builder, PreventClick);
                builder.CloseComponent();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 1, "onclick", preventClick);
                }
                """,
                includePanel: true,
                includeEventCallback: true));

        AssertRenderHelperBoundary(exception, "caller-owned", "Event modifiers are only supported on HTML element frames");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionHelperEventModifierOnComponentFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, bool preventClick)
                {
                    WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(localBuilder, 1, "onclick", preventClick);
                }

                builder.OpenComponent<Panel>(0);
                RenderBody(builder, PreventClick);
                builder.CloseComponent();
                """,
                "",
                includePanel: true,
                includeEventCallback: true));

        AssertRenderHelperBoundary(exception, "caller-owned", "Event modifiers are only supported on HTML element frames");
    }

    [TestMethod]
    public void RazorVuePipeline_WithNestedCallerOwnedHelpers_LowersNestedCapturedReplayInCallOrder()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderOuter(builder, Title);
                builder.CloseElement();
                """,
                """
                private void RenderOuter(RenderTreeBuilder builder, string? title)
                {
                    builder.AddAttribute(1, "class", title);
                    RenderInner(builder, title);
                }

                private void RenderInner(RenderTreeBuilder builder, string? title)
                {
                    builder.OpenElement(2, "span");
                    builder.AddContent(3, title);
                    builder.CloseElement();
                }
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setAttribute(\"class\", title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"span\", null, title));");

        var outerScopeIndex = artifact.ModuleCode.IndexOf("((title) => {", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, outerScopeIndex);
        var attributeIndex = artifact.ModuleCode.IndexOf("__jazorRenderContext.setAttribute(\"class\", title);", outerScopeIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, attributeIndex);
        var innerScopeIndex = artifact.ModuleCode.IndexOf("((title) => {", attributeIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, innerScopeIndex);
        var innerInvocationIndex = artifact.ModuleCode.IndexOf("})(title);", innerScopeIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, innerInvocationIndex);
        var outerInvocationIndex = artifact.ModuleCode.IndexOf("})(props.title);", innerInvocationIndex, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, outerInvocationIndex);
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedNoExtraParameterHelperChangingActiveFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    builder.CloseElement();
                    builder.OpenElement(1, "article");
                    builder.AddContent(2, Title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "active caller-owned node");
    }

    [TestMethod]
    public void CreateRenderTree_WithCallerOwnedLocalFunctionHelperChangingActiveFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    localBuilder.CloseElement();
                    localBuilder.OpenElement(1, "article");
                    localBuilder.AddContent(2, title);
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

        AssertRenderHelperBoundary(exception, "caller-owned", "active caller-owned node");
    }

    [TestMethod]
    public void RazorVuePipeline_WithCallerOwnedNoExtraParameterHelperChangingActiveFrame_ThrowsCanonicalizationFailed()
    {
        var exception = AssertPipelineFails(
            RenderHelperSource(
                """
                builder.OpenElement(0, "section");
                RenderBody(builder);
                builder.CloseElement();
                """,
                """
                private void RenderBody(RenderTreeBuilder builder)
                {
                    builder.CloseElement();
                    builder.OpenElement(1, "article");
                    builder.AddContent(2, Title);
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "active caller-owned node");
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
    public void CreateRenderTree_WithCallerOwnedLocalFunctionHelperLeavingElementOpen_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    localBuilder.OpenElement(1, "span");
                    localBuilder.AddContent(2, title);
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

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
    public void CreateRenderTree_WithCallerOwnedLocalFunctionHelperClosingCallerElement_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    localBuilder.AddContent(1, title);
                    localBuilder.CloseElement();
                }

                builder.OpenElement(0, "section");
                RenderBody(builder, Title);
                builder.CloseElement();
                """,
                ""));

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
    public void CreateRenderTree_WithCallerOwnedLocalFunctionHelperLeavingRegionOpen_ThrowsCanonicalizationFailed()
    {
        var exception = AssertCreateRenderTreeFails(
            RenderHelperSource(
                """
                void RenderBody(RenderTreeBuilder localBuilder, string? title)
                {
                    localBuilder.OpenRegion(1);
                    localBuilder.OpenElement(2, "span");
                    localBuilder.AddContent(3, title);
                    localBuilder.CloseElement();
                }

                builder.OpenComponent<Panel>(0);
                RenderBody(builder, Title);
                builder.CloseComponent();
                """,
                "",
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
                    var helper = new HelperBox<string>();
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, HelperBox<string>.Label);
                    builder.CloseElement();
                }
                """,
                """
                private sealed class HelperBox<T>
                {
                    public static string Label = "generic";
                }
                """));

        AssertHelperClassBoundary(exception, "HelperBox", "generic helper classes require erased value-only usage");
    }

    [TestMethod]
    public void RazorVuePipeline_WithErasedGenericHelperClassInImperativeRender_LowersRuntimeHelperClass()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
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
                """)))
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "class HelperBox");
        StringAssert.Contains(artifact.ModuleCode, "new HelperBox(props.title)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(helper.value);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithErasedGenericHelperClassInImperativeRender_LowersRuntimeHelperClass()
    {
        var context = CreateContext(RenderHelperSource(
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
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "RenderHelperHost");
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.SfcText, "class HelperBox");
        StringAssert.Contains(artifact.SfcText, "new HelperBox(props.title)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(helper.value);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithGenericHelperClassTypeParameterTokenInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertPipelineThrows(
            RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperBox<string>(Title);
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.TypeName);
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

                    public string TypeName => typeof(T).Name;
                }
                """));

        AssertHelperClassBoundary(exception, "HelperBox", "generic helper classes require erased value-only usage");
    }

    [TestMethod]
    public void RazorVuePipeline_WithGenericHelperClassStaticMemberInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertPipelineThrows(
            RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperBox<string>(Title);
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, HelperBox<string>.Label);
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

                    public static string Label = "generic";
                }
                """));

        AssertHelperClassBoundary(exception, "HelperBox", "generic helper classes require erased value-only usage");
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

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithComponentHelperClassInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertSfcThrows(
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

        AssertHelperClassBoundary(exception, "HelperComponent", "component types are not same-artifact runtime helper classes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithComponentHelperInstancePropertyInImperativeRender_ThrowsUnsupportedSetupLogicLowering()
    {
        var exception = AssertPipelineThrows(
            RenderHelperSource(
                """
                lock (this)
                {
                    var helper = new HelperComponent { Title = Title };
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, helper.Title);
                    builder.CloseElement();
                }
                """,
                """
                [ECMAScript.ECMAScriptModule("./components/helper-component")]
                private sealed class HelperComponent : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
                """));

        AssertHelperClassBoundary(exception, "HelperComponent", "component types are not same-artifact runtime helper classes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithOpenComponentHelperComponentInImperativeRender_LowersComponentReference()
    {
        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(CreateContext(RenderHelperSource(
                """
                lock (this)
                {
                    builder.OpenComponent<HelperComponent>(0);
                    builder.AddComponentParameter(1, "Title", Title);
                    builder.CloseComponent();
                }
                """,
                """
                [ECMAScript.ECMAScriptModule("./components/helper-component")]
                private sealed class HelperComponent : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
                """)))
            .Artifacts
            .Single(static artifact => artifact.ComponentName == "RenderHelperHost");

        StringAssert.Contains(artifact.ModuleCode, "HelperComponentComponent");
        StringAssert.Contains(artifact.ModuleCode, "import HelperComponentComponent from \"./components/helper-component.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "enterComponent(HelperComponentComponent");
        StringAssert.Contains(artifact.ModuleCode, "setComponentParameter(\"Title\", props.title)");
    }

    private static void AssertGenericRenderHelperRuntimeTypeParameterUsageThrows(
        string helperBody,
        string renderInvocation = "RenderBody<string>(builder);",
        string constraintClause = "",
        string extraMembersPrefix = "")
    {
        var exception = AssertPipelineThrows(
            RenderHelperSource(
                $$"""
                builder.OpenElement(0, "section");
                {{renderInvocation}}
                builder.CloseElement();
                """,
                $$"""
                {{extraMembersPrefix}}

                private void RenderBody<TTitle>(RenderTreeBuilder builder) {{constraintClause}}
                {
                {{Indent(helperBody, "    ")}}
                }
                """));

        AssertRenderHelperBoundary(exception, "caller-owned", "type parameter");
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

    private static int CountOccurrences(string value, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
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
        bool includePanel = false,
        bool includeListCard = false,
        bool includeEventCallback = false,
        bool includeStopClick = false,
        bool includeRenderFlags = false,
        bool includeSecondaryFlag = false,
        bool includeAdditionalAttributes = false)
    {
        var stopClickMember = includeStopClick
            ? """

                    [Parameter]
                    public bool StopClick { get; set; }
            """
            : string.Empty;
        var eventCallbackMembers = includeEventCallback
            ? $$"""

                    [Parameter]
                    public bool PreventClick { get; set; }

                    [Parameter]
                    public EventCallback OnClick { get; set; }
            {{stopClickMember}}
            """
            : string.Empty;
        var renderFlagsMembers = includeRenderFlags
            ? """

                    [Parameter]
                    public bool ShowPrimary { get; set; }
            """
            : string.Empty;
        var secondaryFlagMember = includeSecondaryFlag
            ? """

                    [Parameter]
                    public bool ShowSecondary { get; set; }
            """
            : string.Empty;
        var additionalAttributesMember = includeAdditionalAttributes
            ? """

                    [Parameter]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
            """
            : string.Empty;

        return $$"""
           using System;
           using System.Collections.Generic;
           using ECMAScript.VueContract;
           using Microsoft.AspNetCore.Components;
           using Microsoft.AspNetCore.Components.Rendering;
           using Microsoft.AspNetCore.Components.Web;

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

           """ : string.Empty)}}{{(includeListCard ? """
               [ECMAScript.ECMAScriptModule("./components/list-card")]
               public class ListCard : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public RenderFragment? Header { get; set; }

                   [Parameter]
                   public RenderFragment<int>? ItemTemplate { get; set; }
               }

           """ : string.Empty)}}    [ECMAScript.ECMAScriptModule("./components/render-helper-host")]
               public class RenderHelperHost : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public string? Title { get; set; }
           {{eventCallbackMembers}}
           {{renderFlagsMembers}}
           {{secondaryFlagMember}}
           {{additionalAttributesMember}}

                   protected override void BuildRenderTree(RenderTreeBuilder builder)
                   {
           {{Indent(renderBody, "            ")}}
                   }

           {{Indent(extraMembers, "        ")}}
               }
           }
           """;
    }

    private static string Indent(string text, string indent)
        => string.Join(
            Environment.NewLine,
            text.Trim().Split(["\r\n", "\n"], StringSplitOptions.None).Select(line => indent + line));
}
