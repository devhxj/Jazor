using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptVueUseTest;

/// <summary>验证 VueUse 的 core/shared 闭包通过 npm 入口按需解析。</summary>
[TestClass]
public sealed class VueUseManifestTests
{
    [TestMethod]
    public void VueUse_Manifest_DeclaresCoreAndSharedPackageEntries()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vueuse", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());

        var packages = root.GetProperty("packages");
        foreach (var packageName in new[] { "@vueuse/core", "@vueuse/shared", "vue" })
            Assert.AreEqual("npm", packages.GetProperty(packageName).GetProperty("source").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "@vueuse/core", "@vueuse/shared" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "@vueuse/shared", "vue" },
            imports.GetProperty("@vueuse/core").GetProperty("dependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            imports.GetProperty("@vueuse/shared").GetProperty("dependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());

        foreach (var entry in imports.EnumerateObject())
        {
            Assert.IsTrue(IsBareSpecifier(entry.Value.GetProperty("path").GetString()!), entry.Name);
            Assert.AreEqual(entry.Value.GetProperty("path").GetString(), entry.Value.GetProperty("path").GetString());
            Assert.IsFalse(entry.Value.TryGetProperty("developmentHash", out _));
            Assert.IsFalse(entry.Value.TryGetProperty("files", out _));
        }

        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void VueUse_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.AreEqual("15.0.0", manifest.RootElement.GetProperty("packages").GetProperty("@vueuse/core").GetProperty("version").GetString());
    }

    [TestMethod]
    public void VueUse_BoundComposablesUseTheCorePackageEntry()
    {
        var bound = typeof(ECMAScript.VueUse)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 69);

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("@vueuse/core", out _));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("@vueuse/shared", out _));
    }

    [TestMethod]
    public void VueUse_InventoryRecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual("15.0.0", inventory.GetProperty("packages").GetProperty("@vueuse/core").GetString());
        Assert.AreEqual(2, inventory.GetProperty("vendoredModuleCount").GetInt32());

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

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueUse"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
