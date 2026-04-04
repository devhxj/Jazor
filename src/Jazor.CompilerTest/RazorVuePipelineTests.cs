using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue.Analysis.Artifacts;
using Jazor.RazorVue.Analysis.Extensibility;
using Jazor.Razor;
using Jazor.RazorVue;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVuePipelineTests
{
    [TestMethod]
    public void RazorVue_Pipeline_ProducesFallbackRenderFunctionWhenNoRenderTreeExists()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
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
                [ECMAScript.ECMAScriptModule("./components/counter-card")]
                public class CounterCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback OnSave { get; set; }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);

        Assert.AreEqual("RazorVue.Pipeline.Tests", catalog.AssemblyName);
        Assert.HasCount(1, catalog.Artifacts);

        var artifact = catalog.Artifacts[0];
        Assert.AreEqual("CounterCard", artifact.ComponentName);
        Assert.AreEqual("components/counter-card.mjs", artifact.RelativeModulePath);
        CollectionAssert.AreEquivalent(new[] { "vue" }, artifact.Imports.ToArray());
        Assert.HasCount(1, artifact.SourceOrigins);
        Assert.IsTrue(artifact.Hints.RequiresVueRuntime);
        Assert.IsTrue(artifact.Hints.SupportsSsr);
        Assert.AreEqual(HmrBoundaryKind.Unknown, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.DescriptorHash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.TemplateHash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.LogicHash));
        StringAssert.Contains(artifact.ModuleCode, "defineComponent");
        StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h } from \"vue\";");
        StringAssert.Contains(artifact.ModuleCode, "name: \"CounterCard\"");
        StringAssert.Contains(artifact.ModuleCode, "setup(props, { emit, slots, expose, attrs })");
        StringAssert.Contains(artifact.ModuleCode, "return () => null;");
        StringAssert.Contains(artifact.ModuleCode, "\"value\"");
        StringAssert.Contains(artifact.ModuleCode, "\"update:value\"");
        StringAssert.Contains(artifact.ModuleCode, "\"save\"");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSimpleBuildRenderTreeToVueRenderFunction()
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
                public class CounterCard : VueComponent
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

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts[0];

        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", { \"data-count\": props.value }, props.title);");
        Assert.IsTrue(artifact.SourceOrigins.Length >= 4);
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.TemplateHash));
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNestedComponentToVueComponentCall()
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
                public class ChildCard : VueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        CollectionAssert.Contains(artifact.Imports.ToArray(), "vue");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "ChildCardComponent");
        StringAssert.Contains(artifact.ModuleCode, "import ChildCardComponent from \"./components/child-card.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(ChildCardComponent, null, null);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNestedComponentWithPropsAndDefaultSlot()
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
                public class ChildCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "Value", Value);
                        builder.AddContent(2, "inner");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "return () => h(ChildCardComponent, { \"value\": props.value }, { default: () => \"inner\" });");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersConditionalAndForEachRenderTreeStructures()
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
                public class ChildCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int[] Items { get; set; } = Array.Empty<int>();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Value > 0)
                        {
                            builder.OpenComponent<ChildCard>(0);
                            builder.AddAttribute(1, "Value", Value);
                            builder.CloseComponent();
                        }

                        foreach (var item in Items)
                        {
                            builder.OpenElement(2, "li");
                            builder.AddContent(3, item);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "(props.value > 0) ? h(ChildCardComponent, { \"value\": props.value }, null) : null");
        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item) => h(\"li\", null, item))");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersForEachComponentNodes()
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
                public class ChildCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
                {
                    [Parameter]
                    public int[] Items { get; set; } = Array.Empty<int>();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        foreach (var item in Items)
                        {
                            builder.OpenComponent<ChildCard>(0);
                            builder.AddAttribute(1, "Value", item);
                            builder.CloseComponent();
                        }
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item) => h(ChildCardComponent, { \"value\": item }, null))");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNestedComponentWithListenersAndNamedSlots()
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
                public class ChildCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment? Header { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "Value", Value);
                        builder.AddAttribute(2, "ValueChanged", ValueChanged);
                        builder.AddAttribute(3, "Header", ChildContent);
                        builder.AddContent(4, "inner");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "\"value\": props.value");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:value\": props.valueChanged");
        StringAssert.Contains(artifact.ModuleCode, "header: () => slots.default ? slots.default() : null");
        StringAssert.Contains(artifact.ModuleCode, "default: () => \"inner\"");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNestedComponentWithScopedSlotAttribute()
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
                public class ChildCard : VueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : VueComponent
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (context) => props.itemTemplate(context)");
    }

    [TestMethod]
    public void RazorVue_Pipeline_AllowsInjectedRazorSemanticFrontend()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule]
                public class InjectedCard : VueComponent
                {
                }
            }
            """);

        var pipeline = new RazorVuePipeline(new TestRazorSemanticFrontend());
        var catalog = pipeline.Execute(compilation);

        Assert.HasCount(1, catalog.Artifacts);
        Assert.AreEqual("InjectedCard", catalog.Artifacts[0].ComponentName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_SortsArtifactsByRelativePath()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./zeta")]
                public class ZetaCard : VueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./alpha")]
                public class AlphaCard : VueComponent
                {
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);

        Assert.HasCount(2, catalog.Artifacts);
        Assert.AreEqual("AlphaCard", catalog.Artifacts[0].ComponentName);
        Assert.AreEqual("alpha.mjs", catalog.Artifacts[0].RelativeModulePath);
        Assert.AreEqual("ZetaCard", catalog.Artifacts[1].ComponentName);
        Assert.AreEqual("zeta.mjs", catalog.Artifacts[1].RelativeModulePath);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForRenderOnlyComponents()
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
                [ECMAScript.ECMAScriptModule("./components/template-only")]
                public class TemplateOnlyCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForSafeLifecycleMethods()
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
                [ECMAScript.ECMAScriptModule("./components/logic-safe")]
                public class LogicSafeCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForRiskyLifecycleMethods()
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
                [ECMAScript.ECMAScriptModule("./components/full-reload")]
                public class FullReloadCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public void Dispose()
                    {
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_SeparatesDescriptorTemplateAndLogicHashes()
    {
        var descriptorA = new RazorVuePipeline().Execute(CreateContext(
            """
            using System;
            using Jazor.RazorVue;
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
                [ECMAScript.ECMAScriptModule("./components/hash-card")]
                public class HashCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }
            }
            """)).Artifacts.Single().Identity;

        var descriptorB = new RazorVuePipeline().Execute(CreateContext(
            """
            using System;
            using Jazor.RazorVue;
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
                [ECMAScript.ECMAScriptModule("./components/hash-card")]
                public class HashCard : VueComponent
                {
                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """)).Artifacts.Single().Identity;

        var templateA = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-template")]
                public class HashTemplateCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var templateB = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-template")]
                public class HashTemplateCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "changed");
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var logicA = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-logic")]
                public class HashLogicCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public int Calculate()
                    {
                        return Value + 1;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var logicB = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-logic")]
                public class HashLogicCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public int CalculateVariant()
                    {
                        return Value + 2;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        Assert.AreNotEqual(descriptorA.DescriptorHash, descriptorB.DescriptorHash);
        Assert.AreEqual(descriptorA.TemplateHash, descriptorB.TemplateHash);
        Assert.AreEqual(descriptorA.LogicHash, descriptorB.LogicHash);

        Assert.AreEqual(templateA.DescriptorHash, templateB.DescriptorHash);
        Assert.AreNotEqual(templateA.TemplateHash, templateB.TemplateHash);
        Assert.AreEqual(templateA.LogicHash, templateB.LogicHash);

        Assert.AreEqual(logicA.DescriptorHash, logicB.DescriptorHash);
        Assert.AreEqual(logicA.TemplateHash, logicB.TemplateHash);
        Assert.AreNotEqual(logicA.LogicHash, logicB.LogicHash);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForComponentWithNoPropsEmitsOrSlots()
    {
        // A component with no props, emits, or slots cannot be hot-reloaded safely because
        // there is no reactive contract to track changes through.
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/static-badge")]
                public class StaticBadge : VueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, "static");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNamedSlotAttribute_UsingPascalCaseAttributeName()
    {
        // Verifies that an attribute passed as PascalCase (e.g. "Header") is correctly
        // dispatched to a named slot declared as RenderFragment? Header on the child component.
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : VueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    [Parameter]
                    public RenderFragment? Footer { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : VueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        builder.AddAttribute(1, "Header", ChildContent);
                        builder.AddAttribute(2, "Footer", ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static a => a.ComponentName == "Page");
        StringAssert.Contains(artifact.ModuleCode, "header: () =>");
        StringAssert.Contains(artifact.ModuleCode, "footer: () =>");
    }

    [TestMethod]
    public void RazorVue_Pipeline_DoesNotTreatNonRenderFragmentAttributeAsSlot()
    {
        // Verifies that a non-RenderFragment attribute (int prop named "Count") is not treated
        // as a slot and instead falls through to the regular prop-binding path.
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
                [ECMAScript.ECMAScriptModule("./components/counter")]
                public class Counter : VueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Counter>(0);
                        builder.AddAttribute(1, "Count", Value);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static a => a.ComponentName == "Host");
        // Count is a plain int prop, should emit as a prop binding, not a slot
        StringAssert.Contains(artifact.ModuleCode, "\"count\": props.value");
        // No slot emission for Count
        Assert.IsFalse(artifact.ModuleCode.Contains("count: ()"), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForComponentWithNoPropsEmitsOrSlotsEvenWithStaticTemplate()
    {
        // A component with zero props/emits/slots still has no reactive contract even when its
        // BuildRenderTree body is static, so the HMR boundary remains FullReloadRequired.
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/pure-static")]
                public class PureStaticBadge : VueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, "hello");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        // No props, no emits, no slots, but has a static template body.
        // Expected: FullReloadRequired because with no reactive contract there is no safe
        // incremental reload boundary — the whole component must be replaced.
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
        // Verify the static template is present in the output so the test is not vacuous.
        StringAssert.Contains(artifact.ModuleCode, "\"span\"");
        StringAssert.Contains(artifact.ModuleCode, "\"hello\"");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNamedSlotAttribute_MultiWordPascalCaseBecomesLowerCamelCaseInOutput()
    {
        // Verifies that a multi-word PascalCase slot name (e.g. "HeaderContent") is
        // lowered to lowerCamelCase ("headerContent") in the Vue slots object, not kebab-case.
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
                [ECMAScript.ECMAScriptModule("./components/card")]
                public class Card : VueComponent
                {
                    [Parameter]
                    public RenderFragment? HeaderContent { get; set; }

                    [Parameter]
                    public RenderFragment? FooterActions { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : VueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Card>(0);
                        builder.AddAttribute(1, "HeaderContent", ChildContent);
                        builder.AddAttribute(2, "FooterActions", ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static a => a.ComponentName == "Page");
        // Multi-word PascalCase slot names must be lowercamelCase in the Vue slots object.
        StringAssert.Contains(artifact.ModuleCode, "headerContent: () =>");
        StringAssert.Contains(artifact.ModuleCode, "footerActions: () =>");
        // Must NOT appear as the raw PascalCase or as kebab-case.
        Assert.IsFalse(artifact.ModuleCode.Contains("\"HeaderContent\":"), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"header-content\":"), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_DoesNotLowerNonCallableScopedSlotAttributeAsInvokedSlot()
    {
        // Negative test: when a scoped-slot parameter (RenderFragment<T>) on the child
        // receives a non-callable constant value (a string literal instead of a
        // RenderFragment<T>-typed expression), the lowering must NOT produce an
        // `(context) => expr(context)` invocation — it should either emit a passthrough
        // prop binding or skip slot emission rather than generating broken JavaScript.
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
                public class Child : VueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : VueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        // Pass a plain string constant — not a RenderFragment<int> value.
                        builder.AddAttribute(1, "ItemTemplate", "not-callable");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static a => a.ComponentName == "Host");
        // The lowering must NOT emit a scoped slot invocation pattern for a non-callable value.
        Assert.IsFalse(
            artifact.ModuleCode.Contains("(context) => \"not-callable\"(context)"),
            $"Non-callable value was incorrectly lowered as an invoked scoped slot.\n{artifact.ModuleCode}");
        Assert.IsFalse(
            artifact.ModuleCode.Contains("(context) => props.itemTemplate(context)"),
            $"Non-callable constant was incorrectly treated as a prop reference.\n{artifact.ModuleCode}");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsForComponentFieldUsedInTemplateExpression()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/field-card")]
                public class FieldCard : VueComponent
                {
                    private int _count = 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, _count);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<NotSupportedException>(() => new RazorVuePipeline().Execute(context));
        StringAssert.Contains(exception.Message, "does not support component field '_count'");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsForComponentMethodCalledInTemplateExpression()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/method-card")]
                public class MethodCard : VueComponent
                {
                    private int Calculate()
                    {
                        return 42;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, Calculate());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<NotSupportedException>(() => new RazorVuePipeline().Execute(context));
        StringAssert.Contains(exception.Message, "does not support calling component method 'Calculate'");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForComponentWithDisposeAsyncLifecycle()
    {
        // DisposeAsync is a risky lifecycle method (resource cleanup), so even with a valid
        // render tree and props the boundary must be FullReloadRequired.
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/disposable-async")]
                public class DisposableAsyncCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public ValueTask DisposeAsync()
                    {
                        return default;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForComponentWithOnlyLogicMethods()
    {
        // A component with props and user-defined logic methods (but no lifecycle hooks)
        // should be classified as LogicSafe because the logic methods influence behavior.
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
                [ECMAScript.ECMAScriptModule("./components/logic-card")]
                public class LogicCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public int Calculate()
                    {
                        return Value * 2;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForMultipleSafeLifecycleHooks()
    {
        // A component with multiple safe lifecycle hooks (OnInitialized + OnAfterRender)
        // and a template should still be LogicSafe, not FullReloadRequired.
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
                [ECMAScript.ECMAScriptModule("./components/multi-lifecycle")]
                public class MultiLifecycleCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void OnInitialized()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryWhenDisposeCoexistsWithSafeLifecycle()
    {
        // Even when a safe lifecycle hook (OnParametersSet) is present, the presence of
        // Dispose should dominate and force FullReloadRequired.
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
                [ECMAScript.ECMAScriptModule("./components/dispose-safe-mix")]
                public class DisposeSafeMixCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void OnParametersSet()
                    {
                    }

                    public void Dispose()
                    {
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenLogicMethodSignatureChanges()
    {
        // Verify that adding/changing user logic methods changes the LogicHash
        // but does not affect DescriptorHash or TemplateHash.
        var identityA = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-logic-sig")]
                public class HashLogicSigCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public int Foo()
                    {
                        return 1;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var identityB = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/hash-logic-sig")]
                public class HashLogicSigCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public int Foo()
                    {
                        return 1;
                    }

                    public int Bar(int x)
                    {
                        return x;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        Assert.AreEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.TemplateHash, identityB.TemplateHash);
        Assert.AreNotEqual(identityA.LogicHash, identityB.LogicHash);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForAsyncSafeLifecycleMethods()
    {
        // Async safe lifecycle hooks should be classified the same as their sync variants.
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/async-safe-lifecycle")]
                public class AsyncSafeLifecycleCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForOnParametersSetAsyncLifecycle()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/async-parameters-set")]
                public class AsyncParametersSetCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForShouldRenderLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/should-render")]
                public class ShouldRenderCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        return true;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForSetParametersAsyncLifecycle()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/set-parameters-async")]
                public class SetParametersAsyncCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public override Task SetParametersAsync(ParameterView parameters)
                    {
                        return base.SetParametersAsync(parameters);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryWhenShouldRenderCoexistsWithSafeLifecycle()
    {
        // ShouldRender is a risky lifecycle override. Even when safe lifecycle hooks
        // (OnInitialized) are also present, risky should dominate and force FullReloadRequired.
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
                [ECMAScript.ECMAScriptModule("./components/should-render-with-safe")]
                public class ShouldRenderWithSafeCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void OnInitialized()
                    {
                    }

                    protected override bool ShouldRender()
                    {
                        return Value > 0;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryWhenSetParametersAsyncCoexistsWithSafeLifecycle()
    {
        // SetParametersAsync is a risky lifecycle override. Even when safe lifecycle hooks
        // (OnParametersSet) are also present, risky should dominate and force FullReloadRequired.
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/set-params-async-with-safe")]
                public class SetParamsAsyncWithSafeCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void OnParametersSet()
                    {
                    }

                    public override Task SetParametersAsync(ParameterView parameters)
                    {
                        return base.SetParametersAsync(parameters);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesUnknownBoundaryForComponentWithPropsButNoTemplateBody()
    {
        // When a component has a reactive contract (props) but produces no render nodes,
        // it bypasses the zero-contract FullReload guard yet still has no template/logical boundary.
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
                [ECMAScript.ECMAScriptModule("./components/props-no-template")]
                public class PropsNoTemplateCard : VueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.Unknown, artifact.Identity.HmrBoundaryKind);
    }

    private sealed class TestRazorSemanticFrontend : IRazorSemanticFrontend
    {
        public string Name => "Jazor.CompilerTest.TestRazorSemanticFrontend";

        public bool CanHandle(Compilation compilation)
            => RazorVueCompilationContext.TryCreate(compilation) is not null;

        public RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol)
            => GetRequiredContext(compilation).ClassifyEntry(symbol);

        public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation)
            => GetRequiredContext(compilation).CreateSemanticSnapshots();

        private static RazorVueCompilationContext GetRequiredContext(Compilation compilation)
            => RazorVueCompilationContext.TryCreate(compilation)
               ?? throw new InvalidOperationException("The test frontend expected a valid RazorVue compilation context.");
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CreateCompilation(source);

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create(
            assemblyName: "RazorVue.Pipeline.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: CreateReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static IEnumerable<MetadataReference> CreateReferences()
    {
        var references = Net100.References.All.Cast<MetadataReference>().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));
        return references;
    }
}

