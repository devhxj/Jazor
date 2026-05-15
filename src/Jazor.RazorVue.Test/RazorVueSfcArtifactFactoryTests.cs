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

        var artifact = new RazorVueSfcPipeline(new RazorDocumentEchoTemplateFrontend()).Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.SfcText, "@page \"/todo\"");
        StringAssert.Contains(artifact.SfcText, "Hello from default SFC pipeline");
        Assert.IsFalse(artifact.SfcText.Contains("Hello from generated render tree", StringComparison.Ordinal), artifact.SfcText);
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
                ImmutableArray<RazorVueAttributeEntry>.Empty,
                ImmutableArray<RazorVueComponentSlotTemplateNode>.Empty,
                new RazorVueRenderFragment(
                [
                    new RazorVueElementNode(
                        "span",
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
        StringAssert.Contains(artifact.ScriptSetupText, "const props = defineProps<{ modelValue?: any }>();");
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

        StringAssert.Contains(artifact.TemplateText, "<template #default=\"__jazorSlotContext\">");
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
    public void RazorVue_SfcArtifactFactory_WithComponentLocalVariableDeclarationInBuildRenderTree_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "local variable declaration");
        StringAssert.Contains(exception.Issue.Message, "localTitle");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithLoopBodyComponentLocalVariableDeclarationInBuildRenderTree_ThrowsCanonicalizationFailed()
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "local variable declaration");
        StringAssert.Contains(exception.Issue.Message, "decorated");
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
    public void RazorVue_SfcArtifactFactory_WithTypedSlotTemplateComponentLocalVariableInTemplate_ThrowsUnsupportedTemplateEncoding()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.UnsupportedTemplateEncoding, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "component-local expression");
        StringAssert.Contains(exception.Issue.Message, "decorated");
    }

    [TestMethod]
    public void RazorVue_SfcArtifactFactory_WithConditionalReturnInBuildRenderTree_ThrowsCanonicalizationFailed()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
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
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "return");
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
        StringAssert.Contains(artifact.TemplateText, "{{ item.Title }}");
        StringAssert.Contains(artifact.TemplateText, "<template #[`item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.Value }}");
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
        StringAssert.Contains(artifact.TemplateText, "{{ item.Step }}");
        StringAssert.Contains(artifact.TemplateText, "<template #[`item.profile`]=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "{{ item.Title }}");
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
}
