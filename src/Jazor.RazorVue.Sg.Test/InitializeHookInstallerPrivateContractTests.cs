using System.Reflection;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class InitializeHookInstallerPrivateContractTests
{
    [TestMethod]
    public void CatalogHelpers_UseExactArtifactIdentityAndHostParseOptions()
    {
        var defaultCompilation = CSharpCompilation.Create(
            "RazorVue.InitializeHook.Default",
            [],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var previewTree = CSharpSyntaxTree.ParseText(
            "public sealed class PreviewComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/PreviewComponent.razor.g.cs");
        var previewCompilation = defaultCompilation.AddSyntaxTrees(previewTree);
        var catalogTree = Invoke<SyntaxTree>(
            "CreateCatalogSyntaxTree",
            previewCompilation,
            "public sealed class GeneratedCatalog { }");

        Assert.AreEqual("obj/Jazor.RazorVue/Jazor.Generated.ArtifactCatalog.g.cs", catalogTree.FilePath);
        Assert.AreEqual(LanguageVersion.Preview, ((CSharpParseOptions)catalogTree.Options).LanguageVersion);
        Assert.IsFalse(Invoke<bool>("ContainsArtifactCatalog", previewCompilation));
        Assert.IsTrue(Invoke<bool>("ContainsArtifactCatalog", previewCompilation.AddSyntaxTrees(catalogTree)));

        var differentCaseCatalog = CSharpSyntaxTree.ParseText(
            "public sealed class DifferentCaseCatalog { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "obj/jazor.razorvue/Jazor.Generated.ArtifactCatalog.g.cs");
        Assert.IsFalse(Invoke<bool>(
            "ContainsArtifactCatalog",
            previewCompilation.AddSyntaxTrees(differentCaseCatalog)));

        var defaultCatalogTree = Invoke<SyntaxTree>(
            "CreateCatalogSyntaxTree",
            defaultCompilation,
            "public sealed class DefaultCatalog { }");
        Assert.AreEqual(CSharpParseOptions.Default.LanguageVersion, ((CSharpParseOptions)defaultCatalogTree.Options).LanguageVersion);
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(InitializeHookInstaller)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }
}
