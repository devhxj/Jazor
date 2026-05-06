using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
        StringAssert.Contains(artifact.SfcText, "const props = defineProps<");
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

        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorVueSfcBinding0\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"item in __jazorVueSfcBinding1\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
        StringAssert.Contains(artifact.ScriptSetupText, "import { computed } from \"vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding0 = computed(() => props.visible);");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding1 = computed(() => props.items);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
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
    public void RazorVue_SfcArtifactFactory_LowersPartialRazorGeneratedBuildRenderTreeShape()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.SfcArtifact.PartialRazor.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
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
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding0 = computed(() => doubleCount());");
        StringAssert.Contains(artifact.TemplateText, "{{ __jazorVueSfcBinding0 }}");
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

        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding0 = computed(() => (props.title + \"!\"));");
        StringAssert.Contains(artifact.TemplateText, "<section :title=\"__jazorVueSfcBinding0\" />");
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

        StringAssert.Contains(artifact.TemplateText, "<VTextField :modelValue=\"props.modelValue\" @update:modelValue=\"__jazorVueSfcBinding0\" />");
        StringAssert.Contains(artifact.ScriptSetupText, "const props = defineProps<{ modelValue?: any }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const emit = defineEmits<{ (event: \"update:modelValue\", payload?: any): void }>();");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding0 = computed(() => (__value) => emit(\"update:modelValue\", __value));");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithComponentLocalVariableInTemplate_ThrowsUnsupportedTemplateEncoding()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedTemplateEncoding, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "cannot hoist component-local expression");
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

        StringAssert.Contains(artifact.ScriptSetupText, "import { computed } from \"vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorVueSfcBinding0 = computed(() => (props.count + 1));");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"header\" :value=\"__jazorVueSfcBinding0\" />");
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

    private static RazorVueRenderFragment CreateInjectedSectionTree(string text)
        => new(
            ImmutableArray.Create<RazorVueRenderNode>(
                new RazorVueElementNode(
                    "section",
                    ImmutableArray<RazorVueAttributeNode>.Empty,
                    new RazorVueRenderFragment(
                        ImmutableArray.Create<RazorVueRenderNode>(
                            new RazorVueTextNode(text, ImmutableArray<RazorVueSourceOrigin>.Empty))),
                    ImmutableArray<RazorVueSourceOrigin>.Empty)));
}

