using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptDateFnsTest;

/// <summary>验证 date-fns 绑定通过 npm package exports 提供细粒度入口。</summary>
[TestClass]
public sealed class DateFnsManifestTests
{
    [TestMethod]
    public void DateFns_Manifest_DeclaresBarePackageEntries()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("date-fns", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual(0, root.GetProperty("requires").EnumerateObject().Count());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "date-fns", "date-fns/locale" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.AreEqual(entry.Name, entry.Value.GetProperty("development").GetString());
            Assert.AreEqual(entry.Name, entry.Value.GetProperty("production").GetString());
            Assert.IsFalse(entry.Value.TryGetProperty("developmentHash", out _));
            Assert.IsFalse(entry.Value.TryGetProperty("files", out _));
        }

        Assert.AreEqual("npm", root.GetProperty("packages").GetProperty("date-fns").GetProperty("source").GetString());
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void DateFns_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.AreEqual("4.4.0", manifest.RootElement.GetProperty("packages").GetProperty("date-fns").GetProperty("version").GetString());
    }

    [TestMethod]
    public void DateFns_BoundEntriesMatchPackageImportKeys()
    {
        var functionNames = ReadBoundExportNames(typeof(ECMAScript.DateFns));
        Assert.IsTrue(functionNames.Count >= 59, "The first date-fns slice should expose the curated function set.");
        var localeNames = typeof(ECMAScript.DateFnsLocale)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static property => property.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .ToArray();
        Assert.IsTrue(localeNames.Length > 0, "The locale bridge must retain typed locale entries.");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var imports = manifest.RootElement.GetProperty("imports");
        Assert.IsTrue(imports.TryGetProperty("date-fns", out _));
        Assert.IsTrue(imports.TryGetProperty("date-fns/locale", out _));
    }

    [TestMethod]
    public void DateFns_InventoryFingerprintMatchesItsPayload()
    {
        var inventory = GetInventory();
        var fingerprint = inventory.GetProperty("fingerprint").GetString()!;
        var payload = new Dictionary<string, object?>();
        foreach (var property in inventory.EnumerateObject())
        {
            if (property.Name == "fingerprint")
                continue;
            payload.Add(property.Name, property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetInt32(),
                JsonValueKind.Array => property.Value.EnumerateArray().Select(static item => item.GetString()).ToArray(),
                _ => throw new InvalidOperationException("Unexpected inventory payload shape."),
            });
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var recomputed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        Assert.AreEqual(fingerprint, recomputed);
    }

    private static HashSet<string> ReadBoundExportNames(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.DateFns"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
