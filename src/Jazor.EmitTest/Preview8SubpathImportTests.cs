using System.Text.Json;
using System.Text.Json.Nodes;
using Jazor.Common;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class Preview8SubpathImportTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Materialize_DeclaredElementPlusLocale_ReusesPinnedIdentityWithoutSelectingUnusedEntries(bool production)
    {
        using var workspace = new Workspace();
        const string locale = "element-plus/es/locale/lang/zh-cn";
        var consumer = workspace.WriteManifest("consumer", "1.0.0", [locale]);
        var binding = Path.Combine(FindRepositoryRoot(), "src", "ECMAScript.ElementPlus", "manifest.json");

        var result = new LibraryMaterializer().Materialize(
            [consumer, binding], workspace.Output, production ? BuildMode.Production : BuildMode.Development, [locale]);

        Assert.AreEqual(LibraryManifest.LoadMetadata(binding).Packages["element-plus"], result.PackageReferences[locale]);
        Assert.AreEqual(locale, result.ImportPaths[locale]);
        CollectionAssert.AreEqual(new[] { locale }, result.ImportPaths.Keys.ToArray());
        CollectionAssert.AreEqual(new[] { consumer }, result.ManifestPaths.ToArray());
        LibraryPackageWriter.WritePackageProject(workspace.Output, result);
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Output, "package.json")));
        Assert.AreEqual("2.14.5", package.RootElement.GetProperty("dependencies").GetProperty("element-plus").GetString());
        Assert.AreEqual(
            result.PackageReferences[locale].Integrity,
            package.RootElement.GetProperty("jazor").GetProperty("packages").GetProperty("element-plus").GetProperty("integrity").GetString());
    }

    [TestMethod]
    public void Materialize_UndeclaredSubpath_StillFailsWhenPackageIdentityExists()
    {
        using var workspace = new Workspace();
        var provider = workspace.WriteManifest("provider", "4.0.0", ["widget"], "widget", "4.0.0");

        var exception = Assert.Throws<LibraryException>(() => new LibraryMaterializer().Materialize(
            [provider], workspace.Output, BuildMode.Production, ["widget/locale/zh-cn"]));

        Assert.AreEqual("JAZOR_LIBRARY_IMPORT_MISSING", exception.Code);
        StringAssert.Contains(exception.Message, "widget/locale/zh-cn");
        Assert.IsFalse(Directory.Exists(workspace.Output));
    }

    [TestMethod]
    [DataRow("4.1.0", "sha512-second")]
    [DataRow("4.0.0", "sha512-second")]
    [DataRow("4.0.0", "sha512-FIRST")]
    public void Materialize_AmbiguousPackageIdentity_ReportsProvidersBeforeWritingOutput(string secondVersion, string secondIntegrity)
    {
        using var workspace = new Workspace();
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["widget/locale/zh-cn"]);
        var first = workspace.WriteManifest("first", "4.0.0", ["widget"], "widget", "4.0.0", "sha512-first");
        var second = workspace.WriteManifest("second", secondVersion, ["widget/feature"], "widget", secondVersion, secondIntegrity);

        var exception = Assert.Throws<LibraryException>(() => new LibraryMaterializer().Materialize(
            [consumer, second, first], workspace.Output, BuildMode.Production, ["widget/locale/zh-cn"]));

        Assert.AreEqual("JAZOR_LIBRARY_PACKAGE_CONFLICT", exception.Code);
        StringAssert.Contains(exception.Message, "widget/locale/zh-cn");
        StringAssert.Contains(exception.Message, first);
        StringAssert.Contains(exception.Message, second);
        StringAssert.Contains(exception.Message, "Declare the intended package identity explicitly");
        Assert.IsFalse(Directory.Exists(workspace.Output));
    }

    [TestMethod]
    public void Materialize_ExplicitConsumerPackageMetadata_RemainsAuthoritative()
    {
        using var workspace = new Workspace();
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["widget/locale/zh-cn"], "widget", "3.9.0", "sha512-consumer");
        var first = workspace.WriteManifest("first", "4.0.0", ["widget"], "widget", "4.0.0", "sha512-first");
        var second = workspace.WriteManifest("second", "4.1.0", ["widget/feature"], "widget", "4.1.0", "sha512-second");

        var result = new LibraryMaterializer().Materialize(
            [first, consumer, second], workspace.Output, BuildMode.Production, ["widget/locale/zh-cn"]);

        Assert.AreEqual("3.9.0", result.PackageReferences["widget/locale/zh-cn"].Version);
        Assert.AreEqual("sha512-consumer", result.PackageReferences["widget/locale/zh-cn"].Integrity);
    }

    [TestMethod]
    public void Materialize_EquivalentPackageDeclarations_ReusesOneIdentity()
    {
        using var workspace = new Workspace();
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["widget/locale/zh-cn"]);
        var first = workspace.WriteManifest("first", "4.0.0", ["widget"], "widget", "4.0.0", "sha512-same");
        var second = workspace.WriteManifest("second", "4.0.0", ["widget/feature"], "widget", "4.0.0", "sha512-same");

        var result = new LibraryMaterializer().Materialize(
            [second, consumer, first], workspace.Output, BuildMode.Production, ["widget/locale/zh-cn"]);

        Assert.AreEqual("4.0.0", result.PackageReferences["widget/locale/zh-cn"].Version);
        Assert.AreEqual("sha512-same", result.PackageReferences["widget/locale/zh-cn"].Integrity);
    }

    [TestMethod]
    public void Materialize_ExistingRootWithoutPackageTable_ReusesItsManifestIdentity()
    {
        using var workspace = new Workspace();
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["widget/locale/zh-cn"]);
        var provider = workspace.WriteManifest("provider", "4.0.0", ["widget"]);

        var result = new LibraryMaterializer().Materialize(
            [consumer, provider], workspace.Output, BuildMode.Production, ["widget/locale/zh-cn"]);

        Assert.AreEqual("4.0.0", result.PackageReferences["widget/locale/zh-cn"].Version);
    }

    [TestMethod]
    public void Materialize_DeclaredScopedAliasSubpath_PreservesAliasIdentity()
    {
        using var workspace = new Workspace();
        const string alias = "npm:@sxzz/popperjs-es@2.11.8";
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["@popperjs/core/lib"]);
        var provider = workspace.WriteManifest("provider", "2.11.8", ["@popperjs/core"], "@popperjs/core", alias, "sha512-alias");

        var result = new LibraryMaterializer().Materialize(
            [consumer, provider], workspace.Output, BuildMode.Production, ["@popperjs/core/lib"]);

        Assert.AreEqual(alias, result.PackageReferences["@popperjs/core/lib"].Version);
        Assert.AreEqual("sha512-alias", result.PackageReferences["@popperjs/core/lib"].Integrity);
        LibraryPackageWriter.WritePackageProject(workspace.Output, result);
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Output, "package.json")));
        Assert.AreEqual(alias, package.RootElement.GetProperty("dependencies").GetProperty("@popperjs/core").GetString());
    }

    [TestMethod]
    public void Materialize_DeclaredJsrSubpath_PreservesAuthoredAndCanonicalIdentity()
    {
        using var workspace = new Workspace();
        var consumer = workspace.WriteManifest("consumer", "1.0.0", ["@std/path/posix"]);
        var provider = workspace.WriteManifest("provider", "1.0.8", ["@std/path"], "@std/path", "jsr:@std/path@1.0.8", source: "jsr");

        var result = new LibraryMaterializer().Materialize(
            [consumer, provider], workspace.Output, BuildMode.Production, ["@std/path/posix"]);

        var reference = result.PackageReferences["@std/path/posix"];
        Assert.AreEqual("jsr", reference.Source);
        Assert.AreEqual("@std/path", reference.AuthoredName);
        Assert.AreEqual("@jsr/std__path", reference.CanonicalName);
        Assert.AreEqual("@jsr/std__path/posix", LibraryPackageIdentity.GetCanonicalSpecifier(reference, "@std/path/posix"));
    }

    [TestMethod]
    public void Materialize_EmbeddedScopedModule_UsesDeclaredPathsAndRelativeModuleClosure()
    {
        using var workspace = new Workspace();
        const string main = "import { value } from './chunk.mjs'; export const result = value + 1;\n";
        const string chunk = "export const value = 41;\n";
        Directory.CreateDirectory(Path.Combine(workspace.Root, "runtime"));
        File.WriteAllText(Path.Combine(workspace.Root, "runtime", "index.mjs"), main);
        File.WriteAllText(Path.Combine(workspace.Root, "runtime", "chunk.mjs"), chunk);
        var manifest = CreateManifest("embedded", "1.0.0", "embedded-mjs");
        manifest["packages"] = new JsonObject
        {
            ["@scope/widget"] = new JsonObject { ["source"] = "embedded-mjs", ["version"] = "1.0.0" }
        };
        manifest["imports"] = new JsonObject
        {
            ["@scope/widget"] = new JsonObject
            {
                ["type"] = "module", ["path"] = "runtime/index.mjs",
                ["hash"] = ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(workspace.Root, "runtime", "index.mjs"))),
                ["moduleDependencies"] = new JsonArray("@scope/widget/chunk")
            },
            ["@scope/widget/chunk"] = new JsonObject
            {
                ["type"] = "module", ["path"] = "runtime/chunk.mjs",
                ["hash"] = ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(workspace.Root, "runtime", "chunk.mjs")))
            }
        };
        var manifestPath = workspace.WriteManifest(manifest);

        var result = new LibraryMaterializer().Materialize(
            [manifestPath], workspace.Output, BuildMode.Production, ["@scope/widget"]);

        Assert.AreEqual("runtime/index.mjs", result.ImportPaths["@scope/widget"]);
        Assert.AreEqual("runtime/chunk.mjs", result.ImportPaths["@scope/widget/chunk"]);
        Assert.AreEqual(main, File.ReadAllText(Path.Combine(workspace.Output, "runtime", "index.mjs")));
        Assert.AreEqual(chunk, File.ReadAllText(Path.Combine(workspace.Output, "runtime", "chunk.mjs")));
        LibraryPackageWriter.WritePackageProject(workspace.Output, result);
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Output, "package.json")));
        Assert.IsFalse(package.RootElement.GetProperty("dependencies").TryGetProperty("@scope/widget", out _));
    }

    private static JsonObject CreateManifest(string id, string version, string source)
        => new()
        {
            ["schemaVersion"] = 2, ["libraryId"] = id, ["version"] = version, ["source"] = source,
            ["requires"] = new JsonObject(), ["styles"] = new JsonArray(), ["files"] = new JsonArray()
        };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Unable to find the repository root.");
    }

    private sealed class Workspace : IDisposable
    {
        public string Root { get; } = Path.Combine(FindRepositoryRoot(), ".tmp", "preview8-subpaths", "fixtures", Guid.NewGuid().ToString("N"));
        public string Output => Path.Combine(Root, "out");

        public Workspace() => Directory.CreateDirectory(Root);

        public string WriteManifest(
            string id, string version, string[] imports, string? packageName = null,
            string? packageVersion = null, string? integrity = null, string source = "npm")
        {
            var manifest = CreateManifest(id, version, source);
            manifest["imports"] = new JsonObject(imports.Select(specifier =>
                new KeyValuePair<string, JsonNode?>(specifier, new JsonObject { ["type"] = "module", ["path"] = specifier })));
            if (packageName is not null)
            {
                manifest["packages"] = new JsonObject
                {
                    [packageName] = new JsonObject { ["source"] = source, ["version"] = packageVersion, ["integrity"] = integrity }
                };
            }
            return WriteManifest(manifest);
        }

        public string WriteManifest(JsonObject manifest)
        {
            var path = Path.Combine(Root, manifest["libraryId"]!.GetValue<string>() + ".manifest.json");
            File.WriteAllText(path, manifest.ToJsonString());
            return path;
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
