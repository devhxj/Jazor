using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRenderFrontendContractTests
{
    [TestMethod]
    public void BuildRenderTreeBaselineExtractor_CoversHandwrittenAndRazorGeneratedRenderBodies()
    {
        var handwrittenContext = CreateHandwrittenBuildRenderTreeContext();
        var handwrittenSnapshot = handwrittenContext.CreateSemanticSnapshots()
            .Single(static snapshot => snapshot.Descriptor.Name == "CounterCard");

        var handwrittenBaseline = BuildRenderTreeTemplateFrontend.Instance.CreateRenderBaseline(
            handwrittenContext,
            handwrittenSnapshot);

        AssertElementTextPattern(handwrittenBaseline, "section");

        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        var (razorContext, razorSnapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RenderFrontendContract.RazorGeneratedBaseline.Tests",
            documentPath,
            """<section>@Title</section>""",
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var razorGeneratedBaseline = BuildRenderTreeTemplateFrontend.Instance.CreateRenderBaseline(
            razorContext,
            razorSnapshot);

        AssertElementTextPattern(razorGeneratedBaseline, "section");
        Assert.IsNotNull(razorSnapshot.RazorSourceGeneratorDocument);
    }

    [TestMethod]
    public void BaselineFirstTemplateFrontend_AppliesOptionalEnhancementAfterBaselineExtraction()
    {
        var context = CreateHandwrittenBuildRenderTreeContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.Descriptor.Name == "CounterCard");
        var baselineExtractor = new RecordingBaselineExtractor(BuildRenderTreeTemplateFrontend.Instance);
        var enhancement = new RecordingEnhancement();
        var frontend = new RazorVueBaselineFirstTemplateFrontend(baselineExtractor, enhancement);

        var renderTree = frontend.CreateRenderTree(context, snapshot);

        Assert.IsTrue(baselineExtractor.WasCalled);
        Assert.IsTrue(enhancement.WasCalled);
        Assert.AreSame(baselineExtractor.RenderTree, enhancement.ReceivedBaseline);
        var marker = renderTree.Children.Single() as RazorVueTextNode;
        Assert.IsNotNull(marker);
        Assert.AreEqual("enhanced", marker.Text);
    }

    [TestMethod]
    public void BaselineFirstTemplateFrontend_PreservesBaselineWhenEnhancementHasNoInput()
    {
        var context = CreateHandwrittenBuildRenderTreeContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.Descriptor.Name == "CounterCard");
        var frontend = new RazorVueBaselineFirstTemplateFrontend(
            BuildRenderTreeTemplateFrontend.Instance,
            new RazorVueRazorIrTemplateFrontend());

        var renderTree = frontend.CreateRenderTree(context, snapshot);

        AssertElementTextPattern(renderTree, "section");
        Assert.IsNull(snapshot.RazorSourceGeneratorDocument);
    }

    [TestMethod]
    public void BaselineFirstTemplateFrontend_DoesNotClaimExactSourceEnhancementWhenRazorIrInputIsMissing()
    {
        var context = CreateHandwrittenBuildRenderTreeContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.Descriptor.Name == "CounterCard");
        var baselineFrontend = new RazorVueBaselineFirstTemplateFrontend(BuildRenderTreeTemplateFrontend.Instance);
        var enhancedFrontend = new RazorVueBaselineFirstTemplateFrontend(
            BuildRenderTreeTemplateFrontend.Instance,
            new RazorVueRazorIrTemplateFrontend());

        var baselineArtifact = new RazorVueSfcArtifactFactory(baselineFrontend).Lower(context, snapshot);
        var enhancedArtifact = new RazorVueSfcArtifactFactory(enhancedFrontend).Lower(context, snapshot);

        Assert.AreEqual(baselineArtifact.SfcText, enhancedArtifact.SfcText);
        Assert.AreEqual(baselineArtifact.Identity.TemplateHash, enhancedArtifact.Identity.TemplateHash);
        Assert.IsFalse(
            enhancedArtifact.SourceOrigins.Any(static origin => origin.MappingQuality == RazorVueMappingQuality.ExactSource),
            "Razor IR enhancement must not claim exact Razor source origins when the snapshot has no Razor SG/IR document.");
    }

    [TestMethod]
    public void BuildRenderTreeBaselineExtractor_ThrowsActionableIssueWhenRenderBodyIsMissing()
    {
        var context = CreatePartialComponentWithoutGeneratedRenderBodyContext();
        var snapshot = context.CreateSemanticSnapshots().Single();

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            BuildRenderTreeTemplateFrontend.Instance.CreateRenderBaseline(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "BuildRenderTree");
        StringAssert.Contains(exception.Issue.Message, "Demo.Components.PartialOnly");
        StringAssert.Contains(exception.Issue.Message, "Razor SG tail");
        StringAssert.Contains(exception.Issue.Message, "generated C#");
    }

    [TestMethod]
    public void RazorIrEnhancement_OnlyAddsExactSourceOriginsWhenBaselineShapeMatches()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RenderFrontendContract.RazorIrEnhancement.Matching.Tests",
            documentPath,
            """<section>@Title</section>""",
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());
        var baselineFrontend = new RazorVueBaselineFirstTemplateFrontend(BuildRenderTreeTemplateFrontend.Instance);
        var enhancedFrontend = new RazorVueBaselineFirstTemplateFrontend(
            BuildRenderTreeTemplateFrontend.Instance,
            new RazorVueRazorIrTemplateFrontend());

        var baselineArtifact = new RazorVueSfcArtifactFactory(baselineFrontend).Lower(context, snapshot);
        var enhancedArtifact = new RazorVueSfcArtifactFactory(enhancedFrontend).Lower(context, snapshot);

        Assert.AreEqual(baselineArtifact.SfcText, enhancedArtifact.SfcText);
        Assert.AreEqual(baselineArtifact.Identity.TemplateHash, enhancedArtifact.Identity.TemplateHash);
        Assert.AreEqual(baselineArtifact.Identity.LogicHash, enhancedArtifact.Identity.LogicHash);
        CollectionAssert.AreEqual(baselineArtifact.Imports.ToArray(), enhancedArtifact.Imports.ToArray());
        Assert.IsTrue(
            enhancedArtifact.SourceOrigins.Any(origin =>
                origin.MappingQuality == RazorVueMappingQuality.ExactSource &&
                PathsEqual(origin.SourceFilePath, documentPath)),
            "Razor IR enhancement should add exact Razor source origins without changing generated SFC output. Origins:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, enhancedArtifact.SourceOrigins.Select(DescribeOrigin)));
    }

    [TestMethod]
    public void RazorIrEnhancement_NoOpsWhenRazorIrShapeWouldChangeBaselineSemantics()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RenderFrontendContract.RazorIrEnhancement.Mismatch.Tests",
            documentPath,
            """<section>@Title</section>""",
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());
        var baseline = new RazorVueRenderFragment(
        [
            new RazorVueTextNode("baseline-owned-render", ImmutableArray<RazorVueSourceOrigin>.Empty)
        ]);
        var enhancement = new RazorVueRazorIrTemplateFrontend();

        var enhanced = enhancement.TryEnhanceRenderTree(context, snapshot, baseline, out var enhancedRenderTree);

        Assert.IsFalse(enhanced);
        Assert.AreSame(baseline, enhancedRenderTree);
        var text = enhancedRenderTree.Children.Single() as RazorVueTextNode;
        Assert.IsNotNull(text);
        Assert.AreEqual("baseline-owned-render", text.Text);
    }

    private static RazorVueCompilationContext CreateHandwrittenBuildRenderTreeContext()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RenderFrontendContract.HandwrittenBaseline.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace ECMAScript
                    {
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

                            protected override void BuildRenderTree(RenderTreeBuilder builder)
                            {
                                builder.OpenElement(0, "section");
                                builder.AddContent(1, Title);
                                builder.CloseElement();
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "CounterCard.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static RazorVueCompilationContext CreatePartialComponentWithoutGeneratedRenderBodyContext()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RenderFrontendContract.MissingRenderBody.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
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
                        [ECMAScript.ECMAScriptModule("./components/partial-only")]
                        public partial class PartialOnly : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "PartialOnly.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static void AssertElementTextPattern(RazorVueRenderFragment renderTree, string tagName)
    {
        var element = renderTree.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(element);
        Assert.AreEqual(tagName, element.TagName);
        Assert.AreEqual(1, element.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(element.Children.Children[0]);
    }

    private static bool PathsEqual(string? left, string? right)
        => string.Equals(
            NormalizePath(left),
            NormalizePath(right),
            StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Replace('\\', '/');

    private static string DescribeOrigin(RazorVueSourceOrigin origin)
        => $"{origin.MappingQuality}|{origin.SourceFilePath}|{origin.SourceSpanStart}|{origin.SourceSpanLength}";

    private sealed class RecordingBaselineExtractor(IRazorVueRenderBaselineExtractor inner)
        : IRazorVueRenderBaselineExtractor
    {
        public string Name => "RecordingBaselineExtractor";

        public bool WasCalled { get; private set; }

        public RazorVueRenderFragment? RenderTree { get; private set; }

        public RazorVueRenderFragment CreateRenderBaseline(
            RazorVueCompilationContext context,
            RazorVueSemanticSnapshot snapshot)
        {
            WasCalled = true;
            RenderTree = inner.CreateRenderBaseline(context, snapshot);
            return RenderTree;
        }
    }

    private sealed class RecordingEnhancement : IRazorVueRenderEnhancement
    {
        public string Name => "RecordingEnhancement";

        public bool WasCalled { get; private set; }

        public RazorVueRenderFragment? ReceivedBaseline { get; private set; }

        public bool TryEnhanceRenderTree(
            RazorVueCompilationContext context,
            RazorVueSemanticSnapshot snapshot,
            RazorVueRenderFragment baselineRenderTree,
            out RazorVueRenderFragment enhancedRenderTree)
        {
            WasCalled = true;
            ReceivedBaseline = baselineRenderTree;
            enhancedRenderTree = new RazorVueRenderFragment(
            [
                new RazorVueTextNode("enhanced", ImmutableArray<RazorVueSourceOrigin>.Empty)
            ]);
            return true;
        }
    }
}
