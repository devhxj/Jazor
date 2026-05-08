using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueReflectedRazorIrReaderTests
{
    [TestMethod]
    public void TryCreateDocument_ForOfficialRazorCodeDocument_ProjectsNeutralIr()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string importsText = "@using Demo.Shared";
        const string documentText = """
            @page "/todo"
            <section><h1>@Title</h1><p>Hello</p></section>
            """;

        var (codeDocument, csharpDocument, _) = CreateOfficialDocument(
            documentPath,
            importsPath,
            documentText,
            importsText);

        var created = RazorVueReflectedRazorIrReader.TryCreateDocument(
            "TodoApp_razor.g.cs",
            codeDocument,
            csharpDocument,
            out var document,
            out var failure);

        Assert.IsTrue(created, failure);
        Assert.AreEqual(documentPath, document.PrimaryDocument.Path);
        Assert.AreEqual(1, document.ImportDocuments.Length);
        Assert.AreEqual(importsPath, document.ImportDocuments[0].Path);
        Assert.AreEqual(RazorVueRazorIrNodeKind.Document, document.DocumentNode.Kind);
        Assert.IsTrue(
            Enumerate(document.DocumentNode).Any(static node => node.Kind == RazorVueRazorIrNodeKind.MarkupElement && node.TagName == "section"),
            "Projected neutral IR did not include the expected section markup element.");
    }

    [TestMethod]
    public void TryCreateDocument_ForOfficialRazorCodeDocument_ProjectsSourceMappings()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<h1>@Title</h1>""";

        var (codeDocument, csharpDocument, _) = CreateOfficialDocument(
            documentPath,
            importsPath,
            documentText,
            importsText: "@using Demo.Shared");

        var created = RazorVueReflectedRazorIrReader.TryCreateDocument(
            "TodoApp_razor.g.cs",
            codeDocument,
            csharpDocument,
            out var document,
            out var failure);

        Assert.IsTrue(created, failure);
        Assert.IsTrue(document.SourceMappings.Length > 0, "No Razor source mappings were projected from RazorCSharpDocument.");
        Assert.IsTrue(
            document.SourceMappings.Any(mapping => string.Equals(mapping.OriginalSpan.FilePath, documentPath, StringComparison.OrdinalIgnoreCase)),
            "Projected source mappings did not preserve the primary Razor document path.");
    }

    [TestMethod]
    public void TryCreateDocument_ForStaticMarkupWithoutSourceMappings_StillProjectsIr()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<h1>Hello</h1>""";

        var (codeDocument, csharpDocument, _) = CreateOfficialDocument(
            documentPath,
            importsPath,
            documentText,
            importsText: "@using Demo.Shared");

        var created = RazorVueReflectedRazorIrReader.TryCreateDocument(
            "TodoApp_razor.g.cs",
            codeDocument,
            csharpDocument,
            out var document,
            out var failure);

        Assert.IsTrue(created, failure);
        Assert.AreEqual(0, document.SourceMappings.Length);
        Assert.IsTrue(
            Enumerate(document.DocumentNode)
                .Any(static node => string.Equals(node.Content, "<h1>Hello</h1>", StringComparison.Ordinal)),
            "Static markup without C# source mappings must still project neutral Razor IR content.");
    }

    [TestMethod]
    public void TryCreateDocument_ForInvalidHostOutput_ReturnsFailure()
    {
        var created = RazorVueReflectedRazorIrReader.TryCreateDocument(
            "invalid",
            new object(),
            new object(),
            out _,
            out var failure);

        Assert.IsFalse(created);
        StringAssert.Contains(failure, "RazorCodeDocument");
    }

    private static (RazorCodeDocument CodeDocument, RazorCSharpDocument CSharpDocument, Compilation Compilation) CreateOfficialDocument(
        string documentPath,
        string importsPath,
        string documentText,
        string importsText)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.ReflectedReader.Tests",
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
                    path: "TodoApp.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, compilation);
        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))),
            RazorFileKind.Component,
            ImmutableArray.Create(
                RazorVueRazorCodeDocumentProvider.CreateSourceDocument(new Jazor.RazorVue.RazorVueRazorDocument(importsPath, SourceText.From(importsText)))),
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);
        return (codeDocument, csharpDocument, compilation);
    }

    private static IEnumerable<RazorVueRazorIrNode> Enumerate(RazorVueRazorIrNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var nested in Enumerate(child))
                yield return nested;
        }
    }

}
