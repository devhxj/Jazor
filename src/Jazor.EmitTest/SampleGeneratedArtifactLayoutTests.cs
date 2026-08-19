using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SampleGeneratedArtifactLayoutTests
{
    private static readonly string[] SampleManifestRelativePaths =
    [
        Path.Combine("samples", "ECMAScript.Pinia.Counter", "Pinia.Counter.Host", "jazor", "jazor-manifest.json"),
        Path.Combine("samples", "ECMAScript.VueRoute.MemorySmoke", "VueRoute.MemorySmoke.Host", "jazor", "jazor-manifest.json"),
        Path.Combine("samples", "Jazor.MultiProject", "Sample.Host", "jazor", "jazor-manifest.json")
    ];

    [TestMethod]
    public void CheckedInSampleManifests_UsePortableRootIdentityAndExistingArtifacts()
    {
        var repositoryRoot = FindRepositoryRoot();

        foreach (var relativePath in SampleManifestRelativePaths)
        {
            var manifestPath = Path.Combine(repositoryRoot, relativePath);
            Assert.IsTrue(File.Exists(manifestPath), "Sample manifest is missing: " + relativePath);

            var manifest = LoadManifest(manifestPath);
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(manifest.RootAssemblyName),
                "Sample manifest RootAssemblyName is empty: " + relativePath);
            Assert.IsTrue(
                string.IsNullOrWhiteSpace(manifest.RootAssemblyPath),
                "Sample manifest must not embed a developer-specific RootAssemblyPath: " + relativePath);

            AssertManifestArtifacts(manifest, manifestPath, relativePath);
        }
    }

    private static void AssertManifestArtifacts(
        ManifestModel manifest,
        string manifestPath,
        string manifestRelativePath)
    {
        Assert.IsTrue(
            manifest.Modules.Count > 0,
            "Sample manifest Modules array is missing: " + manifestRelativePath);

        foreach (var module in manifest.Modules)
        {
            var moduleDescription = GetModuleDescription(module, manifestRelativePath);
            var modulePath = ResolveManifestRelativePath(manifestPath, module.RelativePath, moduleDescription);
            Assert.IsTrue(File.Exists(modulePath), "Manifest module file is missing: " + moduleDescription);

            if (!string.IsNullOrWhiteSpace(module.SourceMapPath))
            {
                var sourceMapPath = ResolveManifestRelativePath(manifestPath, module.SourceMapPath, moduleDescription);
                Assert.IsTrue(File.Exists(sourceMapPath), "Manifest source map file is missing: " + moduleDescription);
            }

        }
    }

    private static string GetModuleDescription(ModuleEntry module, string manifestRelativePath)
        => manifestRelativePath + " :: " + (!string.IsNullOrWhiteSpace(module.Id) ? module.Id : module.RelativePath);

    private static string ResolveManifestRelativePath(
        string manifestPath,
        string relativePath,
        string description)
    {
        Assert.IsFalse(Path.IsPathRooted(relativePath), "Manifest artifact path must be relative: " + description);
        Assert.IsFalse(relativePath.Contains('\\'), "Manifest artifact path must use URL-style '/' separators: " + description);

        var manifestDirectory = Path.GetDirectoryName(manifestPath)
            ?? throw new InvalidOperationException("Could not resolve manifest directory for " + manifestPath);
        var resolvedPath = Path.GetFullPath(Path.Combine(manifestDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootedManifestDirectory = Path.GetFullPath(manifestDirectory);
        if (!rootedManifestDirectory.EndsWith(Path.DirectorySeparatorChar))
            rootedManifestDirectory += Path.DirectorySeparatorChar;

        Assert.IsTrue(
            resolvedPath.StartsWith(rootedManifestDirectory, StringComparison.OrdinalIgnoreCase),
            "Manifest artifact path escapes its output directory: " + description);

        return resolvedPath;
    }

    private static ManifestModel LoadManifest(string manifestPath)
        => ManifestModel.TryLoad(manifestPath)
            ?? throw new FileNotFoundException("Sample manifest was not found: " + manifestPath, manifestPath);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from " + AppContext.BaseDirectory);
    }
}
