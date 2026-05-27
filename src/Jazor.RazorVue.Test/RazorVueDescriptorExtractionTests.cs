using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text.Json;
using ECMAScript;
using ECMAScript.TDesign;
using ECMAScript.Vuetify;
using static ECMAScript.Vue3;

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
    public void RazorVue_Snapshot_RouteAttributes_AreProjectedIntoDescriptor()
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

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/catalog-page")]
                [Route("/")]
                [Route("/catalog")]
                public class CatalogPage : ComponentBase, IVueComponent
                {
                }
            }
            """);

        CollectionAssert.AreEqual(
            new[] { "/", "/catalog" },
            snapshot.Descriptor.RouteTemplates.ToArray());
    }

    [TestMethod]
    public void RazorVue_Snapshot_UsesRazorPageDirective_WhenRuntimeRouteAttributesAreMissing()
    {
        const string documentPath = @"D:\repo\Demo\Pages\CatalogPage.razor";
        const string razorDocumentText = """
            @page "/"
            @page "/catalog"
            <section>Catalog</section>
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.RouteFallback.Tests",
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
                        [ECMAScript.ECMAScriptModule("./pages/catalog-page")]
                        public partial class CatalogPage : ComponentBase, IVueComponent
                        {
                        }
                    }
                    """,
                    path: "CatalogPage.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    $$"""
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Pages
                    {
                        public partial class CatalogPage
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                    #line 1 "{{documentPath}}"
                                __builder.AddContent(0, "Catalog");
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    path: "CatalogPage.razor.g.cs")
            ],
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation = (CSharpCompilation)InjectCarrierCompilation(
            compilation,
            documentPath.Replace('\\', '/'),
            string.Empty,
            razorDocumentText,
            string.Empty);
        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        CollectionAssert.AreEqual(
            new[] { "/", "/catalog" },
            snapshot.Descriptor.RouteTemplates.ToArray());
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
    public void RazorVue_Context_LibrarySlotPattern_IsProjectedIntoDescriptor()
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
                public sealed record ItemSlotContext;

                [VueLibraryComponent("demo/components", "DynamicSlotHost")]
                [VueSlot(nameof(Item), Name = "item", NamePattern = "item.${string}", PatternOnly = true, ContextParameterName = "item")]
                public sealed class DynamicSlotHost : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public RenderFragment<ItemSlotContext>? Item { get; set; }
                }
            }
            """);

        var descriptor = context.DiscoverLibraryComponents()
            .Single(static descriptor => descriptor.FullName == "Demo.Ui.Custom.DynamicSlotHost");
        var slot = descriptor.Slots.Single();

        Assert.AreEqual("Item", slot.PublicName);
        Assert.AreEqual("item", slot.Name);
        Assert.AreEqual("item.${string}", slot.NamePattern);
        Assert.IsTrue(slot.PatternOnly);
        Assert.AreEqual("item", slot.Parameters[0].Name);
        Assert.AreEqual("Demo.Ui.Custom.ItemSlotContext", slot.Parameters[0].TypeName);
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

        foreach (var descriptor in vuetifyDescriptors)
        {
            var additionalAttributes = descriptor.Props.SingleOrDefault(static prop => prop.PublicName == "AdditionalAttributes");
            Assert.IsNotNull(additionalAttributes, descriptor.FullName);
            Assert.AreEqual("additionalAttributes", additionalAttributes!.Name, descriptor.FullName);
            Assert.IsTrue(additionalAttributes.CaptureUnmatchedValues, descriptor.FullName);
            Assert.AreEqual("System.Collections.Generic.IReadOnlyDictionary<string, object?>?", additionalAttributes.TypeName, descriptor.FullName);
        }

        CollectionAssert.AreEquivalent(
            VuetifyTestMetadata.RuntimeComponentExportNames,
            vuetifyDescriptors
                .Select(static descriptor => descriptor.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());
        Assert.AreEqual(VuetifyTestMetadata.RuntimeComponentExportNames.Length, vuetifyDescriptors.Length);
        Assert.AreEqual(VuetifyTestMetadata.StrongAuthoringComponentNames.Length, vuetifyDescriptors.Length);
        Assert.AreEqual(0, VuetifyTestMetadata.RuntimeOnlyAuthoringComponentNames.Length);
        CollectionAssert.IsSubsetOf(
            VuetifyTestMetadata.RuntimeOnlyAuthoringComponentNames,
            VuetifyTestMetadata.RuntimeComponentExportNames);
        CollectionAssert.IsSubsetOf(
            VuetifyTestMetadata.StrongAuthoringComponentNames,
            VuetifyTestMetadata.RuntimeComponentExportNames);

        var textField = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTextField");
        Assert.AreEqual("vuetify/components", textField.ImportSpecifier);
        Assert.AreEqual("VTextField", textField.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, textField.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, textField.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", textField.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textField.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("placeholder", textField.Props.Single(static prop => prop.PublicName == "Placeholder").Name);
        Assert.AreEqual("hint", textField.Props.Single(static prop => prop.PublicName == "Hint").Name);
        Assert.AreEqual("persistentHint", textField.Props.Single(static prop => prop.PublicName == "PersistentHint").Name);
        Assert.AreEqual("persistentPlaceholder", textField.Props.Single(static prop => prop.PublicName == "PersistentPlaceholder").Name);
        Assert.AreEqual("prefix", textField.Props.Single(static prop => prop.PublicName == "Prefix").Name);
        Assert.AreEqual("suffix", textField.Props.Single(static prop => prop.PublicName == "Suffix").Name);
        Assert.AreEqual("color", textField.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("baseColor", textField.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("bgColor", textField.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("readonly", textField.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("clearable", textField.Props.Single(static prop => prop.PublicName == "Clearable").Name);
        Assert.AreEqual("persistentClear", textField.Props.Single(static prop => prop.PublicName == "PersistentClear").Name);
        Assert.AreEqual("focused", textField.Props.Single(static prop => prop.PublicName == "Focused").Name);
        Assert.AreEqual("counter", textField.Props.Single(static prop => prop.PublicName == "Counter").Name);
        Assert.AreEqual("counterValue", textField.Props.Single(static prop => prop.PublicName == "CounterValue").Name);
        Assert.AreEqual("errorMessages", textField.Props.Single(static prop => prop.PublicName == "ErrorMessages").Name);
        Assert.AreEqual("hideDetails", textField.Props.Single(static prop => prop.PublicName == "HideDetails").Name);
        Assert.AreEqual("validateOn", textField.Props.Single(static prop => prop.PublicName == "ValidateOn").Name);
        Assert.AreEqual("variant", textField.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("density", textField.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("type", textField.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("autofocus", textField.Props.Single(static prop => prop.PublicName == "Autofocus").Name);
        Assert.AreEqual("reverse", textField.Props.Single(static prop => prop.PublicName == "Reverse").Name);
        Assert.AreEqual("update:focused", textField.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("additionalAttributes", textField.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(textField.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyCounterValue?", textField.Props.Single(static prop => prop.PublicName == "Counter").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyCounterValueSource?", textField.Props.Single(static prop => prop.PublicName == "CounterValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyValidateOn?", textField.Props.Single(static prop => prop.PublicName == "ValidateOn").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldSlotContext", textField.Slots.Single(static slot => slot.Name == "prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldSlotContext", textField.Slots.Single(static slot => slot.Name == "append-inner").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputDetailsSlotContext", textField.Slots.Single(static slot => slot.Name == "details").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VCounterSlotContext", textField.Slots.Single(static slot => slot.Name == "counter").Parameters[0].TypeName);

        var calendar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCalendar");
        Assert.AreEqual("vuetify/labs/components", calendar.ImportSpecifier);
        Assert.AreEqual("VCalendar", calendar.ExportName);
        Assert.AreEqual("modelValue", calendar.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", calendar.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("allowedDates", calendar.Props.Single(static prop => prop.PublicName == "AllowedDates").Name);
        Assert.AreEqual("events", calendar.Props.Single(static prop => prop.PublicName == "Events").Name);
        Assert.AreEqual("intervalFormat", calendar.Props.Single(static prop => prop.PublicName == "IntervalFormat").Name);
        Assert.AreEqual("next", calendar.Emits.Single(static emit => emit.RazorAlias == "Next").Name);
        Assert.AreEqual("prev", calendar.Emits.Single(static emit => emit.RazorAlias == "Prev").Name);
        Assert.AreEqual("header", calendar.Slots.Single(static slot => slot.PublicName == "Header").Name);
        Assert.AreEqual("event", calendar.Slots.Single(static slot => slot.PublicName == "EventContent").Name);

        var timePicker = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTimePicker");
        Assert.AreEqual("vuetify/labs/components", timePicker.ImportSpecifier);
        Assert.AreEqual("VTimePicker", timePicker.ExportName);
        Assert.AreEqual("modelValue", timePicker.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", timePicker.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("allowedHours", timePicker.Props.Single(static prop => prop.PublicName == "AllowedHours").Name);
        Assert.AreEqual("allowedMinutes", timePicker.Props.Single(static prop => prop.PublicName == "AllowedMinutes").Name);
        Assert.AreEqual("allowedSeconds", timePicker.Props.Single(static prop => prop.PublicName == "AllowedSeconds").Name);
        Assert.AreEqual("update:viewMode", timePicker.Emits.Single(static emit => emit.RazorAlias == "ViewModeChanged").Name);
        Assert.AreEqual("update:period", timePicker.Emits.Single(static emit => emit.RazorAlias == "PeriodChanged").Name);

        var picker = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VPicker");
        Assert.AreEqual("vuetify/labs/components", picker.ImportSpecifier);
        Assert.AreEqual("VPicker", picker.ExportName);
        Assert.AreEqual("theme", picker.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", picker.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", picker.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("position", picker.Props.Single(static prop => prop.PublicName == "Position").Name);
        Assert.AreEqual("location", picker.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("bgColor", picker.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("hideHeader", picker.Props.Single(static prop => prop.PublicName == "HideHeader").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", picker.Props.Single(static prop => prop.PublicName == "Elevation").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", picker.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", picker.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.IsTrue(picker.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(picker.Slots.Single(static slot => slot.Name == "header").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(picker.Slots.Single(static slot => slot.Name == "actions").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(picker.Slots.Single(static slot => slot.Name == "title").Parameters.IsDefaultOrEmpty);

        var pullToRefresh = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VPullToRefresh");
        Assert.AreEqual("vuetify/labs/components", pullToRefresh.ImportSpecifier);
        Assert.AreEqual("VPullToRefresh", pullToRefresh.ExportName);
        Assert.AreEqual("disabled", pullToRefresh.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("pullDownThreshold", pullToRefresh.Props.Single(static prop => prop.PublicName == "PullDownThreshold").Name);
        Assert.AreEqual("load", pullToRefresh.Emits.Single(static emit => emit.RazorAlias == "Load").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VPullToRefreshLoadOptions", pullToRefresh.Emits.Single(static emit => emit.RazorAlias == "Load").PayloadTypeName);
        Assert.IsTrue(pullToRefresh.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.AreEqual("ECMAScript.Vuetify.VPullToRefreshPanelSlotContext", pullToRefresh.Slots.Single(static slot => slot.Name == "pullDownPanel").Parameters[0].TypeName);

        var dateInput = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDateInput");
        Assert.AreEqual("vuetify/labs/components", dateInput.ImportSpecifier);
        Assert.AreEqual("VDateInput", dateInput.ExportName);
        Assert.AreEqual("modelValue", dateInput.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", dateInput.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("update:focused", dateInput.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("save", dateInput.Emits.Single(static emit => emit.RazorAlias == "Save").Name);
        Assert.AreEqual("cancel", dateInput.Emits.Single(static emit => emit.RazorAlias == "Cancel").Name);
        Assert.AreEqual("cancelText", dateInput.Props.Single(static prop => prop.PublicName == "CancelText").Name);
        Assert.AreEqual("okText", dateInput.Props.Single(static prop => prop.PublicName == "OkText").Name);
        Assert.AreEqual("hideActions", dateInput.Props.Single(static prop => prop.PublicName == "HideActions").Name);
        Assert.AreEqual("mobile", dateInput.Props.Single(static prop => prop.PublicName == "Mobile").Name);
        Assert.AreEqual("mobileBreakpoint", dateInput.Props.Single(static prop => prop.PublicName == "MobileBreakpoint").Name);
        Assert.AreEqual("displayFormat", dateInput.Props.Single(static prop => prop.PublicName == "DisplayFormat").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VDateInputDisplayFormatValue?", dateInput.Props.Single(static prop => prop.PublicName == "DisplayFormat").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerModelValue?", dateInput.Props.Single(static prop => prop.PublicName == "Min").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerModelValue?", dateInput.Props.Single(static prop => prop.PublicName == "Max").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldSlotContext", dateInput.Slots.Single(static slot => slot.Name == "prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldSlotContext", dateInput.Slots.Single(static slot => slot.Name == "append-inner").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputDetailsSlotContext", dateInput.Slots.Single(static slot => slot.Name == "details").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VCounterSlotContext", dateInput.Slots.Single(static slot => slot.Name == "counter").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDateInputActionsSlotContext", dateInput.Slots.Single(static slot => slot.Name == "actions").Parameters[0].TypeName);

        var fileUpload = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VFileUpload");
        Assert.AreEqual("vuetify/labs/components", fileUpload.ImportSpecifier);
        Assert.AreEqual("VFileUpload", fileUpload.ExportName);
        Assert.AreEqual("modelValue", fileUpload.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", fileUpload.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("browseText", fileUpload.Props.Single(static prop => prop.PublicName == "BrowseText").Name);
        Assert.AreEqual("dividerText", fileUpload.Props.Single(static prop => prop.PublicName == "DividerText").Name);
        Assert.AreEqual("hideBrowse", fileUpload.Props.Single(static prop => prop.PublicName == "HideBrowse").Name);
        Assert.AreEqual("showSize", fileUpload.Props.Single(static prop => prop.PublicName == "ShowSize").Name);
        Assert.AreEqual("ECMAScript.Vue3.File[]?", fileUpload.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", fileUpload.Props.Single(static prop => prop.PublicName == "Length").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFileUploadBrowseSlotContext", fileUpload.Slots.Single(static slot => slot.Name == "browse").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFileUploadInputSlotContext", fileUpload.Slots.Single(static slot => slot.Name == "input").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFileUploadItemSlotContext", fileUpload.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.IsTrue(fileUpload.Slots.Single(static slot => slot.Name == "icon").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(fileUpload.Slots.Single(static slot => slot.Name == "title").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(fileUpload.Slots.Single(static slot => slot.Name == "divider").Parameters.IsDefaultOrEmpty);

        var iconBtn = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VIconBtn");
        Assert.AreEqual("vuetify/labs/components", iconBtn.ImportSpecifier);
        Assert.AreEqual("VIconBtn", iconBtn.ExportName);
        Assert.AreEqual("update:active", iconBtn.Emits.Single(static emit => emit.RazorAlias == "ActiveChanged").Name);
        Assert.AreEqual("activeColor", iconBtn.Props.Single(static prop => prop.PublicName == "ActiveColor").Name);
        Assert.AreEqual("activeIcon", iconBtn.Props.Single(static prop => prop.PublicName == "ActiveIcon").Name);
        Assert.AreEqual("activeVariant", iconBtn.Props.Single(static prop => prop.PublicName == "ActiveVariant").Name);
        Assert.AreEqual("baseVariant", iconBtn.Props.Single(static prop => prop.PublicName == "BaseVariant").Name);
        Assert.AreEqual("iconSize", iconBtn.Props.Single(static prop => prop.PublicName == "IconSize").Name);
        Assert.AreEqual("iconSizes", iconBtn.Props.Single(static prop => prop.PublicName == "IconSizes").Name);
        Assert.AreEqual("sizes", iconBtn.Props.Single(static prop => prop.PublicName == "Sizes").Name);
        Assert.AreEqual("text", iconBtn.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", iconBtn.Props.Single(static prop => prop.PublicName == "ActiveVariant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", iconBtn.Props.Single(static prop => prop.PublicName == "BaseVariant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VIconBtnSizeMap?", iconBtn.Props.Single(static prop => prop.PublicName == "IconSizes").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VIconBtnSizeMap?", iconBtn.Props.Single(static prop => prop.PublicName == "Sizes").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VIconBtnTextValue?", iconBtn.Props.Single(static prop => prop.PublicName == "Text").TypeName);
        Assert.IsTrue(iconBtn.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(iconBtn.Slots.Single(static slot => slot.Name == "loader").Parameters.IsDefaultOrEmpty);
        Assert.AreEqual("update:hour", timePicker.Emits.Single(static emit => emit.RazorAlias == "HourChanged").Name);
        Assert.AreEqual("update:minute", timePicker.Emits.Single(static emit => emit.RazorAlias == "MinuteChanged").Name);
        Assert.AreEqual("update:second", timePicker.Emits.Single(static emit => emit.RazorAlias == "SecondChanged").Name);
        Assert.AreEqual("title", timePicker.Slots.Single(static slot => slot.PublicName == "TitleContent").Name);
        Assert.AreEqual("actions", timePicker.Slots.Single(static slot => slot.PublicName == "Actions").Name);

        var treeview = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTreeview");
        Assert.AreEqual("vuetify/labs/components", treeview.ImportSpecifier);
        Assert.AreEqual("VTreeview", treeview.ExportName);
        Assert.AreEqual("modelValue", treeview.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", treeview.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("items", treeview.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("activated", treeview.Props.Single(static prop => prop.PublicName == "Activated").Name);
        Assert.AreEqual("selected", treeview.Props.Single(static prop => prop.PublicName == "Selected").Name);
        Assert.AreEqual("opened", treeview.Props.Single(static prop => prop.PublicName == "Opened").Name);
        Assert.AreEqual("activeStrategy", treeview.Props.Single(static prop => prop.PublicName == "ActiveStrategy").Name);
        Assert.AreEqual("selectStrategy", treeview.Props.Single(static prop => prop.PublicName == "SelectStrategy").Name);
        Assert.AreEqual("loadChildren", treeview.Props.Single(static prop => prop.PublicName == "LoadChildren").Name);
        Assert.AreEqual("style", treeview.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", treeview.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("click:open", treeview.Emits.Single(static emit => emit.RazorAlias == "OpenClicked").Name);
        Assert.AreEqual("click:select", treeview.Emits.Single(static emit => emit.RazorAlias == "SelectClicked").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTreeviewClickPayload", treeview.Emits.Single(static emit => emit.RazorAlias == "OpenClicked").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTreeviewClickPayload", treeview.Emits.Single(static emit => emit.RazorAlias == "SelectClicked").PayloadTypeName);
        Assert.AreEqual("prepend", treeview.Slots.Single(static slot => slot.PublicName == "Prepend").Name);
        Assert.AreEqual("append", treeview.Slots.Single(static slot => slot.PublicName == "Append").Name);
        Assert.AreEqual("title", treeview.Slots.Single(static slot => slot.PublicName == "TitleContent").Name);
        Assert.AreEqual("subtitle", treeview.Slots.Single(static slot => slot.PublicName == "SubtitleContent").Name);
        Assert.AreEqual("item", treeview.Slots.Single(static slot => slot.PublicName == "ItemContent").Name);
        Assert.AreEqual("header", treeview.Slots.Single(static slot => slot.PublicName == "Header").Name);
        Assert.AreEqual("divider", treeview.Slots.Single(static slot => slot.PublicName == "Divider").Name);
        Assert.AreEqual("subheader", treeview.Slots.Single(static slot => slot.PublicName == "Subheader").Name);
        Assert.AreEqual(
            "ECMAScript.Vuetify.VTreeviewNodeSlotContext",
            treeview.Slots.Single(static slot => slot.PublicName == "Prepend").Parameters[0].TypeName);
        Assert.AreEqual(
            "ECMAScript.Vuetify.VTreeviewItemSlotContext",
            treeview.Slots.Single(static slot => slot.PublicName == "ItemContent").Parameters[0].TypeName);
        Assert.AreEqual(
            "ECMAScript.Vuetify.VTreeviewStructuralItemSlotContext",
            treeview.Slots.Single(static slot => slot.PublicName == "Header").Parameters[0].TypeName);

        var button = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBtn");
        Assert.AreEqual("active", button.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("activeColor", button.Props.Single(static prop => prop.PublicName == "ActiveColor").Name);
        Assert.AreEqual("activeReadonly", button.Props.Single(static prop => prop.PublicName == "ActiveReadonly").Name);
        Assert.AreEqual("baseColor", button.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("prependIcon", button.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("appendIcon", button.Props.Single(static prop => prop.PublicName == "AppendIcon").Name);
        Assert.AreEqual("color", button.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("variant", button.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("size", button.Props.Single(static prop => prop.PublicName == "Size").Name);
        Assert.AreEqual("loading", button.Props.Single(static prop => prop.PublicName == "Loading").Name);
        Assert.AreEqual("block", button.Props.Single(static prop => prop.PublicName == "Block").Name);
        Assert.AreEqual("border", button.Props.Single(static prop => prop.PublicName == "Border").Name);
        Assert.AreEqual("height", button.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("width", button.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("minHeight", button.Props.Single(static prop => prop.PublicName == "MinHeight").Name);
        Assert.AreEqual("maxWidth", button.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("rounded", button.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("elevation", button.Props.Single(static prop => prop.PublicName == "Elevation").Name);
        Assert.AreEqual("exact", button.Props.Single(static prop => prop.PublicName == "Exact").Name);
        Assert.AreEqual("href", button.Props.Single(static prop => prop.PublicName == "Href").Name);
        Assert.AreEqual("target", button.Props.Single(static prop => prop.PublicName == "Target").Name);
        Assert.AreEqual("to", button.Props.Single(static prop => prop.PublicName == "To").Name);
        Assert.AreEqual("replace", button.Props.Single(static prop => prop.PublicName == "Replace").Name);
        Assert.AreEqual("icon", button.Props.Single(static prop => prop.PublicName == "Icon").Name);
        Assert.AreEqual("slim", button.Props.Single(static prop => prop.PublicName == "Slim").Name);
        Assert.AreEqual("stacked", button.Props.Single(static prop => prop.PublicName == "Stacked").Name);
        Assert.AreEqual("symbol", button.Props.Single(static prop => prop.PublicName == "Symbol").Name);
        Assert.AreEqual("density", button.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("location", button.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("position", button.Props.Single(static prop => prop.PublicName == "Position").Name);
        Assert.AreEqual("tag", button.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("type", button.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("value", button.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("ripple", button.Props.Single(static prop => prop.PublicName == "Ripple").Name);
        Assert.AreEqual("additionalAttributes", button.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(button.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", button.Props.Single(static prop => prop.PublicName == "Text").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", button.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", button.Props.Single(static prop => prop.PublicName == "Loading").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", button.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", button.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", button.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.IsTrue(button.Slots.Single(static slot => slot.Name == "prepend").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(button.Slots.Single(static slot => slot.Name == "append").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(button.Slots.Single(static slot => slot.Name == "loader").Parameters.IsDefaultOrEmpty);

        var fab = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VFab");
        Assert.AreEqual("modelValue", fab.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", fab.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("app", fab.Props.Single(static prop => prop.PublicName == "App").Name);
        Assert.AreEqual("appear", fab.Props.Single(static prop => prop.PublicName == "Appear").Name);
        Assert.AreEqual("extended", fab.Props.Single(static prop => prop.PublicName == "Extended").Name);
        Assert.AreEqual("layout", fab.Props.Single(static prop => prop.PublicName == "Layout").Name);
        Assert.AreEqual("offset", fab.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("transition", fab.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("location", fab.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("name", fab.Props.Single(static prop => prop.PublicName == "Name").Name);
        Assert.AreEqual("order", fab.Props.Single(static prop => prop.PublicName == "Order").Name);
        Assert.AreEqual("absolute", fab.Props.Single(static prop => prop.PublicName == "Absolute").Name);
        Assert.AreEqual("active", fab.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("baseColor", fab.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("prependIcon", fab.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("variant", fab.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("theme", fab.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("readonly", fab.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("tile", fab.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("selectedClass", fab.Props.Single(static prop => prop.PublicName == "SelectedClass").Name);
        Assert.AreEqual("additionalAttributes", fab.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(fab.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", fab.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", fab.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", fab.Props.Single(static prop => prop.PublicName == "Order").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", fab.Props.Single(static prop => prop.PublicName == "Text").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", fab.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.IsTrue(fab.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var speedDial = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSpeedDial");
        Assert.AreEqual("modelValue", speedDial.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", speedDial.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("offset", speedDial.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("location", speedDial.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("origin", speedDial.Props.Single(static prop => prop.PublicName == "Origin").Name);
        Assert.AreEqual("transition", speedDial.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("zIndex", speedDial.Props.Single(static prop => prop.PublicName == "ZIndex").Name);
        Assert.AreEqual("activatorProps", speedDial.Props.Single(static prop => prop.PublicName == "ActivatorProps").Name);
        Assert.AreEqual("contentProps", speedDial.Props.Single(static prop => prop.PublicName == "ContentProps").Name);
        Assert.AreEqual("submenu", speedDial.Props.Single(static prop => prop.PublicName == "Submenu").Name);
        Assert.AreEqual("additionalAttributes", speedDial.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(speedDial.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOverlayOffsetValue?", speedDial.Props.Single(static prop => prop.PublicName == "Offset").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOriginValue?", speedDial.Props.Single(static prop => prop.PublicName == "Origin").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSpeedDialDefaultSlotContext", speedDial.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VOverlayActivatorContext", speedDial.Slots.Single(static slot => slot.Name == "activator").Parameters[0].TypeName);

        var confirmEdit = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VConfirmEdit");
        Assert.AreEqual("modelValue", confirmEdit.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", confirmEdit.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("save", confirmEdit.Emits.Single(static emit => emit.RazorAlias == "Save").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", confirmEdit.Emits.Single(static emit => emit.RazorAlias == "Save").PayloadTypeName);
        Assert.AreEqual("cancel", confirmEdit.Emits.Single(static emit => emit.RazorAlias == "Cancel").Name);
        Assert.AreEqual("color", confirmEdit.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("cancelText", confirmEdit.Props.Single(static prop => prop.PublicName == "CancelText").Name);
        Assert.AreEqual("okText", confirmEdit.Props.Single(static prop => prop.PublicName == "OkText").Name);
        Assert.AreEqual("disabled", confirmEdit.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("hideActions", confirmEdit.Props.Single(static prop => prop.PublicName == "HideActions").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", confirmEdit.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyConfirmEditDisabled?", confirmEdit.Props.Single(static prop => prop.PublicName == "Disabled").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VConfirmEditSlotContext", confirmEdit.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var checkbox = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCheckbox");
        Assert.AreEqual("vuetify/components", checkbox.ImportSpecifier);
        Assert.AreEqual("VCheckbox", checkbox.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, checkbox.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, checkbox.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", checkbox.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", checkbox.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("color", checkbox.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", checkbox.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("readonly", checkbox.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("hideDetails", checkbox.Props.Single(static prop => prop.PublicName == "HideDetails").Name);
        Assert.AreEqual("trueValue", checkbox.Props.Single(static prop => prop.PublicName == "TrueValue").Name);
        Assert.AreEqual("falseValue", checkbox.Props.Single(static prop => prop.PublicName == "FalseValue").Name);
        Assert.AreEqual("indeterminate", checkbox.Props.Single(static prop => prop.PublicName == "Indeterminate").Name);
        Assert.AreEqual("focused", checkbox.Props.Single(static prop => prop.PublicName == "Focused").Name);
        Assert.AreEqual("update:focused", checkbox.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("additionalAttributes", checkbox.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(checkbox.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", checkbox.Props.Single(static prop => prop.PublicName == "TrueValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", checkbox.Props.Single(static prop => prop.PublicName == "TrueIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlInputDefaultSlotContext", checkbox.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputSlotContext", checkbox.Slots.Single(static slot => slot.Name == "prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputSlotContext", checkbox.Slots.Single(static slot => slot.Name == "append").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputDetailsSlotContext", checkbox.Slots.Single(static slot => slot.Name == "details").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VMessagesMessageSlotContext", checkbox.Slots.Single(static slot => slot.Name == "message").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlLabelSlotContext", checkbox.Slots.Single(static slot => slot.Name == "label").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlInputSlotContext", checkbox.Slots.Single(static slot => slot.Name == "input").Parameters[0].TypeName);

        var switchDescriptor = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSwitch");
        Assert.AreEqual("modelValue", switchDescriptor.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", switchDescriptor.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("inset", switchDescriptor.Props.Single(static prop => prop.PublicName == "Inset").Name);
        Assert.AreEqual("loading", switchDescriptor.Props.Single(static prop => prop.PublicName == "Loading").Name);
        Assert.AreEqual("flat", switchDescriptor.Props.Single(static prop => prop.PublicName == "Flat").Name);
        Assert.AreEqual("trueIcon", switchDescriptor.Props.Single(static prop => prop.PublicName == "TrueIcon").Name);
        Assert.AreEqual("falseIcon", switchDescriptor.Props.Single(static prop => prop.PublicName == "FalseIcon").Name);
        Assert.AreEqual("update:focused", switchDescriptor.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlInputDefaultSlotContext", switchDescriptor.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "append").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputDetailsSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "details").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VMessagesMessageSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "message").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlInputSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "input").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLoaderSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "loader").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSwitchSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "thumb").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSwitchSlotContext", switchDescriptor.Slots.Single(static slot => slot.Name == "track-true").Parameters[0].TypeName);

        var buttonGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBtnGroup");
        Assert.AreEqual("baseColor", buttonGroup.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("divided", buttonGroup.Props.Single(static prop => prop.PublicName == "Divided").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", buttonGroup.Props.Single(static prop => prop.PublicName == "Border").TypeName);

        var buttonToggle = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBtnToggle");
        Assert.AreEqual("modelValue", buttonToggle.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", buttonToggle.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("mandatory", buttonToggle.Props.Single(static prop => prop.PublicName == "Mandatory").Name);
        Assert.AreEqual("multiple", buttonToggle.Props.Single(static prop => prop.PublicName == "Multiple").Name);
        Assert.AreEqual("selectedClass", buttonToggle.Props.Single(static prop => prop.PublicName == "SelectedClass").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", buttonToggle.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMandatoryValue?", buttonToggle.Props.Single(static prop => prop.PublicName == "Mandatory").TypeName);

        var cardItem = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCardItem");
        Assert.AreEqual("prependIcon", cardItem.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("appendAvatar", cardItem.Props.Single(static prop => prop.PublicName == "AppendAvatar").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", cardItem.Props.Single(static prop => prop.PublicName == "Title").TypeName);
        Assert.IsTrue(cardItem.Slots.Any(static slot => slot.Name == "prepend"));
        Assert.IsTrue(cardItem.Slots.Any(static slot => slot.Name == "title"));

        var footer = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VFooter");
        Assert.AreEqual("app", footer.Props.Single(static prop => prop.PublicName == "App").Name);
        Assert.AreEqual("order", footer.Props.Single(static prop => prop.PublicName == "Order").Name);
        Assert.AreEqual("theme", footer.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", footer.Props.Single(static prop => prop.PublicName == "Border").TypeName);

        var rating = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRating");
        Assert.AreEqual("modelValue", rating.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", rating.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("halfIncrements", rating.Props.Single(static prop => prop.PublicName == "HalfIncrements").Name);
        Assert.AreEqual("itemLabelPosition", rating.Props.Single(static prop => prop.PublicName == "ItemLabelPosition").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", rating.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", rating.Props.Single(static prop => prop.PublicName == "FullIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VRatingItemSlotContext", rating.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VRatingItemLabelSlotContext", rating.Slots.Single(static slot => slot.Name == "item-label").Parameters[0].TypeName);

        var table = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTable");
        Assert.AreEqual("fixedHeader", table.Props.Single(static prop => prop.PublicName == "FixedHeader").Name);
        Assert.AreEqual("fixedFooter", table.Props.Single(static prop => prop.PublicName == "FixedFooter").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", table.Props.Single(static prop => prop.PublicName == "Density").TypeName);

        var banner = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBanner");
        Assert.AreEqual("avatar", banner.Props.Single(static prop => prop.PublicName == "Avatar").Name);
        Assert.AreEqual("bgColor", banner.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("stacked", banner.Props.Single(static prop => prop.PublicName == "Stacked").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMobileValue?", banner.Props.Single(static prop => prop.PublicName == "Mobile").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", banner.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.IsTrue(banner.Slots.Any(static slot => slot.Name == "actions"));

        var bottomNavigation = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBottomNavigation");
        Assert.AreEqual("modelValue", bottomNavigation.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", bottomNavigation.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("active", bottomNavigation.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("update:active", bottomNavigation.Emits.Single(static emit => emit.RazorAlias == "ActiveChanged").Name);
        Assert.AreEqual("mode", bottomNavigation.Props.Single(static prop => prop.PublicName == "Mode").Name);
        Assert.AreEqual("selectedClass", bottomNavigation.Props.Single(static prop => prop.PublicName == "SelectedClass").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", bottomNavigation.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBottomNavigationMode?", bottomNavigation.Props.Single(static prop => prop.PublicName == "Mode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", bottomNavigation.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", bottomNavigation.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("class", bottomNavigation.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", bottomNavigation.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("style", bottomNavigation.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", bottomNavigation.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsFalse(bottomNavigation.Props.Any(static prop => prop.PublicName == "SelectedValue"));
        Assert.IsFalse(bottomNavigation.Props.Any(static prop => prop.PublicName == "ActiveColor"));
        Assert.IsFalse(bottomNavigation.Props.Any(static prop => prop.PublicName == "Shift"));

        var bottomSheet = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBottomSheet");
        Assert.AreEqual("modelValue", bottomSheet.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", bottomSheet.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("activatorProps", bottomSheet.Props.Single(static prop => prop.PublicName == "ActivatorProps").Name);
        Assert.AreEqual("contentProps", bottomSheet.Props.Single(static prop => prop.PublicName == "ContentProps").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VOverlayActivatorContext", bottomSheet.Slots.Single(static slot => slot.Name == "activator").Parameters[0].TypeName);

        var emptyState = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VEmptyState");
        Assert.AreEqual("actionText", emptyState.Props.Single(static prop => prop.PublicName == "ActionText").Name);
        Assert.AreEqual("textWidth", emptyState.Props.Single(static prop => prop.PublicName == "TextWidth").Name);
        Assert.AreEqual("click:action", emptyState.Emits.Single(static emit => emit.RazorAlias == "ActionClick").Name);
        Assert.AreEqual("ECMAScript.Event", emptyState.Emits.Single(static emit => emit.RazorAlias == "ActionClick").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", emptyState.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", emptyState.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyJustify?", emptyState.Props.Single(static prop => prop.PublicName == "Justify").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VEmptyStateActionsSlotContext", emptyState.Slots.Single(static slot => slot.Name == "actions").Parameters[0].TypeName);
        Assert.IsTrue(emptyState.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(emptyState.Slots.Single(static slot => slot.Name == "headline").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(emptyState.Slots.Single(static slot => slot.Name == "title").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(emptyState.Slots.Single(static slot => slot.Name == "media").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(emptyState.Slots.Single(static slot => slot.Name == "text").Parameters.IsDefaultOrEmpty);

        var skeletonLoader = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSkeletonLoader");
        Assert.AreEqual("type", skeletonLoader.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("loadingText", skeletonLoader.Props.Single(static prop => prop.PublicName == "LoadingText").Name);
        Assert.AreEqual("boilerplate", skeletonLoader.Props.Single(static prop => prop.PublicName == "Boilerplate").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySkeletonLoaderTypeSetting?", skeletonLoader.Props.Single(static prop => prop.PublicName == "Type").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", skeletonLoader.Props.Single(static prop => prop.PublicName == "Elevation").TypeName);
        Assert.IsTrue(skeletonLoader.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var parallax = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VParallax");
        Assert.AreEqual("scale", parallax.Props.Single(static prop => prop.PublicName == "Scale").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", parallax.Props.Single(static prop => prop.PublicName == "Scale").TypeName);
        Assert.IsTrue(parallax.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(parallax.Slots.Single(static slot => slot.Name == "placeholder").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(parallax.Slots.Single(static slot => slot.Name == "error").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(parallax.Slots.Single(static slot => slot.Name == "sources").Parameters.IsDefaultOrEmpty);

        var code = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCode");
        Assert.AreEqual("tag", code.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("string?", code.Props.Single(static prop => prop.PublicName == "Tag").TypeName);
        Assert.IsTrue(code.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var timeline = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTimeline");
        Assert.AreEqual("theme", timeline.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("density", timeline.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("lineInset", timeline.Props.Single(static prop => prop.PublicName == "LineInset").Name);
        Assert.AreEqual("lineThickness", timeline.Props.Single(static prop => prop.PublicName == "LineThickness").Name);
        Assert.AreEqual("truncateLine", timeline.Props.Single(static prop => prop.PublicName == "TruncateLine").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTimelineAlign?", timeline.Props.Single(static prop => prop.PublicName == "Align").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTimelineDirection?", timeline.Props.Single(static prop => prop.PublicName == "Direction").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTimelineJustify?", timeline.Props.Single(static prop => prop.PublicName == "Justify").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTimelineSide?", timeline.Props.Single(static prop => prop.PublicName == "Side").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTimelineTruncateLine?", timeline.Props.Single(static prop => prop.PublicName == "TruncateLine").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", timeline.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", timeline.Props.Single(static prop => prop.PublicName == "LineThickness").TypeName);
        Assert.IsTrue(timeline.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var localeProvider = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VLocaleProvider");
        Assert.AreEqual("locale", localeProvider.Props.Single(static prop => prop.PublicName == "Locale").Name);
        Assert.AreEqual("fallbackLocale", localeProvider.Props.Single(static prop => prop.PublicName == "FallbackLocale").Name);
        Assert.AreEqual("messages", localeProvider.Props.Single(static prop => prop.PublicName == "Messages").Name);
        Assert.AreEqual("rtl", localeProvider.Props.Single(static prop => prop.PublicName == "Rtl").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", localeProvider.Props.Single(static prop => prop.PublicName == "Messages").TypeName);
        Assert.AreEqual("bool?", localeProvider.Props.Single(static prop => prop.PublicName == "Rtl").TypeName);
        Assert.IsTrue(localeProvider.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var defaultsProvider = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDefaultsProvider");
        Assert.AreEqual("defaults", defaultsProvider.Props.Single(static prop => prop.PublicName == "Defaults").Name);
        Assert.AreEqual("disabled", defaultsProvider.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("reset", defaultsProvider.Props.Single(static prop => prop.PublicName == "Reset").Name);
        Assert.AreEqual("root", defaultsProvider.Props.Single(static prop => prop.PublicName == "Root").Name);
        Assert.AreEqual("scoped", defaultsProvider.Props.Single(static prop => prop.PublicName == "Scoped").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", defaultsProvider.Props.Single(static prop => prop.PublicName == "Defaults").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", defaultsProvider.Props.Single(static prop => prop.PublicName == "Reset").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", defaultsProvider.Props.Single(static prop => prop.PublicName == "Root").TypeName);
        Assert.IsTrue(defaultsProvider.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var virtualScroll = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VVirtualScroll");
        Assert.AreEqual("items", virtualScroll.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemHeight", virtualScroll.Props.Single(static prop => prop.PublicName == "ItemHeight").Name);
        Assert.AreEqual("itemKey", virtualScroll.Props.Single(static prop => prop.PublicName == "ItemKey").Name);
        Assert.AreEqual("renderless", virtualScroll.Props.Single(static prop => prop.PublicName == "Renderless").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueValue[]?", virtualScroll.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", virtualScroll.Props.Single(static prop => prop.PublicName == "ItemKey").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VVirtualScrollSlotContext", virtualScroll.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var infiniteScroll = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VInfiniteScroll");
        Assert.AreEqual("load", infiniteScroll.Emits.Single(static emit => emit.RazorAlias == "Load").Name);
        Assert.AreEqual("tag", infiniteScroll.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("direction", infiniteScroll.Props.Single(static prop => prop.PublicName == "Direction").Name);
        Assert.AreEqual("side", infiniteScroll.Props.Single(static prop => prop.PublicName == "Side").Name);
        Assert.AreEqual("mode", infiniteScroll.Props.Single(static prop => prop.PublicName == "Mode").Name);
        Assert.AreEqual("margin", infiniteScroll.Props.Single(static prop => prop.PublicName == "Margin").Name);
        Assert.AreEqual("loadMoreText", infiniteScroll.Props.Single(static prop => prop.PublicName == "LoadMoreText").Name);
        Assert.AreEqual("emptyText", infiniteScroll.Props.Single(static prop => prop.PublicName == "EmptyText").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyInfiniteScrollSide?", infiniteScroll.Props.Single(static prop => prop.PublicName == "Side").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyInfiniteScrollMode?", infiniteScroll.Props.Single(static prop => prop.PublicName == "Mode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInfiniteScrollLoadOptions", infiniteScroll.Emits.Single(static emit => emit.RazorAlias == "Load").PayloadTypeName);
        Assert.IsTrue(infiniteScroll.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.AreEqual("ECMAScript.Vuetify.VInfiniteScrollSlotContext", infiniteScroll.Slots.Single(static slot => slot.Name == "loading").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInfiniteScrollSlotContext", infiniteScroll.Slots.Single(static slot => slot.Name == "error").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInfiniteScrollSlotContext", infiniteScroll.Slots.Single(static slot => slot.Name == "empty").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInfiniteScrollSlotContext", infiniteScroll.Slots.Single(static slot => slot.Name == "load-more").Parameters[0].TypeName);

        var expansionPanel = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VExpansionPanel");
        Assert.AreEqual("collapseIcon", expansionPanel.Props.Single(static prop => prop.PublicName == "CollapseIcon").Name);
        Assert.AreEqual("expandIcon", expansionPanel.Props.Single(static prop => prop.PublicName == "ExpandIcon").Name);
        Assert.AreEqual("hideActions", expansionPanel.Props.Single(static prop => prop.PublicName == "HideActions").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", expansionPanel.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VExpansionPanelTitleSlotContext", expansionPanel.Slots.Single(static slot => slot.Name == "title").Parameters[0].TypeName);

        var overlay = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VOverlay");
        Assert.AreEqual("modelValue", overlay.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", overlay.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("attach", overlay.Props.Single(static prop => prop.PublicName == "Attach").Name);
        Assert.AreEqual("locationStrategy", overlay.Props.Single(static prop => prop.PublicName == "LocationStrategy").Name);
        Assert.AreEqual("scrollStrategy", overlay.Props.Single(static prop => prop.PublicName == "ScrollStrategy").Name);
        Assert.AreEqual("zIndex", overlay.Props.Single(static prop => prop.PublicName == "ZIndex").Name);
        Assert.AreEqual("afterEnter", overlay.Emits.Single(static emit => emit.RazorAlias == "AfterEnter").Name);
        Assert.AreEqual("afterLeave", overlay.Emits.Single(static emit => emit.RazorAlias == "AfterLeave").Name);
        Assert.AreEqual("click:outside", overlay.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").Name);
        Assert.AreEqual("keydown", overlay.Emits.Single(static emit => emit.RazorAlias == "Keydown").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAttachTarget?", overlay.Props.Single(static prop => prop.PublicName == "Attach").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocationStrategy?", overlay.Props.Single(static prop => prop.PublicName == "LocationStrategy").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", overlay.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.MouseEvent", overlay.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").PayloadTypeName);
        Assert.AreEqual("ECMAScript.KeyboardEvent", overlay.Emits.Single(static emit => emit.RazorAlias == "Keydown").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VOverlayActivatorContext", overlay.Slots.Single(static slot => slot.Name == "activator").Parameters[0].TypeName);

        var hover = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VHover");
        Assert.AreEqual("modelValue", hover.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", hover.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("openDelay", hover.Props.Single(static prop => prop.PublicName == "OpenDelay").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VHoverDefaultSlotContext", hover.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var lazy = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VLazy");
        Assert.AreEqual("modelValue", lazy.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", lazy.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("options", lazy.Props.Single(static prop => prop.PublicName == "Options").Name);
        Assert.AreEqual("minHeight", lazy.Props.Single(static prop => prop.PublicName == "MinHeight").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIntersectionObserverOptions?", lazy.Props.Single(static prop => prop.PublicName == "Options").TypeName);

        var responsive = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VResponsive");
        Assert.AreEqual("aspectRatio", responsive.Props.Single(static prop => prop.PublicName == "AspectRatio").Name);
        Assert.AreEqual("contentClass", responsive.Props.Single(static prop => prop.PublicName == "ContentClass").Name);
        Assert.AreEqual("inline", responsive.Props.Single(static prop => prop.PublicName == "Inline").Name);
        Assert.IsTrue(responsive.Slots.Single(static slot => slot.Name == "additional").Parameters.IsDefaultOrEmpty);

        var itemGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VItemGroup");
        Assert.AreEqual("modelValue", itemGroup.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", itemGroup.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("mandatory", itemGroup.Props.Single(static prop => prop.PublicName == "Mandatory").Name);
        Assert.AreEqual("valueComparator", itemGroup.Props.Single(static prop => prop.PublicName == "ValueComparator").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VItemGroupDefaultSlotContext", itemGroup.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var chipGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VChipGroup");
        Assert.AreEqual("centerActive", chipGroup.Props.Single(static prop => prop.PublicName == "CenterActive").Name);
        Assert.AreEqual("direction", chipGroup.Props.Single(static prop => prop.PublicName == "Direction").Name);
        Assert.AreEqual("showArrows", chipGroup.Props.Single(static prop => prop.PublicName == "ShowArrows").Name);
        Assert.AreEqual("valueComparator", chipGroup.Props.Single(static prop => prop.PublicName == "ValueComparator").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyShowArrowsValue?", chipGroup.Props.Single(static prop => prop.PublicName == "ShowArrows").TypeName);

        var slideGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSlideGroup");
        Assert.AreEqual("modelValue", slideGroup.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", slideGroup.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("centerActive", slideGroup.Props.Single(static prop => prop.PublicName == "CenterActive").Name);
        Assert.AreEqual("mobileBreakpoint", slideGroup.Props.Single(static prop => prop.PublicName == "MobileBreakpoint").Name);
        Assert.AreEqual("showArrows", slideGroup.Props.Single(static prop => prop.PublicName == "ShowArrows").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", slideGroup.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("int?", slideGroup.Props.Single(static prop => prop.PublicName == "Max").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDisplayBreakpoint?", slideGroup.Props.Single(static prop => prop.PublicName == "MobileBreakpoint").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyShowArrowsValue?", slideGroup.Props.Single(static prop => prop.PublicName == "ShowArrows").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSlideGroupSlotContext", slideGroup.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSlideGroupSlotContext", slideGroup.Slots.Single(static slot => slot.Name == "prev").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSlideGroupSlotContext", slideGroup.Slots.Single(static slot => slot.Name == "next").Parameters[0].TypeName);

        var counter = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCounter");
        Assert.AreEqual("active", counter.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("disabled", counter.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("max", counter.Props.Single(static prop => prop.PublicName == "Max").Name);
        Assert.AreEqual("value", counter.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("transition", counter.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", counter.Props.Single(static prop => prop.PublicName == "Max").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", counter.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VCounterDefaultSlotContext", counter.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var kbd = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VKbd");
        Assert.AreEqual("tag", kbd.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.IsTrue(kbd.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var label = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VLabel");
        Assert.AreEqual("text", label.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("theme", label.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("click", label.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", label.Emits.Single(static emit => emit.RazorAlias == "OnClick").PayloadTypeName);

        var layout = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VLayout");
        Assert.AreEqual("overlaps", layout.Props.Single(static prop => prop.PublicName == "Overlaps").Name);
        Assert.AreEqual("fullHeight", layout.Props.Single(static prop => prop.PublicName == "FullHeight").Name);
        Assert.AreEqual("width", layout.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("string[]?", layout.Props.Single(static prop => prop.PublicName == "Overlaps").TypeName);

        var messages = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VMessages");
        Assert.AreEqual("active", messages.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("messages", messages.Props.Single(static prop => prop.PublicName == "Messages").Name);
        Assert.AreEqual("transition", messages.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMessagesValue?", messages.Props.Single(static prop => prop.PublicName == "Messages").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VMessagesMessageSlotContext", messages.Slots.Single(static slot => slot.Name == "message").Parameters[0].TypeName);

        var input = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VInput");
        Assert.AreEqual("modelValue", input.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", input.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("focused", input.Props.Single(static prop => prop.PublicName == "Focused").Name);
        Assert.AreEqual("update:focused", input.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("click:prepend", input.Emits.Single(static emit => emit.RazorAlias == "PrependClick").Name);
        Assert.AreEqual("click:append", input.Emits.Single(static emit => emit.RazorAlias == "AppendClick").Name);
        Assert.AreEqual("direction", input.Props.Single(static prop => prop.PublicName == "Direction").Name);
        Assert.AreEqual("maxErrors", input.Props.Single(static prop => prop.PublicName == "MaxErrors").Name);
        Assert.AreEqual("rules", input.Props.Single(static prop => prop.PublicName == "Rules").Name);
        Assert.AreEqual("validationValue", input.Props.Single(static prop => prop.PublicName == "ValidationValue").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", input.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyNullableBoolean?", input.Props.Single(static prop => prop.PublicName == "Disabled").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyValidationRule[]?", input.Props.Single(static prop => prop.PublicName == "Rules").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputSlotContext", input.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VInputDetailsSlotContext", input.Slots.Single(static slot => slot.Name == "details").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VMessagesMessageSlotContext", input.Slots.Single(static slot => slot.Name == "message").Parameters[0].TypeName);

        var field = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VField");
        Assert.AreEqual("modelValue", field.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", field.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("update:focused", field.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("click:clear", field.Emits.Single(static emit => emit.RazorAlias == "ClearClick").Name);
        Assert.AreEqual("click:appendInner", field.Emits.Single(static emit => emit.RazorAlias == "AppendInnerClick").Name);
        Assert.AreEqual("click:prependInner", field.Emits.Single(static emit => emit.RazorAlias == "PrependInnerClick").Name);
        Assert.AreEqual("appendInnerIcon", field.Props.Single(static prop => prop.PublicName == "AppendInnerIcon").Name);
        Assert.AreEqual("centerAffix", field.Props.Single(static prop => prop.PublicName == "CenterAffix").Name);
        Assert.AreEqual("singleLine", field.Props.Single(static prop => prop.PublicName == "SingleLine").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", field.Props.Single(static prop => prop.PublicName == "Loading").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconColorValue?", field.Props.Single(static prop => prop.PublicName == "IconColor").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldSlotContext", field.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VFieldLabelSlotContext", field.Slots.Single(static slot => slot.Name == "label").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLoaderSlotContext", field.Slots.Single(static slot => slot.Name == "loader").Parameters[0].TypeName);

        var selectionControl = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSelectionControl");
        Assert.AreEqual("modelValue", selectionControl.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", selectionControl.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("defaultsTarget", selectionControl.Props.Single(static prop => prop.PublicName == "DefaultsTarget").Name);
        Assert.AreEqual("valueComparator", selectionControl.Props.Single(static prop => prop.PublicName == "ValueComparator").Name);
        Assert.AreEqual("trueValue", selectionControl.Props.Single(static prop => prop.PublicName == "TrueValue").Name);
        Assert.AreEqual("falseValue", selectionControl.Props.Single(static prop => prop.PublicName == "FalseValue").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", selectionControl.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyNullableBoolean?", selectionControl.Props.Single(static prop => prop.PublicName == "Multiple").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlDefaultSlotContext", selectionControl.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlLabelSlotContext", selectionControl.Slots.Single(static slot => slot.Name == "label").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectionControlInputSlotContext", selectionControl.Slots.Single(static slot => slot.Name == "input").Parameters[0].TypeName);

        var selectionControlGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSelectionControlGroup");
        Assert.AreEqual("modelValue", selectionControlGroup.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", selectionControlGroup.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("defaultsTarget", selectionControlGroup.Props.Single(static prop => prop.PublicName == "DefaultsTarget").Name);
        Assert.AreEqual("valueComparator", selectionControlGroup.Props.Single(static prop => prop.PublicName == "ValueComparator").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", selectionControlGroup.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.IsTrue(selectionControlGroup.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var window = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VWindow");
        Assert.AreEqual("modelValue", window.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", window.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("showArrows", window.Props.Single(static prop => prop.PublicName == "ShowArrows").Name);
        Assert.AreEqual("selectedClass", window.Props.Single(static prop => prop.PublicName == "SelectedClass").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", window.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyWindowShowArrowsValue?", window.Props.Single(static prop => prop.PublicName == "ShowArrows").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTouchValue?", window.Props.Single(static prop => prop.PublicName == "Touch").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMandatoryValue?", window.Props.Single(static prop => prop.PublicName == "Mandatory").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowSlotContext", window.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowSlotContext", window.Slots.Single(static slot => slot.Name == "additional").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowControlSlotContext", window.Slots.Single(static slot => slot.Name == "prev").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowControlSlotContext", window.Slots.Single(static slot => slot.Name == "next").Parameters[0].TypeName);

        var carousel = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCarousel");
        Assert.AreEqual("modelValue", carousel.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", carousel.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("delimiterIcon", carousel.Props.Single(static prop => prop.PublicName == "DelimiterIcon").Name);
        Assert.AreEqual("hideDelimiters", carousel.Props.Single(static prop => prop.PublicName == "HideDelimiters").Name);
        Assert.AreEqual("hideDelimiterBackground", carousel.Props.Single(static prop => prop.PublicName == "HideDelimiterBackground").Name);
        Assert.AreEqual("verticalDelimiters", carousel.Props.Single(static prop => prop.PublicName == "VerticalDelimiters").Name);
        Assert.AreEqual("showArrows", carousel.Props.Single(static prop => prop.PublicName == "ShowArrows").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", carousel.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", carousel.Props.Single(static prop => prop.PublicName == "DelimiterIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", carousel.Props.Single(static prop => prop.PublicName == "Height").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", carousel.Props.Single(static prop => prop.PublicName == "Interval").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", carousel.Props.Single(static prop => prop.PublicName == "Progress").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyCarouselVerticalDelimiters?", carousel.Props.Single(static prop => prop.PublicName == "VerticalDelimiters").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowSlotContext", carousel.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowControlSlotContext", carousel.Slots.Single(static slot => slot.Name == "prev").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VWindowControlSlotContext", carousel.Slots.Single(static slot => slot.Name == "next").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VCarouselItemSlotContext", carousel.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);

        var stepper = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VStepper");
        Assert.AreEqual("modelValue", stepper.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", stepper.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("altLabels", stepper.Props.Single(static prop => prop.PublicName == "AltLabels").Name);
        Assert.AreEqual("bgColor", stepper.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("completeIcon", stepper.Props.Single(static prop => prop.PublicName == "CompleteIcon").Name);
        Assert.AreEqual("hideActions", stepper.Props.Single(static prop => prop.PublicName == "HideActions").Name);
        Assert.AreEqual("itemTitle", stepper.Props.Single(static prop => prop.PublicName == "ItemTitle").Name);
        Assert.AreEqual("itemValue", stepper.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("nonLinear", stepper.Props.Single(static prop => prop.PublicName == "NonLinear").Name);
        Assert.AreEqual("prevText", stepper.Props.Single(static prop => prop.PublicName == "PrevText").Name);
        Assert.AreEqual("nextText", stepper.Props.Single(static prop => prop.PublicName == "NextText").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", stepper.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStepperItems?", stepper.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", stepper.Props.Single(static prop => prop.PublicName == "CompleteIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMandatoryValue?", stepper.Props.Single(static prop => prop.PublicName == "Mandatory").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMobileValue?", stepper.Props.Single(static prop => prop.PublicName == "Mobile").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDisplayBreakpoint?", stepper.Props.Single(static prop => prop.PublicName == "MobileBreakpoint").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", stepper.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", stepper.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperNavigationSlotContext", stepper.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperNavigationSlotContext", stepper.Slots.Single(static slot => slot.Name == "actions").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "header").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "header-item").Parameters[0].TypeName);
        Assert.AreEqual("header-item.${string}", stepper.Slots.Single(static slot => slot.Name == "header-item").NamePattern);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "icon").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "title").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "subtitle").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperContentItemSlotContext", stepper.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("item.${string}", stepper.Slots.Single(static slot => slot.Name == "item").NamePattern);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperActionButtonSlotContext", stepper.Slots.Single(static slot => slot.Name == "prev").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperActionButtonSlotContext", stepper.Slots.Single(static slot => slot.Name == "next").Parameters[0].TypeName);

        var stepperVertical = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VStepperVertical");
        Assert.AreEqual("vuetify/labs/components", stepperVertical.ImportSpecifier);
        Assert.AreEqual("VStepperVertical", stepperVertical.ExportName);
        Assert.AreEqual("modelValue", stepperVertical.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", stepperVertical.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("variant", stepperVertical.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("collapseIcon", stepperVertical.Props.Single(static prop => prop.PublicName == "CollapseIcon").Name);
        Assert.AreEqual("expandIcon", stepperVertical.Props.Single(static prop => prop.PublicName == "ExpandIcon").Name);
        Assert.AreEqual("hideActions", stepperVertical.Props.Single(static prop => prop.PublicName == "HideActions").Name);
        Assert.AreEqual("itemTitle", stepperVertical.Props.Single(static prop => prop.PublicName == "ItemTitle").Name);
        Assert.AreEqual("itemValue", stepperVertical.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", stepperVertical.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyExpansionPanelVariant?", stepperVertical.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMandatoryValue?", stepperVertical.Props.Single(static prop => prop.PublicName == "Mandatory").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStepperItems?", stepperVertical.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", stepperVertical.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", stepperVertical.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalSlotContext", stepperVertical.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalActionSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "actions").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalItemSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "icon").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalItemSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "title").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalItemSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "subtitle").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalActionSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "prev").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalActionSlotContext", stepperVertical.Slots.Single(static slot => slot.Name == "next").Parameters[0].TypeName);
        var verticalHeaderItemSlot = stepperVertical.Slots.Single(static slot => slot.Name == "header-item");
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalItemSlotContext", verticalHeaderItemSlot.Parameters[0].TypeName);
        Assert.AreEqual("header-item.${string}", verticalHeaderItemSlot.NamePattern);
        Assert.IsTrue(verticalHeaderItemSlot.PatternOnly);
        var verticalItemSlot = stepperVertical.Slots.Single(static slot => slot.Name == "item");
        Assert.AreEqual("ECMAScript.Vuetify.VStepperVerticalItemSlotContext", verticalItemSlot.Parameters[0].TypeName);
        Assert.AreEqual("item.${string}", verticalItemSlot.NamePattern);
        Assert.IsTrue(verticalItemSlot.PatternOnly);

        var colorPicker = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VColorPicker");
        Assert.AreEqual("modelValue", colorPicker.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", colorPicker.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("mode", colorPicker.Props.Single(static prop => prop.PublicName == "Mode").Name);
        Assert.AreEqual("update:mode", colorPicker.Emits.Single(static emit => emit.RazorAlias == "ModeChanged").Name);
        Assert.AreEqual("modes", colorPicker.Props.Single(static prop => prop.PublicName == "Modes").Name);
        Assert.AreEqual("canvasHeight", colorPicker.Props.Single(static prop => prop.PublicName == "CanvasHeight").Name);
        Assert.AreEqual("dotSize", colorPicker.Props.Single(static prop => prop.PublicName == "DotSize").Name);
        Assert.AreEqual("hideCanvas", colorPicker.Props.Single(static prop => prop.PublicName == "HideCanvas").Name);
        Assert.AreEqual("hideSliders", colorPicker.Props.Single(static prop => prop.PublicName == "HideSliders").Name);
        Assert.AreEqual("hideInputs", colorPicker.Props.Single(static prop => prop.PublicName == "HideInputs").Name);
        Assert.AreEqual("showSwatches", colorPicker.Props.Single(static prop => prop.PublicName == "ShowSwatches").Name);
        Assert.AreEqual("swatches", colorPicker.Props.Single(static prop => prop.PublicName == "Swatches").Name);
        Assert.AreEqual("swatchesMaxHeight", colorPicker.Props.Single(static prop => prop.PublicName == "SwatchesMaxHeight").Name);
        Assert.AreEqual("bgColor", colorPicker.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("hideHeader", colorPicker.Props.Single(static prop => prop.PublicName == "HideHeader").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyColorValue?", colorPicker.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyColorPickerMode?", colorPicker.Props.Single(static prop => prop.PublicName == "Mode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyColorPickerModes?", colorPicker.Props.Single(static prop => prop.PublicName == "Modes").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyColorPickerSwatches?", colorPicker.Props.Single(static prop => prop.PublicName == "Swatches").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", colorPicker.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", colorPicker.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.IsTrue(colorPicker.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(colorPicker.Slots.Single(static slot => slot.Name == "header").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(colorPicker.Slots.Single(static slot => slot.Name == "actions").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(colorPicker.Slots.Single(static slot => slot.Name == "title").Parameters.IsDefaultOrEmpty);

        var datePicker = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDatePicker");
        Assert.AreEqual("modelValue", datePicker.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", datePicker.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("multiple", datePicker.Props.Single(static prop => prop.PublicName == "Multiple").Name);
        Assert.AreEqual("min", datePicker.Props.Single(static prop => prop.PublicName == "Min").Name);
        Assert.AreEqual("max", datePicker.Props.Single(static prop => prop.PublicName == "Max").Name);
        Assert.AreEqual("year", datePicker.Props.Single(static prop => prop.PublicName == "Year").Name);
        Assert.AreEqual("update:year", datePicker.Emits.Single(static emit => emit.RazorAlias == "YearChanged").Name);
        Assert.AreEqual("month", datePicker.Props.Single(static prop => prop.PublicName == "Month").Name);
        Assert.AreEqual("update:month", datePicker.Emits.Single(static emit => emit.RazorAlias == "MonthChanged").Name);
        Assert.AreEqual("viewMode", datePicker.Props.Single(static prop => prop.PublicName == "ViewMode").Name);
        Assert.AreEqual("update:viewMode", datePicker.Emits.Single(static emit => emit.RazorAlias == "ViewModeChanged").Name);
        Assert.AreEqual("active", datePicker.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("showAdjacentMonths", datePicker.Props.Single(static prop => prop.PublicName == "ShowAdjacentMonths").Name);
        Assert.AreEqual("weeksInMonth", datePicker.Props.Single(static prop => prop.PublicName == "WeeksInMonth").Name);
        Assert.AreEqual("firstDayOfWeek", datePicker.Props.Single(static prop => prop.PublicName == "FirstDayOfWeek").Name);
        Assert.AreEqual("allowedDates", datePicker.Props.Single(static prop => prop.PublicName == "AllowedDates").Name);
        Assert.AreEqual("hideWeekdays", datePicker.Props.Single(static prop => prop.PublicName == "HideWeekdays").Name);
        Assert.AreEqual("showWeek", datePicker.Props.Single(static prop => prop.PublicName == "ShowWeek").Name);
        Assert.AreEqual("reverseTransition", datePicker.Props.Single(static prop => prop.PublicName == "ReverseTransition").Name);
        Assert.AreEqual("controlHeight", datePicker.Props.Single(static prop => prop.PublicName == "ControlHeight").Name);
        Assert.AreEqual("modeIcon", datePicker.Props.Single(static prop => prop.PublicName == "ModeIcon").Name);
        Assert.AreEqual("header", datePicker.Props.Single(static prop => prop.PublicName == "HeaderText").Name);
        Assert.AreEqual("headerColor", datePicker.Props.Single(static prop => prop.PublicName == "HeaderColor").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerModelValue?", datePicker.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerMultipleValue?", datePicker.Props.Single(static prop => prop.PublicName == "Multiple").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerViewMode?", datePicker.Props.Single(static prop => prop.PublicName == "ViewMode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyCalendarWeekdays?", datePicker.Props.Single(static prop => prop.PublicName == "Weekdays").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDatePickerAllowedDatesValue?", datePicker.Props.Single(static prop => prop.PublicName == "AllowedDates").TypeName);
        Assert.IsTrue(datePicker.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(datePicker.Slots.Single(static slot => slot.Name == "actions").Parameters.IsDefaultOrEmpty);
        var datePickerHeaderSlot = datePicker.Slots.Single(static slot => slot.Name == "header");
        Assert.AreEqual("HeaderContent", datePickerHeaderSlot.PublicName);
        Assert.AreEqual("ECMAScript.Vuetify.VDatePickerHeaderSlotContext", datePickerHeaderSlot.Parameters[0].TypeName);
        Assert.IsTrue(datePicker.Slots.Single(static slot => slot.Name == "title").Parameters.IsDefaultOrEmpty);

        var tabsWindow = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTabsWindow");
        Assert.AreEqual("modelValue", tabsWindow.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", tabsWindow.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", tabsWindow.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.IsTrue(tabsWindow.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var tabsWindowItem = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTabsWindowItem");
        Assert.AreEqual("value", tabsWindowItem.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("reverseTransition", tabsWindowItem.Props.Single(static prop => prop.PublicName == "ReverseTransition").Name);
        Assert.AreEqual("group:selected", tabsWindowItem.Emits.Single(static emit => emit.RazorAlias == "GroupSelected").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", tabsWindowItem.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", tabsWindowItem.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupSelectedEvent", tabsWindowItem.Emits.Single(static emit => emit.RazorAlias == "GroupSelected").PayloadTypeName);
        Assert.IsTrue(tabsWindowItem.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);

        var noSsr = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VNoSsr");
        Assert.IsTrue(noSsr.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var systemBar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSystemBar");
        Assert.AreEqual("window", systemBar.Props.Single(static prop => prop.PublicName == "Window").Name);
        Assert.AreEqual("order", systemBar.Props.Single(static prop => prop.PublicName == "Order").Name);
        Assert.AreEqual("tile", systemBar.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", systemBar.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);

        var themeProvider = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VThemeProvider");
        Assert.AreEqual("withBackground", themeProvider.Props.Single(static prop => prop.PublicName == "WithBackground").Name);
        Assert.AreEqual("theme", themeProvider.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", themeProvider.Props.Single(static prop => prop.PublicName == "Tag").Name);

        var dialog = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDialog");
        Assert.AreEqual("modelValue", dialog.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", dialog.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("afterEnter", dialog.Emits.Single(static emit => emit.RazorAlias == "AfterEnter").Name);
        Assert.AreEqual("afterLeave", dialog.Emits.Single(static emit => emit.RazorAlias == "AfterLeave").Name);
        Assert.AreEqual("click:outside", dialog.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").Name);
        Assert.AreEqual("keydown", dialog.Emits.Single(static emit => emit.RazorAlias == "Keydown").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", dialog.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").PayloadTypeName);
        Assert.AreEqual("ECMAScript.KeyboardEvent", dialog.Emits.Single(static emit => emit.RazorAlias == "Keydown").PayloadTypeName);
        Assert.AreEqual("absolute", dialog.Props.Single(static prop => prop.PublicName == "Absolute").Name);
        Assert.AreEqual("attach", dialog.Props.Single(static prop => prop.PublicName == "Attach").Name);
        Assert.AreEqual("contained", dialog.Props.Single(static prop => prop.PublicName == "Contained").Name);
        Assert.AreEqual("disabled", dialog.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("eager", dialog.Props.Single(static prop => prop.PublicName == "Eager").Name);
        Assert.AreEqual("fullscreen", dialog.Props.Single(static prop => prop.PublicName == "Fullscreen").Name);
        Assert.AreEqual("noClickAnimation", dialog.Props.Single(static prop => prop.PublicName == "NoClickAnimation").Name);
        Assert.AreEqual("persistent", dialog.Props.Single(static prop => prop.PublicName == "Persistent").Name);
        Assert.AreEqual("retainFocus", dialog.Props.Single(static prop => prop.PublicName == "RetainFocus").Name);
        Assert.AreEqual("scrollable", dialog.Props.Single(static prop => prop.PublicName == "Scrollable").Name);
        Assert.AreEqual("closeOnBack", dialog.Props.Single(static prop => prop.PublicName == "CloseOnBack").Name);
        Assert.AreEqual("closeOnContentClick", dialog.Props.Single(static prop => prop.PublicName == "CloseOnContentClick").Name);
        Assert.AreEqual("openOnClick", dialog.Props.Single(static prop => prop.PublicName == "OpenOnClick").Name);
        Assert.AreEqual("openOnFocus", dialog.Props.Single(static prop => prop.PublicName == "OpenOnFocus").Name);
        Assert.AreEqual("openOnHover", dialog.Props.Single(static prop => prop.PublicName == "OpenOnHover").Name);
        Assert.AreEqual("openDelay", dialog.Props.Single(static prop => prop.PublicName == "OpenDelay").Name);
        Assert.AreEqual("closeDelay", dialog.Props.Single(static prop => prop.PublicName == "CloseDelay").Name);
        Assert.AreEqual("activatorProps", dialog.Props.Single(static prop => prop.PublicName == "ActivatorProps").Name);
        Assert.AreEqual("contentProps", dialog.Props.Single(static prop => prop.PublicName == "ContentProps").Name);
        Assert.AreEqual("contentClass", dialog.Props.Single(static prop => prop.PublicName == "ContentClass").Name);
        Assert.AreEqual("maxWidth", dialog.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("width", dialog.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("scrollStrategy", dialog.Props.Single(static prop => prop.PublicName == "ScrollStrategy").Name);
        Assert.AreEqual("location", dialog.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("origin", dialog.Props.Single(static prop => prop.PublicName == "Origin").Name);
        Assert.AreEqual("offset", dialog.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("locationStrategy", dialog.Props.Single(static prop => prop.PublicName == "LocationStrategy").Name);
        Assert.AreEqual("scrim", dialog.Props.Single(static prop => prop.PublicName == "Scrim").Name);
        Assert.AreEqual("theme", dialog.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("transition", dialog.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("zIndex", dialog.Props.Single(static prop => prop.PublicName == "ZIndex").Name);
        Assert.AreEqual("class", dialog.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", dialog.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("height", dialog.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("maxHeight", dialog.Props.Single(static prop => prop.PublicName == "MaxHeight").Name);
        Assert.AreEqual("minHeight", dialog.Props.Single(static prop => prop.PublicName == "MinHeight").Name);
        Assert.AreEqual("minWidth", dialog.Props.Single(static prop => prop.PublicName == "MinWidth").Name);
        Assert.AreEqual("opacity", dialog.Props.Single(static prop => prop.PublicName == "Opacity").Name);
        Assert.AreEqual("target", dialog.Props.Single(static prop => prop.PublicName == "Target").Name);
        Assert.AreEqual("activator", dialog.Props.Single(static prop => prop.PublicName == "ActivatorTarget").Name);
        Assert.AreEqual("additionalAttributes", dialog.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(dialog.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAttachTarget?", dialog.Props.Single(static prop => prop.PublicName == "Attach").TypeName);
        Assert.AreEqual("bool?", dialog.Props.Single(static prop => prop.PublicName == "OpenOnClick").TypeName);
        Assert.AreEqual("bool?", dialog.Props.Single(static prop => prop.PublicName == "OpenOnFocus").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", dialog.Props.Single(static prop => prop.PublicName == "ActivatorProps").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", dialog.Props.Single(static prop => prop.PublicName == "ContentProps").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", dialog.Props.Single(static prop => prop.PublicName == "ContentClass").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", dialog.Props.Single(static prop => prop.PublicName == "MaxWidth").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", dialog.Props.Single(static prop => prop.PublicName == "Width").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyScrollStrategy?", dialog.Props.Single(static prop => prop.PublicName == "ScrollStrategy").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", dialog.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOriginValue?", dialog.Props.Single(static prop => prop.PublicName == "Origin").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOverlayOffsetValue?", dialog.Props.Single(static prop => prop.PublicName == "Offset").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocationStrategy?", dialog.Props.Single(static prop => prop.PublicName == "LocationStrategy").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyScrimValue?", dialog.Props.Single(static prop => prop.PublicName == "Scrim").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", dialog.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", dialog.Props.Single(static prop => prop.PublicName == "ZIndex").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", dialog.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", dialog.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDialogTarget?", dialog.Props.Single(static prop => prop.PublicName == "Target").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDialogActivatorTarget?", dialog.Props.Single(static prop => prop.PublicName == "ActivatorTarget").TypeName);
        var activator = dialog.Slots.Single(static slot => slot.Name == "activator");
        Assert.HasCount(1, activator.Parameters);
        Assert.AreEqual("context", activator.Parameters[0].Name);
        Assert.AreEqual("ECMAScript.Vuetify.VDialogActivatorContext", activator.Parameters[0].TypeName);

        var app = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VApp");
        Assert.AreEqual("fullHeight", app.Props.Single(static prop => prop.PublicName == "FullHeight").Name);
        Assert.IsTrue(app.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var appBar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAppBar");
        Assert.AreEqual("modelValue", appBar.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", appBar.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("location", appBar.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAppBarLocation?", appBar.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", appBar.Props.Single(static prop => prop.PublicName == "Height").TypeName);

        var main = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VMain");
        Assert.AreEqual("scrollable", main.Props.Single(static prop => prop.PublicName == "Scrollable").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", main.Props.Single(static prop => prop.PublicName == "MinHeight").TypeName);
        Assert.IsTrue(main.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var navigationDrawer = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VNavigationDrawer");
        Assert.AreEqual("modelValue", navigationDrawer.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", navigationDrawer.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("rail", navigationDrawer.Props.Single(static prop => prop.PublicName == "Rail").Name);
        Assert.AreEqual("update:rail", navigationDrawer.Emits.Single(static emit => emit.RazorAlias == "RailChanged").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyNavigationDrawerLocation?", navigationDrawer.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyScrimValue?", navigationDrawer.Props.Single(static prop => prop.PublicName == "Scrim").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", navigationDrawer.Props.Single(static prop => prop.PublicName == "Width").TypeName);

        var container = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VContainer");
        Assert.AreEqual("tag", container.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("height", container.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("maxWidth", container.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("class", container.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", container.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("fluid", container.Props.Single(static prop => prop.PublicName == "Fluid").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", container.Props.Single(static prop => prop.PublicName == "Height").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", container.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", container.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(container.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var row = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRow");
        Assert.AreEqual("tag", row.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("class", row.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", row.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("alignContentSm", row.Props.Single(static prop => prop.PublicName == "AlignContentSm").Name);
        Assert.AreEqual("justifyMd", row.Props.Single(static prop => prop.PublicName == "JustifyMd").Name);
        Assert.AreEqual("alignLg", row.Props.Single(static prop => prop.PublicName == "AlignLg").Name);
        Assert.AreEqual("dense", row.Props.Single(static prop => prop.PublicName == "Dense").Name);
        Assert.AreEqual("noGutters", row.Props.Single(static prop => prop.PublicName == "NoGutters").Name);
        Assert.AreEqual("align", row.Props.Single(static prop => prop.PublicName == "Align").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", row.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", row.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(row.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var column = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCol");
        CollectionAssert.AreEqual(new[] { "vuetify" }, column.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, column.StyleDependencies.ToArray());
        Assert.AreEqual("tag", column.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("class", column.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", column.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("alignSelf", column.Props.Single(static prop => prop.PublicName == "AlignSelf").Name);
        Assert.AreEqual("orderMd", column.Props.Single(static prop => prop.PublicName == "OrderMd").Name);
        Assert.AreEqual("offsetLg", column.Props.Single(static prop => prop.PublicName == "OffsetLg").Name);
        Assert.AreEqual("sm", column.Props.Single(static prop => prop.PublicName == "Sm").Name);
        Assert.AreEqual("cols", column.Props.Single(static prop => prop.PublicName == "Cols").Name);
        Assert.AreEqual("md", column.Props.Single(static prop => prop.PublicName == "Md").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGridSpanValue?", column.Props.Single(static prop => prop.PublicName == "Cols").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGridSpanValue?", column.Props.Single(static prop => prop.PublicName == "Md").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", column.Props.Single(static prop => prop.PublicName == "Order").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", column.Props.Single(static prop => prop.PublicName == "OffsetMd").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", column.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", column.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(column.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var toolbar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VToolbar");
        CollectionAssert.AreEqual(new[] { "vuetify" }, toolbar.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, toolbar.StyleDependencies.ToArray());
        Assert.AreEqual("theme", toolbar.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", toolbar.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", toolbar.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("tile", toolbar.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("elevation", toolbar.Props.Single(static prop => prop.PublicName == "Elevation").Name);
        Assert.AreEqual("class", toolbar.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", toolbar.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("border", toolbar.Props.Single(static prop => prop.PublicName == "Border").Name);
        Assert.AreEqual("absolute", toolbar.Props.Single(static prop => prop.PublicName == "Absolute").Name);
        Assert.AreEqual("collapse", toolbar.Props.Single(static prop => prop.PublicName == "Collapse").Name);
        Assert.AreEqual("color", toolbar.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", toolbar.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("extended", toolbar.Props.Single(static prop => prop.PublicName == "Extended").Name);
        Assert.AreEqual("extensionHeight", toolbar.Props.Single(static prop => prop.PublicName == "ExtensionHeight").Name);
        Assert.AreEqual("flat", toolbar.Props.Single(static prop => prop.PublicName == "Flat").Name);
        Assert.AreEqual("floating", toolbar.Props.Single(static prop => prop.PublicName == "Floating").Name);
        Assert.AreEqual("height", toolbar.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("image", toolbar.Props.Single(static prop => prop.PublicName == "Image").Name);
        Assert.AreEqual("title", toolbar.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("additionalAttributes", toolbar.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(toolbar.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", toolbar.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", toolbar.Props.Single(static prop => prop.PublicName == "Elevation").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", toolbar.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", toolbar.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", toolbar.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyToolbarDensityValue?", toolbar.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", toolbar.Props.Single(static prop => prop.PublicName == "ExtensionHeight").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", toolbar.Props.Single(static prop => prop.PublicName == "Height").TypeName);
        Assert.IsTrue(toolbar.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("image", toolbar.Slots.Single(static slot => slot.PublicName == "ImageContent").Name);
        Assert.AreEqual("prepend", toolbar.Slots.Single(static slot => slot.PublicName == "Prepend").Name);
        Assert.AreEqual("append", toolbar.Slots.Single(static slot => slot.PublicName == "Append").Name);
        Assert.AreEqual("title", toolbar.Slots.Single(static slot => slot.PublicName == "TitleContent").Name);
        Assert.AreEqual("extension", toolbar.Slots.Single(static slot => slot.PublicName == "Extension").Name);

        var toolbarItems = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VToolbarItems");
        Assert.AreEqual("color", toolbarItems.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("variant", toolbarItems.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("class", toolbarItems.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", toolbarItems.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", toolbarItems.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", toolbarItems.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", toolbarItems.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(toolbarItems.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var toolbarTitle = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VToolbarTitle");
        Assert.AreEqual("tag", toolbarTitle.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("class", toolbarTitle.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", toolbarTitle.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("text", toolbarTitle.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", toolbarTitle.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", toolbarTitle.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(toolbarTitle.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("text", toolbarTitle.Slots.Single(static slot => slot.PublicName == "TextContent").Name);

        var textarea = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTextarea");
        Assert.AreEqual("rows", textarea.Props.Single(static prop => prop.PublicName == "Rows").Name);
        Assert.AreEqual("modelValue", textarea.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textarea.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("placeholder", textarea.Props.Single(static prop => prop.PublicName == "Placeholder").Name);
        Assert.AreEqual("hint", textarea.Props.Single(static prop => prop.PublicName == "Hint").Name);
        Assert.AreEqual("persistentHint", textarea.Props.Single(static prop => prop.PublicName == "PersistentHint").Name);
        Assert.AreEqual("readonly", textarea.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("autoGrow", textarea.Props.Single(static prop => prop.PublicName == "AutoGrow").Name);
        Assert.AreEqual("counter", textarea.Props.Single(static prop => prop.PublicName == "Counter").Name);
        Assert.AreEqual("maxRows", textarea.Props.Single(static prop => prop.PublicName == "MaxRows").Name);
        Assert.AreEqual("variant", textarea.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("density", textarea.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("additionalAttributes", textarea.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(textarea.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", textarea.Props.Single(static prop => prop.PublicName == "Rows").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", textarea.Props.Single(static prop => prop.PublicName == "MaxRows").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyCounterValue?", textarea.Props.Single(static prop => prop.PublicName == "Counter").TypeName);

        var toggle = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSwitch");
        Assert.AreEqual("modelValue", toggle.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", toggle.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("color", toggle.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", toggle.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("readonly", toggle.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("inset", toggle.Props.Single(static prop => prop.PublicName == "Inset").Name);
        Assert.AreEqual("loading", toggle.Props.Single(static prop => prop.PublicName == "Loading").Name);
        Assert.AreEqual("hideDetails", toggle.Props.Single(static prop => prop.PublicName == "HideDetails").Name);
        Assert.AreEqual("additionalAttributes", toggle.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(toggle.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", toggle.Props.Single(static prop => prop.PublicName == "Loading").TypeName);

        var select = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSelect");
        Assert.AreEqual("modelValue", select.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", select.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("modelValue", select.Props.Single(static prop => prop.PublicName == "SelectedValue").Name);
        Assert.AreEqual("update:modelValue", select.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Name);
        Assert.IsTrue(select.Props.Single(static prop => prop.PublicName == "SelectedValue").AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, select.Props.Single(static prop => prop.PublicName == "SelectedValue").Kind);
        Assert.AreEqual(VueEmitKind.ModelUpdate, select.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Kind);
        Assert.AreEqual("multiple", select.Props.Single(static prop => prop.PublicName == "Multiple").Name);
        Assert.AreEqual("items", select.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemTitle", select.Props.Single(static prop => prop.PublicName == "ItemTitle").Name);
        Assert.AreEqual("itemValue", select.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("itemProps", select.Props.Single(static prop => prop.PublicName == "ItemProps").Name);
        Assert.AreEqual("returnObject", select.Props.Single(static prop => prop.PublicName == "ReturnObject").Name);
        Assert.AreEqual("chips", select.Props.Single(static prop => prop.PublicName == "Chips").Name);
        Assert.AreEqual("clearable", select.Props.Single(static prop => prop.PublicName == "Clearable").Name);
        Assert.AreEqual("readonly", select.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("menuProps", select.Props.Single(static prop => prop.PublicName == "MenuProps").Name);
        Assert.AreEqual("density", select.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("variant", select.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("placeholder", select.Props.Single(static prop => prop.PublicName == "Placeholder").Name);
        Assert.AreEqual("persistentPlaceholder", select.Props.Single(static prop => prop.PublicName == "PersistentPlaceholder").Name);
        Assert.AreEqual("prefix", select.Props.Single(static prop => prop.PublicName == "Prefix").Name);
        Assert.AreEqual("suffix", select.Props.Single(static prop => prop.PublicName == "Suffix").Name);
        Assert.AreEqual("prependInnerIcon", select.Props.Single(static prop => prop.PublicName == "PrependInnerIcon").Name);
        Assert.AreEqual("closableChips", select.Props.Single(static prop => prop.PublicName == "ClosableChips").Name);
        Assert.AreEqual("hideNoData", select.Props.Single(static prop => prop.PublicName == "HideNoData").Name);
        Assert.AreEqual("hideSelected", select.Props.Single(static prop => prop.PublicName == "HideSelected").Name);
        Assert.AreEqual("listProps", select.Props.Single(static prop => prop.PublicName == "ListProps").Name);
        Assert.AreEqual("menu", select.Props.Single(static prop => prop.PublicName == "Menu").Name);
        Assert.AreEqual("update:menu", select.Emits.Single(static emit => emit.RazorAlias == "MenuChanged").Name);
        Assert.AreEqual("update:focused", select.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("closeText", select.Props.Single(static prop => prop.PublicName == "CloseText").Name);
        Assert.AreEqual("openText", select.Props.Single(static prop => prop.PublicName == "OpenText").Name);
        Assert.AreEqual("valueComparator", select.Props.Single(static prop => prop.PublicName == "ValueComparator").Name);
        Assert.AreEqual("additionalAttributes", select.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(select.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", select.Props.Single(static prop => prop.PublicName == "MenuProps").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", select.Props.Single(static prop => prop.PublicName == "ListProps").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItems?", select.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", select.Props.Single(static prop => prop.PublicName == "ItemTitle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", select.Props.Single(static prop => prop.PublicName == "ItemValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemPropsSelector?", select.Props.Single(static prop => prop.PublicName == "ItemProps").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", select.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFieldVariant?", select.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectModelValue?", select.Props.Single(static prop => prop.PublicName == "SelectedValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectValueComparator?", select.Props.Single(static prop => prop.PublicName == "ValueComparator").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectItemSlotContext", select.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectChipSlotContext", select.Slots.Single(static slot => slot.Name == "chip").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectSelectionSlotContext", select.Slots.Single(static slot => slot.Name == "selection").Parameters[0].TypeName);
        Assert.IsTrue(select.Slots.Any(static slot => slot.Name == "prepend-item"));
        Assert.IsTrue(select.Slots.Any(static slot => slot.Name == "append-item"));
        Assert.IsTrue(select.Slots.Any(static slot => slot.Name == "no-data"));

        var autocomplete = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAutocomplete");
        Assert.AreEqual("modelValue", autocomplete.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", autocomplete.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("modelValue", autocomplete.Props.Single(static prop => prop.PublicName == "SelectedValue").Name);
        Assert.AreEqual("update:modelValue", autocomplete.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Name);
        Assert.IsTrue(autocomplete.Props.Single(static prop => prop.PublicName == "SelectedValue").AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, autocomplete.Props.Single(static prop => prop.PublicName == "SelectedValue").Kind);
        Assert.AreEqual(VueEmitKind.ModelUpdate, autocomplete.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Kind);
        Assert.AreEqual("chips", autocomplete.Props.Single(static prop => prop.PublicName == "Chips").Name);
        Assert.AreEqual("items", autocomplete.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemTitle", autocomplete.Props.Single(static prop => prop.PublicName == "ItemTitle").Name);
        Assert.AreEqual("itemValue", autocomplete.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("itemProps", autocomplete.Props.Single(static prop => prop.PublicName == "ItemProps").Name);
        Assert.AreEqual("returnObject", autocomplete.Props.Single(static prop => prop.PublicName == "ReturnObject").Name);
        Assert.AreEqual("clearable", autocomplete.Props.Single(static prop => prop.PublicName == "Clearable").Name);
        Assert.AreEqual("readonly", autocomplete.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("menuProps", autocomplete.Props.Single(static prop => prop.PublicName == "MenuProps").Name);
        Assert.AreEqual("density", autocomplete.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("variant", autocomplete.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("noDataText", autocomplete.Props.Single(static prop => prop.PublicName == "NoDataText").Name);
        Assert.AreEqual("search", autocomplete.Props.Single(static prop => prop.PublicName == "Search").Name);
        Assert.AreEqual("update:search", autocomplete.Emits.Single(static emit => emit.RazorAlias == "SearchChanged").Name);
        Assert.AreEqual("autoSelectFirst", autocomplete.Props.Single(static prop => prop.PublicName == "AutoSelectFirst").Name);
        Assert.AreEqual("customFilter", autocomplete.Props.Single(static prop => prop.PublicName == "CustomFilter").Name);
        Assert.AreEqual("customKeyFilter", autocomplete.Props.Single(static prop => prop.PublicName == "CustomKeyFilter").Name);
        Assert.AreEqual("filterKeys", autocomplete.Props.Single(static prop => prop.PublicName == "FilterKeys").Name);
        Assert.AreEqual("filterMode", autocomplete.Props.Single(static prop => prop.PublicName == "FilterMode").Name);
        Assert.AreEqual("noFilter", autocomplete.Props.Single(static prop => prop.PublicName == "NoFilter").Name);
        Assert.AreEqual("additionalAttributes", autocomplete.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(autocomplete.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", autocomplete.Props.Single(static prop => prop.PublicName == "MenuProps").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItems?", autocomplete.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", autocomplete.Props.Single(static prop => prop.PublicName == "ItemTitle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", autocomplete.Props.Single(static prop => prop.PublicName == "ItemValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemPropsSelector?", autocomplete.Props.Single(static prop => prop.PublicName == "ItemProps").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", autocomplete.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFieldVariant?", autocomplete.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectModelValue?", autocomplete.Props.Single(static prop => prop.PublicName == "SelectedValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterFunction?", autocomplete.Props.Single(static prop => prop.PublicName == "CustomFilter").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterKeyFunctions?", autocomplete.Props.Single(static prop => prop.PublicName == "CustomKeyFilter").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterKeys?", autocomplete.Props.Single(static prop => prop.PublicName == "FilterKeys").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterMode?", autocomplete.Props.Single(static prop => prop.PublicName == "FilterMode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectItemSlotContext", autocomplete.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectChipSlotContext", autocomplete.Slots.Single(static slot => slot.Name == "chip").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectSelectionSlotContext", autocomplete.Slots.Single(static slot => slot.Name == "selection").Parameters[0].TypeName);

        var combobox = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCombobox");
        Assert.AreEqual("modelValue", combobox.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", combobox.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("modelValue", combobox.Props.Single(static prop => prop.PublicName == "SelectedValue").Name);
        Assert.AreEqual("update:modelValue", combobox.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Name);
        Assert.IsTrue(combobox.Props.Single(static prop => prop.PublicName == "SelectedValue").AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, combobox.Props.Single(static prop => prop.PublicName == "SelectedValue").Kind);
        Assert.AreEqual(VueEmitKind.ModelUpdate, combobox.Emits.Single(static emit => emit.RazorAlias == "SelectedValueChanged").Kind);
        Assert.AreEqual("autoSelectFirst", combobox.Props.Single(static prop => prop.PublicName == "AutoSelectFirst").Name);
        Assert.AreEqual("clearOnSelect", combobox.Props.Single(static prop => prop.PublicName == "ClearOnSelect").Name);
        Assert.AreEqual("delimiters", combobox.Props.Single(static prop => prop.PublicName == "Delimiters").Name);
        Assert.AreEqual("hideDetails", combobox.Props.Single(static prop => prop.PublicName == "HideDetails").Name);
        Assert.AreEqual("search", combobox.Props.Single(static prop => prop.PublicName == "Search").Name);
        Assert.AreEqual("update:search", combobox.Emits.Single(static emit => emit.RazorAlias == "SearchChanged").Name);
        Assert.AreEqual("customFilter", combobox.Props.Single(static prop => prop.PublicName == "CustomFilter").Name);
        Assert.AreEqual("filterMode", combobox.Props.Single(static prop => prop.PublicName == "FilterMode").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAutoSelectFirstValue?", combobox.Props.Single(static prop => prop.PublicName == "AutoSelectFirst").TypeName);
        Assert.AreEqual("string[]?", combobox.Props.Single(static prop => prop.PublicName == "Delimiters").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItems?", combobox.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectModelValue?", combobox.Props.Single(static prop => prop.PublicName == "SelectedValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterFunction?", combobox.Props.Single(static prop => prop.PublicName == "CustomFilter").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFilterMode?", combobox.Props.Single(static prop => prop.PublicName == "FilterMode").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectItemSlotContext", combobox.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectChipSlotContext", combobox.Slots.Single(static slot => slot.Name == "chip").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSelectSelectionSlotContext", combobox.Slots.Single(static slot => slot.Name == "selection").Parameters[0].TypeName);

        var fileInput = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VFileInput");
        Assert.AreEqual("accept", fileInput.Props.Single(static prop => prop.PublicName == "Accept").Name);
        Assert.AreEqual("counter", fileInput.Props.Single(static prop => prop.PublicName == "Counter").Name);
        Assert.AreEqual("showSize", fileInput.Props.Single(static prop => prop.PublicName == "ShowSize").Name);
        Assert.AreEqual("modelValue", fileInput.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", fileInput.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("bool", fileInput.Props.Single(static prop => prop.PublicName == "Counter").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFileShowSizeValue?", fileInput.Props.Single(static prop => prop.PublicName == "ShowSize").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyFileModelValue?", fileInput.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);

        var numberInput = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VNumberInput");
        Assert.AreEqual("controlVariant", numberInput.Props.Single(static prop => prop.PublicName == "ControlVariant").Name);
        Assert.AreEqual("hideInput", numberInput.Props.Single(static prop => prop.PublicName == "HideInput").Name);
        Assert.AreEqual("precision", numberInput.Props.Single(static prop => prop.PublicName == "Precision").Name);
        Assert.AreEqual("modelValue", numberInput.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", numberInput.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsNull(numberInput.Props.SingleOrDefault(static prop => prop.PublicName == "ControlVariantHidden"));
        Assert.AreEqual("ECMAScript.Number?", numberInput.Props.Single(static prop => prop.PublicName == "Min").TypeName);
        Assert.AreEqual("ECMAScript.Number?", numberInput.Props.Single(static prop => prop.PublicName == "Precision").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyNumberInputControlVariant?", numberInput.Props.Single(static prop => prop.PublicName == "ControlVariant").TypeName);
        Assert.AreEqual("ECMAScript.Number?", numberInput.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);

        var otpInput = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VOtpInput");
        Assert.AreEqual("length", otpInput.Props.Single(static prop => prop.PublicName == "Length").Name);
        Assert.AreEqual("divider", otpInput.Props.Single(static prop => prop.PublicName == "Divider").Name);
        Assert.AreEqual("focusAll", otpInput.Props.Single(static prop => prop.PublicName == "FocusAll").Name);
        Assert.AreEqual("type", otpInput.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("modelValue", otpInput.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", otpInput.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("string?", otpInput.Props.Single(static prop => prop.PublicName == "Divider").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyInputType?", otpInput.Props.Single(static prop => prop.PublicName == "Type").TypeName);

        var radio = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRadio");
        Assert.AreEqual("modelValue", radio.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", radio.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("value", radio.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("falseIcon", radio.Props.Single(static prop => prop.PublicName == "FalseIcon").Name);
        Assert.AreEqual("trueIcon", radio.Props.Single(static prop => prop.PublicName == "TrueIcon").Name);
        Assert.AreEqual("string?", radio.Props.Single(static prop => prop.PublicName == "FalseIcon").TypeName);
        Assert.AreEqual("string?", radio.Props.Single(static prop => prop.PublicName == "TrueIcon").TypeName);

        var rangeSlider = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRangeSlider");
        Assert.AreEqual("modelValue", rangeSlider.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", rangeSlider.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("trackColor", rangeSlider.Props.Single(static prop => prop.PublicName == "TrackColor").Name);
        Assert.AreEqual("thumbLabel", rangeSlider.Props.Single(static prop => prop.PublicName == "ThumbLabel").Name);
        Assert.AreEqual("showTicks", rangeSlider.Props.Single(static prop => prop.PublicName == "ShowTicks").Name);
        Assert.AreEqual("direction", rangeSlider.Props.Single(static prop => prop.PublicName == "Direction").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRangeSliderModelValue?", rangeSlider.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Number?", rangeSlider.Props.Single(static prop => prop.PublicName == "Step").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanAlwaysValue?", rangeSlider.Props.Single(static prop => prop.PublicName == "ThumbLabel").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySliderDirection?", rangeSlider.Props.Single(static prop => prop.PublicName == "Direction").TypeName);

        var slider = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSlider");
        Assert.AreEqual("modelValue", slider.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", slider.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("trackColor", slider.Props.Single(static prop => prop.PublicName == "TrackColor").Name);
        Assert.AreEqual("thumbColor", slider.Props.Single(static prop => prop.PublicName == "ThumbColor").Name);
        Assert.AreEqual("showTicks", slider.Props.Single(static prop => prop.PublicName == "ShowTicks").Name);
        Assert.AreEqual("direction", slider.Props.Single(static prop => prop.PublicName == "Direction").Name);
        Assert.AreEqual("ECMAScript.Number?", slider.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Number?", slider.Props.Single(static prop => prop.PublicName == "Min").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanAlwaysValue?", slider.Props.Single(static prop => prop.PublicName == "ShowTicks").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySliderDirection?", slider.Props.Single(static prop => prop.PublicName == "Direction").TypeName);

        var list = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VList");
        Assert.AreEqual("density", list.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("additionalAttributes", list.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(list.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", list.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.IsTrue(list.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var listItem = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VListItem");
        Assert.AreEqual("title", listItem.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("subtitle", listItem.Props.Single(static prop => prop.PublicName == "Subtitle").Name);
        Assert.IsTrue(listItem.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var alert = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAlert");
        Assert.AreEqual("modelValue", alert.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", alert.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("click:close", alert.Emits.Single(static emit => emit.RazorAlias == "ClickClose").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", alert.Emits.Single(static emit => emit.RazorAlias == "ClickClose").PayloadTypeName);
        Assert.AreEqual("type", alert.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("variant", alert.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("color", alert.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("theme", alert.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", alert.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", alert.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("tile", alert.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("position", alert.Props.Single(static prop => prop.PublicName == "Position").Name);
        Assert.AreEqual("location", alert.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("elevation", alert.Props.Single(static prop => prop.PublicName == "Elevation").Name);
        Assert.AreEqual("height", alert.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("maxHeight", alert.Props.Single(static prop => prop.PublicName == "MaxHeight").Name);
        Assert.AreEqual("maxWidth", alert.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("minHeight", alert.Props.Single(static prop => prop.PublicName == "MinHeight").Name);
        Assert.AreEqual("minWidth", alert.Props.Single(static prop => prop.PublicName == "MinWidth").Name);
        Assert.AreEqual("width", alert.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("density", alert.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("class", alert.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", alert.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("border", alert.Props.Single(static prop => prop.PublicName == "Border").Name);
        Assert.AreEqual("borderColor", alert.Props.Single(static prop => prop.PublicName == "BorderColor").Name);
        Assert.AreEqual("closable", alert.Props.Single(static prop => prop.PublicName == "Closable").Name);
        Assert.AreEqual("closeIcon", alert.Props.Single(static prop => prop.PublicName == "CloseIcon").Name);
        Assert.AreEqual("closeLabel", alert.Props.Single(static prop => prop.PublicName == "CloseLabel").Name);
        Assert.AreEqual("icon", alert.Props.Single(static prop => prop.PublicName == "Icon").Name);
        Assert.AreEqual("prominent", alert.Props.Single(static prop => prop.PublicName == "Prominent").Name);
        Assert.AreEqual("title", alert.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("additionalAttributes", alert.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(alert.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAlertType?", alert.Props.Single(static prop => prop.PublicName == "Type").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", alert.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", alert.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", alert.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyPosition?", alert.Props.Single(static prop => prop.PublicName == "Position").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", alert.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", alert.Props.Single(static prop => prop.PublicName == "Elevation").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", alert.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", alert.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAlertBorderValue?", alert.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", alert.Props.Single(static prop => prop.PublicName == "CloseIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyAlertIconValue?", alert.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.IsTrue(alert.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("prepend", alert.Slots.Single(static slot => slot.PublicName == "Prepend").Name);
        Assert.AreEqual("title", alert.Slots.Single(static slot => slot.PublicName == "TitleContent").Name);
        Assert.AreEqual("text", alert.Slots.Single(static slot => slot.PublicName == "TextContent").Name);
        Assert.AreEqual("append", alert.Slots.Single(static slot => slot.PublicName == "Append").Name);
        var alertClose = alert.Slots.Single(static slot => slot.PublicName == "Close");
        Assert.AreEqual("close", alertClose.Name);
        Assert.AreEqual("ECMAScript.Vuetify.VAlertCloseSlotContext", alertClose.Parameters[0].TypeName);

        var chip = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VChip");
        Assert.AreEqual("click", chip.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.AreEqual("update:modelValue", chip.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("click:close", chip.Emits.Single(static emit => emit.RazorAlias == "ClickClose").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", chip.Emits.Single(static emit => emit.RazorAlias == "ClickClose").PayloadTypeName);
        Assert.AreEqual("group:selected", chip.Emits.Single(static emit => emit.RazorAlias == "GroupSelected").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupSelectedEvent", chip.Emits.Single(static emit => emit.RazorAlias == "GroupSelected").PayloadTypeName);
        Assert.AreEqual("modelValue", chip.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("activeClass", chip.Props.Single(static prop => prop.PublicName == "ActiveClass").Name);
        Assert.AreEqual("appendAvatar", chip.Props.Single(static prop => prop.PublicName == "AppendAvatar").Name);
        Assert.AreEqual("baseColor", chip.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("closeIcon", chip.Props.Single(static prop => prop.PublicName == "CloseIcon").Name);
        Assert.AreEqual("filterIcon", chip.Props.Single(static prop => prop.PublicName == "FilterIcon").Name);
        Assert.AreEqual("prependAvatar", chip.Props.Single(static prop => prop.PublicName == "PrependAvatar").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyGroupModelValue?", chip.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", chip.Props.Single(static prop => prop.PublicName == "CloseIcon").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", chip.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        var chipDefaultContent = chip.Slots.Single(static slot => slot.PublicName == "DefaultContent");
        Assert.AreEqual("default", chipDefaultContent.Name);
        Assert.AreEqual("ECMAScript.Vuetify.VChipDefaultSlotContext", chipDefaultContent.Parameters[0].TypeName);
        Assert.AreEqual("label", chip.Slots.Single(static slot => slot.PublicName == "LabelContent").Name);
        Assert.AreEqual("prepend", chip.Slots.Single(static slot => slot.PublicName == "Prepend").Name);
        Assert.AreEqual("append", chip.Slots.Single(static slot => slot.PublicName == "Append").Name);
        Assert.AreEqual("close", chip.Slots.Single(static slot => slot.PublicName == "Close").Name);
        Assert.AreEqual("filter", chip.Slots.Single(static slot => slot.PublicName == "FilterContent").Name);
        Assert.IsTrue(chip.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var form = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VForm");
        Assert.AreEqual("fastFail", form.Props.Single(static prop => prop.PublicName == "FastFail").Name);
        Assert.AreEqual("readonly", form.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("validateOn", form.Props.Single(static prop => prop.PublicName == "ValidateOn").Name);
        Assert.AreEqual("modelValue", form.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("class", form.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", form.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("update:modelValue", form.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("submit", form.Emits.Single(static emit => emit.RazorAlias == "Submit").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VFormSubmitEvent", form.Emits.Single(static emit => emit.RazorAlias == "Submit").PayloadTypeName);
        Assert.AreEqual("additionalAttributes", form.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(form.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyValidateOn?", form.Props.Single(static prop => prop.PublicName == "ValidateOn").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", form.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", form.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        var formDefaultContent = form.Slots.Single(static slot => slot.PublicName == "ChildContent");
        Assert.AreEqual("default", formDefaultContent.Name);
        Assert.AreEqual("ECMAScript.Vuetify.VFormDefaultSlotContext", formDefaultContent.Parameters[0].TypeName);
        Assert.IsTrue(formDefaultContent.IsDefault);

        var breadcrumbs = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBreadcrumbs");
        Assert.AreEqual("items", breadcrumbs.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBreadcrumbItems?", breadcrumbs.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.IsTrue(breadcrumbs.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var dataTable = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDataTable");
        Assert.AreEqual("modelValue", dataTable.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", dataTable.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("headers", dataTable.Props.Single(static prop => prop.PublicName == "Headers").Name);
        Assert.AreEqual("items", dataTable.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemValue", dataTable.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("itemSelectable", dataTable.Props.Single(static prop => prop.PublicName == "ItemSelectable").Name);
        Assert.AreEqual("returnObject", dataTable.Props.Single(static prop => prop.PublicName == "ReturnObject").Name);
        Assert.AreEqual("page", dataTable.Props.Single(static prop => prop.PublicName == "Page").Name);
        Assert.AreEqual("update:page", dataTable.Emits.Single(static emit => emit.RazorAlias == "PageChanged").Name);
        Assert.AreEqual("itemsPerPage", dataTable.Props.Single(static prop => prop.PublicName == "ItemsPerPage").Name);
        Assert.AreEqual("update:itemsPerPage", dataTable.Emits.Single(static emit => emit.RazorAlias == "ItemsPerPageChanged").Name);
        Assert.AreEqual("itemsPerPageOptions", dataTable.Props.Single(static prop => prop.PublicName == "ItemsPerPageOptions").Name);
        Assert.AreEqual("sortBy", dataTable.Props.Single(static prop => prop.PublicName == "SortBy").Name);
        Assert.AreEqual("update:sortBy", dataTable.Emits.Single(static emit => emit.RazorAlias == "SortByChanged").Name);
        Assert.AreEqual("groupBy", dataTable.Props.Single(static prop => prop.PublicName == "GroupBy").Name);
        Assert.AreEqual("expanded", dataTable.Props.Single(static prop => prop.PublicName == "Expanded").Name);
        Assert.AreEqual("update:expanded", dataTable.Emits.Single(static emit => emit.RazorAlias == "ExpandedChanged").Name);
        Assert.AreEqual("update:options", dataTable.Emits.Single(static emit => emit.RazorAlias == "OptionsChanged").Name);
        Assert.AreEqual("update:currentItems", dataTable.Emits.Single(static emit => emit.RazorAlias == "CurrentItemsChanged").Name);
        Assert.AreEqual("search", dataTable.Props.Single(static prop => prop.PublicName == "Search").Name);
        Assert.AreEqual("showSelect", dataTable.Props.Single(static prop => prop.PublicName == "ShowSelect").Name);
        Assert.AreEqual("selectStrategy", dataTable.Props.Single(static prop => prop.PublicName == "SelectStrategy").Name);
        Assert.AreEqual("showExpand", dataTable.Props.Single(static prop => prop.PublicName == "ShowExpand").Name);
        Assert.AreEqual("expandOnClick", dataTable.Props.Single(static prop => prop.PublicName == "ExpandOnClick").Name);
        Assert.AreEqual("hideDefaultBody", dataTable.Props.Single(static prop => prop.PublicName == "HideDefaultBody").Name);
        Assert.AreEqual("hideDefaultFooter", dataTable.Props.Single(static prop => prop.PublicName == "HideDefaultFooter").Name);
        Assert.AreEqual("hideDefaultHeader", dataTable.Props.Single(static prop => prop.PublicName == "HideDefaultHeader").Name);
        Assert.AreEqual("hideNoData", dataTable.Props.Single(static prop => prop.PublicName == "HideNoData").Name);
        Assert.AreEqual("noDataText", dataTable.Props.Single(static prop => prop.PublicName == "NoDataText").Name);
        Assert.AreEqual("loading", dataTable.Props.Single(static prop => prop.PublicName == "Loading").Name);
        Assert.AreEqual("loadingText", dataTable.Props.Single(static prop => prop.PublicName == "LoadingText").Name);
        Assert.AreEqual("disableSort", dataTable.Props.Single(static prop => prop.PublicName == "DisableSort").Name);
        Assert.AreEqual("multiSort", dataTable.Props.Single(static prop => prop.PublicName == "MultiSort").Name);
        Assert.AreEqual("mustSort", dataTable.Props.Single(static prop => prop.PublicName == "MustSort").Name);
        Assert.AreEqual("fixedHeader", dataTable.Props.Single(static prop => prop.PublicName == "FixedHeader").Name);
        Assert.AreEqual("fixedFooter", dataTable.Props.Single(static prop => prop.PublicName == "FixedFooter").Name);
        Assert.AreEqual("headerProps", dataTable.Props.Single(static prop => prop.PublicName == "HeaderProps").Name);
        Assert.AreEqual("rowProps", dataTable.Props.Single(static prop => prop.PublicName == "RowProps").Name);
        Assert.AreEqual("cellProps", dataTable.Props.Single(static prop => prop.PublicName == "CellProps").Name);
        Assert.AreEqual("itemsPerPageText", dataTable.Props.Single(static prop => prop.PublicName == "ItemsPerPageText").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableHeaders?", dataTable.Props.Single(static prop => prop.PublicName == "Headers").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableItems?", dataTable.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableSelectedValues?", dataTable.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItemKey?", dataTable.Props.Single(static prop => prop.PublicName == "ItemValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableItemsPerPageOptions?", dataTable.Props.Single(static prop => prop.PublicName == "ItemsPerPageOptions").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableSortItems?", dataTable.Props.Single(static prop => prop.PublicName == "SortBy").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableSelectStrategy?", dataTable.Props.Single(static prop => prop.PublicName == "SelectStrategy").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", dataTable.Props.Single(static prop => prop.PublicName == "Loading").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableRowProps?", dataTable.Props.Single(static prop => prop.PublicName == "RowProps").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableCellProps?", dataTable.Props.Single(static prop => prop.PublicName == "CellProps").TypeName);
        Assert.IsTrue(dataTable.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableSlotContext", dataTable.Slots.Single(static slot => slot.Name == "top").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableHeadersSlotContext", dataTable.Slots.Single(static slot => slot.Name == "headers").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableHeaderCellSlotContext", dataTable.Slots.Single(static slot => slot.Name == "header.data-table-select").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableSlotContext", dataTable.Slots.Single(static slot => slot.Name == "body.prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableItemSlotContext", dataTable.Slots.Single(static slot => slot.Name == "item").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataTableGroupHeaderSlotContext", dataTable.Slots.Single(static slot => slot.Name == "group-header").Parameters[0].TypeName);
        Assert.IsTrue(dataTable.Slots.Any(static slot => slot.Name == "footer.prepend"));
        Assert.AreEqual("loading", dataTable.Slots.Single(static slot => slot.PublicName == "LoadingContent").Name);
        Assert.IsTrue(dataTable.Slots.Any(static slot => slot.Name == "no-data"));

        var dataIterator = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VDataIterator");
        Assert.AreEqual("modelValue", dataIterator.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", dataIterator.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("items", dataIterator.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemValue", dataIterator.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("itemSelectable", dataIterator.Props.Single(static prop => prop.PublicName == "ItemSelectable").Name);
        Assert.AreEqual("returnObject", dataIterator.Props.Single(static prop => prop.PublicName == "ReturnObject").Name);
        Assert.AreEqual("page", dataIterator.Props.Single(static prop => prop.PublicName == "Page").Name);
        Assert.AreEqual("update:page", dataIterator.Emits.Single(static emit => emit.RazorAlias == "PageChanged").Name);
        Assert.AreEqual("itemsPerPage", dataIterator.Props.Single(static prop => prop.PublicName == "ItemsPerPage").Name);
        Assert.AreEqual("update:itemsPerPage", dataIterator.Emits.Single(static emit => emit.RazorAlias == "ItemsPerPageChanged").Name);
        Assert.AreEqual("sortBy", dataIterator.Props.Single(static prop => prop.PublicName == "SortBy").Name);
        Assert.AreEqual("update:sortBy", dataIterator.Emits.Single(static emit => emit.RazorAlias == "SortByChanged").Name);
        Assert.AreEqual("groupBy", dataIterator.Props.Single(static prop => prop.PublicName == "GroupBy").Name);
        Assert.AreEqual("update:groupBy", dataIterator.Emits.Single(static emit => emit.RazorAlias == "GroupByChanged").Name);
        Assert.AreEqual("expanded", dataIterator.Props.Single(static prop => prop.PublicName == "Expanded").Name);
        Assert.AreEqual("update:expanded", dataIterator.Emits.Single(static emit => emit.RazorAlias == "ExpandedChanged").Name);
        Assert.AreEqual("update:options", dataIterator.Emits.Single(static emit => emit.RazorAlias == "OptionsChanged").Name);
        Assert.AreEqual("update:currentItems", dataIterator.Emits.Single(static emit => emit.RazorAlias == "CurrentItemsChanged").Name);
        Assert.AreEqual("customKeySort", dataIterator.Props.Single(static prop => prop.PublicName == "CustomKeySort").Name);
        Assert.AreEqual("customFilter", dataIterator.Props.Single(static prop => prop.PublicName == "CustomFilter").Name);
        Assert.AreEqual("customKeyFilter", dataIterator.Props.Single(static prop => prop.PublicName == "CustomKeyFilter").Name);
        Assert.AreEqual("filterKeys", dataIterator.Props.Single(static prop => prop.PublicName == "FilterKeys").Name);
        Assert.AreEqual("filterMode", dataIterator.Props.Single(static prop => prop.PublicName == "FilterMode").Name);
        Assert.AreEqual("noFilter", dataIterator.Props.Single(static prop => prop.PublicName == "NoFilter").Name);
        Assert.AreEqual("transition", dataIterator.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataIteratorSelectedValues?", dataIterator.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataIteratorItems?", dataIterator.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", dataIterator.Props.Single(static prop => prop.PublicName == "Page").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataTableSortItems?", dataIterator.Props.Single(static prop => prop.PublicName == "SortBy").TypeName);
        Assert.AreEqual("string[]?", dataIterator.Props.Single(static prop => prop.PublicName == "Expanded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDataIteratorSortFunctions?", dataIterator.Props.Single(static prop => prop.PublicName == "CustomKeySort").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", dataIterator.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataIteratorSlotContext", dataIterator.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataIteratorSlotContext", dataIterator.Slots.Single(static slot => slot.Name == "header").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VDataIteratorSlotContext", dataIterator.Slots.Single(static slot => slot.Name == "footer").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLoaderSlotContext", dataIterator.Slots.Single(static slot => slot.Name == "loader").Parameters[0].TypeName);
        Assert.IsTrue(dataIterator.Slots.Single(static slot => slot.Name == "no-data").Parameters.IsDefaultOrEmpty);

        var sheet = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSheet");
        Assert.AreEqual("theme", sheet.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", sheet.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", sheet.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("tile", sheet.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("position", sheet.Props.Single(static prop => prop.PublicName == "Position").Name);
        Assert.AreEqual("location", sheet.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("elevation", sheet.Props.Single(static prop => prop.PublicName == "Elevation").Name);
        Assert.AreEqual("height", sheet.Props.Single(static prop => prop.PublicName == "Height").Name);
        Assert.AreEqual("width", sheet.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("minHeight", sheet.Props.Single(static prop => prop.PublicName == "MinHeight").Name);
        Assert.AreEqual("maxWidth", sheet.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("class", sheet.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", sheet.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("border", sheet.Props.Single(static prop => prop.PublicName == "Border").Name);
        Assert.AreEqual("color", sheet.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("string?", sheet.Props.Single(static prop => prop.PublicName == "Theme").TypeName);
        Assert.AreEqual("string?", sheet.Props.Single(static prop => prop.PublicName == "Tag").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", sheet.Props.Single(static prop => prop.PublicName == "Elevation").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", sheet.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyPosition?", sheet.Props.Single(static prop => prop.PublicName == "Position").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", sheet.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", sheet.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", sheet.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", sheet.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.IsTrue(sheet.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var icon = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VIcon");
        Assert.AreEqual("theme", icon.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", icon.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("size", icon.Props.Single(static prop => prop.PublicName == "Size").Name);
        Assert.AreEqual("class", icon.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", icon.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("color", icon.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("disabled", icon.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("start", icon.Props.Single(static prop => prop.PublicName == "Start").Name);
        Assert.AreEqual("end", icon.Props.Single(static prop => prop.PublicName == "End").Name);
        Assert.AreEqual("icon", icon.Props.Single(static prop => prop.PublicName == "Icon").Name);
        Assert.AreEqual("opacity", icon.Props.Single(static prop => prop.PublicName == "Opacity").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", icon.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", icon.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", icon.Props.Single(static prop => prop.PublicName == "Opacity").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", icon.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", icon.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(icon.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var pagination = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VPagination");
        Assert.AreEqual("modelValue", pagination.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", pagination.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", pagination.Props.Single(static prop => prop.PublicName == "Length").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", pagination.Props.Single(static prop => prop.PublicName == "TotalVisible").TypeName);

        var image = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VImg");
        Assert.AreEqual("src", image.Props.Single(static prop => prop.PublicName == "Src").Name);
        Assert.AreEqual("alt", image.Props.Single(static prop => prop.PublicName == "Alt").Name);
        Assert.AreEqual("lazySrc", image.Props.Single(static prop => prop.PublicName == "LazySrc").Name);
        Assert.AreEqual("srcset", image.Props.Single(static prop => prop.PublicName == "Srcset").Name);
        Assert.AreEqual("aspectRatio", image.Props.Single(static prop => prop.PublicName == "AspectRatio").Name);
        Assert.AreEqual("rounded", image.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("crossorigin", image.Props.Single(static prop => prop.PublicName == "CrossOrigin").Name);
        Assert.AreEqual("referrerpolicy", image.Props.Single(static prop => prop.PublicName == "ReferrerPolicy").Name);
        Assert.AreEqual("loadstart", image.Emits.Single(static emit => emit.RazorAlias == "LoadStart").Name);
        Assert.AreEqual("load", image.Emits.Single(static emit => emit.RazorAlias == "Load").Name);
        Assert.AreEqual("error", image.Emits.Single(static emit => emit.RazorAlias == "LoadError").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VImgSource?", image.Props.Single(static prop => prop.PublicName == "Src").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", image.Props.Single(static prop => prop.PublicName == "Height").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", image.Props.Single(static prop => prop.PublicName == "Width").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", image.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", image.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", image.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", image.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", image.Props.Single(static prop => prop.PublicName == "ContentClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIntersectionObserverOptions?", image.Props.Single(static prop => prop.PublicName == "Options").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VImgDraggableValue?", image.Props.Single(static prop => prop.PublicName == "Draggable").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VImgCrossOrigin?", image.Props.Single(static prop => prop.PublicName == "CrossOrigin").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VImgReferrerPolicy?", image.Props.Single(static prop => prop.PublicName == "ReferrerPolicy").TypeName);
        Assert.IsTrue(image.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("placeholder", image.Slots.Single(static slot => slot.PublicName == "Placeholder").Name);
        Assert.AreEqual("error", image.Slots.Single(static slot => slot.PublicName == "ErrorContent").Name);
        Assert.AreEqual("sources", image.Slots.Single(static slot => slot.PublicName == "Sources").Name);

        var tooltip = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTooltip");
        Assert.AreEqual("modelValue", tooltip.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", tooltip.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("id", tooltip.Props.Single(static prop => prop.PublicName == "Id").Name);
        Assert.AreEqual("interactive", tooltip.Props.Single(static prop => prop.PublicName == "Interactive").Name);
        Assert.AreEqual("text", tooltip.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("location", tooltip.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("origin", tooltip.Props.Single(static prop => prop.PublicName == "Origin").Name);
        Assert.AreEqual("offset", tooltip.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("openOnClick", tooltip.Props.Single(static prop => prop.PublicName == "OpenOnClick").Name);
        Assert.AreEqual("openOnHover", tooltip.Props.Single(static prop => prop.PublicName == "OpenOnHover").Name);
        Assert.AreEqual("openOnFocus", tooltip.Props.Single(static prop => prop.PublicName == "OpenOnFocus").Name);
        Assert.AreEqual("openDelay", tooltip.Props.Single(static prop => prop.PublicName == "OpenDelay").Name);
        Assert.AreEqual("closeDelay", tooltip.Props.Single(static prop => prop.PublicName == "CloseDelay").Name);
        Assert.AreEqual("minWidth", tooltip.Props.Single(static prop => prop.PublicName == "MinWidth").Name);
        Assert.AreEqual("maxWidth", tooltip.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("activatorProps", tooltip.Props.Single(static prop => prop.PublicName == "ActivatorProps").Name);
        Assert.AreEqual("contentProps", tooltip.Props.Single(static prop => prop.PublicName == "ContentProps").Name);
        Assert.AreEqual("additionalAttributes", tooltip.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(tooltip.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", tooltip.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", tooltip.Props.Single(static prop => prop.PublicName == "ActivatorProps").TypeName);
        var tooltipActivator = tooltip.Slots.Single(static slot => slot.Name == "activator");
        Assert.AreEqual("ECMAScript.Vuetify.VOverlayActivatorContext", tooltipActivator.Parameters[0].TypeName);
        Assert.IsTrue(form.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var menu = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VMenu");
        Assert.AreEqual("modelValue", menu.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", menu.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("closeOnContentClick", menu.Props.Single(static prop => prop.PublicName == "CloseOnContentClick").Name);
        Assert.AreEqual("closeOnBack", menu.Props.Single(static prop => prop.PublicName == "CloseOnBack").Name);
        Assert.AreEqual("openOnClick", menu.Props.Single(static prop => prop.PublicName == "OpenOnClick").Name);
        Assert.AreEqual("openOnHover", menu.Props.Single(static prop => prop.PublicName == "OpenOnHover").Name);
        Assert.AreEqual("openOnFocus", menu.Props.Single(static prop => prop.PublicName == "OpenOnFocus").Name);
        Assert.AreEqual("location", menu.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("offset", menu.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("scrollStrategy", menu.Props.Single(static prop => prop.PublicName == "ScrollStrategy").Name);
        Assert.AreEqual("activatorProps", menu.Props.Single(static prop => prop.PublicName == "ActivatorProps").Name);
        Assert.AreEqual("contentProps", menu.Props.Single(static prop => prop.PublicName == "ContentProps").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", menu.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueProps?", menu.Props.Single(static prop => prop.PublicName == "ContentProps").TypeName);
        var menuActivator = menu.Slots.Single(static slot => slot.Name == "activator");
        Assert.AreEqual("ECMAScript.Vuetify.VOverlayActivatorContext", menuActivator.Parameters[0].TypeName);
        Assert.IsTrue(menu.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var avatar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VAvatar");
        Assert.AreEqual("image", avatar.Props.Single(static prop => prop.PublicName == "Image").Name);
        Assert.AreEqual("icon", avatar.Props.Single(static prop => prop.PublicName == "Icon").Name);
        Assert.AreEqual("text", avatar.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("theme", avatar.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", avatar.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("tile", avatar.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("class", avatar.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", avatar.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("border", avatar.Props.Single(static prop => prop.PublicName == "Border").Name);
        Assert.AreEqual("start", avatar.Props.Single(static prop => prop.PublicName == "Start").Name);
        Assert.AreEqual("end", avatar.Props.Single(static prop => prop.PublicName == "End").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", avatar.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", avatar.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", avatar.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", avatar.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", avatar.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", avatar.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.IsTrue(avatar.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var badge = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VBadge");
        Assert.AreEqual("transition", badge.Props.Single(static prop => prop.PublicName == "Transition").Name);
        Assert.AreEqual("theme", badge.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", badge.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", badge.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("tile", badge.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("location", badge.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("class", badge.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", badge.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("bordered", badge.Props.Single(static prop => prop.PublicName == "Bordered").Name);
        Assert.AreEqual("content", badge.Props.Single(static prop => prop.PublicName == "Content").Name);
        Assert.AreEqual("icon", badge.Props.Single(static prop => prop.PublicName == "Icon").Name);
        Assert.AreEqual("floating", badge.Props.Single(static prop => prop.PublicName == "Floating").Name);
        Assert.AreEqual("inline", badge.Props.Single(static prop => prop.PublicName == "Inline").Name);
        Assert.AreEqual("label", badge.Props.Single(static prop => prop.PublicName == "Label").Name);
        Assert.AreEqual("max", badge.Props.Single(static prop => prop.PublicName == "Max").Name);
        Assert.AreEqual("offsetX", badge.Props.Single(static prop => prop.PublicName == "OffsetX").Name);
        Assert.AreEqual("textColor", badge.Props.Single(static prop => prop.PublicName == "TextColor").Name);
        Assert.AreEqual("update:modelValue", badge.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", badge.Props.Single(static prop => prop.PublicName == "Content").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTransitionValue?", badge.Props.Single(static prop => prop.PublicName == "Transition").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", badge.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", badge.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyIconValue?", badge.Props.Single(static prop => prop.PublicName == "Icon").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", badge.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", badge.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("badge", badge.Slots.Single(static slot => slot.PublicName == "BadgeContent").Name);
        Assert.IsTrue(badge.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var progressCircular = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VProgressCircular");
        Assert.AreEqual("theme", progressCircular.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", progressCircular.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("size", progressCircular.Props.Single(static prop => prop.PublicName == "Size").Name);
        Assert.AreEqual("class", progressCircular.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", progressCircular.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("modelValue", progressCircular.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("bgColor", progressCircular.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("rotate", progressCircular.Props.Single(static prop => prop.PublicName == "Rotate").Name);
        Assert.AreEqual("width", progressCircular.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", progressCircular.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyProgressCircularIndeterminateValue?", progressCircular.Props.Single(static prop => prop.PublicName == "Indeterminate").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", progressCircular.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", progressCircular.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(progressCircular.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("ECMAScript.Vuetify.VProgressCircularDefaultSlotContext", progressCircular.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var progressLinear = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VProgressLinear");
        Assert.AreEqual("theme", progressLinear.Props.Single(static prop => prop.PublicName == "Theme").Name);
        Assert.AreEqual("tag", progressLinear.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("rounded", progressLinear.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("tile", progressLinear.Props.Single(static prop => prop.PublicName == "Tile").Name);
        Assert.AreEqual("location", progressLinear.Props.Single(static prop => prop.PublicName == "Location").Name);
        Assert.AreEqual("class", progressLinear.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", progressLinear.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("absolute", progressLinear.Props.Single(static prop => prop.PublicName == "Absolute").Name);
        Assert.AreEqual("active", progressLinear.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("modelValue", progressLinear.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("bgColor", progressLinear.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("bgOpacity", progressLinear.Props.Single(static prop => prop.PublicName == "BgOpacity").Name);
        Assert.AreEqual("bufferValue", progressLinear.Props.Single(static prop => prop.PublicName == "BufferValue").Name);
        Assert.AreEqual("bufferColor", progressLinear.Props.Single(static prop => prop.PublicName == "BufferColor").Name);
        Assert.AreEqual("bufferOpacity", progressLinear.Props.Single(static prop => prop.PublicName == "BufferOpacity").Name);
        Assert.AreEqual("clickable", progressLinear.Props.Single(static prop => prop.PublicName == "Clickable").Name);
        Assert.AreEqual("opacity", progressLinear.Props.Single(static prop => prop.PublicName == "Opacity").Name);
        Assert.AreEqual("reverse", progressLinear.Props.Single(static prop => prop.PublicName == "Reverse").Name);
        Assert.AreEqual("roundedBar", progressLinear.Props.Single(static prop => prop.PublicName == "RoundedBar").Name);
        Assert.AreEqual("update:modelValue", progressLinear.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("ECMAScript.Number", progressLinear.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", progressLinear.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", progressLinear.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyLocation?", progressLinear.Props.Single(static prop => prop.PublicName == "Location").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", progressLinear.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", progressLinear.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.IsTrue(progressLinear.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("ECMAScript.Vuetify.VProgressLinearDefaultSlotContext", progressLinear.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var otpInputDescriptor = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VOtpInput");
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", otpInputDescriptor.Props.Single(static prop => prop.PublicName == "Length").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", otpInputDescriptor.Props.Single(static prop => prop.PublicName == "Loading").TypeName);

        var chipDescriptor = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VChip");
        Assert.AreEqual("prependIcon", chipDescriptor.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("appendIcon", chipDescriptor.Props.Single(static prop => prop.PublicName == "AppendIcon").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Text").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Size").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBorderValue?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Border").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRippleValue?", chipDescriptor.Props.Single(static prop => prop.PublicName == "Ripple").TypeName);

        var radioGroup = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VRadioGroup");
        Assert.AreEqual("modelValue", radioGroup.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", radioGroup.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("color", radioGroup.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", radioGroup.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("readonly", radioGroup.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("hideDetails", radioGroup.Props.Single(static prop => prop.PublicName == "HideDetails").Name);
        Assert.AreEqual("messages", radioGroup.Props.Single(static prop => prop.PublicName == "Messages").Name);
        Assert.AreEqual("additionalAttributes", radioGroup.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(radioGroup.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyDensity?", radioGroup.Props.Single(static prop => prop.PublicName == "Density").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyHideDetailsValue?", radioGroup.Props.Single(static prop => prop.PublicName == "HideDetails").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMessagesValue?", radioGroup.Props.Single(static prop => prop.PublicName == "Messages").TypeName);
        Assert.IsTrue(radioGroup.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var snackbar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSnackbar");
        Assert.AreEqual("modelValue", snackbar.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", snackbar.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("text", snackbar.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("timer", snackbar.Props.Single(static prop => prop.PublicName == "Timer").Name);
        Assert.AreEqual("zIndex", snackbar.Props.Single(static prop => prop.PublicName == "ZIndex").Name);
        Assert.AreEqual("activator", snackbar.Props.Single(static prop => prop.PublicName == "ActivatorTarget").Name);
        Assert.AreEqual("click:outside", snackbar.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").Name);
        Assert.AreEqual("keydown", snackbar.Emits.Single(static emit => emit.RazorAlias == "Keydown").Name);
        Assert.AreEqual("afterEnter", snackbar.Emits.Single(static emit => emit.RazorAlias == "AfterEnter").Name);
        Assert.AreEqual("afterLeave", snackbar.Emits.Single(static emit => emit.RazorAlias == "AfterLeave").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", snackbar.Emits.Single(static emit => emit.RazorAlias == "ClickOutside").PayloadTypeName);
        Assert.AreEqual("ECMAScript.KeyboardEvent", snackbar.Emits.Single(static emit => emit.RazorAlias == "Keydown").PayloadTypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyVariant?", snackbar.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOverlayOffsetValue?", snackbar.Props.Single(static prop => prop.PublicName == "Offset").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOverlayTarget?", snackbar.Props.Single(static prop => prop.PublicName == "Target").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyOverlayActivatorTarget?", snackbar.Props.Single(static prop => prop.PublicName == "ActivatorTarget").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", snackbar.Props.Single(static prop => prop.PublicName == "Timer").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", snackbar.Props.Single(static prop => prop.PublicName == "Timeout").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRoundedValue?", snackbar.Props.Single(static prop => prop.PublicName == "Rounded").TypeName);
        Assert.IsTrue(snackbar.Slots.Single(static slot => slot.IsDefault).IsDefault);
        Assert.AreEqual("activator", snackbar.Slots.Single(static slot => slot.PublicName == "Activator").Name);
        Assert.AreEqual("text", snackbar.Slots.Single(static slot => slot.PublicName == "TextContent").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VSnackbarActionsSlotContext", snackbar.Slots.Single(static slot => slot.PublicName == "Actions").Parameters[0].TypeName);

        var snackbarQueue = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSnackbarQueue");
        Assert.AreEqual("modelValue", snackbarQueue.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", snackbarQueue.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("variant", snackbarQueue.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("closable", snackbarQueue.Props.Single(static prop => prop.PublicName == "Closable").Name);
        Assert.AreEqual("closeText", snackbarQueue.Props.Single(static prop => prop.PublicName == "CloseText").Name);
        Assert.AreEqual("offset", snackbarQueue.Props.Single(static prop => prop.PublicName == "Offset").Name);
        Assert.AreEqual("origin", snackbarQueue.Props.Single(static prop => prop.PublicName == "Origin").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySnackbarQueueMessages?", snackbarQueue.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyBooleanStringValue?", snackbarQueue.Props.Single(static prop => prop.PublicName == "Closable").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSnackbarQueueSlotContext", snackbarQueue.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSnackbarQueueSlotContext", snackbarQueue.Slots.Single(static slot => slot.Name == "text").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VSnackbarQueueActionsSlotContext", snackbarQueue.Slots.Single(static slot => slot.Name == "actions").Parameters[0].TypeName);

        var sparkline = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSparkline");
        Assert.AreEqual("autoDraw", sparkline.Props.Single(static prop => prop.PublicName == "AutoDraw").Name);
        Assert.AreEqual("autoDrawDuration", sparkline.Props.Single(static prop => prop.PublicName == "AutoDrawDuration").Name);
        Assert.AreEqual("gradientDirection", sparkline.Props.Single(static prop => prop.PublicName == "GradientDirection").Name);
        Assert.AreEqual("labelSize", sparkline.Props.Single(static prop => prop.PublicName == "LabelSize").Name);
        Assert.AreEqual("itemValue", sparkline.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("modelValue", sparkline.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("showLabels", sparkline.Props.Single(static prop => prop.PublicName == "ShowLabels").Name);
        Assert.AreEqual("autoLineWidth", sparkline.Props.Single(static prop => prop.PublicName == "AutoLineWidth").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySparklineItems?", sparkline.Props.Single(static prop => prop.PublicName == "ModelValue").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySparklineItems?", sparkline.Props.Single(static prop => prop.PublicName == "Labels").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySparklineSmoothValue?", sparkline.Props.Single(static prop => prop.PublicName == "Smooth").TypeName);
        Assert.IsTrue(sparkline.Slots.Single(static slot => slot.IsDefault).Parameters.IsDefaultOrEmpty);
        Assert.AreEqual("ECMAScript.Vuetify.VSparklineLabelSlotContext", sparkline.Slots.Single(static slot => slot.Name == "label").Parameters[0].TypeName);

        var validation = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VValidation");
        Assert.AreEqual("focused", validation.Props.Single(static prop => prop.PublicName == "Focused").Name);
        Assert.AreEqual("update:focused", validation.Emits.Single(static emit => emit.RazorAlias == "FocusedChanged").Name);
        Assert.AreEqual("disabled", validation.Props.Single(static prop => prop.PublicName == "Disabled").Name);
        Assert.AreEqual("readonly", validation.Props.Single(static prop => prop.PublicName == "Readonly").Name);
        Assert.AreEqual("errorMessages", validation.Props.Single(static prop => prop.PublicName == "ErrorMessages").Name);
        Assert.AreEqual("maxErrors", validation.Props.Single(static prop => prop.PublicName == "MaxErrors").Name);
        Assert.AreEqual("rules", validation.Props.Single(static prop => prop.PublicName == "Rules").Name);
        Assert.AreEqual("modelValue", validation.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", validation.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.AreEqual("validateOn", validation.Props.Single(static prop => prop.PublicName == "ValidateOn").Name);
        Assert.AreEqual("validationValue", validation.Props.Single(static prop => prop.PublicName == "ValidationValue").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyNullableBoolean?", validation.Props.Single(static prop => prop.PublicName == "Disabled").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyMessagesValue?", validation.Props.Single(static prop => prop.PublicName == "ErrorMessages").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", validation.Props.Single(static prop => prop.PublicName == "MaxErrors").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyValidationRule[]?", validation.Props.Single(static prop => prop.PublicName == "Rules").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VValidationSlotContext", validation.Slots.Single(static slot => slot.IsDefault).Parameters[0].TypeName);

        var tabs = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTabs");
        Assert.AreEqual("modelValue", tabs.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", tabs.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);
        Assert.IsTrue(tabs.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var tab = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VTab");
        Assert.AreEqual("value", tab.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("text", tab.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.IsTrue(tab.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var spacer = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VSpacer");
        Assert.AreEqual("class", spacer.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", spacer.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("tag", spacer.Props.Single(static prop => prop.PublicName == "Tag").Name);
        Assert.AreEqual("ECMAScript.Vue3.VueClassValue?", spacer.Props.Single(static prop => prop.PublicName == "CssClass").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyStyleValue?", spacer.Props.Single(static prop => prop.PublicName == "CssStyle").TypeName);
        Assert.AreEqual("additionalAttributes", spacer.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").Name);
        Assert.IsTrue(spacer.Props.Single(static prop => prop.PublicName == "AdditionalAttributes").CaptureUnmatchedValues);
        Assert.AreEqual(0, spacer.Emits.Length);
        Assert.IsTrue(spacer.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var card = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.Vuetify.VCard");
        Assert.AreEqual("title", card.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("subtitle", card.Props.Single(static prop => prop.PublicName == "Subtitle").Name);
        Assert.AreEqual("text", card.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("prependIcon", card.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("appendIcon", card.Props.Single(static prop => prop.PublicName == "AppendIcon").Name);
        Assert.AreEqual("prependAvatar", card.Props.Single(static prop => prop.PublicName == "PrependAvatar").Name);
        Assert.AreEqual("appendAvatar", card.Props.Single(static prop => prop.PublicName == "AppendAvatar").Name);
        Assert.AreEqual("image", card.Props.Single(static prop => prop.PublicName == "Image").Name);
        Assert.AreEqual("variant", card.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("density", card.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.AreEqual("elevation", card.Props.Single(static prop => prop.PublicName == "Elevation").Name);
        Assert.AreEqual("rounded", card.Props.Single(static prop => prop.PublicName == "Rounded").Name);
        Assert.AreEqual("maxWidth", card.Props.Single(static prop => prop.PublicName == "MaxWidth").Name);
        Assert.AreEqual("href", card.Props.Single(static prop => prop.PublicName == "Href").Name);
        Assert.AreEqual("to", card.Props.Single(static prop => prop.PublicName == "To").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", card.Props.Single(static prop => prop.PublicName == "Title").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueStringNumberValue?", card.Props.Single(static prop => prop.PublicName == "MaxWidth").TypeName);
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "text"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "title"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "subtitle"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "image"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "prepend"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "append"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "actions"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "item"));
        Assert.IsTrue(card.Slots.Single(static slot => slot.IsDefault).IsDefault);

        Assert.AreEqual("items", list.Props.Single(static prop => prop.PublicName == "Items").Name);
        Assert.AreEqual("itemTitle", list.Props.Single(static prop => prop.PublicName == "ItemTitle").Name);
        Assert.AreEqual("itemValue", list.Props.Single(static prop => prop.PublicName == "ItemValue").Name);
        Assert.AreEqual("itemChildren", list.Props.Single(static prop => prop.PublicName == "ItemChildren").Name);
        Assert.AreEqual("itemProps", list.Props.Single(static prop => prop.PublicName == "ItemProps").Name);
        Assert.AreEqual("itemType", list.Props.Single(static prop => prop.PublicName == "ItemType").Name);
        Assert.AreEqual("lines", list.Props.Single(static prop => prop.PublicName == "Lines").Name);
        Assert.AreEqual("slim", list.Props.Single(static prop => prop.PublicName == "Slim").Name);
        Assert.AreEqual("bgColor", list.Props.Single(static prop => prop.PublicName == "BgColor").Name);
        Assert.AreEqual("variant", list.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifySelectItems?", list.Props.Single(static prop => prop.PublicName == "Items").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyListLines?", list.Props.Single(static prop => prop.PublicName == "Lines").TypeName);

        Assert.AreEqual("prependIcon", listItem.Props.Single(static prop => prop.PublicName == "PrependIcon").Name);
        Assert.AreEqual("appendIcon", listItem.Props.Single(static prop => prop.PublicName == "AppendIcon").Name);
        Assert.AreEqual("prependAvatar", listItem.Props.Single(static prop => prop.PublicName == "PrependAvatar").Name);
        Assert.AreEqual("appendAvatar", listItem.Props.Single(static prop => prop.PublicName == "AppendAvatar").Name);
        Assert.AreEqual("active", listItem.Props.Single(static prop => prop.PublicName == "Active").Name);
        Assert.AreEqual("activeClass", listItem.Props.Single(static prop => prop.PublicName == "ActiveClass").Name);
        Assert.AreEqual("baseColor", listItem.Props.Single(static prop => prop.PublicName == "BaseColor").Name);
        Assert.AreEqual("lines", listItem.Props.Single(static prop => prop.PublicName == "Lines").Name);
        Assert.AreEqual("ripple", listItem.Props.Single(static prop => prop.PublicName == "Ripple").Name);
        Assert.AreEqual("href", listItem.Props.Single(static prop => prop.PublicName == "Href").Name);
        Assert.AreEqual("to", listItem.Props.Single(static prop => prop.PublicName == "To").Name);
        Assert.AreEqual("click", listItem.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyTextValue?", listItem.Props.Single(static prop => prop.PublicName == "Title").TypeName);
        Assert.AreEqual("ECMAScript.Vue3.VueValue?", listItem.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VuetifyRippleValue?", listItem.Props.Single(static prop => prop.PublicName == "Ripple").TypeName);
        var listItemDefaultSlot = listItem.Slots.Single(static slot => slot.IsDefault);
        Assert.IsTrue(listItemDefaultSlot.Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(listItem.Slots.Any(static slot => slot.Name == "prepend"));
        Assert.IsTrue(listItem.Slots.Any(static slot => slot.Name == "append"));
        Assert.IsTrue(listItem.Slots.Any(static slot => slot.Name == "title"));
        Assert.IsTrue(listItem.Slots.Any(static slot => slot.Name == "subtitle"));
        Assert.AreEqual("ECMAScript.Vuetify.VListItemSlotContext", listItem.Slots.Single(static slot => slot.Name == "prepend").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VListItemSlotContext", listItem.Slots.Single(static slot => slot.Name == "append").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VListItemTitleSlotContext", listItem.Slots.Single(static slot => slot.Name == "title").Parameters[0].TypeName);
        Assert.AreEqual("ECMAScript.Vuetify.VListItemSubtitleSlotContext", listItem.Slots.Single(static slot => slot.Name == "subtitle").Parameters[0].TypeName);
    }

    [TestMethod]
    public void RazorVue_Context_DiscoversTDesignPackageLibraryDescriptors_FromReferencedAssembly()
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

        var tdesignDescriptors = descriptors
            .Where(static descriptor => descriptor.ResolutionNamespace == "ECMAScript.TDesign")
            .OrderBy(static descriptor => descriptor.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (var descriptor in tdesignDescriptors)
        {
            var additionalAttributes = descriptor.Props.SingleOrDefault(static prop => prop.PublicName == "AdditionalAttributes");
            Assert.IsNotNull(additionalAttributes, descriptor.FullName);
            Assert.AreEqual("additionalAttributes", additionalAttributes!.Name, descriptor.FullName);
            Assert.IsTrue(additionalAttributes.CaptureUnmatchedValues, descriptor.FullName);
            Assert.AreEqual("System.Collections.Generic.IReadOnlyDictionary<string, object?>?", additionalAttributes.TypeName, descriptor.FullName);
            CollectionAssert.AreEqual(new[] { "tdesign-vue-next/es/style/index.css" }, descriptor.StyleDependencies.ToArray(), descriptor.FullName);
            CollectionAssert.AreEqual(new[] { "tdesign" }, descriptor.PluginRequirements.ToArray(), descriptor.FullName);
        }

        CollectionAssert.AreEquivalent(
            TDesignTestMetadata.RuntimeComponentExportNames,
            tdesignDescriptors.Select(static descriptor => descriptor.Name).ToArray());
        Assert.AreEqual(TDesignTestMetadata.RuntimeComponentExportNames.Length, tdesignDescriptors.Length);
        Assert.AreEqual(TDesignTestMetadata.StrongAuthoringComponentNames.Length, tdesignDescriptors.Length);

        var button = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TButton");
        Assert.AreEqual("tdesign-vue-next", button.ImportSpecifier);
        Assert.AreEqual("Button", button.ExportName);
        Assert.AreEqual("content", button.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("class", button.Props.Single(static prop => prop.PublicName == "CssClass").Name);
        Assert.AreEqual("style", button.Props.Single(static prop => prop.PublicName == "CssStyle").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignButtonShape?", button.Props.Single(static prop => prop.PublicName == "Shape").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignButtonTheme?", button.Props.Single(static prop => prop.PublicName == "Theme").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignButtonVariant?", button.Props.Single(static prop => prop.PublicName == "Variant").TypeName);
        Assert.AreEqual("click", button.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.IsTrue(button.Slots.Single(static slot => slot.Name == "icon").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(button.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var menu = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TMenu");
        Assert.AreEqual("Menu", menu.ExportName);
        Assert.AreEqual("width", menu.Props.Single(static prop => prop.PublicName == "Width").Name);
        Assert.AreEqual("value", menu.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("defaultValue", menu.Props.Single(static prop => prop.PublicName == "DefaultValue").Name);
        Assert.AreEqual("defaultExpanded", menu.Props.Single(static prop => prop.PublicName == "DefaultExpanded").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignMenuWidthValue?", menu.Props.Single(static prop => prop.PublicName == "Width").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignMenuValue?", menu.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("change", menu.Emits.Single(static emit => emit.RazorAlias == "OnChange").Name);
        Assert.AreEqual("expand", menu.Emits.Single(static emit => emit.RazorAlias == "OnExpand").Name);
        Assert.IsTrue(menu.Slots.Single(static slot => slot.Name == "logo").Parameters.IsDefaultOrEmpty);
        Assert.IsTrue(menu.Slots.Single(static slot => slot.Name == "operations").Parameters.IsDefaultOrEmpty);

        var menuItem = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TMenuItem");
        Assert.AreEqual("MenuItem", menuItem.ExportName);
        Assert.AreEqual("content", menuItem.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("to", menuItem.Props.Single(static prop => prop.PublicName == "To").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignMenuRouteTarget?", menuItem.Props.Single(static prop => prop.PublicName == "To").TypeName);
        Assert.AreEqual("click", menuItem.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignMenuItemClickContext", menuItem.Emits.Single(static emit => emit.RazorAlias == "OnClick").PayloadTypeName);
        Assert.IsTrue(menuItem.Slots.Single(static slot => slot.Name == "icon").Parameters.IsDefaultOrEmpty);

        var card = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TCard");
        Assert.AreEqual("Card", card.ExportName);
        Assert.AreEqual("bodyClassName", card.Props.Single(static prop => prop.PublicName == "BodyCssClass").Name);
        Assert.AreEqual("bodyStyle", card.Props.Single(static prop => prop.PublicName == "BodyCssStyle").Name);
        Assert.AreEqual("headerClassName", card.Props.Single(static prop => prop.PublicName == "HeaderCssClass").Name);
        Assert.AreEqual("headerStyle", card.Props.Single(static prop => prop.PublicName == "HeaderCssStyle").Name);
        Assert.AreEqual("footerClassName", card.Props.Single(static prop => prop.PublicName == "FooterCssClass").Name);
        Assert.AreEqual("footerStyle", card.Props.Single(static prop => prop.PublicName == "FooterCssStyle").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignStyles?", card.Props.Single(static prop => prop.PublicName == "BodyCssStyle").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignCardTheme?", card.Props.Single(static prop => prop.PublicName == "Theme").TypeName);
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "header"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "footer"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "actions"));
        Assert.IsTrue(card.Slots.Any(static slot => slot.Name == "avatar"));

        var breadcrumb = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TBreadcrumb");
        Assert.AreEqual("Breadcrumb", breadcrumb.ExportName);
        Assert.AreEqual("maxItemWidth", breadcrumb.Props.Single(static prop => prop.PublicName == "MaxItemWidth").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignBreadcrumbTheme?", breadcrumb.Props.Single(static prop => prop.PublicName == "Theme").TypeName);
        Assert.IsTrue(breadcrumb.Slots.Any(static slot => slot.Name == "separator"));
        Assert.IsTrue(breadcrumb.Slots.Any(static slot => slot.Name == "ellipsis"));

        var breadcrumbItem = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TBreadcrumbItem");
        Assert.AreEqual("BreadcrumbItem", breadcrumbItem.ExportName);
        Assert.AreEqual("content", breadcrumbItem.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("to", breadcrumbItem.Props.Single(static prop => prop.PublicName == "To").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignMenuRouteTarget?", breadcrumbItem.Props.Single(static prop => prop.PublicName == "To").TypeName);
        Assert.AreEqual("click", breadcrumbItem.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.AreEqual("ECMAScript.MouseEvent", breadcrumbItem.Emits.Single(static emit => emit.RazorAlias == "OnClick").PayloadTypeName);
        Assert.IsTrue(breadcrumbItem.Slots.Any(static slot => slot.Name == "icon"));

        var link = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TLink");
        Assert.AreEqual("Link", link.ExportName);
        Assert.AreEqual("content", link.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignLinkDownloadValue?", link.Props.Single(static prop => prop.PublicName == "Download").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignLinkTheme?", link.Props.Single(static prop => prop.PublicName == "Theme").TypeName);
        Assert.AreEqual("click", link.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.IsTrue(link.Slots.Any(static slot => slot.Name == "prefixIcon"));
        Assert.IsTrue(link.Slots.Any(static slot => slot.Name == "suffixIcon"));

        var tabs = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TTabs");
        Assert.AreEqual("Tabs", tabs.ExportName);
        Assert.AreEqual("value", tabs.Props.Single(static prop => prop.PublicName == "Value").Name);
        Assert.AreEqual("defaultValue", tabs.Props.Single(static prop => prop.PublicName == "DefaultValue").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabValue?", tabs.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.IsTrue(tabs.Props.Single(static prop => prop.PublicName == "Value").AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, tabs.Props.Single(static prop => prop.PublicName == "Value").Kind);
        Assert.AreEqual("change", tabs.Emits.Single(static emit => emit.RazorAlias == "ValueChanged").Name);
        Assert.AreEqual(VueEmitKind.ModelUpdate, tabs.Emits.Single(static emit => emit.RazorAlias == "ValueChanged").Kind);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabValue", tabs.Emits.Single(static emit => emit.RazorAlias == "ValueChanged").PayloadTypeName);
        Assert.AreEqual("add", tabs.Emits.Single(static emit => emit.RazorAlias == "OnAdd").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabAddContext", tabs.Emits.Single(static emit => emit.RazorAlias == "OnAdd").PayloadTypeName);
        Assert.AreEqual("dragSort", tabs.Emits.Single(static emit => emit.RazorAlias == "OnDragSort").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabsDragSortContext", tabs.Emits.Single(static emit => emit.RazorAlias == "OnDragSort").PayloadTypeName);
        Assert.AreEqual("remove", tabs.Emits.Single(static emit => emit.RazorAlias == "OnRemove").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabRemoveContext", tabs.Emits.Single(static emit => emit.RazorAlias == "OnRemove").PayloadTypeName);
        Assert.IsTrue(tabs.Slots.Any(static slot => slot.Name == "action"));

        var tabPanel = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TTabPanel");
        Assert.AreEqual("TabPanel", tabPanel.ExportName);
        Assert.AreEqual("label", tabPanel.Props.Single(static prop => prop.PublicName == "LabelText").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabValue?", tabPanel.Props.Single(static prop => prop.PublicName == "Value").TypeName);
        Assert.AreEqual("remove", tabPanel.Emits.Single(static emit => emit.RazorAlias == "OnRemove").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignTabPanelRemoveContext", tabPanel.Emits.Single(static emit => emit.RazorAlias == "OnRemove").PayloadTypeName);
        Assert.IsTrue(tabPanel.Slots.Any(static slot => slot.Name == "label"));
        Assert.IsTrue(tabPanel.Slots.Any(static slot => slot.IsDefault));

        var avatar = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TAvatar");
        Assert.AreEqual("Avatar", avatar.ExportName);
        Assert.AreEqual("content", avatar.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignAvatarShape?", avatar.Props.Single(static prop => prop.PublicName == "Shape").TypeName);
        Assert.AreEqual("error", avatar.Emits.Single(static emit => emit.RazorAlias == "OnError").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignAvatarErrorContext", avatar.Emits.Single(static emit => emit.RazorAlias == "OnError").PayloadTypeName);
        Assert.IsTrue(avatar.Slots.Any(static slot => slot.Name == "icon"));

        var avatarGroup = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TAvatarGroup");
        Assert.AreEqual("AvatarGroup", avatarGroup.ExportName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignAvatarGroupCascading?", avatarGroup.Props.Single(static prop => prop.PublicName == "Cascading").TypeName);
        Assert.IsTrue(avatarGroup.Slots.Any(static slot => slot.Name == "collapseAvatar"));

        var badge = tdesignDescriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.TDesign.TBadge");
        Assert.AreEqual("Badge", badge.ExportName);
        Assert.AreEqual("count", badge.Props.Single(static prop => prop.PublicName == "CountValue").Name);
        Assert.AreEqual("content", badge.Props.Single(static prop => prop.PublicName == "Text").Name);
        Assert.AreEqual("ECMAScript.TDesign.TDesignBadgeCountValue?", badge.Props.Single(static prop => prop.PublicName == "CountValue").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignBadgeOffset?", badge.Props.Single(static prop => prop.PublicName == "Offset").TypeName);
        Assert.AreEqual("ECMAScript.TDesign.TDesignBadgeShape?", badge.Props.Single(static prop => prop.PublicName == "Shape").TypeName);
        Assert.IsTrue(badge.Slots.Any(static slot => slot.Name == "count"));
    }

    [TestMethod]
    public void Vuetify_InputInfrastructureSlotContexts_PreserveOfficialRefContracts()
    {
        AssertPropertyType<VInputSlotContext>("Id", typeof(VueComputedRef<string>));
        AssertPropertyType<VInputSlotContext>("MessagesId", typeof(VueComputedRef<string>));
        AssertPropertyType<VInputSlotContext>("IsDirty", typeof(VueComputedRef<bool>));
        AssertPropertyType<VInputSlotContext>("IsDisabled", typeof(VueComputedRef<bool>));
        AssertPropertyType<VInputSlotContext>("IsReadonly", typeof(VueComputedRef<bool>));
        AssertPropertyType<VInputSlotContext>("IsPristine", typeof(IVueRef<bool>));
        AssertPropertyType<VInputSlotContext>("IsValid", typeof(VueComputedRef<bool?>));
        AssertPropertyType<VInputSlotContext>("IsValidating", typeof(IVueRef<bool>));

        AssertPropertyType<VFieldSlotContext>("IsActive", typeof(IVueRef<bool>));
        AssertPropertyType<VFieldSlotContext>("IsFocused", typeof(IVueRef<bool>));
        AssertPropertyType<VFieldSlotContext>("ControlRef", typeof(IVueRef<Element?>));
        AssertPropertyType<VFieldLabelSlotContext>("IsActive", typeof(IVueRef<bool>));
        AssertPropertyType<VFieldLabelSlotContext>("IsFocused", typeof(IVueRef<bool>));
        AssertPropertyType<VFieldLabelSlotContext>("ControlRef", typeof(IVueRef<Element?>));

        AssertPropertyType<VSelectionControlDefaultSlotContext>("BackgroundColorClasses", typeof(IVueRef<string[]>));
        AssertPropertyType<VSelectionControlDefaultSlotContext>("BackgroundColorStyles", typeof(IVueRef<VuetifyCssProperties>));
        AssertPropertyType<VSelectionControlInputSlotContext>("Model", typeof(VueWritableComputedRef<bool>));
        AssertPropertyType<VSelectionControlInputSlotContext>("TextColorClasses", typeof(IVueRef<string[]>));
        AssertPropertyType<VSelectionControlInputSlotContext>("TextColorStyles", typeof(IVueRef<VuetifyCssProperties>));
        AssertPropertyType<VSelectionControlInputSlotContext>("BackgroundColorClasses", typeof(IVueRef<string[]>));
        AssertPropertyType<VSelectionControlInputSlotContext>("BackgroundColorStyles", typeof(IVueRef<VuetifyCssProperties>));
        AssertPropertyType<VSelectionControlInputDefaultSlotContext>("BackgroundColorClasses", typeof(IVueRef<string[]>));
        AssertPropertyType<VSelectionControlInputDefaultSlotContext>("BackgroundColorStyles", typeof(IVueRef<VuetifyCssProperties>));

        AssertPropertyType<VSwitchSlotContext>("Model", typeof(IVueRef<bool>));
        AssertPropertyType<VSwitchSlotContext>("IsValid", typeof(VueComputedRef<bool?>));

        AssertPropertyType<VuetifyWindowGroupProvide>("Selected", typeof(IVueRef<string[]>));
        AssertPropertyType<VuetifyWindowGroupProvide>("SelectedClass", typeof(IVueRef<string?>));
        AssertPropertyType<VuetifyWindowGroupProvide>("Items", typeof(VueComputedRef<VuetifyWindowGroupItem[]>));
        AssertPropertyType<VuetifyWindowGroupProvide>("Disabled", typeof(IVueRef<bool?>));
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
                [VueProp(nameof(Label), VuePropKind.HtmlLike, Name = "buttonLabel", Required = true, DefaultExpression = "'Save'", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(OnSubmit), VueEmitKind.LibrarySpecific, Name = "onSaveNow", PayloadTypeName = "Demo.Payload")]
                [VueSlot(nameof(Footer), Name = "actions", Required = true, ContextTypeName = "Demo.FooterContext", ContextParameterName = "item")]
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
    public void RazorVue_Snapshot_IgnoresNullForgivingParameterInitializer_AsDefaultSource()
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
                [ECMAScript.ECMAScriptModule("./components/required-card")]
                public class RequiredCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = default!;
                }
            }
            """);

        var titleProp = snapshot.Descriptor.Props.Single(static prop => prop.PublicName == "Title");

        Assert.AreEqual((string?)null, titleProp.DefaultExpression);
        Assert.AreEqual(VuePropDefaultSource.None, titleProp.DefaultSource);
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
    public void RazorVue_Snapshot_ContainsSupportedLogicProperties()
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

                    private string Prefix => "Count: ";

                    public string FormatTitle()
                        => Prefix + Value;
                }
            }
            """);

        Assert.AreEqual(0, snapshot.Logic.Fields.Length);
        Assert.AreEqual(1, snapshot.Logic.Properties.Length);
        Assert.AreEqual("Prefix", snapshot.Logic.Properties[0].Name);
        Assert.IsTrue(snapshot.Logic.Properties[0].IsReadOnly);
        Assert.AreEqual(VueLogicPropertyLoweringKind.GetterFunction, snapshot.Logic.Properties[0].LoweringKind);
        Assert.AreEqual("FormatTitle", snapshot.Logic.Methods.Single().Name);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ClassifiesCustomGetterPrivateSetterWithoutLaterWritesAsGetterFunction()
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
                    private string _prefix = "Count: ";

                    private string Prefix
                    {
                        get => _prefix.Trim();
                        set => _prefix = value;
                    }

                    public string FormatTitle()
                        => Prefix;
                }
            }
            """);

        Assert.AreEqual(1, snapshot.Logic.Properties.Length);
        Assert.AreEqual("Prefix", snapshot.Logic.Properties[0].Name);
        Assert.IsFalse(snapshot.Logic.Properties[0].IsReadOnly);
        Assert.AreEqual(VueLogicPropertyLoweringKind.GetterFunction, snapshot.Logic.Properties[0].LoweringKind);
        Assert.AreEqual("FormatTitle", snapshot.Logic.Methods.Single().Name);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ClassifiesDeclarationInitializedSetupPropertyAsValueBinding()
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
                [ECMAScript.ECMAScriptModule("./components/value-property-card")]
                public class ValuePropertyCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    private string Prefix { get; } = "Count: ";

                    public string FormatTitle()
                        => Prefix + Value;
                }
            }
            """);

        Assert.AreEqual(1, snapshot.Logic.Properties.Length);
        Assert.AreEqual("Prefix", snapshot.Logic.Properties[0].Name);
        Assert.IsTrue(snapshot.Logic.Properties[0].IsReadOnly);
        Assert.AreEqual(VueLogicPropertyLoweringKind.ValueBinding, snapshot.Logic.Properties[0].LoweringKind);
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

        compilation = (CSharpCompilation)InjectCarrierCompilation(compilation, documentPath, importsPath, "<section>Hello</section>", "@using Demo.Shared");
        context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        Assert.IsNotNull(snapshot.RazorIrCarrier);
        Assert.AreEqual(documentPath, snapshot.RazorIrCarrier.DocumentPath);
        CollectionAssert.AreEqual(new[] { importsPath }, snapshot.RazorIrCarrier.ImportDocumentPaths.ToArray());
    }

    [TestMethod]
    public void RazorVue_Context_ResolvesPrimaryAndImportRazorDocuments_FromCarrier()
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

        compilation = (CSharpCompilation)InjectCarrierCompilation(
            compilation,
            documentPath.Replace('\\', '/'),
            importsPath.Replace('\\', '/'),
            documentText,
            importsText);
        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        Assert.IsNotNull(snapshot.RazorIrCarrier);
        Assert.AreEqual(documentText, snapshot.RazorIrCarrier.DocumentText);
        Assert.AreEqual(1, snapshot.RazorIrCarrier.Imports.Length);
        Assert.AreEqual(importsText, snapshot.RazorIrCarrier.Imports[0].Text);
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

        Assert.IsNull(snapshot.RazorIrCarrier);
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

    private static void AssertPropertyType<TDeclaringType>(string propertyName, Type expectedType)
    {
        var property = typeof(TDeclaringType).GetProperty(propertyName);
        Assert.IsNotNull(property, $"{typeof(TDeclaringType).FullName}.{propertyName}");
        Assert.AreEqual(expectedType, Nullable.GetUnderlyingType(property!.PropertyType) ?? property.PropertyType);
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

    private static Compilation InjectCarrierCompilation(
        Compilation compilation,
        string documentPath,
        string importsPath,
        string documentText,
        string importsText)
    {
        var componentTree = compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith(".razor.cs", StringComparison.Ordinal));
        var importsJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                Path = importsPath,
                Text = importsText
            }
        });
        var updatedSource = InjectCarrierAttribute(componentTree.ToString(), documentPath, importsJson, documentText);
        return compilation.ReplaceSyntaxTree(
            componentTree,
            CSharpSyntaxTree.ParseText(updatedSource, path: componentTree.FilePath));
    }

    private static string InjectCarrierAttribute(string componentSource, string documentPath, string importsJson, string documentText)
    {
        if (componentSource.Contains("RazorVueRazorIrCarrierAttribute", StringComparison.Ordinal))
            return componentSource;

        const string attributePrefix = "[ECMAScript.ECMAScriptModule";
        var markerStart = componentSource.IndexOf(attributePrefix, StringComparison.Ordinal);
        if (markerStart < 0)
            return componentSource;

        var markerEnd = componentSource.IndexOf(']', markerStart);
        if (markerEnd < 0)
            return componentSource;

        var marker = componentSource.Substring(markerStart, markerEnd - markerStart + 1);
        var insertion = string.Join(
            Environment.NewLine,
            marker,
            "    [Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute(",
            "        " + ToVerbatimLiteral(documentPath) + ",",
            "        " + ToVerbatimLiteral(importsJson) + ",",
            "        " + ToVerbatimLiteral(documentText) + ")]");

        return componentSource.Remove(markerStart, marker.Length).Insert(markerStart, insertion);
    }

    private static string ToVerbatimLiteral(string text)
        => "@\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
