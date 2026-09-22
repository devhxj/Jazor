using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptMonacoTest;

/// <summary>验证 Monaco 通过 npm 的入口和 worker 子路径提供按需资源。</summary>
[TestClass]
public sealed class MonacoManifestTests
{
    [TestMethod]
    public void Monaco_Manifest_DeclaresEditorAndWorkerPackageEntries()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("monaco", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual(6, root.GetProperty("imports").EnumerateObject().Count());

        var imports = root.GetProperty("imports");
        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.IsTrue(IsBareSpecifier(entry.Value.GetProperty("path").GetString()!), entry.Name);
            Assert.AreEqual(entry.Value.GetProperty("path").GetString(), entry.Value.GetProperty("path").GetString());
            Assert.IsFalse(entry.Value.TryGetProperty("developmentHash", out _));
            Assert.IsFalse(entry.Value.TryGetProperty("files", out _));
        }

        CollectionAssert.Contains(imports.EnumerateObject().Select(static entry => entry.Name).ToArray(), "monaco-editor");
        CollectionAssert.Contains(imports.EnumerateObject().Select(static entry => entry.Name).ToArray(), "monaco-editor/editor/editor.worker.start.js");
        Assert.AreEqual("npm", root.GetProperty("packages").GetProperty("monaco-editor").GetProperty("source").GetString());
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());

        var licenseFiles = root.GetProperty("files").EnumerateArray().ToArray();
        Assert.AreEqual(2, licenseFiles.Length);
        foreach (var file in licenseFiles)
            AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void Monaco_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.AreEqual("0.56.0", manifest.RootElement.GetProperty("packages").GetProperty("monaco-editor").GetProperty("version").GetString());
    }

    [TestMethod]
    public void Monaco_BoundEditorApisUseTheAuthorEntryAndWorkerEdges()
    {
        var bound = typeof(ECMAScript.Monaco)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 11);

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var imports = manifest.RootElement.GetProperty("imports");
        Assert.IsTrue(imports.TryGetProperty("monaco-editor", out _));
        Assert.AreEqual(5, imports.EnumerateObject().Count(static entry => entry.Name.Contains("worker", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Monaco_InventoryRecordsVersionAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("monaco-editor").GetString());
        Assert.AreEqual(6, inventory.GetProperty("validatedEntryCount").GetInt32());
        Assert.IsTrue(inventory.GetProperty("runtimeNote").GetString()!.Contains("node_modules", StringComparison.Ordinal));

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
                JsonValueKind.Object => property.Value.EnumerateObject().ToDictionary(static item => item.Name, static item => (object?)item.Value.GetString()),
                _ => throw new InvalidOperationException("Unexpected inventory payload shape."),
            });
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var recomputed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        Assert.AreEqual(fingerprint, recomputed);
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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.Monaco"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
