using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using ECMAScript.UI.Vue.Vuetify;
using Jazor.Analyzer;
using Jazor.Razor;
using Jazor.RazorVue;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVueAnalyzerTests
{
    [TestMethod]
    public async Task RazorVue_Entry_ValidVueComponent_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
            {
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006", "JAZORVUE007", "JAZORVUE008", "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE012", "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_ComponentBaseOnly_ReportsJAZORVUE002()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE002");
        AssertNoDiagnostic(diagnostics, "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_StaticModule_RemainsOnLegacyPath()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public static class InvalidModule
            {
                public static Version Value = new();
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZOR001");
        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006", "JAZORVUE007", "JAZORVUE008", "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE012");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                public void Trigger()
                {
                    StateHasChanged();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE004");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    return Value > 0;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ConstantTrueShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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
            public class ValidComponent : VueComponent
            {
                protected override bool ShouldRender()
                {
                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ComponentBaseShouldRenderPassThrough_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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
            public class ValidComponent : VueComponent
            {
                protected override bool ShouldRender()
                {
                    return base.ShouldRender();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseOnlySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
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
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NonTrivialSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    Value++;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SupportedSetParametersAsyncBaseThenEmit_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
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
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PassThroughSetParametersAsyncToSupportedBaseEmit_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            public abstract class SetParametersAsyncBaseComponent : VueComponent
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : SetParametersAsyncBaseComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SetParametersAsyncWithBaseEmitAndDerivedEmit_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            public abstract class SetParametersAsyncBaseComponent : VueComponent
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : SetParametersAsyncBaseComponent
            {
                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_UnknownLibraryParameter_ReportsJAZORVUE007()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VBtn>(0);
                    builder.AddAttribute(1, "Href", "#");
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE007");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryBindTarget_ReportsJAZORVUE008()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                public string? Text { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VBtn>(0);
                    builder.AddAttribute(1, nameof(VBtn.Text), Text);
                    builder.AddAttribute(2, "TextChanged", EventCallback.Factory.Create<string?>(this, HandleTextChanged));
                    builder.CloseComponent();
                }

                private void HandleTextChanged(string? value)
                {
                    Text = value;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidLibraryParameter_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
            {
                [Parameter]
                public string? Label { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), Label);
                    builder.CloseComponent();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE007", "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidLibraryBindTarget_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
            {
                [Parameter]
                public string? Value { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.ModelValue), Value);
                    builder.AddAttribute(2, nameof(VTextField.ModelValueChanged), EventCallback.Factory.Create<string?>(this, HandleModelValueChanged));
                    builder.CloseComponent();
                }

                private void HandleModelValueChanged(string? value)
                {
                    Value = value;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE007", "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_UnknownLibrarySlot_ReportsJAZORVUE009()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                [Parameter]
                public RenderFragment? ChildContent { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VIcon>(0);
                    builder.AddContent(1, ChildContent);
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE009");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TypedLibrarySlotContextMisuse_ReportsJAZORVUE010()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDialog>(0);
                    builder.AddAttribute(1, nameof(VDialog.Activator), "not-callable");
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE010");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DuplicateLibrarySlotAssignment_ReportsJAZORVUE011()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
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
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE011");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidTypedLibrarySlot_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.UI.Vue.Vuetify;
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

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
            {
                [Parameter]
                public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDialog>(0);
                    builder.AddAttribute(1, nameof(VDialog.Activator), Activator);
                    builder.CloseComponent();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE009", "JAZORVUE010", "JAZORVUE011");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryComponentDeclaration_ReportsJAZORVUE012()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
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

            public sealed class InvalidLibraryComponent : VueLibraryComponent
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE012");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        var compilation = CreateCompilation(source);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new Jazor.Analyzer.Analyzer(),
            new RazorVueEntryAnalyzer(),
            new RazorVueMisuseAnalyzer(),
            new RazorVueAuthoringAnalyzer());

        var compileErrors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, compileErrors.Length, string.Join(Environment.NewLine, compileErrors.Select(static x => x.ToString())));

        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        // Roslyn test compilations need both the Razor substrate assembly and the
        // Vue-facing assembly because metadata references do not bring transitive
        // project references along automatically.
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VBtn).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Analyzer.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string id)
        => Assert.IsTrue(
            diagnostics.Any(diagnostic => diagnostic.Id == id),
            $"Expected diagnostic {id}, actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

    private static void AssertNoDiagnostic(IEnumerable<Diagnostic> diagnostics, params string[] ids)
    {
        var unexpected = diagnostics
            .Where(diagnostic => ids.Contains(diagnostic.Id, StringComparer.Ordinal))
            .ToArray();

        Assert.AreEqual(0, unexpected.Length, string.Join(Environment.NewLine, unexpected.Select(static x => x.ToString())));
    }
}
