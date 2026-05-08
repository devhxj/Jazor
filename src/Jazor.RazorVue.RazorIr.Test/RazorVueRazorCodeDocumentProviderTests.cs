using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text.Json;

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

        var compilation = CreateCarrierBackedCompilation(documentPath, importsPath, documentText, importsText);
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
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

        var compilation = CreateCarrierBackedCompilation(
            documentPath,
            importsPath,
            documentText,
            importsText,
            componentSource:
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
            generatedBuildRenderTreeSource:
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
            """);

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single(static item => item.Descriptor.Name == "TodoApp");
        Assert.IsNotNull(snapshot.RazorIrCarrier);

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
                    global using static ECMAScript.Vue3;
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

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        var snapshot = context.CreateSemanticSnapshots().Single();

        var provider = new RazorVueRazorCodeDocumentProvider();
        var created = provider.TryCreate(context, snapshot, out _);

        Assert.IsFalse(created);
    }

    [TestMethod]
    public void TryCreate_ForCarrierBackedRazorSnapshot_DoesNotRequireAdditionalRazorDocuments()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string importsText = "@using Demo.Shared";
        const string documentText = """
            @page "/todo"
            <section><h1>@Title</h1><p>Hello</p></section>
            """;

        var compilation = CreateCarrierBackedCompilation(documentPath, importsPath, documentText, importsText);
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();
        Assert.IsNotNull(snapshot.RazorIrCarrier);

        var provider = new RazorVueRazorCodeDocumentProvider();
        var created = provider.TryCreate(context, snapshot, out var handle);

        Assert.IsTrue(created);
        Assert.AreEqual(documentPath, handle.PrimaryDocument.Path);
        Assert.AreEqual(documentText, handle.PrimaryDocument.Text.ToString());
        Assert.AreEqual(1, handle.ImportDocuments.Length);
        Assert.AreEqual(importsPath, handle.ImportDocuments[0].Path);
        Assert.AreEqual(importsText, handle.ImportDocuments[0].Text.ToString());
    }

    private static CSharpCompilation CreateCarrierBackedCompilation(
        string documentPath,
        string importsPath,
        string documentText,
        string importsText,
        string? componentSource = null,
        string? generatedBuildRenderTreeSource = null)
    {
        var importsJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                Path = importsPath,
                Text = importsText
            }
        });
        componentSource ??= string.Join(
            Environment.NewLine,
            "namespace Demo.Pages",
            "{",
            "    [ECMAScript.ECMAScriptModule(\"./components/todo-app\")]",
            "    [Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute(",
            "        " + ToVerbatimLiteral(documentPath) + ",",
            "        " + ToVerbatimLiteral(importsJson) + ",",
            "        " + ToVerbatimLiteral(documentText) + ")]",
            "    public partial class TodoApp : ComponentBase, IVueComponent",
            "    {",
            "        [Parameter]",
            "        public string? Title { get; set; }",
            "    }",
            "}");
        componentSource = InjectCarrierAttribute(componentSource, documentPath, importsJson, documentText);
        generatedBuildRenderTreeSource ??=
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
            """;

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorSdk.Provider.Carrier.Tests",
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
                    """,
                    path: "ECMAScriptModuleAttribute.cs"),
                CSharpSyntaxTree.ParseText(
                    componentSource,
                    path: "TodoApp.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    generatedBuildRenderTreeSource,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static string ToVerbatimLiteral(string text)
        => "@\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string InjectCarrierAttribute(
        string source,
        string documentPath,
        string importsJson,
        string documentText)
    {
        if (source.Contains("RazorVueRazorIrCarrierAttribute", StringComparison.Ordinal))
            return source;

        const string marker = "[ECMAScript.ECMAScriptModule(\"./components/todo-app\")]";
        var carrierAttribute = string.Join(
            Environment.NewLine,
            marker,
            "    [Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute(",
            "        " + ToVerbatimLiteral(documentPath) + ",",
            "        " + ToVerbatimLiteral(importsJson) + ",",
            "        " + ToVerbatimLiteral(documentText) + ")]");

        return source.Contains(marker, StringComparison.Ordinal)
            ? source.Replace(marker, carrierAttribute, StringComparison.Ordinal)
            : source;
    }
}
