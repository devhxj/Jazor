namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ProductionRazorCompilerReferenceTests
{
    [TestMethod]
    public void RazorVueProductionProjects_DoNotReferenceRazorCompiler()
    {
        var root = FindRepositoryRoot();
        var productionProjectPaths = new[]
        {
            "src/Jazor.Analyzer/Jazor.Analyzer.csproj",
            "src/Jazor.RazorVue/Jazor.RazorVue.csproj",
            "src/Jazor/Jazor.csproj"
        };

        foreach (var relativePath in productionProjectPaths)
        {
            var projectPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(projectPath), "Expected production project was not found: " + relativePath);

            var projectText = File.ReadAllText(projectPath);
            Assert.IsFalse(
                projectText.Contains("Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal),
                relativePath + " must not reference or package Microsoft.CodeAnalysis.Razor.Compiler.");
            Assert.IsFalse(
                projectText.Contains("Microsoft.AspNetCore.Razor.Utilities.Shared", StringComparison.Ordinal),
                relativePath + " must not reference or package Microsoft.AspNetCore.Razor.Utilities.Shared.");
        }
    }

    [TestMethod]
    public void DeprecatedRazorExtensionProject_IsNotPresent()
    {
        var root = FindRepositoryRoot();
        var deprecatedProjectPath = Path.Combine(
            root,
            "src",
            "Jazor.RazorVue.RazorExtension",
            "Jazor.RazorVue.RazorExtension.csproj");

        Assert.IsFalse(
            File.Exists(deprecatedProjectPath),
            "The deprecated RazorExtension project must not remain in the RazorVue production source tree.");
    }

    [TestMethod]
    public void RazorVueProductionAssemblies_DoNotReferenceRazorCompiler()
    {
        var productionAssemblies = new[]
        {
            typeof(Jazor.RazorVue.RazorSdk.RazorSgGeneratedCSharpBinder).Assembly,
            typeof(Jazor.RazorVue.Analysis.RazorVueGenerator).Assembly
        };

        foreach (var assembly in productionAssemblies)
        {
            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
                .Select(static item => item.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(
                referencedAssemblyNames,
                "Microsoft.CodeAnalysis.Razor.Compiler",
                assembly.GetName().Name + " must not reference Microsoft.CodeAnalysis.Razor.Compiler.");
            CollectionAssert.DoesNotContain(
                referencedAssemblyNames,
                "Microsoft.AspNetCore.Razor.Utilities.Shared",
                assembly.GetName().Name + " must not reference Microsoft.AspNetCore.Razor.Utilities.Shared.");
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Jazor.slnx")))
                return current;

            var parent = Directory.GetParent(current);
            if (parent is null)
                break;

            current = parent.FullName;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx could not be located.");
    }
}
