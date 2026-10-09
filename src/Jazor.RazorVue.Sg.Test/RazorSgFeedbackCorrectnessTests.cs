using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgFeedbackCorrectnessTests
{
    [TestMethod]
    public async Task MissingModule_RoutedVueComponent_ReportsActionableCandidateDiagnostic()
    {
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/MissingModuleFeedback.razor"),
            "@page \"/missing-module\"\n<div>Missing</div>",
            "namespace Demo.Pages; public partial class MissingModuleFeedback : ComponentBase, IVueComponent { }",
            "Demo.Pages");

        var diagnostic = diagnostics.Single(item => item.Id == "JAZORVGA027");
        StringAssert.Contains(diagnostic.GetMessage(), "MissingModuleFeedback");
        StringAssert.Contains(diagnostic.GetMessage(), "[ECMAScriptModule(\"./components/name\")]");
        Assert.AreNotEqual(Location.None, diagnostic.Location);
    }

    [TestMethod]
    public async Task PlainComponentBase_RazorReusableComponent_ReportsMarkerDiagnosticBeforeUse()
    {
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Components/PlainReusableFeedback.razor"),
            "<div>Reusable</div>",
            "namespace Demo.Components; public partial class PlainReusableFeedback : ComponentBase { }",
            "Demo.Components");

        var diagnostic = diagnostics.Single(item => item.Id == "JAZORVGA027");
        StringAssert.Contains(diagnostic.GetMessage(), "IVueComponent");
        StringAssert.Contains(diagnostic.GetMessage(), "PlainReusableFeedback");
    }

    [TestMethod]
    public void CandidateAudit_AbstractSourceBasesAndExternalBindings_DoNotRequireGeneratedModules()
    {
        var compilation = CSharpCompilation.Create(
            "Feedback.Candidates",
            [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using static ECMAScript.Vue;
                namespace Demo;
                public abstract class AppComponentBase : ComponentBase, IVueComponent { }
                [ECMAScript("binding")]
                public sealed class ExternalBinding : AppComponentBase { }
                public class ServerComponent : ComponentBase { }
                """, new CSharpParseOptions(LanguageVersion.Preview), "ExternalBinding.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.IsEmpty(ComponentSelector.ValidateCurrentComponentContracts(compilation));
        Assert.IsTrue(RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation, CancellationToken.None, out var catalog, out var diagnostics));
        Assert.IsNull(catalog);
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task PascalClass_ExternalBindingWithCssClass_ReportsMappedAttributeDiagnostic()
    {
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/AttributeCollisionFeedback.razor"),
            "@using ECMAScript.ElementPlus\n<ElCard Class=\"panel\">Content</ElCard>",
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/attribute-collision-feedback")]
            public partial class AttributeCollisionFeedback : ComponentBase, IVueComponent { }
            """,
            "Demo.Pages");

        var diagnostic = diagnostics.Single(item => item.Id == "JAZORVGA028");
        StringAssert.Contains(diagnostic.GetMessage(), "CssClass");
        StringAssert.Contains(diagnostic.GetMessage(), "Class");
        StringAssert.Contains(diagnostic.Location.GetLineSpan().Path, "AttributeCollisionFeedback.razor");
    }

    [TestMethod]
    public async Task BindingConstantDefaults_PreserveInheritanceUnionAndExplicitSplatPrecedence()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/BindingDefaultsFeedback.razor"),
            """
            @using Demo.Bindings
            <DefaultTable />
            <DefaultTable MaxHeight="@((TableHeight)240)" CssClass="explicit" />
            <DefaultTable @attributes="Overrides" />
            """,
            """
            using System.Collections.Generic;
            namespace Demo.Pages;
            [ECMAScriptModule("./components/binding-defaults-feedback")]
            public partial class BindingDefaultsFeedback : ComponentBase, IVueComponent
            {
                private IReadOnlyDictionary<string, object?> Overrides { get; } =
                    new Dictionary<string, object?> { ["MaxHeight"] = 120, ["class"] = "splat", ["enabled"] = false };
            }
            """,
            "Demo.Pages",
            "Demo.Pages.BindingDefaultsFeedback",
            new Dictionary<string, string>
            {
                ["Bindings.cs"] = """
                namespace Demo.Bindings;
                [ECMAScript]
                public readonly union TableHeight(int, string);
                public abstract class TableContract : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("maxHeight")]
                    public TableHeight MaxHeight { get; set; } = 480;
                    [Parameter, ECMAScriptName("class")]
                    public string? CssClass { get; set; } = "default-class";
                    [Parameter, ECMAScriptName("enabled")]
                    public bool Enabled { get; set; } = true;
                    [Parameter(CaptureUnmatchedValues = true)]
                    public System.Collections.Generic.IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                }
                [ECMAScript("feedback-binding"), ECMAScriptName("Table")]
                public sealed class DefaultTable : TableContract { }
                """
            });

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/binding-defaults-feedback.js", observation.ModuleText,
            "binding-defaults-feedback.test.mjs",
            """
            import assert from "node:assert/strict";
            import component from "./components/binding-defaults-feedback.js";
            const render = component.setup({}, { slots: {} });
            const children = render().children.filter(node => node?.name?.name === "table");
            assert.equal(children[0].props.maxHeight, 480);
            assert.equal(children[0].props.class, "default-class");
            assert.equal(children[0].props.enabled, true);
            assert.equal(children[1].props.maxHeight, 240);
            assert.equal(children[1].props.class, "explicit");
            assert.equal(children[2].props.maxHeight, 120);
            assert.equal(children[2].props.class, "splat");
            assert.equal(children[2].props.enabled, false);
            """,
            new Dictionary<string, string>
            {
                ["node_modules/feedback-binding/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/feedback-binding/index.mjs"] = "export const Table = { name: 'table' };"
            });
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task BindingNonConstantDefault_RequiresExplicitOverrideInsteadOfRepeatingSideEffectsDuringRender(bool overridden)
    {
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/NonConstantDefaultsFeedback.razor"),
            "@using Demo.Bindings\n" + (overridden ? "<DynamicDefaultTable MaxHeight=\"240\" />" : "<DynamicDefaultTable />"),
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/non-constant-defaults-feedback")]
            public partial class NonConstantDefaultsFeedback : ComponentBase, IVueComponent { }
            """,
            "Demo.Pages",
            new Dictionary<string, string>
            {
                ["DynamicDefaultBindings.cs"] = """
                namespace Demo.Bindings;
                [ECMAScript("feedback-binding")]
                public sealed class DynamicDefaultTable : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("maxHeight")]
                    public int MaxHeight { get; set; } = GetHeight();
                    private static int GetHeight() => 480;
                }
                """
            });

        if (overridden)
        {
            Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
            return;
        }

        var diagnostic = diagnostics.Single(item => item.Id == "JAZORVGA021");
        StringAssert.Contains(diagnostic.GetMessage(), "compile-time constant initializer");
        StringAssert.Contains(diagnostic.GetMessage(), "MaxHeight");
        StringAssert.Contains(diagnostic.Location.GetLineSpan().Path, "DynamicDefaultBindings.cs");
    }

    [TestMethod]
    public void CandidateAudit_MultipleInvalidCandidates_ReportsStableOrderAndEveryMissingContract()
    {
        var compilation = CSharpCompilation.Create(
            "Feedback.CandidateOrder",
            [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using static ECMAScript.Vue;
                namespace Demo;
                public class ZMissingModule : ComponentBase, IVueComponent { }
                [ECMAScriptModule("./components/a")]
                public class AMissingMarker : ComponentBase { }
                public class BNotComponentBase : IVueComponent { }
                """, new CSharpParseOptions(LanguageVersion.Preview), "Candidates.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.IsFalse(RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation, CancellationToken.None, out var catalog, out var diagnostics));
        Assert.IsNull(catalog);
        Assert.HasCount(3, diagnostics);
        StringAssert.Contains(diagnostics[0].Message, "AMissingMarker");
        StringAssert.Contains(diagnostics[1].Message, "BNotComponentBase");
        StringAssert.Contains(diagnostics[2].Message, "ZMissingModule");
        Assert.IsTrue(diagnostics.All(item => item.Category == RazorVueDiagnosticCategory.ComponentCandidate));
    }
}
