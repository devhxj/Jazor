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
    public void RazorVue_CanonicalModelFactory_LiftsNestedPropertyProjectionFromInvocation_WhenSingleEvaluationMustBePreserved()
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
                [ECMAScript.ECMAScriptModule("./components/title-card")]
                public class TitleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    private string GetTitle()
                        => Title ?? string.Empty;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, GetTitle().Length);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var root = AssertNode<RazorVueCanonicalElementNode>(canonical.Template.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(root.Children.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        StringAssert.Contains(interpolation.ExpressionText, "getTitle", StringComparison.Ordinal);
        Assert.AreEqual(RazorVueTemplateEncodability.DirectTemplate, interpolation.TemplateEncodability);
        Assert.AreEqual(RazorVueTemplateExpressionSafety.RequiresSetupBinding, interpolation.TemplateExpressionSafety);
        Assert.AreEqual(RazorVueSideEffectClassification.RepeatedEvaluationRisk, interpolation.SideEffectClassification);
        Assert.AreEqual("__jazor$0", sfc.ScriptSetupBlock.LiftedBindings.Single().Name);
        Assert.AreEqual(interpolation.ExpressionText, sfc.ScriptSetupBlock.LiftedBindings.Single().ExpressionText);
        Assert.AreEqual("__jazor$0", sfc.TemplateBlock.BindingSites.Single().TemplateExpressionText);
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
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.AreEqual("root/child[0]/if", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("props.visible", sfc.TemplateBlock.BindingSites[0].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/whenTrue/child[0]/forEach", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("props.items", sfc.TemplateBlock.BindingSites[1].TemplateExpressionText);
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
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.AreEqual("root/child[0]/for/init", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("0", sfc.TemplateBlock.BindingSites[0].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/for/limit", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("props.count", sfc.TemplateBlock.BindingSites[1].TemplateExpressionText);
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
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.AreEqual("root/child[0]/for/init", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("props.start", sfc.TemplateBlock.BindingSites[0].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/for/limit", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("props.count", sfc.TemplateBlock.BindingSites[1].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/for/step", sfc.TemplateBlock.BindingSites[2].SitePath);
        Assert.AreEqual("props.step", sfc.TemplateBlock.BindingSites[2].TemplateExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_ClassifiesCountStyleForWithSimpleAssignmentStep_AsTemplateViaSetupBinding()
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
                        for (var i = Start; i <= Count; i = i + Step)
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
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.AreEqual("root/child[0]/for/init", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("props.start", sfc.TemplateBlock.BindingSites[0].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/for/limit", sfc.TemplateBlock.BindingSites[1].SitePath);
        Assert.AreEqual("props.count", sfc.TemplateBlock.BindingSites[1].TemplateExpressionText);
        Assert.AreEqual("root/child[0]/for/step", sfc.TemplateBlock.BindingSites[2].SitePath);
        Assert.AreEqual("props.step", sfc.TemplateBlock.BindingSites[2].TemplateExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsVNodeKeys_ForElementAndComponent()
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
                        builder.OpenElement(0, "section");
                        builder.SetKey("root");
                        builder.OpenComponent<ChildCard>(1);
                        builder.SetKey(Id);
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var root = AssertNode<RazorVueCanonicalElementNode>(canonical.Template.Children.Single());
        var child = AssertNode<RazorVueCanonicalComponentNode>(root.Children.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.IsNotNull(root.Key);
        Assert.AreEqual("\"root\"", root.Key.ExpressionText);
        Assert.AreEqual(RazorVueExpressionBindingKind.Literal, root.Key.BindingKind);
        Assert.AreEqual(RazorVueTemplateEncodability.DirectTemplate, root.Key.TemplateEncodability);

        Assert.IsNotNull(child.Key);
        Assert.AreEqual("props.id", child.Key.ExpressionText);
        Assert.AreEqual(RazorVueTemplateEncodability.DirectTemplate, child.Key.TemplateEncodability);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
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
            ContainerContractFullName: null,
            RouteTemplates: ImmutableArray<string>.Empty,
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
                        TemplateExpressionSafety: RazorVueTemplateExpressionSafety.NotTemplateSafe,
                        SideEffectClassification: RazorVueSideEffectClassification.RepeatedEvaluationRisk,
                        SourceOrigins: ImmutableArray<RazorVueSourceOrigin>.Empty))),
            ImperativeRootProgram: null,
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
    public void RazorVue_CanonicalModelFactory_CreatesImperativeRootProgram_ForConditionalReturnBuildRenderTree()
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
                    public bool Hide { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        Assert.IsTrue(canonical.ImperativeRootProgram is not null);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, canonical.ImperativeRootProgram.Kind);
        Assert.IsTrue(canonical.Template.Children.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_CreatesRenderFunctionSemanticModel_ForImperativeRootProgram()
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
                    public bool Hide { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, sfc.RenderMode);
        Assert.IsTrue(sfc.ImperativeRootProgram is not null);
        Assert.IsTrue(sfc.TemplateBlock.Template.Children.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_SfcSemanticModelFactory_PreservesTemplateMode_ForDeclarativeCanonicalModel()
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, sfc.RenderMode);
        Assert.IsTrue(sfc.ImperativeRootProgram is null);
        Assert.IsFalse(sfc.TemplateBlock.Template.Children.IsDefaultOrEmpty);
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
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.AreEqual("root/child[0]/child[0]/slotArg", sfc.TemplateBlock.BindingSites[0].SitePath);
        Assert.AreEqual("(props.count + 1)", sfc.TemplateBlock.BindingSites[0].TemplateExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentDefaultSlotForwarding_ToNamedChildSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "footer");
        var slotOutlet = AssertNode<RazorVueCanonicalSlotOutletNode>(slot.Children.Children.Single());
        Assert.AreEqual("default", slotOutlet.SlotName);
        Assert.IsNull(slotOutlet.ArgumentExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsTypedAddContentRenderFragment_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsTypedAddContentRenderFragmentLocalCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsUnwrittenSettableCurrentComponentRenderFragmentPropertyCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_WithNonPrivateSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsAnalyzableCurrentComponentRenderFragmentPropertyCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsAnalyzableCurrentComponentRenderFragmentAutoPropertyCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsAnalyzableCurrentComponentRenderFragmentFieldCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsChainedCurrentComponentRenderFragmentPropertyCarrier_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_WithSelfReferentialCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "Template");
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_WithCyclicCurrentComponentRenderFragmentPropertyCarriers_ThrowsCanonicalizationFailed()
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "TemplateA");
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsZeroArgumentCurrentComponentRenderFragmentFactoryMethod_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
        var span = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsZeroArgumentLocalRenderFragmentFactoryMethod_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());

        Assert.AreEqual("item", scope.ScopeName);
        Assert.AreEqual("42", scope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsZeroArgumentCurrentComponentRenderFragmentFactoryMethod_ToStructuredTypedSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");

        Assert.AreEqual("item", slot.ParameterName);
        var paragraph = AssertNode<RazorVueCanonicalElementNode>(slot.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(paragraph.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsZeroArgumentLocalRenderFragmentFactoryMethod_ToStructuredTypedSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");

        Assert.AreEqual("item", slot.ParameterName);
        var paragraph = AssertNode<RazorVueCanonicalElementNode>(slot.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethod_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);

        var span = AssertNode<RazorVueCanonicalElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var titleInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[0]);
        var itemInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[1]);
        Assert.AreEqual("title", titleInterpolation.ExpressionText);
        Assert.AreEqual("item", itemInterpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsGenericParameterizedCurrentComponentRenderFragmentFactoryMethod_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);

        var span = AssertNode<RazorVueCanonicalElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var titleInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[0]);
        var itemInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[1]);
        Assert.AreEqual("title", titleInterpolation.ExpressionText);
        Assert.AreEqual("item", itemInterpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethodWithOmittedOptionalParameter_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("\"fallback-title\"", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethodWithParamsParameter_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var valuesScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("values", valuesScope.ScopeName);
        Assert.AreEqual("[props.title, \"suffix\"]", valuesScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(valuesScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);

        var span = AssertNode<RazorVueCanonicalElementNode>(itemScope.Children.Children.Single());
        var lengthInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[0]);
        var itemInterpolation = AssertNode<RazorVueCanonicalInterpolationNode>(span.Children.Children[1]);
        Assert.AreEqual("values.length", lengthInterpolation.ExpressionText);
        Assert.AreEqual("item", itemInterpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethodUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplate_ToStructuredTypedSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");
        Assert.AreEqual("item", slot.ParameterName);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(slot.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var span = AssertNode<RazorVueCanonicalElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplateUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");
        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(slot.Children.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedLocalRenderFragmentCarrierInitializedFromFactoryMethod_PreservingCapturedScopeOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedLocalRenderFragmentCarrierAssignedImmediatelyFromFactoryMethod_PreservingCapturedScopeOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethod_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var itemScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        Assert.AreEqual("42", itemScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethodForTypedSlotTemplate_ToStructuredTypedSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");
        Assert.AreEqual("item", slot.ParameterName);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(slot.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_WithCyclicCurrentComponentRenderFragmentFactoryMethods_ThrowsCanonicalizationFailed()
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursive");
        StringAssert.Contains(exception.Issue.Message, "CreateTemplateA");
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithExtraParameters_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("title", scope.ScopeName);
        Assert.AreEqual("props.title", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("title", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsGenericCurrentComponentRenderHelperMethodWithExtraParameters_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("title", scope.ScopeName);
        Assert.AreEqual("props.title", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("title", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsBuildRenderTreeLocalFunctionHelperWithExtraParameters_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("title", scope.ScopeName);
        Assert.AreEqual("props.title", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("title", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsBuildRenderTreeLocalFunctionHelperWithOmittedOptionalParameter_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("title", scope.ScopeName);
        Assert.AreEqual("\"fallback-title\"", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("title", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithParamsParameter_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("values", scope.ScopeName);
        Assert.AreEqual("[props.title, \"suffix\"]", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("values.length", interpolation.ExpressionText);
        Assert.AreEqual("__jazor$0", sfc.ScriptSetupBlock.LiftedBindings.Single().Name);
        Assert.AreEqual(scope.InitializerExpressionText, sfc.ScriptSetupBlock.LiftedBindings.Single().ExpressionText);
        Assert.AreEqual("root/child[0]/scopeInit", sfc.TemplateBlock.BindingSites.Single().SitePath);
        Assert.AreEqual("__jazor$0", sfc.TemplateBlock.BindingSites.Single().TemplateExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithMultipleExtraParameters_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(subtitleScope.Children.Children.Single());
        Assert.HasCount(2, section.Attributes);
        var titleAttribute = AssertNode<RazorVueCanonicalAttributeBinding>(section.Attributes[0]);
        var subtitleAttribute = AssertNode<RazorVueCanonicalAttributeBinding>(section.Attributes[1]);
        Assert.AreEqual("data-title", titleAttribute.Name);
        Assert.AreEqual("title", titleAttribute.ExpressionText);
        Assert.AreEqual("data-subtitle", subtitleAttribute.Name);
        Assert.AreEqual("subtitle", subtitleAttribute.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsBuildRenderTreeLocalFunctionHelperWithMultipleExtraParameters_ToNestedTemplateScopeNodes()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(subtitleScope.Children.Children.Single());
        Assert.HasCount(2, section.Attributes);
        var titleAttribute = AssertNode<RazorVueCanonicalAttributeBinding>(section.Attributes[0]);
        var subtitleAttribute = AssertNode<RazorVueCanonicalAttributeBinding>(section.Attributes[1]);
        Assert.AreEqual("data-title", titleAttribute.Name);
        Assert.AreEqual("title", titleAttribute.ExpressionText);
        Assert.AreEqual("data-subtitle", subtitleAttribute.Name);
        Assert.AreEqual("subtitle", subtitleAttribute.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsBuildRenderTreeLocalFunctionHelperWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var subtitleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.AreEqual("props.subtitle", subtitleScope.InitializerExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithExtraParameterBackedTemplateLocal_ToScopeThenLocalDeclaration()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var localDeclaration = AssertNode<RazorVueCanonicalLocalDeclarationNode>(titleScope.Children.Children[0]);
        Assert.AreEqual("localTitle", localDeclaration.LocalName);
        Assert.AreEqual("title", localDeclaration.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(titleScope.Children.Children[1]);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("localTitle", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsBuildRenderTreeLocalFunctionHelperWithExtraParameterBackedTemplateLocal_ToScopeThenLocalDeclaration()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("props.title", titleScope.InitializerExpressionText);

        var localDeclaration = AssertNode<RazorVueCanonicalLocalDeclarationNode>(titleScope.Children.Children[0]);
        Assert.AreEqual("localTitle", localDeclaration.LocalName);
        Assert.AreEqual("title", localDeclaration.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(titleScope.Children.Children[1]);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("localTitle", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsLoopInvokedCurrentComponentRenderHelperMethodWithExtraParameterBackedTemplateLocal_ToLoopThenScopeThenLocalDeclaration()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var loop = AssertNode<RazorVueCanonicalForEachNode>(canonical.Template.Children.Single());
        Assert.AreEqual("item", loop.ItemName);
        Assert.AreEqual("props.items", loop.SourceExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(loop.Body.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("item", titleScope.InitializerExpressionText);

        var localDeclaration = AssertNode<RazorVueCanonicalLocalDeclarationNode>(titleScope.Children.Children[0]);
        Assert.AreEqual("localTitle", localDeclaration.LocalName);
        Assert.AreEqual("title", localDeclaration.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(titleScope.Children.Children[1]);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("localTitle", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsLoopInvokedBuildRenderTreeLocalFunctionHelperWithExtraParameterBackedTemplateLocal_ToLoopThenScopeThenLocalDeclaration()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);

        var loop = AssertNode<RazorVueCanonicalForEachNode>(canonical.Template.Children.Single());
        Assert.AreEqual("item", loop.ItemName);
        Assert.AreEqual("props.items", loop.SourceExpressionText);

        var titleScope = AssertNode<RazorVueCanonicalTemplateScopeNode>(loop.Body.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.AreEqual("item", titleScope.InitializerExpressionText);

        var localDeclaration = AssertNode<RazorVueCanonicalLocalDeclarationNode>(titleScope.Children.Children[0]);
        Assert.AreEqual("localTitle", localDeclaration.LocalName);
        Assert.AreEqual("title", localDeclaration.InitializerExpressionText);

        var section = AssertNode<RazorVueCanonicalElementNode>(titleScope.Children.Children[1]);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("localTitle", interpolation.ExpressionText);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperMethodWithOmittedOptionalParameter_ToTemplateScopeNode()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var scope = AssertNode<RazorVueCanonicalTemplateScopeNode>(canonical.Template.Children.Single());
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("title", scope.ScopeName);
        Assert.AreEqual("\"fallback-title\"", scope.InitializerExpressionText);
        var section = AssertNode<RazorVueCanonicalElementNode>(scope.Children.Children.Single());
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(section.Children.Children.Single());
        Assert.AreEqual("title", interpolation.ExpressionText);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_CanonicalModelFactory_MapsStringEnumComponentProp_ToDirectStaticAttribute()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/card-host")]
                public class CardHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VCard>(0);
                        builder.AddAttribute(1, nameof(VCard.Variant), VuetifyVariant.Outlined);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var attribute = component.Attributes.OfType<RazorVueCanonicalAttributeBinding>().Single();
        var sfc = new RazorVueSfcSemanticModelFactory().Create(canonical);

        Assert.AreEqual("variant", attribute.Name);
        Assert.AreEqual("\"outlined\"", attribute.ExpressionText);
        Assert.AreEqual(RazorVueExpressionBindingKind.Literal, attribute.BindingKind);
        Assert.AreEqual(RazorVueLiteralValueKind.String, attribute.LiteralValueKind);
        Assert.AreEqual(RazorVueTemplateEncodability.DirectTemplate, attribute.TemplateEncodability);
        Assert.AreEqual(RazorVueTemplateExpressionSafety.DirectTemplateSafe, attribute.TemplateExpressionSafety);
        Assert.AreEqual(RazorVueSideEffectClassification.None, attribute.SideEffectClassification);
        Assert.IsTrue(sfc.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty);
        Assert.IsTrue(sfc.TemplateBlock.BindingSites.IsDefaultOrEmpty);
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
    public void RazorVue_CanonicalModelFactory_MapsLocalCarrierTypedSlotTemplate_ToStructuredSlot()
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
        var canonical = CreateBuildRenderTreeCanonicalFactory().Create(context, snapshot);
        var component = AssertNode<RazorVueCanonicalComponentNode>(canonical.Template.Children.Single());
        var slot = component.Slots.Single(static candidate => candidate.SlotName == "itemTemplate");

        Assert.AreEqual("item", slot.ParameterName);
        var paragraph = AssertNode<RazorVueCanonicalElementNode>(slot.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        var interpolation = AssertNode<RazorVueCanonicalInterpolationNode>(paragraph.Children.Children.Single());
        Assert.AreEqual("item", interpolation.ExpressionText);
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

