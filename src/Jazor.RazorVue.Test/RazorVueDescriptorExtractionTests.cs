using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueDescriptorExtractionTests
{
    [TestMethod]
    public void RazorVue_Context_DiscoversVueComponentCandidates()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public static class LegacyModule
            {
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected void BuildRenderTree(object builder)
                    {
                    }
                }
            }
            """);

        var candidates = context.DiscoverComponentCandidates();
        Assert.HasCount(1, candidates);
        Assert.AreEqual("Counter", candidates[0].ComponentSymbol.Name);
        Assert.AreEqual(RazorVueEntryKind.RazorVueComponent, candidates[0].EntryKind);
        Assert.IsNotNull(candidates[0].BuildRenderTreeMethod);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ParameterEventCallbackAndSlots_AreProjectedIntoDescriptor()
    {
        var snapshot = CreateSingleSnapshot(
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
                [ECMAScript.ECMAScriptModule("./components/counter")]
                public class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<int> OnSave { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    [Parameter]
                    public RenderFragment<string>? Header { get; set; }
                }
            }
            """);

        var descriptor = snapshot.Descriptor;
        Assert.AreEqual("Counter", descriptor.Name);
        Assert.AreEqual("Demo.Components.Counter", descriptor.FullName);
        Assert.AreEqual(VueComponentSourceKind.UserComponent, descriptor.SourceKind);
        Assert.AreEqual("Demo.Components", descriptor.ResolutionNamespace);
        Assert.AreEqual("./components/counter.mjs", descriptor.ImportSpecifier);
        Assert.AreEqual("default", descriptor.ExportName);

        var titleProp = descriptor.Props.Single(prop => prop.PublicName == "Title");
        Assert.AreEqual("title", titleProp.Name);
        Assert.AreEqual("string?", titleProp.TypeName);
        Assert.IsFalse(titleProp.AcceptsBinding);
        Assert.AreEqual(VuePropKind.Normal, titleProp.Kind);

        var valueProp = descriptor.Props.Single(prop => prop.PublicName == "Value");
        Assert.AreEqual("value", valueProp.Name);
        Assert.IsTrue(valueProp.AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, valueProp.Kind);

        var onSaveEmit = descriptor.Emits.Single(emit => emit.RazorAlias == "OnSave");
        Assert.AreEqual("save", onSaveEmit.Name);
        Assert.AreEqual("int", onSaveEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.Normal, onSaveEmit.Kind);

        var valueChangedEmit = descriptor.Emits.Single(emit => emit.RazorAlias == "ValueChanged");
        Assert.AreEqual("update:value", valueChangedEmit.Name);
        Assert.AreEqual("int", valueChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, valueChangedEmit.Kind);

        var defaultSlot = descriptor.Slots.Single(slot => slot.IsDefault);
        Assert.AreEqual("default", defaultSlot.Name);
        Assert.IsEmpty(defaultSlot.Parameters);

        var headerSlot = descriptor.Slots.Single(slot => slot.Name == "header");
        Assert.IsFalse(headerSlot.IsDefault);
        Assert.HasCount(1, headerSlot.Parameters);
        Assert.AreEqual("context", headerSlot.Parameters[0].Name);
        Assert.AreEqual("string", headerSlot.Parameters[0].TypeName);

        Assert.AreEqual("Counter", snapshot.ComponentSymbol.Name);
        Assert.HasCount(1, snapshot.Origins);
        Assert.AreEqual(RazorVueOriginKind.Component, snapshot.Origins[0].OriginKind);
        Assert.AreEqual(RazorVueMappingQuality.MappedFromGenerated, snapshot.Origins[0].MappingQuality);
        Assert.AreEqual(RazorVueOriginProvenance.GeneratedSyntaxLocation, snapshot.Origins[0].Provenance);
    }

    [TestMethod]
    public void RazorVue_Context_DiscoversLibraryComponentDescriptors_FromStubMetadata()
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/styles")]
                [VueLibraryPluginRequirement("demo-host")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Text { get; set; }

                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }
            """);

        var descriptors = context.DiscoverLibraryComponents();
        var descriptor = descriptors.Single(static descriptor => descriptor.FullName == "Demo.Ui.Custom.DemoButton");
        Assert.AreEqual("DemoButton", descriptor.Name);
        Assert.AreEqual("Demo.Ui.Custom.DemoButton", descriptor.FullName);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, descriptor.SourceKind);
        Assert.AreEqual("Demo.Ui.Custom", descriptor.ResolutionNamespace);
        Assert.AreEqual("demo/components", descriptor.ImportSpecifier);
        Assert.AreEqual("DemoButton", descriptor.ExportName);
        CollectionAssert.AreEqual(new[] { "demo/styles" }, descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-host" }, descriptor.PluginRequirements.ToArray());

        var textProp = descriptor.Props.Single(prop => prop.PublicName == "Text");
        Assert.AreEqual("text", textProp.Name);

        var onClickEmit = descriptor.Emits.Single(emit => emit.RazorAlias == "OnClick");
        Assert.AreEqual("click", onClickEmit.Name);

        var defaultSlot = descriptor.Slots.Single(slot => slot.IsDefault);
        Assert.AreEqual("default", defaultSlot.Name);
    }

    [TestMethod]
    public void RazorVue_Snapshot_CaptureUnmatchedValuesParameter_IsProjectedIntoDescriptor()
    {
        var snapshot = CreateSingleSnapshot(
            """
            using System;
            using System.Collections.Generic;
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                }
            }
            """);

        var descriptor = snapshot.Descriptor;
        var additionalAttributes = descriptor.Props.Single(static prop => prop.PublicName == "AdditionalAttributes");
        Assert.IsTrue(additionalAttributes.CaptureUnmatchedValues);
    }

    [TestMethod]
    public void RazorVue_Context_InvalidComponentCaptureUnmatchedValuesDeclaration_ThrowsStructuredIssue()
    {
        var context = CreateContext(
            """
            using System;
            using System.Collections.Generic;
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
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyList<string>? AdditionalAttributes { get; set; }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => context.CreateSemanticSnapshots());
        Assert.AreEqual(RazorVueIssueCode.InvalidComponentDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "CaptureUnmatchedValues");
        StringAssert.Contains(exception.Issue.Message, "AdditionalAttributes");
    }

    [TestMethod]
    public void RazorVue_Context_DiscoversVuetifyPackageLibraryDescriptors_FromReferencedAssembly()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/host-card")]
                public class HostCard : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var descriptors = context.DiscoverLibraryComponents();

        var vuetifyDescriptors = descriptors
            .Where(static descriptor => descriptor.ResolutionNamespace == "ECMAScript.Vuetify")
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "VAlert", "VAutocomplete", "VAvatar", "VBadge", "VBreadcrumbs", "VBtn", "VCard", "VCardText", "VCardTitle", "VCheckbox", "VChip", "VCol", "VContainer", "VDataTable", "VDialog", "VDivider", "VForm", "VIcon", "VImg", "VList", "VListItem", "VMenu", "VPagination", "VProgressCircular", "VProgressLinear", "VRadioGroup", "VRow", "VSelect", "VSheet", "VSnackbar", "VSpacer", "VSwitch", "VTab", "VTabs", "VTextField", "VTextarea", "VToolbar", "VToolbarTitle", "VTooltip" },
            vuetifyDescriptors
                .Select(static descriptor => descriptor.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());

        var textField = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTextField");
        Assert.AreEqual("vuetify/components", textField.ImportSpecifier);
        Assert.AreEqual("VTextField", textField.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, textField.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, textField.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", textField.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textField.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var checkbox = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCheckbox");
        Assert.AreEqual("vuetify/components", checkbox.ImportSpecifier);
        Assert.AreEqual("VCheckbox", checkbox.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, checkbox.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, checkbox.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", checkbox.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", checkbox.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var dialog = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDialog");
        var activator = dialog.Slots.Single(static slot => slot.Name == "activator");
        Assert.HasCount(1, activator.Parameters);
        Assert.AreEqual("context", activator.Parameters[0].Name);
        Assert.AreEqual("ECMAScript.Vuetify.VDialogActivatorContext", activator.Parameters[0].TypeName);

        var column = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCol");
        CollectionAssert.AreEqual(new[] { "vuetify" }, column.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, column.StyleDependencies.ToArray());
        Assert.AreEqual("cols", column.Props.Single(static prop => prop.PublicName == "Cols").Name);
        Assert.AreEqual("md", column.Props.Single(static prop => prop.PublicName == "Md").Name);

        var toolbar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VToolbar");
        CollectionAssert.AreEqual(new[] { "vuetify" }, toolbar.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, toolbar.StyleDependencies.ToArray());
        Assert.AreEqual("color", toolbar.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", toolbar.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.IsTrue(toolbar.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var textarea = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTextarea");
        Assert.AreEqual("rows", textarea.Props.Single(static prop => prop.PublicName == "Rows").Name);
        Assert.AreEqual("modelValue", textarea.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textarea.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var toggle = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSwitch");
        Assert.AreEqual("modelValue", toggle.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", toggle.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var select = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSelect");
        Assert.AreEqual("modelValue", select.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", select.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("multiple", select.Props.Single(static prop => prop.PublicName == "Multiple").Name);

        var autocomplete = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAutocomplete");
        Assert.AreEqual("modelValue", autocomplete.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", autocomplete.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("chips", autocomplete.Props.Single(static prop => prop.PublicName == "Chips").Name);

        var list = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VList");
        Assert.AreEqual("density", list.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.IsTrue(list.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var listItem = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VListItem");
        Assert.AreEqual("title", listItem.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("subtitle", listItem.Props.Single(static prop => prop.PublicName == "Subtitle").Name);
        Assert.IsTrue(listItem.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var alert = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAlert");
        Assert.AreEqual("type", alert.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("variant", alert.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.IsTrue(alert.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var chip = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VChip");
        Assert.AreEqual("click", chip.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.IsTrue(chip.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var form = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VForm");
        Assert.AreEqual("fastFail", form.Props.Single(static prop => prop.PublicName == "FastFail").Name);

        var breadcrumbs = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBreadcrumbs");
        Assert.AreEqual("items", breadcrumbs.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.IsTrue(breadcrumbs.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var dataTable = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDataTable");
        Assert.AreEqual("headers", dataTable.Props.Single(static prop => prop.PublicName == "Headers").Name);
        Assert.AreEqual("items", dataTable.Props.Single(static prop => prop.PublicName == "Items").Name);

        var pagination = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VPagination");
        Assert.AreEqual("modelValue", pagination.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", pagination.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var image = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VImg");
        Assert.AreEqual("src", image.Props.Single(static prop => prop.PublicName == "Src").Name);
        Assert.AreEqual("alt", image.Props.Single(static prop => prop.PublicName == "Alt").Name);

        var tooltip = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTooltip");
        Assert.AreEqual("text", tooltip.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.IsTrue(tooltip.Slots.Any(static slot => slot.Name == "activator"));
        Assert.IsTrue(form.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var menu = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VMenu");
        Assert.AreEqual("modelValue", menu.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", menu.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsTrue(menu.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var avatar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAvatar");
        Assert.AreEqual("image", avatar.Props.Single(static prop => prop.PublicName == "Image").Name);
        Assert.IsTrue(avatar.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var badge = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBadge");
        Assert.AreEqual("content", badge.Props.Single(static prop => prop.PublicName == "Content").Name);
        Assert.IsTrue(badge.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var progressCircular = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VProgressCircular");
        Assert.AreEqual("modelValue", progressCircular.Props.Single(static prop => prop.PublicName == "ModelValue").Name);

        var progressLinear = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VProgressLinear");
        Assert.AreEqual("modelValue", progressLinear.Props.Single(static prop => prop.PublicName == "ModelValue").Name);

        var radioGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRadioGroup");
        Assert.AreEqual("modelValue", radioGroup.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", radioGroup.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsTrue(radioGroup.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var snackbar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSnackbar");
        Assert.AreEqual("modelValue", snackbar.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", snackbar.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsTrue(snackbar.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var tabs = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTabs");
        Assert.AreEqual("modelValue", tabs.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", tabs.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsTrue(tabs.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var tab = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTab");
        Assert.AreEqual("value", tab.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("text", tab.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.IsTrue(tab.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var spacer = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSpacer");
        Assert.AreEqual(0, spacer.Props.Length);
        Assert.AreEqual(0, spacer.Emits.Length);
        Assert.AreEqual(0, spacer.Slots.Length);
    }

    [TestMethod]
    public void RazorVue_Context_InvalidLibraryStyleDependencyDeclaration_ThrowsStructuredIssue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/styles")]
                [VueLibraryStyle(" demo/styles ")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => context.DiscoverLibraryComponents());
        Assert.AreEqual(RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "duplicate style dependency");
    }

    [TestMethod]
    public void RazorVue_Context_InvalidLibraryPluginRequirementDeclaration_ThrowsStructuredIssue()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryPluginRequirement("demo-host")]
                [VueLibraryPluginRequirement(" demo-host ")]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => context.DiscoverLibraryComponents());
        Assert.AreEqual(RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "duplicate plugin requirement");
    }

    [TestMethod]
    public void RazorVue_Context_AppliesExplicitLibraryAuthoringOverrides()
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryProp(nameof(Label), VuePropKind.HtmlLike, Name = "buttonLabel", Required = true, DefaultExpression = "'Save'", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(OnSubmit), VueEmitKind.LibrarySpecific, Name = "onSaveNow", PayloadTypeName = "Demo.Payload")]
                [VueLibrarySlot(nameof(Footer), Name = "actions", Required = true, ContextTypeName = "Demo.FooterContext", ContextParameterName = "item")]
                [VueLibraryComponentFlags(VueComponentFlags.RequiresExplicitChildren | VueComponentFlags.IsFormControl)]
                public sealed class DemoButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string? Label { get; set; }

                    [Parameter]
                    public EventCallback OnSubmit { get; set; }

                    [Parameter]
                    public RenderFragment<string>? Footer { get; set; }
                }
            }
            """);

        var descriptor = context.DiscoverLibraryComponents()
            .Single(static item => item.FullName == "Demo.Ui.Custom.DemoButton");
        var prop = descriptor.Props.Single(static item => item.PublicName == "Label");
        var emit = descriptor.Emits.Single(static item => item.RazorAlias == "OnSubmit");
        var slot = descriptor.Slots.Single(static item => item.PublicName == "Footer");

        Assert.AreEqual("buttonLabel", prop.Name);
        Assert.AreEqual(VuePropKind.HtmlLike, prop.Kind);
        Assert.IsTrue(prop.Required);
        Assert.IsTrue(prop.AcceptsBinding);
        Assert.AreEqual("'Save'", prop.DefaultExpression);

        Assert.AreEqual("onSaveNow", emit.Name);
        Assert.AreEqual("Demo.Payload", emit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.LibrarySpecific, emit.Kind);

        Assert.AreEqual("actions", slot.Name);
        Assert.AreEqual("Footer", slot.PublicName);
        Assert.IsFalse(slot.IsDefault);
        Assert.IsTrue(slot.Required);
        Assert.AreEqual("item", slot.Parameters[0].Name);
        Assert.AreEqual("Demo.FooterContext", slot.Parameters[0].TypeName);

        Assert.AreEqual(
            VueComponentFlags.RequiresExplicitChildren | VueComponentFlags.IsFormControl,
            descriptor.Flags);
    }

    [TestMethod]
    public void RazorVue_Candidate_ExtractsLifecycleAndLogicMethods()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
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
                public class LifecycleCard : ComponentBase, IVueComponent, IDisposable, IAsyncDisposable
                {
                    protected override void OnInitialized()
                    {
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }

                    public void Dispose()
                    {
                    }

                    public ValueTask DisposeAsync()
                        => ValueTask.CompletedTask;

                    public int Calculate(int value)
                        => value + 1;

                    public async Task RefreshAsync()
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        var candidate = context.DiscoverComponentCandidates().Single();

        Assert.IsNotNull(candidate.OnInitializedMethod);
        Assert.IsNotNull(candidate.OnParametersSetMethod);
        Assert.IsNotNull(candidate.OnAfterRenderMethod);
        Assert.IsNotNull(candidate.DisposeMethod);
        Assert.IsNotNull(candidate.DisposeAsyncMethod);

        var logicNames = candidate.LogicMethods.Select(static method => method.Name).ToArray();
        CollectionAssert.Contains(logicNames, "Calculate");
        CollectionAssert.Contains(logicNames, "RefreshAsync");
    }

    [TestMethod]
    public void RazorVue_Snapshot_ContainsLifecycleAndLogicDescriptors()
    {
        var snapshot = CreateSingleSnapshot(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
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
                public class LifecycleCard : ComponentBase, IVueComponent, IDisposable, IAsyncDisposable
                {
                    protected override void OnInitialized()
                    {
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }

                    public void Dispose()
                    {
                    }

                    public ValueTask DisposeAsync()
                        => ValueTask.CompletedTask;

                    public int Calculate(int value)
                        => value + 1;

                    public async Task RefreshAsync()
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        Assert.IsTrue(snapshot.Lifecycle.HasOnInitialized);
        Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSet);
        Assert.IsTrue(snapshot.Lifecycle.HasOnAfterRender);
        Assert.IsTrue(snapshot.Lifecycle.HasDispose);
        Assert.IsTrue(snapshot.Lifecycle.HasDisposeAsync);
        Assert.IsTrue(snapshot.Lifecycle.HasAnyHook);
        Assert.IsNotNull(snapshot.DisposeMethod);
        Assert.IsNotNull(snapshot.DisposeAsyncMethod);

        var calculate = snapshot.Logic.Methods.Single(method => method.Name == "Calculate");
        Assert.AreEqual(1, calculate.Arity);
        Assert.IsFalse(calculate.IsAsync);

        var refresh = snapshot.Logic.Methods.Single(method => method.Name == "RefreshAsync");
        Assert.AreEqual(0, refresh.Arity);
        Assert.IsTrue(refresh.IsAsync);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ContainsSupportedLogicFieldsAndHelpers()
    {
        var snapshot = CreateSingleSnapshot(
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
                [ECMAScript.ECMAScriptModule("./components/helper-card")]
                public class HelperCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private readonly string TitleText = "Count: ";

                    public string FormatTitle()
                        => TitleText + Value;
                }
            }
            """);

        Assert.AreEqual(1, snapshot.Logic.Fields.Length);
        Assert.AreEqual("TitleText", snapshot.Logic.Fields[0].Name);
        Assert.AreEqual("FormatTitle", snapshot.Logic.Methods.Single().Name);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ResolvesPrimaryRazorDocumentAndImports_FromRazorGeneratedBuildRenderTree()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.RazorDocument.Tests",
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
                    #line 1 "{{importsPath}}"
                    using System;
                    #line default
                    #line hidden
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Pages
                    {
                        public partial class TodoApp
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                    #line 1 "{{documentPath}}"
                                __builder.OpenElement(0, "section");
                                __builder.AddContent(1, "Hello");
                                __builder.CloseElement();
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

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        Assert.AreEqual(documentPath, snapshot.RazorDocumentPath);
        CollectionAssert.AreEqual(new[] { importsPath }, snapshot.RazorImportDocumentPaths.ToArray());
    }

    [TestMethod]
    public void RazorVue_Context_ResolvesPrimaryAndImportRazorDocuments_FromIndexedAdditionalTexts()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string importsText = "@using Demo.Shared";
        const string documentText = """
            @page "/todo"
            <section>Hello from Razor doc</section>
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.RazorDocument.Catalog.Tests",
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
                    #line 1 "{{importsPath}}"
                    using System;
                    #line default
                    #line hidden
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Pages
                    {
                        public partial class TodoApp
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                    #line 1 "{{documentPath}}"
                                __builder.AddContent(0, "Hello from Razor doc");
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

        var context = RazorVueCompilationContext.TryCreate(
            compilation,
            RazorVueRazorDocumentSet.Create(
            [
                new RazorVueRazorDocument(importsPath.Replace('\\', '/'), SourceText.From(importsText)),
                new RazorVueRazorDocument(documentPath.Replace('\\', '/'), SourceText.From(documentText))
            ]));
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        Assert.IsTrue(context.TryGetPrimaryRazorDocument(snapshot, out var primaryDocument));
        Assert.AreEqual(documentText, primaryDocument.Text.ToString());

        var importDocuments = context.GetRazorImportDocuments(snapshot);
        Assert.AreEqual(1, importDocuments.Length);
        Assert.AreEqual(importsText, importDocuments[0].Text.ToString());
    }

    [TestMethod]
    public void RazorVue_Snapshot_LeavesRazorDocumentReferenceEmpty_ForPlainCSharpComponent()
    {
        var snapshot = CreateSingleSnapshot(
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
                [ECMAScript.ECMAScriptModule("./components/plain-card")]
                public class PlainCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        Assert.IsNull(snapshot.RazorDocumentPath);
        Assert.IsTrue(snapshot.RazorImportDocumentPaths.IsDefaultOrEmpty);
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

    private static RazorVueSemanticSnapshot CreateSingleSnapshot(string source)
    {
        var context = CreateContext(source);
        var candidates = context.DiscoverComponentCandidates();
        Assert.HasCount(1, candidates);
        return context.CreateSemanticSnapshot(candidates[0]);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = RazorVueMetadataReferences.Create();

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
