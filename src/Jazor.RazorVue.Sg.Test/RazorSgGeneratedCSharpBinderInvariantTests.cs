using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgGeneratedCSharpBinderInvariantTests
{
    [TestMethod]
    public void TryBind_DerivesOnceForMixedTreePresence_AndResolvesSharedGeneratedHelper()
    {
        var fixture = CreateTwoComponentFixture();
        var hookCompilation = fixture.BaseCompilation.AddSyntaxTrees(fixture.CounterTree);
        var batch = new RazorSgTailBatch(
            hookCompilation,
            ImmutableArray.Create(fixture.CounterDocument, fixture.ChildDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out var result, out var failure);

        Assert.IsTrue(bound, failure);
        Assert.IsNotNull(result);
        Assert.AreEqual(RazorSgCompilationBindingMode.DerivedHookCompilation, result.BindingMode);
        Assert.AreEqual(1, result.ReusedGeneratedTreeCount);
        Assert.AreEqual(1, result.DerivedGeneratedTreeCount);
        Assert.AreEqual(2, result.Components.Length);
        Assert.AreEqual(
            1,
            result.Compilation.SyntaxTrees.Count(tree => string.Equals(tree.FilePath, fixture.CounterDocument.HintName, StringComparison.Ordinal)));
        Assert.AreEqual(
            1,
            result.Compilation.SyntaxTrees.Count(tree => string.Equals(tree.FilePath, fixture.ChildDocument.HintName, StringComparison.Ordinal)));
        Assert.AreEqual(
            0,
            result.Compilation.GetDiagnostics().Count(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [TestMethod]
    public void TryBind_IgnoresNonComponentGeneratedDocument_WhileBindingComponentDocuments()
    {
        var baseCompilation = CreateCompilation(
            """
            namespace Demo.Pages;

            public partial class Counter : global::Microsoft.AspNetCore.Components.ComponentBase
            {
            }
            """);
        var componentDocument = CreateDocument(
            "Counter.razor.g.cs",
            "Pages/Counter.razor",
            """
            namespace Demo.Pages;

            public partial class Counter
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "counter");
                }
            }
            """);
        var importsDocument = CreateDocument(
            "_Imports_razor.g.cs",
            "_Imports.razor",
            """
            namespace Demo;

            // Razor imports emit an Execute host, not a render component.
            public partial class _Imports : object
            {
                protected void Execute()
                {
                }
            }
            """);
        var batch = new RazorSgTailBatch(
            baseCompilation,
            ImmutableArray.Create(componentDocument, importsDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out var result, out var failure);

        Assert.IsTrue(bound, failure);
        Assert.IsNotNull(result);
        Assert.AreEqual(RazorSgCompilationBindingMode.DerivedHookCompilation, result.BindingMode);
        Assert.AreEqual(2, result.Documents.Length);
        Assert.AreEqual(1, result.Components.Length);
        Assert.AreEqual("Counter", result.Components[0].ComponentSymbol.Name);
        Assert.AreEqual(importsDocument.HintName, result.Documents.Single(document => document.HintName == importsDocument.HintName).HintName);
    }

    [TestMethod]
    public void TryBind_RejectsStaleCurrentGeneratedTree()
    {
        var fixture = CreateTwoComponentFixture();
        var staleTree = Parse(
            """
            namespace Demo.Pages;

            public partial class Counter
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "stale");
                }
            }
            """,
            fixture.CounterDocument.HintName);
        var hookCompilation = fixture.BaseCompilation.AddSyntaxTrees(staleTree);
        var batch = new RazorSgTailBatch(
            hookCompilation,
            ImmutableArray.Create(fixture.CounterDocument, fixture.ChildDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out _, out var failure);

        Assert.IsFalse(bound);
        StringAssert.Contains(failure ?? string.Empty, "stale or conflicting tree");
    }

    [TestMethod]
    public void TryBind_RejectsDuplicateHintNamesBeforeReconcilingTrees()
    {
        var fixture = CreateTwoComponentFixture();
        var duplicateHintDocument = new RazorSgGeneratedDocument(
            fixture.CounterDocument.HintName,
            "Pages/Alternate.razor",
            SourceText.From(fixture.ChildDocument.GeneratedCSharp.ToString(), Encoding.UTF8),
            ImmutableArray<RazorSgSourceMapping>.Empty);
        var batch = new RazorSgTailBatch(
            fixture.BaseCompilation,
            ImmutableArray.Create(fixture.CounterDocument, duplicateHintDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out _, out var failure);

        Assert.IsFalse(bound);
        StringAssert.Contains(failure ?? string.Empty, "duplicate hint name");
    }

    [TestMethod]
    public void TryBind_DoesNotClaimHandwrittenBuildRenderTreeOutsideGeneratedDocument()
    {
        const string hintName = "Counter.razor.g.cs";
        var baseCompilation = CreateCompilation(
            """
            namespace Demo.Pages;

            public partial class Counter : global::Microsoft.AspNetCore.Components.ComponentBase
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "handwritten");
                }
            }
            """);
        var generatedDocument = CreateDocument(
            hintName,
            "Pages/Counter.razor",
            """
            namespace Demo.Pages;

            public partial class Counter
            {
            }
            """);
        var batch = new RazorSgTailBatch(baseCompilation, ImmutableArray.Create(generatedDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out _, out var failure);

        Assert.IsFalse(bound);
        StringAssert.Contains(failure ?? string.Empty, "did not declare BuildRenderTree");
    }

    [TestMethod]
    public void TryBind_BindsGenericGeneratedComponent()
    {
        const string hintName = "GenericCounter.razor.g.cs";
        var baseCompilation = CreateCompilation(
            """
            namespace Demo.Pages;

            public partial class GenericCounter<TItem> : global::Microsoft.AspNetCore.Components.ComponentBase
            {
            }
            """);
        var generatedDocument = CreateDocument(
            hintName,
            "Pages/GenericCounter.razor",
            """
            namespace Demo.Pages;

            public partial class GenericCounter<TItem>
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, default(TItem));
                }
            }
            """);
        var batch = new RazorSgTailBatch(baseCompilation, ImmutableArray.Create(generatedDocument));

        var bound = RazorSgGeneratedCSharpBinder.TryBind(batch, out var result, out var failure);

        Assert.IsTrue(bound, failure);
        Assert.IsNotNull(result);
        Assert.AreEqual(RazorSgCompilationBindingMode.DerivedHookCompilation, result.BindingMode);
        Assert.AreEqual(1, result.Components.Length);
        Assert.AreEqual("GenericCounter", result.Components[0].ComponentSymbol.Name);
        Assert.AreEqual(1, result.Components[0].ComponentSymbol.Arity);
    }

    private static TwoComponentFixture CreateTwoComponentFixture()
    {
        var baseCompilation = CreateCompilation(
            """
            namespace Demo.Pages;

            public partial class Counter : global::Microsoft.AspNetCore.Components.ComponentBase
            {
            }

            public partial class Child : global::Microsoft.AspNetCore.Components.ComponentBase
            {
            }
            """);
        var counterDocument = CreateDocument(
            "Counter.razor.g.cs",
            "Pages/Counter.razor",
            """
            namespace Demo.Pages;

            internal static class SharedGeneratedHelper
            {
                internal const string Text = "from generated helper";
            }

            public partial class Counter
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, SharedGeneratedHelper.Text);
                }
            }
            """);
        var childDocument = CreateDocument(
            "Child.razor.g.cs",
            "Pages/Child.razor",
            """
            namespace Demo.Pages;

            public partial class Child
            {
                protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, SharedGeneratedHelper.Text);
                }
            }
            """);
        return new TwoComponentFixture(
            baseCompilation,
            Parse(counterDocument.GeneratedCSharp.ToString(), counterDocument.HintName),
            counterDocument,
            childDocument);
    }

    private static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create(
            assemblyName: "RazorSg.GeneratedCSharpBinder.Invariant.Tests",
            syntaxTrees: [Parse(source, "Components.razor.cs")],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static RazorSgGeneratedDocument CreateDocument(string hintName, string sourcePath, string source)
        => new(
            hintName,
            sourcePath,
            SourceText.From(source, Encoding.UTF8),
            ImmutableArray<RazorSgSourceMapping>.Empty);

    private static SyntaxTree Parse(string source, string path)
        => CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview),
            path);

    private sealed record TwoComponentFixture(
        CSharpCompilation BaseCompilation,
        SyntaxTree CounterTree,
        RazorSgGeneratedDocument CounterDocument,
        RazorSgGeneratedDocument ChildDocument);
}
