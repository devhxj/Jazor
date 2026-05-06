using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueTemplateFrontendParityTests
{
    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForMarkupAndInterpolation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<section><h1>@Title</h1><p>Hello</p></section>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Markup.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForAttributes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" class="hero">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Attributes.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForComponentAndDefaultChildContent()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<ChildCard Title="@Title"><p>Body</p></ChildCard>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Component.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentAndChildComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontend_FallsBackToBuildRenderTree_OnlyForHandwrittenBuildRenderTreeComponents()
    {
        var context = CreateBuildRenderTreeOnlyContext();
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "CounterCard");

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, BuildRenderTreeTemplateFrontend.Instance, context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontend_WithRazorGeneratedBuildRenderTreeButNoBoundRazorDocument_Throws()
    {
        var context = CreateGeneratedRazorContextWithoutBoundDocuments();
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();

        Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.RazorDocumentPath));

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => RazorVuePreferredTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        StringAssert.Contains(exception.Message, "requires a bound Razor document");
    }

    private static void AssertParity(
        IRazorVueTemplateFrontend expectedFrontend,
        IRazorVueTemplateFrontend actualFrontend,
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        var expectedTree = expectedFrontend.CreateRenderTree(context, snapshot);
        var actualTree = actualFrontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            DescribeStructure(expectedTree),
            DescribeStructure(actualTree),
            "Template frontend render tree diverged.");

        Assert.AreEqual(CountOrigins(expectedTree), CountOrigins(actualTree), "Template origin entry count diverged.");
        Assert.IsTrue(
            EnumerateOrigins(actualTree).All(origin => string.Equals(origin.SourceFilePath, snapshot.RazorDocumentPath, StringComparison.OrdinalIgnoreCase) ||
                                                     string.IsNullOrWhiteSpace(snapshot.RazorDocumentPath)),
            "Template frontend emitted a non-primary Razor document path in template origins.");
        if (!string.IsNullOrWhiteSpace(snapshot.RazorDocumentPath))
        {
            Assert.IsTrue(
                EnumerateOrigins(actualTree).All(origin => origin.MappingQuality == RazorVueMappingQuality.ExactSource),
                "Preferred Razor IR template path should preserve exact Razor source origins.");
        }

        var expectedArtifact = new RazorVueArtifactFactory(expectedFrontend).Lower(context, snapshot);
        var actualArtifact = new RazorVueArtifactFactory(actualFrontend).Lower(context, snapshot);

        Assert.AreEqual(expectedArtifact.ModuleCode, actualArtifact.ModuleCode, "Generated module code diverged.");
        CollectionAssert.AreEqual(expectedArtifact.Imports.ToArray(), actualArtifact.Imports.ToArray(), "Generated imports diverged.");
        CollectionAssert.AreEqual(expectedArtifact.Styles.ToArray(), actualArtifact.Styles.ToArray(), "Generated styles diverged.");
        CollectionAssert.AreEqual(expectedArtifact.PluginRequirements.ToArray(), actualArtifact.PluginRequirements.ToArray(), "Generated plugin requirements diverged.");
        Assert.AreEqual(expectedArtifact.Identity.TemplateHash, actualArtifact.Identity.TemplateHash, "TemplateHash diverged.");
        Assert.AreEqual(expectedArtifact.Identity.LogicHash, actualArtifact.Identity.LogicHash, "LogicHash diverged.");
        Assert.AreEqual(expectedArtifact.Identity.HmrBoundaryKind, actualArtifact.Identity.HmrBoundaryKind, "HMR boundary diverged.");

        if (!string.IsNullOrWhiteSpace(snapshot.RazorDocumentPath))
        {
            Assert.IsTrue(
                actualArtifact.SourceOrigins.Any(origin => origin.MappingQuality == RazorVueMappingQuality.ExactSource),
                "Preferred Razor IR artifact did not preserve exact Razor source origins.");
        }
    }

    private static Jazor.RazorVue.RazorVueCompilationContext CreateBuildRenderTreeOnlyContext()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.PreferredFrontend.Fallback.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
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
                        [ECMAScript.ECMAScriptModule("./components/counter-card")]
                        public class CounterCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }

                            protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
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

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static Jazor.RazorVue.RazorVueCompilationContext CreateGeneratedRazorContextWithoutBoundDocuments()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.PreferredFrontend.GeneratedWithoutDocs.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
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

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    $$"""
                    #line 1 "{{importsPath}}"
                    using Demo.Pages;
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
                                __builder.AddContent(1, Title);
                                __builder.CloseElement();
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation, Jazor.RazorVue.RazorVueRazorDocumentSet.Empty);
        Assert.IsNotNull(context);
        return context;
    }

    private static string DescribeStructure(RazorVueRenderFragment fragment)
    {
        var builder = new StringBuilder();
        AppendFragment(builder, fragment, depth: 0);
        return builder.ToString();
    }

    private static void AppendFragment(StringBuilder builder, RazorVueRenderFragment fragment, int depth)
    {
        foreach (var node in fragment.Children)
            AppendNode(builder, node, depth);
    }

    private static void AppendNode(StringBuilder builder, RazorVueRenderNode node, int depth)
    {
        builder.Append(' ', depth * 2);

        switch (node)
        {
            case RazorVueElementNode element:
                builder.Append("Element(").Append(element.TagName).Append(')');
                AppendAttributes(builder, element.Attributes, includeOrigins: false);
                builder.AppendLine();
                AppendFragment(builder, element.Children, depth + 1);
                break;
            case RazorVueComponentNode component:
                builder.Append("Component(").Append(component.ComponentName).Append('|').Append(component.ComponentFullName).Append(')');
                AppendAttributes(builder, component.Attributes, includeOrigins: false);
                builder.AppendLine();
                AppendFragment(builder, component.Children, depth + 1);
                break;
            case RazorVueTextNode text:
                builder.Append("Text(").Append(text.Text).Append(')');
                builder.AppendLine();
                break;
            case RazorVueExpressionNode expression:
                builder.Append("Expression(").Append(expression.Expression.Syntax.ToString()).Append(')');
                builder.AppendLine();
                break;
            case RazorVueSlotOutletNode slot:
                builder.Append("Slot(").Append(slot.SlotName).Append(')');
                if (slot.Argument is not null)
                    builder.Append(" arg=").Append(slot.Argument.Syntax.ToString());
                builder.AppendLine();
                break;
            case RazorVueConditionalNode conditional:
                builder.Append("Conditional(").Append(conditional.Condition.Syntax.ToString()).Append(')');
                builder.AppendLine();
                AppendFragment(builder, conditional.WhenTrue, depth + 1);
                if (!conditional.WhenFalse.Children.IsDefaultOrEmpty)
                {
                    builder.Append(' ', (depth + 1) * 2).AppendLine("Else");
                    AppendFragment(builder, conditional.WhenFalse, depth + 2);
                }
                break;
            case RazorVueForEachNode loop:
                builder.Append("ForEach(").Append(loop.ItemName).Append(':').Append(loop.Source.Syntax.ToString()).Append(')');
                builder.AppendLine();
                AppendFragment(builder, loop.Body, depth + 1);
                break;
            default:
                builder.Append(node.GetType().Name).AppendLine();
                break;
        }
    }

    private static void AppendAttributes(StringBuilder builder, ImmutableArray<RazorVueAttributeNode> attributes, bool includeOrigins)
    {
        if (attributes.IsDefaultOrEmpty)
            return;

        builder.Append(" attrs=[");
        for (var index = 0; index < attributes.Length; index++)
        {
            if (index > 0)
                builder.Append(", ");

            var attribute = attributes[index];
            builder.Append(attribute.Name).Append('=').Append(attribute.Value?.Syntax.ToString() ?? "true");
            if (includeOrigins)
                builder.Append('@').Append(DescribeOrigins(attribute.Origins));
        }

        builder.Append(']');
    }

    private static int CountOrigins(RazorVueRenderFragment fragment)
        => EnumerateOrigins(fragment).Count();

    private static IEnumerable<RazorVueSourceOrigin> EnumerateOrigins(RazorVueRenderFragment fragment)
    {
        foreach (var node in fragment.Children)
        {
            foreach (var origin in EnumerateOrigins(node))
                yield return origin;
        }
    }

    private static IEnumerable<RazorVueSourceOrigin> EnumerateOrigins(RazorVueRenderNode node)
    {
        foreach (var origin in node.Origins)
            yield return origin;

        switch (node)
        {
            case RazorVueElementNode element:
                foreach (var attribute in element.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var child in element.Children.Children)
                {
                    foreach (var origin in EnumerateOrigins(child))
                        yield return origin;
                }
                break;
            case RazorVueComponentNode component:
                foreach (var attribute in component.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var child in component.Children.Children)
                {
                    foreach (var origin in EnumerateOrigins(child))
                        yield return origin;
                }
                break;
            case RazorVueConditionalNode conditional:
                foreach (var origin in EnumerateOrigins(conditional.WhenTrue))
                    yield return origin;
                foreach (var origin in EnumerateOrigins(conditional.WhenFalse))
                    yield return origin;
                break;
            case RazorVueForEachNode loop:
                foreach (var origin in EnumerateOrigins(loop.Body))
                    yield return origin;
                break;
        }
    }

    private static string DescribeOrigins(ImmutableArray<RazorVueSourceOrigin> origins)
        => string.Join(
            ";",
            origins.Select(static origin =>
                $"{origin.OriginKind}|{origin.SourceFilePath}|{origin.SourceSpanStart}|{origin.SourceSpanLength}|{origin.MappingQuality}"));
}
