using Jazor.VueHost.LanguageServers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostLanguageServerCatalogTests
{
    [TestMethod]
    public void LanguageServerCatalog_CreateDefault_PicksUpRoslynRazorAndDesignTimeTargetsFromCSharpExtension()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var roslynPath = Path.Combine(root, "Microsoft.CodeAnalysis.LanguageServer.exe");
            var razorSourceGeneratorPath = Path.Combine(root, "Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler.dll");
            var razorExtensionPath = Path.Combine(root, "Microsoft.VisualStudioCode.RazorExtension.dll");
            var razorDesignTimePath = Path.Combine(root, "Microsoft.NET.Sdk.Razor.DesignTime.targets");
            var csharpDesignTimePath = Path.Combine(root, "Microsoft.CSharpExtension.DesignTime.targets");
            File.WriteAllText(roslynPath, string.Empty);
            File.WriteAllText(razorSourceGeneratorPath, string.Empty);
            File.WriteAllText(razorExtensionPath, string.Empty);
            File.WriteAllText(razorDesignTimePath, string.Empty);
            File.WriteAllText(csharpDesignTimePath, string.Empty);

            var originalRoslyn = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_SERVER");
            var originalExtension = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_EXTENSION");
            var originalSourceGenerator = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_SOURCE_GENERATOR");
            var originalRazorTargets = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_DESIGN_TIME");
            var originalCSharpTargets = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_CSHARP_DESIGN_TIME");
            try
            {
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_SERVER", roslynPath);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_EXTENSION", razorExtensionPath);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_SOURCE_GENERATOR", razorSourceGeneratorPath);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_DESIGN_TIME", razorDesignTimePath);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_CSHARP_DESIGN_TIME", csharpDesignTimePath);
                var catalog = LanguageServerCatalog.CreateDefault();

                Assert.IsNotNull(catalog.Roslyn);
                CollectionAssert.Contains(catalog.Roslyn.Arguments, "--extension");
                CollectionAssert.Contains(catalog.Roslyn.Arguments, "--razorSourceGenerator");
                CollectionAssert.Contains(catalog.Roslyn.Arguments, "--razorDesignTimePath");
                CollectionAssert.Contains(catalog.Roslyn.Arguments, "--csharpDesignTimePath");
                StringAssert.EndsWith(catalog.RoslynExtensionAssemblyPath ?? string.Empty, "Microsoft.VisualStudioCode.RazorExtension.dll");
                StringAssert.EndsWith(catalog.RazorSourceGeneratorPath ?? string.Empty, "Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler.dll");
                StringAssert.EndsWith(catalog.RazorDesignTimePath ?? string.Empty, "Microsoft.NET.Sdk.Razor.DesignTime.targets");
                StringAssert.EndsWith(catalog.CSharpDesignTimePath ?? string.Empty, "Microsoft.CSharpExtension.DesignTime.targets");
            }
            finally
            {
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_SERVER", originalRoslyn);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_ROSLYN_EXTENSION", originalExtension);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_SOURCE_GENERATOR", originalSourceGenerator);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_RAZOR_DESIGN_TIME", originalRazorTargets);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_CSHARP_DESIGN_TIME", originalCSharpTargets);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void LanguageServerCatalog_CreateDefault_PicksUpWorkspaceVolarServer()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var entryPoint = Path.Combine(root, "node_modules", "@vue", "language-server", "bin", "vue-language-server.js");
            var tsServerPath = Path.Combine(root, "node_modules", "typescript", "lib", "tsserver.js");
            Directory.CreateDirectory(Path.GetDirectoryName(entryPoint)!);
            Directory.CreateDirectory(Path.GetDirectoryName(tsServerPath)!);
            File.WriteAllText(entryPoint, string.Empty);
            File.WriteAllText(tsServerPath, string.Empty);

            var originalDirectory = Directory.GetCurrentDirectory();
            var originalTsServer = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_TSSERVER");
            try
            {
                Directory.SetCurrentDirectory(root);
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_TSSERVER", tsServerPath);
                var catalog = LanguageServerCatalog.CreateDefault();

                Assert.IsNotNull(catalog.Volar);
                Assert.AreEqual("node", catalog.Volar.FileName);
                CollectionAssert.AreEqual(
                    new[] { entryPoint, "--stdio", $"--tsdk={Path.GetDirectoryName(tsServerPath)}" },
                    catalog.Volar.Arguments);
            }
            finally
            {
                Environment.SetEnvironmentVariable("JAZOR_VUEHOST_TSSERVER", originalTsServer);
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "jazor-vuehost-language-server-catalog-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
