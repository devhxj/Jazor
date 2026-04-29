using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Jazor.RazorVue.Test;

// Layering rule: RazorVue pipeline core must come from Jazor.RazorVue, not from Jazor.RazorVue.Analysis.
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
                public class CounterCard : ComponentBase, IVueComponent
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
                public class ChildCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
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
        CollectionAssert.Contains(artifact.Imports.ToArray(), "./components/child-card.mjs");
        StringAssert.Contains(artifact.ModuleCode, "import ChildCardComponent from \"./components/child-card.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(ChildCardComponent, null, null);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersLibraryComponentToNamedImport()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/toolbar-card")]
                public class ToolbarCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VBtn>(0);
                        builder.AddAttribute(1, nameof(VBtn.Text), "Save");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ToolbarCard");

        CollectionAssert.Contains(artifact.Imports.ToArray(), "vuetify/components");
        CollectionAssert.Contains(artifact.Styles.ToArray(), "vuetify/styles");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
        StringAssert.Contains(artifact.ModuleCode, "import { VBtn as VBtnComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(VBtnComponent, { \"text\": \"Save\" }, null);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_AggregatesLibraryImportsFromSameSpecifier()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/toolbar-card")]
                public class ToolbarCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenComponent<VBtn>(1);
                        builder.CloseComponent();
                        builder.OpenComponent<VCard>(2);
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var pipeline = new RazorVuePipeline();
        var catalog = pipeline.Execute(context);
        var artifact = catalog.Artifacts.Single(static artifact => artifact.ComponentName == "ToolbarCard");

        CollectionAssert.AreEquivalent(new[] { "vue", "vuetify/components" }, artifact.Imports.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
        StringAssert.Contains(artifact.ModuleCode, "import { VBtn as VBtnComponent, VCard as VCardComponent } from \"vuetify/components\";");
        Assert.AreEqual(1, artifact.ModuleCode.Split("from \"vuetify/components\";", StringSplitOptions.None).Length - 1, artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersCustomLibraryComponent_WithStylesAndPluginRequirements()
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

            namespace Demo.Authoring
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/button.css")]
                [VueLibraryPluginRequirement("demo-host")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Text { get; set; }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/library-host")]
                public class LibraryHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Authoring.DemoButton>(0);
                        builder.AddAttribute(1, nameof(Demo.Authoring.DemoButton.Text), "Save");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        CollectionAssert.Contains(artifact.Imports.ToArray(), "demo/components");
        StringAssert.Contains(artifact.ModuleCode, "import { DemoButton as DemoButtonComponent } from \"demo/components\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(DemoButtonComponent, { \"text\": \"Save\" }, null);");
        CollectionAssert.AreEqual(new[] { "demo/button.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-host" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_AggregatesNamedImports_AndNormalizesDuplicateCustomLibraryRequirements()
    {
        // Cross-component library requirements should be trimmed and deduplicated in the emitted host artifact.
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

            namespace Demo.Authoring
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/button.css")]
                [VueLibraryPluginRequirement("demo-host")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                }

                [VueLibraryComponent("demo/components", "DemoCard")]
                [VueLibraryStyle(" demo/button.css ")]
                [VueLibraryStyle("demo/card.css")]
                [VueLibraryPluginRequirement(" demo-host ")]
                [VueLibraryPluginRequirement("feature-flags")]
                public sealed class DemoCard : ComponentBase, IVueLibraryComponent
                {
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/library-host")]
                public class LibraryHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenComponent<Demo.Authoring.DemoButton>(1);
                        builder.CloseComponent();
                        builder.OpenComponent<Demo.Authoring.DemoCard>(2);
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        CollectionAssert.AreEquivalent(new[] { "vue", "demo/components" }, artifact.Imports.ToArray());
        StringAssert.Contains(artifact.ModuleCode, "import { DemoButton as DemoButtonComponent, DemoCard as DemoCardComponent } from \"demo/components\";");
        Assert.AreEqual(1, artifact.ModuleCode.Split("from \"demo/components\";", StringSplitOptions.None).Length - 1, artifact.ModuleCode);
        CollectionAssert.AreEqual(new[] { "demo/button.css", "demo/card.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-host", "feature-flags" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersCustomLibraryComponent_UsingExplicitAuthoringOverrides()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Jazor.RazorVue.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [VueLibraryProp(nameof(Label), VuePropKind.LibrarySpecific, Name = "buttonLabel", Required = true)]
                [VueLibraryEmit(nameof(OnSubmit), VueEmitKind.LibrarySpecific, Name = "onSaveNow")]
                [VueLibrarySlot(nameof(Footer), Name = "actions")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Label { get; set; }

                    [Parameter]
                    public EventCallback OnSubmit { get; set; }

                    [Parameter]
                    public RenderFragment? Footer { get; set; }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/library-host")]
                public class LibraryHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnSave { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Authoring.DemoButton>(0);
                        builder.AddAttribute(1, nameof(Demo.Authoring.DemoButton.Label), "Save");
                        builder.AddAttribute(2, nameof(Demo.Authoring.DemoButton.OnSubmit), OnSave);
                        builder.AddAttribute(3, nameof(Demo.Authoring.DemoButton.Footer), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "\"buttonLabel\": \"Save\"");
        StringAssert.Contains(artifact.ModuleCode, "\"onSaveNow\": props.onSave");
        StringAssert.Contains(artifact.ModuleCode, "actions: () => slots.default ? slots.default() : null");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyPackageEventAndModelBindings()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                public class EditorCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    [Parameter]
                    public string? ModelValue { get; set; }

                    [Parameter]
                    public EventCallback<string?> ModelValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenComponent<VBtn>(1);
                        builder.AddAttribute(2, nameof(VBtn.OnClick), OnClick);
                        builder.AddAttribute(3, nameof(VBtn.Text), "Save");
                        builder.CloseComponent();
                        builder.OpenComponent<VTextField>(4);
                        builder.AddAttribute(5, nameof(VTextField.Label), "Name");
                        builder.AddAttribute(6, nameof(VTextField.ModelValue), ModelValue);
                        builder.AddAttribute(7, nameof(VTextField.ModelValueChanged), ModelValueChanged);
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VBtn as VBtnComponent, VTextField as VTextFieldComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"onClick\": props.onClick");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.modelValue");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.modelValueChanged");
        CollectionAssert.Contains(artifact.Styles.ToArray(), "vuetify/styles");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyDialogActivatorScopedSlot()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/dialog-host")]
                public class DialogHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDialog>(0);
                        builder.AddAttribute(1, nameof(VDialog.Activator), Activator);
                        builder.AddContent(2, ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VDialog as VDialogComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "activator: (context) => props.activator(context)");
        StringAssert.Contains(artifact.ModuleCode, "default: () => slots.default ? slots.default() : null");
        CollectionAssert.Contains(artifact.Styles.ToArray(), "vuetify/styles");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyLayoutComposition()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/dashboard-card")]
                public class DashboardCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VContainer>(0);
                        builder.AddAttribute(1, nameof(VContainer.Fluid), true);
                        builder.OpenComponent<VRow>(2);
                        builder.AddAttribute(3, nameof(VRow.Align), "center");
                        builder.OpenComponent<VCol>(4);
                        builder.AddAttribute(5, nameof(VCol.Cols), 12);
                        builder.AddAttribute(6, nameof(VCol.Md), 6);
                        builder.OpenComponent<VCard>(7);
                        builder.OpenComponent<VCardTitle>(8);
                        builder.AddAttribute(9, nameof(VCardTitle.Text), "Dashboard");
                        builder.CloseComponent();
                        builder.OpenComponent<VCardText>(10);
                        builder.AddContent(11, "Ready");
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VCard as VCardComponent, VCardText as VCardTextComponent, VCardTitle as VCardTitleComponent, VCol as VColComponent, VContainer as VContainerComponent, VRow as VRowComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"fluid\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"align\": \"center\"");
        StringAssert.Contains(artifact.ModuleCode, "\"cols\": 12");
        StringAssert.Contains(artifact.ModuleCode, "\"md\": 6");
        StringAssert.Contains(artifact.ModuleCode, "\"text\": \"Dashboard\"");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyToolbarAndCheckboxComposition()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/preferences-card")]
                public class PreferencesCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Enabled { get; set; }

                    [Parameter]
                    public EventCallback<bool> EnabledChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VSheet>(0);
                        builder.AddAttribute(1, nameof(VSheet.Color), "surface");
                        builder.AddAttribute(2, nameof(VSheet.Rounded), true);
                        builder.AddAttribute(3, nameof(VSheet.Elevation), 2);
                        builder.OpenComponent<VToolbar>(4);
                        builder.AddAttribute(5, nameof(VToolbar.Color), "primary");
                        builder.AddAttribute(6, nameof(VToolbar.Flat), true);
                        builder.OpenComponent<VToolbarTitle>(7);
                        builder.AddAttribute(8, nameof(VToolbarTitle.Text), "Preferences");
                        builder.CloseComponent();
                        builder.OpenComponent<VSpacer>(9);
                        builder.CloseComponent();
                        builder.OpenComponent<VDivider>(10);
                        builder.AddAttribute(11, nameof(VDivider.Vertical), true);
                        builder.AddAttribute(12, nameof(VDivider.Inset), true);
                        builder.CloseComponent();
                        builder.OpenComponent<VCheckbox>(13);
                        builder.AddAttribute(14, nameof(VCheckbox.Label), "Enabled");
                        builder.AddAttribute(15, nameof(VCheckbox.ModelValue), Enabled);
                        builder.AddAttribute(16, nameof(VCheckbox.ModelValueChanged), EnabledChanged);
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VCheckbox as VCheckboxComponent, VDivider as VDividerComponent, VSheet as VSheetComponent, VSpacer as VSpacerComponent, VToolbar as VToolbarComponent, VToolbarTitle as VToolbarTitleComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"color\": \"surface\"");
        StringAssert.Contains(artifact.ModuleCode, "\"rounded\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"elevation\": 2");
        StringAssert.Contains(artifact.ModuleCode, "\"flat\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"text\": \"Preferences\"");
        StringAssert.Contains(artifact.ModuleCode, "\"vertical\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"inset\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.enabled");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.enabledChanged");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/notification-panel")]
                public class NotificationPanel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Enabled { get; set; }

                    [Parameter]
                    public EventCallback<bool> EnabledChanged { get; set; }

                    [Parameter]
                    public string? Notes { get; set; }

                    [Parameter]
                    public EventCallback<string?> NotesChanged { get; set; }

                    [Parameter]
                    public EventCallback OnPin { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VAlert>(0);
                        builder.AddAttribute(1, nameof(VAlert.Type), "info");
                        builder.AddAttribute(2, nameof(VAlert.Variant), "tonal");
                        builder.AddAttribute(3, nameof(VAlert.Closable), true);
                        builder.AddAttribute(4, nameof(VAlert.Text), "Saved");
                        builder.CloseComponent();

                        builder.OpenComponent<VList>(5);
                        builder.AddAttribute(6, nameof(VList.Density), "compact");
                        builder.AddAttribute(7, nameof(VList.Nav), true);
                        builder.OpenComponent<VListItem>(8);
                        builder.AddAttribute(9, nameof(VListItem.Title), "General");
                        builder.AddAttribute(10, nameof(VListItem.Subtitle), "Workspace defaults");
                        builder.OpenComponent<VChip>(11);
                        builder.AddAttribute(12, nameof(VChip.Color), "success");
                        builder.AddAttribute(13, nameof(VChip.Text), "Pinned");
                        builder.AddAttribute(14, nameof(VChip.OnClick), OnPin);
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();

                        builder.OpenComponent<VSwitch>(15);
                        builder.AddAttribute(16, nameof(VSwitch.Label), "Notifications");
                        builder.AddAttribute(17, nameof(VSwitch.ModelValue), Enabled);
                        builder.AddAttribute(18, nameof(VSwitch.ModelValueChanged), EnabledChanged);
                        builder.CloseComponent();

                        builder.OpenComponent<VTextarea>(19);
                        builder.AddAttribute(20, nameof(VTextarea.Label), "Notes");
                        builder.AddAttribute(21, nameof(VTextarea.Rows), 4);
                        builder.AddAttribute(22, nameof(VTextarea.ModelValue), Notes);
                        builder.AddAttribute(23, nameof(VTextarea.ModelValueChanged), NotesChanged);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VAlert as VAlertComponent, VChip as VChipComponent, VList as VListComponent, VListItem as VListItemComponent, VSwitch as VSwitchComponent, VTextarea as VTextareaComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"type\": \"info\"");
        StringAssert.Contains(artifact.ModuleCode, "\"variant\": \"tonal\"");
        StringAssert.Contains(artifact.ModuleCode, "\"closable\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"density\": \"compact\"");
        StringAssert.Contains(artifact.ModuleCode, "\"nav\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"title\": \"General\"");
        StringAssert.Contains(artifact.ModuleCode, "\"subtitle\": \"Workspace defaults\"");
        StringAssert.Contains(artifact.ModuleCode, "\"onClick\": props.onPin");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.enabled");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.enabledChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"rows\": 4");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.notes");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.notesChanged");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyFormAndStatusComposition()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/profile-form")]
                public class ProfileForm : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Role { get; set; }

                    [Parameter]
                    public EventCallback<string?> RoleChanged { get; set; }

                    [Parameter]
                    public bool MenuOpen { get; set; }

                    [Parameter]
                    public EventCallback<bool> MenuOpenChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VForm>(0);
                        builder.AddAttribute(1, nameof(VForm.FastFail), true);
                        builder.OpenComponent<VSelect>(2);
                        builder.AddAttribute(3, nameof(VSelect.Label), "Role");
                        builder.AddAttribute(4, nameof(VSelect.ModelValue), Role);
                        builder.AddAttribute(5, nameof(VSelect.ModelValueChanged), RoleChanged);
                        builder.AddAttribute(6, nameof(VSelect.Multiple), false);
                        builder.CloseComponent();
                        builder.OpenComponent<VMenu>(7);
                        builder.AddAttribute(8, nameof(VMenu.ModelValue), MenuOpen);
                        builder.AddAttribute(9, nameof(VMenu.ModelValueChanged), MenuOpenChanged);
                        builder.AddAttribute(10, nameof(VMenu.CloseOnContentClick), false);
                        builder.OpenComponent<VBadge>(11);
                        builder.AddAttribute(12, nameof(VBadge.Content), "3");
                        builder.AddAttribute(13, nameof(VBadge.Color), "error");
                        builder.OpenComponent<VAvatar>(14);
                        builder.AddAttribute(15, nameof(VAvatar.Color), "primary");
                        builder.AddAttribute(16, nameof(VAvatar.Size), "large");
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.CloseComponent();
                        builder.OpenComponent<VProgressLinear>(17);
                        builder.AddAttribute(18, nameof(VProgressLinear.Color), "success");
                        builder.AddAttribute(19, nameof(VProgressLinear.ModelValue), 64d);
                        builder.CloseComponent();
                        builder.OpenComponent<VProgressCircular>(20);
                        builder.AddAttribute(21, nameof(VProgressCircular.Color), "primary");
                        builder.AddAttribute(22, nameof(VProgressCircular.Indeterminate), true);
                        builder.CloseComponent();
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VAvatar as VAvatarComponent, VBadge as VBadgeComponent, VForm as VFormComponent, VMenu as VMenuComponent, VProgressCircular as VProgressCircularComponent, VProgressLinear as VProgressLinearComponent, VSelect as VSelectComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"fastFail\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.role");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.roleChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"closeOnContentClick\": false");
        StringAssert.Contains(artifact.ModuleCode, "\"content\": \"3\"");
        StringAssert.Contains(artifact.ModuleCode, "\"size\": \"large\"");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": 64");
        StringAssert.Contains(artifact.ModuleCode, "\"indeterminate\": true");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.Vuetify;
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
                [ECMAScript.ECMAScriptModule("./components/navigation-shell")]
                public class NavigationShell : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Query { get; set; }

                    [Parameter]
                    public EventCallback<string?> QueryChanged { get; set; }

                    [Parameter]
                    public string? Preference { get; set; }

                    [Parameter]
                    public EventCallback<string?> PreferenceChanged { get; set; }

                    [Parameter]
                    public string? ActiveTab { get; set; }

                    [Parameter]
                    public EventCallback<string?> ActiveTabChanged { get; set; }

                    [Parameter]
                    public bool SnackbarOpen { get; set; }

                    [Parameter]
                    public EventCallback<bool> SnackbarOpenChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VTabs>(0);
                        builder.AddAttribute(1, nameof(VTabs.Color), "primary");
                        builder.AddAttribute(2, nameof(VTabs.Grow), true);
                        builder.AddAttribute(3, nameof(VTabs.ModelValue), ActiveTab);
                        builder.AddAttribute(4, nameof(VTabs.ModelValueChanged), ActiveTabChanged);
                        builder.OpenComponent<VTab>(5);
                        builder.AddAttribute(6, nameof(VTab.Text), "Overview");
                        builder.AddAttribute(7, nameof(VTab.Value), "overview");
                        builder.CloseComponent();
                        builder.OpenComponent<VTab>(8);
                        builder.AddAttribute(9, nameof(VTab.Text), "History");
                        builder.AddAttribute(10, nameof(VTab.Value), "history");
                        builder.CloseComponent();
                        builder.CloseComponent();

                        builder.OpenComponent<VRadioGroup>(11);
                        builder.AddAttribute(12, nameof(VRadioGroup.Label), "Preference");
                        builder.AddAttribute(13, nameof(VRadioGroup.Inline), true);
                        builder.AddAttribute(14, nameof(VRadioGroup.ModelValue), Preference);
                        builder.AddAttribute(15, nameof(VRadioGroup.ModelValueChanged), PreferenceChanged);
                        builder.CloseComponent();

                        builder.OpenComponent<VAutocomplete>(16);
                        builder.AddAttribute(17, nameof(VAutocomplete.Label), "Search");
                        builder.AddAttribute(18, nameof(VAutocomplete.ModelValue), Query);
                        builder.AddAttribute(19, nameof(VAutocomplete.ModelValueChanged), QueryChanged);
                        builder.AddAttribute(20, nameof(VAutocomplete.Chips), true);
                        builder.CloseComponent();

                        builder.OpenComponent<VSnackbar>(21);
                        builder.AddAttribute(22, nameof(VSnackbar.ModelValue), SnackbarOpen);
                        builder.AddAttribute(23, nameof(VSnackbar.ModelValueChanged), SnackbarOpenChanged);
                        builder.AddAttribute(24, nameof(VSnackbar.Color), "success");
                        builder.AddAttribute(25, nameof(VSnackbar.Timeout), 2000);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "import { VAutocomplete as VAutocompleteComponent, VRadioGroup as VRadioGroupComponent, VSnackbar as VSnackbarComponent, VTab as VTabComponent, VTabs as VTabsComponent } from \"vuetify/components\";");
        StringAssert.Contains(artifact.ModuleCode, "\"grow\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.activeTab");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.activeTabChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"text\": \"Overview\"");
        StringAssert.Contains(artifact.ModuleCode, "\"value\": \"history\"");
        StringAssert.Contains(artifact.ModuleCode, "\"inline\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.preference");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.preferenceChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"chips\": true");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.query");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.queryChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"modelValue\": props.snackbarOpen");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:modelValue\": props.snackbarOpenChanged");
        StringAssert.Contains(artifact.ModuleCode, "\"timeout\": 2000");
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithNonCallableVuetifyDialogActivator_ReportsSlotContextMisuse()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/dialog-host")]
                public class DialogHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDialog>(0);
                        builder.AddAttribute(1, nameof(VDialog.Activator), "not-callable");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.SlotContextMisuse, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "Activator");
        StringAssert.Contains(exception.Issue.Message, "VDialog");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ReportsUnknownSlot()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/icon-host")]
                public class IconHost : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VIcon>(0);
                        builder.AddContent(1, "warn");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.UnknownSlot, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ChildContent");
        StringAssert.Contains(exception.Issue.Message, "VIcon");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithImplicitTypedLibraryDefaultSlot_ReportsSlotContextMisuse()
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.SlotContextMisuse, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ChildContent");
        StringAssert.Contains(exception.Issue.Message, "TypedContentPanel");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithDuplicateLibraryDefaultSlotAssignment_ReportsDuplicateSlotValue()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/dialog-host")]
                public class DialogHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDialog>(0);
                        builder.AddAttribute(1, nameof(VDialog.ChildContent), ChildContent);
                        builder.AddContent(2, ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.DuplicateSlotValue, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "ChildContent");
        StringAssert.Contains(exception.Issue.Message, "VDialog");
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithDuplicateLibraryNamedSlotAssignment_ReportsDuplicateSlotValue()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
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
                [ECMAScript.ECMAScriptModule("./components/dialog-host")]
                public class DialogHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<VDialog>(0);
                        builder.AddAttribute(1, nameof(VDialog.Activator), Activator);
                        builder.AddAttribute(2, nameof(VDialog.Activator), Activator);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.DuplicateSlotValue, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "Activator");
        StringAssert.Contains(exception.Issue.Message, "VDialog");
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
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
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
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
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
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_LowersInheritedDefaultSlotOutletInOwnTemplate()
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
                public abstract class ParentCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ParentCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, ChildContent);
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, slots.default ? slots.default() : null);");
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
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment? Header { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single(static artifact => artifact.ComponentName == "ParentCard");

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (context) => props.itemTemplate(context)");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedScopedSlotAttribute()
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
                public class InjectedCard : ComponentBase, IVueComponent
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
                public class ZetaCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./alpha")]
                public class AlphaCard : ComponentBase, IVueComponent
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
                public class TemplateOnlyCard : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_LowersInheritedBuildRenderTreeTemplate()
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
                public abstract class TemplateOnlyCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/template-only")]
                public class TemplateOnlyCard : TemplateOnlyCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnParametersSetLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], () => {");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"update:value\", props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnParametersSetAsyncLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        return ValueChanged.InvokeAsync(Value);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughOnParametersSetAsyncLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        return ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    protected override Task OnParametersSetAsync()
                    {
                        return base.OnParametersSetAsync();
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
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnInitializedLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
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
        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"update:value\", props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnInitializedAsyncLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
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
        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnAfterRenderLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(() => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"readyChanged\", currentFirstRender);");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        var emitIndex = artifact.ModuleCode.IndexOf("emit(\"readyChanged\", currentFirstRender);", StringComparison.Ordinal);
        var resetIndex = artifact.ModuleCode.IndexOf("firstRender = false;", StringComparison.Ordinal);
        Assert.IsTrue(emitIndex >= 0, artifact.ModuleCode);
        Assert.IsTrue(resetIndex >= 0, artifact.ModuleCode);
        Assert.IsTrue(emitIndex < resetIndex, artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersInheritedOnAfterRenderAsyncLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
        var resetIndex = artifact.ModuleCode.IndexOf("firstRender = false;", StringComparison.Ordinal);
        var emitIndex = artifact.ModuleCode.IndexOf("await emit(\"readyChanged\", currentFirstRender);", StringComparison.Ordinal);
        Assert.IsTrue(resetIndex >= 0, artifact.ModuleCode);
        Assert.IsTrue(emitIndex >= 0, artifact.ModuleCode);
        Assert.IsTrue(resetIndex < emitIndex, artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughOnAfterRenderAsyncLifecycle()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return base.OnAfterRenderAsync(firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForComponentBaseOnInitializedAsyncPassThrough()
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
                [ECMAScript.ECMAScriptModule("./components/init-card")]
                public class InitCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return base.OnInitializedAsync();
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("onMounted(async () => {", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForComponentBaseOnParametersSetAsyncPassThrough()
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
                [ECMAScript.ECMAScriptModule("./components/params-card")]
                public class ParamsCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        return base.OnParametersSetAsync();
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(() => [props.value], async () => {", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForComponentBaseOnAfterRenderAsyncPassThrough()
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
                [ECMAScript.ECMAScriptModule("./components/render-card")]
                public class RenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return base.OnAfterRenderAsync(firstRender);
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("onMounted(async () => {", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("onUpdated(async () => {", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("let firstRender = true;", StringComparison.Ordinal), artifact.ModuleCode);
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
                public class LogicSafeCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Value);
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
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForNoOpDisposeLifecycle()
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
                public class FullReloadCard : ComponentBase, IVueComponent
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForInheritedNoOpDisposeLifecycle()
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
                public abstract class FullReloadCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/full-reload")]
                public class FullReloadCard : FullReloadCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
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
                public class HashCard : ComponentBase, IVueComponent
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
                public class HashCard : ComponentBase, IVueComponent
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
                public class HashTemplateCard : ComponentBase, IVueComponent
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
                public class HashTemplateCard : ComponentBase, IVueComponent
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
                public class HashLogicCard : ComponentBase, IVueComponent
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
                public class HashLogicCard : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_NoOpLifecycleDoesNotChangeLogicHash()
    {
        var identityWithoutLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/noop-lifecycle-hash")]
                public class NoOpLifecycleHashCard : ComponentBase, IVueComponent
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

        var identityWithNoOpLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                [ECMAScript.ECMAScriptModule("./components/noop-lifecycle-hash")]
                public class NoOpLifecycleHashCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override void OnParametersSet()
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
            """)).Artifacts.Single().Identity;

        Assert.AreEqual(identityWithoutLifecycle.DescriptorHash, identityWithNoOpLifecycle.DescriptorHash);
        Assert.AreEqual(identityWithoutLifecycle.TemplateHash, identityWithNoOpLifecycle.TemplateHash);
        Assert.AreEqual(identityWithoutLifecycle.LogicHash, identityWithNoOpLifecycle.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, identityWithNoOpLifecycle.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_InheritedNoOpLifecycleDoesNotChangeLogicHash()
    {
        var identityWithoutLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                public abstract class InheritedNoOpLifecycleHashCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/inherited-noop-lifecycle-hash")]
                public class InheritedNoOpLifecycleHashCard : InheritedNoOpLifecycleHashCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var identityWithNoOpLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                public abstract class InheritedNoOpLifecycleHashCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/inherited-noop-lifecycle-hash")]
                public class InheritedNoOpLifecycleHashCard : InheritedNoOpLifecycleHashCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        Assert.AreEqual(identityWithoutLifecycle.DescriptorHash, identityWithNoOpLifecycle.DescriptorHash);
        Assert.AreEqual(identityWithoutLifecycle.TemplateHash, identityWithNoOpLifecycle.TemplateHash);
        Assert.AreEqual(identityWithoutLifecycle.LogicHash, identityWithNoOpLifecycle.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, identityWithNoOpLifecycle.HmrBoundaryKind);
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
                public class StaticBadge : ComponentBase, IVueComponent
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
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

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
                public class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
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
                public class PureStaticBadge : ComponentBase, IVueComponent
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
                public class Card : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? HeaderContent { get; set; }

                    [Parameter]
                    public RenderFragment? FooterActions { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_LowersComponentFieldUsedInTemplateExpressionIntoSetupScope()
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
                public class FieldCard : ComponentBase, IVueComponent
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "let _count = 1;");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"span\", null, _count);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersComponentMethodCalledInTemplateExpressionIntoSetupScope()
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
                public class MethodCard : ComponentBase, IVueComponent
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "function calculate()");
        StringAssert.Contains(artifact.ModuleCode, "return 42;");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"span\", null, calculate());");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSingleArgumentComponentMethodIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/method-card")]
                public class MethodCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private readonly string TitleText = "Count: ";

                    private string FormatTitle(int value)
                    {
                        return TitleText + value;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, FormatTitle(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "const titleText = \"Count: \";");
        StringAssert.Contains(artifact.ModuleCode, "function formatTitle(value)");
        StringAssert.Contains(artifact.ModuleCode, "return (titleText + value);");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"span\", null, formatTitle(props.value));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersTwoArgumentComponentMethodIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/method-card")]
                public class MethodCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private readonly string TitleText = "Count: ";

                    private string FormatTitle(int value, int scale)
                    {
                        return TitleText + (value * scale);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, FormatTitle(Value, 2));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "const titleText = \"Count: \";");
        StringAssert.Contains(artifact.ModuleCode, "function formatTitle(value, scale)");
        StringAssert.Contains(artifact.ModuleCode, "return (titleText + (value * scale));");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"span\", null, formatTitle(props.value, 2));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedNonParameterLifecyclePayload()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    public int InternalValue { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnParametersSet");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForNonParameterOnInitializedLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int InternalValue { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForThisQualifiedNonParameterOnInitializedLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int InternalValue { get; set; }

                    protected override void OnInitialized()
                    {
                        this.ValueChanged.InvokeAsync(this.InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForExpressionNonParameterOnInitializedLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int InternalValue { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(InternalValue + 1);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForNonParameterOnInitializedAsyncLifecyclePayload()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int InternalValue { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return ValueChanged.InvokeAsync(InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitializedAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForNonParameterOnAfterRenderLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRender");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForThisQualifiedNonParameterOnAfterRenderLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        this.ReadyChanged.InvokeAsync(this.InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRender");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForExpressionNonParameterOnAfterRenderLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(!InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRender");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForThisQualifiedNonParameterOnAfterRenderAsyncLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return this.ReadyChanged.InvokeAsync(this.InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersThisQualifiedExpressionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return this.ReadyChanged.InvokeAsync(!firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", !currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersIndependentFirstRenderStateForSyncAndAsyncAfterRenderHooks()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> SyncChanged { get; set; }

                    [Parameter]
                    public EventCallback<bool> AsyncChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        SyncChanged.InvokeAsync(firstRender);
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return AsyncChanged.InvokeAsync(firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(2, artifact.ModuleCode.Split("let firstRender = true;", StringSplitOptions.None).Length - 1, artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"syncChanged\", currentFirstRender);");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"asyncChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForCastFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync((bool)firstRender);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersConditionalFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender ? true : false);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", (currentFirstRender ? true : false));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersNestedConditionalFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender ? (firstRender ? true : false) : false);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", (currentFirstRender ? (currentFirstRender ? true : false) : false));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForChainedFirstRenderExpressionPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender.ToString().Length > 0);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersComparisonFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender == true);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", (currentFirstRender === true));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForCoalescedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        bool? alias = firstRender;
                        return ReadyChanged.InvokeAsync(alias ?? false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForHelperCallFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    private static bool Normalize(bool value) => value;

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(Normalize(firstRender));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForEqualsFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(object.Equals(firstRender, true));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForDeepMemberChainFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(new ReadyEnvelope(new ReadyState(firstRender)).State.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInstanceEqualsFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender.Equals(true));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalFunctionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        bool NormalizeReady(bool value) => value;
                        return ReadyChanged.InvokeAsync(NormalizeReady(firstRender));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForMethodReturnedDeepMemberChainFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    private static ReadyEnvelope BuildEnvelope(bool value)
                    {
                        return new ReadyEnvelope(new ReadyState(value));
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(BuildEnvelope(firstRender).State.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForDeepMemberChainEqualsFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(new ReadyEnvelope(new ReadyState(firstRender)).State.Value.Equals(true));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForHelperReturnedEqualsFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    private static ReadyState BuildReady(bool value)
                    {
                        return new ReadyState(value);
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(BuildReady(firstRender).Value.Equals(true));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForPropertyPatternFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(new ReadyEnvelope(new ReadyState(firstRender)) is { State.Value: true });
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForSwitchExpressionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender switch
                        {
                            true => true,
                            false => false,
                        });
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForPatternVarFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        object boxed = firstRender;
                        return ReadyChanged.InvokeAsync(boxed is bool ready && ready);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForHelperReturnedPropertyPatternFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    private static ReadyEnvelope BuildEnvelope(bool value)
                    {
                        return new ReadyEnvelope(new ReadyState(value));
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(BuildEnvelope(firstRender) is { State.Value: true });
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForObjectInitializerMemberAccessFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyState State { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(new ReadyEnvelope { State = new ReadyState(firstRender) }.State.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForTupleMemberAccessFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync((firstRender, new ReadyState(firstRender)).Item2.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForIsPatternTrueFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is true);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForIsPatternFalseFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForIsPatternNotFalseFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is not false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForIsPatternNotTrueFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is not true);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForIsPatternOrFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is true or false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForTypePatternFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is bool);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForObjectTypePatternFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is object);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForPatternCombinatorFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is true and not false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalLambdaFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        Func<bool, bool> normalizeReady = static value => value;
                        return ReadyChanged.InvokeAsync(normalizeReady(firstRender));
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForNullConditionalCoalescedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyState? State { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync((new ReadyEnvelope { State = new ReadyState(firstRender) }.State?.Value) ?? false);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForTupleDeconstructionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var pair = (firstRender, new ReadyState(firstRender));
                        var (_, readyState) = pair;
                        return ReadyChanged.InvokeAsync(readyState.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersThisQualifiedParenthesizedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return this.ReadyChanged.InvokeAsync((firstRender));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalAliasFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var alias = firstRender;
                        return ReadyChanged.InvokeAsync(alias);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForFirstRenderMemberAccessPayloadOnAfterRenderAsyncLifecycle()
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(new ReadyState(firstRender).Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForExpressionNonParameterOnAfterRenderAsyncLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(!InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForNonParameterOnAfterRenderAsyncLifecyclePayload()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    public bool InternalReady { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(InternalReady);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedNonParameterOnInitializedLifecyclePayload()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    public int InternalValue { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedNonParameterOnInitializedAsyncLifecyclePayload()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    public int InternalValue { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return ValueChanged.InvokeAsync(InternalValue);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitializedAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedNonParameterOnAfterRenderLifecyclePayload()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    public bool InternalReady { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(InternalReady);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRender");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedNonParameterOnAfterRenderAsyncLifecyclePayload()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    public bool InternalReady { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(InternalReady);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForArrayIndexerFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var readyStates = new[] { false, firstRender };
                        return ReadyChanged.InvokeAsync(readyStates[1]);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForListIndexedMemberChainFirstRenderPayloadOnAfterRenderAsyncLifecycle()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                public readonly struct ReadyState
                {
                    public ReadyState(bool value)
                    {
                        Value = value;
                    }

                    public bool Value { get; }
                }

                public sealed class ReadyEnvelope
                {
                    public ReadyEnvelope(ReadyState state)
                    {
                        State = state;
                    }

                    public ReadyState State { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var readyEnvelopes = new List<ReadyEnvelope>
                        {
                            new ReadyEnvelope(new ReadyState(false)),
                            new ReadyEnvelope(new ReadyState(firstRender)),
                        };
                        return ReadyChanged.InvokeAsync(readyEnvelopes[1].State.Value);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForArrayPatternCapturedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedLifecycleLowering()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override void OnInitialized()
                    {
                        var count = 1;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedAsyncLifecycleLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override Task OnInitializedAsync()
                    {
                        var count = 1;
                        return Task.CompletedTask;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitializedAsync");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedAsyncAfterRenderLifecycleLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var count = 1;
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRenderAsync");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedAfterRenderLifecycleLowering()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override void OnAfterRender(bool firstRender)
                    {
                        var count = 1;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnAfterRender");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedParametersSetLifecycleLowering()
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override void OnParametersSet()
                    {
                        var count = 1;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnParametersSet");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForInheritedUnsupportedParametersSetAsyncLifecycleLowering()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                public abstract class LifecycleCardBase : ComponentBase, IVueComponent
                {
                    protected override Task OnParametersSetAsync()
                    {
                        var count = 1;
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : LifecycleCardBase
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnParametersSetAsync");
        Assert.AreEqual("Demo.Components.LifecycleCardBase", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalFirstRenderPayloadOnInitializedLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnInitialized()
                    {
                        var firstRender = 1;
                        ValueChanged.InvokeAsync(firstRender);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitialized");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalFirstRenderPayloadOnInitializedAsyncLifecycle()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        var firstRender = 1;
                        return ValueChanged.InvokeAsync(firstRender);
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnInitializedAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalFirstRenderPayloadOnParametersSetLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        var firstRender = 1;
                        ValueChanged.InvokeAsync(firstRender);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnParametersSet");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForLocalFirstRenderPayloadOnParametersSetAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        var firstRender = 1;
                        return ValueChanged.InvokeAsync(firstRender);
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "OnParametersSetAsync");
        Assert.AreEqual("Demo.Components.LifecycleCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersExpressionFirstRenderPayloadOnAfterRenderLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(!firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(() => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"readyChanged\", !currentFirstRender);");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersExpressionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(!firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", !currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersParenthesizedFirstRenderPayloadOnAfterRenderLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync((firstRender));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(() => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersParenthesizedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync((firstRender));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersAwaitedExpressionFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        await ReadyChanged.InvokeAsync(!firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", !currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSyncAndAsyncOnAfterRenderLifecycle_WithIndependentFirstRenderFlags()
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
                [ECMAScript.ECMAScriptModule("./components/dual-ready-card")]
                public class DualReadyCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> OnReady { get; set; }

                    [Parameter]
                    public EventCallback<bool> OnReadyAsync { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        OnReady.InvokeAsync(firstRender);
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return OnReadyAsync.InvokeAsync(firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(2, artifact.ModuleCode.Split("let firstRender = true;", StringSplitOptions.None).Length - 1, artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"ready\", currentFirstRender);");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyAsync\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersAwaitedParenthesizedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        await ReadyChanged.InvokeAsync((firstRender));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersAwaitReturnThisQualifiedParenthesizedFirstRenderPayloadOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        await this.ReadyChanged.InvokeAsync((firstRender));
                        return;
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersReturnParenthesizedInvocationOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return (ReadyChanged.InvokeAsync((firstRender)));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersExpressionBodiedParenthesizedInvocationOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                        => (ReadyChanged.InvokeAsync(firstRender));

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersAwaitedParenthesizedInvocationOnAfterRenderAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        await (ReadyChanged.InvokeAsync(firstRender));
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "firstRender = false;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForNoOpDisposeAsyncLifecycle()
    {
        // A no-op DisposeAsync should not force a full reload because it does not
        // materialize runtime teardown logic.
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
                public class DisposableAsyncCard : ComponentBase, IVueComponent
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForInheritedNoOpDisposeAsyncLifecycle()
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
                public abstract class DisposableAsyncCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/disposable-async")]
                public class DisposableAsyncCard : DisposableAsyncCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSupportedSetupFieldAndHelperIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/helper-card")]
                public class HelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private readonly string TitleText = "Count: ";

                    public string FormatTitle()
                        => TitleText + Value;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatTitle());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "const titleText = \"Count: \";");
        StringAssert.Contains(artifact.ModuleCode, "function formatTitle()");
        StringAssert.Contains(artifact.ModuleCode, "return (titleText + props.value);");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, formatTitle());");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersTwoLevelHelperCompositionIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/helper-composition-card")]
                public class HelperCompositionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private string FormatOuter(int value)
                        => "Value: " + FormatInner(value);

                    private string FormatInner(int value)
                        => (value * 2).ToString();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatOuter(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "function formatOuter(value)");
        StringAssert.Contains(artifact.ModuleCode, "function formatInner(value)");
        StringAssert.Contains(artifact.ModuleCode, "return (\"Value: \" + formatInner(value));");
        StringAssert.Contains(artifact.ModuleCode, "return ((value * 2)).toString();");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, formatOuter(props.value));");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersTwoLevelHelperCompositionWithFieldAndPropsIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/helper-field-card")]
                public class HelperFieldCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private readonly string Prefix = "Count: ";

                    private string FormatLeaf(int value)
                        => Prefix + value;

                    private string FormatOuter()
                        => FormatLeaf(Value);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatOuter());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        StringAssert.Contains(artifact.ModuleCode, "const prefix = \"Count: \";");
        StringAssert.Contains(artifact.ModuleCode, "function formatLeaf(value)");
        StringAssert.Contains(artifact.ModuleCode, "function formatOuter()");
        StringAssert.Contains(artifact.ModuleCode, "return (prefix + value);");
        StringAssert.Contains(artifact.ModuleCode, "return formatLeaf(props.value);");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSharedInnerHelperOnlyOnceIntoSetupScope()
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
                [ECMAScript.ECMAScriptModule("./components/shared-helper-card")]
                public class SharedHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private string FormatLeaf(int value)
                        => (value + 1).ToString();

                    private string FormatA(int value)
                        => "A:" + FormatLeaf(value);

                    private string FormatB(int value)
                        => "B:" + FormatLeaf(value);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatA(Value));
                        builder.AddContent(2, FormatB(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

        Assert.AreEqual(1, artifact.ModuleCode.Split("function formatLeaf(value)", StringSplitOptions.None).Length - 1, artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "formatA(props.value)");
        StringAssert.Contains(artifact.ModuleCode, "formatB(props.value)");
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForThreeLevelHelperComposition()
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
                [ECMAScript.ECMAScriptModule("./components/three-level-card")]
                public class ThreeLevelCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private string FormatOuter(int value)
                        => "Value: " + FormatMiddle(value);

                    private string FormatMiddle(int value)
                        => "Middle: " + FormatInner(value);

                    private string FormatInner(int value)
                        => (value * 3).ToString();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatOuter(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "FormatInner");
        Assert.AreEqual("Demo.Components.ThreeLevelCard", exception.OwnerComponentFullName);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ThrowsCompilationIssueForAsyncInnerHelperMethod()
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
                [ECMAScript.ECMAScriptModule("./components/async-helper-card")]
                public class AsyncHelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private string FormatOuter(int value)
                        => "Value: " + FormatInnerAsync(value).Result;

                    private async Task<string> FormatInnerAsync(int value)
                    {
                        await Task.Delay(1);
                        return (value * 2).ToString();
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, FormatOuter(Value));
                        builder.CloseElement();
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "FormatInnerAsync");
        Assert.AreEqual("Demo.Components.AsyncHelperCard", exception.OwnerComponentFullName);
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
                public class LogicCard : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForInheritedLogicMethods()
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
                public abstract class LogicCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value)
                    {
                        return value * 2;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/logic-card")]
                public class LogicCard : LogicCardBase
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryForInheritedLogicMethodsWithInheritedNoOpLifecycle()
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
                public abstract class LogicCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override Task OnInitializedAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override void OnParametersSet()
                    {
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/logic-card")]
                public class LogicCard : LogicCardBase
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_InheritedLogicMethodsNoOpLifecycleDoesNotChangeLogicHash()
    {
        var identityWithoutNoOpLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                public abstract class LogicCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value)
                    {
                        return value * 2;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/logic-card")]
                public class LogicCard : LogicCardBase
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

        var identityWithNoOpLifecycle = new RazorVuePipeline().Execute(CreateContext(
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
                public abstract class LogicCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override Task OnInitializedAsync()
                    {
                        return Task.CompletedTask;
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/logic-card")]
                public class LogicCard : LogicCardBase
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

        Assert.AreEqual(identityWithoutNoOpLifecycle.DescriptorHash, identityWithNoOpLifecycle.DescriptorHash);
        Assert.AreEqual(identityWithoutNoOpLifecycle.TemplateHash, identityWithNoOpLifecycle.TemplateHash);
        Assert.AreEqual(identityWithoutNoOpLifecycle.LogicHash, identityWithNoOpLifecycle.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityWithoutNoOpLifecycle.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityWithNoOpLifecycle.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedLogicMethodsCoexistWithInheritedLifecyclePayloadChanges()
    {
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
                public abstract class HashLogicLifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-lifecycle")]
                public class HashLogicLifecycleCard : HashLogicLifecycleCardBase
                {
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
                public abstract class HashLogicLifecycleCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Count);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-lifecycle")]
                public class HashLogicLifecycleCard : HashLogicLifecycleCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedLogicMethodsCoexistWithInheritedLifecycleAwaitShapeChanges()
    {
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
                public abstract class HashLogicLifecycleAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-lifecycle-await-shape")]
                public class HashLogicLifecycleAwaitShapeCard : HashLogicLifecycleAwaitShapeCardBase
                {
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
                public abstract class HashLogicLifecycleAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public int Calculate(int value)
                    {
                        return value * 2;
                    }

                    protected override async Task OnInitializedAsync()
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-lifecycle-await-shape")]
                public class HashLogicLifecycleAwaitShapeCard : HashLogicLifecycleAwaitShapeCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
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
                public class MultiLifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<bool> OnReady { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        OnReady.InvokeAsync(firstRender);
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
    public void RazorVue_Pipeline_LowersPassThroughOnInitializedToSupportedBaseLifecycle()
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
                public abstract class ReadyCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnReady { get; set; }

                    protected override void OnInitialized()
                    {
                        OnReady.InvokeAsync();
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/ready-card-pass-through")]
                public class ReadyCard : ReadyCardBase
                {
                    protected override void OnInitialized()
                    {
                        base.OnInitialized();
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"ready\");");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughOnAfterRenderAsyncToSupportedBaseLifecycle()
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
                public abstract class AsyncReadyCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> OnReady { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return OnReady.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/async-ready-card-pass-through")]
                public class AsyncReadyCard : AsyncReadyCardBase
                {
                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return base.OnAfterRenderAsync(firstRender);
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "onMounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"ready\", currentFirstRender);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryWhenNoOpDisposeCoexistsWithSafeLifecycle()
    {
        // A no-op Dispose should not erase safe lifecycle lowering that already
        // keeps the component inside the logic-safe boundary.
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
                public class DisposeSafeMixCard : ComponentBase, IVueComponent
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryWhenInheritedNoOpDisposeCoexistsWithSafeLifecycle()
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
                public abstract class DisposeSafeMixCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public void Dispose()
                    {
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/dispose-safe-mix")]
                public class DisposeSafeMixCard : DisposeSafeMixCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryWhenInheritedNoOpDisposeAsyncCoexistsWithSafeLifecycle()
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
                public abstract class DisposeAsyncSafeMixCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public ValueTask DisposeAsync()
                    {
                        return default;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/dispose-async-safe-mix")]
                public class DisposeAsyncSafeMixCard : DisposeAsyncSafeMixCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersDisposeLifecycle_ToOnUnmountedEmit()
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
                [ECMAScript.ECMAScriptModule("./components/dispose-card")]
                public class DisposeCard : ComponentBase, IVueComponent, IDisposable
                {
                    [Parameter]
                    public EventCallback<int> ValueDisposed { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    public void Dispose()
                    {
                        ValueDisposed.InvokeAsync(Value);
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
        StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h, onUnmounted } from \"vue\";");
        StringAssert.Contains(artifact.ModuleCode, "onUnmounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"valueDisposed\", props.value);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersDisposeAsyncLifecycle_ToAsyncOnUnmountedEmit()
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
                [ECMAScript.ECMAScriptModule("./components/dispose-async-card")]
                public class DisposeAsyncCard : ComponentBase, IVueComponent, IAsyncDisposable
                {
                    [Parameter]
                    public EventCallback OnDisposed { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    public ValueTask DisposeAsync()
                    {
                        return new ValueTask(OnDisposed.InvokeAsync());
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
        StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h, onUnmounted } from \"vue\";");
        StringAssert.Contains(artifact.ModuleCode, "onUnmounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"disposed\");");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughDisposeToSupportedBaseLifecycle()
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
                public abstract class DisposeCardBase : ComponentBase, IVueComponent, IDisposable
                {
                    [Parameter]
                    public EventCallback<int> ValueDisposed { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    public void Dispose()
                    {
                        ValueDisposed.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/dispose-card-pass-through")]
                public class DisposeCard : DisposeCardBase
                {
                    public new void Dispose()
                    {
                        base.Dispose();
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
        StringAssert.Contains(artifact.ModuleCode, "onUnmounted(() => {");
        StringAssert.Contains(artifact.ModuleCode, "emit(\"valueDisposed\", props.value);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughDisposeAsyncToSupportedBaseLifecycle()
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
                public abstract class DisposeAsyncCardBase : ComponentBase, IVueComponent, IAsyncDisposable
                {
                    [Parameter]
                    public EventCallback<bool> DisposedChanged { get; set; }

                    public ValueTask DisposeAsync()
                    {
                        return new ValueTask(DisposedChanged.InvokeAsync(true));
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/dispose-async-card-pass-through")]
                public class DisposeAsyncCard : DisposeAsyncCardBase
                {
                    public new ValueTask DisposeAsync()
                    {
                        return base.DisposeAsync();
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "dispose");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "onUnmounted(async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"disposedChanged\", true);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_WithUnsupportedDisposeLifecycle_ReportsUnsupportedLifecycleLowering()
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
                [ECMAScript.ECMAScriptModule("./components/dispose-card")]
                public class DisposeCard : ComponentBase, IVueComponent, IDisposable
                {
                    [Parameter]
                    public int Value { get; set; }

                    public void Dispose()
                    {
                        Value++;
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
        Assert.IsNotNull(exception);
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "Dispose");
        StringAssert.Contains(exception.Issue.Message, "DisposeCard");
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
                public class HashLogicSigCard : ComponentBase, IVueComponent
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
                public class HashLogicSigCard : ComponentBase, IVueComponent
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
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryWhenInheritedBaseOnlySetParametersAsyncCoexistsWithNoOpSafeLifecycle()
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
                public abstract class SetParamsAsyncWithSafeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public override Task SetParametersAsync(ParameterView parameters)
                    {
                        return base.SetParametersAsync(parameters);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/set-params-async-with-safe")]
                public class SetParamsAsyncWithSafeCard : SetParamsAsyncWithSafeCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedLogicMethodSignatureChanges()
    {
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
                public abstract class HashLogicSigCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value)
                    {
                        return value * 2;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-sig")]
                public class HashLogicSigCard : HashLogicSigCardBase
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
                public abstract class HashLogicSigCardBase : ComponentBase, IVueComponent
                {
                    public int Calculate(int value, int scale)
                    {
                        return value * scale;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-logic-sig")]
                public class HashLogicSigCard : HashLogicSigCardBase
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

        Assert.AreEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.TemplateHash, identityB.TemplateHash);
        Assert.AreNotEqual(identityA.LogicHash, identityB.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedParameterLifecyclePayloadChanges()
    {
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
                public abstract class HashLifecyclePayloadCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-payload")]
                public class HashLifecyclePayloadCard : HashLifecyclePayloadCardBase
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Value);
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
                public abstract class HashLifecyclePayloadCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-payload")]
                public class HashLifecyclePayloadCard : HashLifecyclePayloadCardBase
                {
                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Count);
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedAfterRenderPayloadChanges()
    {
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
                public abstract class HashAfterRenderPayloadCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Ready { get; set; }

                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-after-render-payload")]
                public class HashAfterRenderPayloadCard : HashAfterRenderPayloadCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Ready);
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
                public abstract class HashAfterRenderPayloadCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Ready { get; set; }

                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(Ready);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-after-render-payload")]
                public class HashAfterRenderPayloadCard : HashAfterRenderPayloadCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Ready);
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        Assert.AreEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.TemplateHash, identityB.TemplateHash);
        Assert.AreNotEqual(identityA.LogicHash, identityB.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenSupportedLifecyclePayloadChanges()
    {
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
                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-payload")]
                public class HashLifecyclePayloadCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Value);
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
                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-payload")]
                public class HashLifecyclePayloadCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnParametersSet()
                    {
                        ValueChanged.InvokeAsync(Count);
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenSupportedLifecycleAwaitShapeChanges()
    {
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
                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-await-shape")]
                public class HashLifecycleAwaitShapeCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(Value);
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
                [ECMAScript.ECMAScriptModule("./components/hash-lifecycle-await-shape")]
                public class HashLifecycleAwaitShapeCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override async Task OnInitializedAsync()
                    {
                        await ValueChanged.InvokeAsync(Value);
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedSupportedLifecycleAwaitShapeChanges()
    {
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
                public abstract class HashInheritedLifecycleAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override void OnInitialized()
                    {
                        ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-inherited-lifecycle-await-shape")]
                public class HashInheritedLifecycleAwaitShapeCard : HashInheritedLifecycleAwaitShapeCardBase
                {
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
                public abstract class HashInheritedLifecycleAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override async Task OnInitializedAsync()
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-inherited-lifecycle-await-shape")]
                public class HashInheritedLifecycleAwaitShapeCard : HashInheritedLifecycleAwaitShapeCardBase
                {
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
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LogicHashChangesWhenInheritedAfterRenderAwaitShapeChanges()
    {
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
                public abstract class HashInheritedAfterRenderAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-inherited-after-render-await-shape")]
                public class HashInheritedAfterRenderAwaitShapeCard : HashInheritedAfterRenderAwaitShapeCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        var identityB = new RazorVuePipeline().Execute(CreateContext(
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
                public abstract class HashInheritedAfterRenderAwaitShapeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        await ReadyChanged.InvokeAsync(firstRender);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/hash-inherited-after-render-await-shape")]
                public class HashInheritedAfterRenderAwaitShapeCard : HashInheritedAfterRenderAwaitShapeCardBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """)).Artifacts.Single().Identity;

        Assert.AreEqual(identityA.DescriptorHash, identityB.DescriptorHash);
        Assert.AreEqual(identityA.TemplateHash, identityB.TemplateHash);
        Assert.AreNotEqual(identityA.LogicHash, identityB.LogicHash);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityA.HmrBoundaryKind);
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, identityB.HmrBoundaryKind);
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
                public class AsyncSafeLifecycleCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<bool> OnReady { get; set; }

                    protected override Task OnInitializedAsync()
                    {
                        return this.ValueChanged.InvokeAsync(this.Value);
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return this.OnReady.InvokeAsync(firstRender);
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
                public class AsyncParametersSetCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    protected override Task OnParametersSetAsync()
                    {
                        return this.ValueChanged.InvokeAsync(this.Value);
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
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForConstantTrueShouldRenderLifecycle()
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
                public class ShouldRenderCard : ComponentBase, IVueComponent
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
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForInheritedConstantTrueShouldRenderLifecycle()
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
                public abstract class ShouldRenderCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/should-render")]
                public class ShouldRenderCard : ShouldRenderCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForComponentBaseShouldRenderPassThrough()
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
                public class ShouldRenderCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        return base.ShouldRender();
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
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForBaseOnlySetParametersAsyncLifecycle()
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
                public class SetParametersAsyncCard : ComponentBase, IVueComponent
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForInheritedBaseOnlySetParametersAsyncLifecycle()
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
                public abstract class SetParametersAsyncCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/set-parameters-async")]
                public class SetParametersAsyncCard : SetParametersAsyncCardBase
                {
                }
            }
            """);

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersBaseThenEmitSetParametersAsyncIntoWatchLifecycle()
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
                public class SetParametersAsyncCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
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
        StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h, watch } from \"vue\";");
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersPassThroughSetParametersAsyncToSupportedBaseEmitLifecycle()
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
                public abstract class SetParametersAsyncCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/set-parameters-async-pass-through")]
                public class SetParametersAsyncCard : SetParametersAsyncCardBase
                {
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
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesBaseEmitPlusDerivedEmitSetParametersAsyncAsFullReload()
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
                public abstract class SetParametersAsyncCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/set-parameters-async-duplicate-emit")]
                public class SetParametersAsyncCard : SetParametersAsyncCardBase
                {
                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
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
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(() => [props.value], async () => {", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryWhenConstantTrueShouldRenderCoexistsWithSafeLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/should-render-with-safe")]
                public class ShouldRenderWithSafeCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnReady { get; set; }

                    protected override void OnInitialized()
                    {
                        OnReady.InvokeAsync();
                    }

                    protected override bool ShouldRender()
                    {
                        return true;
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryWhenUnsupportedShouldRenderCoexistsWithSafeLifecycle()
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
                public abstract class ShouldRenderWithSafeCardBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    protected override bool ShouldRender()
                    {
                        return Value > 0;
                    }
                }

                [ECMAScript.ECMAScriptModule("./components/should-render-with-safe")]
                public class ShouldRenderWithSafeCard : ShouldRenderWithSafeCardBase
                {
                    protected override void OnInitialized()
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
    public void RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryWhenBaseOnlySetParametersAsyncCoexistsWithNoOpSafeLifecycle()
    {
        // Base-only SetParametersAsync does not add runtime behavior. Combined with
        // a no-op OnParametersSet, the component should remain TemplateOnly.
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
                public class SetParamsAsyncWithSafeCard : ComponentBase, IVueComponent
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
        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersSupportedSetParametersAsyncBaseThenEmitLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/set-parameters-async-emit")]
                public class SetParametersAsyncEmitCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        await ValueChanged.InvokeAsync(Value);
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
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForUnsupportedSetParametersAsyncLifecycle()
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
                [ECMAScript.ECMAScriptModule("./components/set-parameters-async-unsupported")]
                public class SetParametersAsyncUnsupportedCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        Value++;
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
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForUnsupportedShouldRenderLifecycle()
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesLogicSafeBoundaryWhenInheritedConstantTrueShouldRenderCoexistsWithSafeLifecycle()
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
                public abstract class ShouldRenderWithSafeCardBase : ComponentBase, IVueComponent
                {
                    protected override bool ShouldRender() => true;
                }

                [ECMAScript.ECMAScriptModule("./components/should-render-with-safe")]
                public class ShouldRenderWithSafeCard : ShouldRenderWithSafeCardBase
                {
                    [Parameter]
                    public EventCallback OnReady { get; set; }

                    protected override void OnInitialized()
                    {
                        OnReady.InvokeAsync();
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

        var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
        StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
    }

    [TestMethod]
    public void RazorVue_Pipeline_ClassifiesFullReloadBoundaryForInheritedUnsupportedShouldRenderLifecycle()
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
                public abstract class ShouldRenderCardBase : ComponentBase, IVueComponent
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

                [ECMAScript.ECMAScriptModule("./components/should-render")]
                public class ShouldRenderCard : ShouldRenderCardBase
                {
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
                public class PropsNoTemplateCard : ComponentBase, IVueComponent
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

    private static RazorVueCompilationContext CreateContextAllowingCompilerErrors(string source)
    {
        var compilation = CreateCompilation(source);
        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create(
            assemblyName: "RazorVue.Pipeline.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: CreateReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static IEnumerable<MetadataReference> CreateReferences()
        => RazorVueMetadataReferences.Create();
}

