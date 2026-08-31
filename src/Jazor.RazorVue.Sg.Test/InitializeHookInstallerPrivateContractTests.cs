using System.Reflection;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class InitializeHookInstallerPrivateContractTests
{
    [TestMethod]
    public void CatalogHelpers_UseExactModuleCatalogIdentityAndHostParseOptions()
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
            "public sealed class GeneratedCatalog { }",
            false);

        Assert.AreEqual("obj/Jazor.RazorVue/Jazor.Generated.ModuleCatalog.g.cs", catalogTree.FilePath);
        Assert.AreEqual(LanguageVersion.Preview, ((CSharpParseOptions)catalogTree.Options).LanguageVersion);
        Assert.IsFalse(Invoke<bool>("ContainsModuleCatalog", previewCompilation));
        Assert.IsTrue(Invoke<bool>("ContainsModuleCatalog", previewCompilation.AddSyntaxTrees(catalogTree)));

        var differentCaseCatalog = CSharpSyntaxTree.ParseText(
            "public sealed class DifferentCaseCatalog { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "obj/jazor.razorvue/Jazor.Generated.ModuleCatalog.g.cs");
        Assert.IsFalse(Invoke<bool>(
            "ContainsModuleCatalog",
            previewCompilation.AddSyntaxTrees(differentCaseCatalog)));

        var compilerCatalogTree = CSharpSyntaxTree.ParseText(
            "namespace Jazor.Generated { internal static partial class ModuleCatalog { } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Jazor.Compiler/Jazor.Compiler.ESGenerator/Jazor.Generated.ModuleCatalog.g.cs");
        Assert.IsTrue(Invoke<bool>(
            "ContainsModuleCatalog",
            previewCompilation.AddSyntaxTrees(compilerCatalogTree)));

        var defaultCatalogTree = Invoke<SyntaxTree>(
            "CreateCatalogSyntaxTree",
            defaultCompilation,
            "public sealed class DefaultCatalog { }",
            false);
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
