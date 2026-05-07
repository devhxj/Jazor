using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue.Sfc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueCanonicalSfcSemanticTests
{
    private static RazorVueCanonicalHModelFactory CreateBuildRenderTreeCanonicalFactory()
        => new(BuildRenderTreeTemplateFrontend.Instance);

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_CreatesTemplateAndSfcSemanticModel_ForSimpleElementComponent()
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
                [ECMAScript.ECMAScriptModule("./components/counter-card")]
                public class CounterCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "data-count", Value);
                        builder.AddContent(2, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("CounterCard", canonical.ComponentName);
        Assert.AreEqual("components/counter-card.mjs", canonical.RelativeComponentPath);
        Assert.HasCount(1, canonical.Template.Children);
        Assert.IsInstanceOfType<RazorVueCanonicalElementNode>(canonical.Template.Children[0]);

        var root = (RazorVueCanonicalElementNode)canonical.Template.Children[0];
        Assert.AreEqual("section", root.TagName);
        Assert.HasCount(1, root.Attributes);
        var rootAttribute = AssertNode<RazorVueCanonicalAttributeBinding>(root.Attributes[0]);
        Assert.AreEqual("data-count", rootAttribute.Name);
        Assert.AreEqual("props.value", rootAttribute.ExpressionText);
        Assert.HasCount(1, root.Children.Children);
        Assert.IsInstanceOfType<RazorVueCanonicalInterpolationNode>(root.Children.Children[0]);

        var interpolation = (RazorVueCanonicalInterpolationNode)root.Children.Children[0];
        Assert.AreEqual("props.title", interpolation.ExpressionText);
        Assert.AreEqual(RazorVueTemplateEncodability.DirectTemplate, interpolation.TemplateEncodability);

        Assert.AreEqual("components/counter-card.vue", sfc.RelativeSfcPath);
        Assert.AreEqual(canonical.ComponentName, sfc.ComponentName);
        Assert.AreEqual(canonical.Template, sfc.TemplateBlock.Template);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.StyleBlocks.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_ClassifiesLoopAndConditional_AsTemplateViaSetupBinding()
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
                [ECMAScript.ECMAScriptModule("./components/list-card")]
                public class ListCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public IEnumerable<string>? Items { get; set; }

                    [Parameter]
                    public bool Visible { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Visible)
                        {
                            foreach (var item in Items!)
                            {
                                builder.OpenElement(0, "span");
                                builder.AddContent(1, item);
                                builder.CloseElement();
                            }
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var conditional = AssertNode<RazorVueCanonicalConditionalNode>(canonical.Template.Children.Single());
        var loop = AssertNode<RazorVueCanonicalForEachNode>(conditional.WhenTrue.Children.Single());

        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, conditional.TemplateEncodability);
        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, loop.TemplateEncodability);

        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);
        Assert.AreEqual(2, sfc.ScriptSetupBlock.LiftedBindings.Length);
        Assert.AreEqual("__jazorVueSfcBinding0", sfc.ScriptSetupBlock.LiftedBindings[0].Name);
        Assert.AreEqual("props.visible", sfc.ScriptSetupBlock.LiftedBindings[0].ExpressionText);
        Assert.AreEqual("__jazorVueSfcBinding1", sfc.ScriptSetupBlock.LiftedBindings[1].Name);
        Assert.AreEqual("props.items", sfc.ScriptSetupBlock.LiftedBindings[1].ExpressionText);
        Assert.AreEqual("root/child[0]/if", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("__jazorVueSfcBinding0", sfc.TemplateBlock.BindingSites[0].BindingName);
        Assert.AreEqual("root/child[0]/whenTrue/child[0]/forEach", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("__jazorVueSfcBinding1", sfc.TemplateBlock.BindingSites[1].BindingName);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_ClassifiesCountStyleFor_AsTemplateViaSetupBinding()
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
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var i = 0; i < Count; i++)
                        {
                            builder.OpenElement(0, "span");
                            builder.AddContent(1, i);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var loop = AssertNode<RazorVueCanonicalForNode>(canonical.Template.Children.Single());

        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, loop.TemplateEncodability);
        Assert.AreEqual("0", loop.InitialValueExpressionText);
        Assert.AreEqual("props.count", loop.LimitValueExpressionText);
        Assert.IsNull(loop.StepValueExpressionText);

        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);
        Assert.AreEqual(2, sfc.ScriptSetupBlock.LiftedBindings.Length);
        Assert.AreEqual("root/child[0]/for/init", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("root/child[0]/for/limit", sfc.TemplateBlock.BindingSites[1].SitePath);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_ClassifiesCountStyleForWithExplicitStep_AsTemplateViaSetupBinding()
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
                    public int Start { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public int Step { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var i = Start; i <= Count; i += Step)
                        {
                            builder.OpenElement(0, "span");
                            builder.AddContent(1, i);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var loop = AssertNode<RazorVueCanonicalForNode>(canonical.Template.Children.Single());

        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, loop.TemplateEncodability);
        Assert.AreEqual("props.start", loop.InitialValueExpressionText);
        Assert.AreEqual("props.count", loop.LimitValueExpressionText);
        Assert.AreEqual("props.step", loop.StepValueExpressionText);

        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);
        Assert.AreEqual(3, sfc.ScriptSetupBlock.LiftedBindings.Length);
        Assert.AreEqual("root/child[0]/for/init", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("root/child[0]/for/limit", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("root/child[0]/for/step", sfc.TemplateBlock.BindingSites[2].SitePath);
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_ThrowsForNotTemplateEncodableCanonicalNode()
    {
        var descriptor = new VueComponentDescriptor(
            Name: "BrokenCard",
            FullName: "Demo.Components.BrokenCard",
            SourceKind: VueComponentSourceKind.UserComponent,
            ResolutionNamespace: "Demo.Components",
            ImportSpecifier: "./components/broken-card",
            ExportName: "default",
            Props: ImmutableArray<VuePropDescriptor>.Empty,
            Emits: ImmutableArray<VueEmitDescriptor>.Empty,
            Slots: ImmutableArray<VueSlotDescriptor>.Empty,
            StyleDependencies: ImmutableArray<string>.Empty,
            PluginRequirements: ImmutableArray<string>.Empty,
            Flags: VueComponentFlags.None);

        var canonical = new RazorVueCanonicalHComponentModel(
            ComponentName: "BrokenCard",
            ComponentFullName: "Demo.Components.BrokenCard",
            RelativeComponentPath: "components/broken-card.mjs",
            Descriptor: descriptor,
            Imports: ImmutableArray<string>.Empty,
            CompilerImports: ImmutableArray<RazorVueCompilerImportBinding>.Empty,
            Styles: ImmutableArray<string>.Empty,
            PluginRequirements: ImmutableArray<string>.Empty,
            Hints: new VueRuntimeHints(true, false, true, false, false, false),
            SourceOrigins: ImmutableArray<RazorVueSourceOrigin>.Empty,
            Template: new RazorVueCanonicalTemplateFragment(
                ImmutableArray.Create<RazorVueCanonicalTemplateNode>(
                    new RazorVueCanonicalInterpolationNode(
                        ExpressionText: "someUnsupportedThing()",
                        BindingKind: RazorVueExpressionBindingKind.RuntimeExpression,
                        TemplateEncodability: RazorVueTemplateEncodability.NotTemplateEncodable,
                        SideEffectClassification: RazorVueSideEffectClassification.RepeatedEvaluationRisk,
                        SourceOrigins: ImmutableArray<RazorVueSourceOrigin>.Empty))),
            Setup: new RazorVueCanonicalSetupModel(
                ImmutableArray<VueLogicFieldDescriptor>.Empty,
                ImmutableArray<VueLogicMethodDescriptor>.Empty,
                ImmutableArray<VueLogicFieldDescriptor>.Empty,
                ImmutableArray<VueLogicMethodDescriptor>.Empty,
                new VueLifecycleDescriptor(false, false, false, false, false, false, false, false, false, false)));

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueSfcSemanticModelFactory().Create(canonical));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedTemplateEncoding, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "does not support canonical node");
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsSlotOutletArgument_ToExplicitBindingSite()
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
                [ECMAScript.ECMAScriptModule("./components/dialog-host")]
                public class DialogHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? Header { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Header, Count + 1);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var element = AssertNode<RazorVueCanonicalElementNode>(canonical.Template.Children.Single());
        var slotOutlet = AssertNode<RazorVueCanonicalSlotOutletNode>(element.Children.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("header", slotOutlet.SlotName);
        Assert.AreEqual("(props.count + 1)", slotOutlet.ArgumentExpressionText);
        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, slotOutlet.TemplateEncodability);
        Assert.AreEqual("__jazorVueSfcBinding0", sfc.ScriptSetupBlock.LiftedBindings[0].Name);
        Assert.AreEqual("(props.count + 1)", sfc.ScriptSetupBlock.LiftedBindings[0].ExpressionText);
        Assert.AreEqual("root/child[0]/child[0]/slotArg", sfc.TemplateBlock.BindingSites[0].SitePath);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsScopedSlotForwarding_ToForwardedSlotBinding()
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
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", ItemTemplate);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single();
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("itemTemplate", slot.SlotName);
        Assert.AreEqual("context", slot.ParameterName);
        Assert.AreEqual(RazorVueCanonicalSlotValueKind.ForwardedSlot, slot.ValueKind);
        Assert.AreEqual("itemTemplate", slot.ForwardedSlotName);
        Assert.IsNull(slot.ValueExpressionText);
        Assert.AreEqual(RazorVueExpressionBindingKind.SlotReference, slot.BindingKind);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_WithNonCallableScopedSlotAttribute_ThrowsSlotContextMisuse()
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
                [ECMAScript.ECMAScriptModule("./components/child")]
                public class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddAttribute(1, "ItemTemplate", "not-callable");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.SlotContextMisuse, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ItemTemplate");
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_CollectsNestedComponentImport_FromExplicitSlotTemplateChildren()
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
                [ECMAScript.ECMAScriptModule("./components/badge-chip")]
                public class BadgeChip : ComponentBase, IVueComponent
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
                        builder.AddAttribute(1, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenComponent<BadgeChip>(2);
                            itemBuilder.AddAttribute(3, "Value", item);
                            itemBuilder.CloseComponent();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        CollectionAssert.Contains(sfc.Imports.ToArray(), "./badge-chip.vue");
        CollectionAssert.Contains(sfc.Imports.ToArray(), "./list-card.vue");
        Assert.IsTrue(
            sfc.ComponentImports.Any(static import =>
                import.ComponentKey.EndsWith(".BadgeChip", StringComparison.Ordinal) &&
                string.Equals(import.ImportSpecifier, "./badge-chip.vue", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, sfc.ComponentImports.Select(static import => $"{import.ComponentKey} => {import.ImportSpecifier}")));
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_DoesNotLiftScopedSlotChildExpressions_IntoTopLevelSetupBindings()
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

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Threshold { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ListCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            if (item > Threshold)
                            {
                                itemBuilder.OpenElement(2, "strong");
                                itemBuilder.AddContent(3, item);
                                itemBuilder.CloseElement();
                            }
                            else
                            {
                                itemBuilder.OpenElement(4, "span");
                                itemBuilder.AddContent(5, item);
                                itemBuilder.CloseElement();
                            }
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page");
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static slot => slot.SlotName == "itemTemplate");
        var conditional = AssertNode<RazorVueCanonicalConditionalNode>(slot.Children.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, conditional.TemplateEncodability);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_DoesNotLiftRazorGeneratedTypedSlotSubtreeBindings_AndCollectsNestedImports()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Canonical.RazorGeneratedTypedSlotSubtree.Tests",
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");
        var conditional = AssertNode<RazorVueCanonicalConditionalNode>(slot.Children.Children.Single());
        var nestedComponent = AssertNode<RazorVueCanonicalComponentNode>(conditional.WhenTrue.Children.Single());
        var eventBinding = nestedComponent.Attributes.OfType<RazorVueCanonicalAttributeBinding>().Single(static attribute => attribute.AttributeKind == RazorVueCanonicalAttributeKind.ComponentEvent);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, conditional.TemplateEncodability);
        Assert.AreEqual(RazorVueTemplateEncodability.TemplateViaSetupBinding, eventBinding.TemplateEncodability);
        Assert.AreEqual("update:modelValue", eventBinding.Name);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
        CollectionAssert.Contains(sfc.Imports.ToArray(), "./item-editor.vue");
        CollectionAssert.Contains(sfc.Imports.ToArray(), "./list-card.vue");
    }

    private static T AssertNode<T>(object value)
        where T : class
    {
        Assert.IsInstanceOfType<T>(value);
        return (T)value;
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Canonical.Tests",
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

