using System.Collections.Immutable;
using System.IO;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

internal static class RazorVueRazorIrTestContextFactory
{
    public static (Jazor.RazorVue.RazorVueCompilationContext Context, RazorVueSemanticSnapshot Snapshot) CreateAlignedContext(
        string assemblyName,
        string documentPath,
        string documentText,
        string componentSource,
        string? importsText = "@using Demo.Pages")
        => CreateContextCore(
            assemblyName,
            documentPath,
            documentText,
            componentSource,
            importsText,
            requireSdkAlignedGeneratedSource: true);

    public static (Jazor.RazorVue.RazorVueCompilationContext Context, RazorVueSemanticSnapshot Snapshot) CreateContext(
        string assemblyName,
        string documentPath,
        string documentText,
        string componentSource,
        string? importsText = "@using Demo.Pages")
        => CreateContextCore(
            assemblyName,
            documentPath,
            documentText,
            componentSource,
            importsText,
            requireSdkAlignedGeneratedSource: false);

    private static (Jazor.RazorVue.RazorVueCompilationContext Context, RazorVueSemanticSnapshot Snapshot) CreateContextCore(
        string assemblyName,
        string documentPath,
        string documentText,
        string componentSource,
        string? importsText,
        bool requireSdkAlignedGeneratedSource)
    {
        var importsPath = Path.Combine(Path.GetDirectoryName(documentPath)!, "_Imports.razor");
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            assemblyName,
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
                    """,
                    options: parseOptions,
                    path: "ECMAScriptModuleAttribute.cs"),
                CSharpSyntaxTree.ParseText(
                    componentSource,
                    options: parseOptions,
                    path: "TodoApp.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, baseCompilation);
        var imports = string.IsNullOrWhiteSpace(importsText)
            ? ImmutableArray<RazorSourceDocument>.Empty
            : ImmutableArray.Create(
                RazorVueRazorCodeDocumentProvider.CreateSourceDocument(
                    new Jazor.RazorVue.RazorVueRazorDocument(importsPath, SourceText.From(importsText))));
        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))),
            RazorFileKind.Component,
            imports,
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);

        var compilation = baseCompilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText(
                csharpDocument.Text,
                options: parseOptions,
                path: Path.GetFileName(documentPath) + ".g.cs"));
        AssertCompilationHasNoErrors(compilation, csharpDocument.Text.ToString());

        var razorDocuments = string.IsNullOrWhiteSpace(importsText)
            ? Jazor.RazorVue.RazorVueRazorDocumentSet.Create(
            [
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))
            ])
            : Jazor.RazorVue.RazorVueRazorDocumentSet.Create(
            [
                new Jazor.RazorVue.RazorVueRazorDocument(importsPath, SourceText.From(importsText)),
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))
            ]);
        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation, razorDocuments);

        Assert.IsNotNull(context);
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single(static item => item.Descriptor.Name == "TodoApp");
        if (requireSdkAlignedGeneratedSource)
            AssertSdkAlignedGeneratedSource(context, snapshot);
        return (context, snapshot);
    }

    public static string CreateParentComponentSource()
        => """
        namespace Demo.Pages
        {
            [ECMAScript.ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
        }
        """;

    public static string CreateParentAndChildComponentSource()
        => """
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
        """;

    public static string GetDocumentTreeDump(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        var provider = new RazorVueRazorCodeDocumentProvider();
        Assert.IsTrue(provider.TryCreate(context, snapshot, out var handle));
        return RazorIrTestHost.DumpIntermediateNodeTree(handle.DocumentNode);
    }

    private static void AssertCompilationHasNoErrors(Compilation compilation, string? generatedCode = null)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
            0,
            errors.Length,
            string.Join(
                Environment.NewLine,
                errors.Select(static diagnostic => diagnostic.ToString()))
            + (string.IsNullOrWhiteSpace(generatedCode)
                ? string.Empty
                : Environment.NewLine + "Generated C#:" + Environment.NewLine + generatedCode));
    }

    private static void AssertSdkAlignedGeneratedSource(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        var provider = new RazorVueRazorCodeDocumentProvider();
        Assert.IsTrue(provider.TryCreate(context, snapshot, out var handle));
        Assert.IsNotNull(snapshot.BuildRenderTreeMethod);

        var syntax = snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Single();
        var compiledText = syntax.SyntaxTree.GetText().ToString();
        var razorText = handle.CSharpDocument.Text.ToString();

        Assert.AreEqual(
            compiledText,
            razorText,
            "Compiled Razor output diverged from provider RazorCodeDocument output."
            + Environment.NewLine
            + "Compiled C#:"
            + Environment.NewLine
            + compiledText
            + Environment.NewLine
            + "Provider C#:"
            + Environment.NewLine
            + razorText);
    }
}
