namespace Jazor.ComplierTest;

[TestClass]
public sealed class CompilerRazorVueBoundaryTests
{
    [TestMethod]
    public void ProductionCompilerSource_DoesNotContainRazorVueProductLowering()
    {
        var repositoryRoot = FindRepositoryRoot();
        var compilerDirectory = Path.Combine(repositoryRoot, "src", "Jazor.Compiler");
        var forbiddenProductTokens = new[]
        {
            "AstConverterProfile.RazorVueRuntime",
            "CurrentComponentMemberClosure",
            "CurrentComponentSemanticWalkerHost",
            "CurrentComponentStateDefaultInitializer",
            "RenderTreeBuilderSemanticWalkerHost",
            "ChildrenToSlotIntrinsic",
            "@jazor/vue-runtime",
            "using Microsoft.AspNetCore.Components",
            "Jazor.RazorVue"
        };

        var violations = Directory.EnumerateFiles(compilerDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.EndsWith("WhiteList.cs.Generate.cs", StringComparison.Ordinal))
            .SelectMany(path =>
            {
                var source = File.ReadAllText(path);
                return forbiddenProductTokens
                    .Where(source.Contains)
                    .Select(token => Path.GetRelativePath(repositoryRoot, path) + ": " + token);
            })
            .ToArray();

        Assert.AreEqual(0, violations.Length, string.Join(Environment.NewLine, violations));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }
}
