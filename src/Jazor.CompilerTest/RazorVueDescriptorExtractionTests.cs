using Basic.Reference.Assemblies;
using ECMAScript.UI.Vue.Vuetify;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVueDescriptorExtractionTests
{
    [TestMethod]
    public void RazorVue_Context_DiscoversVueComponentCandidates()
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

            [ECMAScript.ECMAScriptModule]
            public static class LegacyModule
            {
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule]
                public partial class Counter : VueComponent
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
                [ECMAScript.ECMAScriptModule("./components/counter")]
                public class Counter : VueComponent
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/styles")]
                [VueLibraryPluginRequirement("demo-host")]
                public sealed class DemoButton : VueLibraryComponent
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
    public void RazorVue_Context_DiscoversVuetifyPackageLibraryDescriptors_FromReferencedAssembly()
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
                [ECMAScript.ECMAScriptModule("./components/host-card")]
                public class HostCard : VueComponent
                {
                }
            }
            """);

        var descriptors = context.DiscoverLibraryComponents();

        var vuetifyDescriptors = descriptors
            .Where(static descriptor => descriptor.ResolutionNamespace == "ECMAScript.UI.Vue.Vuetify")
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "VAlert", "VBtn", "VCard", "VCardText", "VCardTitle", "VCheckbox", "VChip", "VCol", "VContainer", "VDialog", "VDivider", "VIcon", "VList", "VListItem", "VRow", "VSheet", "VSpacer", "VSwitch", "VTextField", "VTextarea", "VToolbar", "VToolbarTitle" },
            vuetifyDescriptors
                .Select(static descriptor => descriptor.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());

        var textField = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VTextField");
        Assert.AreEqual("vuetify/components", textField.ImportSpecifier);
        Assert.AreEqual("VTextField", textField.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, textField.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, textField.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", textField.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textField.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var checkbox = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VCheckbox");
        Assert.AreEqual("vuetify/components", checkbox.ImportSpecifier);
        Assert.AreEqual("VCheckbox", checkbox.ExportName);
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, checkbox.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, checkbox.PluginRequirements.ToArray());
        Assert.AreEqual("modelValue", checkbox.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", checkbox.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var dialog = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VDialog");
        var activator = dialog.Slots.Single(static slot => slot.Name == "activator");
        Assert.HasCount(1, activator.Parameters);
        Assert.AreEqual("context", activator.Parameters[0].Name);
        Assert.AreEqual("ECMAScript.UI.Vue.Vuetify.VDialogActivatorContext", activator.Parameters[0].TypeName);

        var column = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VCol");
        CollectionAssert.AreEqual(new[] { "vuetify" }, column.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, column.StyleDependencies.ToArray());
        Assert.AreEqual("cols", column.Props.Single(static prop => prop.PublicName == "Cols").Name);
        Assert.AreEqual("md", column.Props.Single(static prop => prop.PublicName == "Md").Name);

        var toolbar = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VToolbar");
        CollectionAssert.AreEqual(new[] { "vuetify" }, toolbar.PluginRequirements.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, toolbar.StyleDependencies.ToArray());
        Assert.AreEqual("color", toolbar.Props.Single(static prop => prop.PublicName == "Color").Name);
        Assert.AreEqual("density", toolbar.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.IsTrue(toolbar.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var textarea = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VTextarea");
        Assert.AreEqual("rows", textarea.Props.Single(static prop => prop.PublicName == "Rows").Name);
        Assert.AreEqual("modelValue", textarea.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", textarea.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var toggle = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VSwitch");
        Assert.AreEqual("modelValue", toggle.Props.Single(static prop => prop.PublicName == "ModelValue").Name);
        Assert.AreEqual("update:modelValue", toggle.Emits.Single(static emit => emit.RazorAlias == "ModelValueChanged").Name);

        var list = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VList");
        Assert.AreEqual("density", list.Props.Single(static prop => prop.PublicName == "Density").Name);
        Assert.IsTrue(list.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var listItem = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VListItem");
        Assert.AreEqual("title", listItem.Props.Single(static prop => prop.PublicName == "Title").Name);
        Assert.AreEqual("subtitle", listItem.Props.Single(static prop => prop.PublicName == "Subtitle").Name);
        Assert.IsTrue(listItem.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var alert = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VAlert");
        Assert.AreEqual("type", alert.Props.Single(static prop => prop.PublicName == "Type").Name);
        Assert.AreEqual("variant", alert.Props.Single(static prop => prop.PublicName == "Variant").Name);
        Assert.IsTrue(alert.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var chip = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VChip");
        Assert.AreEqual("click", chip.Emits.Single(static emit => emit.RazorAlias == "OnClick").Name);
        Assert.IsTrue(chip.Slots.Single(static slot => slot.IsDefault).IsDefault);

        var spacer = descriptors.Single(static descriptor => descriptor.FullName == "ECMAScript.UI.Vue.Vuetify.VSpacer");
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/styles")]
                [VueLibraryStyle(" demo/styles ")]
                public sealed class DemoButton : VueLibraryComponent
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryPluginRequirement("demo-host")]
                [VueLibraryPluginRequirement(" demo-host ")]
                public sealed class DemoButton : VueLibraryComponent
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => context.DiscoverLibraryComponents());
        Assert.AreEqual(RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "duplicate plugin requirement");
    }

    [TestMethod]
    public void RazorVue_Candidate_ExtractsLifecycleAndLogicMethods()
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : VueComponent, IDisposable, IAsyncDisposable
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
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : VueComponent, IDisposable, IAsyncDisposable
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
                [ECMAScript.ECMAScriptModule("./components/helper-card")]
                public class HelperCard : VueComponent
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
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VBtn).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

