using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SampleGeneratedArtifactLayoutTests
{
    private static readonly string[] SampleManifestRelativePaths =
    [
        Path.Combine("samples", "ECMAScript.Pinia.Counter", "Pinia.Counter.Host", "wwwroot", "jazor", "jazor-manifest.json"),
        Path.Combine("samples", "ECMAScript.VueRoute.MemorySmoke", "VueRoute.MemorySmoke.Host", "wwwroot", "jazor", "jazor-manifest.json"),
        Path.Combine("samples", "Jazor.MultiProject", "Sample.Host", "wwwroot", "jazor", "jazor-manifest.json"),
        Path.Combine("samples", "RazorVue.TodoList", "Todo.Host", "wwwroot", "jazor", "jazor-manifest.json")
    ];

    private static readonly string[] RazorVueManifestRelativePaths =
    [
        Path.Combine("samples", "RazorVue.TodoList", "Todo.Host", "wwwroot", "jazor", "jazor-manifest-razorvue.json")
    ];

    [TestMethod]
    public void CheckedInSampleManifests_TargetCurrentNet11ToolchainAndExistingArtifacts()
    {
        var repositoryRoot = FindRepositoryRoot();

        foreach (var relativePath in SampleManifestRelativePaths)
        {
            var manifestPath = Path.Combine(repositoryRoot, relativePath);
            Assert.IsTrue(File.Exists(manifestPath), "Sample manifest is missing: " + relativePath);

            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var rootAssemblyPath = manifest.RootElement.GetProperty("RootAssemblyPath").GetString();

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(rootAssemblyPath),
                "Sample manifest RootAssemblyPath is empty: " + relativePath);

            var rootAssemblySegments = rootAssemblyPath!
                .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.Contains(
                rootAssemblySegments,
                "bin",
                "Sample manifest RootAssemblyPath should point at a built assembly output: " + relativePath);
            CollectionAssert.Contains(
                rootAssemblySegments,
                "net11.0",
                "Sample manifest RootAssemblyPath should point at the net11.0 output produced by the current toolchain: " + relativePath);
            Assert.IsFalse(
                rootAssemblyPath.Contains("net10.0", StringComparison.OrdinalIgnoreCase),
                "Sample manifest RootAssemblyPath must not point at stale net10.0 output: " + relativePath);

            AssertStandardManifestArtifacts(manifest.RootElement, manifestPath, relativePath);
        }

        foreach (var relativePath in RazorVueManifestRelativePaths)
        {
            var manifestPath = Path.Combine(repositoryRoot, relativePath);
            Assert.IsTrue(File.Exists(manifestPath), "RazorVue sample manifest is missing: " + relativePath);

            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            AssertRazorVueManifestArtifacts(manifest.RootElement, manifestPath, relativePath);
        }
    }

    private static void AssertStandardManifestArtifacts(
        JsonElement manifest,
        string manifestPath,
        string manifestRelativePath)
    {
        Assert.IsTrue(
            manifest.TryGetProperty("Modules", out var modules) && modules.ValueKind == JsonValueKind.Array,
            "Sample manifest Modules array is missing: " + manifestRelativePath);

        foreach (var module in modules.EnumerateArray())
        {
            var moduleDescription = GetModuleDescription(module, manifestRelativePath);
            var relativeModulePath = GetRequiredStringProperty(module, "RelativePath", moduleDescription);
            var modulePath = ResolveManifestRelativePath(manifestPath, relativeModulePath, moduleDescription);
            Assert.IsTrue(File.Exists(modulePath), "Manifest module file is missing: " + moduleDescription);

            if (TryGetNonEmptyStringProperty(module, "SourceMapPath", out var sourceMapRelativePath))
            {
                var sourceMapPath = ResolveManifestRelativePath(manifestPath, sourceMapRelativePath, moduleDescription);
                Assert.IsTrue(File.Exists(sourceMapPath), "Manifest source map file is missing: " + moduleDescription);
            }
        }
    }

    private static void AssertRazorVueManifestArtifacts(
        JsonElement manifest,
        string manifestPath,
        string manifestRelativePath)
    {
        Assert.IsTrue(
            manifest.TryGetProperty("Modules", out var modules) && modules.ValueKind == JsonValueKind.Array,
            "RazorVue manifest Modules array is missing: " + manifestRelativePath);

        foreach (var module in modules.EnumerateArray())
        {
            var moduleDescription = GetModuleDescription(module, manifestRelativePath);
            var relativeModulePath = GetRequiredStringProperty(module, "RelativeModulePath", moduleDescription);
            var modulePath = ResolveManifestRelativePath(manifestPath, relativeModulePath, moduleDescription);
            Assert.IsTrue(File.Exists(modulePath), "RazorVue module file is missing: " + moduleDescription);

            foreach (var propertyName in new[] { "SourceMapPath", "OriginMapPath" })
            {
                if (!TryGetNonEmptyStringProperty(module, propertyName, out var relativeArtifactPath))
                    continue;

                var artifactPath = ResolveManifestRelativePath(manifestPath, relativeArtifactPath, moduleDescription);
                Assert.IsTrue(File.Exists(artifactPath), "RazorVue artifact is missing: " + propertyName + " for " + moduleDescription);
            }

            if (module.TryGetProperty("Imports", out var imports) && imports.ValueKind == JsonValueKind.Array)
                AssertRazorVueRelativeImportsExist(imports, modulePath, manifestPath, moduleDescription);
        }
    }

    private static void AssertRazorVueRelativeImportsExist(
        JsonElement imports,
        string modulePath,
        string manifestPath,
        string moduleDescription)
    {
        var moduleDirectory = Path.GetDirectoryName(modulePath)
            ?? throw new InvalidOperationException("Could not resolve module directory for " + modulePath);
        var manifestDirectory = Path.GetDirectoryName(manifestPath)
            ?? throw new InvalidOperationException("Could not resolve manifest directory for " + manifestPath);
        var rootedManifestDirectory = Path.GetFullPath(manifestDirectory);
        if (!rootedManifestDirectory.EndsWith(Path.DirectorySeparatorChar))
            rootedManifestDirectory += Path.DirectorySeparatorChar;

        foreach (var import in imports.EnumerateArray())
        {
            var importPath = import.GetString();
            if (string.IsNullOrWhiteSpace(importPath) || !importPath.StartsWith(".", StringComparison.Ordinal))
                continue;

            var resolvedPath = Path.GetFullPath(Path.Combine(moduleDirectory, importPath.Replace('/', Path.DirectorySeparatorChar)));
            Assert.IsTrue(
                resolvedPath.StartsWith(rootedManifestDirectory, StringComparison.OrdinalIgnoreCase),
                "RazorVue relative import escapes the manifest output directory: " + importPath + " for " + moduleDescription);
            Assert.IsTrue(File.Exists(resolvedPath), "RazorVue relative import file is missing: " + importPath + " for " + moduleDescription);
        }
    }

    private static string GetModuleDescription(JsonElement module, string manifestRelativePath)
    {
        var id = TryGetNonEmptyStringProperty(module, "Id", out var moduleId)
            ? moduleId
            : TryGetNonEmptyStringProperty(module, "ModuleId", out var razorVueModuleId)
                ? razorVueModuleId
                : "<unknown module>";

        return manifestRelativePath + " :: " + id;
    }

    private static string GetRequiredStringProperty(JsonElement element, string propertyName, string description)
    {
        Assert.IsTrue(
            TryGetNonEmptyStringProperty(element, propertyName, out var value),
            "Manifest property is missing or empty: " + propertyName + " for " + description);

        return value;
    }

    private static bool TryGetNonEmptyStringProperty(
        JsonElement element,
        string propertyName,
        out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

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
