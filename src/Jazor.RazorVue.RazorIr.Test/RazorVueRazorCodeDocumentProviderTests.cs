using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorCodeDocumentProviderTests
{
    [TestMethod]
    public void TryCreate_ForRazorSnapshot_ReturnsCodeDocumentAndDocumentNode()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string importsText = "@using Demo.Shared";
        const string documentText = """
            @page "/todo"
            <section><h1>@Title</h1><p>Hello</p></section>
            """;

        var context = CreateContext(
            CreateRazorGeneratedCompilation(documentPath, importsPath),
            importsPath,
            importsText,
            documentPath,
            documentText);
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();

        var provider = new RazorVueRazorCodeDocumentProvider();
        var created = provider.TryCreate(context, snapshot, out var handle);

        Assert.IsTrue(created);
        Assert.AreEqual(documentPath, handle.PrimaryDocument.Path);
        Assert.AreEqual(1, handle.ImportDocuments.Length);
        Assert.AreEqual(importsPath, handle.ImportDocuments[0].Path);
        Assert.AreEqual("DocumentIntermediateNode", handle.DocumentNode.GetType().Name);
    }

    [TestMethod]
    public void TryCreate_ForRazorSnapshot_DiscoversCurrentCompilationComponentTagHelpers()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string importsText = "@using Demo.Shared";
        const string documentText = """
            <ChildCard Title="@Title">
                <p>Body</p>
            </ChildCard>
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorSdk.Provider.TagHelpers.Tests",
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
                        [ECMAScript.ECMAScriptModule("./components/child-card")]
                        public partial class ChildCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }

                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
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
                                __builder.OpenComponent<ChildCard>(0);
                                __builder.AddAttribute(1, "Title", Title);
                                __builder.CloseComponent();
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var context = CreateContext(
            compilation,
            importsPath,
            importsText,
            documentPath,
            documentText);
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single(static item => item.Descriptor.Name == "TodoApp");

        var provider = new RazorVueRazorCodeDocumentProvider();
        var created = provider.TryCreate(context, snapshot, out var handle);

        Assert.IsTrue(created);
        Assert.IsTrue(
            handle.TagHelpers.Any(static descriptor => descriptor.DisplayName.Contains("Demo.Pages.ChildCard", StringComparison.Ordinal)),
            "The production RazorVue Razor SDK host did not discover the current-compilation component descriptor.");
    }

    [TestMethod]
    public void TryCreate_ForPlainCSharpSnapshotWithoutRazorDocument_ReturnsFalse()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorSdk.Provider.NoRazorDoc.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
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
                        [ECMAScript.ECMAScriptModule("./components/plain-card")]
                        public class PlainCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    path: "PlainCard.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation, Jazor.RazorVue.RazorVueRazorDocumentSet.Empty);
        Assert.IsNotNull(context);
        var snapshot = context.CreateSemanticSnapshots().Single();

        var provider = new RazorVueRazorCodeDocumentProvider();
        var created = provider.TryCreate(context, snapshot, out _);

        Assert.IsFalse(created);
    }

    private static Jazor.RazorVue.RazorVueCompilationContext CreateContext(
        Compilation compilation,
        string importsPath,
        string importsText,
        string documentPath,
        string documentText)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(
            compilation,
            Jazor.RazorVue.RazorVueRazorDocumentSet.Create(
            [
                new Jazor.RazorVue.RazorVueRazorDocument(importsPath, SourceText.From(importsText)),
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))
            ]));
        Assert.IsNotNull(context);
        return context;
    }

    private static CSharpCompilation CreateRazorGeneratedCompilation(string documentPath, string importsPath)
        => CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorSdk.Provider.Tests",
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
                            [Parameter]
                            public string? Title { get; set; }
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
                                __builder.OpenElement(1, "h1");
                                __builder.AddContent(2, Title);
                                __builder.CloseElement();
                                __builder.OpenElement(3, "p");
                                __builder.AddContent(4, "Hello");
                                __builder.CloseElement();
                                __builder.CloseElement();
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
