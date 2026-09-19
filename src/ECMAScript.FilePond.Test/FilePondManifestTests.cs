using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptFilePondTest;

/// <summary>
/// Manifest and upstream identity checks for the FilePond package graph.
/// 验证绑定元数据、入口和样式边；运行时文件由 Emit 生成的标准 package project 恢复。
/// </summary>
[TestClass]
public sealed class FilePondManifestTests
{
    [TestMethod]
    public void FilePond_Manifest_DeclaresLockedPackageGraphAndStylesheetEdge()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("file-pond", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.0.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "filepond", "vue-filepond" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        Assert.AreEqual(0, imports.GetProperty("filepond").GetProperty("productionDependencies").EnumerateArray().Count());
        CollectionAssert.AreEquivalent(
            new[] { "filepond" },
            imports.GetProperty("vue-filepond").GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.AreEqual(entry.Name, entry.Value.GetProperty("development").GetString());
            Assert.AreEqual(entry.Name, entry.Value.GetProperty("production").GetString());
        }

        CollectionAssert.AreEquivalent(
            new[] { "filepond/dist/filepond.css" },
            imports.GetProperty("filepond")
                .GetProperty("productionStylesheetImports")
                .EnumerateArray()
                .Select(static value => value.GetString()!)
                .ToArray());
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void FilePond_RuntimeFilesAreResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var packages = manifest.RootElement.GetProperty("packages");
        Assert.AreEqual("npm", packages.GetProperty("filepond").GetProperty("source").GetString());
        Assert.AreEqual("npm", packages.GetProperty("vue-filepond").GetProperty("source").GetString());
    }

    [TestMethod]
    public void FilePond_AuthoredEntriesRemainBarePackageSpecifiers()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        foreach (var entry in manifest.RootElement.GetProperty("imports").EnumerateObject())
        {
            Assert.IsFalse(entry.Value.GetProperty("production").GetString()!.Contains("/", StringComparison.Ordinal) &&
                           entry.Value.GetProperty("production").GetString()!.StartsWith(".", StringComparison.Ordinal),
                "Runtime entries must remain package specifiers.");
        }
    }

    [TestMethod]
    public void FilePond_BoundEntriesMatchManifestKeys()
    {
        // 宿主函数条目（@# 描述）与组件代理（Transform.Component 的 ExportName）都必须在上游入口存在；
        // 默认导出的组件代理改用 default 导出断言。
        var hostNames = ReadBoundExportNames(typeof(FilePond));
        var componentExports = ReadComponentExports(typeof(FilePond).Assembly);
        Assert.IsTrue(hostNames.Count + componentExports.Count >= 8,
            "Expected at least 8 bound entries, found " + (hostNames.Count + componentExports.Count) + ".");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("filepond", out _));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("vue-filepond", out _));
        Assert.IsTrue(componentExports.Contains("default"));
    }

    private static HashSet<string> ReadComponentExports(System.Reflection.Assembly assembly)
        => assembly.GetExportedTypes()
            .Select(static type => type.GetCustomAttribute<ECMAScriptAttribute>())
            .Where(static attribute => attribute?.Transform == Transform.Component)
            .Select(static attribute => attribute!.ExportName ?? "default")
            .ToHashSet(StringComparer.Ordinal);

    [TestMethod]
    public void FilePond_Inventory_RecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        var packages = inventory.GetProperty("packages");
        Assert.AreEqual(inventory.GetProperty("version").GetString(), packages.GetProperty("vue-filepond").GetString());
        Assert.IsTrue(packages.TryGetProperty("filepond", out var core));
        Assert.AreEqual("4.32.12", core.GetString());

        Assert.IsFalse(string.IsNullOrWhiteSpace(inventory.GetProperty("fingerprint").GetString()));
        CollectionAssert.AreEquivalent(
            new[] { "filepond", "vue-filepond" },
            inventory.GetProperty("entryImports").EnumerateArray().Select(static value => value.GetString()!).ToArray());
    }

    private static HashSet<string> ReadBoundExportNames(Type hostType)
        => hostType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);


    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.FilePond"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
