using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueFragmentSlotCarrierBoundaryTests
{
    [TestMethod]
    public void CreateRenderTree_WithRefParameterRenderFragmentFactory_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> CreateTemplate(ref string? title)
                    {
                        var captured = title;
                        return item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, captured);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var title = Title;
                        builder.AddContent(0, CreateTemplate(ref title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "ref");
    }

    [TestMethod]
    public void CreateRenderTree_WithOutParameterRenderFragmentFactory_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    private RenderFragment<int> CreateTemplate(out string? title)
                    {
                        title = "fallback";
                        return item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(out _), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "out");
    }

    [TestMethod]
    public void CreateRenderTree_WithDataflowGetterRenderFragmentCarrier_ProducesStructuredTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> Template
                    {
                        get
                        {
                            var template = CreateTemplate(Title);
                            return template;
                        }
                    }

                    private RenderFragment<int> CreateTemplate(string? title)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, Template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRenderFragmentFactoryReturningLocalDelegate_LowersStructuredTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> CreateTemplate(string? title)
                    {
                        RenderFragment<int> template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                        return template;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.SfcText, "span");
        StringAssert.Contains(artifact.SfcText, "props.title");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRenderFragmentFactoryReturningLocalDelegateAliasChain_LowersStructuredTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> CreateTemplate(string? title)
                    {
                        RenderFragment<int> template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                        RenderFragment<int> alias;
                        alias = template;
                        return alias;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot);

        StringAssert.Contains(artifact.SfcText, "span");
        StringAssert.Contains(artifact.SfcText, "props.title");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithSideEffectBeforeReturnedRenderFragmentFactoryLocalDelegate_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> CreateTemplate(string? title)
                    {
                        _ = title?.Trim();
                        RenderFragment<int> template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                        return template;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "analyzable return value");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRefParameterRenderFragmentFactory_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> CreateTemplate(ref string? title)
                    {
                        var captured = title;
                        return item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, captured);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var title = Title;
                        builder.AddContent(0, CreateTemplate(ref title), 42);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "ref");
    }

    [TestMethod]
    public void RazorVuePipeline_WithOutParameterRenderFragmentFactory_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    private RenderFragment<int> CreateTemplate(out string? title)
                    {
                        title = "fallback";
                        return item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(out _), 42);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "out");
    }

    [TestMethod]
    public void RazorVuePipeline_WithConditionalGetterRenderFragmentCarrier_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool UseAlternate { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    private RenderFragment<int> Template => UseAlternate
                        ? CreateTemplate(Title)
                        : CreateTemplate(Subtitle);

                    private RenderFragment<int> CreateTemplate(string? title)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, Template, 42);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.FragmentSlotCarrierBoundary.Tests",
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
