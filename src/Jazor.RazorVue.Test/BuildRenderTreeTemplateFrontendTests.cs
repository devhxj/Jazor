using System;
using System.Linq;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class BuildRenderTreeTemplateFrontendTests
{
    [TestMethod]
    public void CreateRenderTree_ForInlineNamedAndTypedSlotTemplates_ProducesStructuredSlotTemplates()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, Title);
                            headerBuilder.CloseElement();
                        }));
                        builder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, item);
                            itemBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length);
        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ChildCard", component.ComponentName);
        Assert.AreEqual(2, component.SlotTemplates.Length);
        Assert.AreEqual(0, component.Attributes.Length);
        Assert.AreEqual(0, component.Children.Children.Length);

        var headerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Header");
        Assert.AreEqual("header", headerSlot.SlotName);
        Assert.IsNull(headerSlot.ParameterName);
        var headerElement = headerSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(headerElement);
        Assert.AreEqual("h1", headerElement.TagName);
        var headerExpression = headerElement.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(headerExpression);

        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        var itemExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
    }

    [TestMethod]
    public void CreateRenderTree_ForAddComponentParameterWrappedInlineSlotTemplates_ProducesStructuredSlotTemplates()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.CompilerServices;
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
                    public RenderFragment? Header { get; set; }

                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder __builder)
                    {
                        __builder.OpenComponent<ChildCard>(0);
                        __builder.AddComponentParameter(1, nameof(ChildCard.Header), RuntimeHelpers.TypeCheck<RenderFragment>((RenderFragment)((headerBuilder) =>
                        {
                            headerBuilder.OpenElement(2, "h1");
                            headerBuilder.AddContent(3, Title);
                            headerBuilder.CloseElement();
                        })));
                        __builder.AddComponentParameter(4, nameof(ChildCard.ItemTemplate), RuntimeHelpers.TypeCheck<RenderFragment<int>>((RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(5, "p");
                            itemBuilder.AddContent(6, item);
                            itemBuilder.CloseElement();
                        })));
                        __builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(2, component.SlotTemplates.Length);
        Assert.AreEqual(0, component.Attributes.Length);

        var headerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Header");
        Assert.IsNull(headerSlot.ParameterName);
        var headerExpression = ((RazorVueElementNode)headerSlot.Children.Children[0]).Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(headerExpression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(headerExpression.Expression);

        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var itemExpression = ((RazorVueElementNode)itemTemplateSlot.Children.Children[0]).Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForLocalCarrierTypedSlotTemplate_ProducesStructuredSlotTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(2, "p");
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", template);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        var itemExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForAddComponentParameterWrappedTypedSlotTemplateWithNestedComponentAndConditional_PreservesStructuredSubtree()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.BuildRenderTreeTemplateFrontend.RazorGeneratedSlotSubtree.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;
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

                    namespace Demo.Components
                    {
                        [ECMAScript.ECMAScriptModule("./components/item-editor")]
                        public class ItemEditor : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public int ModelValue { get; set; }

                            [Parameter]
                            public EventCallback<int> ModelValueChanged { get; set; }
                        }

                        [ECMAScript.ECMAScriptModule("./components/list-card")]
                        public class ListCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public RenderFragment<int>? ItemTemplate { get; set; }
                        }

                        [ECMAScript.ECMAScriptModule("./components/page")]
                        public partial class Page : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public int Threshold { get; set; }

                            [Parameter]
                            public EventCallback<int> ValueChanged { get; set; }
                        }
                    }
                    """,
                    path: "Page.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using Microsoft.AspNetCore.Components.CompilerServices;
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Components
                    {
                        public partial class Page
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                                __builder.OpenComponent<ListCard>(0);
                                __builder.AddComponentParameter(1, nameof(ListCard.ItemTemplate), RuntimeHelpers.TypeCheck<RenderFragment<int>>((RenderFragment<int>)((item) => (__slotBuilder) =>
                                {
                                    if (item > Threshold)
                                    {
                                        __slotBuilder.OpenComponent<ItemEditor>(2);
                                        __slotBuilder.AddComponentParameter(3, nameof(ItemEditor.ModelValue), RuntimeHelpers.TypeCheck<int>(item));
                                        __slotBuilder.AddComponentParameter(4, nameof(ItemEditor.ModelValueChanged), RuntimeHelpers.TypeCheck<EventCallback<int>>(EventCallback.Factory.Create<int>(this, ValueChanged)));
                                        __slotBuilder.CloseComponent();
                                    }
                                    else
                                    {
                                        __slotBuilder.OpenElement(5, "span");
                                        __slotBuilder.AddContent(6, item);
                                        __slotBuilder.CloseElement();
                                    }
                                })));
                                __builder.CloseComponent();
                            }
                        }
                    }
                    """,
                    path: "Page.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var host = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(host);

        var slot = host.SlotTemplates.Single(static candidate => candidate.PublicName == "ItemTemplate");
        Assert.AreEqual("item", slot.ParameterName);

        var conditional = slot.Children.Children.Single() as RazorVueConditionalNode;
        Assert.IsNotNull(conditional);

        var nestedComponent = conditional.WhenTrue.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(nestedComponent);
        Assert.AreEqual("ItemEditor", nestedComponent.ComponentName);
        Assert.AreEqual(2, nestedComponent.Attributes.Length);
        Assert.IsTrue(nestedComponent.SlotTemplates.IsDefaultOrEmpty);
        Assert.IsTrue(nestedComponent.Children.Children.IsDefaultOrEmpty);

        var modelValue = nestedComponent.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "ModelValue");
        Assert.IsNotNull(modelValue.Value);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(modelValue.Value);

        var modelValueChanged = nestedComponent.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "ModelValueChanged");
        Assert.IsNotNull(modelValueChanged.Value);
        Assert.IsInstanceOfType<IInvocationOperation>(modelValueChanged.Value);

        var fallbackElement = conditional.WhenFalse.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(fallbackElement);
        Assert.AreEqual("span", fallbackElement.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithOpenRegionAndInlineRenderFragment_FlattensFragmentContent()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenRegion(1);
                        builder.AddContent(2, (RenderFragment)((fragmentBuilder) =>
                        {
                            fragmentBuilder.OpenElement(3, "span");
                            fragmentBuilder.AddContent(4, Title);
                            fragmentBuilder.CloseElement();
                        }));
                        builder.CloseRegion();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var span = section.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithOpenComponentUsingTypeOf_ResolvesComponentNode()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(ChildCard));
                        builder.AddComponentParameter(1, nameof(ChildCard.Title), Title);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ChildCard", component.ComponentName);
        Assert.AreEqual("ChildCard", component.ResolutionName);
        Assert.AreEqual(1, component.Attributes.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithRenderTreeBuilderLocalAlias_ProducesStructuredNodes()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var b = builder;
                        b.OpenElement(0, "section");
                        b.AddAttribute(1, "class", "demo");
                        b.AddContent(2, Title);
                        b.OpenComponent<ChildCard>(3);
                        b.AddComponentParameter(4, nameof(ChildCard.Title), Title);
                        b.CloseComponent();
                        b.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(1, section.Attributes.Length);
        Assert.AreEqual("class", ((RazorVueAttributeNode)section.Attributes[0]).Name);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children[0]);

        var component = section.Children.Children[1] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ChildCard", component.ComponentName);
        Assert.AreEqual(1, component.Attributes.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedSlotTemplateWithLocalBuilderAlias_PreservesStructuredSubtree()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.CompilerServices;
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
                [ECMAScript.ECMAScriptModule("./components/item-editor")]
                public class ItemEditor : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/list-card")]
                public class ListCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ListCard>(0);
                        builder.AddComponentParameter(1, nameof(ListCard.ItemTemplate), RuntimeHelpers.TypeCheck<RenderFragment<int>>((RenderFragment<int>)((item) => (__slotBuilder) =>
                        {
                            var slot = __slotBuilder;
                            slot.OpenComponent<ItemEditor>(2);
                            slot.AddComponentParameter(3, nameof(ItemEditor.Value), RuntimeHelpers.TypeCheck<int>(item));
                            slot.CloseComponent();
                        })));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var host = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(host);

        var slot = host.SlotTemplates.Single(static candidate => candidate.PublicName == "ItemTemplate");
        Assert.AreEqual("item", slot.ParameterName);

        var nestedComponent = slot.Children.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(nestedComponent);
        Assert.AreEqual("ItemEditor", nestedComponent.ComponentName);
        Assert.AreEqual(1, nestedComponent.Attributes.Length);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(((RazorVueAttributeNode)nestedComponent.Attributes[0]).Value);
    }

    [TestMethod]
    public void CreateRenderTree_WithUnsupportedRenderTreeBuilderReceiverAlias_ThrowsCanonicalizationFailed()
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
                    private readonly RenderTreeBuilder _cached = new();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var alias = _cached;
                        alias.OpenElement(0, "section");
                        alias.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "local variable declaration");
        StringAssert.Contains(exception.Issue.Message, "alias");
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethod_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder);
                    }

                    private void RenderBody(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithNestedCurrentComponentRenderHelperMethods_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderOuter(builder);
                    }

                    private void RenderOuter(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        RenderInner(builder);
                        builder.CloseElement();
                    }

                    private void RenderInner(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        var span = section.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelper_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder)
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, Title);
                            localBuilder.CloseElement();
                        }

                        RenderBody(builder);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithExpressionBodiedBuildRenderTree_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                        => builder.AddContent(0, Title);
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var expression = renderTree.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithExpressionBodiedCurrentComponentRenderHelperMethod_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder);
                    }

                    private void RenderBody(RenderTreeBuilder builder)
                        => builder.AddContent(0, Title);
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var expression = renderTree.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithExpressionBodiedBuildRenderTreeLocalFunctionHelper_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder)
                            => localBuilder.AddContent(0, Title);

                        RenderBody(builder);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var expression = renderTree.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParameters_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title)
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, title);
                            localBuilder.CloseElement();
                        }

                        RenderBody(builder, Title);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedAttributeMutation_ThrowsCanonicalizationFailed()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title)
                        {
                            localBuilder.AddAttribute(1, "class", title);
                        }

                        builder.OpenElement(0, "section");
                        RenderBody(builder, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "self-contained fragment");
        StringAssert.Contains(exception.Issue.Message, "RenderBody");
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperUsingNamedArguments_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(string? title, RenderTreeBuilder localBuilder)
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, title);
                            localBuilder.CloseElement();
                        }

                        RenderBody(title: Title, localBuilder: builder);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperUsingOmittedOptionalParameter_ProducesStructuredNodes()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title = "fallback-title")
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, title);
                            localBuilder.CloseElement();
                        }

                        RenderBody(builder);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringMultipleExtraParameters_ProducesNestedTemplateScopes()
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

                    [Parameter]
                    public string? Subtitle { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder, Title, Subtitle);
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title, string? subtitle)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "data-title", title);
                        builder.AddAttribute(2, "data-subtitle", subtitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var innerScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(innerScope);
        Assert.AreEqual("subtitle", innerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(innerScope.Initializer);

        var section = innerScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.HasCount(2, section.Attributes);
        var titleAttribute = section.Attributes[0] as RazorVueAttributeNode;
        var subtitleAttribute = section.Attributes[1] as RazorVueAttributeNode;
        Assert.IsNotNull(titleAttribute);
        Assert.IsNotNull(subtitleAttribute);
        Assert.AreEqual("data-title", titleAttribute.Name);
        Assert.AreEqual("data-subtitle", subtitleAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(titleAttribute.Value);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(subtitleAttribute.Value);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringMultipleExtraParameters_ProducesNestedTemplateScopes()
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

                    [Parameter]
                    public string? Subtitle { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title, string? subtitle)
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddAttribute(1, "data-title", title);
                            localBuilder.AddAttribute(2, "data-subtitle", subtitle);
                            localBuilder.CloseElement();
                        }

                        RenderBody(builder, Title, Subtitle);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var innerScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(innerScope);
        Assert.AreEqual("subtitle", innerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(innerScope.Initializer);

        var section = innerScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.HasCount(2, section.Attributes);
        var titleAttribute = section.Attributes[0] as RazorVueAttributeNode;
        var subtitleAttribute = section.Attributes[1] as RazorVueAttributeNode;
        Assert.IsNotNull(titleAttribute);
        Assert.IsNotNull(subtitleAttribute);
        Assert.AreEqual("data-title", titleAttribute.Name);
        Assert.AreEqual("data-subtitle", subtitleAttribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(titleAttribute.Value);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(subtitleAttribute.Value);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
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

                    [Parameter]
                    public string? Subtitle { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(subtitle: Subtitle, builder: builder, title: Title);
                    }

                    private void RenderBody(string? title, RenderTreeBuilder builder, string? subtitle)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "data-title", title);
                        builder.AddAttribute(2, "data-subtitle", subtitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("subtitle", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var innerScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(innerScope);
        Assert.AreEqual("title", innerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(innerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
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

                    [Parameter]
                    public string? Subtitle { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(string? title, RenderTreeBuilder localBuilder, string? subtitle)
                        {
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddAttribute(1, "data-title", title);
                            localBuilder.AddAttribute(2, "data-subtitle", subtitle);
                            localBuilder.CloseElement();
                        }

                        RenderBody(subtitle: Subtitle, localBuilder: builder, title: Title);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("subtitle", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var innerScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(innerScope);
        Assert.AreEqual("title", innerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(innerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodUsingExtraParameterBackedTemplateLocal_ProducesNestedScopes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder, Title);
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        var localTitle = title;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, localTitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var titleScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);

        var localScope = titleScope.Children.Children.OfType<RazorVueLocalDeclarationNode>().Single();
        Assert.IsNotNull(localScope);
        Assert.AreEqual("localTitle", localScope.LocalSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(localScope.Initializer);

        var section = titleScope.Children.Children.OfType<RazorVueElementNode>().Single();
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperUsingExtraParameterBackedTemplateLocal_ProducesNestedScopes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title)
                        {
                            var localTitle = title;
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, localTitle);
                            localBuilder.CloseElement();
                        }

                        RenderBody(builder, Title);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var titleScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);

        var localScope = titleScope.Children.Children.OfType<RazorVueLocalDeclarationNode>().Single();
        Assert.IsNotNull(localScope);
        Assert.AreEqual("localTitle", localScope.LocalSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(localScope.Initializer);

        var section = titleScope.Children.Children.OfType<RazorVueElementNode>().Single();
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithLoopInvokedCurrentComponentRenderHelperMethodUsingExtraParameterBackedTemplateLocal_ProducesNestedLoopAndHelperScopes()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                    public IEnumerable<string>? Items { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        foreach (var item in Items!)
                        {
                            RenderBody(builder, item);
                        }
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        var localTitle = title;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, localTitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var loop = renderTree.Children.Single() as RazorVueForEachNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("item", loop.ItemName);

        var titleScope = loop.Body.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(titleScope.Initializer);

        var localDeclaration = titleScope.Children.Children.OfType<RazorVueLocalDeclarationNode>().Single();
        Assert.AreEqual("localTitle", localDeclaration.LocalSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(localDeclaration.Initializer);

        var section = titleScope.Children.Children.OfType<RazorVueElementNode>().Single();
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithLoopInvokedBuildRenderTreeLocalFunctionHelperUsingExtraParameterBackedTemplateLocal_ProducesNestedLoopAndHelperScopes()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                    public IEnumerable<string>? Items { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, string? title)
                        {
                            var localTitle = title;
                            localBuilder.OpenElement(0, "section");
                            localBuilder.AddContent(1, localTitle);
                            localBuilder.CloseElement();
                        }

                        foreach (var item in Items!)
                        {
                            RenderBody(builder, item);
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var loop = renderTree.Children.Single() as RazorVueForEachNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("item", loop.ItemName);

        var titleScope = loop.Body.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(titleScope.Initializer);

        var localDeclaration = titleScope.Children.Children.OfType<RazorVueLocalDeclarationNode>().Single();
        Assert.AreEqual("localTitle", localDeclaration.LocalSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(localDeclaration.Initializer);

        var section = titleScope.Children.Children.OfType<RazorVueElementNode>().Single();
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParameters_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder, Title);
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAttributeMutation_ThrowsCanonicalizationFailed()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        RenderBody(builder, Title);
                        builder.CloseElement();
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        builder.AddAttribute(1, "class", title);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "self-contained fragment");
        StringAssert.Contains(exception.Issue.Message, "RenderBody");
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodUsingNamedArguments_ProducesStructuredNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(title: Title, builder: builder);
                    }

                    private void RenderBody(string? title, RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodUsingOmittedOptionalParameter_ProducesStructuredNodes()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder);
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title = "fallback-title")
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("title", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithElementSetKey_PreservesNodeKey()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.SetKey("k");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var element = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(element);
        Assert.IsNotNull(element.Key);
        Assert.AreEqual("\"k\"", element.Key.Expression.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentSetKey_PreservesNodeKey()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Id { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.SetKey(Id);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.IsNotNull(component.Key);
        Assert.AreEqual("Id", component.Key.Expression.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_WithAddMultipleAttributes_OnElement_ProducesAttributeSpread()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
                        {
                            ["id"] = "demo"
                        });
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual(1, section.Attributes.Length);
        Assert.IsInstanceOfType<RazorVueAttributeSpreadNode>(section.Attributes[0]);
    }

    [TestMethod]
    public void CreateRenderTree_WithAddElementReferenceCapture_ThrowsCanonicalizationFailed()
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
                    private ElementReference _element;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddElementReferenceCapture(1, value => _element = value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "AddElementReferenceCapture");
    }

    [TestMethod]
    public void CreateRenderTree_WithAddComponentReferenceCapture_ThrowsCanonicalizationFailed()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    private ChildCard? _child;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddComponentReferenceCapture(1, value => _child = (ChildCard)value);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "AddComponentReferenceCapture");
    }

    [TestMethod]
    public void CreateRenderTree_WithTypedAddContentRenderFragment_ProducesTemplateScopeNode()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        }), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var span = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithTypedAddContentRenderFragmentLocalCarrier_ProducesTemplateScopeNode()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        };

                        builder.AddContent(0, template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var span = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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
                    private RenderFragment<int> Template { get; set; } = item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void CreateRenderTree_WithAnalyzableCurrentComponentRenderFragmentPropertyCarrier_ProducesTemplateScopeNode()
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
                    private RenderFragment<int> Template => item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
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

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var span = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithAnalyzableCurrentComponentRenderFragmentFieldCarrier_ProducesTemplateScopeNode()
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
                    private readonly RenderFragment<int> _template = item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
                        itemBuilder.CloseElement();
                    };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, _template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithChainedCurrentComponentRenderFragmentPropertyCarrier_ProducesTemplateScopeNode()
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
                    private RenderFragment<int> PrimaryTemplate => ForwardedTemplate;

                    private RenderFragment<int> ForwardedTemplate => item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
                        itemBuilder.CloseElement();
                    };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, PrimaryTemplate, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var span = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithSelfReferentialCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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
                    private RenderFragment<int> Template => Template;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, Template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "Template");
    }

    [TestMethod]
    public void CreateRenderTree_WithCyclicCurrentComponentRenderFragmentPropertyCarriers_ThrowsCanonicalizationFailed()
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
                    private RenderFragment<int> TemplateA => TemplateB;
                    private RenderFragment<int> TemplateB => TemplateA;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, TemplateA, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "TemplateA");
    }

    [TestMethod]
    public void CreateRenderTree_WithZeroArgumentCurrentComponentRenderFragmentFactoryMethod_ProducesTemplateScopeNode()
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
                    private RenderFragment<int> CreateTemplate()
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);

        var span = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithZeroArgumentLocalRenderFragmentFactoryMethod_ProducesTemplateScopeNode()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> CreateTemplate()
                            => item => itemBuilder =>
                            {
                                itemBuilder.OpenElement(1, "span");
                                itemBuilder.AddContent(2, item);
                                itemBuilder.CloseElement();
                            };

                        builder.AddContent(0, CreateTemplate(), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("item", templateScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(templateScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithZeroArgumentCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplate_ProducesStructuredSlotTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    private RenderFragment<int> CreateTemplate()
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(2, "p");
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", CreateTemplate());
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        var itemExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithZeroArgumentLocalRenderFragmentFactoryMethodForTypedSlotTemplate_ProducesStructuredSlotTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> CreateTemplate()
                            => item => itemBuilder =>
                            {
                                itemBuilder.OpenElement(2, "p");
                                itemBuilder.AddContent(3, item);
                                itemBuilder.CloseElement();
                            };

                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", CreateTemplate());
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethod_ProducesNestedTemplateScopeNodes()
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
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var itemScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(itemScope);
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);

        var span = itemScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(2, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[1]);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentFactoryMethod_ProducesNestedTemplateScopeNodes()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> CreateTemplate(string? title)
                            => item => itemBuilder =>
                            {
                                itemBuilder.OpenElement(1, "span");
                                itemBuilder.AddContent(2, title);
                                itemBuilder.AddContent(3, item);
                                itemBuilder.CloseElement();
                            };

                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var itemScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(itemScope);
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethodUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
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

                    [Parameter]
                    public string? Subtitle { get; set; }

                    private RenderFragment<int> CreateTemplate(string? title, string? subtitle)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, subtitle);
                            itemBuilder.AddContent(4, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(subtitle: Subtitle, title: Title), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("subtitle", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var middleScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(middleScope);
        Assert.AreEqual("title", middleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(middleScope.Initializer);

        var itemScope = middleScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(itemScope);
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplate_ProducesNestedStructuredSlotTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

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
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", CreateTemplate(Title));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);

        var titleScope = itemTemplateSlot.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var paragraph = titleScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("span", paragraph.TagName);
        Assert.AreEqual(2, paragraph.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentFactoryMethodForTypedSlotTemplate_ProducesNestedStructuredSlotTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<int> CreateTemplate(string? title)
                            => item => itemBuilder =>
                            {
                                itemBuilder.OpenElement(1, "span");
                                itemBuilder.AddContent(2, title);
                                itemBuilder.AddContent(3, item);
                                itemBuilder.CloseElement();
                            };

                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", CreateTemplate(Title));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        var titleScope = itemTemplateSlot.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplateUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    private RenderFragment<int> CreateTemplate(string? title, string? subtitle)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, subtitle);
                            itemBuilder.AddContent(4, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", CreateTemplate(subtitle: Subtitle, title: Title));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");

        var outerScope = itemTemplateSlot.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("subtitle", outerScope.ScopeName);

        var middleScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(middleScope);
        Assert.AreEqual("title", middleScope.ScopeName);
    }

    [TestMethod]
    public void CreateRenderTree_WithCyclicCurrentComponentRenderFragmentFactoryMethods_ThrowsCanonicalizationFailed()
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
                    private RenderFragment<int> CreateTemplateA() => CreateTemplateB();
                    private RenderFragment<int> CreateTemplateB() => CreateTemplateA();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplateA(), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursive");
        StringAssert.Contains(exception.Issue.Message, "CreateTemplateA");
    }

    [TestMethod]
    public void CreateRenderTree_WithAddMarkupContent_ThrowsCanonicalizationFailed()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, "<span>unsafe</span>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "AddMarkupContent");
    }

    [TestMethod]
    public void CreateRenderTree_WithMismatchedCloseElement_ThrowsCanonicalizationFailed()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Host>(0);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "CloseElement");
        StringAssert.Contains(exception.Issue.Message, "component");
    }

    [TestMethod]
    public void CreateRenderTree_WithUnclosedOpenNode_ThrowsCanonicalizationFailed()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "dangling");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "unclosed");
        StringAssert.Contains(exception.Issue.Message, "section");
    }

    [TestMethod]
    public void CreateRenderTree_WithMismatchedCloseComponent_ThrowsCanonicalizationFailed()
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "CloseComponent");
        StringAssert.Contains(exception.Issue.Message, "element");
    }

    [TestMethod]
    public void CreateRenderTree_WithConditionalReturn_ThrowsCanonicalizationFailed()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Title is null)
                        {
                            return;
                        }

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "return");
    }

    [TestMethod]
    public void CreateRenderTree_WithWhileLoop_ThrowsCanonicalizationFailed()
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
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var index = 0;
                        while (index < Count)
                        {
                            builder.OpenElement(0, "section");
                            builder.CloseElement();
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "loop statement");
        StringAssert.Contains(exception.Issue.Message, "while");
    }

    [TestMethod]
    public void CreateRenderTree_WithStandaloneFieldMutation_ThrowsCanonicalizationFailed()
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
                    private int _count = 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        _count++;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, _count);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "_count");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentLocalVariableDeclaration_ProducesTemplateScopedLocalNode()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var localTitle = Title;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, localTitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length);
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);

        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithLoopBodyComponentLocalVariableDeclaration_ProducesTemplateScopedLocalNode()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                    public IEnumerable<string>? Items { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        foreach (var item in Items!)
                        {
                            var decorated = item + "!";
                            builder.OpenElement(0, "span");
                            builder.AddContent(1, decorated);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var loop = renderTree.Children.Single() as RazorVueForEachNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual(2, loop.Body.Children.Length);
        var local = loop.Body.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("decorated", local.LocalSymbol.Name);

        var span = loop.Body.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
    }

    [TestMethod]
    public void CreateRenderTree_WithTemplateScopedLocalWithoutInitializer_ThrowsCanonicalizationFailed()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        string? localTitle;
                        localTitle = Title;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, localTitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "requires an initializer");
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.BuildRenderTreeTemplateFrontend.Tests",
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
