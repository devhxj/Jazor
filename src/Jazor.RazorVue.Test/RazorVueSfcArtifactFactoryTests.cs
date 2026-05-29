using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.Json;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueSfcArtifactFactoryTests
{
    private static RazorVueSfcArtifactFactory CreateBuildRenderTreeArtifactFactory()
        => new(BuildRenderTreeTemplateFrontend.Instance);

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersSimpleComponent_ToVueSfcArtifact()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("CounterCard", artifact.ComponentName);
        Assert.AreEqual("components/counter-card.vue", artifact.RelativeSfcPath);
        StringAssert.Contains(artifact.SfcText, "<template>");
        StringAssert.Contains(artifact.SfcText, "<section :data-count=\"props.value\">");
        StringAssert.Contains(artifact.SfcText, "{{ props.title }}");
        StringAssert.Contains(artifact.SfcText, "<script setup lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "const __jazorRawProps = defineProps<");
        StringAssert.Contains(artifact.SfcText, "const props = __jazorRawProps;");
        StringAssert.Contains(artifact.SfcText, "const emit = defineEmits<");
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.DescriptorHash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.TemplateHash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.LogicHash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(artifact.Identity.StyleHash));
        Assert.AreEqual("ts", artifact.ScriptSetupBlock.Language);
        Assert.IsTrue(artifact.StyleBlocks.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersVNodeKeys_ToTemplateKeyBindings()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section :key=\"&quot;root&quot;\">");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent :key=\"props.id\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersElementDomEventWithModifiers_ToVueTemplateEvent()
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<button @click.prevent.stop=\"__jazor$0\" />");
        Assert.IsFalse(artifact.TemplateText.Contains(":onclick=", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersElementDomEventWithDynamicModifier_ToSetupBindingEvent()
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
                    public bool PreventClick { get; set; }

                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", PreventClick);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<button @click=\"(__event) =&gt; { if (props.preventClick) __event?.preventDefault?.(); return (__jazor$0)(__event); }\" />");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => () => emit(\"click\"));");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("computed(() => props.preventClick)", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazorPreventDefault", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersElementDomEventWithDynamicModifierInTemplateScope_ToInlineScopedHandler()
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
                    public bool PreventClick { get; set; }

                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        var localPrevent = PreventClick;
                        builder.OpenElement(1, "button");
                        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 3, "onclick", localPrevent);
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localPrevent) in [props.preventClick]\">");
        StringAssert.Contains(artifact.TemplateText, "<button @click=\"(__event) =&gt; { if (localPrevent) __event?.preventDefault?.(); return (() =&gt; emit(&quot;click&quot;))(__event); }\" />");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazorPreventDefault", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithStringOnAttribute_EmitsPlainHtmlAttribute()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", "return false");
                        builder.AddContent(2, "Go");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<button onclick=\"return false\">");
        StringAssert.Contains(artifact.TemplateText, "Go");
        Assert.IsFalse(artifact.TemplateText.Contains("@click", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains(":onclick", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithComponentEventModifier_ThrowsStructuralIssue()
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
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, nameof(ChildCard.OnClick), EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", true);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "Event modifiers are only supported on HTML element frames.");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterInitializerDefaults_ThroughRuntimeProxy()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/defaults-card")]
                public class DefaultsCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; } = CreateDefaultTitle();

                    [Parameter]
                    public string[]? Items { get; set; } = CreateDefaultItems();

                    private static string CreateDefaultTitle() => "Overview";

                    private static string[] CreateDefaultItems() => ["alpha", "beta"];

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.AddContent(2, Items?.Length ?? 0);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = new RazorVueSfcArtifactFactory(new FixedTemplateFrontend(CreateInjectedSectionTree("Defaults")))
            .Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorRawProps = defineProps<{ items?: any; title?: any }>();");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("const props = __jazorRawProps;\nconst props = new Proxy(", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.ScriptSetupText, "const props = new Proxy(__jazorRawProps, {");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_IgnoresNullForgivingParameterInitializer_AsRuntimeDefaultSource()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/required-card")]
                public class RequiredCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = default!;

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
        var artifact = new RazorVueSfcArtifactFactory(new FixedTemplateFrontend(CreateInjectedSectionTree("Required")))
            .Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const props = __jazorRawProps;");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("new Proxy(__jazorRawProps", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("default!;", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersDeclarationPatternFirstRenderLifecyclePayload_ThroughCompilerFallback()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is bool ready && ready);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "let ready;");
        StringAssert.Contains(artifact.ScriptSetupText, "typeof currentFirstRender === \"boolean\"");
        StringAssert.Contains(artifact.ScriptSetupText, "(ready = currentFirstRender, true)");
        StringAssert.Contains(artifact.ScriptSetupText, "&& ready");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", ((() => {");
        StringAssert.Contains(artifact.SfcText, "await emit(\"readyChanged\", ((() => {");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersArrayPatternFirstRenderLifecyclePayload_ThroughCompilerFallback()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var readyStates = new[] { false, firstRender };
                        var payload = readyStates is [_, var ready] ? ready : false;
                        return ReadyChanged.InvokeAsync(payload);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorLifecycleLocal");
        StringAssert.Contains(artifact.ScriptSetupText, "Array.isArray(__jazorLifecycleLocal");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", __jazorLifecycleLocal");
        StringAssert.Contains(artifact.SfcText, "await emit(\"readyChanged\", __jazorLifecycleLocal");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersElementSplat_ToOrderedVBind()
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
                    public string? Title { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "title", Title);
                        builder.AddAttribute(2, "class", "left");
                        builder.AddMultipleAttributes(3, AdditionalAttributes);
                        builder.AddAttribute(4, "class", "right");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "function __jazorVueMergeAttributes(...sources) {");
        StringAssert.Contains(
            artifact.TemplateText,
            "<section v-bind=\"__jazorVueMergeAttributes({ &quot;title&quot;: props.title, &quot;class&quot;: &quot;left&quot; }, props.additionalAttributes, { &quot;class&quot;: &quot;right&quot; })\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersMergedElementDomEventWithDynamicModifier_ToWrappedEventProp()
    {
        var context = CreateContext(
            """
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool PreventClick { get; set; }

                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, OnClick));
                        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 2, "onclick", PreventClick);
                        builder.AddMultipleAttributes(3, AdditionalAttributes);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "function __jazorVueMergeAttributes(...sources) {");
        StringAssert.Contains(artifact.TemplateText, "&quot;onClick&quot;: (__event) =&gt; { if (props.preventClick) __event?.preventDefault?.(); return (__jazor$0)(__event); }");
        StringAssert.Contains(artifact.TemplateText, "props.additionalAttributes");
        Assert.IsFalse(artifact.TemplateText.Contains("&quot;onclick&quot;", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersVuetifyCardTitle_DefaultSlot()
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
                [ECMAScript.ECMAScriptModule("./components/dashboard-card")]
                public class DashboardCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VCard>(0);
                        builder.OpenComponent<VCardTitle>(1);
                        builder.AddAttribute(2, nameof(VCardTitle.ChildContent), (RenderFragment)((titleBuilder) =>
                        {
                            titleBuilder.AddContent(3, "Dashboard");
                        }));
                        builder.CloseComponent();
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<VCard>");
        StringAssert.Contains(artifact.TemplateText, "<VCardTitle>");
        StringAssert.Contains(artifact.TemplateText, "Dashboard");
        Assert.IsFalse(artifact.TemplateText.Contains("text=\"Dashboard\"", StringComparison.Ordinal), artifact.TemplateText);
        CollectionAssert.Contains(artifact.Styles.ToArray(), "vuetify/styles");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_CanUseInjectedTemplateFrontend_WhenBuildRenderTreeIsAbsent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
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
                [ECMAScript.ECMAScriptModule("./components/injected-card")]
                public class InjectedCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = new RazorVueSfcArtifactFactory(new FixedTemplateFrontend(CreateInjectedSectionTree("Injected SFC")))
            .Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "Injected SFC");
        Assert.IsFalse(artifact.SfcText.Contains("return () => null;", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_CanUseInjectedTemplateFrontend_WhenBuildRenderTreeIsAbsent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
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
                [ECMAScript.ECMAScriptModule("./components/injected-pipeline-card")]
                public class InjectedPipelineCard : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var frontend = new FixedTemplateFrontend(CreateInjectedSectionTree("Injected pipeline SFC"));
        var artifact = new RazorVueSfcPipeline(frontend).Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.TemplateText, "Injected pipeline SFC");
        Assert.AreEqual("InjectedPipelineCard", artifact.ComponentName);
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_DefaultConstructor_UsesDocumentAwareSemanticFrontend()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string razorDocumentText = """
            @page "/todo"
            <section>Hello from default SFC pipeline</section>
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcPipeline.DefaultConstructor.RazorDocument.Tests",
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

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                        }
                    }
                    """,
                    path: "TodoApp.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    $$"""
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Pages
                    {
                        public partial class TodoApp
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                    #line 1 "{{documentPath}}"
                                __builder.AddContent(0, "Hello from generated render tree");
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var context = CreateContext(
            InjectCarrierCompilation(
                compilation,
                documentPath.Replace('\\', '/'),
                razorDocumentText));

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance).Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.SfcText, "Hello from generated render tree");
        Assert.IsFalse(artifact.SfcText.Contains("@page \"/todo\"", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("Hello from default SFC pipeline", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_InjectedRuntimeShapeChangesAffectDescriptorHash()
    {
        var identityA = CreateInjectedContainerHomePageSfcIdentity(
            contractMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }

            [Parameter]
            public RenderFragment? Header { get; set; }
            """,
            implementationAttributes:
            """
            [VueProp(nameof(Title), Name = "menuTitle")]
            [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
            [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "System.String?")]
            [VueSlot(nameof(Header), Name = "header")]
            """,
            implementationMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }

            [Parameter]
            public RenderFragment? Header { get; set; }
            """,
            renderStatements:
            """
            builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
            builder.AddComponentParameter(2, nameof(Demo.Containers.NavShell.Value), Value);
            builder.AddComponentParameter(3, nameof(Demo.Containers.NavShell.ValueChanged), ValueChanged);
            builder.AddComponentParameter(4, nameof(Demo.Containers.NavShell.Header), (RenderFragment)((slotBuilder) =>
            {
                slotBuilder.OpenElement(5, "strong");
                slotBuilder.AddContent(6, "Overview");
                slotBuilder.CloseElement();
            }));
            """,
            pageMembers:
            """
            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }
            """);

        var identityB = CreateInjectedContainerHomePageSfcIdentity(
            contractMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }

            [Parameter]
            public RenderFragment? Header { get; set; }
            """,
            implementationAttributes:
            """
            [VueProp(nameof(Title), Name = "navigationTitle")]
            [VueProp(nameof(Value), VuePropKind.Model, Name = "selectedValue", AcceptsBinding = true)]
            [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:selectedValue", PayloadTypeName = "System.String?")]
            [VueSlot(nameof(Header), Name = "top")]
            """,
            implementationMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }

            [Parameter]
            public RenderFragment? Header { get; set; }
            """,
            renderStatements:
            """
            builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
            builder.AddComponentParameter(2, nameof(Demo.Containers.NavShell.Value), Value);
            builder.AddComponentParameter(3, nameof(Demo.Containers.NavShell.ValueChanged), ValueChanged);
            builder.AddComponentParameter(4, nameof(Demo.Containers.NavShell.Header), (RenderFragment)((slotBuilder) =>
            {
                slotBuilder.OpenElement(5, "strong");
                slotBuilder.AddContent(6, "Overview");
                slotBuilder.CloseElement();
            }));
            """,
            pageMembers:
            """
            [Parameter]
            public string? Value { get; set; }

            [Parameter]
            public EventCallback<string?> ValueChanged { get; set; }
            """);

        Assert.AreNotEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.LogicHash, identityB.LogicHash);
        Assert.AreEqual(identityA.StyleHash, identityB.StyleHash);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_UnusedInjectedPropRuntimeShapeDoesNotAffectDescriptorHash()
    {
        var identityA = CreateInjectedContainerHomePageSfcIdentity(
            contractMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? AccentColor { get; set; }
            """,
            implementationAttributes:
            """
            [VueProp(nameof(Title), Name = "menuTitle")]
            [VueProp(nameof(AccentColor), Name = "accentColor")]
            """,
            implementationMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? AccentColor { get; set; }
            """,
            renderStatements:
            """
            builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
            """);

        var identityB = CreateInjectedContainerHomePageSfcIdentity(
            contractMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? AccentColor { get; set; }
            """,
            implementationAttributes:
            """
            [VueProp(nameof(Title), Name = "menuTitle")]
            [VueProp(nameof(AccentColor), Name = "highlightColor")]
            """,
            implementationMembers:
            """
            [Parameter]
            public string? Title { get; set; }

            [Parameter]
            public string? AccentColor { get; set; }
            """,
            renderStatements:
            """
            builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
            """);

        Assert.AreEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.TemplateHash, identityB.TemplateHash);
        Assert.AreEqual(identityA.LogicHash, identityB.LogicHash);
        Assert.AreEqual(identityA.StyleHash, identityB.StyleHash);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_UsedDynamicPatternSlotShapeAffectsDescriptorHash()
    {
        var contextA = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Library
            {
                [VueLibraryComponent("demo-grid", "DemoGrid")]
                [VueSlot(nameof(ItemTemplate), Name = "item", NamePattern = "item.${string}", PatternOnly = true, ContextTypeName = "System.String", ContextParameterName = "item")]
                public sealed class GridShell : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Library.GridShell>(0);
                        builder.AddAttribute(1, "item.profile", (RenderFragment<string>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(2, "span");
                            slotBuilder.AddContent(3, item);
                            slotBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshotA = contextA.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "HomePage");
        var artifactA = CreateBuildRenderTreeArtifactFactory().Lower(contextA, snapshotA);

        var contextB = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Library
            {
                [VueLibraryComponent("demo-grid", "DemoGrid")]
                [VueSlot(nameof(ItemTemplate), Name = "item", NamePattern = "item.${string}", PatternOnly = true, Required = true, ContextTypeName = "System.String", ContextParameterName = "item")]
                public sealed class GridShell : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Library.GridShell>(0);
                        builder.AddAttribute(1, "item.profile", (RenderFragment<string>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(2, "span");
                            slotBuilder.AddContent(3, item);
                            slotBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshotB = contextB.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "HomePage");
        var artifactB = CreateBuildRenderTreeArtifactFactory().Lower(contextB, snapshotB);

        Assert.AreNotEqual(artifactA.Identity.DescriptorHash, artifactB.Identity.DescriptorHash);
        Assert.AreEqual(artifactA.Identity.TemplateHash, artifactB.Identity.TemplateHash);
        Assert.AreEqual(artifactA.Identity.LogicHash, artifactB.Identity.LogicHash);
        Assert.AreEqual(artifactA.Identity.StyleHash, artifactB.Identity.StyleHash);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersControlFlowIntoTemplateAndSetupBindings()
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.visible\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"item in props.items\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("computed", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCountStyleForLoop_IntoTemplateRangeHelperAndSetupBindings()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(0, props.count, &quot;&lt;&quot;, &quot;++&quot;, null)\">");
        StringAssert.Contains(artifact.TemplateText, "{{ i }}");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("import { computed } from \"vue\";", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueForRange = (start, limit, conditionOperator, stepOperator, stepValue) => {");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersStaticallyZeroIterationCountStyleForLoop_IntoTemplateRangeHelper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/zero-range-card")]
                public class ZeroRangeCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var i = 1; i < 0; i++)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/zero-range-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(1, 0, &quot;&lt;&quot;, &quot;++&quot;, null)\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "{{ i }}");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueForRange = (start, limit, conditionOperator, stepOperator, stepValue) => {");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCountStyleForLoopWithExplicitStep_IntoTemplateRangeHelperAndSetupBindings()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(props.start, props.count, &quot;&lt;=&quot;, &quot;+=&quot;, props.step)\">");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.ScriptSetupText, "const stepDelta = stepOperator === \"++\" ? 1");
        StringAssert.Contains(artifact.ScriptSetupText, "requires a finite non-zero effective step value");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCountStyleForLoopWithSimpleAssignmentStep_IntoTemplateRangeHelperAndSetupBindings()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(props.start, props.count, &quot;&lt;=&quot;, &quot;+=&quot;, props.step)\">");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.ScriptSetupText, "const stepDelta = stepOperator === \"++\" ? 1");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCountStyleForLoopWithStaticLocalStepCarrier_IntoTemplateRangeHelper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        const int step = 2;
                        for (var i = 0; i < Count; i += step)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(step) in [2]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(0, props.count, &quot;&lt;&quot;, &quot;+=&quot;, step)\">");
        Assert.IsFalse(artifact.SfcText.Contains("for (let i = 0; i < props.count; i += 2)", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDynamicAddAssignStep_UsesRenderFunctionToPreservePerIterationEvaluation()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private int GetStep()
                        => 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var i = Start; i <= Count; i += GetStep())
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        StringAssert.Contains(artifact.SfcText, "function getStep()");
        StringAssert.Contains(artifact.SfcText, "for (let i = props.start; i <= props.count; i += getStep())");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorVueForRange", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDynamicSimpleAssignmentStep_UsesRenderFunctionToPreservePerIterationEvaluation()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private int GetStep()
                        => 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var i = Start; i <= Count; i = i + GetStep())
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        StringAssert.Contains(artifact.SfcText, "function getStep()");
        StringAssert.Contains(artifact.SfcText, "for (let i = props.start; i <= props.count; i = i + getStep())");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorVueForRange", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCountStyleForLoopWithCommutativeSimpleAssignmentStep_IntoTemplateRangeHelperAndSetupBindings()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        for (var i = Start; i <= Count; i = Step + i)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"i in __jazorVueForRange(props.start, props.count, &quot;&lt;=&quot;, &quot;+=&quot;, props.step)\">");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.ScriptSetupText, "const stepDelta = stepOperator === \"++\" ? 1");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersRazorGeneratedChildContentLambdaShape()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public class LayoutCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder __builder)
                    {
                        __builder.OpenElement(0, "section");
                        __builder.AddAttribute(1, "ChildContent", (RenderFragment)((__builder2) =>
                        {
                            __builder2.OpenElement(2, "h1");
                            __builder2.AddContent(3, "Title");
                            __builder2.CloseElement();
                            __builder2.OpenElement(4, "p");
                            __builder2.AddContent(5, "Body");
                            __builder2.CloseElement();
                        }));
                        __builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "<h1>");
        StringAssert.Contains(artifact.TemplateText, "Title");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "Body");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersComponentDefaultSlotChildren()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
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
                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var renderTree = new RazorVueRenderFragment(
        [
            new RazorVueComponentNode(
                "LayoutCard",
                "Demo.Components.LayoutCard",
                "LayoutCard",
                null,
                ImmutableArray<RazorVueAttributeEntry>.Empty,
                ImmutableArray<RazorVueComponentSlotTemplateNode>.Empty,
                ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode>.Empty,
                new RazorVueRenderFragment(
                [
                    new RazorVueElementNode(
                        "span",
                        null,
                        ImmutableArray<RazorVueAttributeEntry>.Empty,
                        new RazorVueRenderFragment(
                        [
                            new RazorVueTextNode("Body", ImmutableArray<RazorVueSourceOrigin>.Empty)
                        ]),
                        ImmutableArray<RazorVueSourceOrigin>.Empty)
                ]),
                new RazorVueRenderFragment(
                [
                    new RazorVueElementNode(
                        "span",
                        null,
                        ImmutableArray<RazorVueAttributeEntry>.Empty,
                        new RazorVueRenderFragment(
                        [
                            new RazorVueTextNode("Body", ImmutableArray<RazorVueSourceOrigin>.Empty)
                        ]),
                        ImmutableArray<RazorVueSourceOrigin>.Empty)
                ]),
                ImmutableArray<RazorVueSourceOrigin>.Empty)
        ]);

        var artifact = new RazorVueSfcArtifactFactory(new FixedTemplateFrontend(renderTree)).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<LayoutCardComponent>");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "Body");
        StringAssert.Contains(artifact.TemplateText, "</LayoutCardComponent>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_CollectsLiftedBindingsInsideComponentDefaultSlotChildren()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    public bool Visible { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<LayoutCard>(0);
                        builder.AddAttribute(1, "ChildContent", (RenderFragment)((child) =>
                        {
                            if (Visible)
                            {
                                child.OpenElement(2, "span");
                                child.AddContent(3, "Shown");
                                child.CloseElement();
                            }
                        }));
                        builder.CloseComponent();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Host");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<LayoutCardComponent>");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.visible\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "Shown");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersPartialRazorGeneratedBuildRenderTreeShape()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.PartialRazor.Tests",
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
                        [ECMAScript.ECMAScriptModule("./components/partial-card")]
                        public partial class PartialCard : ComponentBase, IVueComponent
                        {
                        }
                    }
                    """,
                    path: "PartialCard.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using Microsoft.AspNetCore.Components;
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Components
                    {
                        public partial class PartialCard
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                                __builder.OpenElement(0, "section");
                                __builder.AddAttribute(1, "ChildContent", (RenderFragment)((__builder2) =>
                                {
                                    __builder2.OpenElement(2, "span");
                                    __builder2.AddContent(3, "FromRazor");
                                    __builder2.CloseElement();
                                }));
                                __builder.CloseElement();
                            }
                        }
                    }
                    """,
                    path: "PartialCard.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "FromRazor");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_DoesNotLowerDescriptorMembersAsRuntimeSetup()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/descriptor-card")]
                public class DescriptorCard : ComponentBase, IVueComponent
                {
                    private readonly string Prefix = "Title: ";

                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public EventCallback<string?> TitleChanged { get; set; }

                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Prefix + Title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(1, snapshot.Descriptor.Props.Length);
        Assert.AreEqual("title", snapshot.Descriptor.Props[0].Name);
        Assert.AreEqual("Title", snapshot.Descriptor.Props[0].PublicName);
        Assert.AreEqual(VuePropKind.Model, snapshot.Descriptor.Props[0].Kind);
        Assert.AreEqual(1, snapshot.Descriptor.Emits.Length);
        Assert.AreEqual("update:title", snapshot.Descriptor.Emits[0].Name);
        Assert.AreEqual("TitleChanged", snapshot.Descriptor.Emits[0].RazorAlias);
        Assert.AreEqual(VueEmitKind.ModelUpdate, snapshot.Descriptor.Emits[0].Kind);
        Assert.AreEqual(1, snapshot.Descriptor.Slots.Length);
        Assert.AreEqual("itemTemplate", snapshot.Descriptor.Slots[0].Name);
        Assert.AreEqual("ItemTemplate", snapshot.Descriptor.Slots[0].PublicName);

        CollectionAssert.DoesNotContain(snapshot.Logic.Properties.Select(static item => item.Name).ToArray(), "Title");
        CollectionAssert.DoesNotContain(snapshot.Logic.Properties.Select(static item => item.Name).ToArray(), "TitleChanged");
        CollectionAssert.DoesNotContain(snapshot.Logic.Properties.Select(static item => item.Name).ToArray(), "ItemTemplate");
        CollectionAssert.Contains(snapshot.Logic.Fields.Select(static item => item.Name).ToArray(), "Prefix");

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorRawProps = defineProps<{ title?: any }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const emit = defineEmits<{ (event: \"update:title\", payload?: any): void }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const prefix = \"Title: \";");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazor$0 }}");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("let title", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("function title", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("function itemTemplate", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersPartialRazorCsHelperReferencedByGeneratedRenderBody()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.PartialRazor.Helper.Tests",
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
                        [ECMAScript.ECMAScriptModule("./components/partial-helper")]
                        public partial class PartialHelper : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public int Value { get; set; }

                            private string FormatValue() => FormatPrefix() + Value;
                        }
                    }
                    """,
                    path: "PartialHelper.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Components
                    {
                        public partial class PartialHelper
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                                __builder.OpenElement(0, "section");
                                __builder.AddContent(1, FormatValue());
                                __builder.CloseElement();
                            }

                            private string FormatPrefix() => "Value: ";
                        }
                    }
                    """,
                    path: "PartialHelper.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        CollectionAssert.AreEquivalent(
            new[] { "FormatPrefix", "FormatValue" },
            snapshot.Logic.Methods.Select(static method => method.Name).ToArray());
        StringAssert.Contains(artifact.ScriptSetupText, "function formatPrefix()");
        StringAssert.Contains(artifact.ScriptSetupText, "return \"Value: \";");
        StringAssert.Contains(artifact.ScriptSetupText, "function formatValue()");
        StringAssert.Contains(artifact.ScriptSetupText, "formatPrefix() + props.value");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => formatValue());");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazor$0 }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersSetupFieldsMethodsAndLifecycleHooks()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    private int _count = 1;

                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<int> OnReady { get; set; }

                    private int DoubleCount() => _count * 2;

                    protected override void OnInitialized()
                        => OnReady.InvokeAsync(Value);

                    protected override void OnParametersSet()
                        => ValueChanged.InvokeAsync(Value);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, DoubleCount());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { onMounted, watch, computed } from \"vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "let _count = 1;");
        StringAssert.Contains(artifact.ScriptSetupText, "function doubleCount()");
        StringAssert.Contains(artifact.ScriptSetupText, "return (_count * 2);");
        StringAssert.Contains(artifact.ScriptSetupText, "onMounted(() => {");
        StringAssert.Contains(artifact.ScriptSetupText, "emit(\"ready\", props.value);");
        StringAssert.Contains(artifact.ScriptSetupText, "watch(() => [props.value], () => {");
        StringAssert.Contains(artifact.ScriptSetupText, "emit(\"update:value\", props.value);");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => doubleCount());");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazor$0 }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_ImportsNestedUserAndLibraryComponents_IntoScriptSetup()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Authoring
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Text { get; set; }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenComponent<ChildCard>(1);
                        builder.CloseComponent();
                        builder.OpenComponent<Demo.Authoring.DemoButton>(2);
                        builder.AddAttribute(3, nameof(Demo.Authoring.DemoButton.Text), "Save");
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import ChildCardComponent from \"./child-card.vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "import { DemoButton as DemoButton } from \"demo/components\";");
        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent />");
        StringAssert.Contains(artifact.TemplateText, "<DemoButton text=\"Save\" />");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "./child-card.vue");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "demo/components");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersComponentLiteralNonStringProps_ToBoundVueProps()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Authoring
            {
                [VueLibraryComponent("demo/components", "DemoPanel")]
                public sealed class DemoPanel : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public bool Fluid { get; set; }

                    [Parameter]
                    public int Columns { get; set; }

                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Authoring.DemoPanel>(0);
                        builder.AddAttribute(1, nameof(Demo.Authoring.DemoPanel.Fluid), true);
                        builder.AddAttribute(2, nameof(Demo.Authoring.DemoPanel.Columns), 12);
                        builder.AddAttribute(3, nameof(Demo.Authoring.DemoPanel.Title), "Inbox");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<DemoPanel :fluid=\"true\" :columns=\"12\" title=\"Inbox\" />");
        Assert.IsFalse(artifact.TemplateText.Contains(" fluid=\"true\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains(" columns=\"12\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("computed", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLibraryStringLiteralProp_WithoutLiftedBinding()
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
                [ECMAScript.ECMAScriptModule("./components/text-host")]
                public class TextHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VTextField>(0);
                        builder.AddAttribute(1, nameof(VTextField.Density), "comfortable");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<VTextField density=\"comfortable\" />");
        Assert.IsFalse(artifact.TemplateText.Contains(":density=", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("computed", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLibraryStringEnumProp_WithoutLiftedBinding()
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<VCard variant=\"outlined\" />");
        Assert.IsFalse(artifact.TemplateText.Contains(":variant=", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("computed", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersComponentLevelAttributeBinding_ThroughExplicitBindingSite()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "title", Title + "!");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section :title=\"(props.title + &quot;!&quot;)\" />");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LiftsNestedPropertyProjectionFromInvocation_IntoComputedBinding()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "function getTitle()");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => ");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazor$0 }}");
        Assert.IsFalse(artifact.TemplateText.Contains("getTitle(", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersRazorGeneratedEventCallbackFactoryWrapper_ToComponentEmitBridge()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.RazorGeneratedEventCallback.Tests",
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
                    using ECMAScript.Vuetify;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Components
                    {
                        [ECMAScript.ECMAScriptModule("./components/editor-card")]
                        public partial class EditorCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? ModelValue { get; set; }

                            [Parameter]
                            public EventCallback<string?> ModelValueChanged { get; set; }
                        }
                    }
                    """,
                    path: "EditorCard.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using ECMAScript.Vuetify;
                    using Microsoft.AspNetCore.Components.CompilerServices;
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Components
                    {
                        public partial class EditorCard
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                                __builder.OpenComponent<VTextField>(0);
                                __builder.AddComponentParameter(1, nameof(VTextField.ModelValue), RuntimeHelpers.TypeCheck<string?>(ModelValue));
                                __builder.AddComponentParameter(2, nameof(VTextField.ModelValueChanged), RuntimeHelpers.TypeCheck<EventCallback<string?>>(EventCallback.Factory.Create<string?>(this, ModelValueChanged)));
                                __builder.CloseComponent();
                            }
                        }
                    }
                    """,
                    path: "EditorCard.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<VTextField :modelValue=\"props.modelValue\" @update:modelValue=\"__jazor$0\" />");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorRawProps = defineProps<{ modelValue?: any }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const props = __jazorRawProps;");
        StringAssert.Contains(artifact.ScriptSetupText, "const emit = defineEmits<{ (event: \"update:modelValue\", payload?: any): void }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => (__value) => emit(\"update:modelValue\", __value));");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLiftedNamedSlotPresenceCheck_InjectsUseSlotsRuntime()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/card-shell")]
                public class CardShell : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");

                        if (Header is not null)
                        {
                            builder.OpenElement(1, "header");
                            builder.AddContent(2, Header);
                            builder.CloseElement();
                        }
                        else
                        {
                            builder.OpenElement(3, "div");
                            builder.AddContent(4, "fallback");
                            builder.CloseElement();
                        }

                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { computed, useSlots } from \"vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "const slots = useSlots();");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => !((slots.header ?? null) == null));");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelProp_ThrowsUnknownParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/select-host")]
                public class SelectHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VSelect>(0);
                        builder.AddAttribute(1, nameof(VSelect.ModelValue), "admin");
                        builder.AddAttribute(2, nameof(VSelect.SelectedValue), VuetifySelectModelValue.From("user"));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnknownParameter, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VSelect");
        StringAssert.Contains(exception.Issue.Message, "ModelValue");
        StringAssert.Contains(exception.Issue.Message, "SelectedValue");
        StringAssert.Contains(exception.Issue.Message, "modelValue");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/button-host")]
                public class ButtonHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VBtn>(0);
                        builder.AddAttribute(1, "class", "primary-action");
                        builder.AddAttribute(2, "style", "min-width: 160px");
                        builder.AddAttribute(3, "data-tracking-id", "save-order");
                        builder.AddAttribute(4, "aria-label", "Save order");
                        builder.AddAttribute(5, "ripple", false);
                        builder.AddAttribute(6, "viewMode", "month");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(
            artifact.TemplateText,
            "<VBtn class=\"primary-action\" style=\"min-width: 160px\" data-tracking-id=\"save-order\" aria-label=\"Save order\" :ripple=\"false\" viewMode=\"month\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelUpdateEmit_ThrowsUnknownParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/select-host")]
                public class SelectHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<string?> ModelChanged { get; set; }

                    [Parameter]
                    public EventCallback<VuetifySelectModelValue?> SelectedChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VSelect>(0);
                        builder.AddAttribute(1, nameof(VSelect.ModelValueChanged), ModelChanged);
                        builder.AddAttribute(2, nameof(VSelect.SelectedValueChanged), SelectedChanged);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnknownParameter, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VSelect");
        StringAssert.Contains(exception.Issue.Message, "ModelValueChanged");
        StringAssert.Contains(exception.Issue.Message, "SelectedValueChanged");
        StringAssert.Contains(exception.Issue.Message, "update:modelValue");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithInvalidLibraryBindTarget_ThrowsInvalidBindTarget()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/button-host")]
                public class ButtonHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Text { get; set; }

                    [Parameter]
                    public EventCallback<string?> TextChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VBtn>(0);
                        builder.AddAttribute(1, nameof(VBtn.Text), Text);
                        builder.AddAttribute(2, nameof(TextChanged), TextChanged);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.InvalidBindTarget, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VBtn");
        StringAssert.Contains(exception.Issue.Message, "Text");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithUnknownRenderFragmentLibraryAttribute_ThrowsUnknownSlot()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/button-host")]
                public class ButtonHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VBtn>(0);
                        builder.AddAttribute(1, "Footer", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.AddContent(2, "Footer");
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnknownSlot, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VBtn");
        StringAssert.Contains(exception.Issue.Message, "Footer");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithUnknownLibrarySlotTemplate_ThrowsUnknownSlot()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
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
                [ECMAScript.ECMAScriptModule("./components/button-host")]
                public class ButtonHost : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var renderTree = new RazorVueRenderFragment(
        [
            new RazorVueComponentNode(
                "VBtn",
                "ECMAScript.Vuetify.VBtn",
                "VBtn",
                null,
                ImmutableArray<RazorVueAttributeEntry>.Empty,
                [
                    new RazorVueComponentSlotTemplateNode(
                        "Footer",
                        "footer",
                        null,
                        null,
                        new RazorVueRenderFragment(
                        [
                            new RazorVueTextNode("Footer", ImmutableArray<RazorVueSourceOrigin>.Empty)
                        ]),
                        ImmutableArray<RazorVueSourceOrigin>.Empty)
                ],
                ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode>.Empty,
                RazorVueRenderFragment.Empty,
                RazorVueRenderFragment.Empty,
                ImmutableArray<RazorVueSourceOrigin>.Empty)
        ]);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueSfcArtifactFactory(new FixedTemplateFrontend(renderTree)).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnknownSlot, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VBtn");
        StringAssert.Contains(exception.Issue.Message, "Footer");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ThrowsUnknownSlot()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/icon-host")]
                public class IconHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDivider>(0);
                        builder.AddContent(1, "warn");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnknownSlot, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VDivider");
        StringAssert.Contains(exception.Issue.Message, "ChildContent");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithImplicitTypedLibraryDefaultSlot_EmitsParameterizedDefaultTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Library
            {
                [VueLibraryComponent("demo/components", "TypedContentPanel")]
                public sealed class TypedContentPanel : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ChildContent { get; set; }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/typed-content-host")]
                public class TypedContentHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Library.TypedContentPanel>(0);
                        builder.AddContent(1, "warn");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "TypedContentHost");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #default=\"context\">");
        StringAssert.Contains(artifact.TemplateText, "warn");
        StringAssert.Contains(artifact.TemplateText, "</template>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDuplicateLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDialog>(0);
                        builder.AddAttribute(1, nameof(VDialog.ChildContent), ChildContent);
                        builder.AddAttribute(2, nameof(VDialog.ChildContent), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.DuplicateSlotValue, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VDialog");
        StringAssert.Contains(exception.Issue.Message, "ChildContent");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithImplicitAndExplicitLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/chip-host")]
                public class ChipHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<VChipDefaultSlotContext>? ChipDefault { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VChip>(0);
                        builder.AddAttribute(1, nameof(VChip.DefaultContent), ChipDefault);
                        builder.AddContent(2, "Pin");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.DuplicateSlotValue, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VChip");
        StringAssert.Contains(exception.Issue.Message, "DefaultContent");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDuplicateLibraryNamedSlotAssignment_ThrowsDuplicateSlotValue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/alert-host")]
                public class AlertHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Prepend { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VAlert>(0);
                        builder.AddAttribute(1, nameof(VAlert.Prepend), Prepend);
                        builder.AddAttribute(2, nameof(VAlert.Prepend), Prepend);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.DuplicateSlotValue, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "VAlert");
        StringAssert.Contains(exception.Issue.Message, "Prepend");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithComponentLocalVariableDeclarationInBuildRenderTree_LowersTemplateScopedAlias()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/local-card")]
                public class LocalCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithTemplateScopedLocalWithoutInitializerThenImmediateAssignmentInBuildRenderTree_LowersTemplateScopedAlias()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/local-card")]
                public class LocalCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLoopBodyComponentLocalVariableDeclarationInBuildRenderTree_LowersTemplateScopedAlias()
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
                [ECMAScript.ECMAScriptModule("./components/local-loop-card")]
                public class LocalLoopCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"item in props.items\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(decorated) in [(item + &quot;!&quot;)]\">".Replace("&quot;", "\""));
        StringAssert.Contains(artifact.TemplateText, "{{ decorated }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersExpressionBodiedCurrentComponentRenderHelperMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "{{ props.title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithExtraParameters()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        Assert.IsFalse(
            artifact.TemplateText.Contains("</template>\r\n</template>", StringComparison.Ordinal) ||
            artifact.TemplateText.Contains("</template>\n</template>", StringComparison.Ordinal),
            artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithInParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderBody(builder, Title);
                    }

                    private void RenderBody(RenderTreeBuilder builder, in string? title)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, title);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersGenericCurrentComponentRenderHelperMethodWithExtraParameters()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        Assert.IsFalse(
            artifact.TemplateText.Contains("</template>\r\n</template>", StringComparison.Ordinal) ||
            artifact.TemplateText.Contains("</template>\n</template>", StringComparison.Ordinal),
            artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithExtraParameters()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        Assert.IsFalse(
            artifact.TemplateText.Contains("</template>\r\n</template>", StringComparison.Ordinal) ||
            artifact.TemplateText.Contains("</template>\n</template>", StringComparison.Ordinal),
            artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithInParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        void RenderBody(RenderTreeBuilder localBuilder, in string? title)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithNamedArguments()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithOmittedOptionalParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithMultipleExtraParameters()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(subtitle) in [props.subtitle]\">");
        StringAssert.Contains(artifact.TemplateText, "<section");
        StringAssert.Contains(artifact.TemplateText, ":data-title=\"title\"");
        StringAssert.Contains(artifact.TemplateText, ":data-subtitle=\"subtitle\"");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithMultipleExtraParameters()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(subtitle) in [props.subtitle]\">");
        StringAssert.Contains(artifact.TemplateText, "<section");
        StringAssert.Contains(artifact.TemplateText, ":data-title=\"title\"");
        StringAssert.Contains(artifact.TemplateText, ":data-subtitle=\"subtitle\"");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        Assert.IsTrue(subtitleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < titleIndex, artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        Assert.IsTrue(subtitleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < titleIndex, artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithExtraParameterBackedTemplateLocal()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersBuildRenderTreeLocalFunctionHelperWithExtraParameterBackedTemplateLocal()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLoopInvokedCurrentComponentRenderHelperMethodWithExtraParameterBackedTemplateLocal()
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
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"item in props.items\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [item]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLoopInvokedBuildRenderTreeLocalFunctionHelperWithExtraParameterBackedTemplateLocal()
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
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"item in props.items\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [item]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithNamedArguments()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithOmittedOptionalParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentRenderHelperMethodWithParamsParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => [props.title, \"suffix\"]);");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(values) in [__jazor$0]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ values.length }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithTypedSlotTemplateComponentLocalVariableInTemplate_LowersTemplateScopedAlias()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, nameof(ChildCard.ItemTemplate), (RenderFragment<int>)((item) => (slotBuilder) =>
                        {
                            var decorated = item + 1;
                            slotBuilder.OpenElement(2, "span");
                            slotBuilder.AddContent(3, decorated);
                            slotBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(decorated) in [(item + 1)]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ decorated }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLocalCarrierTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithConditionalReturnInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/conditional-return-card")]
                public class ConditionalReturnCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/conditional-return-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hide\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDynamicShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((props.value > 0)))");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        StringAssert.Contains(artifact.SfcText, "const __jazorNextVNode = (() => {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, props.value));");
        StringAssert.Contains(artifact.SfcText, "return __jazorRenderContext.finish();");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalPrefixedShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-local-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        var threshold = Value + 1;
                        return threshold > 2;
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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " = props.value + 1;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " > 2;");
        StringAssert.Contains(artifact.SfcText, "})()))");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalMutationShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-local-mutation-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        var props = Value;
                        props++;
                        props += 2;
                        return props > 3;
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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " = props.value;");
        StringAssert.Contains(artifact.SfcText, "++;");
        StringAssert.Contains(artifact.SfcText, " += 2;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " > 3;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.IsFalse(artifact.SfcText.Contains("let props =", StringComparison.Ordinal), artifact.SfcText);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithIfElseShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-if-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        if (Value < 0)
                        {
                            return false;
                        }

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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "if (props.value < 0) {");
        StringAssert.Contains(artifact.SfcText, "return false;");
        StringAssert.Contains(artifact.SfcText, "return props.value > 0;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSwitchShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-switch-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        switch (Value)
                        {
                            case 0:
                                return false;
                            case 1:
                                return true;
                            default:
                                return Value > 1;
                        }
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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "switch (props.value) {");
        StringAssert.Contains(artifact.SfcText, "case 0:");
        StringAssert.Contains(artifact.SfcText, "return false;");
        StringAssert.Contains(artifact.SfcText, "case 1:");
        StringAssert.Contains(artifact.SfcText, "return true;");
        StringAssert.Contains(artifact.SfcText, "default:");
        StringAssert.Contains(artifact.SfcText, "return props.value > 1;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithPatternLocalShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-pattern-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public object? Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        if (Value is int props)
                        {
                            return props > 0;
                        }

                        return false;
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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, "typeof props.value === \"number\"");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " > 0;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.IsFalse(artifact.SfcText.Contains("let props =", StringComparison.Ordinal), artifact.SfcText);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRecursivePatternDeclaredLocalShouldRender_LowersRenderFunctionVueSfcWithCachedRenderGate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/should-render-recursive-pattern-sfc")]
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public object? Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        if (Value is int { } props)
                        {
                            return props > 0;
                        }

                        return false;
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

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderHasRendered = false;");
        StringAssert.Contains(artifact.SfcText, "if (__jazorShouldRenderHasRendered && !((() => {");
        StringAssert.Contains(artifact.SfcText, "let __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, "typeof props.value === \"number\"");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderLocal");
        StringAssert.Contains(artifact.SfcText, " > 0;");
        StringAssert.Contains(artifact.SfcText, "return __jazorShouldRenderCachedVNode;");
        Assert.IsFalse(artifact.SfcText.Contains("let props =", StringComparison.Ordinal), artifact.SfcText);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithBalancedRegionInImperativeBuildRenderTree_EmitsRegionDepthValidation()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/imperative-region-card")]
                public class ImperativeRegionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    private bool ShouldHide()
                        => Hide;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShouldHide())
                        {
                            return;
                        }

                        builder.OpenElement(0, "section");
                        builder.OpenRegion(1);
                        builder.AddContent(2, "ready");
                        builder.CloseRegion();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.openRegion();");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.closeRegion();");
        StringAssert.Contains(artifact.SfcText, "openRegion() { __regions.push(__stack.length); },");
        StringAssert.Contains(artifact.SfcText, "if (__regions.length !== 0) throw new Error(\"RazorVue imperative render bridge completed with unclosed regions.\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithTypedRenderFragmentAddContentInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

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

                        var index = 0;
                        while (index < Count)
                        {
                            builder.AddContent(0, template, index);
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let template = (title => item => {");
        StringAssert.Contains(artifact.SfcText, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(item);");
        StringAssert.Contains(artifact.SfcText, "})(props.title);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(template, index);");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("itemBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryTypedRenderFragmentAddContentInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

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

                        RenderFragment<int> template = CreateTemplate(Title);

                        var index = 0;
                        while (index < Count)
                        {
                            builder.AddContent(0, template, index);
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let template = (title => item => {");
        StringAssert.Contains(artifact.SfcText, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(item);");
        StringAssert.Contains(artifact.SfcText, "})(props.title);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(template, index);");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("itemBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryUntypedRenderFragmentAddContentInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment CreateHeader(string? title)
                            => headerBuilder =>
                            {
                                headerBuilder.OpenElement(1, "header");
                                headerBuilder.AddContent(2, title);
                                headerBuilder.CloseElement();
                            };

                        var index = 0;
                        while (index < Count)
                        {
                            builder.AddContent(0, CreateHeader(Title));
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append((title => () => {");
        StringAssert.Contains(artifact.SfcText, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.enterElement(\"header\");");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "})(props.title));");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("headerBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryBackedUntypedRenderFragmentAddContentInsideImperativeBuildRenderTree_EvaluatesCapturedArgumentsOnce()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    private int _revision;

                    private string NextTitle()
                    {
                        _revision++;
                        return "title-" + _revision;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment CreateHeader(string title)
                            => headerBuilder =>
                            {
                                headerBuilder.OpenElement(1, "header");
                                headerBuilder.AddContent(2, title);
                                headerBuilder.CloseElement();
                            };

                        RenderFragment header = CreateHeader(NextTitle());
                        var index = 0;
                        while (index < 2)
                        {
                            builder.AddContent(0, header);
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let header = (title => () => {");
        StringAssert.Contains(artifact.SfcText, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "})(nextTitle());");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(header);");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.SfcText, "})(nextTitle());"),
            artifact.SfcText);
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateHeader(nextTitle())", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createHeader(nextTitle())", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("headerBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryTypedRenderFragmentComponentParameterInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
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

                        var index = 0;
                        while (index < 1)
                        {
                            builder.OpenComponent<ChildCard>(0);
                            builder.AddAttribute(1, "ItemTemplate", CreateTemplate(Title));
                            builder.CloseComponent();
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ImperativeFragmentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterComponent(ChildCardComponent");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setAttribute(\"ItemTemplate\", (title => item => {");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(item);");
        StringAssert.Contains(artifact.SfcText, "})(props.title));");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("itemBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryUntypedRenderFragmentDefaultSlotInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment CreateHeader(string? title)
                            => headerBuilder =>
                            {
                                headerBuilder.OpenElement(1, "header");
                                headerBuilder.AddContent(2, title);
                                headerBuilder.CloseElement();
                            };

                        var index = 0;
                        while (index < 1)
                        {
                            builder.OpenComponent<Panel>(0);
                            builder.AddAttribute(1, "ChildContent", CreateHeader(Title));
                            builder.CloseComponent();
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ImperativeFragmentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterComponent(PanelComponent");
        StringAssert.Contains(artifact.SfcText, "\"ChildContent\": { runtimeName: \"default\" }");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setAttribute(\"ChildContent\", (title => () => {");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.enterElement(\"header\");");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "})(props.title));");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("headerBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLocalFunctionFactoryBackedUntypedRenderFragmentDefaultSlotInsideImperativeBuildRenderTree_EvaluatesCapturedArgumentsOnce()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                [ECMAScript.ECMAScriptModule("./components/imperative-fragment-card")]
                public class ImperativeFragmentCard : ComponentBase, IVueComponent
                {
                    private int _revision;

                    private string NextTitle()
                    {
                        _revision++;
                        return "title-" + _revision;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment CreateHeader(string title)
                            => headerBuilder =>
                            {
                                headerBuilder.OpenElement(1, "header");
                                headerBuilder.AddContent(2, title);
                                headerBuilder.CloseElement();
                            };

                        RenderFragment header = CreateHeader(NextTitle());
                        var index = 0;
                        while (index < 2)
                        {
                            builder.OpenComponent<Panel>(0);
                            builder.AddAttribute(1, "ChildContent", header);
                            builder.CloseComponent();
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ImperativeFragmentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let header = (title => () => {");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "})(nextTitle());");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setAttribute(\"ChildContent\", header);");
        Assert.AreEqual(
            1,
            CountOccurrences(artifact.SfcText, "})(nextTitle());"),
            artifact.SfcText);
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateHeader(nextTitle())", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createHeader(nextTitle())", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("headerBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentMemberBackedTypedRenderFragmentAddContentInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    public string? Prefix { get; set; }

                    [Parameter]
                    public int ModelValue { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/imperative-slot-host")]
                public class ImperativeSlotHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowEditor { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment<int> Template => CreateTemplate(Title);

                    private RenderFragment<int> CreateTemplate(string? title)
                        => item => slotBuilder =>
                        {
                            slotBuilder.OpenComponent<ItemEditor>(4);
                            slotBuilder.AddAttribute(5, nameof(ItemEditor.Prefix), title);
                            slotBuilder.AddAttribute(6, nameof(ItemEditor.ModelValue), item);
                            slotBuilder.CloseComponent();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (!ShowEditor)
                        {
                            builder.OpenElement(0, "p");
                            builder.AddContent(1, "fallback");
                            builder.CloseElement();
                            return;
                        }

                        builder.AddContent(2, Template, 42);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ImperativeSlotHost");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append((title => item => {");
        StringAssert.Contains(artifact.SfcText, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.enterComponent(ItemEditorComponent, __jazorImperativeComponentMetadata_ItemEditor);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.setAttribute(\"Prefix\", title);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.setAttribute(\"ModelValue\", item);");
        StringAssert.Contains(artifact.SfcText, "})(props.title), 42);");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createTemplate(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("slotBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentMemberBackedUntypedRenderFragmentDefaultSlotInsideImperativeBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                [ECMAScript.ECMAScriptModule("./components/imperative-slot-host")]
                public class ImperativeSlotHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowHeader { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    private RenderFragment Header => CreateHeader(Title);

                    private RenderFragment CreateHeader(string? title)
                        => headerBuilder =>
                        {
                            headerBuilder.OpenElement(1, "header");
                            headerBuilder.AddContent(2, title);
                            headerBuilder.CloseElement();
                        };

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (!ShowHeader)
                        {
                            builder.OpenElement(0, "p");
                            builder.AddContent(1, "fallback");
                            builder.CloseElement();
                            return;
                        }

                        builder.OpenComponent<Panel>(2);
                        builder.AddAttribute(3, nameof(Panel.ChildContent), Header);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ImperativeSlotHost");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterComponent(PanelComponent");
        StringAssert.Contains(artifact.SfcText, "\"ChildContent\": { runtimeName: \"default\" }");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setAttribute(\"ChildContent\", (title => () => {");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.enterElement(\"header\");");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeRenderContext0.append(title);");
        StringAssert.Contains(artifact.SfcText, "})(props.title));");
        Assert.IsFalse(
            artifact.SfcText.Contains("CreateHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("createHeader(props.title)", StringComparison.Ordinal) ||
            artifact.SfcText.Contains("headerBuilder =>", StringComparison.Ordinal),
            artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedAttributeMutationPlusChildEmission_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedRegionChildEmission_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/render-helper-card")]
                public class RenderHelperCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "((title) => {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"span\", null, title));");
        StringAssert.Contains(artifact.SfcText, "})(props.title);");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorRenderContext.openRegion();", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("__jazorRenderContext.closeRegion();", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedImplicitDefaultSlotAssignment_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () =>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedAmbientDefaultSlotChild_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () =>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedRegionAmbientDefaultSlotChild_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setComponentParameter(\"ChildContent\", () =>");
        Assert.IsFalse(
            artifact.SfcText.Contains("__jazorRenderContext.append(h(\"span\", null, title));", StringComparison.Ordinal),
            artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("__jazorRenderContext.openRegion();", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("__jazorRenderContext.closeRegion();", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedNamedAndTypedSlotAssignments_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setComponentParameter(\"Header\", () =>");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", (item) =>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSwitchStatementInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/switch-card")]
                public class SwitchCard : ComponentBase, IVueComponent
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
                                return;
                            default:
                                builder.OpenElement(2, "section");
                                builder.AddContent(3, Count);
                                builder.CloseElement();
                                return;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/switch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "switch (props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"empty\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(props.count);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSimpleConstantSwitchStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/simple-switch-card")]
                public class SimpleSwitchCard : ComponentBase, IVueComponent
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
                            case 1:
                                builder.OpenElement(2, "span");
                                builder.AddContent(3, "one");
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(4, "section");
                                builder.AddContent(5, Count);
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/simple-switch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(__jazorSwitchValue) in [props.count]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue === 0\">");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "empty");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue === 1\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "one");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.count }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSimpleConstantSwitchEmptyCaseFallthrough_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/simple-switch-card")]
                public class SimpleSwitchCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Count)
                        {
                            case 0:
                            case 1:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, "small");
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/simple-switch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(__jazorSwitchValue) in [props.count]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue === 0 || __jazorSwitchValue === 1\">");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "small");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.count }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSimpleConstantSwitchNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/simple-switch-helper-card")]
                public class SimpleSwitchHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Count)
                        {
                            case 0:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, Helper.Text);
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/simple-switch-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "switch (props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(props.count);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSimpleConstantSwitchNestedHelperTypeCondition_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/simple-switch-helper-condition-card")]
                public class SimpleSwitchHelperConditionCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static int Value => 0;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Helper.Value)
                        {
                            case 0:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, "ready");
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(2, "section");
                                builder.AddContent(3, "fallback");
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/simple-switch-helper-condition-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "switch (Helper.value)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithGuardedConstantPatternSwitchStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guarded-switch-card")]
                public class GuardedSwitchCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Count)
                        {
                            case 0 when Count < 2:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, "guarded zero");
                                builder.CloseElement();
                                break;
                            case > 5:
                                builder.OpenElement(2, "strong");
                                builder.AddContent(3, "large");
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(4, "section");
                                builder.AddContent(5, Count);
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guarded-switch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(__jazorSwitchValue) in [props.count]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue === 0 &amp;&amp; props.count &lt; 2\">");
        StringAssert.Contains(artifact.TemplateText, "guarded zero");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue &gt; 5\">");
        StringAssert.Contains(artifact.TemplateText, "<strong>");
        StringAssert.Contains(artifact.TemplateText, "large");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.count }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDeclarationPatternSwitchStatementInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/declaration-switch-card")]
                public class DeclarationSwitchCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public object? Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Value)
                        {
                            case string text when text.Length > 0:
                                builder.OpenElement(0, "p");
                                builder.AddContent(1, text);
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(2, "section");
                                builder.AddContent(3, "empty");
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/declaration-switch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "const __swpat$");
        StringAssert.Contains(artifact.SfcText, "text.length > 0");
        Assert.IsFalse(artifact.SfcText.Contains("<template v-if=", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSimpleConstantSwitchGotoCase_ThrowsStructuralIssue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/switch-card")]
                public class SwitchCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        switch (Count)
                        {
                            case 0:
                                goto case 1;
                            case 1:
                                builder.OpenElement(0, "section");
                                builder.AddContent(1, Count);
                                builder.CloseElement();
                                break;
                            default:
                                builder.OpenElement(2, "span");
                                builder.AddContent(3, "many");
                                builder.CloseElement();
                                break;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "does not support 'goto'");
        StringAssert.Contains(exception.Issue.Message, "Jazor.Compiler does not provide an equivalent JavaScript lowering");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hide\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnInvocationCondition_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    private bool ShouldHide()
                        => Hide;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShouldHide())
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (shouldHide())");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-helper-card")]
                public class GuardHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }

                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Helper.Text);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/guard-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (props.hide)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnNestedHelperTypeCondition_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-helper-condition-card")]
                public class GuardHelperConditionCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static bool Hide => false;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Helper.Hide)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/guard-helper-condition-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (Helper.hide)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnElseOutput_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }
                        else
                        {
                            builder.OpenElement(0, "p");
                            builder.AddContent(1, "else");
                            builder.CloseElement();
                        }

                        builder.OpenElement(2, "section");
                        builder.AddContent(3, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hide\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "else");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootElseGuardReturnOutput_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowIntro { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShowIntro)
                        {
                            builder.OpenElement(0, "p");
                            builder.AddContent(1, "intro");
                            builder.CloseElement();
                        }
                        else
                        {
                            return;
                        }

                        builder.OpenElement(2, "section");
                        builder.AddContent(3, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.showIntro\">");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "intro");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNestedGuardReturnInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-guard-card")]
                public class NestedGuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowPanel { get; set; }

                    [Parameter]
                    public bool HideDetails { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShowPanel)
                        {
                            if (HideDetails)
                            {
                                return;
                            }

                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "details");
                            builder.CloseElement();
                        }
                        else
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, "fallback");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/nested-guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.showPanel\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hideDetails\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "details");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "fallback");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNestedGuardReturnInvocationCondition_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-guard-card")]
                public class NestedGuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowPanel { get; set; }

                    [Parameter]
                    public bool HideDetails { get; set; }

                    private bool ShouldHideDetails()
                        => HideDetails;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShowPanel)
                        {
                            if (ShouldHideDetails())
                            {
                                return;
                            }

                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "details");
                            builder.CloseElement();
                        }
                        else
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, "fallback");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/nested-guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (props.showPanel)");
        StringAssert.Contains(artifact.SfcText, "if (shouldHideDetails())");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNestedGuardReturnTailMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-guard-card")]
                public class NestedGuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowPanel { get; set; }

                    [Parameter]
                    public bool HideDetails { get; set; }

                    private int _count;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (ShowPanel)
                        {
                            if (HideDetails)
                            {
                                return;
                            }

                            _count++;
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, _count);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/nested-guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (props.showPanel)");
        StringAssert.Contains(artifact.SfcText, "_count++");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDoubleReturnNoTemplateOutput_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/empty-guard-card")]
                public class EmptyGuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/empty-guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "if (props.hide)");
        StringAssert.Contains(artifact.SfcText, "return;");
        Assert.IsFalse(artifact.SfcText.Contains("<template v-if=", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnBranchMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    private int _count;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }
                        else
                        {
                            _count++;
                            builder.OpenElement(0, "p");
                            builder.AddContent(1, _count);
                            builder.CloseElement();
                        }

                        builder.OpenElement(2, "section");
                        builder.AddContent(3, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "_count++");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootGuardReturnTailMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/guard-card")]
                public class GuardCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }

                    private int _count;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (Hide)
                        {
                            return;
                        }

                        _count++;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, _count);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/guard-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "_count++");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithTryCatchFinallyInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"fallback\");");
        StringAssert.Contains(artifact.SfcText, "_count++;");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyNoOpCleanupInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-helper-card")]
                public class TryHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Helper.Text);
                            builder.CloseElement();
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/try-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyStaticCleanupInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddAttribute(1, "class", "body");
                            builder.AddContent(2, "ready");
                            builder.CloseElement();
                        }
                        finally
                        {
                            builder.OpenElement(3, "footer");
                            builder.AddContent(4, "cleanup");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section class=\"body\">");
        StringAssert.Contains(artifact.TemplateText, "ready");
        StringAssert.Contains(artifact.TemplateText, "<footer>");
        StringAssert.Contains(artifact.TemplateText, "cleanup");
        Assert.IsTrue(
            artifact.TemplateText.IndexOf("<section", StringComparison.Ordinal) <
            artifact.TemplateText.IndexOf("<footer", StringComparison.Ordinal),
            artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyDynamicTryAndStaticCleanupInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Title);
                            builder.CloseElement();
                        }
                        finally
                        {
                            builder.OpenElement(2, "footer");
                            builder.AddContent(3, "cleanup");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(props.title);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"footer\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyStaticTryAndDynamicCleanupInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Footer { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        finally
                        {
                            builder.OpenElement(2, "footer");
                            builder.AddContent(3, Footer);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(props.footer);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryFinallyMutationInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
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
                        finally
                        {
                            _count++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "_count++;");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchNoOpRecoveryInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
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
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-catch-helper-card")]
                public class TryCatchHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Helper.Text);
                            builder.CloseElement();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/try-catch-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTypedTryCatchNoOpRecoveryInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/typed-catch-card")]
                public class TypedCatchCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/typed-catch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchFilterNoOpRecoveryInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/filter-catch-card")]
                public class FilterCatchCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Ready { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        catch (Exception) when (Ready)
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/filter-catch-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "catch");
        StringAssert.Contains(artifact.SfcText, "if (!props.ready)");
        StringAssert.Contains(artifact.SfcText, "throw");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchVariablePayloadInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/catch-payload-card")]
                public class CatchPayloadCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        catch (Exception error)
                        {
                            builder.OpenElement(2, "p");
                            builder.AddContent(3, error.Message);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/catch-payload-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch (error) {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(error.message);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchBareRethrowInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/rethrow-card")]
                public class RethrowCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        try
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        catch (Exception error)
                        {
                            throw;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/rethrow-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch (error) {");
        StringAssert.Contains(artifact.SfcText, "throw error;");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchOutputRecoveryInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
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
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchFinallyNoOpRecoveryCleanupInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
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
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootTryCatchFinallyRecoveryAndNoOpCleanupInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/try-card")]
                public class TryCard : ComponentBase, IVueComponent
                {
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
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/try-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithUsingDeclarationInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/using-card")]
                public class UsingCard : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = new TestDisposable();
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/using-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let disposable = new ");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
        StringAssert.Contains(artifact.SfcText, "if (disposable !== null)");
        StringAssert.Contains(artifact.SfcText, "disposable.dispose();");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithUsingDeclarationNestedHelperTypeInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-using-card")]
                public class NestedUsingCard : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = new TestDisposable();
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class TestDisposable", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/nested-using-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let disposable = new TestDisposable");
        StringAssert.Contains(artifact.SfcText, "disposable.dispose();");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDefaultUsingDeclarationInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/default-using-declaration-card")]
                public class DefaultUsingDeclarationCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = default(IDisposable);
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/default-using-declaration-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorCreateRenderContext", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("try {", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("dispose", StringComparison.OrdinalIgnoreCase), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootNullUsingDeclarationInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/null-using-declaration-card")]
                public class NullUsingDeclarationCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = (IDisposable?)null;
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/null-using-declaration-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorCreateRenderContext", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("try {", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("dispose", StringComparison.OrdinalIgnoreCase), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDefaultUsingDeclarationLocalReadInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/default-using-declaration-read-card")]
                public class DefaultUsingDeclarationReadCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = default(IDisposable);
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, disposable is null);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/default-using-declaration-read-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let disposable = null;");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "if (disposable !== null)");
        StringAssert.Contains(artifact.SfcText, "_6f97d94b6f2e4bc1(disposable);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDefaultUsingDeclarationNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/default-using-helper-card")]
                public class DefaultUsingHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = default(IDisposable);
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Helper.Text);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/default-using-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let disposable = null;");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
        StringAssert.Contains(artifact.SfcText, "if (disposable !== null)");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDefaultUsingStatementNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/default-using-helper-card")]
                public class DefaultUsingHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using (default(IDisposable))
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Helper.Text);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/default-using-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "let ");
        StringAssert.Contains(artifact.SfcText, " = null;");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
        StringAssert.Contains(artifact.SfcText, "if (");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNestedHelperTypeTypeReferenceAndStaticMemberInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-type-card")]
                public class NestedTypeCard : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public static string Describe() => "helper";

                        public void Dispose() { }
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        lock (this)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, typeof(TestDisposable).Name);
                            builder.AddContent(2, TestDisposable.Describe());
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class TestDisposable", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/nested-type-card.vue", artifact.RelativeSfcPath);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "TestDisposable.name");
        StringAssert.Contains(artifact.SfcText, "TestDisposable.describe()");
        StringAssert.Contains(artifact.SfcText, "try {");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithStaticNestedHelperTypeStaticMemberInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/static-helper-card")]
                public class StaticHelperCard : ComponentBase, IVueComponent
                {
                    private static class StaticHelpers
                    {
                        public static string Label = "helper";

                        public static string Describe() => Label;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        lock (this)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, typeof(StaticHelpers).Name);
                            builder.AddContent(2, StaticHelpers.Describe());
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class StaticHelpers", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/static-helper-card.vue", artifact.RelativeSfcPath);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "static label = \"helper\";");
        StringAssert.Contains(artifact.SfcText, "static describe()");
        StringAssert.Contains(artifact.SfcText, "StaticHelpers.name");
        StringAssert.Contains(artifact.SfcText, "StaticHelpers.describe()");
        StringAssert.Contains(artifact.SfcText, "try {");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithReferencedLocalFunctionDeclarationInMixedImperativeRenderSegment_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/local-function-card")]
                public class LocalFunctionCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/local-function-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "h(\"header\", null, props.title)");
        StringAssert.Contains(artifact.SfcText, "function AppendLine(value)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(value);");
        StringAssert.Contains(artifact.SfcText, "while (index < 1) {");
        StringAssert.Contains(artifact.SfcText, "AppendLine(\"ready\");");
        StringAssert.Contains(artifact.SfcText, "h(\"footer\", null, \"done\")");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLabeledBlockInMixedImperativeRenderSegment_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/labeled-card")]
                public class LabeledCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/labeled-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "h(\"header\", null, props.title)");
        StringAssert.Contains(artifact.SfcText, "renderBlock: {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"labeled\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.leaveElement();");
        StringAssert.Contains(artifact.SfcText, "h(\"footer\", null, \"done\")");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootLabeledBlockInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/labeled-card")]
                public class LabeledCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        renderBlock:
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "labeled");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/labeled-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "labeled");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootLabeledBlockNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/labeled-helper-card")]
                public class LabeledHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        renderBlock:
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Helper.Text);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/labeled-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "renderBlock: {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootLabeledBlockContainingMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/labeled-card")]
                public class LabeledCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        renderBlock:
                        {
                            var count = 0;
                            count++;
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, count);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/labeled-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "renderBlock: {");
        StringAssert.Contains(artifact.SfcText, "count++");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(count);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithGotoStatementInBuildRenderTree_ThrowsUnsupportedImperativeRenderLowering()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/goto-card")]
                public class GotoCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        goto renderBlock;

                        renderBlock:
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "labeled");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "goto");
        StringAssert.Contains(exception.Issue.Message, "Jazor.Compiler");
        StringAssert.Contains(exception.Issue.Message, "control flow");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDeconstructionAssignmentInMixedImperativeRenderSegment_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/deconstruction-card")]
                public class DeconstructionCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/deconstruction-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "h(\"header\", null, props.title)");
        StringAssert.Contains(artifact.SfcText, "label = pair.");
        StringAssert.Contains(artifact.SfcText, "suffix = pair.");
        StringAssert.Contains(artifact.SfcText, "while (index < 1) {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"p\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(label);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(suffix);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"footer\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(label);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNestedHelperTypeClrImportInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/nested-clr-using-card")]
                public class NestedClrUsingCard : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose()
                        {
                            if (Array.IndexOf(new[] { "ready", "done" }, "ready") < 0)
                                throw new InvalidOperationException();
                        }
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using var disposable = new TestDisposable();
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var importIndex = artifact.SfcText.IndexOf("from \"System/ArrayModule.js\";", StringComparison.Ordinal);
        var classIndex = artifact.SfcText.IndexOf("class TestDisposable", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/nested-clr-using-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(importIndex >= 0, artifact.SfcText);
        Assert.IsTrue(classIndex > importIndex, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "class TestDisposable");
        StringAssert.Contains(artifact.SfcText, "dispose() {");
        StringAssert.Contains(artifact.SfcText, "System/ArrayModule.js");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithUsingExpressionInterfaceResourceInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                [ECMAScript.ECMAScriptModule("./components/using-interface-card")]
                public class UsingInterfaceCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/using-interface-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "class TestDisposable");
        StringAssert.Contains(artifact.SfcText, "function getDisposable()");
        StringAssert.Contains(artifact.SfcText, "return new TestDisposable");
        StringAssert.Contains(artifact.SfcText, "let ");
        StringAssert.Contains(artifact.SfcText, " = getDisposable();");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
        StringAssert.Contains(artifact.SfcText, "!== null)");
        StringAssert.Contains(artifact.SfcText, "_6f97d94b6f2e4bc1(");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootNullUsingStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/null-using-card")]
                public class NullUsingCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using ((IDisposable?)null)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/null-using-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDefaultUsingStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/default-using-card")]
                public class DefaultUsingCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        using (default(IDisposable))
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/default-using-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithAwaitUsingDeclarationInBuildRenderTree_ThrowsUnsupportedImperativeRenderLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/await-using-card")]
                public class AwaitUsingCard : ComponentBase, IVueComponent
                {
                    private sealed class AsyncDisposable : IAsyncDisposable
                    {
                        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                    }

                    protected override async void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        await using var disposable = new AsyncDisposable();
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "await using");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithAwaitUsingStatementInBuildRenderTree_ThrowsUnsupportedImperativeRenderLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/await-using-card")]
                public class AwaitUsingCard : ComponentBase, IVueComponent
                {
                    private sealed class AsyncDisposable : IAsyncDisposable
                    {
                        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                    }

                    protected override async void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        await using (var disposable = new AsyncDisposable())
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "await using");
        StringAssert.Contains(exception.Message, "synchronous");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithAwaitExpressionInBuildRenderTree_ThrowsUnsupportedImperativeRenderLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/await-card")]
                public class AwaitCard : ComponentBase, IVueComponent
                {
                    protected override async void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        await Task.Yield();
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "await");
        StringAssert.Contains(exception.Message, "synchronous");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithAwaitForEachInBuildRenderTree_ThrowsUnsupportedImperativeRenderLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/await-foreach-card")]
                public class AwaitForEachCard : ComponentBase, IVueComponent
                {
                    protected override async void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        await foreach (var item in Items)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, item);
                            builder.CloseElement();
                        }
                    }

                    private IAsyncEnumerable<string> Items => default!;
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedImperativeRenderLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "await foreach");
        StringAssert.Contains(exception.Message, "synchronous");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithReadonlyObjectGateLockStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/lock-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithConstructorNullableReadonlyGateLockStatement_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    private readonly object? _gate = new();

                    public LockCard()
                    {
                        _gate = null;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        lock (_gate!)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/lock-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "if (_gate == null)");
        StringAssert.Contains(artifact.SfcText, "throw new TypeError(\"obj\");");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLockThisStatementInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        lock (this)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/lock-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersConstantAddMarkupContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span><p>ok</p></section>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLocalAddMarkupContentCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersDeclarationInitializedNonConstAddMarkupContentCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersImmediatelyAssignedAddMarkupContentCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersReadonlyAddMarkupContentPropertyCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenSettableAddMarkupContentPropertyCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersReadonlyAddMarkupContentFieldCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenNonReadonlyAddMarkupContentFieldCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersMarkupStringCastFromUnwrittenNonReadonlyStringFieldCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
                {
                    private string _heroMarkup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, (MarkupString)_heroMarkup);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentStaticMarkupFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLocalFunctionStaticMarkupFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersFactoryBackedAddMarkupContentPropertyCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterIndependentStaticMarkupFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersStaticMarkupFactoryMethodWithOmittedOptionalParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(ignored) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersStaticMarkupFactoryMethodWithParamsParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => [props.title, \"suffix\"]);");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(values) in [__jazor$0]\">");
        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "<p>");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithScriptAddMarkupContent_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-card")]
                public class MarkupCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddMarkupContent(0, "<script>alert('x')</script>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "script");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersConstantMarkupStringAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithInlineEventMarkupStringAddContent_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, (MarkupString)"<section onclick=\"alert('x')\">safe</section>");
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "onclick");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersNewMarkupStringAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, new MarkupString("<section class=\"hero\"><span>safe</span><p>ok</p></section>"));
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLocalMarkupStringCarrierAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersReadonlyMarkupStringPropertyCarrierAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenSettableMarkupStringPropertyCarrierAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersReadonlyMarkupStringFieldCarrierAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenNonReadonlyMarkupStringFieldCarrierAddContent()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCurrentComponentMarkupStringFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLocalFunctionMarkupStringFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersFactoryBackedImmediateMarkupStringLocalCarrier()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterIndependentMarkupStringFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersMarkupStringFactoryMethodWithOmittedOptionalParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(ignored) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersMarkupStringFactoryMethodWithParamsParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/markup-string-card")]
                public class MarkupStringCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => [props.title, \"suffix\"]);");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(values) in [__jazor$0]\">");
        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithThrowStatementInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/throw-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "if (props.fail) {");
        StringAssert.Contains(artifact.SfcText, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(\"ready\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithForLoopAndContinueInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/for-continue-card")]
                public class ForContinueCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/for-continue-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNonCountStyleForLoopInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/for-multi-iterator-card")]
                public class ForMultiIteratorCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var total = 0;
                        for (var index = 0; index < Count; index++, total++)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, index);
                            builder.AddContent(2, total);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/for-multi-iterator-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const total = 0;");
        StringAssert.Contains(artifact.SfcText, "for (let index = 0; index < props.count; index++, total++)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNextFunctionForIteratorInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/for-next-card")]
                public class ForNextCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    private int Next(int value)
                        => value + 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var index = 0; index < Count; index = Next(index))
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/for-next-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "function next(value)");
        StringAssert.Contains(artifact.SfcText, "for (let index = 0; index < props.count; index = next(index))");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorVueForRange", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithMultiplicativeForIteratorInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/for-multiply-card")]
                public class ForMultiplyCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public int Step { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var index = 1; index < Count; index = index * Step)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/for-multiply-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "for (let index = 1; index < props.count; index = index * props.step)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorVueForRange", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLoopVariableDependentForStepInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/for-loop-local-step-card")]
                public class ForLoopLocalStepCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        for (var index = 1; index < Count; index += index)
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/for-loop-local-step-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "for (let index = 1; index < props.count; index += index)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorVueForRange", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDoWhileLoopInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/do-while-card")]
                public class DoWhileCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var index = 0;
                        do
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, index);
                            builder.CloseElement();
                            index++;
                        }
                        while (index < Count);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/do-while-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.SfcText, "index++;");
        StringAssert.Contains(artifact.SfcText, "while (index < props.count);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootWhileFalseInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/while-zero-card")]
                public class WhileZeroCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        while (false)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "unreachable");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/while-zero-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"false\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "unreachable");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDoWhileFalseInBuildRenderTree_LowersTemplateSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/do-while-once-card")]
                public class DoWhileOnceCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        do
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        while (false);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/do-while-once-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootRuntimeWhileConditionInBuildRenderTree_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/while-runtime-card")]
                public class WhileRuntimeCard : ComponentBase, IVueComponent
                {
                    private bool ShouldContinue() => false;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        while (ShouldContinue())
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "runtime");
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/while-runtime-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "function shouldContinue()");
        StringAssert.Contains(artifact.SfcText, "while (shouldContinue())");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDoWhileFalseContainingContinue_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/do-while-continue-card")]
                public class DoWhileContinueCard : ComponentBase, IVueComponent
                {
                    private bool ShouldSkip() => false;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        do
                        {
                            if (ShouldSkip())
                            {
                                continue;
                            }

                            builder.OpenElement(0, "section");
                            builder.AddContent(1, "ready");
                            builder.CloseElement();
                        }
                        while (false);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/do-while-continue-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "if (shouldSkip()) {");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "while (false);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDoWhileFalseNestedHelperTypeReference_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/do-while-helper-card")]
                public class DoWhileHelperCard : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        do
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, Helper.Text);
                            builder.CloseElement();
                        }
                        while (false);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual("components/do-while-helper-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
        StringAssert.Contains(artifact.SfcText, "while (false);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootWhileFalseContainingMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/while-zero-card")]
                public class WhileZeroCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var count = 0;
                        while (false)
                        {
                            count++;
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, count);
                            builder.CloseElement();
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/while-zero-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "while (false)");
        StringAssert.Contains(artifact.SfcText, "count++");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithRootDoWhileFalseContainingMutation_LowersRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/do-while-once-card")]
                public class DoWhileOnceCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var count = 0;
                        do
                        {
                            count++;
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, count);
                            builder.CloseElement();
                        }
                        while (false);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/do-while-once-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "count++");
        StringAssert.Contains(artifact.SfcText, "while (false);");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithDeclarativeSiblingsAroundWhileLoopInBuildRenderTree_LowersMixedRenderFunctionVueSfc()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/mixed-loop-card")]
                public class MixedLoopCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/mixed-loop-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"header\", null, \"start\"));");
        StringAssert.Contains(artifact.SfcText, "let index = 0;");
        StringAssert.Contains(artifact.SfcText, "while (index < props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"footer\", null, \"end\"));");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithAttributeSpreadSiblingAroundWhileLoop_InjectsMergeHelperForMixedRenderFunctionVueSfc()
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
                [ECMAScript.ECMAScriptModule("./components/mixed-spread-loop-card")]
                public class MixedSpreadLoopCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "header");
                        builder.AddAttribute(1, "class", "start");
                        builder.AddMultipleAttributes(2, AdditionalAttributes);
                        builder.CloseElement();

                        var index = 0;
                        while (index < Count)
                        {
                            builder.OpenElement(3, "section");
                            builder.AddContent(4, index);
                            builder.CloseElement();
                            index++;
                        }
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/mixed-spread-loop-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        StringAssert.Contains(artifact.SfcText, "function __jazorVueMergeAttributes(...sources) {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"header\", __jazorVueMergeAttributes({ \"class\": \"start\" }, props.additionalAttributes)));");
        StringAssert.Contains(artifact.SfcText, "while (index < props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithForEachLoopAndBreakInBuildRenderTree_LowersRenderFunctionVueSfc()
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
                [ECMAScript.ECMAScriptModule("./components/foreach-break-card")]
                public class ForEachBreakCard : ComponentBase, IVueComponent
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.AreEqual("components/foreach-break-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "for (let item of props.items)");
        StringAssert.Contains(artifact.SfcText, "break;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersTypedSlotOutletArgument_IntoTemplateAndSetupBinding()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.IsFalse(artifact.ScriptSetupText.Contains("import { computed } from \"vue\";", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"header\" :value=\"(props.count + 1)\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersTypedAddContentRenderFragment_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersTypedAddContentRenderFragmentLocalCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNonPrivateSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithHelperMutatedSettableCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private void MutateTemplate()
                    {
                        Template = item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(3, "strong");
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersAnalyzableCurrentComponentRenderFragmentPropertyCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenSettableCurrentComponentRenderFragmentPropertyCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersAnalyzableCurrentComponentRenderFragmentAutoPropertyCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersAnalyzableCurrentComponentRenderFragmentFieldCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersUnwrittenNonReadonlyCurrentComponentRenderFragmentFieldCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNonPrivateNonReadonlyCurrentComponentRenderFragmentFieldCarrier_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment shape");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersChainedCurrentComponentRenderFragmentPropertyCarrier_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithSelfReferentialCurrentComponentRenderFragmentPropertyCarrier_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "Template");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCyclicCurrentComponentRenderFragmentPropertyCarriers_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "TemplateA");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersZeroArgumentCurrentComponentRenderFragmentFactoryMethod_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersZeroArgumentLocalRenderFragmentFactoryMethod_IntoTemplateScopeWrapper()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersZeroArgumentCurrentComponentRenderFragmentFactoryMethod_ForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersZeroArgumentLocalRenderFragmentFactoryMethod_ForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethod_IntoNestedTemplateScopeWrappers()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodWithInParameter_IntoNestedTemplateScopeWrappers()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private RenderFragment<int> CreateTemplate(in string? title)
                        => CreateTemplateCore(title);

                    private RenderFragment<int> CreateTemplateCore(string? capturedTitle)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, capturedTitle);
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(capturedTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ capturedTitle }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersGenericParameterizedCurrentComponentRenderFragmentFactoryMethod_IntoNestedTemplateScopeWrappers()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodWithOmittedOptionalParameter_IntoNestedTemplateScopeWrappers()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodWithParamsParameter_IntoNestedTemplateScopeWrappers()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => [props.title, \"suffix\"]);");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(values) in [__jazor$0]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ values.length }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        var itemIndex = artifact.TemplateText.IndexOf("<template v-for=\"(item) in [42]\">", StringComparison.Ordinal);
        Assert.IsTrue(subtitleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(itemIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < titleIndex, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < itemIndex, artifact.TemplateText);
        Assert.IsTrue(titleIndex < itemIndex, artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodForTypedSlotTemplateUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        var slotIndex = artifact.TemplateText.IndexOf("<template #itemTemplate=\"item\">", StringComparison.Ordinal);
        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        Assert.IsTrue(slotIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex > slotIndex, artifact.TemplateText);
        Assert.IsTrue(titleIndex > subtitleIndex, artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedLocalRenderFragmentCarrierInitializedFromFactoryMethod_PreservingCapturedScopeOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedLocalRenderFragmentCarrierAssignedImmediatelyFromFactoryMethod_PreservingCapturedScopeOrder()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethod()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentPropertyCarrierInitializedFromFactoryMethodForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersAnalyzableCurrentComponentRenderFragmentAutoPropertyCarrierForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactoryMethodWithInParameter_ForTypedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private RenderFragment<int> CreateTemplate(in string? title)
                        => CreateTemplateCore(title);

                    private RenderFragment<int> CreateTemplateCore(string? capturedTitle)
                        => item => itemBuilder =>
                        {
                            itemBuilder.OpenElement(1, "span");
                            itemBuilder.AddContent(2, capturedTitle);
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(capturedTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ capturedTitle }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithCyclicCurrentComponentRenderFragmentFactoryMethods_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursive");
        StringAssert.Contains(exception.Issue.Message, "CreateTemplateA");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_ThrowsForRenderFragmentFactoryForwardingInParameterByReference()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                    private RenderFragment<int> CreateTemplate(in string? title)
                    {
                        ConsumeByRef(in title);
                        return item => itemBuilder =>
                        {
                            itemBuilder.AddContent(1, item);
                        };
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, CreateTemplate(Title), 42);
                    }

                    private static void ConsumeByRef(in string? value)
                    {
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "by-reference");
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCallableScopedSlotForwarding_ToNestedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ChildCardComponent>");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"context\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"itemTemplate\" v-bind=\"context\" />");
        Assert.IsFalse(artifact.TemplateText.Contains("props.itemTemplate("), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("{{ props.itemTemplate"), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersInheritedCallableScopedSlotForwarding_ToNestedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

                public abstract class ParentCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ParentCardBase
                {
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"context\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"itemTemplate\" v-bind=\"context\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersDefaultSlotForwarding_FromChildContent_ToNestedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "Page"));

        StringAssert.Contains(artifact.TemplateText, "<template #footer>");
        StringAssert.Contains(artifact.TemplateText, "<slot />");
        Assert.IsFalse(artifact.TemplateText.Contains("{{ props.childContent }}"), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersInlineNamedSlotTemplate_ToNestedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #header>");
        StringAssert.Contains(artifact.TemplateText, "<h1>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.title }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersDeclarationInitializedSetupProperty_IntoScriptSetupConstBinding()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/value-property-card")]
                public class ValuePropertyCard : ComponentBase, IVueComponent
                {
                    private string Prefix { get; } = "Count: ";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Prefix);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ValuePropertyCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "{{ prefix }}");
        StringAssert.Contains(artifact.ScriptSetupText, "const prefix = \"Count: \";");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("function prefix()", StringComparison.Ordinal), artifact.ScriptSetupText);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersCustomGetterPrivateSetterSetupPropertyWithoutLaterWrites_IntoScriptSetupFunction()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/custom-property-card")]
                public class CustomPropertyCard : ComponentBase, IVueComponent
                {
                    private string _prefix = "Count: ";

                    private string Prefix
                    {
                        get => _prefix.Trim();
                        set => _prefix = value;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Prefix);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "CustomPropertyCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "{{ prefix() }}");
        StringAssert.Contains(artifact.ScriptSetupText, "let _prefix = \"Count: \";");
        StringAssert.Contains(artifact.ScriptSetupText, "function prefix()");
        StringAssert.Contains(artifact.ScriptSetupText, "return _prefix.trim();");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_EmitsLifecycleSetupBindingsBeforeImmediateWatchRegistration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    private string Prefix { get; } = "Count: ";

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Prefix);
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

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "LifecycleCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const prefix = \"Count: \";");
        StringAssert.Contains(artifact.ScriptSetupText, "watch(() => [props.value], () => {");
        StringAssert.Contains(artifact.ScriptSetupText, "emit(\"update:value\", prefix);");
        var setupIndex = artifact.ScriptSetupText.IndexOf("const prefix = \"Count: \";", StringComparison.Ordinal);
        var watchIndex = artifact.ScriptSetupText.IndexOf("watch(() => [props.value], () => {", StringComparison.Ordinal);
        Assert.IsTrue(setupIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(watchIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(setupIndex < watchIndex, artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_EmitsLifecycleHelperSetupBindingsBeforeImmediateWatchRegistration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    private string Prefix { get; } = "Count: ";

                    private string FormatLabel()
                        => Prefix + Value;

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(FormatLabel());
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

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "LifecycleCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "const prefix = \"Count: \";");
        StringAssert.Contains(artifact.ScriptSetupText, "function formatLabel()");
        StringAssert.Contains(artifact.ScriptSetupText, "return (prefix + props.value);");
        StringAssert.Contains(artifact.ScriptSetupText, "watch(() => [props.value], () => {");
        StringAssert.Contains(artifact.ScriptSetupText, "emit(\"update:value\", formatLabel());");
        var prefixIndex = artifact.ScriptSetupText.IndexOf("const prefix = \"Count: \";", StringComparison.Ordinal);
        var helperIndex = artifact.ScriptSetupText.IndexOf("function formatLabel()", StringComparison.Ordinal);
        var watchIndex = artifact.ScriptSetupText.IndexOf("watch(() => [props.value], () => {", StringComparison.Ordinal);
        Assert.IsTrue(prefixIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(helperIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(watchIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(prefixIndex < helperIndex, artifact.ScriptSetupText);
        Assert.IsTrue(helperIndex < watchIndex, artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersSetupHelperWithInParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/setup-in-helper-card")]
                public class SetupInHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private static string FormatLabel(in int value)
                        => "Count: " + value;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatLabel(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "function formatLabel(value)");
        StringAssert.Contains(artifact.ScriptSetupText, "return (\"Count: \" + value);");
        StringAssert.Contains(artifact.ScriptSetupText, "formatLabel(props.value)");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazor$0 }}");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLifecycleHelperWithInParameter()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-in-helper-card")]
                public class LifecycleInHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    private static string FormatLabel(in int value)
                        => "Count: " + value;

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(FormatLabel(Value));
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, Value);
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "function formatLabel(value)");
        StringAssert.Contains(artifact.ScriptSetupText, "watch(() => [props.value], () => {");
        StringAssert.Contains(artifact.ScriptSetupText, "emit(\"update:value\", formatLabel(props.value));");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_ThrowsForSetupHelperForwardingInParameterByReference()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/setup-in-helper-escape-card")]
                public class SetupInHelperEscapeCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private static string FormatLabel(in int value)
                    {
                        ConsumeByRef(in value);
                        return "Count: " + value;
                    }

                    private static void ConsumeByRef(in int value)
                    {
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var current = Value;
                        builder.AddContent(0, FormatLabel(in current));
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "read-only 'in' value parameter");
        StringAssert.Contains(exception.Issue.Message, "by-reference invocation");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersSetParametersAsyncBaseThenMultipleEmitsInOrder()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ReadyChanged.InvokeAsync(Value > 0);
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

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "LifecycleCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "watch(() => [props.value], async () => {");
        var valueEmitIndex = artifact.ScriptSetupText.IndexOf("await emit(\"update:value\", props.value);", StringComparison.Ordinal);
        var readyEmitIndex = artifact.ScriptSetupText.IndexOf("await emit(\"readyChanged\", (props.value > 0));", StringComparison.Ordinal);
        Assert.IsTrue(valueEmitIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(readyEmitIndex >= 0, artifact.ScriptSetupText);
        Assert.IsTrue(valueEmitIndex < readyEmitIndex, artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersLibrarySlotNamesWithDots_ToVueSlotTemplates()
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
                [ECMAScript.ECMAScriptModule("./components/report-table")]
                public class ReportTable : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<VDataTableHeaderCellSlotContext>? HeaderSelect { get; set; }

                    [Parameter]
                    public RenderFragment? FooterPrepend { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDataTable>(0);
                        builder.AddAttribute(1, nameof(VDataTable.HeaderSelect), HeaderSelect);
                        builder.AddAttribute(2, nameof(VDataTable.FooterPrepend), FooterPrepend);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ReportTable");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #[`header.data-table-select`]=\"context\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"headerSelect\" v-bind=\"context\" />");
        StringAssert.Contains(artifact.TemplateText, "<template #[`footer.prepend`]>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"footerPrepend\" />");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersVuetifyStepperDynamicSlots_WithExactAuthoredSlotKeys()
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
                [ECMAScript.ECMAScriptModule("./components/vuetify-stepper-dynamic-sfc")]
                public class VuetifyStepperDynamicSfc : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VStepper>(0);
                        builder.AddAttribute(1, "header-item.profile", (RenderFragment<VStepperItemSlotContext>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(2, "strong");
                            slotBuilder.AddContent(3, item.Title);
                            slotBuilder.CloseElement();
                        }));
                        builder.AddAttribute(4, "item.profile", (RenderFragment<VStepperContentItemSlotContext>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(5, "section");
                            slotBuilder.AddContent(6, item.Value);
                            slotBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "VuetifyStepperDynamicSfc");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #[`header-item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.title }}");
        StringAssert.Contains(artifact.TemplateText, "<template #[`item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.value }}");
        Assert.IsFalse(artifact.TemplateText.Contains("<template #header-item=", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("<template #item=", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersVuetifyStepperVerticalDynamicSlots_WithExactAuthoredSlotKeys()
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
                [ECMAScript.ECMAScriptModule("./components/vuetify-stepper-vertical-dynamic-sfc")]
                public class VuetifyStepperVerticalDynamicSfc : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VStepperVertical>(0);
                        builder.AddAttribute(1, "header-item.profile", (RenderFragment<VStepperVerticalItemSlotContext>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(2, "strong");
                            slotBuilder.AddContent(3, item.Step);
                            slotBuilder.CloseElement();
                        }));
                        builder.AddAttribute(4, "item.profile", (RenderFragment<VStepperVerticalItemSlotContext>)((item) => (slotBuilder) =>
                        {
                            slotBuilder.OpenElement(5, "section");
                            slotBuilder.AddContent(6, item.Title);
                            slotBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "VuetifyStepperVerticalDynamicSfc");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #[`header-item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.step }}");
        StringAssert.Contains(artifact.TemplateText, "<template #[`item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.title }}");
        Assert.IsFalse(artifact.TemplateText.Contains("<template #header-item=", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("<template #item=", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersInlineTypedSlotTemplate_ToNestedScopedSlotTemplate()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "ItemTemplate", (RenderFragment<int>)((item) => (itemBuilder) =>
                        {
                            itemBuilder.OpenElement(2, "p");
                            itemBuilder.AddContent(3, item);
                            itemBuilder.CloseElement();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "ParentCard");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_LowersRazorGeneratedTypedSlotTemplate_WithNestedComponentEventBridgeAndConditional()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.RazorGeneratedTypedSlotSubtree.Tests",
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
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<ListCardComponent>");
        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"(item &gt; props.threshold)\">");
        StringAssert.Contains(artifact.TemplateText, "<ItemEditorComponent :modelValue=\"item\" @update:modelValue=\"(__value) =&gt; emit(&quot;valueChanged&quot;, __value)\" />");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
        StringAssert.Contains(artifact.SfcText, "import ItemEditorComponent from \"./item-editor.vue\";");
        StringAssert.Contains(artifact.SfcText, "import ListCardComponent from \"./list-card.vue\";");
        Assert.IsFalse(artifact.ScriptSetupText.Contains("__jazor$", StringComparison.Ordinal), artifact.ScriptSetupText);
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithNonCallableScopedSlotAttribute_ThrowsSlotContextMisuse()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.SlotContextMisuse, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ItemTemplate");
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.Tests",
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

    private static RazorVueCompilationContext CreateContext(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private sealed class FixedTemplateFrontend(RazorVueRenderFragment renderTree) : IRazorVueTemplateFrontend
    {
        public string Name => "Jazor.RazorVue.Test.FixedSfcTemplateFrontend";

        public RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
        {
            _ = context;
            _ = snapshot;
            return renderTree;
        }
    }

    private sealed class RazorDocumentEchoTemplateFrontend : IRazorVueTemplateFrontend
    {
        public string Name => "Jazor.RazorVue.Test.RazorDocumentEchoSfcTemplateFrontend";

        public RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
        {
            Assert.IsNotNull(snapshot.RazorIrCarrier);
            return new RazorVueRenderFragment(
                ImmutableArray.Create<RazorVueRenderNode>(
                    new RazorVueTextNode(snapshot.RazorIrCarrier.DocumentText.Trim(), ImmutableArray<RazorVueSourceOrigin>.Empty)));
        }
    }

    private static Compilation InjectCarrierCompilation(Compilation compilation, string documentPath, string documentText)
    {
        var componentTree = compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith(".razor.cs", StringComparison.Ordinal));
        var updatedSource = InjectCarrierAttribute(componentTree.ToString(), documentPath, documentText);
        return compilation.ReplaceSyntaxTree(
            componentTree,
            CSharpSyntaxTree.ParseText(updatedSource, path: componentTree.FilePath));
    }

    private static string InjectCarrierAttribute(string componentSource, string documentPath, string documentText)
    {
        if (componentSource.Contains("RazorVueRazorIrCarrierAttribute", StringComparison.Ordinal))
            return componentSource;

        const string marker = "[ECMAScript.ECMAScriptModule(\"./components/todo-app\")]";
        var replacement = string.Join(
            Environment.NewLine,
            marker,
            "    [Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute(",
            "        " + ToVerbatimLiteral(documentPath) + ",",
            "        " + ToVerbatimLiteral(JsonSerializer.Serialize(Array.Empty<object>())) + ",",
            "        " + ToVerbatimLiteral(documentText) + ")]");

        return componentSource.Replace(marker, replacement, StringComparison.Ordinal);
    }

    private static string ToVerbatimLiteral(string text)
        => "@\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static RazorVueRenderFragment CreateInjectedSectionTree(string text)
        => new(
            ImmutableArray.Create<RazorVueRenderNode>(
                new RazorVueElementNode(
                    "section",
                    null,
                    ImmutableArray<RazorVueAttributeEntry>.Empty,
                    new RazorVueRenderFragment(
                        ImmutableArray.Create<RazorVueRenderNode>(
                            new RazorVueTextNode(text, ImmutableArray<RazorVueSourceOrigin>.Empty))),
                    ImmutableArray<RazorVueSourceOrigin>.Empty)));

    private static VueSfcArtifactIdentity CreateInjectedContainerHomePageSfcIdentity(
        string contractMembers,
        string implementationAttributes,
        string implementationMembers,
        string renderStatements,
        string pageMembers = "")
    {
        var context = CreateContext(
            CreateInjectedContainerHomePageSource(
                contractMembers,
                implementationAttributes,
                implementationMembers,
                renderStatements,
                pageMembers));
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "HomePage");
        return CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot).Identity;
    }

    private static string CreateInjectedContainerHomePageSource(
        string contractMembers,
        string implementationAttributes,
        string implementationMembers,
        string renderStatements,
        string pageMembers)
        => $$"""
        using System;
        using ECMAScript.VueContract;
        using ECMAScript.VueContract.Descriptor;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

        [assembly: VueInject(
            typeof(Demo.Containers.NavShell),
            typeof(Demo.Implementations.ElementPlusNavShell))]

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Containers
        {
            [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
            public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
            {
        {{contractMembers}}
            }
        }

        namespace Demo.Implementations
        {
            [VueLibraryComponent("element-plus", "ElMenu")]
        {{implementationAttributes}}
            public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
            {
        {{implementationMembers}}
            }
        }

        namespace Demo.Pages
        {
            [ECMAScript.ECMAScriptModule("./pages/home-page")]
            public sealed class HomePage : ComponentBase, IVueComponent
            {
        {{pageMembers}}
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
        {{renderStatements}}
                    builder.CloseComponent();
                }
            }
        }
        """;

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
