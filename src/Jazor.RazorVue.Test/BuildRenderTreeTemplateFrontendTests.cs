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
    public void CreateRenderTree_ForElementDomEventWithPreventDefaultAndStopPropagation_ModifierMetadataStaysOnAttribute()
    {
        var context = CreateContext(
            """
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
                        WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(builder, 3, "onclick", true);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var button = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(button);
        var attribute = button.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("onclick", attribute.Name);
        Assert.IsNotNull(attribute.EventModifiers.PreventDefault);
        Assert.IsNotNull(attribute.EventModifiers.StopPropagation);
    }

    [TestMethod]
    public void CreateRenderTree_ForFalseElementDomEventModifier_DoesNotApplyModifierMetadata()
    {
        var context = CreateContext(
            """
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 1, "onclick", false);
                        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, OnClick));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
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
    public void CreateRenderTree_ForLaterFalseElementDomEventModifier_ClearsModifierMetadata()
    {
        var context = CreateContext(
            """
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 3, "onclick", false);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
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
    public void CreateRenderTree_ForCurrentComponentDefaultSlotForwarding_ToNamedChildSlot_ProducesSlotOutletTemplate()
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Footer { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        builder.AddAttribute(1, "Footer", ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var footerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Footer");
        Assert.AreEqual("footer", footerSlot.SlotName);
        Assert.IsNull(footerSlot.ParameterName);

        var slotOutlet = footerSlot.Children.Children.Single() as RazorVueSlotOutletNode;
        Assert.IsNotNull(slotOutlet);
        Assert.AreEqual("default", slotOutlet.SlotName);
        Assert.IsNull(slotOutlet.Argument);
    }

    [TestMethod]
    public void CreateRenderTree_ForCurrentComponentScopedSlotForwarding_ToTypedChildSlot_ProducesForwardedSlotAttribute()
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        builder.AddAttribute(1, "ItemTemplate", ItemTemplate);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(0, component.SlotTemplates.Length);

        var forwardedAttribute = component.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(forwardedAttribute);
        Assert.AreEqual("ItemTemplate", forwardedAttribute.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(forwardedAttribute.Value);
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
    public void CreateRenderTree_WithCallerOwnedRenderHelperRegion_ProducesChildReplay()
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
                        builder.OpenRegion(1);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                        builder.CloseRegion();
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
        Assert.IsFalse(section.ReplayOperations.IsDefaultOrEmpty);
        var scopedReplay = section.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.AreEqual(1, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        var childReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeChildReplayOperation;
        Assert.IsNotNull(childReplay);
        var replaySpan = childReplay.Child as RazorVueElementNode;
        Assert.IsNotNull(replaySpan);
        Assert.AreEqual("span", replaySpan.TagName);
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
    public void CreateRenderTree_WithOpenComponentUsingTypeOfLocalCarrier_ResolvesComponentNode()
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
                        var childType = typeof(ChildCard);
                        builder.OpenComponent(0, childType);
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
    public void CreateRenderTree_WithOpenComponentUsingImmediatelyAssignedTypeOfLocalCarrier_ResolvesComponentNode()
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
                        Type childType;
                        childType = typeof(ChildCard);
                        builder.OpenComponent(0, childType);
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
    public void CreateRenderTree_WithOpenComponentUsingTypeOfPropertyCarrier_ResolvesComponentNode()
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

                    private Type ChildType => typeof(ChildCard);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, ChildType);
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
    public void CreateRenderTree_WithOpenComponentUsingTypeOfReadonlyFieldCarrier_ResolvesComponentNode()
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

                    private readonly Type _childType = typeof(ChildCard);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, _childType);
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
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedAttributeMutation_PreservesOpenElementAttributes()
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
    public void CreateRenderTree_WithGenericCurrentComponentRenderHelperMethodRequiringExtraParameters_ProducesStructuredNodes()
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

                    private void RenderBody<TTitle>(RenderTreeBuilder builder, TTitle title)
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
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAttributeMutation_PreservesOpenElementAttributes()
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
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedSetKey_PreservesOpenElementKey()
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
                        builder.SetKey(title);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsNotNull(section.Key);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(section.Key.Expression);
        Assert.AreEqual(1, section.Key.CapturedBindings.Length);
        Assert.AreEqual("title", section.Key.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(section.Key.CapturedBindings[0].Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAddMultipleAttributes_PreservesOpenElementSpread()
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
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        RenderBody(builder, AdditionalAttributes);
                        builder.CloseElement();
                    }

                    private void RenderBody(RenderTreeBuilder builder, IReadOnlyDictionary<string, object?>? attributes)
                    {
                        builder.AddMultipleAttributes(1, attributes);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual(1, section.Attributes.Length);
        var spread = section.Attributes[0] as RazorVueAttributeSpreadNode;
        Assert.IsNotNull(spread);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(spread.Expression);
        Assert.AreEqual(1, spread.CapturedBindings.Length);
        Assert.AreEqual("attributes", spread.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(spread.CapturedBindings[0].Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAttributeMutationPlusChildEmission_PreservesOpenElementShape()
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
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
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
        Assert.AreEqual(1, section.Attributes.Length);
        Assert.AreEqual(1, section.Children.Children.Length);

        var attribute = section.Attributes[0] as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.CapturedBindings[0].Initializer);

        var span = section.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedAttributeMutationPlusChildEmission_PreservesOpenElementShape()
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
                            localBuilder.OpenElement(2, "span");
                            localBuilder.AddContent(3, title);
                            localBuilder.CloseElement();
                        }

                        builder.OpenElement(0, "section");
                        RenderBody(builder, Title);
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
        Assert.AreEqual(1, section.Attributes.Length);
        Assert.AreEqual(1, section.Children.Children.Length);

        var attribute = section.Attributes[0] as RazorVueAttributeNode;
        Assert.IsNotNull(attribute);
        Assert.AreEqual("class", attribute.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(attribute.Value);
        Assert.AreEqual(1, attribute.CapturedBindings.Length);
        Assert.AreEqual("title", attribute.CapturedBindings[0].ParameterSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(attribute.CapturedBindings[0].Initializer);

        var span = section.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedImplicitDefaultSlotAssignment_PreservesOpenComponentDefaultSlotShape()
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        RenderBody(builder, Title);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.OpenElement(2, "span");
                            childBuilder.AddContent(3, title);
                            childBuilder.CloseElement();
                        }));
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.AreEqual(1, component.ImplicitDefaultSlotAssignments.Length);
        Assert.IsTrue(component.AmbientDefaultSlotChildren.Children.IsDefaultOrEmpty);

        var defaultSlot = component.ImplicitDefaultSlotAssignments[0];
        var span = defaultSlot.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAmbientDefaultSlotChild_PreservesOpenComponentDefaultSlotShape()
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        RenderBody(builder, Title);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        builder.OpenElement(1, "span");
                        builder.AddContent(2, title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.IsTrue(component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty);
        Assert.AreEqual(1, component.AmbientDefaultSlotChildren.Children.Length);

        var span = component.AmbientDefaultSlotChildren.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedRegionAmbientDefaultSlotChild_PreservesOpenComponentDefaultSlotShape()
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        RenderBody(builder, Title);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
                    {
                        builder.OpenRegion(1);
                        builder.OpenElement(2, "span");
                        builder.AddContent(3, title);
                        builder.CloseElement();
                        builder.CloseRegion();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("Panel", component.ComponentName);
        Assert.IsTrue(component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty);
        Assert.AreEqual(1, component.AmbientDefaultSlotChildren.Children.Length);

        var span = component.AmbientDefaultSlotChildren.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);

        var scopedReplay = component.ReplayOperations.OfType<RazorVueOpenNodeScopedReplayOperation>().Single();
        Assert.AreEqual(1, scopedReplay.CapturedBindings.Length);
        Assert.AreEqual("title", scopedReplay.CapturedBindings[0].ParameterSymbol.Name);
        var slotReplay = scopedReplay.Operations.Single() as RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation;
        Assert.IsNotNull(slotReplay);
        Assert.AreEqual(1, slotReplay.Children.Children.Length);
        var replaySpan = slotReplay.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(replaySpan);
        Assert.AreEqual("span", replaySpan.TagName);
        var replayExpression = replaySpan.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(replayExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(replayExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedNamedAndTypedSlotAssignments_PreservesOpenComponentSlotShape()
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
                [ECMAScript.ECMAScriptModule("./components/list-card")]
                public class ListCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

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
                        builder.OpenComponent<ListCard>(0);
                        RenderBody(builder, Title);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder, string? title)
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
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ListCard", component.ComponentName);
        Assert.AreEqual(2, component.SlotTemplates.Length);
        Assert.IsTrue(component.Attributes.IsDefaultOrEmpty);

        var headerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Header");
        Assert.IsNull(headerSlot.ParameterName);
        var headerElement = headerSlot.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(headerElement);
        var headerExpression = headerElement.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(headerExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(headerExpression.Expression);

        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(3, paragraph.Children.Children.Length);
        var titleExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(titleExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(titleExpression.Expression);
        var itemExpression = paragraph.Children.Children[2] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedNamedAndTypedSlotAssignments_PreservesOpenComponentSlotShape()
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
                [ECMAScript.ECMAScriptModule("./components/list-card")]
                public class ListCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

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
                        void RenderBody(RenderTreeBuilder localBuilder, string? title)
                        {
                            localBuilder.AddAttribute(1, "Header", (RenderFragment)((headerBuilder) =>
                            {
                                headerBuilder.OpenElement(2, "h1");
                                headerBuilder.AddContent(3, title);
                                headerBuilder.CloseElement();
                            }));
                            localBuilder.AddAttribute(4, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                            {
                                itemBuilder.OpenElement(5, "p");
                                itemBuilder.AddContent(6, title);
                                itemBuilder.AddContent(7, " ");
                                itemBuilder.AddContent(8, item);
                                itemBuilder.CloseElement();
                            }));
                        }

                        builder.OpenComponent<ListCard>(0);
                        RenderBody(builder, Title);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(2, component.SlotTemplates.Length);

        var headerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Header");
        var headerExpression = ((RazorVueElementNode)headerSlot.Children.Children.Single()).Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(headerExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(headerExpression.Expression);

        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        var titleExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(titleExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(titleExpression.Expression);
        var itemExpression = paragraph.Children.Children[2] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodAndCallerOwnedScopedSlotForwarding_PreservesForwardedSlotAttribute()
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
                [ECMAScript.ECMAScriptModule("./components/list-card")]
                public class ListCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ListCard>(0);
                        RenderBody(builder);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder)
                    {
                        builder.AddAttribute(1, nameof(ListCard.ItemTemplate), ItemTemplate);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(0, component.SlotTemplates.Length);
        var forwardedAttribute = component.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(forwardedAttribute);
        Assert.AreEqual("ItemTemplate", forwardedAttribute.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(forwardedAttribute.Value);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodAndCallerOwnedNamedSlotForwardingViaAddComponentParameter_PreservesForwardedSlotAttribute()
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
                public sealed record HeaderContext(string Title);

                [ECMAScript.ECMAScriptModule("./components/nav-shell")]
                public class NavShell : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<HeaderContext>? Header { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<HeaderContext>? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<NavShell>(0);
                        RenderBody(builder);
                        builder.CloseComponent();
                    }

                    private void RenderBody(RenderTreeBuilder builder)
                    {
                        builder.AddComponentParameter(1, nameof(NavShell.Header), RuntimeHelpers.TypeCheck<RenderFragment<HeaderContext>?>(Header));
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var component = renderTree.Children.Single() as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(0, component.SlotTemplates.Length);
        var forwardedAttribute = component.Attributes.Single() as RazorVueAttributeNode;
        Assert.IsNotNull(forwardedAttribute);
        Assert.AreEqual("Header", forwardedAttribute.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(forwardedAttribute.Value);
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
    public void CreateRenderTree_WithCurrentComponentRenderHelperMethodUsingParamsParameter_ProducesStructuredNodes()
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
                        RenderBody(builder, Title, "suffix");
                    }

                    private void RenderBody(RenderTreeBuilder builder, params string?[] values)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, values.Length);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var templateScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(templateScope);
        Assert.AreEqual("values", templateScope.ScopeName);
        Assert.IsInstanceOfType<IArrayCreationOperation>(templateScope.Initializer);

        var section = templateScope.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(expression.Expression);
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
    public void CreateRenderTree_WithUnwrittenSettableCurrentComponentRenderFragmentPropertyCarrier_ProducesTemplateScopeNode()
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
    public void CreateRenderTree_WithNonPrivateSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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
                    internal RenderFragment<int> Template { get; set; } = item => itemBuilder =>
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
    public void CreateRenderTree_WithReassignedSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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

                    public Host()
                    {
                        Template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(3, "em");
                            itemBuilder.AddContent(4, item);
                            itemBuilder.CloseElement();
                        };
                    }

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
    public void CreateRenderTree_WithAnalyzableCurrentComponentRenderFragmentAutoPropertyCarrier_ProducesTemplateScopeNode()
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
                    private RenderFragment<int> Template { get; } = item => itemBuilder =>
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
    public void CreateRenderTree_WithUnwrittenNonReadonlyCurrentComponentRenderFragmentFieldCarrier_ProducesTemplateScopeNode()
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
                    private RenderFragment<int> _template = item => itemBuilder =>
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
    public void CreateRenderTree_WithNonPrivateNonReadonlyCurrentComponentRenderFragmentFieldCarrier_ThrowsCanonicalizationFailed()
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
                    internal RenderFragment<int> _template = item => itemBuilder =>
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void CreateRenderTree_WithReassignedNonReadonlyCurrentComponentRenderFragmentFieldCarrier_ThrowsCanonicalizationFailed()
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
                    private RenderFragment<int> _template = item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
                        itemBuilder.CloseElement();
                    };

                    public Host()
                    {
                        _template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(3, "em");
                            itemBuilder.AddContent(4, item);
                            itemBuilder.CloseElement();
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, _template, 42);
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
    public void CreateRenderTree_WithGenericParameterizedCurrentComponentRenderFragmentFactoryMethod_ProducesNestedTemplateScopeNodes()
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

                    private RenderFragment<int> CreateTemplate<TTitle>(TTitle title)
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
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethodWithOmittedOptionalParameter_ProducesNestedTemplateScopeNodes()
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
                    private RenderFragment<int> CreateTemplate(string? title = "fallback-title")
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, title);
                            itemBuilder.AddContent(3, item);
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

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(outerScope.Initializer);

        var itemScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(itemScope);
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentFactoryMethodUsingParamsParameter_ProducesNestedTemplateScopeNodes()
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

                    private RenderFragment<int> CreateTemplate(params string?[] values)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, values.Length);
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title, "suffix"), 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("values", outerScope.ScopeName);
        Assert.IsInstanceOfType<IArrayCreationOperation>(outerScope.Initializer);

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
    public void CreateRenderTree_WithGenericParameterizedLocalRenderFragmentFactoryMethod_ProducesNestedTemplateScopeNodes()
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
                        RenderFragment<int> CreateTemplate<TTitle>(TTitle title)
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
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentCarrierInitializedFromFactoryMethod_PreservesCapturedScopeOutsideTypedFragmentScope()
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
                        RenderFragment<int> template = CreateTemplate(Title);
                        builder.AddContent(0, template, 42);
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
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentCarrierAssignedImmediatelyFromFactoryMethod_PreservesCapturedScopeOutsideTypedFragmentScope()
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
                        RenderFragment<int> template;
                        template = CreateTemplate(Title);
                        builder.AddContent(0, template, 42);
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
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentCarrierAssignedAfterSiblingLocalDeclaration_PreservesCapturedScopeOutsideTypedFragmentScope()
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
                        RenderFragment<int> template;
                        var revision = 0;
                        template = CreateTemplate(Title);
                        builder.AddContent(0, template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(2, renderTree.Children.Length);
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("revision", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<ILiteralOperation>(local.Initializer);

        var outerScope = renderTree.Children[1] as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("title", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var itemScope = outerScope.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(itemScope);
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethod_ProducesNestedTemplateScopeNodes()
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

                    private RenderFragment<int> Template => CreateTemplate(Title);

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
    public void CreateRenderTree_WithParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethodForTypedSlotTemplate_ProducesStructuredSlotTemplate()
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

                    private RenderFragment<int> Template => CreateTemplate(Title);

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
                        builder.AddAttribute(1, "ItemTemplate", Template);
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
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterizedLocalRenderFragmentCarrierAssignedImmediatelyFromFactoryMethodForTypedSlotTemplate_ProducesStructuredSlotTemplate()
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
                        RenderFragment<int> template;
                        template = CreateTemplate(Title);
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", template);
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
    }

    [TestMethod]
    public void CreateRenderTree_WithMixedImperativeTypedSlotUsingImmediatelyAssignedLocalRenderFragmentCarrier_ProducesStructuredSlotTemplate()
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
                        RenderFragment<int> template;
                        template = CreateTemplate(Title);
                        var revision = 0;

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();

                        revision = revision + 1;

                        builder.OpenComponent<ChildCard>(4);
                        builder.AddAttribute(5, "ItemTemplate", template);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(4, renderTree.Children.Length);
        Assert.IsTrue(renderTree.Children.Any(static child => child is RazorVueElementNode));
        Assert.IsTrue(renderTree.Children.Any(static child => child is RazorVueLocalDeclarationNode));
        Assert.IsTrue(renderTree.Children.Any(static child => child is RazorVueImperativeBlockNode));

        var component = renderTree.Children.OfType<RazorVueComponentNode>().Single();
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);

        var titleScope = itemTemplateSlot.Children.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(titleScope);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithRenderFragmentLocalCarrierAssignedNonImmediately_ThrowsCanonicalizationFailed()
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
                        RenderFragment<int> template;
                        builder.OpenElement(0, "section");
                        builder.CloseElement();
                        template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, item);
                            itemBuilder.CloseElement();
                        };
                        builder.AddContent(3, template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "same linear local-declaration prefix");
    }

    [TestMethod]
    public void CreateRenderTree_WithRenderFragmentLocalCarrierAssignedImmediatelyThenReassigned_ThrowsCanonicalizationFailed()
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
                        RenderFragment<int> template;
                        template = CreateTemplate(Title);
                        template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(4, "strong");
                            itemBuilder.AddContent(5, item);
                            itemBuilder.CloseElement();
                        };
                        builder.AddContent(6, template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "assigned exactly once");
    }

    [TestMethod]
    public void CreateRenderTree_WithMixedImperativeDeclarationInitializedRenderFragmentLocalCarrierThenReassigned_ThrowsCanonicalizationFailed()
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
                        RenderFragment<int> template = CreateTemplate(Title);

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();

                        template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(4, "strong");
                            itemBuilder.AddContent(5, item);
                            itemBuilder.CloseElement();
                        };

                        builder.OpenComponent<ChildCard>(6);
                        builder.AddAttribute(7, "ItemTemplate", template);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_WithDeclarationInitializedRenderFragmentLocalCarrierThenReassigned_PreservesImperativeRootBlock()
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
                        RenderFragment<int> template = CreateTemplate(Title);
                        template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(4, "strong");
                            itemBuilder.AddContent(5, item);
                            itemBuilder.CloseElement();
                        };
                        builder.AddContent(6, template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithAnalyzableCurrentComponentRenderFragmentAutoPropertyCarrierForTypedSlotTemplate_ProducesStructuredSlotTemplate()
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
                    private RenderFragment<int> Template { get; } = item => itemBuilder =>
                    {
                        itemBuilder.OpenElement(1, "span");
                        itemBuilder.AddContent(2, item);
                        itemBuilder.CloseElement();
                    };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", Template);
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

        var span = itemTemplateSlot.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.TagName);
        var expression = span.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
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
    public void CreateRenderTree_WithConstantAddMarkupContent_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span><p>ok</p></section>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var classAttribute = section.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("hero", classAttribute.Value.ConstantValue.Value);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithLocalAddMarkupContentCarrier_ProducesStaticMarkupNodes()
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
                        const string markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddMarkupContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithDeclarationInitializedNonConstAddMarkupContentCarrier_ProducesStaticMarkupNodes()
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
                        string markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddMarkupContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithImmediatelyAssignedAddMarkupContentCarrier_ProducesStaticMarkupNodes()
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
                        string markup;
                        markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddMarkupContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithImmediatelyAssignedAddMarkupContentCarrierAfterSiblingLocalDeclaration_ProducesStaticMarkupNodes()
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
                        string markup;
                        var revision = 0;
                        markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddMarkupContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length);
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("revision", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<ILiteralOperation>(local.Initializer);

        var markup = renderTree.Children[1] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(markup);
        Assert.AreEqual("markup", markup.LocalSymbol.Name);

        var section = renderTree.Children[2] as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithReadonlyAddMarkupContentPropertyCarrier_ProducesStaticMarkupNodes()
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
                    private string HeroMarkup => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithUnwrittenSettableAddMarkupContentPropertyCarrier_ProducesStaticMarkupNodes()
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
                    private string HeroMarkup { get; set; } = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithReadonlyAddMarkupContentFieldCarrier_ProducesStaticMarkupNodes()
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
                    private readonly string _heroMarkup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, _heroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithUnwrittenNonReadonlyAddMarkupContentFieldCarrier_ProducesStaticMarkupNodes()
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
                    private string _heroMarkup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, _heroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentStaticMarkupFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, CreateMarkup());
                    }

                    private string CreateMarkup()
                        => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithLocalFunctionStaticMarkupFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, CreateMarkup());

                        string CreateMarkup()
                            => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithFactoryBackedAddMarkupContentPropertyCarrier_ProducesStaticMarkupNodes()
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
                    private string HeroMarkup => CreateMarkup();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, HeroMarkup);
                    }

                    private string CreateMarkup()
                        => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterIndependentStaticMarkupFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, CreateMarkup(Title));
                    }

                    private string CreateMarkup(string? ignored)
                        => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("ignored", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var section = outerScope.Children.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithStaticMarkupFactoryMethodUsingOmittedOptionalParameter_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, CreateMarkup());
                    }

                    private string CreateMarkup(string? ignored = "fallback-title")
                        => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("ignored", outerScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(outerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithStaticMarkupFactoryMethodUsingParamsParameter_ProducesStaticMarkupNodes()
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
                        builder.AddMarkupContent(0, CreateMarkup(Title, "suffix"));
                    }

                    private string CreateMarkup(params string?[] values)
                        => "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("values", outerScope.ScopeName);
        Assert.IsInstanceOfType<IArrayCreationOperation>(outerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithCurrentComponentMarkupStringFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, CreateMarkup());
                    }

                    private MarkupString CreateMarkup()
                        => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithLocalFunctionMarkupStringFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, CreateMarkup());

                        MarkupString CreateMarkup()
                            => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithParameterIndependentMarkupStringFactoryMethod_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, CreateMarkup(Title));
                    }

                    private MarkupString CreateMarkup(string? ignored)
                        => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("ignored", outerScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(outerScope.Initializer);

        var section = outerScope.Children.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithMarkupStringFactoryMethodUsingOmittedOptionalParameter_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, CreateMarkup());
                    }

                    private MarkupString CreateMarkup(string? ignored = "fallback-title")
                        => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("ignored", outerScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(outerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithMarkupStringFactoryMethodUsingParamsParameter_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, CreateMarkup(Title, "suffix"));
                    }

                    private MarkupString CreateMarkup(params string?[] values)
                        => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var outerScope = renderTree.Children.Single() as RazorVueTemplateScopeNode;
        Assert.IsNotNull(outerScope);
        Assert.AreEqual("values", outerScope.ScopeName);
        Assert.IsInstanceOfType<IArrayCreationOperation>(outerScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_WithFactoryBackedImmediateMarkupStringLocalCarrier_ProducesStaticMarkupNodes()
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
                        MarkupString markup;
                        markup = CreateMarkup();
                        builder.AddContent(0, markup);
                    }

                    private MarkupString CreateMarkup()
                        => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.OfType<RazorVueElementNode>().Single(static node => node.TagName == "section");
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithDynamicAddMarkupContentCarrier_ThrowsCanonicalizationFailed()
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
                        var markup = "<section>" + Title + "</section>";
                        builder.AddMarkupContent(0, markup);
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
    public void CreateRenderTree_WithImmediatelyAssignedAddMarkupContentCarrierThenReassigned_ThrowsCanonicalizationFailed()
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
                        string markup;
                        markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        markup = "<section class=\"hero\"><span>changed</span></section>";
                        builder.AddMarkupContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "markup");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_WithReassignedSettableAddMarkupContentPropertyCarrier_ThrowsCanonicalizationFailed()
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
                    private string HeroMarkup { get; set; } = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    public Host()
                    {
                        HeroMarkup = "<section class=\"hero\"><span>changed</span></section>";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, HeroMarkup);
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
    public void CreateRenderTree_WithNonPrivateSettableAddMarkupContentPropertyCarrier_ThrowsCanonicalizationFailed()
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
                    internal string HeroMarkup { get; set; } = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, HeroMarkup);
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
    public void CreateRenderTree_WithConstantMarkupStringAddContent_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        var classAttribute = section.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("hero", classAttribute.Value.ConstantValue.Value);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithNewMarkupStringAddContent_ProducesStaticMarkupNodes()
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
                        builder.AddContent(0, new MarkupString("<section class=\"hero\"><span>safe</span><p>ok</p></section>"));
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithLocalMarkupStringCarrierAddContent_ProducesStaticMarkupNodes()
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
                        MarkupString markup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithReadonlyMarkupStringPropertyCarrierAddContent_ProducesStaticMarkupNodes()
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
                    private MarkupString HeroMarkup => (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithUnwrittenSettableMarkupStringPropertyCarrierAddContent_ProducesStaticMarkupNodes()
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
                    private MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithReadonlyMarkupStringFieldCarrierAddContent_ProducesStaticMarkupNodes()
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
                    private readonly MarkupString _heroMarkup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, _heroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithUnwrittenNonReadonlyMarkupStringFieldCarrierAddContent_ProducesStaticMarkupNodes()
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
                    private MarkupString _heroMarkup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, _heroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithImmediatelyAssignedMarkupStringLocalCarrierAddContent_ProducesStaticMarkupNodes()
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
                        MarkupString markup;
                        markup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        builder.AddContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        var section = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_WithImmediatelyAssignedMarkupStringLocalCarrierThenReassigned_ThrowsCanonicalizationFailed()
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
                        MarkupString markup;
                        markup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";
                        markup = (MarkupString)"<section class=\"hero\"><span>changed</span></section>";
                        builder.AddContent(0, markup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString local 'markup'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_WithReassignedSettableMarkupStringPropertyCarrierAddContent_ThrowsCanonicalizationFailed()
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
                    private MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    public Host()
                    {
                        HeroMarkup = (MarkupString)"<section class=\"hero\"><span>changed</span></section>";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString");
    }

    [TestMethod]
    public void CreateRenderTree_WithNonPrivateSettableMarkupStringPropertyCarrierAddContent_ThrowsCanonicalizationFailed()
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
                    internal MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, HeroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString");
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
    public void CreateRenderTree_WithConditionalReturn_ProducesImperativeMethodBodyNode()
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
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        Assert.AreEqual(1, imperative.VisibleParameters.Length);
        Assert.AreEqual("builder", imperative.VisibleParameters[0].Name);
    }

    [TestMethod]
    public void CreateRenderTree_WithWhileLoop_ProducesImperativeLoopBlockNode()
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
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_WithDoWhileLoop_ProducesImperativeLoopBlockNode()
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
                        do
                        {
                            builder.OpenElement(0, "section");
                            builder.CloseElement();
                            index++;
                        }
                        while (index < Count);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_WithDeclarativeSiblingsAroundWhileLoop_PromotesOnlyLoopBlock()
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
                        builder.OpenElement(0, "header");
                        builder.AddContent(1, "start");
                        builder.CloseElement();

                        var index = 0;
                        while (index < Count)
                        {
                            builder.OpenElement(2, "section");
                            builder.AddContent(3, index);
                            builder.CloseElement();
                            index++;
                        }

                        builder.OpenElement(4, "footer");
                        builder.AddContent(5, "end");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length);
        Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[2]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, ((RazorVueImperativeBlockNode)renderTree.Children[1]).Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithForLoopAndContinue_ProducesImperativeLoopBlockNode()
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
                        for (var index = 0; index < Count; index++)
                        {
                            if ((index % 2) == 0)
                            {
                                continue;
                            }

                            builder.OpenElement(0, "section");
                            builder.AddContent(1, index);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_WithNonCountStyleForLoop_ProducesImperativeLoopBlockNode()
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

                    [Parameter]
                    public int Total { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var index = 0; index < Count; index++, Total++)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, index);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_WithForEachLoopAndBreak_ProducesImperativeLoopBlockNode()
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
                    public IReadOnlyList<int>? Items { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        foreach (var item in Items!)
                        {
                            if (item < 0)
                            {
                                break;
                            }

                            builder.OpenElement(0, "section");
                            builder.AddContent(1, item);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "item");
    }

    [TestMethod]
    public void CreateRenderTree_WithSwitchStatement_ProducesImperativeSwitchBlockNode()
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
                        switch (Count)
                        {
                            case 0:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, "empty");
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(2, "section");
                                builder.AddContent(3, Count);
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.SwitchBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithTryCatchFinally_ProducesImperativeTryBlockNode()
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
                    private int _count;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        catch
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, "fallback");
                            builder.CloseElement();
                        }
                        finally
                        {
                            _count++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithThrowStatement_ProducesImperativeMethodBodyNode()
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
                [ECMAScript.ECMAScriptModule("./components/throw-card")]
                public class ThrowCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Fail)
                        {
                            throw new InvalidOperationException("boom");
                        }

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithUsingStatement_ProducesImperativeTryBlockNode()
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
                public sealed class TestDisposable : IDisposable
                {
                    public void Dispose() { }
                }

                [ECMAScript.ECMAScriptModule("./components/using-statement-card")]
                public class UsingStatementCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using (GetDisposable())
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                    }

                    private IDisposable GetDisposable() => new TestDisposable();
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithLockStatement_ProducesImperativeLockBlockNode()
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
                [ECMAScript.ECMAScriptModule("./components/lock-card")]
                public class LockCard : ComponentBase, IVueComponent
                {
                    private readonly object _gate = new();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        lock (_gate)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LockBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_WithStandaloneFieldMutation_ProducesImperativeLocalBlockNode()
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
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        Assert.AreEqual(2, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithMixedReferencedLocalFunctionImperativeSegment_PreservesLocalFunctionDeclarationInSegment()
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
                        builder.OpenElement(0, "header");
                        builder.AddContent(1, Title);
                        builder.CloseElement();

                        void AppendLine(string value)
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, value);
                            builder.CloseElement();
                        }

                        var index = 0;
                        while (index < 1)
                        {
                            AppendLine("ready");
                            index++;
                        }

                        builder.OpenElement(4, "footer");
                        builder.AddContent(5, "done");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length);
        Assert.AreEqual("header", ((RazorVueElementNode)renderTree.Children[0]).TagName);

        var imperative = renderTree.Children[1] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        Assert.IsInstanceOfType<ILocalFunctionOperation>(imperative.Operations[0]);
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IVariableDeclarationGroupOperation));
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IWhileLoopOperation));

        Assert.AreEqual("footer", ((RazorVueElementNode)renderTree.Children[2]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_WithMixedDeconstructionAssignmentImperativeSegment_PreservesDeconstructionDeclarationInSegment()
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
                        builder.OpenElement(0, "header");
                        builder.AddContent(1, Title);
                        builder.CloseElement();

                        var pair = (Title, "ready");
                        var (label, suffix) = pair;
                        var index = 0;
                        while (index < 1)
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, label);
                            builder.AddContent(4, suffix);
                            builder.CloseElement();
                            index++;
                        }

                        builder.OpenElement(5, "footer");
                        builder.AddContent(6, label);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length);
        Assert.AreEqual("header", ((RazorVueElementNode)renderTree.Children[0]).TagName);

        var imperative = renderTree.Children[1] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IExpressionStatementOperation { Operation: IDeconstructionAssignmentOperation }));
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IWhileLoopOperation));
        Assert.IsTrue(imperative.Operations.Any(static operation =>
            operation is IExpressionStatementOperation { Operation: IInvocationOperation { TargetMethod.Name: "OpenElement" } }));
    }

    [TestMethod]
    public void CreateRenderTree_WithMixedLabeledBlock_ProducesImperativeSegment()
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
                        builder.OpenElement(0, "header");
                        builder.AddContent(1, Title);
                        builder.CloseElement();

                        renderBlock:
                        {
                            builder.OpenElement(2, "section");
                            builder.AddContent(3, "labeled");
                            builder.CloseElement();
                        }

                        builder.OpenElement(4, "footer");
                        builder.AddContent(5, "done");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length);
        Assert.AreEqual("header", ((RazorVueElementNode)renderTree.Children[0]).TagName);

        var imperative = renderTree.Children[1] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        Assert.IsInstanceOfType<ILabeledOperation>(imperative.Operations[0]);

        Assert.AreEqual("footer", ((RazorVueElementNode)renderTree.Children[2]).TagName);
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
    public void CreateRenderTree_WithTemplateScopedLocalWithoutInitializerThenImmediateAssignment_ProducesTemplateScopedLocalNode()
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
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length);
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(local.Initializer);

        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_WithTemplateScopedLocalAssignedAfterSiblingLocalDeclaration_ProducesOrderedLocalDeclarationNodes()
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
                        var revision = 0;
                        localTitle = Title;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, localTitle);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length);

        var revision = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(revision);
        Assert.AreEqual("revision", revision.LocalSymbol.Name);
        Assert.IsInstanceOfType<ILiteralOperation>(revision.Initializer);

        var localTitle = renderTree.Children[1] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(localTitle);
        Assert.AreEqual("localTitle", localTitle.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(localTitle.Initializer);

        var section = renderTree.Children[2] as RazorVueElementNode;
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
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
