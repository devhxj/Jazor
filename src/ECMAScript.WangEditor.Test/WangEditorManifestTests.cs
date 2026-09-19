using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECMAScript;
using ECMAScript.Contract;

namespace ECMAScriptWangEditorTest;

/// <summary>验证 WangEditor 的组件入口、编辑器入口和 stylesheet edge 使用 npm graph。</summary>
[TestClass]
public sealed class WangEditorManifestTests
{
    [TestMethod]
    public void WangEditor_Manifest_DeclaresPackageGraphAndStylesheetEdge()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("wang-editor", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.0.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var packages = root.GetProperty("packages");
        Assert.AreEqual("npm", packages.GetProperty("@wangeditor/editor").GetProperty("source").GetString());
        Assert.AreEqual("npm", packages.GetProperty("@wangeditor/editor-for-vue").GetProperty("source").GetString());
        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@wangeditor/editor", "@wangeditor/editor-for-vue" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "@wangeditor/editor", "vue" },
            imports.GetProperty("@wangeditor/editor-for-vue").GetProperty("productionDependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "@wangeditor/editor/dist/css/style.css" },
            imports.GetProperty("@wangeditor/editor").GetProperty("productionStylesheetImports")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());

        foreach (var entry in imports.EnumerateObject())
        {
            Assert.IsTrue(IsBareSpecifier(entry.Value.GetProperty("production").GetString()!), entry.Name);
            Assert.AreEqual(entry.Value.GetProperty("development").GetString(), entry.Value.GetProperty("production").GetString());
            Assert.IsFalse(entry.Value.TryGetProperty("developmentHash", out _));
            Assert.IsFalse(entry.Value.TryGetProperty("files", out _));
        }

        foreach (var file in root.GetProperty("files").EnumerateArray())
            AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void WangEditor_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
    }

    [TestMethod]
    public void WangEditor_ComponentDescriptorsRemainMappedToTheVueEntry()
    {
        var components = typeof(ECMAScript.WangEditor).Assembly.GetExportedTypes()
            .Select(static type => (Type: type, Attribute: type.GetCustomAttribute<ECMAScriptAttribute>()))
            .Where(static item => item.Attribute?.Transform == Transform.Component)
            .ToArray();
        Assert.IsTrue(components.Length >= 2);
        foreach (var component in components)
        {
            Assert.AreEqual("@wangeditor/editor-for-vue", component.Attribute!.Import);
            Assert.IsFalse(string.IsNullOrWhiteSpace(component.Attribute.ExportName));
        }

        Assert.IsTrue(components.Any(static item => item.Attribute!.ExportName == "Editor"));
        Assert.IsTrue(components.Any(static item => item.Attribute!.ExportName == "Toolbar"));
    }

    [TestMethod]
    public void WangEditor_InventoryRecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual("5.1.23", inventory.GetProperty("packages").GetProperty("@wangeditor/editor").GetString());
        Assert.AreEqual(2, inventory.GetProperty("validatedModuleCount").GetInt32());
        Assert.AreEqual("MIT", inventory.GetProperty("packageLicenses").GetProperty("@wangeditor/editor-for-vue").GetString());

        var payload = new Dictionary<string, object?>();
        foreach (var property in inventory.EnumerateObject())
        {
            if (property.Name == "fingerprint")
                continue;
            payload.Add(property.Name, property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetInt32(),
                JsonValueKind.Object => property.Value.EnumerateObject().ToDictionary(static item => item.Name, static item => (object?)item.Value.GetString()),
                JsonValueKind.Array => property.Value.EnumerateArray().Select(static item => item.GetString()).ToArray(),
                _ => throw new InvalidOperationException("Unexpected inventory payload shape."),
            });
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var recomputed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        Assert.AreEqual(inventory.GetProperty("fingerprint").GetString(), recomputed);
    }

    private static bool IsBareSpecifier(string value)
        => !string.IsNullOrWhiteSpace(value) && !value.StartsWith('.', StringComparison.Ordinal) && !value.StartsWith('/', StringComparison.Ordinal);

    private static void AssertFileHash(string relativePath, string expectedHash)
    {
        var fullPath = GetProjectPath(relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.IsTrue(File.Exists(fullPath), relativePath);
        var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath))).ToLowerInvariant();
        Assert.AreEqual(expectedHash.ToLowerInvariant(), actual, relativePath);
    }

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.WangEditor"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
