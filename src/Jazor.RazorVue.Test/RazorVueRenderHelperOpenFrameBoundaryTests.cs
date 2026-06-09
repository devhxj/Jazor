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
        bool includeEventCallback = false,
        bool includeStopClick = false,
        bool includeRenderFlags = false)
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

        return $$"""
           using System;
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

           """ : string.Empty)}}    [ECMAScript.ECMAScriptModule("./components/render-helper-host")]
               public class RenderHelperHost : ComponentBase, IVueComponent
               {
                   [Parameter]
                   public string? Title { get; set; }
           {{eventCallbackMembers}}
           {{renderFlagsMembers}}

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
