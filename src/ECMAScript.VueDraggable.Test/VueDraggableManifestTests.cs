using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECMAScript;
using ECMAScript.Contract;
using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ECMAScriptVueDraggableTest;

/// <summary>验证 VueDraggable 的耦合式组件入口通过 npm package graph 交付。</summary>
[TestClass]
public sealed class VueDraggableManifestTests
{
    [TestMethod]
    public void VueDraggable_Manifest_DeclaresComponentPackageEntry()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-draggable", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());

        var packages = root.GetProperty("packages");
        Assert.AreEqual("npm", packages.GetProperty("vue-draggable-plus").GetProperty("source").GetString());
        Assert.AreEqual("npm", packages.GetProperty("vue").GetProperty("source").GetString());
        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "vue-draggable-plus" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        var entry = imports.GetProperty("vue-draggable-plus");
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual("vue-draggable-plus", entry.GetProperty("production").GetString());
        Assert.IsFalse(entry.TryGetProperty("developmentHash", out _));
        Assert.IsFalse(entry.TryGetProperty("files", out _));
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void VueDraggable_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
    }

    [TestMethod]
    public void VueDraggable_ComponentProxyUsesThePackageDefaultAndNamedEntry()
    {
        var attribute = typeof(ECMAScript.VueDraggableList<>).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(attribute);
        Assert.AreEqual("vue-draggable-plus", attribute!.Import);
        Assert.AreEqual(Transform.Component, attribute.Transform);
        Assert.AreEqual("VueDraggable", attribute.ExportName);

        var bound = typeof(ECMAScript.VueDraggable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static description => description.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .ToArray();
        Assert.IsTrue(bound.Length >= 1);
    }

    [TestMethod]
    public void VueDraggable_InventoryRecordsPackageVersionAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("vue-draggable-plus").GetString());
        Assert.AreEqual(1, inventory.GetProperty("vendoredModuleCount").GetInt32());

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

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueDraggable"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
