using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptFloatingUiTest;

/// <summary>验证 Floating UI 的多包闭包通过 npm exports 保持可选入口。</summary>
[TestClass]
public sealed class FloatingUiManifestTests
{
    [TestMethod]
    public void FloatingUi_Manifest_DeclaresMultiPackageGraph()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("floating-ui", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());

        var packages = root.GetProperty("packages");
        foreach (var packageName in new[] { "@floating-ui/vue", "@floating-ui/dom", "@floating-ui/core", "@floating-ui/utils", "vue" })
            Assert.AreEqual("npm", packages.GetProperty(packageName).GetProperty("source").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@floating-ui/vue", "@floating-ui/dom", "@floating-ui/core", "@floating-ui/utils", "@floating-ui/utils/dom" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "@floating-ui/dom", "@floating-ui/utils/dom" },
            imports.GetProperty("@floating-ui/vue").GetProperty("dependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());

        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
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
    public void FloatingUi_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.AreEqual("1.1.9", manifest.RootElement.GetProperty("packages").GetProperty("@floating-ui/vue").GetProperty("version").GetString());
    }

    [TestMethod]
    public void FloatingUi_BoundExportsUseTheVuePackageEntry()
    {
        var bound = typeof(ECMAScript.FloatingUi)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 12, "The first Floating UI slice should retain the placement helpers.");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("@floating-ui/vue", out _));
    }

    [TestMethod]
    public void FloatingUi_InventoryFingerprintMatchesItsPayload()
    {
        var inventory = GetInventory();
        var packages = inventory.GetProperty("packages");
        Assert.AreEqual(inventory.GetProperty("version").GetString(), packages.GetProperty("@floating-ui/vue").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(inventory.GetProperty("fingerprint").GetString()));

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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.FloatingUi"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
