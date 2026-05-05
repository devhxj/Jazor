using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue.Sfc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueCanonicalSfcSemanticTests
{
    [TestMethod]
    public void RazorVue_CanonicalModelFactory_CreatesTemplateAndSfcSemanticModel_ForSimpleElementComponent()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
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
        var canonical = new RazorVueCanonicalHModelFactory().Create(context, snapshot);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("CounterCard", canonical.ComponentName);
        Assert.AreEqual("components/counter-card.mjs", canonical.RelativeComponentPath);
        Assert.HasCount(1, canonical.Template.Children);
        Assert.IsInstanceOfType<RazorVueCanonicalElementNode>(canonical.Template.Children[0]);

        var root = (RazorVueCanonicalElementNode)canonical.Template.Children[0];
        Assert.AreEqual("section", root.TagName);
        Assert.HasCount(1, root.Attributes);
        Assert.AreEqual("data-count", root.Attributes[0].Name);
        Assert.AreEqual("props.value", root.Attributes[0].ExpressionText);
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
            using Jazor.RazorVue;
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
        var canonical = new RazorVueCanonicalHModelFactory().Create(context, snapshot);
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
            using Jazor.RazorVue;
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
        var canonical = new RazorVueCanonicalHModelFactory().Create(context, snapshot);
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
            using Jazor.RazorVue;
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
        var canonical = new RazorVueCanonicalHModelFactory().Create(context, snapshot);
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
            using Jazor.RazorVue;
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
            new RazorVueCanonicalHModelFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.SlotContextMisuse, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ItemTemplate");
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
