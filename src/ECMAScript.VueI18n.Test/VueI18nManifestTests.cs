using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VueI18nTest;

/// <summary>验证 Vue I18n 通过 npm exports 交付单一作者入口。</summary>
[TestClass]
public sealed class VueI18nManifestTests
{
    [TestMethod]
    public void VueI18n_Manifest_DeclaresBarePackageEntryAndLicense()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-i18n", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.0.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var entry = root.GetProperty("imports").GetProperty("vue-i18n");
        Assert.AreEqual("vue-i18n", entry.GetProperty("development").GetString());
        Assert.AreEqual("vue-i18n", entry.GetProperty("production").GetString());
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.IsFalse(entry.TryGetProperty("developmentHash", out _));
        Assert.IsFalse(entry.TryGetProperty("files", out _));
        Assert.AreEqual("npm", root.GetProperty("packages").GetProperty("vue-i18n").GetProperty("source").GetString());
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());

        foreach (var file in root.GetProperty("files").EnumerateArray())
            AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void VueI18n_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
    }

    [TestMethod]
    public void VueI18n_BoundExportsUseTheAuthorPackageEntry()
    {
        var bound = typeof(ECMAScript.VueI18n)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 2);
        CollectionAssert.Contains(bound.ToArray(), "createI18n");
        CollectionAssert.Contains(bound.ToArray(), "useI18n");
    }

    [TestMethod]
    public void VueI18n_ComposerSurfaceCoversLocaleMessagesAndFormatting()
    {
        var composer = typeof(ECMAScript.VueI18nComposer);
        Assert.AreEqual(typeof(ECMAScript.Vue.IVueRef<string>), composer.GetProperty("Locale")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.IVueRef<string>), composer.GetProperty("FallbackLocale")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<ECMAScript.Vue.VueDictionary>), composer.GetProperty("Messages")!.PropertyType);
        Assert.AreEqual(typeof(string[]), composer.GetProperty("AvailableLocales")!.PropertyType);
        Assert.IsTrue(composer.GetMethods().Any(static method => method.Name == "D"));
    }

    [TestMethod]
    public void VueI18n_InventoryRecordsPackageVersionAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("vue-i18n").GetString());
        Assert.AreEqual(1, inventory.GetProperty("validatedModuleCount").GetInt32());

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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueI18n"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
