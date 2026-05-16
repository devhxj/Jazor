using System.IO;
using System.Reflection;
using Jazor.Common;
using Jazor.RazorVue;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class SampleElementPlusInjectRealSourceTests
{
    [TestMethod]
    public void Sample_ElementPlusInject_RealElementAdminLayout_LowersPipelineArtifact()
    {
        var context = CreateSampleLibraryContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "ElementAdminLayout");
        var artifact = new RazorVueArtifactFactory(BuildRenderTreeTemplateFrontend.Instance)
            .Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "import VbenSidebarMenuComponent from \"./components/element-sidebar-menu.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "\"selectedKey\": props.selectedKey");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:selectedKey\": (__value) => emit(\"update:selectedKey\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"expandedKeys\": props.expandedKeys");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:expandedKeys\": (__value) => emit(\"update:expandedKeys\", __value)");
    }

    [TestMethod]
    public void Sample_ElementPlusInject_AllSnapshots_LowerIndividually()
    {
        var context = CreateSampleLibraryContext();
        var lowerer = new RazorVueArtifactFactory(BuildRenderTreeTemplateFrontend.Instance);

        foreach (var snapshot in context.CreateSemanticSnapshots().OrderBy(static item => item.ComponentSymbol.ToDisplayString(), StringComparer.Ordinal))
        {
            Console.WriteLine("lowering: " + snapshot.ComponentSymbol.ToDisplayString());
            lowerer.Lower(context, snapshot);
        }
    }

    [TestMethod]
    public void Sample_ElementPlusInject_SidebarMenu_UnionProjection_IsErasedFromLoweredArtifacts()
    {
        var context = CreateSampleLibraryContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "ElementSidebarMenu");

        var pipelineArtifact = new RazorVueArtifactFactory(BuildRenderTreeTemplateFrontend.Instance)
            .Lower(context, snapshot);
        var sfcArtifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance)
            .Lower(context, snapshot);

        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains(".AsArray", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
        Assert.IsFalse(sfcArtifact.SfcText.Contains(".AsArray", StringComparison.Ordinal), sfcArtifact.SfcText);
        StringAssert.Contains(pipelineArtifact.ModuleCode, "props.items.length > 0");
        StringAssert.Contains(sfcArtifact.SfcText, "props.items.length > 0");
        StringAssert.Contains(sfcArtifact.SfcText, "v-for=\"item in props.items\"");
    }

    [TestMethod]
    public void Sample_ElementPlusInject_SidebarMenuNode_ChildrenGuard_IsNullishSafe()
    {
        var context = CreateSampleLibraryContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "ElementSidebarMenuNode");

        var pipelineArtifact = new RazorVueArtifactFactory(BuildRenderTreeTemplateFrontend.Instance)
            .Lower(context, snapshot);
        var sfcArtifact = new RazorVueSfcArtifactFactory(BuildRenderTreeTemplateFrontend.Instance)
            .Lower(context, snapshot);

        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains(".AsArray", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
        Assert.IsFalse(sfcArtifact.SfcText.Contains(".AsArray", StringComparison.Ordinal), sfcArtifact.SfcText);
        StringAssert.Contains(sfcArtifact.SfcText, "props.item.children == null");
        StringAssert.Contains(sfcArtifact.SfcText, "props.item.children.length > 0");
        StringAssert.Contains(sfcArtifact.SfcText, "props.item.key");
        StringAssert.Contains(sfcArtifact.SfcText, "props.item.children");
        Assert.IsFalse(
            sfcArtifact.SfcText.Contains("props.item.Children", StringComparison.Ordinal),
            sfcArtifact.SfcText);
    }

    private static RazorVueCompilationContext CreateSampleLibraryContext()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var libraryRoot = Path.Combine(
            repositoryRoot,
            "samples",
            "ECMAScript.Vben.ElementPlusInject",
            "Vben.ElementPlusInject.Library");

        var sourceFiles = Directory
            .EnumerateFiles(libraryRoot, "*.cs", SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        var syntaxTrees = sourceFiles
            .Select(static path => System.IO.File.ReadAllText(path))
            .SelectMany(RazorVueMetadataReferences.CreateSyntaxTrees)
            .ToArray();

        var elementPlusAssemblyPath = Path.Combine(
            repositoryRoot,
            "src",
            "ECMAScript.ElementPlus",
            "bin",
            "Debug",
            "net11.0",
            "ECMAScript.ElementPlus.dll");
        Assert.IsTrue(System.IO.File.Exists(elementPlusAssemblyPath), $"Missing ElementPlus assembly: {elementPlusAssemblyPath}");

        var references = RazorVueMetadataReferences.Create(
            MetadataReference.CreateFromFile(elementPlusAssemblyPath));

        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            assemblyName: "ECMAScript.Vben.Sample.ElementPlusInject.Tests",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context!;
    }

    private static string ResolveRepositoryRoot()
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assert.IsNotNull(location);

        var current = new DirectoryInfo(location!);
        while (current is not null && !System.IO.File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            current = current.Parent;

        Assert.IsNotNull(current, "Repository root with Jazor.slnx was not found.");
        return current!.FullName;
    }
}
